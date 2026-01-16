using Microsoft.AspNetCore.Mvc;
using Stripe; 
using Microsoft.EntityFrameworkCore;
using Throb.Data.Entities;
using Throb.Repository.Interfaces;
using Throb.Service.Interfaces.Payment;

public class HomeController : Controller
{
    private readonly ICourseRepository _courseRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IStudentCourseRepository _studentCourseRepository;
    private readonly IConfiguration _configuration;
    private readonly IPaymentProvider _paymentProvider;

    public HomeController(
        ICourseRepository courseRepository,
        IStudentRepository studentRepository,
        IStudentCourseRepository studentCourseRepository,
        IConfiguration configuration,
        IPaymentProvider paymentProvider)
    {
        _courseRepository = courseRepository;
        _studentRepository = studentRepository;
        _studentCourseRepository = studentCourseRepository;
        _configuration = configuration;
        _paymentProvider = paymentProvider;
    }
    [HttpGet]
    public IActionResult Register(int? courseId)
    {
        var courses = _courseRepository.GetAll();
        ViewBag.Courses = courses;

        if (courseId.HasValue)
        {
            var selectedCourse = courses.FirstOrDefault(c => c.Id == courseId.Value);
            ViewBag.SelectedCourse = selectedCourse;
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(Student student, string paymentMethod, int? courseId)
    {
        // 1. التحقق من صحة البيانات المدخلة
        if (ModelState.IsValid)
        {
            // 2. التحقق من وجود الكورس المختار للحصول على السعر
            if (!courseId.HasValue)
            {
                ModelState.AddModelError("", "الرجاء اختيار كورس للتسجيل.");
                ViewBag.Courses = _courseRepository.GetAll();
                return View(student);
            }

            var course = _courseRepository.GetById(courseId.Value);
            if (course == null) return NotFound();

            // 3. حفظ بيانات الطالب وربطه بالكورس (Logic ثابت لا يتغير بتغير طريقة الدفع)
            student.CreateAt = DateTime.Now;
            _studentRepository.Add(student);

            var studentCourse = new StudentCourse
            {
                StudentId = student.Id,
                CourseId = courseId.Value
            };
            _studentCourseRepository.Add(studentCourse);

            try
            {
                // 4. استخدام الـ Strategy Pattern لاختيار وتنفيذ عملية الدفع
                // نطلب الاستراتيجية بناءً على خيار المستخدم (Card أو Cash)
                var strategy = _paymentProvider.GetStrategy(paymentMethod);

                // تنفيذ العملية (ستعيد ClientSecret في حال البطاقة، أو رسالة نجاح في حال الكاش)
                var paymentResult = await strategy.ExecutePaymentAsync(course.CoursePrice, student.Id);

                TempData["RegistrationSuccess"] = "You have successfully registered for the course!";

                // 5. التوجيه بناءً على طريقة الدفع المختار
                if (paymentMethod == "Card")
                {
                    // نمرر الـ ClientSecret للـ Action التالي (Payment) ليتم عرضه في واجهة Stripe
                    TempData["ClientSecret"] = paymentResult;
                    return RedirectToAction("Payment", new { studentId = student.Id });
                }
                else // في حالة الدفع النقدي (Cash)
                {
                    TempData["PaymentStatus"] = "Registration successful, please pay on delivery.";
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                // معالجة الأخطاء الناتجة عن الـ API أو عدم دعم الطريقة
                ModelState.AddModelError("", $"حدث خطأ أثناء معالجة الدفع: {ex.Message}");
            }
        }

        // إعادة عرض الصفحة في حال وجود أخطاء
        ViewBag.Courses = _courseRepository.GetAll();
        return View(student);
    }

    public IActionResult Payment(int studentId)
    {
        StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];


        var student = _studentRepository.GetById(studentId);
        if (student == null) return NotFound();

      
        var studentCourse = _studentCourseRepository.GetAll().FirstOrDefault(sc => sc.StudentId == studentId);
        if (studentCourse == null) return NotFound();

        var course = _courseRepository.GetById(studentCourse.CourseId);
        if (course == null) return NotFound();

        
        var priceInCents = (long)(course.CoursePrice * 100); 

        var options = new PaymentIntentCreateOptions
        {
            Amount = priceInCents,
            Currency = "usd", 
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true },
        };

        var service = new PaymentIntentService();
        try
        {
            var intent = service.Create(options);

          
            ViewBag.ClientSecret = intent.ClientSecret;
            ViewBag.CourseName = course.Name;
            ViewBag.CoursePrice = course.CoursePrice;
        }
        catch (StripeException ex)
        {
            TempData["PaymentStatus"] = $"خطأ في الاتصال ببوابة الدفع: {ex.Message}";
            return RedirectToAction("Index", "Home");
        }

        return View(student);
    }





    // 1. قمنا بتغيير النوع من HttpPost إلى HttpGet لأن إعادة التوجيه تأتي كـ GET
    [HttpGet]
    public IActionResult PaymentConfirmation(int studentId)
    {
        // 2. إعداد مفتاح Stripe
        StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];

        var service = new PaymentIntentService();

        // 3. جلب الـ payment_intent من روابط الـ URL (Query String)
        // الرابط الذي أرسلته يحتوي على payment_intent=pi_3So8S3...
        var paymentIntentId = Request.Query["payment_intent"].ToString();

        if (string.IsNullOrEmpty(paymentIntentId))
        {
            TempData["PaymentStatus"] = "حدث خطأ: لم يتم العثور على معرّف المعاملة.";
            return RedirectToAction("Index", "Home");
        }

        try
        {
            // 4. التحقق من حالة الدفع من سيرفرات Stripe مباشرة
            var paymentIntent = service.Get(paymentIntentId);

            if (paymentIntent.Status == "succeeded")
            {
                // هنا يمكنك إضافة منطق تحديث قاعدة البيانات لجعل حالة الكورس "مدفوع"
                TempData["PaymentStatus"] = $"تم الدفع بنجاح! رقم المعاملة: {paymentIntent.Id}";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["PaymentStatus"] = $"حالة الدفع الحالية: {paymentIntent.Status}";
                return RedirectToAction("Index", "Home");
            }
        }
        catch (StripeException ex)
        {
            TempData["PaymentStatus"] = $"خطأ في Stripe: {ex.Message}";
            return RedirectToAction("Index", "Home");
        }
    }
    public IActionResult Index()
    {
        ViewBag.PaymentStatus = TempData["PaymentStatus"];

        // جلب الكورسات مع المدربين كما في الكود الخاص بك
        var coursesWithInstructors = _courseRepository.GetAll()
            .Include(c => c.InstructorCourses)
            .ThenInclude(ic => ic.Instructor)
            .ToList();

        // قوائم لتخزين المعرفات (IDs)
        var enrolledCourseIds = new List<int>();
        var instructorCourseIds = new List<int>();

        if (User.Identity.IsAuthenticated)
        {
            var userName = User.Identity.Name;

            // 1. إذا كان المستخدم طالباً
            if (User.IsInRole("Student"))
            {
                // جلب الطالب أولاً بناءً على اسمه أو بريده الإلكتروني
                var student = _studentRepository.GetAll().FirstOrDefault(s => s.Name == userName || s.Email == userName);
                if (student != null)
                {
                    // جلب قائمة بأرقام الكورسات المسجل بها هذا الطالب
                    enrolledCourseIds = _studentCourseRepository.GetAll()
                        .Where(sc => sc.StudentId == student.Id)
                        .Select(sc => sc.CourseId)
                        .ToList();
                }
            }

            // 2. إذا كان المستخدم مدرباً
            if (User.IsInRole("Instructor"))
            {
                // ملاحظة: ستحتاج للتأكد أن لديك InstructorRepository أو الوصول لجدول InstructorCourse
                // لنفترض أننا سنصل إليها عبر الـ Repository المتاح أو استعلام مباشر إذا كان مسموحاً
                instructorCourseIds = coursesWithInstructors
                    .Where(c => c.InstructorCourses.Any(ic => ic.Instructor.Name == userName))
                    .Select(c => c.Id)
                    .ToList();
            }
        }

        // تمرير القوائم للـ View عبر ViewBag
        ViewBag.EnrolledCourseIds = enrolledCourseIds;
        ViewBag.InstructorCourseIds = instructorCourseIds;

        return View(coursesWithInstructors);
    }


    [HttpPost]
    public IActionResult JoinCourse(int courseId)
    {
        var studentId = HttpContext.Session.GetString("StudentId");

        if (string.IsNullOrEmpty(studentId))
        {
            return RedirectToAction("Login", "Account", new { returnUrl = $"/Home/JoinCourse/{courseId}" });
        }

        var course = _courseRepository.GetById(courseId);

        var studentCourse = new StudentCourse
        {
            StudentId = int.Parse(studentId),
            CourseId = courseId
        };

        _studentCourseRepository.Add(studentCourse);

        TempData["RegistrationSuccess"] = $"You have successfully joined the course: {course.Name}";

        return RedirectToAction("Details", "Course", new { id = courseId });
    }

    public IActionResult Privacy()
    {
        ViewData["Title"] = "Privacy Policy";
        return View();
    }
}
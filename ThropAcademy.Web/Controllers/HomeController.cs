using Microsoft.AspNetCore.Mvc;
using Stripe; 
using Microsoft.EntityFrameworkCore;
using Throb.Data.Entities;
using Throb.Repository.Interfaces;

public class HomeController : Controller
{
    private readonly ICourseRepository _courseRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IStudentCourseRepository _studentCourseRepository;

    public HomeController(
        ICourseRepository courseRepository,
        IStudentRepository studentRepository,
        IStudentCourseRepository studentCourseRepository)
    {
        _courseRepository = courseRepository;
        _studentRepository = studentRepository;
        _studentCourseRepository = studentCourseRepository;
    }

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

    // معالجة التسجيل عند إرسال البيانات
    [HttpPost]
    public IActionResult Register(Student student, string paymentMethod, int? courseId)
    {
        if (ModelState.IsValid)
        {
            student.CreateAt = DateTime.Now;
            _studentRepository.Add(student);

            if (courseId.HasValue)
            {
                var studentCourse = new StudentCourse
                {
                    StudentId = student.Id,
                    CourseId = courseId.Value
                    // ⚠️ يمكنك إضافة حقل IsPaid = false هنا
                };
                _studentCourseRepository.Add(studentCourse);
            }

            TempData["RegistrationSuccess"] = "You have successfully registered for the course!";

            if (paymentMethod == "Card")
            {
                // توجيه إلى صفحة الدفع (Stripe)
                return RedirectToAction("Payment", new { studentId = student.Id });
            }
            else if (paymentMethod == "Cash")
            {
                TempData["PaymentStatus"] = "Registration successful, please pay on delivery.";
                return RedirectToAction("Index", "Home");
            }
        }

        return View(student);
    }

    // 💰 دالة عرض صفحة الدفع (تم التعديل لإضافة منطق Stripe)
    public IActionResult Payment(int studentId)
    {
        var student = _studentRepository.GetById(studentId);
        if (student == null) return NotFound();

        // 1. جلب الكورس والسعر
        // يجب أن تفترض وجود دالة GetByStudentId في الـ Repository الخاص بك
        var studentCourse = _studentCourseRepository.GetAll().FirstOrDefault(sc => sc.StudentId == studentId);
        if (studentCourse == null) return NotFound();

        var course = _courseRepository.GetById(studentCourse.CourseId);
        if (course == null) return NotFound();

        // 2. تهيئة Stripe بالمفتاح السري (Test Key)
        // ⚠️ يجب استبدال هذا بالمفتاح السري التجريبي (sk_test_...)
        StripeConfiguration.ApiKey = "sk_test_51QA5Q7HruNG7D4CWjJofbpFUQzfAYYwFoLdQ3IYo6qK0k7BfGQZBrxsxjdGqIoUnbFDxo897jPOT5yWLaq3HbjMM0058PsMGmh";

        // 3. إنشاء Payment Intent
        var priceInCents = (long)(course.CoursePrice * 100); // تحويل السعر إلى السنت/قروش

        var options = new PaymentIntentCreateOptions
        {
            Amount = priceInCents,
            Currency = "usd", // استخدم عملة دولية للاختبار
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true },
        };

        var service = new PaymentIntentService();
        try
        {
            var intent = service.Create(options);

            // 4. تمرير Client Secret إلى الواجهة الأمامية
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



    // ✅ دالة جديدة لمعالجة رد Stripe بعد التوجيه
    // ... داخل HomeController.cs

    // 💰 دالة جديدة أو معدّلة لمعالجة رد Stripe بعد التوجيه
    [HttpGet] // يتم استخدامها عند التوجيه
    [HttpPost] // يتم استخدامها عند الفشل في التوجيه
    public IActionResult PaymentConfirmation(int studentId)
    {
        // 1. إعادة تهيئة Stripe
        StripeConfiguration.ApiKey = "sk_test_51QA5Q7HruNG7D4CWjJofbpFUQzfAYYwFoLdQ3IYo6qK0k7BfGQZBrxsxjdGqIoUnbFDxo897jPOT5yWLaq3HbjMM0058PsMGmh";

        var service = new PaymentIntentService();
        // ابحث عن المعرف في الاستعلام (إذا جاء من التوجيه)
        var paymentIntentId = Request.Query["payment_intent"].ToString();

        // إذا لم يأتِ من الاستعلام، فسنفترض أنه تم إرساله كـ "paymentIntentId" في نموذج الـ POST
        if (string.IsNullOrEmpty(paymentIntentId))
        {
            paymentIntentId = Request.Form["paymentIntentId"].ToString();
        }


        if (string.IsNullOrEmpty(paymentIntentId))
        {
            TempData["PaymentStatus"] = "حدث خطأ في عملية الدفع. لم يتم العثور على معرّف المعاملة.";
            // بدلاً من العودة لصفحة الدفع، العودة للصفحة الرئيسية أسهل
            return RedirectToAction("Index", "Home");
        }

        try
        {
            var paymentIntent = service.Get(paymentIntentId);

            // 2. التحقق من حالة الدفع
            if (paymentIntent.Status == "succeeded")
            {
                // 3. تحديث قاعدة البيانات لتأكيد الدفع (خطوة أكاديمية هامة)
                // قم بتفعيل هذا الكود الفعلي لتسجيل الدفع في قاعدة البيانات:
                /*
                var studentCourse = _studentCourseRepository.GetAll().FirstOrDefault(sc => sc.StudentId == studentId);
                if (studentCourse != null)
                {
                    // studentCourse.IsPaid = true; // افترض أن لديك حقل IsPaid
                    // _studentCourseRepository.Update(studentCourse);
                }
                */

                TempData["PaymentStatus"] = $"تم الدفع بنجاح! تم تسجيلك في الكورس. رقم المعاملة: {paymentIntent.Id}.";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // فشل أو انتظار التأكيد
                TempData["PaymentStatus"] = $"فشلت عملية الدفع أو ما زالت قيد المعالجة. الحالة: {paymentIntent.Status}";
                return RedirectToAction("Index", "Home");
            }
        }
        catch (StripeException ex)
        {
            TempData["PaymentStatus"] = $"خطأ في التحقق من الدفع: {ex.Message}";
            return RedirectToAction("Index", "Home");
        }
    }
    // ...

    // 🏠 عرض الصفحة الرئيسية (تم الحفاظ عليها)
    public IActionResult Index()
    {
        ViewBag.PaymentStatus = TempData["PaymentStatus"];

        var coursesWithInstructors = _courseRepository.GetAll()
            .Include(c => c.InstructorCourses)
            .ThenInclude(ic => ic.Instructor)
            .ToList();

        return View(coursesWithInstructors);
    }

    // منطق الانضمام للكورس (تم الحفاظ عليه)
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
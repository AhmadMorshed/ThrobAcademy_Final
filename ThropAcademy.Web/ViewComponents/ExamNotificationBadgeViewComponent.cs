using Microsoft.AspNetCore.Mvc;
using Throb.Service.Interfaces; // تأكد من استدعاء مسار الواجهات الخاص بك
using System.Threading.Tasks;
using System.Linq;

namespace ThropAcademy.Web.ViewComponents
{
    public class ExamNotificationBadgeViewComponent : ViewComponent
    {
        private readonly IExamRequestService _examRequestService;

        // حقن الخدمة التي تملكها بالفعل
        public ExamNotificationBadgeViewComponent(IExamRequestService examRequestService)
        {
            _examRequestService = examRequestService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // جلب جميع الطلبات وحساب عددها
            // ملاحظة: يفضل مستقبلاً إضافة دالة GetCountAsync في الخدمة لتحسين الأداء
            var allRequests = await _examRequestService.GetAllAsync();
            int count = allRequests?.Count ?? 0;

            return View(count);
        }
    }
}
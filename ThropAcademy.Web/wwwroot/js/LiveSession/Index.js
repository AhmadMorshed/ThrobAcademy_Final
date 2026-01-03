
// جافا سكريبت لتوجيه المستخدم إلى الرابط المدخل عند الضغط على زر "Join"
document.querySelectorAll('.btn-join-action').forEach(button => {
    button.addEventListener('click', function () {
        var courseId = this.id.split('_')[1]; // الحصول على معرّف الكورس
        var discordLink = document.getElementById('discordLink_' + courseId).value; // رابط Discord
        var vConnectLink = document.getElementById('vConnectLink_' + courseId).value; // رابط vConnect

        // تحديد الرابط للانتقال إليه (يفضل Discord إذا كان موجوداً)
        var linkToOpen = discordLink || vConnectLink;

        if (linkToOpen) {
            // فتح الرابط في نافذة جديدة
            window.open(linkToOpen, '_blank');
        } else {
            alert('الرجاء إدخال رابط صالح لـ Discord أو vConnect.');
        }

        // ⚠️ ملاحظة: إذا كان هذا الزر يستخدم لحفظ الروابط في قاعدة البيانات، يجب
        // عليك إرسال طلب AJAX إلى المتحكم (Controller) هنا بدلاً من فتح النافذة.
        // يجب إضافة event.preventDefault() إلى الدالة saveLinks في الـ View في هذه الحالة.
    });
});
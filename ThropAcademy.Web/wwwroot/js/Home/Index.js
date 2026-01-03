
// إخفاء شاشة التحميل بعد تحميل المحتوى
document.addEventListener('DOMContentLoaded', function () {
    const loadingScreen = document.getElementById('loading-screen');

    if (loadingScreen) {
        // نستخدم setTimeout لضمان ظهور الشاشة لفترة وجيزة
        setTimeout(() => {
            loadingScreen.classList.add('hidden');
        }, 500); // إخفاء بعد 500ms
    }
});
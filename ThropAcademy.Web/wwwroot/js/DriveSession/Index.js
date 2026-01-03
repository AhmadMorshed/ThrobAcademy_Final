
// إخفاء شاشة التحميل بعد تحميل المحتوى
document.addEventListener('DOMContentLoaded', function () {
    const loadingScreen = document.getElementById('loading-screen');

    if (loadingScreen) {
        // نستخدم setTimeout لضمان ظهور الشاشة لفترة وجيزة
        setTimeout(() => {
            loadingScreen.classList.add('hidden');
        }, 500); // إخفاء بعد 500ms
    }

    // JavaScript for smooth scrolling
    const scrollButton = document.querySelector('.scroll-down-btn');
    if (scrollButton) {
        scrollButton.addEventListener('click', function (e) {
            e.preventDefault();
            const targetId = this.getAttribute('href');
            const targetElement = document.querySelector(targetId);

            if (targetElement) {
                targetElement.scrollIntoView({
                    behavior: 'smooth'
                });
            }
        });
    }
});
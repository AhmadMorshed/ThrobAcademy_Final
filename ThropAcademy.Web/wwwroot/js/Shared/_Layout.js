/* wwwroot/js/layout-script.js */

/**
 * دالة إضافة/إزالة كلاس "navbar-scrolled" عند التمرير
 */
function scrollFunction() {
    var navbar = document.querySelector('.navbar');
    if (!navbar) return;

    // يتم التفعيل عندما يتجاوز التمرير 50px من الأعلى
    if (document.body.scrollTop > 50 || document.documentElement.scrollTop > 50) {
        navbar.classList.add('navbar-scrolled');
    } else {
        navbar.classList.remove('navbar-scrolled');
    }
}

// ربط الدالة بحدث التمرير
window.onscroll = scrollFunction;

// تشغيلها مرة واحدة عند التحميل (لتطبيقها في حال تم تحميل الصفحة وهي في وضع التمرير)
document.addEventListener('DOMContentLoaded', scrollFunction);
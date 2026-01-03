/* wwwroot/js/login-script.js */

// تأثير الضغط على الزر
document.getElementById('loginForm').addEventListener('submit', function (e) {
    const button = document.getElementById('loginButton');

    // نضمن أن الزر يرتد قليلاً عند الضغط
    button.style.transform = 'scale(0.98)';

    // إعادة التحجيم بعد فترة قصيرة
    setTimeout(() => {
        button.style.transform = 'scale(1)';
    }, 100);
});
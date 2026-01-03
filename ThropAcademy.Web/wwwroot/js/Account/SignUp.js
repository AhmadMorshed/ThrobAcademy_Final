/* wwwroot/js/signup-script.js */

// تأثير الضغط على الزر (للتغذية الراجعة البصرية)
document.getElementById('signupForm').addEventListener('submit', function (e) {
    const button = document.querySelector('.btn-submit');

    // نضمن أن الزر يرتد قليلاً عند الضغط
    button.style.transform = 'scale(0.98)';

    // إعادة التحجيم بعد فترة قصيرة
    setTimeout(() => {
        button.style.transform = 'scale(1)';
    }, 100);
});
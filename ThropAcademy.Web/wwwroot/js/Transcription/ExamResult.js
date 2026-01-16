document.addEventListener("DOMContentLoaded", function () {
    const progressCircle = document.getElementById('progressCircle');

    if (progressCircle) {
        // الحصول على النسبة من خاصية data-pct
        const percentage = parseFloat(progressCircle.getAttribute('data-pct'));

        // حساب الـ Offset (المحيط هو 565.48)
        const circumference = 565.48;
        const offset = circumference - (percentage / 100) * circumference;

        // تفعيل الانيميشن بعد تأخير بسيط لجعل التأثير ملحوظاً
        setTimeout(() => {
            progressCircle.style.strokeDashoffset = offset;
        }, 300);
    }
});
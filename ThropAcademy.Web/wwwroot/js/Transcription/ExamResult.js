document.addEventListener("DOMContentLoaded", () => {
    const progressEl = document.querySelector(".circular-progress-modern");
    const percentNumber = document.querySelector(".percent-number");

    if (progressEl && percentNumber) {
        const target = parseFloat(progressEl.getAttribute("data-percent"));
        let current = 0;
        const duration = 1500; // مدة أطول قليلاً لجمالية الحركة
        const stepTime = 15;
        const increment = target / (duration / stepTime);

        const counter = setInterval(() => {
            current += increment;
            if (current >= target) {
                percentNumber.innerText = target + "%";
                clearInterval(counter);
            } else {
                percentNumber.innerText = Math.ceil(current) + "%";
            }
        }, stepTime);
    }
});
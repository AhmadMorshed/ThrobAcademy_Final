document.addEventListener("DOMContentLoaded", () => {
    const checks = document.querySelectorAll(".q-check-input");
    const counter = document.querySelector(".badge-val");
    const autoTimeDisplay = document.getElementById("autoTime");
    const hiddenDuration = document.getElementById("DurationMinutes");
    const qCards = document.querySelectorAll(".q-card-premium");
    const filters = document.querySelectorAll(".f-btn");

    function updateLogic() {
        const checkedCount = document.querySelectorAll(".q-check-input:checked").length;
        counter.textContent = checkedCount;
        autoTimeDisplay.textContent = checkedCount; // 1 سؤال = 1 دقيقة
        hiddenDuration.value = checkedCount;
    }

    filters.forEach(btn => {
        btn.addEventListener("click", () => {
            filters.forEach(f => f.classList.remove("active"));
            btn.classList.add("active");
            const f = btn.dataset.filter;
            qCards.forEach(c => c.style.display = (f === 'all' || c.dataset.type === f) ? 'flex' : 'none');
        });
    });

    qCards.forEach(card => {
        card.addEventListener("click", (e) => {
            if (e.target.type !== "checkbox") {
                const cb = card.querySelector(".q-check-input");
                cb.checked = !cb.checked;
                cb.dispatchEvent(new Event("change"));
            }
        });
    });

    checks.forEach(c => {
        c.addEventListener("change", () => {
            c.closest(".q-card-premium").classList.toggle("is-selected", c.checked);
            updateLogic();
        });
    });

    updateLogic();
});
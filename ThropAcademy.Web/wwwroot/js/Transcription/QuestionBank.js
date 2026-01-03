document.addEventListener('DOMContentLoaded', () => {
    const searchInput = document.getElementById('smartSearch');
    const filterBtns = document.querySelectorAll('.filter-btn');
    const questions = document.querySelectorAll('.question-wrapper');
    const noResults = document.getElementById('noResults');

    function filterData() {
        const searchTerm = searchInput.value.toLowerCase().trim();
        const activeFilter = document.querySelector('.filter-btn.active').dataset.filter;
        let visibleCount = 0;

        questions.forEach(q => {
            const content = q.getAttribute('data-content');
            const type = q.getAttribute('data-type');

            const matchesSearch = content.includes(searchTerm);
            const matchesFilter = (activeFilter === 'all' || type === activeFilter);

            if (matchesSearch && matchesFilter) {
                q.style.display = 'block';
                visibleCount++;
            } else {
                q.style.display = 'none';
            }
        });

        // إظهار رسالة "لا توجد نتائج"
        noResults.style.display = visibleCount === 0 ? 'block' : 'none';
    }

    // حدث البحث
    searchInput.addEventListener('input', filterData);

    // حدث الفلترة عند الضغط على الأزرار
    filterBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            filterBtns.forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            filterData();
        });
    });
});
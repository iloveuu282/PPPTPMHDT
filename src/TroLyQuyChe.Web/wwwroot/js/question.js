(() => {
    const form = document.getElementById('question-form');
    if (!form) return;
    const question = document.getElementById('Question');
    const date = document.getElementById('ApplicableDate');
    const button = document.getElementById('ask-button');
    const updateCount = () => { document.getElementById('question-count').textContent = `${question.value.length}/2000`; };
    const updateDate = () => {
        document.getElementById('date-mode').textContent = date.value
            ? `Tra theo ngày đã chọn: ${date.value.split('-').reverse().join('/')}`
            : 'Mặc định: quy chế hiện hành · Giờ Việt Nam';
    };
    document.querySelectorAll('[data-question]').forEach(suggestion => suggestion.addEventListener('click', () => {
        question.value = suggestion.dataset.question;
        updateCount();
        question.focus();
    }));
    question.addEventListener('input', updateCount);
    question.addEventListener('keydown', event => {
        if ((event.ctrlKey || event.metaKey) && event.key === 'Enter') {
            event.preventDefault();
            if (!button.disabled) form.requestSubmit();
        }
    });
    date.addEventListener('change', updateDate);
    document.getElementById('clear-date').addEventListener('click', () => { date.value = ''; updateDate(); date.focus(); });
    form.addEventListener('submit', event => {
        if (button.disabled) { event.preventDefault(); return; }
        button.disabled = true;
        form.setAttribute('aria-busy', 'true');
        document.getElementById('ask-label').textContent = 'Đang tra cứu…';
        document.getElementById('submit-status').textContent = 'Đang tra cứu quy chế và lưu câu trả lời.';
    });
    // Browsers can restore a page, including disabled controls, from their back cache.
    window.addEventListener('pageshow', () => {
        button.disabled = false;
        form.setAttribute('aria-busy', 'false');
        document.getElementById('ask-label').textContent = 'Hỏi quy chế';
        document.getElementById('submit-status').textContent = '';
        updateCount(); updateDate();
    });
    updateCount(); updateDate();
})();

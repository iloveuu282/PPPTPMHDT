// Only leave-notice clauses have leave-duration bounds. Values for other topics are not submitted.
document.querySelectorAll('fieldset').forEach(fieldset => {
    const topic = fieldset.querySelector('select[name$=".Topic"]');
    const range = fieldset.querySelector('.leave-range');
    if (!topic || !range) return;
    function update() {
        const relevant = topic.value === 'LeaveNotice';
        range.hidden = !relevant;
        range.querySelectorAll('input').forEach(input => { input.disabled = !relevant; });
    }
    topic.addEventListener('change', update);
    update();
});

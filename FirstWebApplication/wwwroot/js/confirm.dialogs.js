// global confirm handler for elements with data-confirm
document.addEventListener('click', function (e) {
    const trigger = e.target.closest('[data-confirm]');
    if (!trigger) return;

    const message = trigger.getAttribute('data-confirm') || 'Are you sure?';
    if (!window.confirm(message)) {
        e.preventDefault();
        e.stopPropagation();
    }
});
// wwwroot/js/filter.popover.js
(function () {
    document.addEventListener('DOMContentLoaded', function () {
        if (!(window.bootstrap && bootstrap.Popover)) return;

        var filterTrigger = document.getElementById('filterPopoverBtn');
        if (!filterTrigger) return;

        var popFilter = new bootstrap.Popover(filterTrigger, {
            container: 'body',
            html: true,
            sanitize: false,
            content: function () {
                var el = document.getElementById('filter-popover-content');
                return el ? el.innerHTML : '';
            },
            placement: 'bottom',
            customClass: 'filter-popover-custom',
            trigger: 'manual'
        });

        filterTrigger.addEventListener('click', function (e) {
            e.preventDefault();
            try { popFilter.toggle(); } catch (err) { /* ignore */ }
        });

        document.addEventListener('click', function (e) {
            var popovers = Array.from(document.querySelectorAll('.popover'));
            var clickedInsidePopover = popovers.some(function (p) { return p.contains(e.target); });
            var clickedOnTrigger = filterTrigger && filterTrigger.contains(e.target);

            if (!clickedInsidePopover && !clickedOnTrigger) {
                try { if (popFilter) popFilter.hide(); } catch (err) { /* ignore */ }
            }
        });

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                try { if (popFilter) popFilter.hide(); } catch (err) { /* ignore */ }
            }
        });
    });
})();
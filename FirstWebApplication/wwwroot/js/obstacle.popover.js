// wwwroot/js/obstacle.popover.js
(function () {
    document.addEventListener('DOMContentLoaded', function () {
        const btn = document.getElementById('obstaclePopoverBtn');
        if (!btn || typeof bootstrap === 'undefined') return;

        const contentHtml = `
        <p>Select the type that best matches the obstacle. If unsure, pick <strong>Other</strong> and describe it.</p>
        <ul>
          <li><strong>Tower</strong> - large, permanent tower structures (e.g., observation towers, communication towers).</li>
          <li><strong>Crane</strong> - construction cranes, usually temporary structures used for lifting.</li>
          <li><strong>Building</strong> - permanent buildings or large man-made structures.</li>
          <li><strong>Power line</strong> - overhead electrical lines carrying power.</li>
          <li><strong>Mast</strong> - tall structures used for telecom, radio or observation purposes.</li>
          <li><strong>Other</strong> — anything not covered by the categories above (e.g., balloons, temporary objects, unusual structures).</li>
        </ul>
      `;

        const pop = new bootstrap.Popover(btn, {
            content: contentHtml,
            html: true,
            trigger: 'manual',
            sanitize: false
        });

        btn.addEventListener('click', (e) => {
            e.preventDefault();
            if (document.querySelector('.popover')) {
                pop.hide();
            } else {
                pop.show();
            }
        });

        function hideIfOutside(ev) {
            const popEl = document.querySelector('.popover');
            if (!popEl) return;
            if (btn.contains(ev.target) || popEl.contains(ev.target)) return;
            pop.hide();
        }

        document.addEventListener('click', hideIfOutside);
        document.addEventListener('keydown', (ev) => {
            if (ev.key === 'Escape' || ev.key === 'Esc') {
                if (document.querySelector('.popover')) pop.hide();
            }
        });
    });
})();
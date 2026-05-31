// ── InventoryPro Global JavaScript ────────────────────────────────────────

// Auto-dismiss alerts after 5 seconds
document.addEventListener('DOMContentLoaded', function () {

    // Auto-close success alerts
    const alerts = document.querySelectorAll('.alert-success, .alert-info');
    alerts.forEach(function (alert) {
        setTimeout(function () {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            if (bsAlert) bsAlert.close();
        }, 5000);
    });

    // Highlight active navbar link based on current URL
    const currentPath = window.location.pathname.toLowerCase();
    document.querySelectorAll('.navbar-nav .nav-link').forEach(function (link) {
        const href = link.getAttribute('href');
        if (href && currentPath.startsWith(href.toLowerCase()) && href !== '/') {
            link.classList.add('active');
        }
    });

    // Confirm before delete forms
    document.querySelectorAll('form[data-confirm]').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            const msg = form.getAttribute('data-confirm');
            if (!confirm(msg || 'Are you sure?')) {
                e.preventDefault();
            }
        });
    });

    // Add loading spinner to submit buttons on form submit
    document.querySelectorAll('form').forEach(function (form) {
        form.addEventListener('submit', function () {
            const btn = form.querySelector('button[type="submit"]');
            if (btn && !btn.disabled) {
                setTimeout(function () {
                    btn.innerHTML =
                        '<span class="spinner-border spinner-border-sm me-2"></span>' +
                        'Processing...';
                    btn.disabled = true;
                }, 10);
            }
        });
    });
});

// Format currency as South African Rand
function formatRand(amount) {
    return 'R ' + parseFloat(amount).toFixed(2)
        .replace(/\d(?=(\d{3})+\.)/g, '$&,');
}
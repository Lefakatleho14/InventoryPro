// ── InventoryPro Website Scripts ──────────────────────────────────────────

document.addEventListener('DOMContentLoaded', function () {

    // Highlight active nav link
    const currentPage = window.location.pathname.split('/').pop();
    document.querySelectorAll('.nav-links a').forEach(link => {
        const href = link.getAttribute('href');
        if (href === currentPage ||
            (currentPage === '' && href === 'index.html')) {
            link.classList.add('active');
        }
    });

    // Smooth scroll for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({ behavior: 'smooth', block: 'start' });
            }
        });
    });

    // Contact form submission
    const contactForm = document.getElementById('contactForm');
    if (contactForm) {
        contactForm.addEventListener('submit', function (e) {
            e.preventDefault();
            const btn = contactForm.querySelector('button[type="submit"]');
            btn.textContent = 'Message Sent!';
            btn.disabled = true;
            btn.style.background = '#16A34A';
            setTimeout(() => {
                contactForm.reset();
                btn.textContent = 'Send Message';
                btn.disabled = false;
                btn.style.background = '';
            }, 3000);
        });
    }

    // Login form submission
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', function (e) {
            e.preventDefault();
            const email = document.getElementById('email').value;
            const password = document.getElementById('password').value;
            const errorDiv = document.getElementById('loginError');

            if (!email || !password) {
                errorDiv.style.display = 'flex';
                errorDiv.textContent = '⚠️ Please enter both email and password.';
                return;
            }

            // Demo: redirect to features page
            errorDiv.style.display = 'none';
            window.location.href = 'features.html';
        });
    }

    // Animate stats on scroll
    const statNumbers = document.querySelectorAll('.stat-number[data-target]');
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                animateNumber(entry.target);
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.5 });

    statNumbers.forEach(el => observer.observe(el));

    function animateNumber(el) {
        const target = parseInt(el.getAttribute('data-target'));
        const prefix = el.getAttribute('data-prefix') || '';
        const suffix = el.getAttribute('data-suffix') || '';
        const duration = 1500;
        const step = target / (duration / 16);
        let current = 0;

        const timer = setInterval(() => {
            current += step;
            if (current >= target) {
                current = target;
                clearInterval(timer);
            }
            el.textContent = prefix + Math.floor(current).toLocaleString() + suffix;
        }, 16);
    }
});

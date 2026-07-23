// ===== Navbar shadow on scroll =====
document.addEventListener("DOMContentLoaded", function () {
    const navbar = document.querySelector(".navbar");

    if (navbar) {
        window.addEventListener("scroll", function () {
            if (window.scrollY > 10) {
                navbar.classList.add("scrolled");
            } else {
                navbar.classList.remove("scrolled");
            }
        });
    }

    // ===== Scroll-reveal for elements with class "reveal" =====
    const revealEls = document.querySelectorAll(".reveal");

    const revealObserver = new IntersectionObserver(
        function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add("is-visible");
                    revealObserver.unobserve(entry.target);
                }
            });
        },
        { threshold: 0.15 }
    );

    revealEls.forEach(function (el) {
        revealObserver.observe(el);
    });

    // ===== Animated stat counters =====
    const counters = document.querySelectorAll(".stat-number[data-target]");

    const counterObserver = new IntersectionObserver(
        function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    animateCounter(entry.target);
                    counterObserver.unobserve(entry.target);
                }
            });
        },
        { threshold: 0.5 }
    );

    counters.forEach(function (counter) {
        counterObserver.observe(counter);
    });

    function animateCounter(el) {
        const target = el.getAttribute("data-target");
        const prefix = el.getAttribute("data-prefix") || "";
        const suffix = el.getAttribute("data-suffix") || "";
        const numericTarget = parseFloat(target);
        const duration = 1200;
        const startTime = performance.now();

        function step(now) {
            const progress = Math.min((now - startTime) / duration, 1);
            const eased = 1 - Math.pow(1 - progress, 3);
            const current = (numericTarget * eased).toFixed(
                numericTarget % 1 !== 0 ? 1 : 0
            );
            el.textContent = prefix + current + suffix;

            if (progress < 1) {
                requestAnimationFrame(step);
            } else {
                el.textContent = prefix + numericTarget + suffix;
            }
        }

        requestAnimationFrame(step);
    }
});
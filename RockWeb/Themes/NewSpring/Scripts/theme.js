document.addEventListener("DOMContentLoaded", function () {
    // Initialize Skrollr
    var s = skrollr.init();

    setTimeout(function () {
        skrollr.get().refresh();
    }, 0);

    if (s.isMobile()) {
        s.destroy();
    }

    if (window.innerWidth > 667) {
        // Desktop
        offset = "30px";
    } else {
        // Mobile
        offset = "15px";
    }

    window.scroll = new SmoothScroll("a[data-scroll]", {
        speed: 500,
        speedAsDuration: true,
        //   header: '#navigation-wrapper',
        offset: offset,
        easing: "easeInOutCubic",
    });
});

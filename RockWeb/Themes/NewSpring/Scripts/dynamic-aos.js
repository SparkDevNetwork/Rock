document.addEventListener(
  "DOMContentLoaded",
  function () {
    let content = document.querySelector('#content');
    content.querySelectorAll("h1:not([data-aos], [data-aos] *), h2:not([data-aos], [data-aos] *), h3:not([data-aos], [data-aos] *), h4:not([data-aos], [data-aos] *), h5:not([data-aos], [data-aos] *), h6:not([data-aos], [data-aos] *), p:not([data-aos], [data-aos] *), i:not([data-aos], [data-aos] i, #cms-admin-footer i), .dropdown-toggle").forEach(function (a, t) {
        a.dataset.aos = "fade-up";
    }),
    AOS.init({
      disable: "phone",
      startEvent: "DOMContentLoaded",
      initClassName: "aos-init",
      animatedClassName: "aos-animate",
      useClassNames: !1,
      disableMutationObserver: !1,
      debounceDelay: 50,
      throttleDelay: 99,
      offset: 100,
      delay: 0,
      duration: 400,
      easing: "ease-in-out",
      once: !1,
      mirror: !1,
      anchorPlacement: "top-bottom",
    });
  },
  !1
);

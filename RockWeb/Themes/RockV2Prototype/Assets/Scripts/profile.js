Sys.Application.add_load(function() {
    Rock.controls.priorityNav.initialize({controlId: 'overflow-nav'});

    $('#overflow-nav').on('show.bs.dropdown', function (e) {
        $(this).closest('.zone-nav').addClass('overflow-visible');
    }).on('hide.bs.dropdown', function (e) {
        $(this).closest('.zone-nav').removeClass('overflow-visible');
    })

    const header = document.querySelector("#profilenavigation");
    const profileImage = document.querySelector("#profile-image");

    if (profileImage) {
        const profileImageOptions = {
        rootMargin: `${header.getBoundingClientRect().bottom * -1}px`,
        threshold: 0
        };

        const profileImageObserver = new IntersectionObserver(function(
        entries,
        profileImageObserver
        ) {
        entries.forEach(entry => {
            if (entry.rootBounds && !entry.isIntersecting) {
                header.classList.add("nav-scrolled");
            } else {
                header.classList.remove("nav-scrolled");
            }
        });
        }, profileImageOptions);

        profileImageObserver.observe(profileImage);
    } else {
        console.log("No Bio Block");
        header.classList.add("nav-scrolled");
        header.classList.add("nav-scrolled-notransition");
    }
});

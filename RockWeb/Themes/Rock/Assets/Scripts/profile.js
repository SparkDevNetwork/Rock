Sys.Application.add_load(function() {
    Rock.controls.priorityNav.initialize({controlId: 'overflow-nav'});

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
        console.log("no profile image");
        header.classList.add("nav-scrolled");
    }
});

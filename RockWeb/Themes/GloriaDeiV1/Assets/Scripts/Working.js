//{% include '~~/Assets/LavaC:\github\RockNew\RockWeb\Themes\GloriaDeiV1\Assets\Scripts/Working.lava' %}

$(() => {

    // -------------------------------
    // - sidebar toggler
    // -------------------------------
    $('.js-sidebar-toggle').on('click', () => {
        $('.js-sidebar-toggle').add('.js-global-sidebar-menu').toggleClass('in')
    });


    // -------------------------------
    // - sidebar menu
    // -------------------------------
    $('.js-nav-level-one').on('click', function(e) {
        e.preventDefault();

        if ($(this).hasClass('in')) {
            $(this).removeClass('in');
        } else {
            $('.js-nav-level-one').removeClass('in');
            $(this).addClass('in');
        }
    });

})




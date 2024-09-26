
export default function navbar() {
    $(document).ready(()=> {

        // click event for mobile nav
        $('.js-mobile-nav-toggler').click(() => {
            $('.js-mobile-nav-toggler').toggleClass('active');
        });

        //mega menu js
        $('.js-mega-super').each((i, obj) => {
            $(obj).on('mouseenter', () => {
                let megaData = $(obj).data('mega');
                let subnavWidth;

                // locate correct cta & display
                $('.js-mega-menu-cta-item-custom').each((i, objx) => {
                    let ctaMegaData = $(objx).data('mega');
                    $(objx).removeClass('in');

                    if (ctaMegaData == megaData) {
                        $(objx).addClass('in');
                    }
                });

                // locate correct subnav, display it, and set width of subnav section
                $('.js-mega-menu-subnav-item').each((i, objy) => {
                    let subnavMegaData = $(objy).data('mega');
                    $(objy).removeClass('in');

                    if (subnavMegaData == megaData) {
                        $(objy).addClass('in');

                        subnavWidth = $(objy).innerWidth();

                        $('.js-mega-menu-subnav-section').css('width',`${subnavWidth}px`)

                    }
                });

                // move mega menu under the hovered nav item
                let windowWidth = $(window).width();
                let megaMenuWidth = $('.mega-menu-cta-section').width() + subnavWidth;
                let megaMenuContainerWidth = $('.js-mega-menu-container').width();
                let megaMenuContainerGutter = (windowWidth - megaMenuContainerWidth) / 2;
                let navItemLeftPosition = $(obj).offset().left;
                let megaMenuLeftPosition = 0;

                if ((navItemLeftPosition + megaMenuWidth) < (windowWidth - 30)) {
                    megaMenuLeftPosition = navItemLeftPosition - megaMenuContainerGutter;
                } else {
                    megaMenuLeftPosition = windowWidth - megaMenuWidth - megaMenuContainerGutter - 30;
                }

                $('.js-mega-menu').css('opacity',`1`);
                $('.js-mega-menu').css('visibility',`visible`);
                $('.js-mega-menu').css('left',`${megaMenuLeftPosition}px`);
            });
        });

        // hide mega menu on mouseleave
        $('.js-main-nav').on('mouseleave', () => {
            $('.js-mega-menu').css('opacity',`0`);
            $('.js-mega-menu').css('left',`0`);
            $('.js-mega-menu').css('visibility',`hidden`);
            $('.js-mega-menu-cta-item-custom').removeClass('in');
            $('.js-mega-menu-subnav-item').removeClass('in');
        });


        // set main element section margin top (to account for fixed position header)
        setTopSpacing();
    });

    $(window).on('resize', ()=> {
        setTopSpacing();
    });

    function setTopSpacing() {
        let headerHeight = $('header').height();
        $('main').css('marginTop',`${headerHeight}px`);
    }
}

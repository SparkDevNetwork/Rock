// Table of Contents
// 1. Site Header
// 1. Scrolled In
// 2. Animation One
// 3. Slide Up Item Animation + CheckStuck + text-rotator + js overlaid video
// 3. Site Hero
// #. Helper Functions


//-------------------------
// 1. Site Header
//-------------------------
$(document).ready(() => {
    var scrollTop = $(window).scrollTop();

    scrollTop = $(window).scrollTop();
    $('.js-site-header').toggleClass('stuck',(scrollTop > 0));

    $(window).scroll(() => {
        scrollTop = $(window).scrollTop();
        $('.js-site-header').toggleClass('stuck',(scrollTop > 0));
    });
});




//-------------------------
// 1. Scrolled In
//-------------------------
$(document).ready(() => {
    let scrollInItems = $('.js-scroll-in');
    var scrollTimeout;
    var throttle = 50;

    if ( $(scrollInItems).length ) {


        $(scrollInItems).each((i, obj) => {
            updateScrolled(obj);
        });


        $(window).scroll(() => {
            if (!scrollTimeout) {
                scrollTimeout = setTimeout(function () {
                    $(scrollInItems).each((i, obj) => {
                        updateScrolled(obj);
                    });
                    scrollTimeout = null;
                }, throttle);
            }
        });
    }
});






//-------------------------
// 2. Animation One
//-------------------------

$(document).ready(() => {
    if ($(window).width() >= 768){
        doAnimationOne();
    }

    // $(window).on('resize', () => {
    //     $('.js-animation-1-body').each((i, obj) => {
    //         // set min-height & width on banner body so text doesn't resize
    //         resizeAnimationOne(obj);
    //     });
    // });
});

const doAnimationOne = () => {
    $('.js-animation-1').each((i, obj) => {
        $(obj).parent().css('height','fit-content');

        // set min-height & width on banner body so text doesn't resize
        let body = $(obj).find('.js-animation-1-body');
        resizeAnimationOne(body);

        $(obj).css('width','12px');

        setTimeout(() => {
            $(obj).addClass('in');
        } ,500);

        $(obj).on('animationend', () => {
            $(obj).css('width','100%');
        });

        $(body).on('animationend', () => {
            $(body).css('transform','TranslateY(0)');
            $(body).css('min-width','unset').css('min-height','unset');
        });
    });
}

const resizeAnimationOne = (body) => {
     let bodyHeight = $(body).parent().outerHeight();
     let bodyWidth = $(body).parent().outerWidth();

     $(body).css('min-width',`${bodyWidth}px`).css('min-height',`${bodyHeight}px`);
}







//-------------------------
// 3. Slide Up Item Animation + CheckStuck + text-rotator + js overlaid video + js scroll left
//-------------------------
$(document).ready(() => {
    $('.js-slide-up-item').find('>:first-child').each((i, obj) => {
        $(obj).on('animationend', () => {
            $(obj).css('transform','TranslateY(0)');
        });
    });



    // text rotator
    $('.js-text-rotator').each((i, obj) => {
        let items = $(obj).find('.js-text-rotator-item');
        let totalItems = items.length;
        let maxHeight = 0;

        $(items).each((x, objx) => {
            let height = $(objx).height();

            if ( maxHeight == 0 || height > maxHeight ) {
                maxHeight = height + 10;
            }
        });

        $(obj).css('max-height',`${ maxHeight }px`).css('height',`${ maxHeight }px`);

        setInterval(() => {
            let activeItemIndex = -1;

            $(items).each((i, obj) => {
                if ($(obj).hasClass('active')) {
                    activeItemIndex = $(obj).index();
                }

                $(obj).removeClass('active');
            });

            if ((activeItemIndex + 1) < totalItems || activeItemIndex == -1) {
                activeItemIndex ++;
            } else {
                activeItemIndex = 0;
            }

            $(items[activeItemIndex]).addClass('active');
        }, 5000);
    });

//    setTimeout(() => {
//        $('.js-text-rotator-item').each((i, obj) => {
//            $(obj).css('transition','transform 1.2s ease');
//        });
//    }, 300);


    // - js overlaid video
     $('.js-overlaid-video').each((i, obj) => {
        let plyr = $(obj).find('.plyr');
        let id = $(obj).attr('id');
        console.log(id);

        $(plyr).on('click', () => {
            setTimeout(() => {
                if($(plyr).hasClass('plyr--playing')) {
                    $(`[data-hide-on-play="#${ id }"]`).addClass('out');
                    console.log('playing');
                } else {
                    $(`[data-hide-on-play="#${ id }"]`).removeClass('out');
                    console.log('not playing');
                }
            }, 100);
        });
    });


    // - js scroll left
    const centeredLocation = 300;
    const speed = .6;

    $('.js-scroll-left').each((i, obj) => {
        getTranslateLeft(obj, centeredLocation, speed);

        $(window).on('scroll', () => {
            getTranslateLeft(obj, centeredLocation, speed);
        });
    });
});





//-------------------------
// 4. Site Hero
//-------------------------
$(document).ready(() => {
    $('header.site-header').on('mouseleave', () => {
        $('.js-primary-nav-item a').removeClass('hovered');
        $('.js-mega-menu-section').removeClass('active');
    });

    // mega menu js
    $('.js-primary-nav-item a').each((i, obj) => {
        let pageId = $(obj).data('page');

        $(obj).on('mouseenter', () => {
            $('.js-primary-nav-item a').removeClass('hovered');
            $(obj).addClass('hovered');

            $('.js-mega-menu-section').removeClass('active');
            $(`.js-mega-menu-section[data-page="${ pageId }"]`).addClass('active');
        });
    });

    // sidebar js
    $('.js-sidebar-nav-item a').each((i, obj) => {
        let pageId = $(obj).data('page');

        $(obj).on('click', (e) => {
            e.preventDefault();
            $('.js-sidebar-menu-section').removeClass('active');
            $(`.js-sidebar-menu-section[data-page="${ pageId }"]`).addClass('active');
        });
    });

    $('.js-sidebar-menu-back a').on('click', () => {
        $('.js-sidebar-menu-section').removeClass('active');
    });

    // search input js
    $('.js-sidebar-menu-section .js-search').on('keypress', (e) => {
        if (e.charCode == 13) {
            let value = $('.js-sidebar-menu-section .js-search').val();
            window.location.replace(`/search?q=${value}`)
        }
    });


});









//-------------------------
// #. Helper Functions
//-------------------------

// ------------ Check if Element Scrolled Into View ------------
// updateScrolled('. box2')
function updateScrolled(elem) {
    var docViewTop = $(window).scrollTop();
    var docViewBottom = docViewTop + window.innerHeight;
    var elemTop = $(elem).offset().top;

    if ((elemTop <= docViewBottom) && (elemTop >= docViewTop)) {
        $(elem).addClass('scrolled-in');
    }
}



// ------------ getTranslateLeft (.js-scroll-left) ------------
const getTranslateLeft = (obj, centeredLocation, speed) => {
    let top = obj.getBoundingClientRect().top;
    let translateLeft = speed * (top - centeredLocation);
    $(obj).css('transform',`translateX(${translateLeft}px`);
}









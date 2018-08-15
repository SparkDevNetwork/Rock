(function ($) {
    $('body').addClass('is-loading');
    var menuSelector = $('#sidebar ul li a');

    $(document)
        .ready(function () {
            if(window.location.hash) {
                $("html, body")
                    .removeClass('is-loading')
                    .animate({ scrollTop: $(window.location.hash).scrollTop() }, 1);
            } else {
                setTimeout(function () {
                    $("html, body")
                        .animate({scrollTop: $(document).height()}, 1)
                        .animate({scrollTop: $("#download").scrollTop()}, 1);
                    $('body').removeClass('is-loading').addClass('init-paroller');
                }, 1200);
            }
            $('.show-more').on('click', function () {
               $(this).toggleClass('active');
               if($(this).hasClass('active')) {
                   $(this).next('.popover').slideDown();
               } else {
                   $(this).next('.popover').slideUp();
               }
            });
            $('a[href^="#"]').on('click', function (e) {
                e.preventDefault();
                var target = this.hash;
                $target = $(target);

                $('html, body').animate({
                    'scrollTop': $target.offset().top
                }, 600, 'linear', function () {
                    window.location.hash = target;
                    $(document).on("scroll", initScroll(menuSelector));
                });
            });
        })
        .on('scroll', function () {
            initScroll(menuSelector);
        });

    function initScroll(menuSelector) {
        var scrollPosition = $(document).scrollTop() + 5;

        menuSelector.each(function () {
            var currLink = $(this);
            var refElement = $(currLink.attr("href"));
            if (refElement.position().top <= scrollPosition && refElement.position().top + refElement.height() > scrollPosition) {
                menuSelector.removeClass("active");
                currLink.addClass("active");
            }
            else {
                currLink.removeClass("active");
            }
        });
    }

})(jQuery);
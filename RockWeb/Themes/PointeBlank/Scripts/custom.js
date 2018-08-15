$(function () {
    var offset = 10;
    $('a.page-scroll').bind('click', function (event) {
        if ($(this).attr("href") == '#page-top') {

            offset = 0;
        }
        else {
            offset = 0;
        }
        var $anchor = $(this);
        $('html, body').stop().animate({
            scrollTop: $($anchor.attr('href')).offset().top + offset
        }, 1500, 'easeInOutExpo');

        event.preventDefault();
    });

    $('.carousel').carousel({
        interval: 1000 * 6
    });
});
var bodyScroll;

$(function () {
    $(window).on('resize', function () {
        resizeBody();
    });
});

Sys.Application.add_load(function () {

    if (bodyScroll) {
        bodyScroll.destroy();
        bodyScroll = null;
    }

    resizeBody();

    if ($('.checkin-scroll-panel').length) {
      bodyScroll = new IScroll('.checkin-scroll-panel', {
        scrollbars: true,
        mouseWheel: true,
        interactiveScrollbars: true,
        shrinkScrollbars: 'scale',
        fadeScrollbars: false,
        scrollbars: 'custom'
      });
    }
});

function resizeBody() {
    var headerHeight = $('.checkin-header').outerHeight(true);
    var footerHeight = $('.checkin-footer').outerHeight(true);
    var bodyWidth = $('.checkin-body').width();

    $('.checkin-scroll-panel').css('top', headerHeight);
    $('.checkin-scroll-panel').css('bottom', footerHeight);
    $('.checkin-scroll-panel').css('width', bodyWidth);
}
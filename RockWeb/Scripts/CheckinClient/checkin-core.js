var bodyScroll;

Sys.Application.add_load(function () {

    resizeBody();
    bodyScroll = new IScroll('.checkin-scroll-panel', {
        scrollbars: true,
        mouseWheel: true,
        interactiveScrollbars: true,
        shrinkScrollbars: 'scale',
        fadeScrollbars: false,
        scrollbars: 'custom'
    });

    $(window).on('resize', function () {
        resizeBody();
    });

});

function resizeBody() {
    var headerHeight = $('.checkin-header').outerHeight(true);
    var footerHeight = $('.checkin-footer').outerHeight(true);
    var bodyWidth = $('.checkin-body').width();

    $('.checkin-scroll-panel').css('top', headerHeight);
    $('.checkin-scroll-panel').css('bottom', footerHeight);
    $('.checkin-scroll-panel').css('width', bodyWidth);
}
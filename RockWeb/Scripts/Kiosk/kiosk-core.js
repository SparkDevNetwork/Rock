var bodyScroll;

Sys.Application.add_load(function () {

    if ($(".js-kioskscrollpanel").length) {

        resizeBody();
        bodyScroll = new IScroll('.js-kioskscrollpanel .scrollpanel', {
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
    } 

});

function resizeBody() {
    var headerHeight = $('.js-kioskscrollpanel header').outerHeight(true);
    var footerHeight = $('.js-kioskscrollpanel footer').outerHeight(true);
    var bodyWidth = $('.js-kioskscrollpanel .js-scrollcontainer').width();

    $('.scrollpanel').css('top', headerHeight);
    $('.scrollpanel').css('bottom', footerHeight);
    $('.scrollpanel').css('width', bodyWidth);
}
var bodyScroll;

Sys.Application.add_load(function () {

    if ($(".js-pnlgivingunitselect").length) {

        resizeBody();
        bodyScroll = new IScroll('.js-pnlgivingunitselect .scrollpanel', {
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
    var headerHeight = $('.js-pnlgivingunitselect header').outerHeight(true);
    var footerHeight = $('.js-pnlgivingunitselect footer').outerHeight(true);
    var bodyWidth = $('.js-pnlgivingunitselect main').width();

    $('.scrollpanel').css('top', headerHeight);
    $('.scrollpanel').css('bottom', footerHeight);
    $('.scrollpanel').css('width', bodyWidth);
}
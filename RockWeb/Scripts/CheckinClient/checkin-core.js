var bodyScroll;

$(function () {
    $(window).on('resize', function () {
        resizeBody();
    });
});

Sys.Application.add_load(function () {

    if (bodyScroll) {
        try {
            bodyScroll.destroy();
        } catch (e) { }
        bodyScroll = null;
    }

    // wait until everything is done rendering before doing the resizeBody
    setTimeout(function () {

        resizeBody();

        if ($('.checkin-scroll-panel').length) {
            bodyScroll = new IScroll('.checkin-scroll-panel',
                {
                    mouseWheel: true,
                    interactiveScrollbars: true,
                    shrinkScrollbars: 'scale',
                    fadeScrollbars: false,
                    scrollbars: 'custom',
                    click: false,
                    preventDefaultException: { tagName: /.*/ }
                });
        }
    }, 1);

});

function resizeBody() {

    // It is possible that checkin-body might not yet be visible due to active jquery
    // fadein animations. (If it isn't visible, bodyWidth will be 0, which would cause
    // checkin-scroll-panel to disaooear). It might be not visible just for a couple of
    // milliseconds, so the timing has to be very close for this problem to occur.
    // So, to prevent this problem, wait for up to 500 milliseconds for it to become visible.
    var checkinBodyVisible = $('.checkin-body').is(':visible');
    if (!checkinBodyVisible) {

        // not visible yet, so wait 50 milliseconds and try again
        var retryCount = 0;
        var checkVisibleInterval = setInterval(function () {
            var pollCheckinBodyVisible = $('.checkin-body').is(':visible');
            retryCount++;
            if (pollCheckinBodyVisible || retryCount > 10) {

                // stop polling once it becomes visible (or we retried more than 10 times)
                clearInterval(checkVisibleInterval);

                if (pollCheckinBodyVisible) {
                    // if it is visible, call resizeBody again
                    resizeBody();
                }
            }
        }, 50);

        return;
    }

    var footerHeight = $('.checkin-footer').outerHeight(true);
    var bodyWidth = $('.checkin-body').width();
    var bodyPosition = $('.checkin-body').position();

    $('.checkin-scroll-panel').css('top', bodyPosition.top).css('bottom', footerHeight).css('width', bodyWidth);
}

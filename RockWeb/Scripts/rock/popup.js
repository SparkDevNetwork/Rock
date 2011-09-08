(function ($) {

    $.fn.popup = function (options) {

        debug(this);

        var opts = $.extend({}, $.fn.popup.defaults, options);

        return this.each(function () {

            $this = $(this);

            var o = $.meta ? $.extend({}, opts, $this.data()) : opts;

            var target = {
                href: false,
                inline: false,
                iframe: false
            }

            target.href = o.href ? o.href : $this.attr('href');

            if (target.href && target.href.charAt(0) == '#') {
                target.inline = true;
            }
            else {
                target.iframe = true;
            }

            var cbOptions = $.extend({}, o, target);

            $this.colorbox(cbOptions);

        });

    };

    $.fn.popup.close = function () {
        $.fn.colorbox.close();
    };

    $.fn.popup.defaults = {
        transition: 'elastic',
        speed: 350,
        title: false,
        rel: false,
        width: '562px',
        height: false,
        innerWidth: false,
        innerHeight: false,
        initialWidth: '300',
        initialHeight: '100',
        maxWidth: false,
        maxHeight: false,
        scalePhotos: true,
        scrolling: true,
        html: false,
        photo: false,
        opacity: 0.85,
        open: false,
        returnFocus: true,
        fastIframe: true,
        preloading: true,
        overlayClose: true,
        escKey: true,
        arrowKey: true,
        loop: true,
        slideshow: false,
        slideshowSpeed: 2500,
        slideshowAuto: true,
        slideshowStart: "start slideshow",
        slideshowStop: "stop slideshow",
        current: "image {current} of {total}",
        previous: "previous",
        next: "next",
        close: "close",
        onOpen: false,
        onLoad: false,
        onComplete: false,
        onCleanup: false,
        onClosed: false,
        top: false,
        bottom: false,
        left: false,
        right: false,
        fixed: false,
        data: false
    };

    function debug($obj) {
        if (window.console && window.console.log)
            window.console.log('popup selection count: ' + $obj.size());
    };

})(jQuery);    
        
        
(function ($) {

    $.fn.rockFadeIn = function () {
        this.css("display", "none");
        this.fadeIn(400);
    }

}(jQuery));

// Paul Irish's debounced resize event
(function (c, b) { var a = function (g, d, e) { var h; return function f() { var k = this, j = arguments; function i() { if (!e) { g.apply(k, j) } h = null } if (h) { clearTimeout(h) } else { if (e) { g.apply(k, j) } } h = setTimeout(i, d || 100) } }; jQuery.fn[b] = function (d) { return d ? this.bind("resize", a(d)) : this.trigger(b) } })(jQuery, "smartresize");

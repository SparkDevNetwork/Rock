System.register([], function (exports_1, context_1) {
    "use strict";
    var __moduleName = context_1 && context_1.id;
    function smoothScrollToTop() {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
    exports_1("smoothScrollToTop", smoothScrollToTop);
    return {
        setters: [],
        execute: function () {
            exports_1("default", {
                smoothScrollToTop: smoothScrollToTop
            });
        }
    };
});
//# sourceMappingURL=Page.js.map
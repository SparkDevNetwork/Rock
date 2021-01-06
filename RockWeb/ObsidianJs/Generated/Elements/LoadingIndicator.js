System.register(["../Vendor/Vue/vue.js"], function (exports_1, context_1) {
    "use strict";
    var vue_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_js_1.defineComponent({
                name: 'LoadingIndicator',
                template: "\n<div class=\"text-muted\">\n    Loading...\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=LoadingIndicator.js.map
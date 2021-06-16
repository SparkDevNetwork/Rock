System.register(["vue"], function (exports_1, context_1) {
    "use strict";
    var vue_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'LoadingIndicator',
                template: "\n<div class=\"text-muted\">\n    Loading...\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=LoadingIndicator.js.map
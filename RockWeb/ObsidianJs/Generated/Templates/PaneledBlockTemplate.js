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
                name: 'PaneledBlockTemplate',
                template: "<div class=\"panel panel-block\">\n    <div class=\"panel-heading\">\n        <h1 class=\"panel-title\">\n            <slot name=\"title\" />\n        </h1>\n    </div>\n    <div class=\"panel-body\">\n        <div class=\"block-content\">\n            <slot />\n        </div>\n    </div>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=PaneledBlockTemplate.js.map
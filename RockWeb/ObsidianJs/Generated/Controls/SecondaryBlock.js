System.register(["../Vendor/Vue/vue.js", "../Store/Index.js"], function (exports_1, context_1) {
    "use strict";
    var vue_js_1, Index_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            },
            function (Index_js_1_1) {
                Index_js_1 = Index_js_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_js_1.defineComponent({
                name: 'SecondaryBlock',
                computed: {
                    isVisible: function () {
                        return Index_js_1.default.state.areSecondaryBlocksShown;
                    }
                },
                template: "\n<div class=\"secondary-block\">\n    <slot v-if=\"isVisible\" />\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=SecondaryBlock.js.map
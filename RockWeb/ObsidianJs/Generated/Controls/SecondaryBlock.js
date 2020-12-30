define(["require", "exports", "../Vendor/Vue/vue.js", "../Store/Index.js"], function (require, exports, vue_js_1, Index_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = vue_js_1.defineComponent({
        name: 'SecondaryBlock',
        computed: {
            isVisible: function () {
                return Index_js_1.default.state.areSecondaryBlocksShown;
            }
        },
        template: "<div class=\"secondary-block\">\n    <slot v-if=\"isVisible\" />\n</div>"
    });
});
//# sourceMappingURL=SecondaryBlock.js.map
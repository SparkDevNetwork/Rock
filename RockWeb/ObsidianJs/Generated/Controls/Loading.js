define(["require", "exports", "../Elements/LoadingIndicator.js", "../Vendor/Vue/vue.js"], function (require, exports, LoadingIndicator_js_1, vue_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = vue_js_1.defineComponent({
        name: 'Loading',
        components: {
            LoadingIndicator: LoadingIndicator_js_1.default
        },
        props: {
            isLoading: {
                type: Boolean,
                required: true
            }
        },
        template: "\n<div>\n    <slot v-if=\"!isLoading\" />\n    <LoadingIndicator v-else />\n</div>"
    });
});
//# sourceMappingURL=Loading.js.map
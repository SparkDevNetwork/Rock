System.register(["../Elements/LoadingIndicator.js", "../Vendor/Vue/vue.js"], function (exports_1, context_1) {
    "use strict";
    var LoadingIndicator_js_1, vue_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (LoadingIndicator_js_1_1) {
                LoadingIndicator_js_1 = LoadingIndicator_js_1_1;
            },
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_js_1.defineComponent({
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
            }));
        }
    };
});
//# sourceMappingURL=Loading.js.map
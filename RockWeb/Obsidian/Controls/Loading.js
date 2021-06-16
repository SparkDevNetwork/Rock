System.register(["../Elements/LoadingIndicator", "vue"], function (exports_1, context_1) {
    "use strict";
    var LoadingIndicator_1, vue_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (LoadingIndicator_1_1) {
                LoadingIndicator_1 = LoadingIndicator_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'Loading',
                components: {
                    LoadingIndicator: LoadingIndicator_1.default
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
System.register(["../Elements/loadingIndicator", "vue"], function (exports_1, context_1) {
    "use strict";
    var loadingIndicator_1, vue_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (loadingIndicator_1_1) {
                loadingIndicator_1 = loadingIndicator_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "Loading",
                components: {
                    LoadingIndicator: loadingIndicator_1.default
                },
                props: {
                    isLoading: {
                        type: Boolean,
                        required: true
                    }
                },
                template: `
<div>
    <slot v-if="!isLoading" />
    <LoadingIndicator v-else />
</div>`
            }));
        }
    };
});
//# sourceMappingURL=loading.js.map
System.register(["vue", "../Store/index"], function (exports_1, context_1) {
    "use strict";
    var vue_1, index_1, store;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (index_1_1) {
                index_1 = index_1_1;
            }
        ],
        execute: function () {
            store = index_1.useStore();
            exports_1("default", vue_1.defineComponent({
                name: "SecondaryBlock",
                computed: {
                    isVisible() {
                        return store.state.areSecondaryBlocksShown;
                    }
                },
                template: `
<div class="secondary-block">
    <slot v-if="isVisible" />
</div>`
            }));
        }
    };
});
//# sourceMappingURL=secondaryBlock.js.map
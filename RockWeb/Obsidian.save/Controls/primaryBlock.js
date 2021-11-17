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
                name: "PrimaryBlock",
                props: {
                    hideSecondaryBlocks: {
                        type: Boolean,
                        default: false
                    }
                },
                methods: {
                    setAreSecondaryBlocksShown(isVisible) {
                        store.setAreSecondaryBlocksShown(isVisible);
                    }
                },
                watch: {
                    hideSecondaryBlocks() {
                        this.setAreSecondaryBlocksShown(!this.hideSecondaryBlocks);
                    }
                },
                template: `<slot />`
            }));
        }
    };
});
//# sourceMappingURL=primaryBlock.js.map
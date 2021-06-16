System.register(["vue", "../Store/Index"], function (exports_1, context_1) {
    "use strict";
    var vue_1, Index_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Index_1_1) {
                Index_1 = Index_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'PrimaryBlock',
                props: {
                    hideSecondaryBlocks: {
                        type: Boolean,
                        default: false
                    }
                },
                methods: {
                    setAreSecondaryBlocksShown: function (isVisible) {
                        Index_1.default.commit('setAreSecondaryBlocksShown', { areSecondaryBlocksShown: isVisible });
                    }
                },
                watch: {
                    hideSecondaryBlocks: function () {
                        this.setAreSecondaryBlocksShown(!this.hideSecondaryBlocks);
                    }
                },
                template: "<slot />"
            }));
        }
    };
});
//# sourceMappingURL=PrimaryBlock.js.map
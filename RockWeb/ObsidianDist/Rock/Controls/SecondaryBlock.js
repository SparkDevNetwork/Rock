define(["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = {
        name: 'SecondaryBlock',
        computed: {
            isVisible: function () {
                return this.$store.state.areSecondaryBlocksShown;
            }
        },
        template: "<div class=\"secondary-block\">\n    <slot v-if=\"isVisible\" />\n</div>"
    };
});
//# sourceMappingURL=SecondaryBlock.js.map
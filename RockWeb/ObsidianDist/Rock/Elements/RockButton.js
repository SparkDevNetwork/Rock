define(["require", "exports", "../Vendor/Vue/vue.js"], function (require, exports, vue_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = vue_js_1.defineComponent({
        name: 'RockButton',
        props: {
            isLoading: {
                type: Boolean,
                default: false
            },
            loadingText: {
                type: String,
                default: 'Loading...'
            }
        },
        emits: [
            'click'
        ],
        methods: {
            handleClick: function () {
                this.$emit('click');
            }
        },
        template: "<button class=\"btn\" :disabled=\"isLoading\" @click.prevent=\"handleClick\">\n    <template v-if=\"isLoading\">\n        {{loadingText}}\n    </template>\n    <slot v-else />\n</button>"
    });
});
//# sourceMappingURL=RockButton.js.map
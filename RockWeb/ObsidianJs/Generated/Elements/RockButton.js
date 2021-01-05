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
            },
            type: {
                type: String,
                default: 'button'
            },
            disabled: {
                type: Boolean,
                default: false
            }
        },
        emits: [
            'click'
        ],
        methods: {
            handleClick: function (event) {
                this.$emit('click', event);
            }
        },
        template: "<button class=\"btn\" :disabled=\"isLoading || disabled\" @click=\"handleClick\" :type=\"type\">\n    <template v-if=\"isLoading\">\n        {{loadingText}}\n    </template>\n    <slot v-else />\n</button>"
    });
});
//# sourceMappingURL=RockButton.js.map
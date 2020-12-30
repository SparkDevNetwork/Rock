define(["require", "exports", "../Vendor/Vue/vue.js"], function (require, exports, vue_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = vue_js_1.defineComponent({
        name: 'Alert',
        props: {
            dismissible: {
                type: Boolean,
                default: false
            },
        },
        emits: [
            'dismiss'
        ],
        methods: {
            onDismiss: function () {
                this.$emit('dismiss');
            }
        },
        template: "<div class=\"alert\">\n    <button v-if=\"dismissible\" type=\"button\" class=\"close\" @click=\"onDismiss\">\n        <span>&times;</span>\n    </button>\n    <slot />\n</div>"
    });
});
//# sourceMappingURL=Alert.js.map
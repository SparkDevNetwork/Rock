System.register(["../Vendor/Vue/vue.js"], function (exports_1, context_1) {
    "use strict";
    var vue_js_1, AlertType;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            }
        ],
        execute: function () {
            (function (AlertType) {
                AlertType["default"] = "default";
                AlertType["success"] = "success";
                AlertType["info"] = "info";
                AlertType["danger"] = "danger";
                AlertType["warning"] = "warning";
                AlertType["primary"] = "primary";
                AlertType["validation"] = "validation";
            })(AlertType || (AlertType = {}));
            exports_1("AlertType", AlertType);
            exports_1("default", vue_js_1.defineComponent({
                name: 'Alert',
                props: {
                    dismissible: {
                        type: Boolean,
                        default: false
                    },
                    alertType: {
                        type: String,
                        default: AlertType.default
                    }
                },
                emits: [
                    'dismiss'
                ],
                methods: {
                    onDismiss: function () {
                        this.$emit('dismiss');
                    }
                },
                computed: {
                    typeClass: function () {
                        return "alert-" + this.alertType;
                    },
                },
                template: "\n<div class=\"alert\" :class=\"typeClass\">\n    <button v-if=\"dismissible\" type=\"button\" class=\"close\" @click=\"onDismiss\">\n        <span>&times;</span>\n    </button>\n    <slot />\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=Alert.js.map
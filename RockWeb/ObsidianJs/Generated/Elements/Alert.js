System.register(["../Vendor/Vue/vue.js"], function (exports_1, context_1) {
    "use strict";
    var vue_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_js_1.defineComponent({
                name: 'Alert',
                props: {
                    dismissible: {
                        type: Boolean,
                        default: false
                    },
                    default: {
                        type: Boolean,
                        default: false
                    },
                    success: {
                        type: Boolean,
                        default: false
                    },
                    info: {
                        type: Boolean,
                        default: false
                    },
                    danger: {
                        type: Boolean,
                        default: false
                    },
                    warning: {
                        type: Boolean,
                        default: false
                    },
                    primary: {
                        type: Boolean,
                        default: false
                    },
                    validation: {
                        type: Boolean,
                        default: false
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
                        if (this.danger) {
                            return 'alert-danger';
                        }
                        if (this.warning) {
                            return 'alert-warning';
                        }
                        if (this.success) {
                            return 'alert-success';
                        }
                        if (this.info) {
                            return 'alert-info';
                        }
                        if (this.primary) {
                            return 'alert-primary';
                        }
                        if (this.validation) {
                            return 'alert-validation';
                        }
                        return 'btn-default';
                    },
                },
                template: "\n<div class=\"alert\" :class=\"typeClass\">\n    <button v-if=\"dismissible\" type=\"button\" class=\"close\" @click=\"onDismiss\">\n        <span>&times;</span>\n    </button>\n    <slot />\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=Alert.js.map
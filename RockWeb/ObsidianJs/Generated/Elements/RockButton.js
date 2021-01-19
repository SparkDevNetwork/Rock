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
                    link: {
                        type: Boolean,
                        default: false
                    },
                    xs: {
                        type: Boolean,
                        default: false
                    },
                    sm: {
                        type: Boolean,
                        default: false
                    },
                    lg: {
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
                computed: {
                    typeClass: function () {
                        if (this.danger) {
                            return 'btn-danger';
                        }
                        if (this.warning) {
                            return 'btn-warning';
                        }
                        if (this.success) {
                            return 'btn-success';
                        }
                        if (this.info) {
                            return 'btn-info';
                        }
                        if (this.primary) {
                            return 'btn-primary';
                        }
                        if (this.link) {
                            return 'btn-link';
                        }
                        return 'btn-default';
                    },
                    sizeClass: function () {
                        if (this.xs) {
                            return 'btn-xs';
                        }
                        if (this.sm) {
                            return 'btn-sm';
                        }
                        if (this.lg) {
                            return 'btn-lg';
                        }
                        return '';
                    },
                    cssClasses: function () {
                        return "btn " + this.typeClass + " " + this.sizeClass;
                    }
                },
                template: "<button :class=\"cssClasses\" :disabled=\"isLoading || disabled\" @click=\"handleClick\" :type=\"type\">\n    <template v-if=\"isLoading\">\n        {{loadingText}}\n    </template>\n    <slot v-else />\n</button>"
            }));
        }
    };
});
//# sourceMappingURL=RockButton.js.map
System.register(["vue"], function (exports_1, context_1) {
    "use strict";
    var vue_1, BtnType, BtnSize;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            }
        ],
        execute: function () {
            (function (BtnType) {
                BtnType["default"] = "default";
                BtnType["primary"] = "primary";
                BtnType["danger"] = "danger";
                BtnType["warning"] = "warning";
                BtnType["success"] = "success";
                BtnType["info"] = "info";
                BtnType["link"] = "link";
            })(BtnType || (BtnType = {}));
            exports_1("BtnType", BtnType);
            (function (BtnSize) {
                BtnSize["default"] = "";
                BtnSize["xs"] = "xs";
                BtnSize["sm"] = "sm";
                BtnSize["lg"] = "lg";
            })(BtnSize || (BtnSize = {}));
            exports_1("BtnSize", BtnSize);
            exports_1("default", vue_1.defineComponent({
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
                    btnType: {
                        type: String,
                        default: BtnType.default
                    },
                    btnSize: {
                        type: String,
                        default: BtnSize.default
                    }
                },
                emits: [
                    'click'
                ],
                methods: {
                    handleClick: function (event) {
                        if (!this.isLoading) {
                            this.$emit('click', event);
                        }
                    }
                },
                computed: {
                    typeClass: function () {
                        return "btn-" + this.btnType;
                    },
                    sizeClass: function () {
                        if (!this.btnSize) {
                            return '';
                        }
                        return "btn-" + this.btnSize;
                    },
                    cssClasses: function () {
                        return "btn " + this.typeClass + " " + this.sizeClass;
                    }
                },
                template: "\n<button :class=\"cssClasses\" :disabled=\"isLoading || disabled\" @click=\"handleClick\" :type=\"type\">\n    <template v-if=\"isLoading\">\n        {{loadingText}}\n    </template>\n    <slot v-else />\n</button>"
            }));
        }
    };
});
//# sourceMappingURL=RockButton.js.map
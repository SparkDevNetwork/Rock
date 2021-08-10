System.register(["vue", "../Elements/RockButton"], function (exports_1, context_1) {
    "use strict";
    var vue_1, RockButton_1, ValidationField;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (RockButton_1_1) {
                RockButton_1 = RockButton_1_1;
            }
        ],
        execute: function () {
            (function (ValidationField) {
                ValidationField[ValidationField["CardNumber"] = 0] = "CardNumber";
                ValidationField[ValidationField["Expiry"] = 1] = "Expiry";
                ValidationField[ValidationField["SecurityCode"] = 2] = "SecurityCode";
            })(ValidationField || (ValidationField = {}));
            exports_1("ValidationField", ValidationField);
            exports_1("default", vue_1.defineComponent({
                name: 'Dialog',
                components: {
                    RockButton: RockButton_1.default
                },
                props: {
                    modelValue: {
                        type: Boolean,
                        required: true
                    },
                    dismissible: {
                        type: Boolean,
                        default: true
                    }
                },
                data: function () {
                    return {
                        doShake: false
                    };
                },
                computed: {
                    hasHeader: function () {
                        return !!this.$slots['header'];
                    }
                },
                methods: {
                    close: function () {
                        this.$emit('update:modelValue', false);
                    },
                    shake: function () {
                        var _this = this;
                        if (!this.doShake) {
                            this.doShake = true;
                            setTimeout(function () { return _this.doShake = false; }, 1000);
                        }
                    },
                    centerOnScreen: function () {
                        var _this = this;
                        this.$nextTick(function () {
                            var div = _this.$refs['modalDiv'];
                            if (!div) {
                                return;
                            }
                            var height = div.offsetHeight;
                            var margin = height / 2;
                            div.style.marginTop = "-" + margin + "px";
                        });
                    }
                },
                watch: {
                    modelValue: {
                        immediate: true,
                        handler: function () {
                            var body = document.body;
                            var cssClasses = ['modal-open', 'page-overflow'];
                            if (this.modelValue) {
                                for (var _i = 0, cssClasses_1 = cssClasses; _i < cssClasses_1.length; _i++) {
                                    var cssClass = cssClasses_1[_i];
                                    body.classList.add(cssClass);
                                }
                                this.centerOnScreen();
                            }
                            else {
                                for (var _a = 0, cssClasses_2 = cssClasses; _a < cssClasses_2.length; _a++) {
                                    var cssClass = cssClasses_2[_a];
                                    body.classList.remove(cssClass);
                                }
                            }
                        }
                    }
                },
                template: "\n<div v-if=\"modelValue\">\n    <div @click=\"shake\" class=\"modal-scrollable\" style=\"z-index: 1060;\">\n        <div @click.stop ref=\"modalDiv\" class=\"modal fade in\" :class=\"{'animated shake': doShake}\" tabindex=\"-1\" role=\"dialog\" style=\"display: block;\">\n            <div class=\"modal-dialog\">\n                <div class=\"modal-content\">\n                    <div v-if=\"hasHeader\" class=\"modal-header\">\n                        <button v-if=\"dismissible\" @click=\"close\" type=\"button\" class=\"close\" style=\"margin-top: -10px;\">\u00D7</button>\n                        <slot name=\"header\" />\n                    </div>\n                    <div class=\"modal-body\">\n                        <button v-if=\"!hasHeader && dismissible\" @click=\"close\" type=\"button\" class=\"close\" style=\"margin-top: -10px;\">\u00D7</button>\n                        <slot />\n                    </div>\n                    <div v-if=\"$slots.footer\" class=\"modal-footer\">\n                        <slot name=\"footer\" />\n                    </div>\n                </div>\n            </div>\n        </div>\n    </div>\n    <div class=\"modal-backdrop fade in\" style=\"z-index: 1050;\"></div>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=Dialog.js.map
System.register(["vue", "./Index", "../Elements/TextBox", "../Services/Boolean"], function (exports_1, context_1) {
    "use strict";
    var vue_1, Index_1, TextBox_1, Boolean_1, fieldTypeGuid, ConfigurationValueKey;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Index_1_1) {
                Index_1 = Index_1_1;
            },
            function (TextBox_1_1) {
                TextBox_1 = TextBox_1_1;
            },
            function (Boolean_1_1) {
                Boolean_1 = Boolean_1_1;
            }
        ],
        execute: function () {
            fieldTypeGuid = '9C204CD0-1233-41C5-818A-C5DA439445AA';
            (function (ConfigurationValueKey) {
                ConfigurationValueKey["IsPassword"] = "ispassword";
                ConfigurationValueKey["MaxCharacters"] = "maxcharacters";
                ConfigurationValueKey["ShowCountDown"] = "showcountdown";
            })(ConfigurationValueKey || (ConfigurationValueKey = {}));
            exports_1("default", Index_1.registerFieldType(fieldTypeGuid, vue_1.defineComponent({
                name: 'TextField',
                components: {
                    TextBox: TextBox_1.default
                },
                props: Index_1.getFieldTypeProps(),
                data: function () {
                    return {
                        internalValue: ''
                    };
                },
                computed: {
                    safeValue: function () {
                        return (this.modelValue || '').trim();
                    },
                    configAttributes: function () {
                        var attributes = {};
                        var maxCharsConfig = this.configurationValues[ConfigurationValueKey.MaxCharacters];
                        if (maxCharsConfig && maxCharsConfig.Value) {
                            var maxCharsValue = Number(maxCharsConfig.Value);
                            if (maxCharsValue) {
                                attributes.maxLength = maxCharsValue;
                            }
                        }
                        var showCountDownConfig = this.configurationValues[ConfigurationValueKey.ShowCountDown];
                        if (showCountDownConfig && showCountDownConfig.Value) {
                            var showCountDownValue = Boolean_1.asBooleanOrNull(showCountDownConfig.Value) || false;
                            if (showCountDownValue) {
                                attributes.showCountDown = showCountDownValue;
                            }
                        }
                        return attributes;
                    },
                    isPassword: function () {
                        var isPasswordConfig = this.configurationValues[ConfigurationValueKey.IsPassword];
                        return Boolean_1.asBooleanOrNull(isPasswordConfig === null || isPasswordConfig === void 0 ? void 0 : isPasswordConfig.Value) || false;
                    },
                    passwordDisplay: function () {
                        return this.safeValue ? '********' : '';
                    }
                },
                watch: {
                    internalValue: function () {
                        this.$emit('update:modelValue', this.internalValue);
                    },
                    modelValue: {
                        immediate: true,
                        handler: function () {
                            this.internalValue = this.modelValue || '';
                        }
                    }
                },
                template: "\n<TextBox v-if=\"isEditMode\" v-model=\"internalValue\" v-bind=\"configAttributes\" :type=\"isPassword ? 'password' : ''\" />\n<span v-else-if=\"isPassword\">{{passwordDisplay}}</span>\n<span v-else>{{ safeValue }}</span>"
            })));
        }
    };
});
//# sourceMappingURL=TextField.js.map
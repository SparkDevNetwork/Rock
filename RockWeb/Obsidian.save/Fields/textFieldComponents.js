System.register(["vue", "./utils", "../Elements/textBox", "../Services/boolean"], function (exports_1, context_1) {
    "use strict";
    var vue_1, utils_1, textBox_1, boolean_1, ConfigurationValueKey, EditComponent;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (utils_1_1) {
                utils_1 = utils_1_1;
            },
            function (textBox_1_1) {
                textBox_1 = textBox_1_1;
            },
            function (boolean_1_1) {
                boolean_1 = boolean_1_1;
            }
        ],
        execute: function () {
            (function (ConfigurationValueKey) {
                ConfigurationValueKey["IsPassword"] = "ispassword";
                ConfigurationValueKey["MaxCharacters"] = "maxcharacters";
                ConfigurationValueKey["ShowCountDown"] = "showcountdown";
            })(ConfigurationValueKey || (ConfigurationValueKey = {}));
            exports_1("EditComponent", EditComponent = vue_1.defineComponent({
                name: "TextField.Edit",
                components: {
                    TextBox: textBox_1.default
                },
                props: utils_1.getFieldEditorProps(),
                data() {
                    return {
                        internalValue: ""
                    };
                },
                computed: {
                    configAttributes() {
                        const attributes = {};
                        const maxCharsConfig = this.configurationValues[ConfigurationValueKey.MaxCharacters];
                        if (maxCharsConfig) {
                            const maxCharsValue = Number(maxCharsConfig);
                            if (maxCharsValue) {
                                attributes.maxLength = maxCharsValue;
                            }
                        }
                        const showCountDownConfig = this.configurationValues[ConfigurationValueKey.ShowCountDown];
                        if (showCountDownConfig && showCountDownConfig) {
                            const showCountDownValue = boolean_1.asBooleanOrNull(showCountDownConfig) || false;
                            if (showCountDownValue) {
                                attributes.showCountDown = showCountDownValue;
                            }
                        }
                        return attributes;
                    },
                    isPassword() {
                        const isPasswordConfig = this.configurationValues[ConfigurationValueKey.IsPassword];
                        return boolean_1.asBooleanOrNull(isPasswordConfig) || false;
                    }
                },
                watch: {
                    internalValue() {
                        this.$emit("update:modelValue", this.internalValue);
                    },
                    modelValue: {
                        immediate: true,
                        handler() {
                            this.internalValue = this.modelValue || "";
                        }
                    }
                },
                template: `
<TextBox v-model="internalValue" v-bind="configAttributes" :type="isPassword ? 'password' : ''" />
`
            }));
        }
    };
});
//# sourceMappingURL=textFieldComponents.js.map
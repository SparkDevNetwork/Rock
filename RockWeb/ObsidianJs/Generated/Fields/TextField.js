System.register(["../Vendor/Vue/vue.js", "./Index.js", "../Elements/TextBox.js", "../Filters/Boolean.js"], function (exports_1, context_1) {
    "use strict";
    var vue_js_1, Index_js_1, TextBox_js_1, Boolean_js_1, fieldTypeGuid, ConfigurationValueKey;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            },
            function (Index_js_1_1) {
                Index_js_1 = Index_js_1_1;
            },
            function (TextBox_js_1_1) {
                TextBox_js_1 = TextBox_js_1_1;
            },
            function (Boolean_js_1_1) {
                Boolean_js_1 = Boolean_js_1_1;
            }
        ],
        execute: function () {
            fieldTypeGuid = '9C204CD0-1233-41C5-818A-C5DA439445AA';
            (function (ConfigurationValueKey) {
                ConfigurationValueKey["IsPassword"] = "ispassword";
                ConfigurationValueKey["MaxCharacters"] = "maxcharacters";
                ConfigurationValueKey["ShowCountDown"] = "showcountdown";
            })(ConfigurationValueKey || (ConfigurationValueKey = {}));
            exports_1("default", Index_js_1.registerFieldType(fieldTypeGuid, vue_js_1.defineComponent({
                name: 'TextField',
                components: {
                    TextBox: TextBox_js_1.default
                },
                props: Index_js_1.getFieldTypeProps(),
                data: function () {
                    return {
                        internalValue: this.modelValue
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
                            var showCountDownValue = Boolean_js_1.asBooleanOrNull(showCountDownConfig.Value) || false;
                            if (showCountDownValue) {
                                attributes.showCountDown = showCountDownValue;
                            }
                        }
                        return attributes;
                    }
                },
                watch: {
                    internalValue: function () {
                        this.$emit('update:modelValue', this.internalValue);
                    }
                },
                template: "\n<TextBox v-if=\"isEditMode\" v-model=\"internalValue\" v-bind=\"configAttributes\" />\n<span v-else>{{ safeValue }}</span>"
            })));
        }
    };
});
//# sourceMappingURL=TextField.js.map
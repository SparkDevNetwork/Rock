System.register(["vue", "./Index", "../Elements/TextBox", "../Services/Boolean", "../Services/Number"], function (exports_1, context_1) {
    "use strict";
    var vue_1, Index_1, TextBox_1, Boolean_1, Number_1, fieldTypeGuid, ConfigurationValueKey;
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
            },
            function (Number_1_1) {
                Number_1 = Number_1_1;
            }
        ],
        execute: function () {
            fieldTypeGuid = 'C28C7BF3-A552-4D77-9408-DEDCF760CED0';
            (function (ConfigurationValueKey) {
                ConfigurationValueKey["NumberOfRows"] = "numberofrows";
                ConfigurationValueKey["AllowHtml"] = "allowhtml";
                ConfigurationValueKey["MaxCharacters"] = "maxcharacters";
                ConfigurationValueKey["ShowCountDown"] = "showcountdown";
            })(ConfigurationValueKey || (ConfigurationValueKey = {}));
            exports_1("default", Index_1.registerFieldType(fieldTypeGuid, vue_1.defineComponent({
                name: 'MemoField',
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
                    allowHtml: function () {
                        var config = this.configurationValues[ConfigurationValueKey.AllowHtml];
                        return Boolean_1.asBoolean(config === null || config === void 0 ? void 0 : config.Value);
                    },
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
                        var rowsConfig = this.configurationValues[ConfigurationValueKey.NumberOfRows];
                        if (rowsConfig === null || rowsConfig === void 0 ? void 0 : rowsConfig.Value) {
                            var rows = Number_1.toNumber(rowsConfig.Value) || 3;
                            if (rows > 0) {
                                attributes.rows = rows;
                            }
                        }
                        return attributes;
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
                template: "\n<TextBox v-if=\"isEditMode\" v-model=\"internalValue\" v-bind=\"configAttributes\" textMode=\"MultiLine\" />\n<div v-else-if=\"allowHtml\">\n    <div v-html=\"modelValue\"></div>\n</div>\n<span v-else>{{ safeValue }}</span>"
            })));
        }
    };
});
//# sourceMappingURL=MemoField.js.map
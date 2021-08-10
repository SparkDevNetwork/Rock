System.register(["vue", "./Index", "../Elements/DropDownList", "../Elements/RadioButtonList"], function (exports_1, context_1) {
    "use strict";
    var vue_1, Index_1, DropDownList_1, RadioButtonList_1, fieldTypeGuid, ConfigurationValueKey;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Index_1_1) {
                Index_1 = Index_1_1;
            },
            function (DropDownList_1_1) {
                DropDownList_1 = DropDownList_1_1;
            },
            function (RadioButtonList_1_1) {
                RadioButtonList_1 = RadioButtonList_1_1;
            }
        ],
        execute: function () {
            fieldTypeGuid = '7525C4CB-EE6B-41D4-9B64-A08048D5A5C0';
            (function (ConfigurationValueKey) {
                ConfigurationValueKey["Values"] = "values";
                ConfigurationValueKey["FieldType"] = "fieldtype";
                ConfigurationValueKey["RepeatColumns"] = "repeatColumns";
            })(ConfigurationValueKey || (ConfigurationValueKey = {}));
            exports_1("default", Index_1.registerFieldType(fieldTypeGuid, vue_1.defineComponent({
                name: 'SingleSelectField',
                components: {
                    DropDownList: DropDownList_1.default,
                    RadioButtonList: RadioButtonList_1.default
                },
                props: Index_1.getFieldTypeProps(),
                setup: function () {
                    return {
                        isRequired: vue_1.inject('isRequired')
                    };
                },
                data: function () {
                    return {
                        internalValue: ''
                    };
                },
                computed: {
                    safeValue: function () {
                        return (this.modelValue || '').trim();
                    },
                    options: function () {
                        var valuesConfig = this.configurationValues[ConfigurationValueKey.Values];
                        if (valuesConfig && valuesConfig.Value) {
                            var providedOptions = valuesConfig.Value.split(',').map(function (v) {
                                if (v.indexOf('^') !== -1) {
                                    var parts = v.split('^');
                                    var value = parts[0];
                                    var text = parts[1];
                                    return {
                                        key: value,
                                        text: text,
                                        value: value
                                    };
                                }
                                return {
                                    key: v,
                                    text: v,
                                    value: v
                                };
                            });
                            if (this.isRadioButtons && !this.isRequired) {
                                providedOptions.unshift({
                                    key: 'None',
                                    text: 'None',
                                    value: ''
                                });
                            }
                            return providedOptions;
                        }
                        return [];
                    },
                    ddlConfigAttributes: function () {
                        var attributes = {};
                        var fieldTypeConfig = this.configurationValues[ConfigurationValueKey.FieldType];
                        if ((fieldTypeConfig === null || fieldTypeConfig === void 0 ? void 0 : fieldTypeConfig.Value) === 'ddl_enhanced') {
                            attributes.enhanceForLongLists = true;
                        }
                        return attributes;
                    },
                    rbConfigAttributes: function () {
                        var attributes = {};
                        var repeatColumnsConfig = this.configurationValues[ConfigurationValueKey.RepeatColumns];
                        if (repeatColumnsConfig === null || repeatColumnsConfig === void 0 ? void 0 : repeatColumnsConfig.Value) {
                            attributes['repeatColumns'] = Number(repeatColumnsConfig.Value) || 0;
                        }
                        return attributes;
                    },
                    isRadioButtons: function () {
                        var fieldTypeConfig = this.configurationValues[ConfigurationValueKey.FieldType];
                        return (fieldTypeConfig === null || fieldTypeConfig === void 0 ? void 0 : fieldTypeConfig.Value) === 'rb';
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
                template: "\n<RadioButtonList v-if=\"isEditMode && isRadioButtons\" v-model=\"internalValue\" v-bind=\"rbConfigAttributes\" :options=\"options\" horizontal />\n<DropDownList v-else-if=\"isEditMode\" v-model=\"internalValue\" v-bind=\"ddlConfigAttributes\" :options=\"options\" />\n<span v-else>{{ safeValue }}</span>"
            })));
        }
    };
});
//# sourceMappingURL=SingleSelect.js.map
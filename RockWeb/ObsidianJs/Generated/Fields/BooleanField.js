System.register(["vue", "./Index", "../Services/Boolean", "../Elements/DropDownList", "../Elements/Toggle"], function (exports_1, context_1) {
    "use strict";
    var vue_1, Index_1, Boolean_1, DropDownList_1, Toggle_1, fieldTypeGuid, BooleanControlType, ConfigurationValueKey;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Index_1_1) {
                Index_1 = Index_1_1;
            },
            function (Boolean_1_1) {
                Boolean_1 = Boolean_1_1;
            },
            function (DropDownList_1_1) {
                DropDownList_1 = DropDownList_1_1;
            },
            function (Toggle_1_1) {
                Toggle_1 = Toggle_1_1;
            }
        ],
        execute: function () {
            fieldTypeGuid = '1EDAFDED-DFE6-4334-B019-6EECBA89E05A';
            (function (BooleanControlType) {
                BooleanControlType[BooleanControlType["DropDown"] = 0] = "DropDown";
                BooleanControlType[BooleanControlType["Checkbox"] = 1] = "Checkbox";
                BooleanControlType[BooleanControlType["Toggle"] = 2] = "Toggle";
            })(BooleanControlType || (BooleanControlType = {}));
            (function (ConfigurationValueKey) {
                ConfigurationValueKey["BooleanControlType"] = "BooleanControlType";
                ConfigurationValueKey["FalseText"] = "falsetext";
                ConfigurationValueKey["TrueText"] = "truetext";
            })(ConfigurationValueKey || (ConfigurationValueKey = {}));
            exports_1("default", Index_1.registerFieldType(fieldTypeGuid, vue_1.defineComponent({
                name: 'BooleanField',
                components: {
                    DropDownList: DropDownList_1.default,
                    Toggle: Toggle_1.default
                },
                props: Index_1.getFieldTypeProps(),
                data: function () {
                    return {
                        internalBooleanValue: false,
                        internalValue: ''
                    };
                },
                computed: {
                    booleanControlType: function () {
                        var controlTypeConfig = this.configurationValues[ConfigurationValueKey.BooleanControlType];
                        switch (controlTypeConfig === null || controlTypeConfig === void 0 ? void 0 : controlTypeConfig.Value) {
                            case '1':
                                return BooleanControlType.Checkbox;
                            case '2':
                                return BooleanControlType.Toggle;
                            default:
                                return BooleanControlType.DropDown;
                        }
                    },
                    trueText: function () {
                        var trueText = Boolean_1.asYesNoOrNull(true);
                        var trueConfig = this.configurationValues[ConfigurationValueKey.TrueText];
                        if (trueConfig && trueConfig.Value) {
                            trueText = trueConfig.Value;
                        }
                        return trueText || 'Yes';
                    },
                    falseText: function () {
                        var falseText = Boolean_1.asYesNoOrNull(false);
                        var falseConfig = this.configurationValues[ConfigurationValueKey.FalseText];
                        if (falseConfig && falseConfig.Value) {
                            falseText = falseConfig.Value;
                        }
                        return falseText || 'No';
                    },
                    isToggle: function () {
                        return this.booleanControlType === BooleanControlType.Toggle;
                    },
                    valueAsYesNoOrNull: function () {
                        return Boolean_1.asYesNoOrNull(this.modelValue);
                    },
                    toggleOptions: function () {
                        return {
                            trueText: this.trueText,
                            falseText: this.falseText
                        };
                    },
                    dropDownListOptions: function () {
                        var trueVal = Boolean_1.asTrueFalseOrNull(true);
                        var falseVal = Boolean_1.asTrueFalseOrNull(false);
                        return [
                            { key: falseVal, text: this.falseText, value: falseVal },
                            { key: trueVal, text: this.trueText, value: trueVal }
                        ];
                    }
                },
                watch: {
                    internalValue: function () {
                        this.$emit('update:modelValue', this.internalValue);
                    },
                    internalBooleanValue: function () {
                        var valueToEmit = Boolean_1.asTrueFalseOrNull(this.internalBooleanValue) || '';
                        this.$emit('update:modelValue', valueToEmit);
                    },
                    modelValue: {
                        immediate: true,
                        handler: function () {
                            this.internalValue = Boolean_1.asTrueFalseOrNull(this.modelValue) || '';
                            this.internalBooleanValue = Boolean_1.asBoolean(this.modelValue);
                        }
                    }
                },
                template: "\n<Toggle v-if=\"isEditMode && isToggle\" v-model=\"internalBooleanValue\" v-bind=\"toggleOptions\" />\n<DropDownList v-else-if=\"isEditMode\" v-model=\"internalValue\" :options=\"dropDownListOptions\" />\n<span v-else>{{ valueAsYesNoOrNull }}</span>"
            })));
        }
    };
});
//# sourceMappingURL=BooleanField.js.map
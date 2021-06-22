System.register(["vue", "../Elements/DropDownList", "../Elements/ColorPicker", "./Index"], function (exports_1, context_1) {
    "use strict";
    var vue_1, DropDownList_1, ColorPicker_1, Index_1, fieldTypeGuid, ColorControlType, ConfigurationValueKey;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (DropDownList_1_1) {
                DropDownList_1 = DropDownList_1_1;
            },
            function (ColorPicker_1_1) {
                ColorPicker_1 = ColorPicker_1_1;
            },
            function (Index_1_1) {
                Index_1 = Index_1_1;
            }
        ],
        execute: function () {
            fieldTypeGuid = 'D747E6AE-C383-4E22-8846-71518E3DD06F';
            (function (ColorControlType) {
                ColorControlType[ColorControlType["ColorPicker"] = 0] = "ColorPicker";
                ColorControlType[ColorControlType["NamedColor"] = 1] = "NamedColor";
            })(ColorControlType || (ColorControlType = {}));
            (function (ConfigurationValueKey) {
                ConfigurationValueKey["ColorControlType"] = "selectiontype";
                ConfigurationValueKey["ColorPicker"] = "Color Picker";
                ConfigurationValueKey["NamedColor"] = "Named Color";
            })(ConfigurationValueKey || (ConfigurationValueKey = {}));
            exports_1("default", Index_1.registerFieldType(fieldTypeGuid, vue_1.defineComponent({
                name: 'ColorField',
                components: {
                    DropDownList: DropDownList_1.default,
                    ColorPicker: ColorPicker_1.default
                },
                props: Index_1.getFieldTypeProps(),
                data: function () {
                    return {
                        internalBooleanValue: false,
                        internalValue: ''
                    };
                },
                computed: {
                    colorControlType: function () {
                        var controlType = Index_1.getConfigurationValue(ConfigurationValueKey.ColorControlType, this.configurationValues);
                        switch (controlType) {
                            case ConfigurationValueKey.ColorPicker:
                                return ColorControlType.ColorPicker;
                            case ConfigurationValueKey.NamedColor:
                            default:
                                return ColorControlType.NamedColor;
                        }
                    },
                    isColorPicker: function () {
                        return this.colorControlType === ColorControlType.ColorPicker;
                    },
                    isNamedPicker: function () {
                        return this.colorControlType === ColorControlType.NamedColor;
                    },
                    displayValue: function () {
                        return this.internalValue;
                    },
                    dropDownListOptions: function () {
                        return [
                            { key: 'Black', text: 'Black', value: 'Black' },
                            { key: 'White', text: 'White', value: 'White' }
                        ];
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
                template: "\n<DropDownList v-if=\"isEditMode && isNamedPicker\" v-model=\"internalValue\" v-bind=\"dropDownListOptions\" />\n<ColorPicker v-else-if=\"isEditMode && isColorPicker\" v-model=\"internalValue\" />\n<span v-else>{{ displayValue }}</span>"
            })));
        }
    };
});
//# sourceMappingURL=ColorField.js.map
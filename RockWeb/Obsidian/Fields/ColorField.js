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
                            { "key": "Transparent", "text": "Transparent", "value": "Transparent" },
                            { "key": "AliceBlue", "text": "AliceBlue", "value": "AliceBlue" },
                            { "key": "AntiqueWhite", "text": "AntiqueWhite", "value": "AntiqueWhite" },
                            { "key": "Aqua", "text": "Aqua", "value": "Aqua" },
                            { "key": "Aquamarine", "text": "Aquamarine", "value": "Aquamarine" },
                            { "key": "Azure", "text": "Azure", "value": "Azure" },
                            { "key": "Beige", "text": "Beige", "value": "Beige" },
                            { "key": "Bisque", "text": "Bisque", "value": "Bisque" },
                            { "key": "Black", "text": "Black", "value": "Black" },
                            { "key": "BlanchedAlmond", "text": "BlanchedAlmond", "value": "BlanchedAlmond" },
                            { "key": "Blue", "text": "Blue", "value": "Blue" },
                            { "key": "BlueViolet", "text": "BlueViolet", "value": "BlueViolet" },
                            { "key": "Brown", "text": "Brown", "value": "Brown" },
                            { "key": "BurlyWood", "text": "BurlyWood", "value": "BurlyWood" },
                            { "key": "CadetBlue", "text": "CadetBlue", "value": "CadetBlue" },
                            { "key": "Chartreuse", "text": "Chartreuse", "value": "Chartreuse" },
                            { "key": "Chocolate", "text": "Chocolate", "value": "Chocolate" },
                            { "key": "Coral", "text": "Coral", "value": "Coral" },
                            { "key": "CornflowerBlue", "text": "CornflowerBlue", "value": "CornflowerBlue" },
                            { "key": "Cornsilk", "text": "Cornsilk", "value": "Cornsilk" },
                            { "key": "Crimson", "text": "Crimson", "value": "Crimson" },
                            { "key": "Cyan", "text": "Cyan", "value": "Cyan" },
                            { "key": "DarkBlue", "text": "DarkBlue", "value": "DarkBlue" },
                            { "key": "DarkCyan", "text": "DarkCyan", "value": "DarkCyan" },
                            { "key": "DarkGoldenrod", "text": "DarkGoldenrod", "value": "DarkGoldenrod" },
                            { "key": "DarkGray", "text": "DarkGray", "value": "DarkGray" },
                            { "key": "DarkGreen", "text": "DarkGreen", "value": "DarkGreen" },
                            { "key": "DarkKhaki", "text": "DarkKhaki", "value": "DarkKhaki" },
                            { "key": "DarkMagenta", "text": "DarkMagenta", "value": "DarkMagenta" },
                            { "key": "DarkOliveGreen", "text": "DarkOliveGreen", "value": "DarkOliveGreen" },
                            { "key": "DarkOrange", "text": "DarkOrange", "value": "DarkOrange" },
                            { "key": "DarkOrchid", "text": "DarkOrchid", "value": "DarkOrchid" },
                            { "key": "DarkRed", "text": "DarkRed", "value": "DarkRed" },
                            { "key": "DarkSalmon", "text": "DarkSalmon", "value": "DarkSalmon" },
                            { "key": "DarkSeaGreen", "text": "DarkSeaGreen", "value": "DarkSeaGreen" },
                            { "key": "DarkSlateBlue", "text": "DarkSlateBlue", "value": "DarkSlateBlue" },
                            { "key": "DarkSlateGray", "text": "DarkSlateGray", "value": "DarkSlateGray" },
                            { "key": "DarkTurquoise", "text": "DarkTurquoise", "value": "DarkTurquoise" },
                            { "key": "DarkViolet", "text": "DarkViolet", "value": "DarkViolet" },
                            { "key": "DeepPink", "text": "DeepPink", "value": "DeepPink" },
                            { "key": "DeepSkyBlue", "text": "DeepSkyBlue", "value": "DeepSkyBlue" },
                            { "key": "DimGray", "text": "DimGray", "value": "DimGray" },
                            { "key": "DodgerBlue", "text": "DodgerBlue", "value": "DodgerBlue" },
                            { "key": "Firebrick", "text": "Firebrick", "value": "Firebrick" },
                            { "key": "FloralWhite", "text": "FloralWhite", "value": "FloralWhite" },
                            { "key": "ForestGreen", "text": "ForestGreen", "value": "ForestGreen" },
                            { "key": "Fuchsia", "text": "Fuchsia", "value": "Fuchsia" },
                            { "key": "Gainsboro", "text": "Gainsboro", "value": "Gainsboro" },
                            { "key": "GhostWhite", "text": "GhostWhite", "value": "GhostWhite" },
                            { "key": "Gold", "text": "Gold", "value": "Gold" },
                            { "key": "Goldenrod", "text": "Goldenrod", "value": "Goldenrod" },
                            { "key": "Gray", "text": "Gray", "value": "Gray" },
                            { "key": "Green", "text": "Green", "value": "Green" },
                            { "key": "GreenYellow", "text": "GreenYellow", "value": "GreenYellow" },
                            { "key": "Honeydew", "text": "Honeydew", "value": "Honeydew" },
                            { "key": "HotPink", "text": "HotPink", "value": "HotPink" },
                            { "key": "IndianRed", "text": "IndianRed", "value": "IndianRed" },
                            { "key": "Indigo", "text": "Indigo", "value": "Indigo" },
                            { "key": "Ivory", "text": "Ivory", "value": "Ivory" },
                            { "key": "Khaki", "text": "Khaki", "value": "Khaki" },
                            { "key": "Lavender", "text": "Lavender", "value": "Lavender" },
                            { "key": "LavenderBlush", "text": "LavenderBlush", "value": "LavenderBlush" },
                            { "key": "LawnGreen", "text": "LawnGreen", "value": "LawnGreen" },
                            { "key": "LemonChiffon", "text": "LemonChiffon", "value": "LemonChiffon" },
                            { "key": "LightBlue", "text": "LightBlue", "value": "LightBlue" },
                            { "key": "LightCoral", "text": "LightCoral", "value": "LightCoral" },
                            { "key": "LightCyan", "text": "LightCyan", "value": "LightCyan" },
                            { "key": "LightGoldenrodYellow", "text": "LightGoldenrodYellow", "value": "LightGoldenrodYellow" },
                            { "key": "LightGreen", "text": "LightGreen", "value": "LightGreen" },
                            { "key": "LightGray", "text": "LightGray", "value": "LightGray" },
                            { "key": "LightPink", "text": "LightPink", "value": "LightPink" },
                            { "key": "LightSalmon", "text": "LightSalmon", "value": "LightSalmon" },
                            { "key": "LightSeaGreen", "text": "LightSeaGreen", "value": "LightSeaGreen" },
                            { "key": "LightSkyBlue", "text": "LightSkyBlue", "value": "LightSkyBlue" },
                            { "key": "LightSlateGray", "text": "LightSlateGray", "value": "LightSlateGray" },
                            { "key": "LightSteelBlue", "text": "LightSteelBlue", "value": "LightSteelBlue" },
                            { "key": "LightYellow", "text": "LightYellow", "value": "LightYellow" },
                            { "key": "Lime", "text": "Lime", "value": "Lime" },
                            { "key": "LimeGreen", "text": "LimeGreen", "value": "LimeGreen" },
                            { "key": "Linen", "text": "Linen", "value": "Linen" },
                            { "key": "Magenta", "text": "Magenta", "value": "Magenta" },
                            { "key": "Maroon", "text": "Maroon", "value": "Maroon" },
                            { "key": "MediumAquamarine", "text": "MediumAquamarine", "value": "MediumAquamarine" },
                            { "key": "MediumBlue", "text": "MediumBlue", "value": "MediumBlue" },
                            { "key": "MediumOrchid", "text": "MediumOrchid", "value": "MediumOrchid" },
                            { "key": "MediumPurple", "text": "MediumPurple", "value": "MediumPurple" },
                            { "key": "MediumSeaGreen", "text": "MediumSeaGreen", "value": "MediumSeaGreen" },
                            { "key": "MediumSlateBlue", "text": "MediumSlateBlue", "value": "MediumSlateBlue" },
                            { "key": "MediumSpringGreen", "text": "MediumSpringGreen", "value": "MediumSpringGreen" },
                            { "key": "MediumTurquoise", "text": "MediumTurquoise", "value": "MediumTurquoise" },
                            { "key": "MediumVioletRed", "text": "MediumVioletRed", "value": "MediumVioletRed" },
                            { "key": "MidnightBlue", "text": "MidnightBlue", "value": "MidnightBlue" },
                            { "key": "MintCream", "text": "MintCream", "value": "MintCream" },
                            { "key": "MistyRose", "text": "MistyRose", "value": "MistyRose" },
                            { "key": "Moccasin", "text": "Moccasin", "value": "Moccasin" },
                            { "key": "NavajoWhite", "text": "NavajoWhite", "value": "NavajoWhite" },
                            { "key": "Navy", "text": "Navy", "value": "Navy" },
                            { "key": "OldLace", "text": "OldLace", "value": "OldLace" },
                            { "key": "Olive", "text": "Olive", "value": "Olive" },
                            { "key": "OliveDrab", "text": "OliveDrab", "value": "OliveDrab" },
                            { "key": "Orange", "text": "Orange", "value": "Orange" },
                            { "key": "OrangeRed", "text": "OrangeRed", "value": "OrangeRed" },
                            { "key": "Orchid", "text": "Orchid", "value": "Orchid" },
                            { "key": "PaleGoldenrod", "text": "PaleGoldenrod", "value": "PaleGoldenrod" },
                            { "key": "PaleGreen", "text": "PaleGreen", "value": "PaleGreen" },
                            { "key": "PaleTurquoise", "text": "PaleTurquoise", "value": "PaleTurquoise" },
                            { "key": "PaleVioletRed", "text": "PaleVioletRed", "value": "PaleVioletRed" },
                            { "key": "PapayaWhip", "text": "PapayaWhip", "value": "PapayaWhip" },
                            { "key": "PeachPuff", "text": "PeachPuff", "value": "PeachPuff" },
                            { "key": "Peru", "text": "Peru", "value": "Peru" },
                            { "key": "Pink", "text": "Pink", "value": "Pink" },
                            { "key": "Plum", "text": "Plum", "value": "Plum" },
                            { "key": "PowderBlue", "text": "PowderBlue", "value": "PowderBlue" },
                            { "key": "Purple", "text": "Purple", "value": "Purple" },
                            { "key": "Red", "text": "Red", "value": "Red" },
                            { "key": "RosyBrown", "text": "RosyBrown", "value": "RosyBrown" },
                            { "key": "RoyalBlue", "text": "RoyalBlue", "value": "RoyalBlue" },
                            { "key": "SaddleBrown", "text": "SaddleBrown", "value": "SaddleBrown" },
                            { "key": "Salmon", "text": "Salmon", "value": "Salmon" },
                            { "key": "SandyBrown", "text": "SandyBrown", "value": "SandyBrown" },
                            { "key": "SeaGreen", "text": "SeaGreen", "value": "SeaGreen" },
                            { "key": "SeaShell", "text": "SeaShell", "value": "SeaShell" },
                            { "key": "Sienna", "text": "Sienna", "value": "Sienna" },
                            { "key": "Silver", "text": "Silver", "value": "Silver" },
                            { "key": "SkyBlue", "text": "SkyBlue", "value": "SkyBlue" },
                            { "key": "SlateBlue", "text": "SlateBlue", "value": "SlateBlue" },
                            { "key": "SlateGray", "text": "SlateGray", "value": "SlateGray" },
                            { "key": "Snow", "text": "Snow", "value": "Snow" },
                            { "key": "SpringGreen", "text": "SpringGreen", "value": "SpringGreen" },
                            { "key": "SteelBlue", "text": "SteelBlue", "value": "SteelBlue" },
                            { "key": "Tan", "text": "Tan", "value": "Tan" },
                            { "key": "Teal", "text": "Teal", "value": "Teal" },
                            { "key": "Thistle", "text": "Thistle", "value": "Thistle" },
                            { "key": "Tomato", "text": "Tomato", "value": "Tomato" },
                            { "key": "Turquoise", "text": "Turquoise", "value": "Turquoise" },
                            { "key": "Violet", "text": "Violet", "value": "Violet" },
                            { "key": "Wheat", "text": "Wheat", "value": "Wheat" },
                            { "key": "White", "text": "White", "value": "White" },
                            { "key": "WhiteSmoke", "text": "WhiteSmoke", "value": "WhiteSmoke" },
                            { "key": "Yellow", "text": "Yellow", "value": "Yellow" },
                            { "key": "YellowGreen", "text": "YellowGreen", "value": "YellowGreen" }
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
                template: "\n<DropDownList v-if=\"isEditMode && isNamedPicker\" v-model=\"internalValue\" :options=\"dropDownListOptions\" />\n<ColorPicker v-else-if=\"isEditMode && isColorPicker\" v-model=\"internalValue\" />\n<span v-else>{{ displayValue }}</span>"
            })));
        }
    };
});
//# sourceMappingURL=ColorField.js.map
System.register(["vue", "../Elements/dropDownList", "../Elements/colorPicker", "./utils"], function (exports_1, context_1) {
    "use strict";
    var vue_1, dropDownList_1, colorPicker_1, utils_1, ColorControlType, ConfigurationValueKey, namedColors, EditComponent;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (dropDownList_1_1) {
                dropDownList_1 = dropDownList_1_1;
            },
            function (colorPicker_1_1) {
                colorPicker_1 = colorPicker_1_1;
            },
            function (utils_1_1) {
                utils_1 = utils_1_1;
            }
        ],
        execute: function () {
            (function (ColorControlType) {
                ColorControlType[ColorControlType["ColorPicker"] = 0] = "ColorPicker";
                ColorControlType[ColorControlType["NamedColor"] = 1] = "NamedColor";
            })(ColorControlType || (ColorControlType = {}));
            (function (ConfigurationValueKey) {
                ConfigurationValueKey["ColorControlType"] = "selectiontype";
                ConfigurationValueKey["ColorPicker"] = "Color Picker";
                ConfigurationValueKey["NamedColor"] = "Named Color";
            })(ConfigurationValueKey || (ConfigurationValueKey = {}));
            namedColors = [
                "Transparent", "AliceBlue", "AntiqueWhite", "Aqua", "Aquamarine",
                "Azure", "Beige", "Bisque", "Black", "BlanchedAlmond",
                "Blue", "BlueViolet", "Brown", "BurlyWood", "CadetBlue",
                "Chartreuse", "Chocolate", "Coral", "CornflowerBlue", "Cornsilk",
                "Crimson", "Cyan", "DarkBlue", "DarkCyan", "DarkGoldenrod",
                "DarkGray", "DarkGreen", "DarkKhaki", "DarkMagenta", "DarkOliveGreen",
                "DarkOrange", "DarkOrchid", "DarkRed", "DarkSalmon", "DarkSeaGreen",
                "DarkSlateBlue", "DarkSlateGray", "DarkTurquoise", "DarkViolet", "DeepPink",
                "DeepSkyBlue", "DimGray", "DodgerBlue", "Firebrick", "FloralWhite",
                "ForestGreen", "Fuchsia", "Gainsboro", "GhostWhite", "Gold",
                "Goldenrod", "Gray", "Green", "GreenYellow", "Honeydew",
                "HotPink", "IndianRed", "Indigo", "Ivory", "Khaki",
                "Lavender", "LavenderBlush", "LawnGreen", "LemonChiffon", "LightBlue",
                "LightCoral", "LightCyan", "LightGoldenrodYellow", "LightGreen", "LightGray",
                "LightPink", "LightSalmon", "LightSeaGreen", "LightSkyBlue", "LightSlateGray",
                "LightSteelBlue", "LightYellow", "Lime", "LimeGreen", "Linen",
                "Magenta", "Maroon", "MediumAquamarine", "MediumBlue", "MediumOrchid",
                "MediumPurple", "MediumSeaGreen", "MediumSlateBlue", "MediumSpringGreen", "MediumTurquoise",
                "MediumVioletRed", "MidnightBlue", "MintCream", "MistyRose", "Moccasin",
                "NavajoWhite", "Navy", "OldLace", "Olive", "OliveDrab",
                "Orange", "OrangeRed", "Orchid", "PaleGoldenrod", "PaleGreen",
                "PaleTurquoise", "PaleVioletRed", "PapayaWhip", "PeachPuff", "Peru",
                "Pink", "Plum", "PowderBlue", "Purple", "Red",
                "RosyBrown", "RoyalBlue", "SaddleBrown", "Salmon", "SandyBrown",
                "SeaGreen", "SeaShell", "Sienna", "Silver", "SkyBlue",
                "SlateBlue", "SlateGray", "Snow", "SpringGreen", "SteelBlue",
                "Tan", "Teal", "Thistle", "Tomato", "Turquoise",
                "Violet", "Wheat", "White", "WhiteSmoke", "Yellow",
                "YellowGreen"
            ];
            exports_1("EditComponent", EditComponent = vue_1.defineComponent({
                name: "ColorField.Edit",
                components: {
                    DropDownList: dropDownList_1.default,
                    ColorPicker: colorPicker_1.default
                },
                props: utils_1.getFieldEditorProps(),
                data() {
                    return {
                        internalBooleanValue: false,
                        internalValue: "",
                        dropDownListOptions: namedColors.map(v => {
                            return { text: v, value: v };
                        })
                    };
                },
                computed: {
                    colorControlType() {
                        const controlType = this.configurationValues[ConfigurationValueKey.ColorControlType];
                        switch (controlType) {
                            case ConfigurationValueKey.ColorPicker:
                                return ColorControlType.ColorPicker;
                            case ConfigurationValueKey.NamedColor:
                            default:
                                return ColorControlType.NamedColor;
                        }
                    },
                    isColorPicker() {
                        return this.colorControlType === ColorControlType.ColorPicker;
                    },
                    isNamedPicker() {
                        return this.colorControlType === ColorControlType.NamedColor;
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
<DropDownList v-if="isNamedPicker" v-model="internalValue" :options="dropDownListOptions" />
<ColorPicker v-else v-model="internalValue" />
`
            }));
        }
    };
});
//# sourceMappingURL=colorFieldComponents.js.map
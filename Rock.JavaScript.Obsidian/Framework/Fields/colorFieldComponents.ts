// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
import { defineComponent } from "vue";
import DropDownList from "../Elements/dropDownList";
import ColorPicker from "../Elements/colorPicker";
import { getFieldEditorProps } from "./utils";
import { ListItem } from "../ViewModels";

enum ColorControlType {
    ColorPicker,
    NamedColor
}

enum ConfigurationValueKey {
    ColorControlType = "selectiontype",
    ColorPicker = "Color Picker",
    NamedColor = "Named Color"
}

const namedColors: string[] = [
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

export const EditComponent = defineComponent({
    name: "ColorField.Edit",
    components: {
        DropDownList,
        ColorPicker
    },
    props: getFieldEditorProps(),
    data() {
        return {
            internalBooleanValue: false,
            internalValue: "",
            dropDownListOptions: namedColors.map(v => {
                return { text: v, value: v } as ListItem;
            })
        };
    },
    computed: {
        colorControlType(): ColorControlType {
            const controlType = this.configurationValues[ConfigurationValueKey.ColorControlType];

            switch (controlType) {
                case ConfigurationValueKey.ColorPicker:
                    return ColorControlType.ColorPicker;

                case ConfigurationValueKey.NamedColor:
                default:
                    return ColorControlType.NamedColor;
            }
        },
        isColorPicker(): boolean {
            return this.colorControlType === ColorControlType.ColorPicker;
        },
        isNamedPicker(): boolean {
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
});

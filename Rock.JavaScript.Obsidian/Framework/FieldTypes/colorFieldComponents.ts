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
import { computed, defineComponent, ref, watch } from "vue";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import ColorPicker from "@Obsidian/Controls/colorPicker.obs";
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";
import { useVModelPassthrough } from "@Obsidian/Utility/component";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

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
    emits: [
        "update:modelValue"
    ],
    setup(props, { emit }) {
        const internalValue = useVModelPassthrough(props, "modelValue", emit);

        const dropDownListOptions = namedColors.map(v => {
            return { text: v, value: v } as ListItemBag;
        });

        const isNamedPicker = computed((): boolean => {
            return props.configurationValues[ConfigurationValueKey.ColorControlType] === ConfigurationValueKey.NamedColor;
        });

        return {
            internalValue,
            dropDownListOptions,
            isNamedPicker
        };
    },
    template: `
<DropDownList v-if="isNamedPicker" v-model="internalValue" :items="dropDownListOptions" />
<ColorPicker v-else v-model="internalValue" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "ColorField.Configuration",

    components: {
        DropDownList
    },

    props: getFieldConfigurationProps(),

    emits: ["update:modelValue", "updateConfiguration", "updateConfigurationValue"],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const colorControlType = ref("");
        const typeList = [
            { text: ConfigurationValueKey.ColorPicker, value: ConfigurationValueKey.ColorPicker },
            { text: ConfigurationValueKey.NamedColor, value: ConfigurationValueKey.NamedColor }
        ];

        /**
         * Update the modelValue property if any value of the dictionary has
         * actually changed. This helps prevent unwanted postbacks if the value
         * didn't really change - which can happen if multiple values get updated
         * at the same time.
         *
         * @returns true if a new modelValue was emitted to the parent component.
         */
        const maybeUpdateModelValue = (): boolean => {
            const newValue: Record<string, string> = {};

            // Construct the new value that will be emitted if it is different
            // than the current value.
            newValue[ConfigurationValueKey.ColorControlType] = colorControlType.value ?? ConfigurationValueKey.ColorPicker;

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.ColorControlType] !== (props.modelValue[ConfigurationValueKey.ColorControlType] ?? ConfigurationValueKey.ColorPicker);

            // If any value changed then emit the new model value.
            if (anyValueChanged) {
                emit("update:modelValue", newValue);
                return true;
            }
            else {
                return false;
            }
        };

        /**
         * Emits the updateConfigurationValue if the value has actually changed.
         *
         * @param key The key that was possibly modified.
         * @param value The new value.
         */
        const maybeUpdateConfiguration = (key: string, value: string): void => {
            if (maybeUpdateModelValue()) {
                emit("updateConfigurationValue", key, value);
            }
        };

        // Watch for changes coming in from the parent component and update our
        // data to match the new information.
        watch(() => [props.modelValue, props.configurationProperties], () => {
            colorControlType.value = props.modelValue[ConfigurationValueKey.ColorControlType] ?? ConfigurationValueKey.ColorPicker;
        }, {
            immediate: true
        });

        // Watch for changes in properties that require new configuration
        // properties to be retrieved from the server.
        // THIS IS JUST A PLACEHOLDER FOR COPYING TO NEW FIELDS THAT MIGHT NEED IT.
        // THIS FIELD DOES NOT NEED THIS
        watch([], () => {
            if (maybeUpdateModelValue()) {
                emit("updateConfiguration");
            }
        });

        // Watch for changes in properties that only require a local UI update.
        watch(colorControlType, () => maybeUpdateConfiguration(ConfigurationValueKey.ColorControlType, colorControlType.value || ConfigurationValueKey.ColorPicker));

        return {
            colorControlType,
            typeList
        };
    },

    template: `
<div>
    <DropDownList v-model="colorControlType" :items="typeList" :show-blank-item="false" label="Selection Type" help="The type of control to select color" />
</div>
`
});

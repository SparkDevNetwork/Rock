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
import { defineComponent, inject, ref, watch } from "vue";
import MediaSelector from "@Obsidian/Controls/mediaSelector.obs";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import TextBox from "@Obsidian/Controls/textBox.obs";
import KeyValueList from "@Obsidian/Controls/keyValueList.obs";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ConfigurationValueKey } from "./mediaSelectorField.partial";
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";
import { KeyValueItem } from "@Obsidian/Types/Controls/keyValueItem";
import { MediaSelectorMode } from "@Obsidian/Enums/Controls/mediaSelectorMode";
import { toNumber } from "@Obsidian/Utility/numberUtils";

function parseKeyValueItemValue(keyValueItemValue: string | undefined): KeyValueItem[] {
    try {
        return JSON.parse(keyValueItemValue ?? "[]");
    }
    catch {
        return [];
    }
}

function parseMediaSelectorMode(modeNumber: string | undefined) : MediaSelectorMode {
    const mediaSelectorModeNumber = toNumber(modeNumber);
    return mediaSelectorModeNumber == 0 ? MediaSelectorMode.Image : MediaSelectorMode.Audio;
}

export const EditComponent = defineComponent({
    name: "MediaSelectorField.Edit",

    components: {
        MediaSelector
    },

    props: getFieldEditorProps(),

    setup() {
        return {
            isRequired: inject("isRequired") as boolean
        };
    },

    data() {
        return {
            internalValue: [] as string[]
        };
    },

    computed: {
        /** The options to choose from */
        options(): KeyValueItem[] {
            try {
                const ds = JSON.parse(this.configurationValues[ConfigurationValueKey.MediaItems] ?? "[]") as KeyValueItem[];
                return ds.map(v => {
                    return {
                        key: v.key,
                        value: v.value
                    } as KeyValueItem;
                });
            }
            catch {
                return [];
            }
        },
        itemWidth() : string {
            return this.configurationValues[ConfigurationValueKey.ItemWidth] ?? "";
        },
        mode() : MediaSelectorMode {
            try {
                return parseMediaSelectorMode(this.configurationValues[ConfigurationValueKey.Mode]);
            }
            catch {
                return MediaSelectorMode.Image;
            }
        }
    },

    watch: {
        internalValue() {
            this.$emit("update:modelValue", this.internalValue.join(","));
        },

        modelValue: {
            immediate: true,
            handler() {
                const value = this.modelValue || "";

                this.internalValue = value !== "" ? value.split(",") : [];
            }
        }
    },

    template: `
<MediaSelector v-model="internalValue" :mediaItems="options" :mode="mode" :itemWidth="itemWidth" />
`
});

const mediaSelectorOptions: ListItemBag[] = [
    {
        value: "0",
        text: "Image"
    },
    {
        value: "1",
        text: "Audio"
    }
];

export const ConfigurationComponent = defineComponent({
    name: "MediaSelectorField.Configuration",

    components: {
        DropDownList,
        TextBox,
        KeyValueList
    },

    props: getFieldConfigurationProps(),

    emits: [
        "update:modelValue",
        "updateConfiguration",
        "updateConfigurationValue"
    ],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const itemWidth = ref("");
        const mediaItems = ref<KeyValueItem[]>([]);
        const mode = ref("");

        /**
         * Update the modelValue property if any value of the dictionary has
         * actually changed. This helps prevent unwanted postbacks if the value
         * didn't really change - which can happen if multiple values get updated
         * at the same time.
         *
         * @returns true if a new modelValue was emitted to the parent component.
         */
        const maybeUpdateModelValue = (): boolean => {
            const newValue: Record<string, string> = {...props.modelValue};

            // Construct the new value that will be emitted if it is different
            // than the current value.
            newValue[ConfigurationValueKey.ItemWidth] = itemWidth.value ?? "";
            newValue[ConfigurationValueKey.Mode] = mode.value?.toString() ?? "0";
            newValue[ConfigurationValueKey.MediaItems] = JSON.stringify(mediaItems.value ?? []);

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.ItemWidth] !== (props.modelValue[ConfigurationValueKey.ItemWidth] ?? "")
                || newValue[ConfigurationValueKey.Mode] !== (props.modelValue[ConfigurationValueKey.Mode])
                || newValue[ConfigurationValueKey.MediaItems] !== (props.modelValue[ConfigurationValueKey.MediaItems] ?? []);


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
            mode.value = props.modelValue[ConfigurationValueKey.Mode] ?? "0";
            mediaItems.value = parseKeyValueItemValue(props.modelValue[ConfigurationValueKey.MediaItems]);
            itemWidth.value = props.modelValue[ConfigurationValueKey.ItemWidth] ?? "";
        }, {
            immediate: true
        });

        // Watch for changes in properties that only require a local UI update.
        watch(mode, () => maybeUpdateConfiguration(ConfigurationValueKey.Mode, mode.toString() ?? MediaSelectorMode.Image));
        watch(mediaItems, () => maybeUpdateConfiguration(ConfigurationValueKey.MediaItems, JSON.stringify(mediaItems.value) ?? ""), {deep : true});
        watch(itemWidth, () => maybeUpdateConfiguration(ConfigurationValueKey.ItemWidth, itemWidth.value ?? "50px"));

        return {
            mediaItems,
            itemWidth,
            mode,
            mediaSelectorOptions
        };
    },

    template: `
<div>
    <DropDownList v-model="mode"
        label="Mode"
        :items="mediaSelectorOptions"
        :showBlankItem="false" />

    <TextBox v-model="itemWidth"
        label="Item Width"
        help="The width of each media item in pixels or percentage." />

    <KeyValueList v-model="mediaItems"
        label="Media Items"
        help="The items to display. The key will be the name of the item and the value should be the URL to the media file." />
</div>
`
});

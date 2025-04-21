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

import { computed, defineComponent, ref, watch } from "vue";
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";
import ContentChannelItemPicker from "@Obsidian/Controls/contentChannelItemPicker.obs";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import { ConfigurationPropertyKey, ConfigurationValueKey } from "./contentChannelItemField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { updateRefValue } from "@Obsidian/Utility/component";

export const EditComponent = defineComponent({
    name: "ContentChannelItemField.Edit",

    components: {
        ContentChannelItemPicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the text editor.
        const internalValue = ref<ListItemBag | null>();
        // The selected content channel configuration value.
        const contentChannelGuid = ref<string>(props.configurationValues[ConfigurationValueKey.ContentChannel]);

        // Watch for changes to the configuration values and update the text editor.
        watch(() => props.configurationValues, () => {
            contentChannelGuid.value = props.configurationValues[ConfigurationValueKey.ContentChannel];
        });

        // Watch for changes from the parent component and update the text editor.
        watch(() => props.modelValue, () => {
            updateRefValue(internalValue, props.modelValue ? JSON.parse(props.modelValue) : null);
        }, {
            immediate: true
        });

        // Watch for changes from the text editor and update the parent component.
        watch(internalValue, () => {
            emit("update:modelValue", JSON.stringify(internalValue.value ?? ""));
        });

        return {
            internalValue,
            contentChannelGuid
        };
    },

    template: `
    <ContentChannelItemPicker v-model="internalValue" :contentChannelGuid="contentChannelGuid" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "ContentChannelItemField.Configuration",

    components: {
        DropDownList,
    },

    props: getFieldConfigurationProps(),

    emits: ["update:modelValue", "updateConfiguration", "updateConfigurationValue"],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const contentChannelGuid = ref<string>("");

        // The content channel options
        const contentChannels = computed((): ListItemBag[] => {
            const editorModeOptions = props.configurationProperties[ConfigurationPropertyKey.ContentChannels];
            return editorModeOptions ? JSON.parse(editorModeOptions) as ListItemBag[] : [];
        });

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
            newValue[ConfigurationValueKey.ContentChannel] = contentChannelGuid.value;

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.ContentChannel] !== (props.modelValue[ConfigurationValueKey.ContentChannel]);

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
            contentChannelGuid.value = props.modelValue[ConfigurationValueKey.ContentChannel];
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
        watch(contentChannelGuid, () => maybeUpdateConfiguration(ConfigurationValueKey.ContentChannel, contentChannelGuid.value));

        return {
            contentChannelGuid,
            contentChannels
        };
    },

    template: `
    <DropDownList label="Content Channel" v-model="contentChannelGuid" :items="contentChannels" :showBlankItem="true" help="Content Channel to select items from, if left blank any content channel's item can be selected." />
`
});

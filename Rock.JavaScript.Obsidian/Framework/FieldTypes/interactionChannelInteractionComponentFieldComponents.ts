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
import { ConfigurationValueKey } from "./interactionChannelInteractionComponentField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import InteractionChannelPicker from "@Obsidian/Controls/interactionChannelPicker.obs";
import { updateRefValue } from "@Obsidian/Utility/component";
import InteractionChannelInteractionComponentPicker from "@Obsidian/Controls/interactionChannelInteractionComponentPicker.obs";

export const EditComponent = defineComponent({
    name: "InteractionChannelInteractionComponentField.Edit",

    components: {
        InteractionChannelInteractionComponentPicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the text editor.
        const internalValue = ref<ListItemBag>({});

        // The interactionChannel options to choose from.
        const interactionChannelGuid = computed((): string => {
            const value = props.configurationValues[ConfigurationValueKey.InteractionChannel];
            const interactionChannel = JSON.parse(value || "{}") as ListItemBag;
            return interactionChannel?.value ?? "";
        });

        // Watch for changes from the parent component and update the interaction channel picker.
        watch(() => props.modelValue, () => {
            updateRefValue(internalValue,  JSON.parse(props.modelValue || "{}"));
        }, {
            immediate: true
        });

        // Watch for changes from the interaction channel picker and update the parent component.
        watch(internalValue, () => {
            emit("update:modelValue", JSON.stringify(internalValue.value));
        });

        return {
            internalValue,
            interactionChannelGuid
        };
    },

    template: `
    <InteractionChannelInteractionComponentPicker v-model="internalValue" :defaultInteractionChannelGuid="interactionChannelGuid" showBlankItem />
`
});

export const ConfigurationComponent = defineComponent({
    name: "InteractionChannelInteractionComponentField.Configuration",

    components: {
        InteractionChannelPicker
    },

    props: getFieldConfigurationProps(),

    emits: ["update:modelValue", "updateConfiguration", "updateConfigurationValue"],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const interactionChannel = ref<ListItemBag>({});

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
            newValue[ConfigurationValueKey.InteractionChannel] = JSON.stringify(interactionChannel.value);

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.InteractionChannel] !== (props.modelValue[ConfigurationValueKey.InteractionChannel]);

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
            interactionChannel.value = JSON.parse(props.modelValue[ConfigurationValueKey.InteractionChannel] || "{}");
        }, {
            immediate: true
        });

        // Watch for changes in properties that only require a local UI update.
        watch(interactionChannel, () => maybeUpdateConfiguration(ConfigurationValueKey.InteractionChannel, JSON.stringify(interactionChannel.value)));

        return {
            interactionChannel,
        };
    },

    template: `
    <InteractionChannelPicker v-model="interactionChannel" label="Default Interaction Channel" help="The default interaction channel selection" showBlankItem />
`
});

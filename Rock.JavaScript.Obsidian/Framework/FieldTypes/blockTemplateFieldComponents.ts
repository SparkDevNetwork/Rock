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

import { defineComponent, ref, watch } from "vue";
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";
import DefinedValuePicker from "@Obsidian/Controls/definedValuePicker.obs";
import BlockTemplatePicker from "@Obsidian/Controls/blockTemplatePicker.obs";
import { ConfigurationValueKey } from "./blockTemplateField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { updateRefValue } from "@Obsidian/Utility/component";
import { DefinedType } from "@Obsidian/SystemGuids/definedType";

export const EditComponent = defineComponent({
    name: "BlockTemplateField.Edit",

    components: {
        BlockTemplatePicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the text editor.
        const templateValue = ref<string>();
        const templateKey = ref<string>();
        const templateBlockValueGuid = ref<string | null| undefined>();

        // Watch for changes from configuration component and update the text editor.
        watch(() => props.configurationValues, () => {
            templateKey.value = "";
            const templateBlock = JSON.parse(props.configurationValues[ConfigurationValueKey.TemplateBlock] || "{}") as ListItemBag;
            templateBlockValueGuid.value = templateBlock?.value;
        }, {
            immediate: true
        });

        // Watch for changes from the parent component and update the text editor.
        watch(() => props.modelValue, () => {
            const [key, ...rest] = props.modelValue.split("|");
            const value = rest.join("|");

            updateRefValue(templateKey, key);
            updateRefValue(templateValue, value);
        }, {
            immediate: true
        });

        // Watch for changes from the text editor and update the parent component.
        watch([templateValue, templateKey], () => {
            const value = `${templateKey.value ?? ""}|${templateValue.value}`;
            emit("update:modelValue", value);
        });

        return {
            templateValue,
            templateKey,
            templateBlockValueGuid
        };
    },

    template: `
    <BlockTemplatePicker v-model="templateValue" v-model:templateKey="templateKey" :templateBlockValueGuid="templateBlockValueGuid" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "BlockTemplateField.Configuration",

    components: {
        DefinedValuePicker
    },

    props: getFieldConfigurationProps(),

    emits: ["update:modelValue", "updateConfiguration", "updateConfigurationValue"],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const internalValue = ref<ListItemBag>();
        const definedTypeGuid = DefinedType.TemplateBlock;

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
            newValue[ConfigurationValueKey.TemplateBlock] = JSON.stringify(internalValue.value);

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.TemplateBlock] !== (props.modelValue[ConfigurationValueKey.TemplateBlock]);

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
            internalValue.value = JSON.parse(props.modelValue[ConfigurationValueKey.TemplateBlock] || "{}");
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
        watch(internalValue, () => maybeUpdateConfiguration(ConfigurationValueKey.TemplateBlock, JSON.stringify(internalValue.value)));

        return {
            internalValue,
            definedTypeGuid
        };
    },

    template: `
    <DefinedValuePicker v-model="internalValue" label="Template Block" :definedTypeGuid="definedTypeGuid" help="An optional setting to select template block from." showBlankItem />
`
});

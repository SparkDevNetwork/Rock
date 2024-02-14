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
import EntityPicker from "@Obsidian/Controls/entityPicker.obs";
import TextBox from "@Obsidian/Controls/textBox.obs";
import { ConfigurationValueKey } from "./entityField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

export const EditComponent = defineComponent({
    name: "EntityField.Edit",

    components: {
        EntityPicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by entity picker.
        const internalValue = ref<string>();
        const entityType = ref<ListItemBag>();

        // EntityControlHelpTextFormat configuration value, used to format help text on the picker.
        const helpTextFormat = computed((): string => {
            const helpTextFormat = props.configurationValues[ConfigurationValueKey.EntityControlHelpTextFormat];
            return helpTextFormat;
        });

        // Watch for changes from the parent component and update the client UI.
        watch(() => props.modelValue, () => {
            const entityFieldValue =  JSON.parse(props.modelValue || "{}") as EntityFieldValue;
            entityType.value = entityFieldValue.entityType;
            internalValue.value = entityFieldValue.value ?? "";
        }, {
            immediate: true
        });

        // Watch for changes and update the parent component.
        watch([internalValue, entityType], () => {
            const value = internalValue.value;
            const entityFieldValue = {
                value: value,
                entityType: entityType.value
            } as EntityFieldValue;
            emit("update:modelValue", JSON.stringify(entityFieldValue));
        });

        return {
            internalValue,
            entityType,
            helpTextFormat,
        };
    },

    template: `
    <EntityPicker v-model="internalValue" v-model:entityType="entityType" :entityControlHelpTextFormat="helpTextFormat" enhanceForLongLists />
`
});

export const ConfigurationComponent = defineComponent({
    name: "EntityField.Configuration",

    components: {
        TextBox
    },

    props: getFieldConfigurationProps(),

    emits: ["update:modelValue", "updateConfiguration", "updateConfigurationValue"],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const helpTextFormat = ref<string>();

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
            newValue[ConfigurationValueKey.EntityControlHelpTextFormat] = helpTextFormat.value ?? "";

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.EntityControlHelpTextFormat] !== props.modelValue[ConfigurationValueKey.EntityControlHelpTextFormat];

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
            helpTextFormat.value = props.modelValue[ConfigurationValueKey.EntityControlHelpTextFormat];
        }, {
            immediate: true
        });

        // Watch for changes in properties that only require a local UI update.
        watch(helpTextFormat, () => maybeUpdateConfiguration(ConfigurationValueKey.EntityControlHelpTextFormat, helpTextFormat.value ?? ""));

        return {
            helpTextFormat
        };
    },

    template: `
    <TextBox label="Entity Control Help Text Format" v-model="helpTextFormat" help="Include a {0} in places where you want the EntityType name (Campus, Group, etc) to be included and a {1} in places where you want the pluralized EntityType name (Campuses, Groups, etc) to be included." />
`
});

type EntityFieldValue = {
    /** Gets or sets the entity value for this item. */
    value?: string | null | undefined;

    /** Gets or sets entity type for this item. */
    entityType?: ListItemBag | undefined;
};

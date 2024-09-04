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
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import TextBox from "@Obsidian/Controls/textBox.obs";
import EntityTypePicker from "@Obsidian/Controls/entityTypePicker.obs";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import { ConfigurationValueKey } from "./attributeField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { asBoolean, asTrueOrFalseString } from "@Obsidian/Utility/booleanUtils";
import { updateRefValue } from "@Obsidian/Utility/component";

export const EditComponent = defineComponent({
    name: "AttributeField.Edit",

    components: {
        DropDownList
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the text editor.
        const internalValue = ref<string[]>([]);

        // The selected Entity Type.
        const entityTypeGuid = computed((): string | null | undefined => {
            const entityType = JSON.parse(props.configurationValues[ConfigurationValueKey.Entitytype] || "{}") as ListItemBag;
            return entityType?.value;
        });

        // The options to choose from.
        const options = computed((): ListItemBag[] => {
            const attributes = JSON.parse(props.configurationValues[ConfigurationValueKey.ClientValues] || "[]") as ListItemBag[];
            return attributes;
        });

        // Allow Multiple configuration value, sets control to a multi select check box list when true.
        const allowMultiple = computed((): boolean => {
            const allowMultiple = asBoolean(props.configurationValues[ConfigurationValueKey.AllowMultiple]);
            return allowMultiple;
        });

        // Watch for changes from the parent component and update the client UI.
        watch(() => props.modelValue, () => {
            updateRefValue(internalValue, JSON.parse(props.modelValue || "[]"));
        }, {
            immediate: true
        });

        // Watch for changes from the client UI and update the parent component.
        watch(internalValue, () => {
            emit("update:modelValue", allowMultiple.value == true ? JSON.stringify(internalValue.value) : internalValue.value);
        });

        return {
            internalValue,
            options,
            allowMultiple,
            entityTypeGuid
        };
    },

    template: `
    <DropDownList v-if="entityTypeGuid" v-model="internalValue" :items="options" :showBlankItem="true" :multiple="allowMultiple" enhanceForLongLists />
`
});

export const ConfigurationComponent = defineComponent({
    name: "AttributeField.Configuration",

    components: {
        CheckBox,
        TextBox,
        EntityTypePicker
    },

    props: getFieldConfigurationProps(),

    emits: ["update:modelValue", "updateConfiguration", "updateConfigurationValue"],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const allowMultiple = ref<boolean>();
        const qualifierColumn = ref<string>();
        const qualifierValue = ref<string>();
        const entityType = ref<ListItemBag>();

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
            newValue[ConfigurationValueKey.AllowMultiple] = asTrueOrFalseString(allowMultiple.value);
            newValue[ConfigurationValueKey.QualifierColumn] = qualifierColumn.value ?? "";
            newValue[ConfigurationValueKey.QualifierValue] = qualifierValue.value ?? "";
            newValue[ConfigurationValueKey.Entitytype] = JSON.stringify(entityType.value);
            newValue[ConfigurationValueKey.ClientValues] = props.modelValue[ConfigurationValueKey.ClientValues];

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.AllowMultiple] !== (props.modelValue[ConfigurationValueKey.AllowMultiple])
                || newValue[ConfigurationValueKey.QualifierColumn] !== (props.modelValue[ConfigurationValueKey.QualifierColumn])
                || newValue[ConfigurationValueKey.QualifierValue] !== (props.modelValue[ConfigurationValueKey.QualifierValue])
                || newValue[ConfigurationValueKey.Entitytype] !== (props.modelValue[ConfigurationValueKey.Entitytype]);

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
            allowMultiple.value = asBoolean(props.modelValue[ConfigurationValueKey.AllowMultiple]);
            qualifierColumn.value = props.modelValue[ConfigurationValueKey.QualifierColumn];
            qualifierValue.value = props.modelValue[ConfigurationValueKey.QualifierValue];
            entityType.value = JSON.parse(props.modelValue[ConfigurationValueKey.Entitytype] || "{}") as ListItemBag;
        }, {
            immediate: true
        });

        // Watch for changes in properties that require new configuration
        // properties to be retrieved from the server.
        watch([qualifierColumn, qualifierValue, entityType], () => {
            if (maybeUpdateModelValue()) {
                emit("updateConfiguration");
            }
        });

        // Watch for changes in properties that only require a local UI update.
        watch(allowMultiple, () => maybeUpdateConfiguration(ConfigurationValueKey.AllowMultiple, asTrueOrFalseString(allowMultiple.value)));
        watch(qualifierColumn, () => maybeUpdateConfiguration(ConfigurationValueKey.QualifierColumn, qualifierColumn.value ?? ""));
        watch(qualifierValue, () => maybeUpdateConfiguration(ConfigurationValueKey.QualifierValue, qualifierValue.value ?? ""));
        watch(entityType, () => maybeUpdateConfiguration(ConfigurationValueKey.Entitytype, JSON.stringify(entityType.value)));

        return {
            allowMultiple,
            qualifierColumn,
            qualifierValue,
            entityType
        };
    },

    template: `
<EntityTypePicker v-model="entityType" label="Entity Type" :multiple="false" :includeGlobalOption="false" help="The Entity Type to select attributes for." showBlankItem />
<CheckBox v-model="allowMultiple" label="Allow Multiple Values" text="Yes" help="When set, allows multiple system phone numbers to be selected." />
<TextBox v-model="qualifierColumn" label="Qualifier Column" help="Entity column qualifier" />
<TextBox v-model="qualifierValue" label="Qualifier Value" help="Entity column value" />
`
});

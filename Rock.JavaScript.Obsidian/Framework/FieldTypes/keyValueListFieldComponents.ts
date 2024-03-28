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
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import RockFormField from "@Obsidian/Controls/rockFormField.obs";
import TextBox from "@Obsidian/Controls/textBox.obs";
import { asBoolean, asBooleanOrNull, asTrueFalseOrNull } from "@Obsidian/Utility/booleanUtils";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ClientValue, ConfigurationPropertyKey, ConfigurationValueKey, ValueItem } from "./keyValueListField.partial";
import { getFieldConfigurationProps, getFieldEditorProps } from "./utils";
import type { ValidationRule } from "@Obsidian/Types/validationRules";
import { normalizeRules } from "@Obsidian/ValidationRules";
import { isNullOrWhiteSpace, containsHtmlTag } from "@Obsidian/Utility/stringUtils";

function parseModelValue(modelValue: string | undefined): ClientValue[] {
    try {
        return JSON.parse(modelValue ?? "[]") as ClientValue[];
    }
    catch {
        return [];
    }
}

export const EditComponent = defineComponent({
    name: "KeyValueListField.Edit",

    components: {
        RockFormField,
        DropDownList,
        TextBox
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValues = ref(parseModelValue(props.modelValue));

        const valueOptions = computed((): ValueItem[] => {
            try {
                return JSON.parse(props.configurationValues[ConfigurationValueKey.Values] ?? "[]") as ValueItem[];
            }
            catch {
                return [];
            }
        });

        /** The options to choose from in the drop down list */
        const options = computed((): ListItemBag[] => {
            const providedOptions: ListItemBag[] = valueOptions.value.map(v => {
                return {
                    text: v.text,
                    value: v.value
                };
            });

            return providedOptions;
        });

        const hasValues = computed((): boolean => valueOptions.value.length > 0);

        const keyPlaceholder = computed((): string => {
            return props.configurationValues[ConfigurationValueKey.KeyPrompt] ?? "";
        });

        const valuePlaceholder = computed((): string => {
            return props.configurationValues[ConfigurationValueKey.ValuePrompt] ?? "";
        });

        const displayValueFirst = computed((): boolean => {
            return asBoolean(props.configurationValues[ConfigurationValueKey.DisplayValueFirst] ?? "");
        });

        const allowHtml = computed((): boolean => {
            return asBoolean(props.configurationValues[ConfigurationValueKey.AllowHtml] ?? "");
        });

        watch(() => props.modelValue, () => {
            internalValues.value = parseModelValue(props.modelValue);
        });

        watch(() => internalValues.value, () => {
            emit("update:modelValue", JSON.stringify(internalValues.value));
        }, {
            deep: true
        });

        const onAddClick = (): void => {
            let defaultValue = "";

            if (hasValues.value) {
                defaultValue = valueOptions.value[0].value;
            }

            internalValues.value.push({ key: "", value: defaultValue });
        };

        const onRemoveClick = (index: number): void => {
            internalValues.value.splice(index, 1);
        };

        const augmentedRules = computed((): ValidationRule[] => {
            const rules = [] as ValidationRule[];

            if (!allowHtml.value) {
                rules.push(function (value: unknown) {
                    const isArr = Array.isArray(value);
                    if (isNullOrWhiteSpace(value) || (isArr && value.length === 0)) {
                        return true;
                    }

                    if (isArr) {
                        for (let i = 0; i < value.length; i++) {
                            const { key: k, value: v } = value[i];
                            if (containsHtmlTag(String(k)) || containsHtmlTag(String(v))) {
                                return "contains invalid characters. Please make sure that your entries do not contain any angle brackets like < or >.";
                            }
                        }
                    }

                    return true;
                });
            }

            return rules;
        });

        return {
            augmentedRules,
            internalValues,
            hasValues,
            displayValueFirst,
            options,
            keyPlaceholder,
            valuePlaceholder,
            onAddClick,
            onRemoveClick
        };
    },

    template: `
<RockFormField
    :modelValue="internalValues"
    formGroupClasses="key-value-list"
    name="key-value-list"
    :rules="augmentedRules">
    <template #default="{uniqueId}">
        <div class="control-wrapper">
<span :id="uniqueId" class="key-value-list">
    <span class="key-value-rows">
        <div v-for="(value, valueIndex) in internalValues" class="controls controls-row form-control-group">
            <template v-if="!displayValueFirst">
                <input v-model="value.key" class="key-value-key form-control input-width-md" type="text" :placeholder="keyPlaceholder">

                <select v-if="hasValues" v-model="value.value" class="form-control input-width-lg">
                    <option v-for="option in options" :value="option.value" :key="option.value">{{ option.text }}</option>
                </select>
                <input v-else v-model="value.value" class="key-value-value form-control input-width-md" type="text" :placeholder="valuePlaceholder">
            </template>
            <template v-else>
                <select v-if="hasValues" v-model="value.value" class="form-control input-width-lg">
                    <option v-for="option in options" :value="option.value" :key="option.value">{{ option.text }}</option>
                </select>
                <input v-else v-model="value.value" class="key-value-value form-control input-width-md" type="text" :placeholder="valuePlaceholder">

                <input v-model="value.key" class="key-value-key form-control input-width-md" type="text" :placeholder="keyPlaceholder">
            </template>

            <a href="#" @click.prevent="onRemoveClick(valueIndex)" class="btn btn-sm btn-danger"><i class="fa fa-times"></i></a>
        </div>
    </span>
    <div class="control-actions">
        <a class="btn btn-action btn-square btn-xs" href="#" @click.prevent="onAddClick"><i class="fa fa-plus-circle"></i></a>
    </div>
</span>
        </div>
    </template>
</RockFormField>
`
});

export const ConfigurationComponent = defineComponent({
    name: "KeyValueListField.Configuration",

    components: {
        CheckBox,
        DropDownList,
        TextBox
    },

    props: getFieldConfigurationProps(),

    emits: [
        "update:modelValue",
        "updateConfiguration",
        "updateConfigurationValue"
    ],

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const customValues = ref("");
        const internalCustomValues = ref("");
        const keyPrompt = ref("");
        const labelPrompt = ref("");
        const definedType = ref("");
        const allowHtml = ref(false);
        const displayValueFirst = ref(false);

        const definedTypeOptions = computed((): ListItemBag[] => {
            try {
                return JSON.parse(props.configurationProperties[ConfigurationPropertyKey.DefinedTypes] ?? "[]") as ListItemBag[];
            }
            catch {
                return [];
            }
        });

        const onBlur = (): void => {
            internalCustomValues.value = customValues.value;
        };

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
            newValue[ConfigurationValueKey.KeyPrompt] = keyPrompt.value ?? "";
            newValue[ConfigurationValueKey.ValuePrompt] = labelPrompt.value ?? "";
            newValue[ConfigurationValueKey.DefinedType] = definedType.value ?? "";
            newValue[ConfigurationValueKey.CustomValues] = internalCustomValues.value ?? "";
            newValue[ConfigurationValueKey.AllowHtml] = asTrueFalseOrNull(allowHtml.value) ?? "False";
            newValue[ConfigurationValueKey.DisplayValueFirst] = asTrueFalseOrNull(displayValueFirst.value) ?? "False";

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.KeyPrompt] !== (props.modelValue[ConfigurationValueKey.KeyPrompt] ?? "")
                || newValue[ConfigurationValueKey.ValuePrompt] !== (props.modelValue[ConfigurationValueKey.ValuePrompt] ?? "")
                || newValue[ConfigurationValueKey.DefinedType] !== (props.modelValue[ConfigurationValueKey.DefinedType] ?? "")
                || newValue[ConfigurationValueKey.CustomValues] !== (props.modelValue[ConfigurationValueKey.CustomValues] ?? "")
                || newValue[ConfigurationValueKey.AllowHtml] !== (props.modelValue[ConfigurationValueKey.AllowHtml] ?? "False")
                || newValue[ConfigurationValueKey.DisplayValueFirst] !== (props.modelValue[ConfigurationValueKey.DisplayValueFirst] ?? "False");

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
            keyPrompt.value = props.modelValue[ConfigurationValueKey.KeyPrompt] ?? "";
            labelPrompt.value = props.modelValue[ConfigurationValueKey.ValuePrompt] ?? "";
            definedType.value = props.modelValue[ConfigurationValueKey.DefinedType] ?? "";
            customValues.value = props.modelValue[ConfigurationValueKey.CustomValues] ?? "";
            internalCustomValues.value = customValues.value;
            allowHtml.value = asBooleanOrNull(props.modelValue[ConfigurationValueKey.AllowHtml]) ?? false;
            displayValueFirst.value = asBooleanOrNull(props.modelValue[ConfigurationValueKey.DisplayValueFirst]) ?? false;
        }, {
            immediate: true
        });

        // Watch for changes in properties that require new configuration
        // properties to be retrieved from the server.
        watch([definedType, internalCustomValues], () => {
            if (maybeUpdateModelValue()) {
                emit("updateConfiguration");
            }
        });

        // Watch for changes in properties that only require a local UI update.
        watch(keyPrompt, () => maybeUpdateConfiguration(ConfigurationValueKey.KeyPrompt, keyPrompt.value ?? ""));
        watch(labelPrompt, () => maybeUpdateConfiguration(ConfigurationValueKey.ValuePrompt, labelPrompt.value ?? ""));
        watch(allowHtml, () => maybeUpdateConfiguration(ConfigurationValueKey.AllowHtml, asTrueFalseOrNull(allowHtml.value) ?? "False"));
        watch(displayValueFirst, () => maybeUpdateConfiguration(ConfigurationValueKey.DisplayValueFirst, asTrueFalseOrNull(displayValueFirst.value) ?? "False"));

        return {
            allowHtml,
            definedType,
            definedTypeOptions,
            displayValueFirst,
            keyPrompt,
            labelPrompt,
            onBlur,
            customValues
        };
    },

    template: `
<div>
    <TextBox v-model="keyPrompt"
        label="Key Prompt"
        help="The text to display as a prompt in the key textbox." />

    <TextBox v-model="labelPrompt"
        label="Label Prompt"
        help="The text to display as a prompt in the label textbox." />

    <DropDownList v-model="definedType"
        label="Defined Type"
        help="Optional Defined Type to select values from, otherwise values will be free-form text fields."
        :items="definedTypeOptions" />

    <TextBox v-model="customValues"
        label="Custom Values"
        help="Optional list of options to use for the values.  Format is either 'value1,value2,value3,...', or 'value1^text1,value2^text2,value3^text3,...'."
        textMode="multiline"
        @blur="onBlur" />

    <CheckBox v-model="allowHtml"
        label="Allow HTML"
        help="Allow HTML content in values." />

    <CheckBox v-model="displayValueFirst"
        label="Display Value First"
        help="Reverses the display order of the key and the value." />
</div>
`
});

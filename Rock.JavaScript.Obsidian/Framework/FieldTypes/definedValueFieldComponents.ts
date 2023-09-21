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
import { computed, defineComponent, inject, PropType, ref, watch } from "vue";
import CheckBox from "@Obsidian/Controls/checkBox.obs";
import CheckBoxList from "@Obsidian/Controls/checkBoxList.obs";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import RockButton from "@Obsidian/Controls/rockButton.obs";
import NumberBox from "@Obsidian/Controls/numberBox.obs";
import RockFormField from "@Obsidian/Controls/rockFormField.obs";
import TextBox from "@Obsidian/Controls/textBox.obs";
import AttributeValuesContainer from "@Obsidian/Controls/attributeValuesContainer.obs";
import Loading from "@Obsidian/Controls/loading.obs";
import { DefinedValuePickerGetAttributesOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/definedValuePickerGetAttributesOptionsBag";
import { DefinedValuePickerSaveNewValueOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/definedValuePickerSaveNewValueOptionsBag";
import { asBoolean, asTrueFalseOrNull } from "@Obsidian/Utility/booleanUtils";
import { toNumber, toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { useVModelPassthrough } from "@Obsidian/Utility/component";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ClientValue, ConfigurationPropertyKey, ConfigurationValueKey, ValueItem } from "./definedValueField.partial";
import { getFieldEditorProps } from "./utils";
import { BtnType } from "@Obsidian/Enums/Controls/btnType";
import { BtnSize } from "@Obsidian/Enums/Controls/btnSize";
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";
import { useHttp } from "@Obsidian/Utility/http";
import { useSecurityGrantToken } from "@Obsidian/Utility/block";
import { useFieldTypeAttributeGuid } from "@Obsidian/Utility/fieldTypes";

function parseModelValue(modelValue: string | undefined): string {
    try {
        const clientValue = JSON.parse(modelValue ?? "") as ClientValue;

        return clientValue.value;
    }
    catch {
        return "";
    }
}

function getClientValue(value: string | string[], valueOptions: ValueItem[]): ClientValue {
    const values = Array.isArray(value) ? value : [value];
    const selectedValues = valueOptions.filter(v => values.includes(v.value));

    if (selectedValues.length >= 1) {
        return {
            value: selectedValues.map(v => v.value).join(","),
            text: selectedValues.map(v => v.text).join(", "),
            description: selectedValues.map(v => v.description).join(", ")
        };
    }
    else {
        return {
            value: "",
            text: "",
            description: ""
        };
    }
}

export const EditComponent = defineComponent({
    name: "DefinedValueField.Edit",

    components: {
        RockFormField,
        DropDownList,
        CheckBoxList,
        RockButton,
        TextBox,
        AttributeValuesContainer,
        Loading
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref(parseModelValue(props.modelValue));
        const internalValues = ref(parseModelValue(props.modelValue).split(",").filter(v => v !== ""));
        const isShowingAddForm = ref(false);
        const isLoading = ref(false);
        const attributes = ref<Record<string, PublicAttributeBag> | null>(null);
        const attributeValues = ref<Record<string, string>>({});
        const fetchError = ref<false | string>(false);
        const saveError = ref<false | string>(false);
        const securityGrantToken = useSecurityGrantToken();
        const newValue = ref("");
        const newDescription = ref("");
        const valueOptions = computed((): ValueItem[] => {
            try {
                const valueOptions = JSON.parse(props.configurationValues[ConfigurationValueKey.Values] ?? "[]") as ValueItem[];
                addedOptions.value.forEach(addedOption => {
                    if(valueOptions.find(a=>a.value == addedOption.value) == null){
                        valueOptions.push(addedOption);
                    }
                });

                return valueOptions;
            }
            catch {
                return [];
            }
        });

        const addedOptions = ref<ValueItem[]>([]);

        const displayDescription = computed((): boolean => asBoolean(props.configurationValues[ConfigurationValueKey.DisplayDescription]));
        const allowAdd = computed((): boolean => asBoolean(props.configurationValues[ConfigurationValueKey.AllowAddingNewValues]));
        const http = useHttp();

        /** The options to choose from */
        const options = computed((): ListItemBag[] => {
            return valueOptions.value.map(v => {
                return {
                    text: displayDescription.value ? (v.description || v.text) : v.text,
                    value: v.value
                };
            });
        });

        const isMultiple = computed((): boolean => asBoolean(props.configurationValues[ConfigurationValueKey.AllowMultiple]));
        const attributeGuid = useFieldTypeAttributeGuid();
        const configAttributes = computed((): Record<string, unknown> => {
            const attributes: Record<string, unknown> = {};

            const enhancedConfig = props.configurationValues[ConfigurationValueKey.EnhancedSelection];
            if (enhancedConfig) {
                attributes.enhanceForLongLists = asBoolean(enhancedConfig);
            }

            return attributes;
        });

        /** The number of columns wide the checkbox list will be. */
        const repeatColumns = computed((): number => toNumber(props.configurationValues[ConfigurationValueKey.RepeatColumns]));

        watch(() => props.modelValue, () => {
            internalValue.value = parseModelValue(props.modelValue);
            internalValues.value = parseModelValue(props.modelValue).split(",").filter(v => v !== "");
        });

        watch(() => internalValue.value, () => {
            if (!isMultiple.value) {
                const clientValue = getClientValue(internalValue.value, valueOptions.value);

                emit("update:modelValue", JSON.stringify(clientValue));
            }
        });

        watch(() => internalValues.value, () => {
            if (isMultiple.value) {
                const clientValue = getClientValue(internalValues.value, valueOptions.value);

                emit("update:modelValue", JSON.stringify(clientValue));
            }
        });

        async function showAddForm(): Promise<void> {
            if (!allowAdd) return;

            isShowingAddForm.value = true;
            if (attributes.value == null) {
                isLoading.value = true;
                fetchError.value = false;
                saveError.value = false;

                const options: Partial<DefinedValuePickerGetAttributesOptionsBag> = {
                    definedTypeGuid:  props.configurationValues[ConfigurationValueKey.DefinedType],
                    securityGrantToken: securityGrantToken.value
                };
                const url = "/api/v2/Controls/DefinedValuePickerGetAttributes";
                const result = await http.post<PublicAttributeBag[]>(url, undefined, options);

                if (result.isSuccess && result.data) {
                    attributes.value = result.data.reduce(function (acc, val) {
                        acc[val.key as string] = val;
                        return acc;
                    }, {});
                }
                else {
                    attributes.value = null;
                    fetchError.value = "Unable to fetch attribute data.";
                }

                isLoading.value = false;
            }
        }

        function hideAddForm(): void {
            isShowingAddForm.value = false;
            fetchError.value = false;
            saveError.value = false;
        }

        async function saveNewValue(): Promise<void> {
            isLoading.value = true;
            saveError.value = false;

            const options: Partial<DefinedValuePickerSaveNewValueOptionsBag> = {
                definedTypeGuid: props.configurationValues[ConfigurationValueKey.DefinedType],
                securityGrantToken: securityGrantToken.value,
                value: newValue.value,
                description: newDescription.value,
                attributeValues: attributeValues.value,
                updateAttributeGuid: attributeGuid.value
            };
            const url = "/api/v2/Controls/DefinedValuePickerSaveNewValue";
            const result = await http.post<ListItemBag>(url, undefined, options);

            if (result.isSuccess && result.data) {
                addedOptions.value.push({value: result.data.value ?? "", text: result.data.text ?? "", description: ""});
                if (isMultiple) {
                    if (Array.isArray(internalValues.value)) {
                        internalValues.value.push(result.data.value ?? "");
                        const clientValue = getClientValue(internalValues.value, valueOptions.value);
                        emit("update:modelValue", JSON.stringify(clientValue));
                    }
                    else {
                        internalValue.value = result.data.value ?? "";
                        const clientValue = getClientValue(internalValue.value, valueOptions.value);
                        emit("update:modelValue", JSON.stringify(clientValue));

                    }
                }
                else {
                    internalValue.value = result.data.value ?? "";
                }

                const selectableValues = (props.configurationValues[ConfigurationValueKey.SelectableValues]?.split(",") ?? []).filter(s => s !== "");
                if(selectableValues.length > 0 && result.data.value){
                    selectableValues.push(result.data.value);

                    emit("updateConfigurationValue", "selectableValues", selectableValues.join(","));
                }

                emit("updateConfiguration");

                hideAddForm();
                newValue.value = "";
                newDescription.value = "";
                attributeValues.value = {};
            }
            else {
                saveError.value = "Unable to save new Defined Value.";
            }

            isLoading.value = false;
        }

        return {
            configAttributes,
            internalValue,
            internalValues,
            isMultiple,
            isRequired: inject("isRequired") as boolean,
            options,
            repeatColumns,
            allowAdd,
            BtnType,
            showAddForm,
            isShowingAddForm,
            isLoading,
            BtnSize,
            hideAddForm,
            saveNewValue,
            newValue,
            newDescription
        };
    },

    template: `
    <RockFormField
        v-model="internalValue"
        formGroupClasses="rock-defined-value"
        name="definedvalue"
        #default="{uniqueId}"
        :rules="computedRules">
        <div :id="uniqueId" class="form-control-group">
        <template v-if="allowAdd && isShowingAddForm">
        <RockLabel :help="help">{{ label }}</RockLabel>
        <Loading :isLoading="isLoading" class="well">
            <NotificationBox v-if="fetchError" alertType="danger">Error: {{ fetchError }}</NotificationBox>
            <NotificationBox v-else-if="saveError" alertType="danger">Error: {{ saveError }}</NotificationBox>

            <RockForm v-else>
                <TextBox label="Value" v-model="newValue" rules="required" />
                <TextBox label="Description" v-model="newDescription" textMode="multiline" />
                <AttributeValuesContainer v-if="attributes != null" v-model="attributeValues" :attributes="attributes" isEditMode :showCategoryLabel="false" />
                <RockButton :btnType="BtnType.Primary" :btnSize="BtnSize.ExtraSmall"  @click="saveNewValue">Add</RockButton>
                <RockButton :btnType="BtnType.Link" :btnSize="BtnSize.ExtraSmall" @click="hideAddForm">Cancel</RockButton>
            </RockForm>

            <RockButton v-if="fetchError || saveError" :btnType="BtnType.Link" :btnSize="BtnSize.ExtraSmall" @click="hideAddForm">Cancel</RockButton>
        </Loading>
    </template>
    <template v-else>

        <DropDownList v-if="!isMultiple || (isMultiple && configAttributes.enhanceForLongLists)" :multiple="isMultiple" v-model="internalValue" v-bind="configAttributes" :items="options" :showBlankItem="!isRequired">
            <template #inputGroupAppend v-if="allowAdd">
                <span class="input-group-btn">
                    <RockButton @click="showAddForm" :btnType="BtnType.Default" aria-label="Add Item"><i class="fa fa-plus" aria-hidden></i></RockButton>
                </span>
            </template>
        </DropDownList>
        <CheckBoxList v-else v-model="internalValues" :items="options" horizontal :repeatColumns="repeatColumns">
            <template #append v-if="allowAdd">
                <RockButton @click="showAddForm" :btnType="BtnType.Default" aria-label="Add Item"><i class="fa fa-plus" aria-hidden></i></RockButton>
             </template>
        </CheckBoxList>


    </template>
    </div>
</RockFormField>


`
});

export const FilterComponent = defineComponent({
    name: "DefinedValueField.Filter",

    components: {
        EditComponent
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = useVModelPassthrough(props, "modelValue", emit);

        const configurationValues = ref({ ...props.configurationValues });
        configurationValues.value[ConfigurationValueKey.AllowMultiple] = "True";

        watch(() => props.configurationValues, () => {
            configurationValues.value = { ...props.configurationValues };
            configurationValues.value[ConfigurationValueKey.AllowMultiple] = "True";
        });

        return {
            internalValue,
            configurationValues
        };
    },

    template: `
<EditComponent v-model="internalValue" :configurationValues="configurationValues" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "DefinedValueField.Configuration",

    components: {
        DropDownList,
        CheckBoxList,
        CheckBox,
        NumberBox
    },

    props: {
        modelValue: {
            type: Object as PropType<Record<string, string>>,
            required: true
        },
        configurationProperties: {
            type: Object as PropType<Record<string, string>>,
            required: true
        }
    },

    setup(props, { emit }) {
        // Define the properties that will hold the current selections.
        const definedTypeValue = ref("");
        const allowMultipleValues = ref(false);
        const displayDescriptions = ref(false);
        const enhanceForLongLists = ref(false);
        const includeInactive = ref(false);
        const repeatColumns = ref<number | null>(null);
        const selectableValues = ref<string[]>([]);
        const allowAddingNewValues = ref(false);

        /** The defined types that are available to be selected from. */
        const definedTypeItems = ref<ListItemBag[]>([]);

        /** The defined values that are available to be selected from. */
        const definedValueItems = ref<ListItemBag[]>([]);

        /** The options to show in the defined type picker. */
        const definedTypeOptions = computed((): ListItemBag[] => {
            return definedTypeItems.value;
        });

        /** The options to show in the selectable values picker. */
        const definedValueOptions = computed((): ListItemBag[] => definedValueItems.value);

        /** Determines if we have any defined values to show. */
        const hasValues = computed((): boolean => {
            return definedValueItems.value.length > 0;
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
            const newValue: Record<string, string> = {
                ...props.modelValue
            };

            // Construct the new value that will be emitted if it is different
            // than the current value.
            newValue[ConfigurationValueKey.DefinedType] = definedTypeValue.value;
            newValue[ConfigurationValueKey.SelectableValues] = selectableValues.value.join(",");
            newValue[ConfigurationValueKey.AllowMultiple] = asTrueFalseOrNull(allowMultipleValues.value) ?? "False";
            newValue[ConfigurationValueKey.DisplayDescription] = asTrueFalseOrNull(displayDescriptions.value) ?? "False";
            newValue[ConfigurationValueKey.EnhancedSelection] = asTrueFalseOrNull(enhanceForLongLists.value) ?? "False";
            newValue[ConfigurationValueKey.IncludeInactive] = asTrueFalseOrNull(includeInactive.value) ?? "False";
            newValue[ConfigurationValueKey.RepeatColumns] = repeatColumns.value?.toString() ?? "";
            newValue[ConfigurationValueKey.AllowAddingNewValues] = asTrueFalseOrNull(allowAddingNewValues.value) ?? "False";

            // Compare the new value and the old value.
            const anyValueChanged = newValue[ConfigurationValueKey.DefinedType] !== props.modelValue[ConfigurationValueKey.DefinedType]
                || newValue[ConfigurationValueKey.SelectableValues] !== (props.modelValue[ConfigurationValueKey.SelectableValues] ?? "")
                || newValue[ConfigurationValueKey.AllowMultiple] !== (props.modelValue[ConfigurationValueKey.AllowMultiple] ?? "False")
                || newValue[ConfigurationValueKey.DisplayDescription] !== (props.modelValue[ConfigurationValueKey.DisplayDescription] ?? "False")
                || newValue[ConfigurationValueKey.EnhancedSelection] !== (props.modelValue[ConfigurationValueKey.EnhancedSelection] ?? "False")
                || newValue[ConfigurationValueKey.IncludeInactive] !== (props.modelValue[ConfigurationValueKey.IncludeInactive] ?? "False")
                || newValue[ConfigurationValueKey.RepeatColumns] !== (props.modelValue[ConfigurationValueKey.RepeatColumns] ?? "")
                || newValue[ConfigurationValueKey.AllowAddingNewValues] !== (props.modelValue[ConfigurationValueKey.AllowAddingNewValues ?? "False"]);

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
            const definedTypes = props.configurationProperties[ConfigurationPropertyKey.DefinedTypes];
            const definedValues = props.configurationProperties[ConfigurationPropertyKey.DefinedValues];

            definedTypeItems.value = definedTypes ? JSON.parse(props.configurationProperties.definedTypes) as ListItemBag[] : [];
            definedValueItems.value = definedValues ? JSON.parse(props.configurationProperties.definedValues) as ListItemBag[] : [];

            definedTypeValue.value = props.modelValue.definedtype;
            allowMultipleValues.value = asBoolean(props.modelValue[ConfigurationValueKey.AllowMultiple]);
            displayDescriptions.value = asBoolean(props.modelValue[ConfigurationValueKey.DisplayDescription]);
            enhanceForLongLists.value = asBoolean(props.modelValue[ConfigurationValueKey.EnhancedSelection]);
            includeInactive.value = asBoolean(props.modelValue[ConfigurationValueKey.IncludeInactive]);
            repeatColumns.value = toNumberOrNull(props.modelValue[ConfigurationValueKey.RepeatColumns]);
            selectableValues.value = (props.modelValue[ConfigurationValueKey.SelectableValues]?.split(",") ?? []).filter(s => s !== "");
            allowAddingNewValues.value = asBoolean(props.modelValue[ConfigurationValueKey.AllowAddingNewValues]);
        }, {
            immediate: true
        });

        // Watch for changes in properties that require new configuration
        // properties to be retrieved from the server.
        watch([definedTypeValue, selectableValues, displayDescriptions, includeInactive], () => {
            if (maybeUpdateModelValue()) {
                emit("updateConfiguration");
            }
        });

        // Watch for changes in properties that only require a local UI update.
        watch(allowMultipleValues, () => maybeUpdateConfiguration(ConfigurationValueKey.AllowMultiple, asTrueFalseOrNull(allowMultipleValues.value) ?? "False"));
        watch(enhanceForLongLists, () => maybeUpdateConfiguration(ConfigurationValueKey.EnhancedSelection, asTrueFalseOrNull(enhanceForLongLists.value) ?? "False"));
        watch(repeatColumns, () => maybeUpdateConfiguration(ConfigurationValueKey.RepeatColumns, repeatColumns.value?.toString() ?? ""));
        watch(allowAddingNewValues, () => maybeUpdateConfiguration(ConfigurationValueKey.AllowAddingNewValues, asTrueFalseOrNull(allowAddingNewValues.value) ?? "False"));

        return {
            allowMultipleValues,
            definedTypeValue,
            definedTypeOptions,
            definedTypeItems,
            definedValueOptions,
            displayDescriptions,
            enhanceForLongLists,
            hasValues,
            includeInactive,
            repeatColumns,
            selectableValues,
            allowAddingNewValues
        };
    },

    template: `
<div>
    <DropDownList v-model="definedTypeValue" label="Defined Type" :items="definedTypeOptions" :showBlankItem="false" />
    <CheckBox v-model="allowMultipleValues" label="Allow Multiple Values" text="Yes" help="When set, allows multiple defined type values to be selected." />
    <CheckBox v-model="displayDescriptions" label="Display Descriptions" text="Yes" help="When set, the defined value descriptions will be displayed instead of the values." />
    <CheckBox v-model="enhanceForLongLists" label="Enhance For Long Lists" text="Yes" />
    <CheckBox v-model="includeInactive" label="Include Inactive" text="Yes" />
    <NumberBox v-model="repeatColumns" label="Repeat Columns" />
    <CheckBoxList v-if="hasValues" v-model="selectableValues" label="Selectable Values" :items="definedValueOptions" :horizontal="true" />
    <CheckBox v-model="allowAddingNewValues" label="Allow Adding New Values" text="Yes" help="When set the defined type picker can be used to add new defined types." />
</div>
`
});

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

import { computed, defineComponent, PropType, ref, watch } from "vue";
import FieldFilterEditor from "../../../../Controls/fieldFilterEditor";
import FieldTypeEditor from "../../../../Controls/fieldTypeEditor";
import Alert, { AlertType } from "../../../../Elements/alert";
import Modal from "../../../../Controls/modal";
import Panel from "../../../../Controls/panel";
import RockForm from "../../../../Controls/rockForm";
import LoadingIndicator from "../../../../Elements/loadingIndicator";
import NumberBox from "../../../../Elements/numberBox";
import RockButton from "../../../../Elements/rockButton";
import Slider from "../../../../Elements/slider";
import InlineSwitch from "../../../../Elements/switch";
import TextBox from "../../../../Elements/textBox";
import { ValidationResult, ValidationRule } from "../../../../Rules";
import { useInvokeBlockAction } from "../../../../Util/block";
import { FormError } from "../../../../Util/form";
import { areEqual } from "../../../../Util/guid";
import { List } from "../../../../Util/linq";
import { FieldTypeConfigurationViewModel } from "../../../../ViewModels/Controls/fieldTypeEditor";
import { FieldFilterGroup } from "../../../../ViewModels/Reporting/fieldFilterGroup";
import { FieldFilterRule } from "../../../../ViewModels/Reporting/fieldFilterRule";
import { FieldFilterSource } from "../../../../ViewModels/Reporting/fieldFilterSource";
import { FormField, FormFieldType } from "../Shared/types";
import { getFilterGroupTitle, getFilterRuleDescription, timeoutAsync, useFormSources } from "./utils";

/**
 * Check if the two records are equal. This makes sure all the key names match
 * and the associated values also match. Strict checking is performed.
 *
 * @param a The first record value to be compared.
 * @param b The second record value to be compared.
 *
 * @returns True if the two record values are equal, otherwise false.
 */
function shallowStrictEqual(a: Record<string, string>, b: Record<string, string>): boolean {
    const aKeys = Object.keys(a);
    const bKeys = Object.keys(b);

    // Check we have the same number of keys.
    if (aKeys.length !== bKeys.length) {
        return false;
    }

    for (const key of aKeys) {
        // Check that all keys from 'a' exist in 'b'.
        if (!bKeys.includes(key)) {
            return false;
        }

        // Check that all the values from 'a' match those in 'b'.
        if (a[key] !== b[key]) {
            return false;
        }
    }

    return true;
}

export default defineComponent({
    name: "Workflow.FormBuilderDetail.FieldEditAside",
    components: {
        Panel,
        FieldFilterEditor,
        FieldTypeEditor,
        InlineSwitch,
        LoadingIndicator,
        Modal,
        NumberBox,
        RockButton,
        RockForm,
        Slider,
        TextBox,
        Alert
    },

    props: {
        modelValue: {
            type: Object as PropType<FormField>,
            required: true
        },

        formFields: {
            type: Array as PropType<FormField[]>,
            required: true
        }
    },

    emits: [
        "update:modelValue",
        "close",
        "validationChanged"
    ],

    methods: {
        /**
         * Checks if this aside is safe to close or if there are errors that
         * must be corrected first.
         */
        isSafeToClose(): boolean {
            this.formSubmit = true;

            const result = this.validationErrors.length === 0;

            // If there was an error, perform a smooth scroll to the top so
            // they can see the validation results.
            if (!result && this.scrollableElement) {
                this.scrollableElement.scroll({
                    behavior: "smooth",
                    top: 0
                });
            }

            return result;
        }
    },

    setup(props, { emit }) {
        // #region Values
        const invokeBlockAction = useInvokeBlockAction();
        const fieldTypes = useFormSources().fieldTypes ?? [];
        let conditionalSourcesLoadAttempted = false;

        const fieldName = ref(props.modelValue.name);
        const fieldDescription = ref(props.modelValue.description ?? "");
        const fieldKey = ref(props.modelValue.key);
        const fieldSize = ref(props.modelValue.size);
        const isFieldRequired = ref(props.modelValue.isRequired ?? false);
        const isFieldLabelHidden = ref(props.modelValue.isHideLabel ?? false);
        const isShowOnGrid = ref(props.modelValue.isShowOnGrid ?? false);
        const visibilityRule = ref(props.modelValue.visibilityRule ?? null);

        /** The value used by the FieldTypeEditor for editing the field configuration. */
        const fieldTypeValue = ref<FieldTypeConfigurationViewModel>({
            fieldTypeGuid: props.modelValue.fieldTypeGuid,
            configurationValues: props.modelValue.configurationValues ?? {},
            defaultValue: props.modelValue.defaultValue ?? ""
        });

        /** The validation errors for the form. */
        const validationErrors = ref<FormError[]>([]);

        /** True if the form should start to submit. */
        const formSubmit = ref(false);

        /**
         * A reference to the element that will be used for scrolling. This is used
         * to scroll to the top when any validation errors pop up so the individual
         * can see them.
         */
        const scrollableElement = ref<HTMLElement | null>(null);

        /** Contains the model used when editing the field visibility rules. */
        const conditionalModel = ref<FieldFilterGroup | null>(null);

        /**
         * Contains the field filter sources that are available when editing
         * the visibility rules.
         */
        const conditionalSources = ref<FieldFilterSource[] | null>(null);

        /** True if the conditional panel is expanded; otherwise false. */
        const conditionalPanelOpen = ref(false);

        /** True if the conditional modal should be open; otherwise false. */
        const conditionalModalOpen = ref(false);

        // #endregion

        // #region Computed Values

        /**
         * The key which forces the field type editor to reload itself whenever the
         * attribute we are editing changes.
         */
        const fieldTypeEditorKey = computed((): string => `fieldTypeEditor_${props.modelValue.guid}`);

        /** The FormFieldType of the attribute we are editing. */
        const fieldType = computed((): FormFieldType | null => {
            return new List(fieldTypes).firstOrUndefined(f => areEqual(f.guid, props.modelValue.fieldTypeGuid)) ?? null;
        });

        /** The icon to display in the title area. */
        const asideIconSvg = computed((): string => fieldType.value?.svg ?? "");

        /**
         * The validation rules for the attribute key. This uses custom logic
         * to make sure the key entered doesn't already exist in the form.
         */
        const fieldKeyRules = computed((): ValidationRule[] => {
            const rules: ValidationRule[] = ["required"];
            const keys: string[] = props.formFields
                .filter(f => !areEqual(f.guid, props.modelValue.guid))
                .map(f => f.key);

            rules.push((value): ValidationResult => {
                const valueString = value as string;

                if (keys.includes(valueString)) {
                    return "must be unique";
                }

                return "";
            });

            return rules;
        });

        /** Determines if we have any active conditional rules. */
        const hasConditions = computed((): boolean => {
            return !!visibilityRule.value?.rules && visibilityRule.value.rules.length > 0;
        });

        /** Contains the "Show/Hide any/all" title of the field visibility rule. */
        const conditionalTitle = computed((): string => {
            return visibilityRule.value
                ? getFilterGroupTitle(visibilityRule.value)
                : "";
        });

        /** The individual rules that decide if this field will be visible. */
        const conditionalRules = computed((): FieldFilterRule[] => {
            return visibilityRule.value?.rules ?? [];
        });

        /** True if the conditionals panel content is still loading; otherwise false. */
        const isConditionalsLoading = computed((): boolean => !conditionalSources.value);

        // #endregion

        // #region Functions

        /**
         * Gets the description of a single filter rule, including the source name.
         *
         * @param rule The rule that needs to be translated into description text.
         *
         * @returns A string that contains a human friendly description about the rule.
         */
        const getRuleDescription = (rule: FieldFilterRule): string => {
            return getFilterRuleDescription(rule, conditionalSources.value ?? [], props.formFields);
        };

        /**
         * Loads all the conditional sources that will be used by this field during filtering.
         */
        const loadConditionalSources = async (): Promise<void> => {
            // Get all fields except our own.
            const fields = props.formFields.filter(f => !areEqual(f.guid, props.modelValue.guid));

            const getFilterSources = invokeBlockAction<FieldFilterSource[]>("GetFilterSources", {
                formFields: fields
            });

            // Wait at most 2 seconds.
            const result = await Promise.race([getFilterSources, timeoutAsync(2000)]);

            if (!result || !result.isSuccess || !result.data) {
                return;
            }

            conditionalSources.value = result.data;
        };

        // #endregion

        // #region Event Handlers

        /**
         * Event handler for when the back button is clicked.
         */
        const onBackClick = (): void => emit("close");

        /**
         * Event handler for when the field type editor has updated any configuration
         * values.
         *
         * @param value The value that contains the changed information.
         */
        const onFieldTypeModelValueUpdate = (value: FieldTypeConfigurationViewModel): void => {
            emit("update:modelValue", {
                ...props.modelValue,
                configurationValues: value.configurationValues,
                defaultValue: value.defaultValue
            });
        };

        /**
         * Event handler for when the validation state of the form has changed.
         *
         * @param errors Any errors that were detected on the form.
         */
        const onValidationChanged = (errors: FormError[]): void => {
            validationErrors.value = errors;
            emit("validationChanged", errors);
        };

        /**
         * Event handler for when the conditional edit button has been clicked.
         * Prepare the edit modal and open it.
         */
        const onConditionalEditClick = async (): Promise<void> => {
            conditionalModel.value = visibilityRule.value;
            conditionalModalOpen.value = true;
        };

        /**
         * Event handler for when the conditional model save button has been clicked.
         * Store all the updates into our internal values.
         */
        const onConditionalSave = (): void => {
            visibilityRule.value = conditionalModel.value;
            conditionalModalOpen.value = false;
        };

        // #endregion

        // Watch for changes to field name, and if the old value matches the
        // attribute key then update the key to the new value.
        watch(fieldName, (newValue, oldValue) => {
            const oldValueAsKey = oldValue.replace(/[^a-zA-Z0-9_\-.]/g, "");

            if (oldValueAsKey === fieldKey.value) {
                fieldKey.value = newValue.replace(/[^a-zA-Z0-9_\-.]/g, "");
            }
        });

        // Watch for the conditionals panel being opened and if it was the first
        // time then start loading all the filter sources.
        watch(conditionalPanelOpen, () => {
            if (!conditionalPanelOpen.value || conditionalSources.value !== null || conditionalSourcesLoadAttempted) {
                return;
            }

            conditionalSourcesLoadAttempted = true;
            loadConditionalSources();
        });

        // Watch for any changes in our simple field values and update the
        // modelValue.
        watch([fieldName, fieldDescription, fieldKey, fieldSize, isFieldRequired, isFieldLabelHidden, isShowOnGrid, visibilityRule], () => {
            const newValue: FormField = {
                ...props.modelValue,
                name: fieldName.value,
                description: fieldDescription.value,
                key: fieldKey.value,
                size: fieldSize.value,
                isRequired: isFieldRequired.value,
                isHideLabel: isFieldLabelHidden.value,
                isShowOnGrid: isShowOnGrid.value,
                visibilityRule: visibilityRule.value
            };

            emit("update:modelValue", newValue);
        });

        // Watch for any incoming changes from the parent component and update
        // all our individual field values.
        watch(() => props.modelValue, () => {
            fieldName.value = props.modelValue.name;
            fieldDescription.value = props.modelValue.description ?? "";
            fieldKey.value = props.modelValue.key;
            fieldSize.value = props.modelValue.size;
            isFieldRequired.value = props.modelValue.isRequired ?? false;
            isFieldLabelHidden.value = props.modelValue.isHideLabel ?? false;
            isShowOnGrid.value = props.modelValue.isShowOnGrid ?? false;
            visibilityRule.value = props.modelValue.visibilityRule ?? null;

            const isConfigChanged = fieldTypeValue.value.fieldTypeGuid !== props.modelValue.fieldTypeGuid
                || !shallowStrictEqual(fieldTypeValue.value.configurationValues, props.modelValue.configurationValues ?? {})
                || fieldTypeValue.value.defaultValue !== props.modelValue.defaultValue;

            // Only update the field type if anything actually changed.
            if (isConfigChanged) {
                fieldTypeValue.value = {
                    fieldTypeGuid: props.modelValue.fieldTypeGuid,
                    configurationValues: props.modelValue.configurationValues ?? {},
                    defaultValue: props.modelValue.defaultValue ?? ""
                };
            }
        });

        return {
            asideIconSvg,
            conditionalTitle,
            conditionalModalOpen,
            conditionalModel,
            conditionalPanelOpen,
            conditionalRules,
            conditionalSources,
            fieldDescription,
            fieldKey,
            fieldKeyRules,
            fieldName,
            fieldSize,
            fieldTypeEditorKey,
            fieldTypeValue,
            formSubmit,
            getRuleDescription,
            hasConditions,
            isConditionalsLoading,
            isFieldLabelHidden,
            isFieldRequired,
            isShowOnGrid,
            onBackClick,
            onConditionalEditClick,
            onConditionalSave,
            onFieldTypeModelValueUpdate,
            onValidationChanged,
            scrollableElement,
            validationErrors,
            AlertType
        };
    },

    template: `
    <div class="form-sidebar">
    <div class="sidebar-header">
        <div class="sidebar-back" @click="onBackClick">
            <i class="fa fa-chevron-left"></i>
        </div>

        <div class="title">
            <span v-if="asideIconSvg" class="inline-svg icon" v-html="asideIconSvg"></span>
            {{ fieldName }}
        </div>
    </div>

    <div ref="scrollableElement" class="sidebar-body">
        <RockForm v-model:submit="formSubmit" @validationChanged="onValidationChanged" class="sidebar-panels sidebar-field-edit field-edit-aside">
            <Panel :modelValue="true" title="Field Type" :hasCollapse="true">
                <TextBox v-model="fieldName"
                    rules="required"
                    label="Name" />
                <TextBox v-model="fieldDescription"
                    label="Description"
                    textMode="multiline" />
                <FieldTypeEditor :modelValue="fieldTypeValue" :key="fieldTypeEditorKey" @update:modelValue="onFieldTypeModelValueUpdate" isFieldTypeReadOnly />
            </Panel>

            <Panel title="Conditionals" v-model="conditionalPanelOpen" :hasCollapse="true">
                <LoadingIndicator v-if="isConditionalsLoading" />

                <div v-else-if="conditionalSources.length < 1">
                    <Alert :alertType="AlertType.Warning">No source fields available.</Alert>

                    <div class="d-flex justify-content-end">
                        <RockButton btnType="default" btnSize="sm" disabled><i class="fa fa-pencil"></i></RockButton>
                    </div>
                </div>

                <div v-else>
                    <div v-if="hasConditions">
                        <div v-html="conditionalTitle"></div>
                        <ul>
                            <li v-for="rule in conditionalRules" :key="rule.guid">{{ getRuleDescription(rule) }}</li>
                        </ul>
                    </div>

                    <div class="d-flex justify-content-end">
                        <RockButton btnType="default" btnSize="sm" @click="onConditionalEditClick"><i class="fa fa-pencil"></i></RockButton>
                    </div>
                </div>
            </Panel>

            <Panel title="Format" :hasCollapse="true">
                <Slider v-model="fieldSize" label="Column Span" :min="1" :max="12" isIntegerOnly showValueBar/>
                <InlineSwitch v-model="isFieldRequired" text="Required" />
                <InlineSwitch v-model="isFieldLabelHidden" text="Hide Label" />
            </Panel>

            <Panel title="Advanced" :hasCollapse="true">
                <TextBox v-model="fieldKey"
                    :rules="fieldKeyRules"
                    label="Field Key" />
                <InlineSwitch v-model="isShowOnGrid" text="Show on Results Grid" />
            </Panel>
        </RockForm>
    </div>

    <Modal v-model="conditionalModalOpen" title="Conditional Settings" saveText="Save" @save="onConditionalSave">
        <FieldFilterEditor v-model="conditionalModel" :title="fieldName" :sources="conditionalSources" />
    </Modal>
    </div>
`
});

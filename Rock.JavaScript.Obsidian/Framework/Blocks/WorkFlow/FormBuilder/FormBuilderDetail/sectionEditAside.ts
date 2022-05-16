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
import ConfigurableZone from "./configurableZone";
import RockField from "../../../../Controls/rockField";
import DropDownList from "../../../../Elements/dropDownList";
import FieldFilterEditor from "../../../../Controls/fieldFilterEditor";
import LoadingIndicator from "../../../../Elements/loadingIndicator";
import Modal from "../../../../Controls/modal";
import Panel from "../../../../Controls/panel";
import RockButton from "../../../../Elements/rockButton";
import RockForm from "../../../../Controls/rockForm";
import Switch from "../../../../Elements/switch";
import TextBox from "../../../../Elements/textBox";
import { SectionAsideSettings } from "./types";
import { useFormSources, getFilterGroupTitle, getFilterRuleDescription, timeoutAsync } from "./utils";
import { FormError } from "../../../../Util/form";
import { FieldFilterGroup } from "../../../../ViewModels/Reporting/fieldFilterGroup";
import { FieldFilterSource } from "../../../../ViewModels/Reporting/fieldFilterSource";
import { FieldFilterRule } from "../../../../ViewModels/Reporting/fieldFilterRule";
import { areEqual } from "../../../../Util/guid";
import { useInvokeBlockAction } from "../../../../Util/block";
import { FormField } from "../Shared/types";

export default defineComponent({
    name: "Workflow.FormBuilderDetail.SectionEditAside",

    components: {
        ConfigurableZone,
        DropDownList,
        FieldFilterEditor,
        LoadingIndicator,
        Modal,
        Panel,
        RockButton,
        RockField,
        RockForm,
        Switch,
        TextBox
    },

    props: {
        modelValue: {
            type: Object as PropType<SectionAsideSettings>,
            required: true
        },

        formFields: {
            type: Array as PropType<FormField[]>,
            required: true
        }
    },

    emits: [
        "close",
        "update:modelValue",
        "validationChanged"
    ],

    methods: {
        /**
         * Checks if this aside is safe to close or if there are errors that
         * must be corrected first.
         */
        isSafeToClose(): boolean {
            this.formSubmit = true;

            return this.validationErrors.length === 0;
        }
    },

    setup(props, { emit }) {
        // #region Values

        const invokeBlockAction = useInvokeBlockAction();
        let conditionalSourcesLoadAttempted = false;

        const title = ref(props.modelValue.title);
        const description = ref(props.modelValue.description);
        const showHeadingSeparator = ref(props.modelValue.showHeadingSeparator);
        const sectionType = ref(props.modelValue.type ?? "");
        const visibilityRule = ref(props.modelValue.visibilityRule ?? null);

        /** The validation errors for the form. */
        const validationErrors = ref<FormError[]>([]);

        /** True if the form should start to submit. */
        const formSubmit = ref(false);

        /** Used to temporarily disable emitting the modelValue when something changes. */
        let autoSyncModelValue = true;

        const sectionTypeOptions = useFormSources().sectionTypeOptions ?? [];

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
            const getFilterSources = invokeBlockAction<FieldFilterSource[]>("GetFilterSources", {
                formFields: props.formFields
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
         * Event handler for when the validation state of the form has changed.
         *
         * @param errors Any errors that were detected on the form.
         */
        const onValidationChanged = (errors: FormError[]): void => {
            validationErrors.value = errors;
            emit("validationChanged", errors);
        };

        /**
         * Event handler for when the back button is clicked.
         */
        const onBackClick = (): void => emit("close");

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

        // Watch for the conditionals panel being opened and if it was the first
        // time then start loading all the filter sources.
        watch(conditionalPanelOpen, () => {
            if (!conditionalPanelOpen.value || conditionalSources.value !== null || conditionalSourcesLoadAttempted) {
                return;
            }

            conditionalSourcesLoadAttempted = true;
            loadConditionalSources();
        });

        // Watch for changes in the model value and update our internal values.
        watch(() => props.modelValue, () => {
            autoSyncModelValue = false;
            title.value = props.modelValue.title;
            description.value = props.modelValue.description;
            showHeadingSeparator.value = props.modelValue.showHeadingSeparator;
            sectionType.value = props.modelValue.type ?? "";
            visibilityRule.value = props.modelValue.visibilityRule ?? null;
            autoSyncModelValue = true;
        });

        // Watch for changes in our internal values and update the modelValue.
        watch([title, description, showHeadingSeparator, sectionType, visibilityRule], () => {
            if (!autoSyncModelValue) {
                return;
            }

            const value: SectionAsideSettings = {
                ...props.modelValue,
                title: title.value,
                description: description.value,
                showHeadingSeparator: showHeadingSeparator.value,
                type: sectionType.value === "" ? null : sectionType.value,
                visibilityRule: visibilityRule.value
            };

            emit("update:modelValue", value);
        });

        return {
            conditionalTitle,
            conditionalModalOpen,
            conditionalModel,
            conditionalPanelOpen,
            conditionalRules,
            conditionalSources,
            description,
            formSubmit,
            getRuleDescription,
            hasConditions,
            isConditionalsLoading,
            onBackClick,
            title,
            onConditionalEditClick,
            onConditionalSave,
            onValidationChanged,
            sectionType,
            sectionTypeOptions,
            showHeadingSeparator,
            validationErrors
        };
    },

    template: `
<div class="form-sidebar">
    <div class="sidebar-header">
        <div class="sidebar-back" @click="onBackClick">
            <i class="fa fa-chevron-left"></i>
        </div>

        <div class="title">
            Section
        </div>
    </div>

    <RockForm v-model:submit="formSubmit" @validationChanged="onValidationChanged" class="sidebar-body">
        <div class="sidebar-panels">
            <div></div>
            <Panel :modelValue="true" title="Section Configuration" hasCollapse>
                <TextBox v-model="title"
                    label="Title" />
                <TextBox v-model="description"
                    label="Description"
                    textMode="multiline" />
                <Switch v-model="showHeadingSeparator"
                    label="Show Heading Separator" />
                <DropDownList v-model="sectionType"
                    label="Type"
                    :options="sectionTypeOptions"
                    :showBlankItem="false" />
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
        </div>
    </RockForm>

    <Modal v-model="conditionalModalOpen" title="Conditional Settings" saveText="Save" @save="onConditionalSave">
        <FieldFilterEditor v-model="conditionalModel" :title="fieldName" :sources="conditionalSources" />
    </Modal>
</div>
`
});

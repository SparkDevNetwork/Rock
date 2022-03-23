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

import { nextTick } from "vue";
import { computed, defineComponent, ref, watch } from "vue";
import Alert from "../../../Elements/alert";
import Panel from "../../../Controls/panel";
import RockButton from "../../../Elements/rockButton";
import { FieldType } from "../../../SystemGuids";
import { useConfigurationValues, useInvokeBlockAction } from "../../../Util/block";
import { FormError } from "../../../Util/form";
import { areEqual } from "../../../Util/guid";
import { ListItem } from "../../../ViewModels";
import CommunicationsTab from "./FormBuilderDetail/communicationsTab";
import FormBuilderTab from "./FormBuilderDetail/formBuilderTab";
import SettingsTab from "./FormBuilderDetail/settingsTab";
import { FormBuilderDetailConfiguration, FormBuilderSettings, FormCommunication, FormTemplateListItem } from "./FormBuilderDetail/types";
import { provideFormSources } from "./FormBuilderDetail/utils";
import { FormCompletionAction, FormGeneral } from "./Shared/types";

export default defineComponent({
    name: "WorkFlow.FormBuilderDetail",

    components: {
        Alert,
        CommunicationsTab,
        FormBuilderTab,
        Panel,
        RockButton,
        SettingsTab
    },

    setup() {
        const config = useConfigurationValues<FormBuilderDetailConfiguration>();

        const invokeBlockAction = useInvokeBlockAction();

        const form = config.form ?? {};

        const isFormDirty = ref(false);

        const selectedTab = ref(0);

        const recipientOptions = ref<ListItem[]>([]);

        const communicationsViewModel = ref<FormCommunication>({
            confirmationEmail: form.confirmationEmail ?? {},
            notificationEmail: form.notificationEmail ?? {}
        });

        const generalViewModel = ref<FormGeneral>(form.general ?? {});

        const blockTitle = computed((): string => {
            return generalViewModel.value?.name + " Form" ?? "Workflow Form Builder";
        });

        const completionViewModel = ref<FormCompletionAction>(form.completion ?? {});

        const builderViewModel = ref<FormBuilderSettings>({
            allowPersonEntry: form.allowPersonEntry,
            campusSetFrom: form.campusSetFrom,
            footerContent: form.footerContent,
            headerContent: form.headerContent,
            personEntry: form.personEntry,
            sections: form.sections
        });

        const blockError = ref("");

        const formSubmit = ref(false);
        const communicationsValidationErrors = ref<FormError[]>([]);
        const formBuilderValidationErrors = ref<FormError[]>([]);
        const settingsValidationErrors = ref<FormError[]>([]);

        const isFormBuilderTabSelected = computed((): boolean => selectedTab.value === 0);
        const isCommunicationsTabSelected = computed((): boolean => selectedTab.value === 1);
        const isSettingsTabSelected = computed((): boolean => selectedTab.value === 2);

        const formBuilderContainerStyle = computed((): Record<string, string> => {
            return {
                display: isFormBuilderTabSelected.value ? "flex" : "none"
            };
        });

        const communicationsContainerStyle = computed((): Record<string, string> => {
            return {
                display: isCommunicationsTabSelected.value ? "flex" : "none"
            };
        });

        const settingsContainerStyle = computed((): Record<string, string> => {
            return {
                display: isSettingsTabSelected.value ? "flex" : "none"
            };
        });

        const selectedTemplate = computed((): FormTemplateListItem | null => {
            const matches = config.sources?.formTemplateOptions?.filter(t => areEqual(t.value, form.general?.template));

            return matches && matches.length > 0 ? matches[0] : null;
        });

        const onFormBuilderTabClick = (): void => {
            selectedTab.value = 0;
        };

        const onCommunicationsTabClick = (): void => {
            selectedTab.value = 1;
        };

        const onSettingsTabClick = (): void => {
            selectedTab.value = 2;
        };

        const onSaveClick = async (): Promise<void> => {
            // Trigger the submit for validation purposes and then on the next
            // UI pass turn it back off.
            formSubmit.value = true;
            nextTick(() => formSubmit.value = false);

            if (formBuilderValidationErrors.value.length > 0) {
                onFormBuilderTabClick();
                return;
            }

            if (communicationsValidationErrors.value.length > 0) {
                onCommunicationsTabClick();
                return;
            }

            if (settingsValidationErrors.value.length > 0) {
                onSettingsTabClick();
                return;
            }

            const result = await invokeBlockAction("SaveForm", {
                formGuid: config.formGuid,
                formSettings: form
            });

            if (!result.isSuccess) {
                alert(result.errorMessage ?? "Failed to save.");
            }
            else {
                isFormDirty.value = false;
            }
        };

        /**
         * Updates the recipientOptions value with a new list of recipients.
         * This should be called any time an attribute is changed so that
         * the list can be updated in case that attribute is now one of the
         * possible types.
         */
        const updateRecipientOptions = (): void => {
            const options: ListItem[] = [];

            // Include attributes from the main workflow.
            if (config.otherAttributes) {
                for (const attribute of config.otherAttributes) {
                    if (!attribute.guid || !attribute.fieldTypeGuid || !attribute.name) {
                        continue;
                    }

                    if (areEqual(attribute.fieldTypeGuid, FieldType.Person) || areEqual(attribute.fieldTypeGuid, FieldType.Email)) {
                        options.push({
                            value: attribute.guid,
                            text: attribute.name
                        });
                    }
                }
            }

            // If we have any sections defined, then include attributes from
            // the sections that match our criteria.
            if (form.sections) {
                for (const section of form.sections) {
                    if (!section.fields) {
                        continue;
                    }

                    for (const field of section.fields) {
                        if (areEqual(field.fieldTypeGuid, FieldType.Person) || areEqual(field.fieldTypeGuid, FieldType.Email)) {
                            options.push({
                                value: field.guid,
                                text: field.name
                            });
                        }
                    }
                }
            }

            // Sort everything to be alphabetical.
            options.sort((a, b) => {
                if (a.text < b.text) {
                    return -1;
                }
                else if (a.text > b.text) {
                    return 1;
                }
                else {
                    return 0;
                }
            });

            recipientOptions.value = options;
        };

        /**
         * Event handler called before the page unloads. This handler is
         * added whenever the form is dirty and needs to be saved.
         *
         * @param event The event that was raised.
         */
        const onBeforeUnload = (event: BeforeUnloadEvent): void => {
            event.preventDefault();
            event.returnValue = "";
        };

        /**
         * Event handler for when the validation state of the communications tab has changed.
         *
         * @param errors Any errors that were detected on the form.
         */
        const onCommunicationsValidationChanged = (errors: FormError[]): void => {
            communicationsValidationErrors.value = errors;
        };

        /**
         * Event handler for when the validation state of the form builder tab has changed.
         *
         * @param errors Any errors that were detected on the form.
         */
        const onFormBuilderValidationChanged = (errors: FormError[]): void => {
            formBuilderValidationErrors.value = errors;
        };

        /**
         * Event handler for when the validation state of the settings tab has changed.
         *
         * @param errors Any errors that were detected on the form.
         */
        const onSettingsValidationChanged = (errors: FormError[]): void => {
            settingsValidationErrors.value = errors;
        };

        // Watch for changes to our internal values and update the modelValue.
        watch([builderViewModel, communicationsViewModel, generalViewModel, completionViewModel], () => {
            form.allowPersonEntry = builderViewModel.value.allowPersonEntry;
            form.campusSetFrom = builderViewModel.value.campusSetFrom;
            form.footerContent = builderViewModel.value.footerContent;
            form.headerContent = builderViewModel.value.headerContent;
            form.personEntry = builderViewModel.value.personEntry;
            form.sections = builderViewModel.value.sections;

            form.general = generalViewModel.value;
            form.completion = completionViewModel.value;

            form.confirmationEmail = communicationsViewModel.value.confirmationEmail;
            form.notificationEmail = communicationsViewModel.value.notificationEmail;

            updateRecipientOptions();
            isFormDirty.value = true;
        });

        // Watch for changes in the form dirty state and remove/install our
        // handle to prevent accidentally navigating away from the page.
        watch(isFormDirty, () => {
            window.removeEventListener("beforeunload", onBeforeUnload);

            if (isFormDirty.value) {
                window.addEventListener("beforeunload", onBeforeUnload);
            }
        });

        provideFormSources(config.sources ?? {});
        updateRecipientOptions();

        if (!config.formGuid || !config.form) {
            blockError.value = "That form does not exist or it can't be edited.";
        }

        // Set initially selected tab.
        const queryString = new URLSearchParams(window.location.search.toLowerCase());
        if (queryString.has("tab")) {
            const tab = queryString.get("tab");

            if (tab === "communications") {
                selectedTab.value = 1;
            }
            else if (tab === "settings") {
                selectedTab.value = 2;
            }
        }

        return {
            analyticsPageUrl: config.analyticsPageUrl,
            blockError,
            builderViewModel,
            communicationsContainerStyle,
            communicationsValidationErrors,
            communicationsViewModel,
            completionViewModel,
            formBuilderContainerStyle,
            formSubmit,
            isCommunicationsTabSelected,
            isFormBuilderTabSelected,
            isFormDirty,
            isSettingsTabSelected,
            settingsContainerStyle,
            generalViewModel,
            blockTitle,
            submissionsPageUrl: config.submissionsPageUrl,
            onCommunicationsTabClick,
            onCommunicationsValidationChanged,
            onFormBuilderTabClick,
            onFormBuilderValidationChanged,
            onSaveClick,
            onSettingsTabClick,
            onSettingsValidationChanged,
            recipientOptions,
            selectedTemplate
        };
    },

    template: `
<Alert v-if="blockError" alertType="warning">
    {{ blockError }}
</Alert>

<Panel v-else type="block" hasFullscreen :title="blockTitle" titleIconClass="fa fa-poll-h">
    <template #default>
        <v-style>
            /*** Overrides for theme CSS ***/
            .form-builder-detail .form-section {
                margin-bottom: 0px;
            }

            .custom-switch {
                position: relative;
            }

            /*** Style Variables ***/
            .form-builder-detail {
                --zone-color: #ebebeb;
                --zone-action-bg: #f2f8fb;
                --zone-action-text-color: #a7a7a7;
                --zone-active-color: #c9eaf9;
                --zone-active-action-text-color: #83bad3;
                --zone-highlight-color: #ee7725;
                --zone-highlight-action-text-color: #e4bda2;
                --flex-col-gutter: 30px;
            }

            /*** Form Template Items ***/
            .form-builder-detail .form-template-item {
                display: flex;
                align-items: center;
                background-color: #ffffff;
                border: 1px solid #e1e1e1;
                border-left-width: 3px;
                border-radius: 3px;
                padding: 6px;
                font-size: 13px;
                font-weight: 600;
                cursor: grab;
            }

            .form-builder-detail .form-template-item > .fa {
                margin-right: 6px;
            }

            .form-builder-detail .form-template-item > .text {
                white-space: nowrap;
                overflow: hidden;
                text-overflow: ellipsis;
            }

            .form-builder-detail .form-template-item.form-template-item-section {
                border-left-color: #009ce3;
            }

            .form-builder-detail .form-template-item.form-template-item-field {
                border-left-color: #ee7725;
                margin: 0 3px 6px;
                flex: 0 0 auto;
                width: calc(50% - 6px);
            }

            /*** Configuration Asides ***/
            .form-builder-detail .aside-header {
                border-bottom: 1px solid #dfe0e1;
            }

            .form-builder-detail .aside-header:last-child {
                border-right: 1px solid #dfe0e1;
            }

            .form-builder-detail .aside-header .fa + .title {
                margin-left: 4px;
            }

            .form-builder-detail .aside-header .title {
                font-size: 85%;
                font-weight: 600;
            }

            .form-builder-detail .aside-body {
                padding: 15px;
            }

            /*** Configurable Zones ***/
            .form-builder-detail .configurable-zone {
                display: flex;
                position: relative;
                margin-bottom: 12px;
            }

            .form-builder-detail .configurable-zone.zone-section {
                flex-grow: 1;
            }

            .form-builder-detail .configurable-zone.zone-section > .zone-actions{
                position: relative;
            }

            .zone-action {
                z-index: 10;
            }

            .zone-action.stretched-link {
                z-index: 1;
            }

            .form-builder-detail .configurable-zone > .zone-content-container {
                display: flex;
                flex-grow: 1;
                border: 2px solid var(--zone-color);
            }

            .form-builder-detail .configurable-zone.zone-section > .zone-content-container {
                border-style: dashed;
                border-right-style: solid;
            }

            .form-builder-detail .configurable-zone > .zone-content-container > .zone-content {
                flex-grow: 1;
            }

            .form-builder-detail .configurable-zone > .zone-content-container > .zone-content > .zone-body {
                min-height: 100%;
                padding: 20px;
            }

            .form-builder-detail .configurable-zone > .zone-actions {
                background-color: var(--zone-action-bg);
                border: 2px solid var(--zone-color);
                border-left: 0px;
                width: 40px;
                flex-shrink: 0;
                display: flex;
                flex-direction: column;
                justify-content: center;
                align-items: center;
                color: var(--zone-action-text-color);
            }

            .form-builder-detail .configurable-zone > .zone-actions > .zone-action-pad {
                flex-grow: 1;
            }

            .form-builder-detail .configurable-zone > .zone-actions > .zone-action {
                margin: 5px 0px;
                cursor: pointer;
            }

            .form-builder-detail .configurable-zone > .zone-actions > .zone-action-move {
                cursor: grab;
            }

            .form-builder-detail .configurable-zone.active > .zone-content-container {
                border-color: var(--zone-active-color);
            }

            .form-builder-detail .configurable-zone.active > .zone-actions {
                background-color: var(--zone-active-color);
                border-color: var(--zone-active-color);
                color: var(--zone-active-action-text-color);
            }

            .form-builder-detail .configurable-zone.highlight > .zone-content-container {
                border-color: var(--zone-highlight-color);
                border-right-style: dashed;
            }
/*
            .form-builder-detail .configurable-zone.active > .zone-actions > .zone-action,
            .form-builder-detail .configurable-zone:hover > .zone-actions > .zone-action {
                display: initial;
            }
*/

            /*** Form Sections ***/
            .form-builder-detail .form-section {
                display: flex;
                flex-wrap: wrap;
                margin: 0 calc(-.5 * var(--flex-col-gutter)) -10px;
                min-height: 50px;
            }

            .form-builder-detail .form-section .form-template-item.form-template-item-field {
                margin: 0 calc(.5 * var(--flex-col-gutter)) 12px;
                flex-basis: calc(100% - var(--flex-col-gutter));
                padding: 16px;
                font-size: 18px;
            }

            .form-section .configurable-zone > .zone-actions {
                transition: opacity 116ms cubic-bezier(0.2, 0.2, 0.38, 0.9);
                opacity: 0;
                /*transform: scaleX(0);*/
                transform-origin: 0% 0%;
            }

            .form-section .configurable-zone.active > .zone-actions,
            .form-section .configurable-zone:hover > .zone-actions {
                opacity: 1;
                /*transform: scaleX(1);*/
                transform-origin: 0% 0%;
            }

            /*** Flex Column Sizes ***/
            .form-builder-detail .flex-col {
                margin-left: calc(.5 * var(--flex-col-gutter));
                margin-right: calc(.5 * var(--flex-col-gutter));
            }

            .form-builder-detail .flex-col-1 {
                flex-basis: calc(8.3333% - var(--flex-col-gutter));
            }

            .form-builder-detail .flex-col-2 {
                flex-basis: calc(16.6666% - var(--flex-col-gutter));
            }

            .form-builder-detail .flex-col-3 {
                flex-basis: calc(25% - var(--flex-col-gutter));
            }

            .form-builder-detail .flex-col-4 {
                flex-basis: calc(33.3333% - var(--flex-col-gutter));
            }

            .form-builder-detail .flex-col-5 {
                flex-basis: calc(41.6666% - var(--flex-col-gutter));
            }

            .form-builder-detail .flex-col-6 {
                flex-basis: calc(50% - var(--flex-col-gutter));
            }

            .form-builder-detail .flex-col-7 {
                flex-basis: calc(58.3333% - var(--flex-col-gutter));
            }

            .form-builder-detail .flex-col-8 {
                flex-basis: calc(66.6666% - var(--flex-col-gutter));
            }

            .form-builder-detail .flex-col-9 {
                flex-basis: calc(75% - var(--flex-col-gutter));
            }

            .form-builder-detail .flex-col-10 {
                flex-basis: calc(83.3333% - var(--flex-col-gutter));
            }

            .form-builder-detail .flex-col-11 {
                flex-basis: calc(91.6666% - var(--flex-col-gutter));
            }

            .form-builder-detail .flex-col-12 {
                flex-basis: calc(100% - var(--flex-col-gutter));
            }
        </v-style>

        <div ref="bodyElement" class="form-builder-detail d-flex flex-column panel-flex-fill-body overflow-hidden styled-scroll">
            <div class="d-flex align-items-center p-2  border-bottom border-panel" style="box-shadow: rgba(0,0,0,0.15) 0 0 4px;">
                <ul class="nav nav-pills flex-grow-1">
                    <li role="presentation"><a :href="submissionsPageUrl">Submissions</a></li>
                    <li :class="{ active: isFormBuilderTabSelected }" role="presentation"><a href="#" @click.prevent="onFormBuilderTabClick">Form Builder</a></li>
                    <li :class="{ active: isCommunicationsTabSelected }" role="presentation"><a href="#" @click.prevent="onCommunicationsTabClick">Communications</a></li>
                    <li :class="{ active: isSettingsTabSelected }" role="presentation"><a href="#" @click.prevent="onSettingsTabClick">Settings</a></li>
                    <li role="presentation"><a :href="analyticsPageUrl">Analytics</a></li>
                </ul>

                <RockButton btnType="primary" :disabled="!isFormDirty" @click="onSaveClick">Save</RockButton>
            </div>

            <div class="flex-grow-1 overflow-y-hidden" :style="formBuilderContainerStyle">
                <FormBuilderTab v-model="builderViewModel"
                    :templateOverrides="selectedTemplate"
                    :submit="formSubmit"
                    @validationChanged="onFormBuilderValidationChanged" />
            </div>

            <div class="flex-grow-1 overflow-y-hidden" :style="communicationsContainerStyle">
                <CommunicationsTab v-model="communicationsViewModel"
                    :recipientOptions="recipientOptions"
                    :templateOverrides="selectedTemplate"
                    :submit="formSubmit"
                    @validationChanged="onCommunicationsValidationChanged" />
            </div>

            <div class="flex-grow-1 overflow-y-hidden" :style="settingsContainerStyle">
                <SettingsTab v-model="generalViewModel"
                    v-model:completion="completionViewModel"
                    :templateOverrides="selectedTemplate"
                    :submit="formSubmit"
                    @validationChanged="onSettingsValidationChanged" />
            </div>
        </div>
    </template>
</Panel>
`
});

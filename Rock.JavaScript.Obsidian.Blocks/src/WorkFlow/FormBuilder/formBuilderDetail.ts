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
import NotificationBox from "@Obsidian/Controls/notificationBox.obs";
import Panel from "@Obsidian/Controls/panel";
import RockButton from "@Obsidian/Controls/rockButton";
import { FieldType } from "@Obsidian/SystemGuids/fieldType";
import { useConfigurationValues, useInvokeBlockAction } from "@Obsidian/Utility/block";
import { FormError } from "@Obsidian/Utility/form";
import { areEqual } from "@Obsidian/Utility/guid";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import CommunicationsTab from "./FormBuilderDetail/communicationsTab.partial";
import FormBuilderTab from "./FormBuilderDetail/formBuilderTab.partial";
import SettingsTab from "./FormBuilderDetail/settingsTab.partial";
import { FormBuilderDetailConfiguration, FormBuilderSettings, FormCommunication, FormTemplateListItem } from "./FormBuilderDetail/types.partial";
import { provideFormSources } from "./FormBuilderDetail/utils.partial";
import { FormCompletionAction, FormGeneral } from "./Shared/types.partial";

export default defineComponent({
    name: "WorkFlow.FormBuilderDetail",

    components: {
        NotificationBox,
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

        const recipientOptions = ref<ListItemBag[]>([]);

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
            const options: ListItemBag[] = [];

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
                if ((a.text ?? "") < (b.text ?? "")) {
                    return -1;
                }
                else if ((a.text ?? "") > (b.text ?? "")) {
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
<NotificationBox v-if="blockError" alertType="warning">
    {{ blockError }}
</NotificationBox>

<Panel v-else type="block" hasFullscreen :title="blockTitle" titleIconCssClass="fa fa-poll-h">
    <template #default>

        <div ref="bodyElement" class="panel-flex-fill-body styled-scroll">
            <div class="panel-toolbar panel-toolbar-shadow">
                <ul class="nav nav-pills nav-sm">
                    <li role="presentation"><a :href="submissionsPageUrl">Submissions</a></li>
                    <li :class="{ active: isFormBuilderTabSelected }" role="presentation"><a href="#" @click.prevent="onFormBuilderTabClick">Form Builder</a></li>
                    <li :class="{ active: isCommunicationsTabSelected }" role="presentation"><a href="#" @click.prevent="onCommunicationsTabClick">Communications</a></li>
                    <li :class="{ active: isSettingsTabSelected }" role="presentation"><a href="#" @click.prevent="onSettingsTabClick">Settings</a></li>
                    <li role="presentation"><a :href="analyticsPageUrl">Analytics</a></li>
                </ul>

                <RockButton btnType="primary" btnSize="sm" :disabled="!isFormDirty" @click="onSaveClick">Save</RockButton>
            </div>

            <div class="form-builder-container form-builder-grow" :style="formBuilderContainerStyle">
                <FormBuilderTab v-model="builderViewModel"
                    :templateOverrides="selectedTemplate"
                    :submit="formSubmit"
                    @validationChanged="onFormBuilderValidationChanged" />
            </div>

            <div class="communications-container form-builder-grow" :style="communicationsContainerStyle">
                <CommunicationsTab v-model="communicationsViewModel"
                    :recipientOptions="recipientOptions"
                    :templateOverrides="selectedTemplate"
                    :submit="formSubmit"
                    @validationChanged="onCommunicationsValidationChanged" />
            </div>

            <div class="settings-container form-builder-grow" :style="settingsContainerStyle">
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

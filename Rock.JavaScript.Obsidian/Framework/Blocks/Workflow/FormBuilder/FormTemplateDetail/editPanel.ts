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
import Panel from "../../../../Controls/panel";
import RockForm from "../../../../Controls/rockForm";
import AuditDetail from "../../../../Elements/auditDetail";
import CheckBox from "../../../../Elements/checkBox";
import RockButton from "../../../../Elements/rockButton";
import TextBox from "../../../../Elements/textBox";
import { updateRefValue } from "../../../../Util/util";
import { ListItem } from "../../../../ViewModels";
import CompletionSettings from "../Shared/completionSettings";
import ConfirmationEmail from "../Shared/confirmationEmail";
import PersonEntrySettings from "../Shared/personEntrySettings";
import SettingsWell from "../Shared/settingsWell";
import { FormCompletionAction } from "../Shared/types";
import { TemplateEditDetail } from "./types";
import { useSources } from "./utils";

/**
 * The recipient options that are available for the individual to pick from.
 * These are hard-coded values that will then be translated when the form is
 * actually run to lookup the real values.
 */
const recipientOptions: ListItem[] = [
    {
        value: "00000000-0000-0000-0000-000000000001",
        text: "Person"
    },
    {
        value: "00000000-0000-0000-0000-000000000002",
        text: "Spouse"
    }
];

export default defineComponent({
    name: "Workflow.FormTemplateDetail",

    components: {
        AuditDetail,
        CheckBox,
        CompletionSettings,
        ConfirmationEmail,
        Panel,
        PersonEntrySettings,
        RockButton,
        RockForm,
        SettingsWell,
        TextBox
    },

    props: {
        modelValue: {
            type: Object as PropType<TemplateEditDetail>,
            default: {}
        }
    },

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        const sources = useSources();

        // Setup all the standard values that we will be editing.
        const name = ref(props.modelValue.name ?? "");
        const description = ref(props.modelValue.description ?? "");
        const isActive = ref(props.modelValue.isActive ?? true);
        const isLoginRequired = ref(props.modelValue.isLoginRequired ?? false);
        const formHeader = ref(props.modelValue.formHeader ?? "");
        const formFooter = ref(props.modelValue.formFooter ?? "");
        const allowPersonEntry = ref(props.modelValue.allowPersonEntry ?? false);
        const personEntry = ref(props.modelValue.personEntry ?? {});
        const confirmationEmail = ref(props.modelValue.confirmationEmail ?? {});

        // The raw completion action value that will be used to track state.
        const internalCompletionAction = ref(props.modelValue.completionAction);

        /**
         * Use a computed value so that we can provide the CompletionSettings
         * component with a non-null value while using a null value to identify
         * that the completion action isn't enabled.
         */
        const completionAction = computed<FormCompletionAction>({
            get() {
                return internalCompletionAction.value ?? {};
            },
            set(value) {
                if (completionActionEnabled.value) {
                    updateRefValue(internalCompletionAction, value);
                }
            }
        });

        /**
         * Use a computed value to track if the completion action panel is
         * enabled or not.
         */
        const completionActionEnabled = computed<boolean>({
            get() {
                return !!internalCompletionAction.value;
            },
            set(value) {
                updateRefValue(internalCompletionAction, value ? {} : null);
            }
        });

        // Watch for a change in the modelValue and then update all our internal
        // values with what has changed.
        watch(() => props.modelValue, () => {
            updateRefValue(name, props.modelValue.name ?? "");
            updateRefValue(description, props.modelValue.description ?? "");
            updateRefValue(isActive, props.modelValue.isActive ?? true);
            updateRefValue(isLoginRequired, props.modelValue.isLoginRequired ?? false);
            updateRefValue(formHeader, props.modelValue.formHeader ?? "");
            updateRefValue(formFooter, props.modelValue.formFooter ?? "");
            updateRefValue(allowPersonEntry, props.modelValue.allowPersonEntry ?? false);
            updateRefValue(personEntry, props.modelValue.personEntry ?? {});
            updateRefValue(confirmationEmail, props.modelValue.confirmationEmail ?? {});
            updateRefValue(internalCompletionAction, props.modelValue.completionAction);
        });

        // Watch for a change in any of our internal values and then update the
        // modelValue with the new value.
        watch([name, description, isActive, isLoginRequired, formHeader, formFooter, allowPersonEntry, personEntry, confirmationEmail, internalCompletionAction], () => {
            const newValue: TemplateEditDetail = {
                ...props.modelValue,
                name: name.value,
                description: description.value,
                isActive: isActive.value,
                isLoginRequired: isLoginRequired.value,
                formHeader: formHeader.value,
                formFooter: formFooter.value,
                allowPersonEntry: allowPersonEntry.value,
                personEntry: personEntry.value,
                confirmationEmail: confirmationEmail.value,
                completionAction: internalCompletionAction.value
            };

            emit("update:modelValue", newValue);
        });

        return {
            addressTypeOptions: sources.addressTypeOptions,
            allowPersonEntry,
            campusStatusOptions: sources.campusStatusOptions,
            campusTypeOptions: sources.campusTypeOptions,
            completionAction,
            completionActionEnabled,
            confirmationEmail,
            connectionStatusOptions: sources.connectionStatusOptions,
            description,
            formFooter,
            formHeader,
            isActive,
            isLoginRequired,
            name,
            personEntry,
            recipientOptions,
            recordStatusOptions: sources.recordStatusOptions,
            sourceTemplateOptions: sources.emailTemplateOptions
        };
    },

    template: `
<div>
    <div class="row">
        <div class="col-md-6">
            <TextBox v-model="name"
                label="Name"
                rules="required" />
        </div>

        <div class="col-md-6">
            <CheckBox v-model="isActive"
                label="Active" />
        </div>
    </div>

    <TextBox v-model="description"
        label="Description"
        textMode="multiline" />

    <CheckBox v-model="isLoginRequired"
        label="Is Login Required"
        help="Determines if a person needs to be logged in to complete this form." />

    <SettingsWell title="Form Headers and Footers"
        description="The headers and footers below will be displayed on all pages of the forms that use this template.">
        <TextBox v-model="formHeader"
            label="Form Header"
            textMode="multiline" />

        <TextBox v-model="formFooter"
            label="Form Footer"
            textMode="multiline" />
    </SettingsWell>

    <SettingsWell v-model="allowPersonEntry"
        hasEnable
        title="Person Entry Settings"
        description="These settings will lock the forms person entry settings.">
        <PersonEntrySettings v-model="personEntry"
            :recordStatusOptions="recordStatusOptions"
            :connectionStatusOptions="connectionStatusOptions"
            :campusTypeOptions="campusTypeOptions"
            :campusStatusOptions="campusStatusOptions"
            :addressTypeOptions="addressTypeOptions" />
    </SettingsWell>

    <ConfirmationEmail v-model="confirmationEmail"
        :recipientOptions="recipientOptions"
        :sourceTemplateOptions="sourceTemplateOptions" />

    <CompletionSettings v-model="completionAction" v-model:enabled="completionActionEnabled" hasEnable />
</div>
`
});

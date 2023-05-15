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

import { computed } from "vue";
import { defineComponent, PropType, ref, watch } from "vue";
import RockForm from "@Obsidian/Controls/rockForm";
import NotificationBox from "@Obsidian/Controls/notificationBox.obs";
import { FormError } from "@Obsidian/Utility/form";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import ConfirmationEmail from "../Shared/confirmationEmail";
import NotificationEmail from "./notificationEmail.partial";
import { FormCommunication, FormTemplateListItem } from "./types.partial";
import { useFormSources } from "./utils.partial";

export default defineComponent({
    name: "Workflow.FormBuilderDetail.CommunicationsTab",

    components: {
        NotificationBox,
        ConfirmationEmail,
        NotificationEmail,
        RockForm
    },

    props: {
        modelValue: {
            type: Object as PropType<FormCommunication>,
            required: true
        },

        recipientOptions: {
            type: Array as PropType<ListItemBag[]>,
            default: []
        },

        templateOverrides: {
            type: Object as PropType<FormTemplateListItem>
        },

        submit: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    emits: [
        "update:modelValue",
        "validationChanged"
    ],

    setup(props, { emit }) {
        const confirmationEmail = ref(props.modelValue.confirmationEmail ?? {});

        const notificationEmail = ref(props.modelValue.notificationEmail ?? {});

        const formSubmit = ref(false);

        const sources = useFormSources();

        const sourceTemplateOptions = sources.emailTemplateOptions ?? [];
        const campusTopicOptions = sources.campusTopicOptions ?? [];

        const isConfirmationEmailForced = computed((): boolean => props.templateOverrides?.isConfirmationEmailConfigured ?? false);

        /**
         * Event handler for when the validation state of the form has changed.
         *
         * @param errors Any errors that were detected on the form.
         */
        const onValidationChanged = (errors: FormError[]): void => {
            emit("validationChanged", errors);
        };

        watch(() => props.modelValue, () => {
            confirmationEmail.value = props.modelValue.confirmationEmail ?? {};
            notificationEmail.value = props.modelValue.notificationEmail ?? {};
        });

        watch([confirmationEmail, notificationEmail], () => {
            const newValue: FormCommunication = {
                ...props.modelValue,
                confirmationEmail: confirmationEmail.value,
                notificationEmail: notificationEmail.value
            };

            emit("update:modelValue", newValue);
        });

        // Any time the parent component tells us it has attempted to submit
        // then we trigger the submit on our form so it updates the validation.
        watch(() => props.submit, () => {
            if (props.submit) {
                formSubmit.value = true;
            }
        });

        return {
            campusTopicOptions,
            confirmationEmail,
            formSubmit,
            isConfirmationEmailForced,
            notificationEmail,
            onValidationChanged,
            sourceTemplateOptions,
        };
    },

    template: `
<div class="form-builder-scroll">
    <div class="panel-body">
        <RockForm v-model:submit="formSubmit" @validationChanged="onValidationChanged">
            <ConfirmationEmail v-if="!isConfirmationEmailForced" v-model="confirmationEmail" :sourceTemplateOptions="sourceTemplateOptions" :recipientOptions="recipientOptions" />
            <NotificationBox v-else alertType="info">
                <h4 class="alert-heading">Confirmation Email</h4>
                <p>
                    The confirmation e-mail is defined on the template and cannot be changed.
                </p>
            </NotificationBox>

            <NotificationEmail v-model="notificationEmail" :sourceTemplateOptions="sourceTemplateOptions" :campusTopicOptions="campusTopicOptions" />
        </RockForm>
    </div>
</div>
`
});

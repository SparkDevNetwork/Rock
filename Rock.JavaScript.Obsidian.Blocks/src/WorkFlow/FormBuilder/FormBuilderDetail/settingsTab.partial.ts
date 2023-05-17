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

import { computed, defineComponent, ref, PropType, watch } from "vue";
import RockForm from "@Obsidian/Controls/rockForm";
import NotificationBox from "@Obsidian/Controls/notificationBox.obs";
import { useVModelPassthrough } from "@Obsidian/Utility/component";
import CompletionSettings from "../Shared/completionSettings";
import GeneralSettings from "./generalSettings.partial";
import { FormCompletionAction, FormGeneral } from "../Shared/types.partial";
import { FormTemplateListItem } from "./types.partial";
import { FormError } from "@Obsidian/Utility/form";

export default defineComponent({
    name: "Workflow.FormBuilderDetail.SettingsTab",

    components: {
        NotificationBox,
        GeneralSettings,
        CompletionSettings,
        RockForm
    },

    props: {
        modelValue: {
            type: Object as PropType<FormGeneral>,
            required: true
        },

        completion: {
            type: Object as PropType<FormCompletionAction>,
            required: true
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
        "update:completion",
        "validationChanged"
    ],

    setup(props, { emit }) {
        const generalSettings = useVModelPassthrough(props, "modelValue", emit);
        const completionSettings = useVModelPassthrough(props, "completion", emit);
        const formSubmit = ref(false);

        const isConfirmationForced = computed((): boolean => props.templateOverrides?.isConfirmationEmailConfigured ?? false);

        /**
         * Event handler for when the validation state of the form has changed.
         *
         * @param errors Any errors that were detected on the form.
         */
        const onValidationChanged = (errors: FormError[]): void => {
            emit("validationChanged", errors);
        };

        // Any time the parent component tells us it has attempted to submit
        // then we trigger the submit on our form so it updates the validation.
        watch(() => props.submit, () => {
            if (props.submit) {
                formSubmit.value = true;
            }
        });

        return {
            completionSettings,
            formSubmit,
            generalSettings,
            isConfirmationForced,
            onValidationChanged
        };
    },

    template: `
<div class="form-builder-scroll">
    <div class="panel-body">
        <RockForm v-model:submit="formSubmit" @validationChanged="onValidationChanged">
            <GeneralSettings v-model="generalSettings" :templateOverrides="templateOverrides" />

            <CompletionSettings v-if="!isConfirmationForced" v-model="completionSettings" />
            <NotificationBox v-else alertType="info">
                <h4 class="alert-heading">Confirmation Email</h4>
                <p>
                    The completion action is defined on the template and cannot be changed.
                </p>
            </NotificationBox>
        </RockForm>
    </div>
</div>
`
});

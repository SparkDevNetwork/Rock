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
import DropDownList from "../../../../Elements/dropDownList";
import TextBox from "../../../../Elements/textBox";
import { toNumberOrNull } from "../../../../Services/number";
import { ListItem } from "../../../../ViewModels";
import SegmentedPicker from "./segmentedPicker";
import SettingsWell from "./settingsWell";
import { FormCompletionActionType, FormCompletionAction } from "./types";

const typeOptions: ListItem[] = [
    {
        value: FormCompletionActionType.DisplayMessage.toString(),
        text: "Display Message"
    },
    {
        value: FormCompletionActionType.Redirect.toString(),
        text: "Redirect to New Page"
    }
];

/**
 * Displays the UI for the Completion Settings section of the Settings screen.
 */
export default defineComponent({
    name: "Workflow.FormBuilderDetail.CompletionSettings",

    components: {
        DropDownList,
        SegmentedPicker,
        SettingsWell,
        TextBox
    },

    props: {
        modelValue: {
            type: Object as PropType<FormCompletionAction>,
            required: true
        }
    },

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        /** The type of completion logic to use when the form has been completed. */
        const type = ref(props.modelValue.type?.toString() ?? FormCompletionActionType.DisplayMessage.toString());

        /** The message to display to the user. */
        const message = ref(props.modelValue.message ?? "");

        /** The URL to redirect the user to. */
        const redirectUrl = ref(props.modelValue.redirectUrl ?? "");

        /** True if the type is DisplayMessage. */
        const isTypeDisplayMessage = computed((): boolean => type.value === FormCompletionActionType.DisplayMessage.toString());

        /** True if the type is Redirect */
        const isTypeRedirect = computed((): boolean => type.value === FormCompletionActionType.Redirect.toString());

        // Watch for changes in our modelValue and then update all our internal values.
        watch(() => props.modelValue, () => {
            type.value = props.modelValue.type?.toString() ?? FormCompletionActionType.DisplayMessage.toString();
            message.value = props.modelValue.message ?? "";
            redirectUrl.value = props.modelValue.redirectUrl ?? "";
        });

        // Watch for changes on any of our internal values and then update the modelValue.
        watch([type, message, redirectUrl], () => {
            const newValue: FormCompletionAction = {
                ...props.modelValue,
                type: toNumberOrNull(type.value) ?? FormCompletionActionType.DisplayMessage,
                message: message.value,
                redirectUrl: redirectUrl.value
            };

            emit("update:modelValue", newValue);
        });

        return {
            isTypeDisplayMessage,
            isTypeRedirect,
            message,
            redirectUrl,
            type,
            typeOptions
        };
    },

    template: `
<SettingsWell title="Completion Settings"
    description="The settings below determine the actions to take after an individual completes the form.">
    <SegmentedPicker v-model="type"
        :options="typeOptions" />

    <div v-if="isTypeDisplayMessage">
        <TextBox v-model="message"
            label="Completion Message"
            textMode="multiline"
            rules="required" />
    </div>

    <div v-else-if="isTypeRedirect">
        <TextBox v-model="redirectUrl"
            label="Redirect URL"
            rules="required" />
    </div>
</Settingswell>
`
});

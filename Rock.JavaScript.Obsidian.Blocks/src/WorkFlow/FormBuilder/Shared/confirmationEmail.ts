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

import { defineComponent, PropType, ref, watch } from "vue";
import DropDownList from "@Obsidian/Controls/dropDownList";
import InlineSwitch from "@Obsidian/Controls/inlineSwitch";
import TransitionVerticalCollapse from "@Obsidian/Controls/transitionVerticalCollapse";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import EmailSource from "./emailSource";
import SectionContainer from "@Obsidian/Controls/sectionContainer";
import { FormConfirmationEmail, FormEmailSource } from "./types.partial";

/**
 * Displays the UI for the Confirmation Email component in the Communications
 * screen.
 */
export default defineComponent({
    name: "Workflow.FormBuilderDetail.ConfirmationEmail",

    components: {
        DropDownList,
        EmailSource,
        InlineSwitch,
        SectionContainer,
        TransitionVerticalCollapse
    },

    props: {
        modelValue: {
            type: Object as PropType<FormConfirmationEmail>,
            required: true
        },

        recipientOptions: {
            type: Array as PropType<ListItemBag[]>,
            default: []
        },

        sourceTemplateOptions: {
            type: Array as PropType<ListItemBag[]>,
            default: []
        }
    },

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        const enabled = ref(props.modelValue.enabled ?? false);
        const recipientAttributeGuid = ref(props.modelValue.recipientAttributeGuid ?? null);
        const source = ref<FormEmailSource>(props.modelValue.source ?? {});

        // Watch for changes in our modelValue and then update all our internal values.
        watch(() => props.modelValue, () => {
            enabled.value = props.modelValue.enabled ?? false;
            recipientAttributeGuid.value = props.modelValue.recipientAttributeGuid ?? null;
            source.value = props.modelValue.source ?? {};
        });

        // Watch for changes on any of our internal values and then update the modelValue.
        watch([enabled, recipientAttributeGuid, source], () => {
            const newValue: FormConfirmationEmail = {
                ...props.modelValue,
                enabled: enabled.value,
                recipientAttributeGuid: recipientAttributeGuid.value,
                source: source.value
            };

            emit("update:modelValue", newValue);
        });

        return {
            enabled,
            recipientAttributeGuid,
            source
        };
    },

    template: `
<SectionContainer v-model="enabled"
    toggleText="Enable"
    title="Confirmation Email"
    description="The following settings will be used to send an email to the individual who submitted the form.">
    <div class="row">
        <div class="col-md-4">
            <DropDownList v-model="recipientAttributeGuid"
                label="Recipient"
                rules="required"
                :items="recipientOptions" />
        </div>
    </div>

    <div class="mt-3">
        <EmailSource v-model="source" :templateOptions="sourceTemplateOptions" />
    </div>
</SectionContainer>
`
});

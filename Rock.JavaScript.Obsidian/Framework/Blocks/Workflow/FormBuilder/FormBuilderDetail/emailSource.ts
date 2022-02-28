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
import InlineCheckbox from "../../../../Elements/inlineCheckBox";
import TextBox from "../../../../Elements/textBox";
import { toNumberOrNull } from "../../../../Services/number";
import { ListItem } from "../../../../ViewModels";
import SegmentedPicker from "./segmentedPicker";
import { FormEmailSource, FormEmailSourceType } from "./types";

const emailSourceOptions: ListItem[] = [
    {
        value: FormEmailSourceType.UseTemplate.toString(),
        text: "Use Email Template"
    },
    {
        value: FormEmailSourceType.Custom.toString(),
        text: "Provide Custom Email"
    }
];

/**
 * Displays the UI for where an e-mail comes from. Either from a specific e-mail
 * template or a user-defined custom e-mail.
 */
export default defineComponent({
    name: "Workflow.FormBuilderDetail.EmailSource",

    components: {
        DropDownList,
        InlineCheckbox,
        SegmentedPicker,
        TextBox
    },

    props: {
        modelValue: {
            type: Object as PropType<FormEmailSource>,
            default: {}
        },

        templateOptions: {
            type: Array as PropType<ListItem[]>,
            default: []
        }
    },

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        /** The source type currently being edited. */
        const type = ref(props.modelValue.type?.toString() ?? FormEmailSourceType.UseTemplate.toString());

        /** The currently selected template from the list of options. */
        const template = ref(props.modelValue.template ?? "");

        /** The subject text for the custom e-mail. */
        const subject = ref(props.modelValue.subject ?? "");

        /** The Reply-To text for the custom e-mail. */
        const replyTo = ref(props.modelValue.replyTo ?? "");

        /** The body content HTML for the custom e-mail. */
        const body = ref(props.modelValue.body ?? "");

        /** True if the standard header and footer should be appended on send. */
        const appendOrgHeaderAndFooter = ref(props.modelValue.appendOrgHeaderAndFooter ?? false);

        /** True if the currently selected type is for UseTemplate. */
        const isTemplateType = computed((): boolean => type.value === FormEmailSourceType.UseTemplate.toString());

        /** True if the currently selected type is for Custom. */
        const isCustomType = computed((): boolean => type.value === FormEmailSourceType.Custom.toString());

        // Watch for changes for the modelValue and update all our internal values.
        watch(() => props.modelValue, () => {
            type.value = props.modelValue.type?.toString() ?? FormEmailSourceType.UseTemplate.toString();
            template.value = props.modelValue.template ?? "";
            subject.value = props.modelValue.subject ?? "";
            replyTo.value = props.modelValue.replyTo ?? "";
            body.value = props.modelValue.body ?? "";
            appendOrgHeaderAndFooter.value = props.modelValue.appendOrgHeaderAndFooter ?? false;
        });

        // Watch for changes to any of our internal values and update the modelValue.
        watch([type, template, subject, replyTo, body, appendOrgHeaderAndFooter], () => {
            const newValue: FormEmailSource = {
                ...props.modelValue,
                type: toNumberOrNull(type.value) ?? FormEmailSourceType.UseTemplate,
                template: template.value,
                subject: subject.value,
                replyTo: replyTo.value,
                body: body.value,
                appendOrgHeaderAndFooter: appendOrgHeaderAndFooter.value
            };

            emit("update:modelValue", newValue);
        });

        return {
            appendOrgHeaderAndFooter,
            type,
            template,
            subject,
            replyTo,
            body,
            emailSourceOptions,
            isCustomType,
            isTemplateType
        };
    },

    template: `
<div>
    <SegmentedPicker v-model="type"
        :options="emailSourceOptions" />

    <div v-if="isTemplateType">
        <div class="row">
            <div class="col-md-4">
                <DropDownList v-model="template"
                    label="Email Template"
                    rules="required"
                    :options="templateOptions" />
            </div>
        </div>
    </div>

    <div v-else-if="isCustomType">
        <div class="row">
            <div class="col-md-4">
                <TextBox v-model="subject"
                    label="Subject"
                    rules="required" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <TextBox v-model="replyTo"
                    label="Reply To"
                    rules="email" />
            </div>
        </div>

        <TextBox v-model="body"
            label="Email Body"
            textMode="multiline"
            rules="required" />

        <InlineCheckbox v-model="appendOrgHeaderAndFooter"
            label="Append Organization Header and Footer" />
    </div>
</div>
`
});

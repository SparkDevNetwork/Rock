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
import CategoryPicker from "../../../../Controls/categoryPicker";
import Alert from "../../../../Elements/alert";
import CheckBox from "../../../../Elements/checkBox";
import DateTimePicker from "../../../../Elements/dateTimePicker";
import DropDownList from "../../../../Elements/dropDownList";
import InlineSwitch from "../../../../Elements/inlineSwitch";
import TextBox from "../../../../Elements/textBox";
import TransitionVerticalCollapse from "../../../../Elements/transitionVerticalCollapse";
import { EntityType } from "../../../../SystemGuids";
import { updateRefValue } from "../../../../Util/util";
import EmailSource from "../Shared/emailSource";
import SettingsWell from "../Shared/settingsWell";
import { FormGeneral } from "../Shared/types";
import { FormTemplateListItem } from "./types";
import { useFormSources } from "./utils";

/**
 * Displays the UI for the General Settings section of the Settings screen.
 */
export default defineComponent({
    name: "Workflow.FormBuilderDetail.GeneralSettings",

    components: {
        Alert,
        CategoryPicker,
        CheckBox,
        DateTimePicker,
        DropDownList,
        EmailSource,
        InlineSwitch,
        SettingsWell,
        TextBox,
        TransitionVerticalCollapse
    },

    props: {
        modelValue: {
            type: Object as PropType<FormGeneral>,
            required: true
        },

        templateOverrides: {
            type: Object as PropType<FormTemplateListItem>
        }
    },

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        const sources = useFormSources();

        const name = ref(props.modelValue.name ?? "");
        const description = ref(props.modelValue.description ?? "");
        const template = ref(props.modelValue.template ?? "");
        const category = ref(props.modelValue.category ?? null);
        const entryStarts = ref(props.modelValue.entryStarts ?? "");
        const entryEnds = ref(props.modelValue.entryEnds ?? "");
        const isLoginRequired = ref(props.modelValue.isLoginRequired ?? false);

        const isLoginRequiredForced = computed((): boolean => props.templateOverrides?.isLoginRequiredConfigured ?? false);

        // Watch for changes in our modelValue and then update all our internal values.
        watch(() => props.modelValue, () => {
            updateRefValue(name, props.modelValue.name ?? "");
            updateRefValue(description, props.modelValue.description ?? "");
            updateRefValue(template, props.modelValue.template ?? "");
            updateRefValue(category, props.modelValue.category ?? null);
            updateRefValue(entryStarts, props.modelValue.entryStarts ?? "");
            updateRefValue(entryEnds, props.modelValue.entryEnds ?? "");
        });

        // Watch for changes on any of our internal values and then update the modelValue.
        watch([name, description, template, category, isLoginRequired, entryStarts, entryEnds], () => {
            const newValue: FormGeneral = {
                ...props.modelValue,
                name: name.value,
                description: description.value,
                template: template.value,
                category: category.value,
                isLoginRequired: isLoginRequired.value,
                entryStarts: entryStarts.value,
                entryEnds: entryEnds.value,
            };

            emit("update:modelValue", newValue);
        });

        return {
            category,
            description,
            entryStarts,
            entryEnds,
            isLoginRequired,
            isLoginRequiredForced,
            name,
            template,
            templateOptions: sources.formTemplateOptions,
            workflowTypeEntityTypeGuid: EntityType.WorkflowType
        };
    },

    template: `
<SettingsWell title="General Settings"
    description="Update the general settings for the form below.">
    <div class="row">
        <div class="col-md-6">
            <div>
                <TextBox v-model="name"
                    label="Form Name"
                    rules="required" />

                <TextBox v-model="description"
                    label="Description"
                    textMode="multiline" />

                <DropDownList v-model="template"
                    label="Template"
                    :options="templateOptions"
                    />

                <CategoryPicker v-model="category"
                    label="Category"
                    rules="required"
                    :entityTypeGuid="workflowTypeEntityTypeGuid" />
            </div>
        </div>
    </div>

    <CheckBox v-if="!isLoginRequiredForced" v-model="isLoginRequired"
        label="Is Login Required"
        help="Determines if a person needs to be logged in to complete the form." />

    <Alert v-else alertType="info">
        The template has enforced the login required setting.
    </Alert>

    <div class="row">
        <div class="col-md-6">
            <DateTimePicker v-model="entryStarts"
                label="Form Entry Starts" />
        </div>

        <div class="col-md-6">
            <DateTimePicker v-model="entryEnds"
                label="Form Entry Ends" />
        </div>
    </div>
</Settingswell>
`
});

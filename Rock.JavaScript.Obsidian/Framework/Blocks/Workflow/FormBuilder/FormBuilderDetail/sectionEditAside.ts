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
import ConfigurableZone from "./configurableZone";
import RockField from "../../../../Controls/rockField";
import DropDownList from "../../../../Elements/dropDownList";
import Panel from "../../../../Controls/panel";
import RockForm from "../../../../Controls/rockForm";
import Switch from "../../../../Elements/switch";
import TextBox from "../../../../Elements/textBox";
import { SectionAsideSettings } from "./types";
import { useFormSources } from "./utils";
import { confirmDelete } from "../../../../Util/dialogs";
import { FormError } from "../../../../Util/form";

export default defineComponent({
    name: "Workflow.FormBuilderDetail.SectionEditAside",

    components: {
        ConfigurableZone,
        DropDownList,
        Panel,
        RockField,
        RockForm,
        Switch,
        TextBox
    },

    props: {
        modelValue: {
            type: Object as PropType<SectionAsideSettings>,
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
        /** The title of the section that will be displayed. */
        const title = ref(props.modelValue.title);

        /** The descriptive text about the section that will be displayed. */
        const description = ref(props.modelValue.description);

        /** True if the heading separator should be shown in the section. */
        const showHeadingSeparator = ref(props.modelValue.showHeadingSeparator);

        /** The style of the UI to render for the section. */
        const sectionType = ref(props.modelValue.type ?? "");

        /** The validation errors for the form. */
        const validationErrors = ref<FormError[]>([]);

        /** True if the form should start to submit. */
        const formSubmit = ref(false);

        /** Used to temporarily disable emitting the modelValue when something changes. */
        let autoSyncModelValue = true;

        const sectionTypeOptions = useFormSources().sectionTypeOptions ?? [];

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

        // Watch for changes in the model value and update our internal values.
        watch(() => props.modelValue, () => {
            autoSyncModelValue = false;
            title.value = props.modelValue.title;
            description.value = props.modelValue.description;
            showHeadingSeparator.value = props.modelValue.showHeadingSeparator;
            sectionType.value = props.modelValue.type ?? "";
            autoSyncModelValue = true;
        });

        // Watch for changes in our internal values and update the modelValue.
        watch([title, description, showHeadingSeparator, sectionType], () => {
            if (!autoSyncModelValue) {
                return;
            }

            const value: SectionAsideSettings = {
                ...props.modelValue,
                title: title.value,
                description: description.value,
                showHeadingSeparator: showHeadingSeparator.value,
                type: sectionType.value === "" ? null : sectionType.value
            };

            emit("update:modelValue", value);
        });

        return {
            description,
            formSubmit,
            onBackClick,
            title,
            onValidationChanged,
            sectionType,
            sectionTypeOptions,
            showHeadingSeparator,
            validationErrors
        };
    },

    template: `
<div class="d-flex flex-column" style="overflow-y: hidden; flex-grow: 1;">
    <div class="d-flex">
        <div class="d-flex clickable" style="background-color: #484848; color: #fff; align-items: center; justify-content: center; width: 40px;" @click="onBackClick">
            <i class="fa fa-chevron-left"></i>
        </div>

        <div class="p-2 aside-header" style="flex-grow: 1;">
            <span class="title">Section</span>
        </div>
    </div>

    <div class="aside-body d-flex flex-column" style="flex-grow: 1; overflow-y: auto;">
        <RockForm v-model:submit="formSubmit" @validationChanged="onValidationChanged" class="d-flex flex-column" style="flex-grow: 1;">
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
                    :options="sectionTypeOptions" />
            </Panel>

            <Panel title="Conditionals" hasCollapse>
                TODO: Need to build this.
            </Panel>
        </RockForm>
    </div>
</div>
`
});

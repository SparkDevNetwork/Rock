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

import { defineComponent, PropType, ref } from "vue";
import Panel from "../../../../Controls/panel";
import RockForm from "../../../../Controls/rockForm";
import { useVModelPassthrough } from "../../../../Util/component";
import PersonEntrySettings from "./personEntrySettings";
import { FormPersonEntry } from "./types";
import { useFormSources } from "./utils";

export default defineComponent({
    name: "Workflow.FormBuilderDetail.PersonEntryEditAside",
    components: {
        Panel,
        PersonEntrySettings,
        RockForm
    },

    props: {
        modelValue: {
            type: Object as PropType<FormPersonEntry>,
            default: {}
        }
    },

    emits: [
        "update:modelValue",
        "close"
    ],

    methods: {
        /**
         * Checks if this aside is safe to close or if there are errors that
         * must be corrected first.
         */
        isSafeToClose(): boolean {
            this.formSubmit = true;

            return Object.keys(this.validationErrors).length === 0;
        }
    },

    setup(props, { emit }) {
        const internalValue = useVModelPassthrough(props, "modelValue", emit);

        /** The validation errors for the form. */
        const validationErrors = ref<Record<string, string>>({});

        /** True if the form should start to submit. */
        const formSubmit = ref(false);

        /**
         * Event handler for when the back button is clicked.
         */
        const onBackClick = (): void => emit("close");

        /**
         * Event handler for when the validation state of the form has changed.
         * 
         * @param errors Any errors that were detected on the form.
         */
        const onValidationChanged = (errors: Record<string, string>): void => {
            validationErrors.value = errors;
        };

        const options = useFormSources();

        return {
            addressTypeOptions: options.addressTypeOptions ?? [],
            campusStatusOptions: options.campusStatusOptions ?? [],
            campusTypeOptions: options.campusTypeOptions ?? [],
            connectionStatusOptions: options.connectionStatusOptions ?? [],
            recordStatusOptions: options.recordStatusOptions ?? [],
            formSubmit,
            internalValue,
            onBackClick,
            onValidationChanged,
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
            <i class="fa fa-user"></i>
            <span class="title">Person Entry</span>
        </div>
    </div>

    <div class="aside-body d-flex flex-column" style="flex-grow: 1; overflow-y: auto;">
        <RockForm v-model:submit="formSubmit" @validationChanged="onValidationChanged" class="d-flex flex-column" style="flex-grow: 1;">
            <PersonEntrySettings v-model="internalValue"
                isVertical
                :recordStatusOptions="recordStatusOptions"
                :connectionStatusOptions="connectionStatusOptions"
                :campusTypeOptions="campusTypeOptions"
                :campusStatusOptions="campusStatusOptions"
                :addressTypeOptions="addressTypeOptions" />
        </RockForm>
    </div>
</div>
`
});

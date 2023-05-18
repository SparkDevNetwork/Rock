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
import Panel from "@Obsidian/Controls/panel";
import RockForm from "@Obsidian/Controls/rockForm";
import { useVModelPassthrough } from "@Obsidian/Utility/component";
import { FormError } from "@Obsidian/Utility/form";
import PersonEntrySettings from "../Shared/personEntrySettings";
import { FormPersonEntry } from "../Shared/types.partial";
import { useFormSources } from "./utils.partial";

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
        "close",
        "validationChanged"
    ],

    methods: {
        /**
         * Checks if this aside is safe to close or if there are errors that
         * must be corrected first.
         */
        isSafeToClose(): boolean {
            this.formSubmit = true;

            const result = this.validationErrors.length === 0;

            // If there was an error, perform a smooth scroll to the top so
            // they can see the validation results.
            if (!result && this.scrollableElement) {
                this.scrollableElement.scroll({
                    behavior: "smooth",
                    top: 0
                });
            }

            return result;
        }
    },

    setup(props, { emit }) {
        const internalValue = useVModelPassthrough(props, "modelValue", emit);

        const validationErrors = ref<FormError[]>([]);
        const scrollableElement = ref<HTMLElement | null>(null);

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
        const onValidationChanged = (errors: FormError[]): void => {
            validationErrors.value = errors;
            emit("validationChanged", errors);
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
            scrollableElement,
            validationErrors
        };
    },

    template: `
<div class="form-sidebar">
    <div class="sidebar-header">
        <div class="sidebar-back" @click="onBackClick">
            <i class="fa fa-chevron-left"></i>
        </div>

        <span class="title">
            <i class="fa fa-fw fa-user icon"></i>
            Person Entry
        </span>
    </div>

    <div ref="scrollableElement" class="sidebar-body">
        <RockForm v-model:submit="formSubmit" @validationChanged="onValidationChanged" class="sidebar-panels">
            <div class="panel-body">
                <PersonEntrySettings v-model="internalValue"
                    isVertical
                    :recordStatusOptions="recordStatusOptions"
                    :connectionStatusOptions="connectionStatusOptions"
                    :campusTypeOptions="campusTypeOptions"
                    :campusStatusOptions="campusStatusOptions"
                    :addressTypeOptions="addressTypeOptions" />
            </div>
        </RockForm>
    </div>
</div>
`
});

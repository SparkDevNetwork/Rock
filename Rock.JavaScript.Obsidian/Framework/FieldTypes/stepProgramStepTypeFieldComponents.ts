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

import { defineComponent, ref, watch } from "vue";
import { getFieldEditorProps } from "./utils";
import StepProgramStepTypePicker from "@Obsidian/Controls/stepProgramStepTypePicker.obs";
import { StepProgramStepType } from "./stepProgramStepTypeField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";


export const EditComponent = defineComponent({
    name: "StepProgramStepTypeField.Edit",

    components: {
        StepProgramStepTypePicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref({} as StepProgramStepType);
        const stepProgram = ref({} as ListItemBag);
        const stepType = ref({} as ListItemBag);

        watch(() => props.modelValue, () => {
            if (props.modelValue) {
                internalValue.value = JSON.parse(props.modelValue || "{}");
                stepProgram.value = internalValue.value.stepProgram;
                stepType.value = internalValue.value.stepType;
            }
        }, { immediate: true });

        watch(() => [stepProgram.value, stepType.value], () => {
            const newValue = {
                stepProgram: stepProgram.value ?? null,
                stepType: stepType.value ?? null
            };
            emit("update:modelValue", JSON.stringify(newValue));
        }, { deep: true });

        return {
            stepProgram,
            stepType
        };
    },

    template: `
<StepProgramStepTypePicker v-model="stepType" v-model:stepProgram="stepProgram" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "StepProgramStepTypeField.Configuration",

    template: ``
});
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
import { defineComponent, ref, watch } from "vue";
import { getFieldEditorProps } from "./utils";
import SocialSecurityNumberBox from "@Obsidian/Controls/socialSecurityNumberBox.obs";

export const EditComponent = defineComponent({
    name: "SSNField.Edit",

    components: {
        SocialSecurityNumberBox
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref(props.modelValue);

        watch(() => props.modelValue, () => internalValue.value = props.modelValue ?? "");
        watch(internalValue, () => emit("update:modelValue", internalValue.value));

        return {
            internalValue
        };
    },

    template: `
<SocialSecurityNumberBox v-model="internalValue" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "SSNField.Configuration",

    template: ``
});

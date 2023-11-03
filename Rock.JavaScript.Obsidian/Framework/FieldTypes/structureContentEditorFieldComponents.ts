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
import StructuredContentEditor from "@Obsidian/Controls/structuredContentEditor.obs";

export const EditComponent = defineComponent({
    name: "StructuredContentEditorField.Edit",

    components: {
        StructuredContentEditor
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref(props.modelValue) ?? "{}";

        watch(() => props.modelValue, () => internalValue.value = props.modelValue ?? "{}");
        watch(internalValue, () => emit("update:modelValue", internalValue.value));

        return {
            internalValue
        };
    },

    template: `
<StructuredContentEditor v-model="internalValue" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "StructuredContentEditorField.Configuration",

    template: ``
});
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
import AddressControl from "../Controls/addressControl";
import { AddressFieldValue } from "./addressField";

export const EditComponent = defineComponent({
    name: "AddressField.Edit",

    components: {
        AddressControl
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const internalValue = ref({} as AddressFieldValue);

        watch(() => props.modelValue, () => {
            try {
                internalValue.value = JSON.parse(props.modelValue || "{}");
            }
            catch {
                internalValue.value = {};
            }
        }, { immediate: true });

        watch(() => internalValue.value, () => {
            emit("update:modelValue", JSON.stringify(internalValue.value));
        }, { deep: true });

        return {
            internalValue
        };
    },

    template: `
<AddressControl v-model="internalValue" />
`
});


export const ConfigurationComponent = defineComponent({
    name: "AddressField.Configuration",

    template: ``
});

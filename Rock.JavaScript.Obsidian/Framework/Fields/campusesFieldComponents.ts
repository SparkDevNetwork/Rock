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
import { computed, defineComponent, ref, SetupContext, watch, watchEffect } from "vue";
import { getFieldEditorProps } from "./utils";
import CheckBoxList from "../Elements/checkBoxList";
import { toNumberOrNull } from "../Services/number";
import { ConfigurationValueKey } from "./campusesField";
import { ListItem } from "../ViewModels";

export const EditComponent = defineComponent({
    name: "CampusesField.Edit",

    components: {
        CheckBoxList
    },

    props: getFieldEditorProps(),

    setup(props, context: SetupContext) {
        const internalValue = ref(props.modelValue ? props.modelValue.split(",") : []);

        /** The options to choose from in the drop down list */
        const options = computed((): ListItem[] => {
            try {
                return JSON.parse(props.configurationValues[ConfigurationValueKey.Values] ?? "[]") as ListItem[];
            }
            catch {
                return [];
            }
        });

        const repeatColumns = computed(() => {
            const repeatColumnsConfig = props.configurationValues[ConfigurationValueKey.RepeatColumns];

            return toNumberOrNull(repeatColumnsConfig) ?? 4;
        });

        watch(() => props.modelValue, () => internalValue.value = props.modelValue ? props.modelValue.split(",") : []);

        watchEffect(() => context.emit("update:modelValue", internalValue.value.join(",")));

        return {
            internalValue,
            options,
            repeatColumns
        };
    },

    template: `
<CheckBoxList v-model="internalValue" horizontal :options="options" :repeatColumns="repeatColumns" />
`
});

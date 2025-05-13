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
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ConfigurationValueKey } from "./streakTypeField.partial";
import { getFieldEditorProps } from "./utils";

export const EditComponent = defineComponent({
    name: "StreakTypeField.Edit",

    components: {
        DropDownList
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the text editor.
        const internalValue = ref<string>("");
        // The CommunicationTemplate options.
        const options = JSON.parse(props.configurationValues[ConfigurationValueKey.Values] ?? "[]") as ListItemBag[];

        // Watch for changes from the parent component and update the text editor.
        watch(() => props.modelValue, () => {
            internalValue.value = props.modelValue;
        }, {
            immediate: true
        });

        // Watch for changes from the text editor and update the parent component.
        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value);
        });

        return {
            internalValue,
            options
        };
    },

    template: `
        <DropDownList v-model="internalValue" :items="options" horizontal />
    `
});

export const ConfigurationComponent = defineComponent({
    name: "StreakTypeField.Configuration",
    template: `
`
});

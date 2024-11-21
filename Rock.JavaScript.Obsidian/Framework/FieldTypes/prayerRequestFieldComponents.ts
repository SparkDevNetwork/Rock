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
import TextBox from "@Obsidian/Controls/textBox.obs";

// We can't import the ConfigurationValueKey from textField.partial.ts
// because it causes a recursive import back to this file by way of
// the fieldType.ts import in textField.partial.ts.
export const enum ConfigurationValueKey {
    /** Contains "True" if the text field is designed for password entry. */
    IsPassword = "ispassword",

    /** The maximum number of characters allowed in the text entry field. */
    MaxCharacters = "maxcharacters",

    /** Contains "True" if the text field should show the character countdown. */
    ShowCountdown = "showcountdown",

    /** Contains "True" if the text field is designed for first name entry. */
    IsFirstName = "isfirstname",
}

export const EditComponent = defineComponent({
    name: "PrayerRequestField.Edit",

    components: {
        TextBox
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        // The internal value used by the text editor.
        const internalValue = ref("");

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
        };
    },

    template: `<TextBox v-model="internalValue" />`
});

export const ConfigurationComponent = defineComponent({
    name: "PrayerRequestField.Configuration",
    template: ``
});

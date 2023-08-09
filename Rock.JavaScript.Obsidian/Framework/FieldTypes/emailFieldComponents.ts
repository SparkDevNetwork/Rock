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
import { defineComponent } from "vue";
import { getFieldEditorProps } from "./utils";
import EmailBox from "@Obsidian/Controls/emailBox.obs";
import TextBox from "@Obsidian/Controls/textBox.obs";
import { useVModelPassthrough } from "@Obsidian/Utility/component";

export const EditComponent = defineComponent({
    name: "EmailField.Edit",

    components: {
        EmailBox
    },

    props: getFieldEditorProps(),

    data() {
        return {
            internalValue: ""
        };
    },

    watch: {
        internalValue() {
            this.$emit("update:modelValue", this.internalValue);
        },
        modelValue: {
            immediate: true,
            handler() {
                this.internalValue = this.modelValue || "";
            }
        }
    },
    template: `
<EmailBox v-model="internalValue" />
`
});

export const FilterComponent = defineComponent({
    name: "EmailField.Filter",

    components: {
        TextBox
    },

    props: getFieldEditorProps(),

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        const internalValue = useVModelPassthrough(props, "modelValue", emit);

        return {
            internalValue
        };
    },

    template: `
<TextBox v-model="internalValue" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "EmailField.Configuration",

    template: ``
});

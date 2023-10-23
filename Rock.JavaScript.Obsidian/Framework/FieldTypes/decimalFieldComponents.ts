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
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import NumberBox from "@Obsidian/Controls/numberBox.obs";

export const EditComponent = defineComponent({
    name: "DecimalField.Edit",

    components: {
        NumberBox
    },

    props: getFieldEditorProps(),

    data() {
        return {
            /** The user input value as a number of null if it isn't valid. */
            internalValue: null as number | null
        };
    },

    watch: {
        /**
         * Watch for changes to internalValue and emit the new value out to
         * the consuming component.
         */
        internalValue(): void {
            this.$emit("update:modelValue", this.internalValue !== null ? this.internalValue.toString() : "");
        },

        /**
         * Watch for changes to modelValue which indicate the component
         * using us has given us a new value to work with.
         */
        modelValue: {
            immediate: true,
            handler(): void {
                this.internalValue = toNumberOrNull(this.modelValue || "");
            }
        }
    },

    template: `
<NumberBox v-model="internalValue" rules="decimal" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "DecimalField.Configuration",

    template: ``
});

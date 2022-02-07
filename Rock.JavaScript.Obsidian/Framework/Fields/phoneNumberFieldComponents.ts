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
import PhoneNumberBox from "../Elements/phoneNumberBox";

export const EditComponent = defineComponent({
    name: "PhoneNumberField.Edit",

    components: {
        PhoneNumberBox
    },

    props: getFieldEditorProps(),

    data() {
        return {
            internalValue: ""
        };
    },

    computed: {
        configAttributes(): Record<string, number | boolean> {
            const attributes: Record<string, number | boolean> = {};
            return attributes;
        }
    },

    watch: {
        internalValue(): void {
            this.$emit("update:modelValue", this.internalValue);
        },
        modelValue: {
            immediate: true,
            handler(): void {
                this.internalValue = this.modelValue || "";
            }
        }
    },

    template: `
<PhoneNumberBox v-model="internalValue" v-bind="configAttributes" />
`
});

export const ConfigurationComponent = defineComponent({
    name: "PhoneNumberField.Configuration",

    template: ``
});

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
import DropDownList from "../Elements/dropDownList";
import { ListItem } from "../ViewModels";

const enum ConfigurationValueKey {
    HideUnknownGender = "hideUnknownGender"
}

export const EditComponent = defineComponent({

    name: "GenderField.Edit",

    components: {
        DropDownList
    },

    props: getFieldEditorProps(),

    data() {
        
        return {
            internalValue: ""
        };
    },

    computed: {
        dropDownListOptions(): ListItem[] {
            const hideUnknownGenderConfig = this.configurationValues[ConfigurationValueKey.HideUnknownGender];
            const hideUnknownGender = hideUnknownGenderConfig.toLowerCase() === "true";

            if (hideUnknownGender === false) {
                return [
                    { text: "Unknown", value: "0" },
                    { text: "Male", value: "1" },
                    { text: "Female", value: "2" }
                ] as ListItem[];
                }
            else {
                return [
                    { text: "Male", value: "1" },
                    { text: "Female", value: "2" }
                ] as ListItem[];
            }
        }
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
<DropDownList v-model="internalValue" :options="dropDownListOptions" formControlClasses="input-width-md" />
`
});

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
import DateTimePicker from "../Elements/dateTimePicker";
import { asBoolean } from "../Services/boolean";
import { ConfigurationValueKey } from "./dateTimeField";

export const EditComponent = defineComponent({
    name: "DateTimeField.Edit",

    components: {
        DateTimePicker
    },

    props: getFieldEditorProps(),

    setup() {
        return {
        };
    },

    data() {
        return {
            internalValue: "",
            formattedString: ""
        };
    },

    methods: {
        async syncModelValue(): Promise<void> {
            this.internalValue = this.modelValue ?? "";
        },
    },

    computed: {
        dateFormatTemplate(): string {
            const formatConfig = this.configurationValues[ConfigurationValueKey.Format];
            return formatConfig || "MM/dd/yyyy";
        },

        configAttributes(): Record<string, number | boolean> {
            const attributes: Record<string, number | boolean> = {};

            const displayCurrentConfig = this.configurationValues[ConfigurationValueKey.DisplayCurrentOption];
            const displayCurrent = asBoolean(displayCurrentConfig);
            attributes.displayCurrentOption = displayCurrent;
            attributes.isCurrentDateOffset = displayCurrent;

            return attributes;
        }
    },

    watch: {
        internalValue(): void {
            if (this.internalValue !== this.modelValue) {
                const d1 = Date.parse(this.internalValue);
                const d2 = Date.parse(this.modelValue ?? "");

                if (isNaN(d1) || isNaN(d2) || d1 !== d2) {
                    this.$emit("update:modelValue", this.internalValue);
                }
            }
        },

        modelValue: {
            immediate: true,
            async handler(): Promise<void> {
                await this.syncModelValue();
            }
        }
    },
    template: `
<DateTimePicker v-model="internalValue" v-bind="configAttributes" />
`
});

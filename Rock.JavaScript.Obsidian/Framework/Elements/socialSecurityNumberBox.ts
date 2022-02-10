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

import { defineComponent, PropType } from "vue";
import { normalizeRules, rulesPropType, ValidationRule } from "../Rules/index";
import RockFormField from "./rockFormField";

export default defineComponent({
    name: "SocialSecurityNumberBox",
    components: {
        RockFormField
    },
    props: {
        rules: rulesPropType,
        modelValue: {
            type: String as PropType<string>,
            default: ""
        }
    },

    data() {
        return {
            internalArea: "",
            internalGroup: "",
            internalSerial: "",
            internalValue: ""
        };
    },

    methods: {
        getValue(): string {
            const value = `${this.internalArea}${this.internalGroup}${this.internalSerial}`;

            return value;
        },

        keyPress(e: KeyboardEvent): boolean {
            if (/^[0-9]$/.test(e.key) === false) {
                e.preventDefault();
                return false;
            }

            return true;
        },

        keyUp(e: KeyboardEvent): boolean {
            const area = <HTMLInputElement>this.$refs.area;
            const group = <HTMLInputElement>this.$refs.group;
            const serial = <HTMLInputElement>this.$refs.serial;

            // Only move to next field if a number was pressed.
            if (/^[0-9]$/.test(e.key) === false) {
                return true;
            }

            if (area === e.target && area.selectionStart === 3) {
                this.$nextTick(() => {
                    group.focus();
                    group.setSelectionRange(0, 2);
                });
            }
            else if (group === e.target && group.selectionStart === 2) {
                this.$nextTick(() => {
                    serial.focus();
                    serial.setSelectionRange(0, 4);
                });
            }

            return true;
        }
    },

    computed: {
        computedRules(): ValidationRule[] {
            const rules = normalizeRules(this.rules);

            rules.push("ssn");

            return rules;
        }
    },

    watch: {
        modelValue: {
            immediate: true,
            handler() {
                const strippedValue = this.modelValue.replace(/[^0-9]/g, "");

                if (strippedValue.length !== 9) {
                    this.internalArea = "";
                    this.internalGroup = "";
                    this.internalSerial = "";
                }
                else {
                    this.internalArea = strippedValue.substr(0, 3);
                    this.internalGroup = strippedValue.substr(3, 2);
                    this.internalSerial = strippedValue.substr(5, 4);
                }

                this.internalValue = this.getValue();
            }
        },

        internalArea() {
            this.internalValue = this.getValue();

            if (this.internalValue.length === 0 || this.internalValue.length === 9) {
                this.$emit("update:modelValue", this.internalValue);
            }
        },

        internalGroup() {
            this.internalValue = this.getValue();

            if (this.internalValue.length === 0 || this.internalValue.length === 9) {
                this.$emit("update:modelValue", this.internalValue);
            }
        },

        internalSerial() {
            this.internalValue = this.getValue();

            if (this.internalValue.length === 0 || this.internalValue.length === 9) {
                this.$emit("update:modelValue", this.internalValue);
            }
        },
    },

    template: `
<RockFormField
    :modelValue="internalValue"
    formGroupClasses="social-security-number-box"
    name="social-security-number-box"
    :rules="computedRules">
    <template #default="{uniqueId, field}">
        <div class="control-wrapper">
            <div class="form-control-group">
                <input ref="area" class="form-control ssn-part ssn-area" type="password" pattern="[0-9]*" maxlength="3" v-model="internalArea" v-on:keypress="keyPress" v-on:keyup="keyUp" />
                <span class="separator">-</span>
                <input ref="group" class="form-control ssn-part ssn-group" type="password" pattern="[0-9]*" maxlength="2" v-model="internalGroup" v-on:keypress="keyPress" v-on:keyup="keyUp" />
                <span class="separator">-</span>
                <input ref="serial" class="form-control ssn-part ssn-serial" type="text" pattern="[0-9]*" maxlength="4" v-model="internalSerial" v-on:keypress="keyPress" v-on:keyup="keyUp" />
            </div>
        </div>
    </template>
</RockFormField>`
});

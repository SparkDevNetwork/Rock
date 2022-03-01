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

import { defineComponent, PropType } from "vue";
import Alert from "../../../Elements/alert";
import CheckBox from "../../../Elements/checkBox";
import DropDownList from "../../../Elements/dropDownList";
import NumberUpDown from "../../../Elements/numberUpDown";
import NumberUpDownGroup, { NumberUpDownGroupOption } from "../../../Elements/numberUpDownGroup";
import Number from "../../../Services/number";
import GuidHelper, { Guid } from "../../../Util/guid";
import { ListItem } from "../../../ViewModels";
import { RegistrationEntryBlockFeeViewModel, RegistrationEntryBlockFeeItemViewModel } from "./registrationEntryBlockViewModel";

export default defineComponent({
    name: "Event.RegistrationEntry.FeeField",
    components: {
        NumberUpDown,
        NumberUpDownGroup,
        DropDownList,
        CheckBox,
        Alert
    },
    props: {
        modelValue: {
            type: Object as PropType<Record<Guid, number>>,
            required: true
        },
        fee: {
            type: Object as PropType<RegistrationEntryBlockFeeViewModel>,
            required: true
        }
    },
    data() {
        return {
            dropDownValue: "",
            checkboxValue: false
        };
    },
    methods: {
        getItemLabel(item: RegistrationEntryBlockFeeItemViewModel): string {
            const formattedCost = Number.asFormattedString(item.cost, 2);

            if (item.countRemaining) {
                const formattedRemaining = Number.asFormattedString(item.countRemaining, 0);
                return `${item.name} ($${formattedCost}) (${formattedRemaining} remaining)`;
            }

            return `${item.name} ($${formattedCost})`;
        }
    },
    computed: {
        label(): string {
            if (this.singleItem) {
                return this.getItemLabel(this.singleItem);
            }

            return this.fee.name;
        },
        singleItem(): RegistrationEntryBlockFeeItemViewModel | null {
            if (this.fee.items.length !== 1) {
                return null;
            }

            return this.fee.items[ 0 ];
        },
        isHidden(): boolean {
            return !this.fee.items.length;
        },
        isCheckbox(): boolean {
            return !!this.singleItem && !this.fee.allowMultiple;
        },
        isNumberUpDown(): boolean {
            return !!this.singleItem && this.fee.allowMultiple;
        },
        isNumberUpDownGroup(): boolean {
            return this.fee.items.length > 1 && this.fee.allowMultiple;
        },
        isDropDown(): boolean {
            return this.fee.items.length > 1 && !this.fee.allowMultiple;
        },
        dropDownListOptions(): ListItem[] {
            return this.fee.items.map(i => ({
                text: this.getItemLabel(i),
                value: i.guid
            }));
        },
        numberUpDownGroupOptions(): NumberUpDownGroupOption[] {
            return this.fee.items.map(i => ({
                key: i.guid,
                label: this.getItemLabel(i),
                max: i.countRemaining || 100,
                min: 0
            }));
        },
        rules(): string {
            return this.fee.isRequired ? "required" : "";
        }
    },
    watch: {
        modelValue: {
            immediate: true,
            deep: true,
            handler(): void {
                // Set the drop down selecton
                if (this.isDropDown) {
                    this.dropDownValue = "";

                    for (const item of this.fee.items) {
                        if (!this.dropDownValue && this.modelValue[item.guid]) {
                            // Pick the first option that has a qty
                            this.modelValue[item.guid] = 1;
                            this.dropDownValue = item.guid;
                        }
                        else if (this.modelValue[item.guid]) {
                            // Any other quantities need to be zeroed since only one can be picked
                            this.modelValue[item.guid] = 0;
                        }
                    }
                }

                // Set the checkbox selection
                if (this.isCheckbox && this.singleItem) {
                    this.checkboxValue = !!this.modelValue[this.singleItem.guid];
                    this.modelValue[this.singleItem.guid] = this.checkboxValue ? 1 : 0;
                }
            }
        },
        fee: {
            immediate: true,
            handler(): void {
                for (const item of this.fee.items) {
                    this.modelValue[item.guid] = this.modelValue[item.guid] || 0;
                }
            }
        },
        dropDownValue(): void {
            // Drop down implies a quantity of 1. Zero out all items except for the one selected.
            for (const item of this.fee.items) {
                const isSelected = GuidHelper.areEqual(this.dropDownValue, item.guid);
                this.modelValue[item.guid] = isSelected ? 1 : 0;
            }
        },
        checkboxValue(): void {
            if (this.singleItem) {
                // Drop down implies a quantity of 1.
                this.modelValue[this.singleItem.guid] = this.checkboxValue ? 1 : 0;
            }
        }
    },
    template: `
<template v-if="!isHidden">
    <CheckBox v-if="isCheckbox" :label="label" v-model="checkboxValue" :rules="rules" />
    <NumberUpDown v-else-if="isNumberUpDown" :label="label" :min="0" :max="singleItem.countRemaining || 100" v-model="modelValue[singleItem.guid]" :rules="rules" />
    <DropDownList v-else-if="isDropDown" :label="label" :options="dropDownListOptions" v-model="dropDownValue" :rules="rules" formControlClasses="input-width-md" />
    <NumberUpDownGroup v-else-if="isNumberUpDownGroup" :label="label" :options="numberUpDownGroupOptions" v-model="modelValue" :rules="rules" />
    <Alert v-else alertType="danger">This fee configuration is not supported</Alert>
</template>`
});

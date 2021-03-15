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
import { defineComponent } from 'vue';
import { Guid } from '../Util/Guid';
import { getFieldTypeProps, registerFieldType } from './Index';
import { asYesNoOrNull, asTrueFalseOrNull, asBoolean } from '../Services/Boolean';
import DropDownList, { DropDownListOption } from '../Elements/DropDownList';
import Toggle from '../Elements/Toggle';

const fieldTypeGuid: Guid = '1EDAFDED-DFE6-4334-B019-6EECBA89E05A';

enum BooleanControlType {
    DropDown,
    Checkbox,
    Toggle
}

enum ConfigurationValueKey {
    BooleanControlType = 'BooleanControlType',
    FalseText = 'falsetext',
    TrueText = 'truetext'
}

export default registerFieldType(fieldTypeGuid, defineComponent({
    name: 'BooleanField',
    components: {
        DropDownList,
        Toggle
    },
    props: getFieldTypeProps(),
    data() {
        return {
            internalBooleanValue: false,
            internalValue: ''
        };
    },
    computed: {
        booleanControlType(): BooleanControlType {
            const controlTypeConfig = this.configurationValues[ConfigurationValueKey.BooleanControlType];
            switch (controlTypeConfig?.Value) {
                case '1':
                    return BooleanControlType.Checkbox;
                case '2':
                    return BooleanControlType.Toggle;
                default:
                    return BooleanControlType.DropDown;
            }
        },
        trueText(): string {
            let trueText = asYesNoOrNull(true);
            const trueConfig = this.configurationValues[ConfigurationValueKey.TrueText];

            if (trueConfig && trueConfig.Value) {
                trueText = trueConfig.Value;
            }

            return trueText || 'Yes';
        },
        falseText(): string {
            let falseText = asYesNoOrNull(false);
            const falseConfig = this.configurationValues[ConfigurationValueKey.FalseText];

            if (falseConfig && falseConfig.Value) {
                falseText = falseConfig.Value;
            }

            return falseText || 'No';
        },
        isToggle(): boolean {
            return this.booleanControlType === BooleanControlType.Toggle;
        },
        valueAsYesNoOrNull(): string | null {
            return asYesNoOrNull(this.modelValue);
        },
        toggleOptions(): Record<string, unknown> {
            return {
                trueText: this.trueText,
                falseText: this.falseText
            };
        },
        dropDownListOptions(): DropDownListOption[] {
            const trueVal = asTrueFalseOrNull(true);
            const falseVal = asTrueFalseOrNull(false);

            return [
                { key: falseVal, text: this.falseText, value: falseVal },
                { key: trueVal, text: this.trueText, value: trueVal }
            ] as DropDownListOption[];
        }
    },
    watch: {
        internalValue() {
            this.$emit('update:modelValue', this.internalValue);
        },
        internalBooleanValue() {
            const valueToEmit = asTrueFalseOrNull(this.internalBooleanValue) || '';
            this.$emit('update:modelValue', valueToEmit);
        },
        modelValue: {
            immediate: true,
            handler() {
                this.internalValue = asTrueFalseOrNull(this.modelValue) || '';
                this.internalBooleanValue = asBoolean(this.modelValue);
            }
        }
    },
    template: `
<Toggle v-if="isEditMode && isToggle" v-model="internalBooleanValue" v-bind="toggleOptions" />
<DropDownList v-else-if="isEditMode" v-model="internalValue" :options="dropDownListOptions" />
<span v-else>{{ valueAsYesNoOrNull }}</span>`
}));

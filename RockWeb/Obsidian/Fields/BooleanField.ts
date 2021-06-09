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
import { getConfigurationValue, getFieldTypeProps, registerFieldType } from './Index';
import { asYesNoOrNull, asTrueFalseOrNull, asBoolean, asBooleanOrNull } from '../Services/Boolean';
import DropDownList, { DropDownListOption } from '../Elements/DropDownList';
import Toggle from '../Elements/Toggle';
import CheckBox from '../Elements/CheckBox';

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

export default registerFieldType( fieldTypeGuid, defineComponent( {
    name: 'BooleanField',
    components: {
        DropDownList,
        Toggle,
        CheckBox
    },
    props: getFieldTypeProps(),
    data ()
    {
        return {
            internalBooleanValue: false,
            internalValue: ''
        };
    },
    computed: {
        booleanControlType (): BooleanControlType
        {
            const controlType = getConfigurationValue( ConfigurationValueKey.BooleanControlType, this.configurationValues );

            switch ( controlType )
            {
                case '1':
                    return BooleanControlType.Checkbox;
                case '2':
                    return BooleanControlType.Toggle;
                default:
                    return BooleanControlType.DropDown;
            }
        },
        trueText (): string
        {
            let trueText = asYesNoOrNull( true );
            const trueConfig = getConfigurationValue( ConfigurationValueKey.TrueText, this.configurationValues );

            if ( trueConfig )
            {
                trueText = trueConfig;
            }

            return trueText || 'Yes';
        },
        falseText (): string
        {
            let falseText = asYesNoOrNull( false );
            const falseConfig = getConfigurationValue( ConfigurationValueKey.FalseText, this.configurationValues );

            if ( falseConfig )
            {
                falseText = falseConfig;
            }

            return falseText || 'No';
        },
        isToggle (): boolean
        {
            return this.booleanControlType === BooleanControlType.Toggle;
        },
        isCheckBox (): boolean
        {
            return this.booleanControlType === BooleanControlType.Checkbox;
        },
        valueAsBooleanOrNull (): boolean | null
        {
            return asBooleanOrNull( this.modelValue );
        },
        displayValue (): string
        {
            if ( this.valueAsBooleanOrNull === null )
            {
                return '';
            }

            if ( this.valueAsBooleanOrNull )
            {
                return this.trueText;
            }

            return this.falseText;
        },
        toggleOptions (): Record<string, unknown>
        {
            return {
                trueText: this.trueText,
                falseText: this.falseText
            };
        },
        dropDownListOptions (): DropDownListOption[]
        {
            const trueVal = asTrueFalseOrNull( true );
            const falseVal = asTrueFalseOrNull( false );

            return [
                { key: falseVal, text: this.falseText, value: falseVal },
                { key: trueVal, text: this.trueText, value: trueVal }
            ] as DropDownListOption[];
        }
    },
    watch: {
        internalValue ()
        {
            this.$emit( 'update:modelValue', this.internalValue );
        },
        internalBooleanValue ()
        {
            const valueToEmit = asTrueFalseOrNull( this.internalBooleanValue ) || '';
            this.$emit( 'update:modelValue', valueToEmit );
        },
        modelValue: {
            immediate: true,
            handler ()
            {
                this.internalValue = asTrueFalseOrNull( this.modelValue ) || '';
                this.internalBooleanValue = asBoolean( this.modelValue );
            }
        }
    },
    template: `
<Toggle v-if="isEditMode && isToggle" v-model="internalBooleanValue" v-bind="toggleOptions" />
<CheckBox v-else-if="isEditMode && isCheckBox" v-model="internalBooleanValue" :inline="false" />
<DropDownList v-else-if="isEditMode" v-model="internalValue" :options="dropDownListOptions" />
<span v-else>{{ displayValue }}</span>`
} ) );

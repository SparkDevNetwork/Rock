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

import { defineComponent, PropType } from 'vue';
import Alert from '../../../Elements/Alert';
import CheckBox from '../../../Elements/CheckBox';
import DropDownList, { DropDownListOption } from '../../../Elements/DropDownList';
import NumberUpDown from '../../../Elements/NumberUpDown';
import NumberUpDownGroup, { NumberUpDownGroupOption } from '../../../Elements/NumberUpDownGroup';
import Number from '../../../Services/Number';
import GuidHelper, { Guid } from '../../../Util/Guid';
import { RegistrationEntryBlockFeeViewModel, RegistrationEntryBlockFeeItemViewModel } from './RegistrationEntryBlockViewModel';

export default defineComponent( {
    name: 'Event.RegistrationEntry.FeeField',
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
    data()
    {
        return {
            dropDownValue: '',
            checkboxValue: false
        };
    },
    methods: {
        getItemLabel( item: RegistrationEntryBlockFeeItemViewModel )
        {
            const formattedCost = Number.asFormattedString( item.Cost );

            if ( item.CountRemaining )
            {
                const formattedRemaining = Number.asFormattedString( item.CountRemaining, 0 );
                return `${item.Name} ($${formattedCost}) (${formattedRemaining} remaining)`;
            }

            return `${item.Name} ($${formattedCost})`;
        }
    },
    computed: {
        label(): string
        {
            if ( this.singleItem )
            {
                const formattedCost = Number.asFormattedString( this.singleItem.Cost );
                return `${this.fee.Name} ($${formattedCost})`;
            }

            return this.fee.Name;
        },
        singleItem(): RegistrationEntryBlockFeeItemViewModel | null
        {
            if ( this.fee.Items.length !== 1 )
            {
                return null;
            }

            return this.fee.Items[ 0 ];
        },
        isHidden(): boolean
        {
            return !this.fee.Items.length;
        },
        isCheckbox(): boolean
        {
            return !!this.singleItem && !this.fee.AllowMultiple;
        },
        isNumberUpDown(): boolean
        {
            return !!this.singleItem && this.fee.AllowMultiple;
        },
        isNumberUpDownGroup(): boolean
        {
            return this.fee.Items.length > 1 && this.fee.AllowMultiple;
        },
        isDropDown(): boolean
        {
            return this.fee.Items.length > 1 && !this.fee.AllowMultiple;
        },
        dropDownListOptions(): DropDownListOption[]
        {
            return this.fee.Items.map( i => ( {
                key: i.Guid,
                text: this.getItemLabel( i ),
                value: i.Guid
            } ) );
        },
        numberUpDownGroupOptions(): NumberUpDownGroupOption[]
        {
            return this.fee.Items.map( i => ( {
                key: i.Guid,
                label: this.getItemLabel( i ),
                max: i.CountRemaining || 100,
                min: 0
            } ) );
        },
        rules(): string
        {
            return this.fee.IsRequired ? 'required' : '';
        }
    },
    watch: {
        modelValue: {
            immediate: true,
            deep: true,
            handler()
            {
                // Set the drop down selecton
                if ( this.isDropDown )
                {
                    this.dropDownValue = '';

                    for ( const item of this.fee.Items )
                    {
                        if ( !this.dropDownValue && this.modelValue[ item.Guid ] )
                        {
                            // Pick the first option that has a qty
                            this.modelValue[ item.Guid ] = 1;
                            this.dropDownValue = item.Guid;
                        }
                        else if ( this.modelValue[ item.Guid ] )
                        {
                            // Any other quantities need to be zeroed since only one can be picked
                            this.modelValue[ item.Guid ] = 0;
                        }
                    }
                }

                // Set the checkbox selection
                if ( this.isCheckbox && this.singleItem )
                {
                    this.checkboxValue = !!this.modelValue[ this.singleItem.Guid ];
                    this.modelValue[ this.singleItem.Guid ] = this.checkboxValue ? 1 : 0;
                }
            }
        },
        fee: {
            immediate: true,
            handler()
            {
                for ( const item of this.fee.Items )
                {
                    this.modelValue[ item.Guid ] = this.modelValue[ item.Guid ] || 0;
                }
            }
        },
        dropDownValue()
        {
            // Drop down implies a quantity of 1. Zero out all items except for the one selected.
            for ( const item of this.fee.Items )
            {
                const isSelected = GuidHelper.areEqual( this.dropDownValue, item.Guid );
                this.modelValue[ item.Guid ] = isSelected ? 1 : 0;
            }
        },
        checkboxValue()
        {
            if ( this.singleItem )
            {
                // Drop down implies a quantity of 1.
                this.modelValue[ this.singleItem.Guid ] = this.checkboxValue ? 1 : 0;
            }
        }
    },
    template: `
<template v-if="!isHidden">
    <CheckBox v-if="isCheckbox" :label="label" v-model="checkboxValue" :inline="false" :rules="rules" />
    <NumberUpDown v-else-if="isNumberUpDown" :label="label" :min="0" :max="singleItem.CountRemaining || 100" v-model="modelValue[singleItem.Guid]" :rules="rules" />
    <DropDownList v-else-if="isDropDown" :label="label" :options="dropDownListOptions" v-model="dropDownValue" :rules="rules" formControlClasses="input-width-md" />
    <NumberUpDownGroup v-else-if="isNumberUpDownGroup" :label="label" :options="numberUpDownGroupOptions" v-model="modelValue" :rules="rules" />
    <Alert v-else alertType="danger">This fee configuration is not supported</Alert>
</template>`
} );
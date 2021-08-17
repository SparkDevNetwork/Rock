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
import { toNumber } from '../Services/Number';
import RockDate, { RockDateType } from '../Util/RockDate';
import RockFormField from './RockFormField';
import TextBox from './TextBox';

type Rock = {
    controls: {
        datePicker: {
            initialize: ( args: Record<string, unknown> ) => void;
        }
    }
};

export default defineComponent( {
    name: 'DatePicker',
    components: {
        RockFormField,
        TextBox
    },
    props: {
        modelValue: {
            type: String as PropType<RockDateType | null>,
            default: null
        },
        displayCurrentOption: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        isCurrentDateOffset: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },
    emits: [
        'update:modelValue'
    ],
    data: function ()
    {
        return {
            internalValue: null as string | null,
            isCurrent: false,
            currentDiff: '0'
        };
    },
    computed: {
        asRockDateOrNull (): RockDateType | null
        {
            return this.internalValue ? RockDate.toRockDate( new Date( this.internalValue ) ) : null;
        },

        asCurrentDateValue (): string
        {
            const plusMinus = `${toNumber( this.currentDiff )}`
            return `CURRENT:${plusMinus}`;
        },

        valueToEmit (): string | RockDateType | null
        {
            if ( this.isCurrent )
            {
                return this.asCurrentDateValue;
            }

            return this.asRockDateOrNull;
        }
    },
    watch: {
        isCurrentDateOffset: {
            immediate: true,
            handler ()
            {
                if ( !this.isCurrentDateOffset )
                {
                    this.currentDiff = '0';
                }
            }
        },
        isCurrent: {
            immediate: true,
            handler ()
            {
                if ( this.isCurrent )
                {
                    this.internalValue = 'Current';
                }
            }
        },
        valueToEmit ()
        {
            this.$emit( 'update:modelValue', this.valueToEmit );
        },
        modelValue: {
            immediate: true,
            handler ()
            {
                if ( !this.modelValue )
                {
                    this.internalValue = null;
                    this.isCurrent = false;
                    this.currentDiff = '0';
                    return;
                }

                if ( this.modelValue.indexOf( 'CURRENT' ) === 0 )
                {
                    this.isCurrent = true;
                    const parts = this.modelValue.split( ':' );

                    if ( parts.length === 2 )
                    {
                        this.currentDiff = `${toNumber( parts[ 1 ] )}`;
                    }

                    return;
                }

                const month = RockDate.getMonth( this.modelValue );
                const day = RockDate.getDay( this.modelValue );
                const year = RockDate.getYear( this.modelValue );
                this.internalValue = `${month}/${day}/${year}`;
            }
        }
    },
    mounted ()
    {
        const input = this.$refs[ 'input' ] as HTMLInputElement;
        const inputId = input.id;
        const Rock = ( window[ 'Rock' ] as unknown ) as Rock;

        Rock.controls.datePicker.initialize( {
            id: inputId,
            startView: 0,
            showOnFocus: true,
            format: 'mm/dd/yyyy',
            todayHighlight: true,
            forceParse: true,
            onChangeScript: () =>
            {
                if ( !this.isCurrent )
                {
                    this.internalValue = input.value;
                }
            }
        } );
    },
    template: `
<RockFormField formGroupClasses="date-picker" #default="{uniqueId}" name="datepicker" v-model.lazy="internalValue">
    <div class="control-wrapper">
        <div class="form-control-group">
            <div class="form-row">
                <div class="input-group input-width-md js-date-picker date">
                    <input ref="input" type="text" :id="uniqueId" class="form-control" v-model.lazy="internalValue" :disabled="isCurrent" />
                    <span class="input-group-addon">
                        <i class="fa fa-calendar"></i>
                    </span>
                </div>
                <div v-if="displayCurrentOption || isCurrent" class="input-group">
                    <div class="checkbox">
                        <label title="">
                        <input type="checkbox" v-model="isCurrent" />
                        <span class="label-text">Current Date</span></label>
                    </div>
                </div>
            </div>
            <div v-if="isCurrent && isCurrentDateOffset" class="form-row">
                <TextBox label="+- Days" v-model="currentDiff" inputClasses="input-width-md" help="Enter the number of days after the current date to use as the date. Use a negative number to specify days before." />
            </div>
        </div>
    </div>
</RockFormField>`
} );

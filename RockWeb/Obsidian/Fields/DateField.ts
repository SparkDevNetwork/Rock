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
import { defineComponent, inject } from 'vue';
import { Guid } from '../Util/Guid';
import { getFieldTypeProps, registerFieldType } from './Index';
import DatePicker from '../Elements/DatePicker';
import { BlockHttp, BlockHttpGet } from '../Controls/RockBlock';
import { asDateOrNull, asElapsedString, toRockDateOrNull } from '../Services/Date';
import { asBoolean } from '../Services/Boolean';
import { toNumber } from '../Services/Number';
import DatePartsPicker, { getDefaultDatePartsPickerModel } from '../Elements/DatePartsPicker';
import { toRockDate } from '../Util/RockDate';

const fieldTypeGuid: Guid = '6B6AA175-4758-453F-8D83-FCD8044B5F36';

enum ConfigurationValueKey
{
    Format = 'format',
    DisplayDiff = 'displayDiff',
    DisplayCurrentOption = 'displayCurrentOption',
    DatePickerControlType = 'datePickerControlType',
    FutureYearCount = 'futureYearCount'
}

export default registerFieldType( fieldTypeGuid, defineComponent( {
    name: 'DateField',
    components: {
        DatePicker,
        DatePartsPicker
    },
    props: getFieldTypeProps(),
    data ()
    {
        return {
            internalValue: '',
            internalDateParts: getDefaultDatePartsPickerModel(),
            formattedString: ''
        };
    },
    setup ()
    {
        return {
            http: inject( 'http' ) as BlockHttp
        };
    },
    computed: {
        datePartsAsDate (): Date | null
        {
            if ( !this.internalDateParts?.Day || !this.internalDateParts.Month || !this.internalDateParts.Year )
            {
                return null;
            }

            return new Date( this.internalDateParts.Year, this.internalDateParts.Month - 1, this.internalDateParts.Day ) || null;
        },

        isDatePartsPicker (): boolean
        {
            const config = this.configurationValues[ ConfigurationValueKey.DatePickerControlType ];
            return config?.Value?.toLowerCase() === 'date parts picker';
        },

        isCurrentDateValue (): boolean
        {
            return this.internalValue.indexOf( 'CURRENT' ) === 0;
        },

        asDate (): Date | null
        {
            return asDateOrNull( this.internalValue );
        },
        dateFormatTemplate (): string
        {
            const formatConfig = this.configurationValues[ ConfigurationValueKey.Format ];
            return formatConfig?.Value || 'MM/dd/yyyy'
        },
        elapsedString (): string
        {
            const dateValue = this.isDatePartsPicker ? this.datePartsAsDate : this.asDate;

            if ( this.isCurrentDateValue || !dateValue )
            {
                return '';
            }

            const formatConfig = this.configurationValues[ ConfigurationValueKey.DisplayDiff ];
            const displayDiff = asBoolean( formatConfig?.Value );

            if ( !displayDiff )
            {
                return '';
            }

            return asElapsedString( dateValue );
        },
        configAttributes (): Record<string, number | boolean>
        {
            const attributes: Record<string, number | boolean> = {};

            const displayCurrentConfig = this.configurationValues[ ConfigurationValueKey.DisplayCurrentOption ];
            if ( displayCurrentConfig?.Value )
            {
                const displayCurrent = asBoolean( displayCurrentConfig.Value );
                attributes.displayCurrentOption = displayCurrent;
                attributes.isCurrentDateOffset = displayCurrent;
            }

            const futureYearConfig = this.configurationValues[ ConfigurationValueKey.FutureYearCount ];
            if ( futureYearConfig?.Value )
            {
                const futureYears = toNumber( futureYearConfig.Value );

                if ( futureYears > 0 )
                {
                    attributes.futureYearCount = futureYears;
                }
            }

            return attributes;
        }
    },
    methods: {
        async syncModelValue ()
        {
            this.internalValue = this.modelValue || '';
            const asDate = asDateOrNull( this.modelValue );

            if ( asDate )
            {
                this.internalDateParts.Year = asDate.getFullYear();
                this.internalDateParts.Month = asDate.getMonth() + 1;
                this.internalDateParts.Day = asDate.getDate();
            }
            else
            {
                this.internalDateParts.Year = 0;
                this.internalDateParts.Month = 0;
                this.internalDateParts.Day = 0;
            }

            await this.fetchAndSetFormattedValue();
        },

        async fetchAndSetFormattedValue ()
        {
            if ( this.isCurrentDateValue )
            {
                const parts = this.internalValue.split( ':' );
                const diff = parts.length === 2 ? toNumber( parts[ 1 ] ) : 0;

                if ( diff === 1 )
                {
                    this.formattedString = 'Current Date plus 1 day';
                }
                else if ( diff > 0 )
                {
                    this.formattedString = `Current Date plus ${diff} days`;
                }
                else if ( diff === -1 )
                {
                    this.formattedString = 'Current Date minus 1 day';
                }
                else if ( diff < 0 )
                {
                    this.formattedString = `Current Date minus ${Math.abs( diff )} days`;
                }
                else
                {
                    this.formattedString = 'Current Date';
                }
            }
            else if ( this.isDatePartsPicker && this.datePartsAsDate )
            {
                this.formattedString = await this.getFormattedDateString( this.datePartsAsDate, this.dateFormatTemplate );
            }
            else if ( !this.isDatePartsPicker && this.asDate )
            {
                this.formattedString = await this.getFormattedDateString( this.asDate, this.dateFormatTemplate );
            }
            else
            {
                this.formattedString = '';
            }
        },

        async getFormattedDateString ( value: Date | string, format: string )
        {
            const get = this.http.get as BlockHttpGet;
            const result = await get<string>( 'api/Utility/FormatDate', { value, format } );
            return result.data || `${value}`;
        }
    },
    watch: {
        datePartsAsDate ()
        {
            if ( this.isDatePartsPicker )
            {
                this.$emit( 'update:modelValue', toRockDateOrNull( this.datePartsAsDate ) || '' );
            }
        },
        internalValue ()
        {
            if ( !this.isDatePartsPicker )
            {
                this.$emit( 'update:modelValue', this.internalValue || '' );
            }
        },
        modelValue: {
            immediate: true,
            async handler ()
            {
                await this.syncModelValue();
            }
        },
        async dateFormatTemplate ()
        {
            await this.fetchAndSetFormattedValue();
        }
    },
    template: `
<DatePartsPicker v-if="isEditMode && isDatePartsPicker" v-model="internalDateParts" v-bind="configAttributes" />
<DatePicker v-else-if="isEditMode" v-model="internalValue" v-bind="configAttributes" />
<span v-else>
    {{ formattedString }}
    <template v-if="elapsedString">
        ({{ elapsedString }})
    </template>
</span>`
} ) );

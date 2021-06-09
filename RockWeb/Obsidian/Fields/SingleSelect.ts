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
import { registerFieldType, getFieldTypeProps } from './Index';
import DropDownList, { DropDownListOption } from '../Elements/DropDownList';
import RadioButtonList from '../Elements/RadioButtonList';

const fieldTypeGuid: Guid = '7525C4CB-EE6B-41D4-9B64-A08048D5A5C0';

enum ConfigurationValueKey
{
    Values = 'values',
    FieldType = 'fieldtype',
    RepeatColumns = 'repeatColumns'
}

export default registerFieldType(fieldTypeGuid, defineComponent({
    name: 'SingleSelectField',
    components: {
        DropDownList,
        RadioButtonList
    },
    props: getFieldTypeProps(),
    setup ()
    {
        return {
            isRequired: inject ( 'isRequired' ) as boolean
        }
    },
    data() {
        return {
            internalValue: ''
        };
    },
    computed: {
        /** The value to display when not in edit mode */
        safeValue(): string {
            return (this.modelValue || '').trim();
        },

        /** The options to choose from in the drop down list */
        options (): DropDownListOption[]
        {
            const valuesConfig = this.configurationValues[ ConfigurationValueKey.Values ];
            if ( valuesConfig && valuesConfig.Value )
            {
                const providedOptions = valuesConfig.Value.split( ',' ).map( v =>
                {
                    if ( v.indexOf( '^' ) !== -1 )
                    {
                        const parts = v.split( '^' );
                        const value = parts[ 0 ];
                        const text = parts[ 1 ];

                        return {
                            key: value,
                            text,
                            value
                        } as DropDownListOption;
                    }

                    return {
                        key: v,
                        text: v,
                        value: v
                    } as DropDownListOption;
                } );

                if ( this.isRadioButtons && !this.isRequired )
                {
                    providedOptions.unshift( {
                        key: 'None',
                        text: 'None',
                        value: ''
                    } );
                }

                return providedOptions;
            }

            return [];
        },

        /** Any additional attributes that will be assigned to the drop down list control */
        ddlConfigAttributes(): Record<string, number | boolean> {
            const attributes: Record<string, number | boolean> = {};
            const fieldTypeConfig = this.configurationValues[ ConfigurationValueKey.FieldType ];

            if ( fieldTypeConfig?.Value === 'ddl_enhanced' )
            {
                attributes.enhanceForLongLists = true;
            }

            return attributes;
        },

        /** Any additional attributes that will be assigned to the radio button control */
        rbConfigAttributes (): Record<string, number | boolean>
        {
            const attributes: Record<string, number | boolean> = {};
            const repeatColumnsConfig = this.configurationValues[ ConfigurationValueKey.RepeatColumns ];

            if ( repeatColumnsConfig?.Value )
            {
                attributes[ 'repeatColumns' ] = Number( repeatColumnsConfig.Value ) || 0;
            }

            return attributes;
        },

        /** Is the control going to be radio buttons? */
        isRadioButtons (): boolean
        {
            const fieldTypeConfig = this.configurationValues[ ConfigurationValueKey.FieldType ];
            return fieldTypeConfig?.Value === 'rb';
        }
    },
    watch: {
        internalValue() {
            this.$emit('update:modelValue', this.internalValue);
        },
        modelValue: {
            immediate: true,
            handler ()
            {
                this.internalValue = this.modelValue || '';
            }
        }
    },
    template: `
<RadioButtonList v-if="isEditMode && isRadioButtons" v-model="internalValue" v-bind="rbConfigAttributes" :options="options" horizontal />
<DropDownList v-else-if="isEditMode" v-model="internalValue" v-bind="ddlConfigAttributes" :options="options" />
<span v-else>{{ safeValue }}</span>`
}));

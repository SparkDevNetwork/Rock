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
import { registerFieldType, getFieldTypeProps } from './Index';
import DropDownList, { DropDownListOption } from '../Elements/DropDownList';

const fieldTypeGuid: Guid = '7525C4CB-EE6B-41D4-9B64-A08048D5A5C0';

enum ConfigurationValueKey
{
    Values = 'values'
}

export default registerFieldType(fieldTypeGuid, defineComponent({
    name: 'SingleSelectField',
    components: {
        DropDownList
    },
    props: getFieldTypeProps(),
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
                return valuesConfig.Value.split( ',' ).map( v =>
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
            }

            return [];
        },

        /** Any additional attributes that will be assigned to the control */
        configAttributes(): Record<string, number | boolean> {
            const attributes: Record<string, number | boolean> = {};
            return attributes;
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
<DropDownList v-if="isEditMode" v-model="internalValue" v-bind="configAttributes" :options="options" />
<span v-else>{{ safeValue }}</span>`
}));

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
import { defineComponent, PropType, inject } from 'vue';
import DropDownList, { DropDownListOption } from '../Elements/DropDownList';
import { BlockHttp } from './RockBlock';
import DefinedValue from '../ViewModels/CodeGenerated/DefinedValueViewModel';

export default defineComponent( {
    name: 'DefinedValuePicker',
    components: {
        DropDownList
    },
    props: {
        modelValue: {
            type: String as PropType<string>,
            required: true
        },
        label: {
            type: String as PropType<string>,
            default: 'Defined Value'
        },
        definedTypeGuid: {
            type: String as PropType<string>,
            default: ''
        },
        displayDescriptions: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        show: {
            type: Boolean as PropType<boolean>,
            default: true
        }
    },
    setup ()
    {
        return {
            http: inject( 'http' ) as BlockHttp
        };
    },
    emits: [
        'update:modelValue',
        'update:model',
        'receivedDefinedValues'
    ],
    data ()
    {
        return {
            isInitialLoadDone: false,
            internalValue: this.modelValue,
            definedValues: [] as DefinedValue[],
            isLoading: false
        };
    },
    computed: {
        isEnabled (): boolean
        {
            return !!this.definedTypeGuid && !this.isLoading;
        },
        options (): DropDownListOption[]
        {
            return this.definedValues.map( dv => ( {
                key: dv.guid,
                value: dv.guid,
                text: this.displayDescriptions ? dv.description : dv.value
            } as DropDownListOption ) );
        }
    },
    watch: {
        modelValue: function (): void
        {
            this.internalValue = this.modelValue;
        },
        definedTypeGuid: {
            immediate: true,
            handler: async function (): Promise<void>
            {
                if ( !this.definedTypeGuid )
                {
                    this.definedValues = [];
                }
                else
                {
                    this.isLoading = true;
                    const result = await this.http.get<DefinedValue[]>( `/api/v2/controls/definedvaluepickers/${this.definedTypeGuid}` );

                    if ( result && result.data )
                    {
                        this.definedValues = result.data;
                        this.$emit( 'receivedDefinedValues', this.definedValues );
                    }

                    this.isLoading = false;
                }

                this.isInitialLoadDone = true;
            }
        },
        internalValue ()
        {
            this.$emit( 'update:modelValue', this.internalValue );

            const definedValue = this.definedValues.find( dv => dv.guid === this.internalValue ) || null;
            this.$emit( 'update:model', definedValue );
        }
    },
    template: `
<DropDownList v-if="isInitialLoadDone && show" v-model="internalValue" :disabled="!isEnabled" :label="label" :options="options" />`
} );

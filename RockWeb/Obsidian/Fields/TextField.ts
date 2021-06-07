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
import TextBox from '../Elements/TextBox';
import { asBooleanOrNull } from '../Services/Boolean';

const fieldTypeGuid: Guid = '9C204CD0-1233-41C5-818A-C5DA439445AA';

enum ConfigurationValueKey {
    IsPassword = 'ispassword',
    MaxCharacters = 'maxcharacters',
    ShowCountDown = 'showcountdown'
}

export default registerFieldType( fieldTypeGuid, defineComponent( {
    name: 'TextField',
    components: {
        TextBox
    },
    props: getFieldTypeProps(),
    data ()
    {
        return {
            internalValue: ''
        };
    },
    computed: {
        safeValue (): string
        {
            return ( this.modelValue || '' ).trim();
        },
        configAttributes (): Record<string, number | boolean>
        {
            const attributes: Record<string, number | boolean> = {};

            const maxCharsConfig = this.configurationValues[ ConfigurationValueKey.MaxCharacters ];
            if ( maxCharsConfig && maxCharsConfig.Value )
            {
                const maxCharsValue = Number( maxCharsConfig.Value );

                if ( maxCharsValue )
                {
                    attributes.maxLength = maxCharsValue;
                }
            }

            const showCountDownConfig = this.configurationValues[ ConfigurationValueKey.ShowCountDown ];
            if ( showCountDownConfig && showCountDownConfig.Value )
            {
                const showCountDownValue = asBooleanOrNull( showCountDownConfig.Value ) || false;

                if ( showCountDownValue )
                {
                    attributes.showCountDown = showCountDownValue;
                }
            }

            return attributes;
        },
        isPassword (): boolean
        {
            const isPasswordConfig = this.configurationValues[ ConfigurationValueKey.IsPassword ];
            return asBooleanOrNull( isPasswordConfig?.Value ) || false;
        },
        passwordDisplay (): string
        {
            return this.safeValue ? '********' : '';
        }
    },
    watch: {
        internalValue ()
        {
            this.$emit( 'update:modelValue', this.internalValue );
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
<TextBox v-if="isEditMode" v-model="internalValue" v-bind="configAttributes" :type="isPassword ? 'password' : ''" />
<span v-else-if="isPassword">{{passwordDisplay}}</span>
<span v-else>{{ safeValue }}</span>`
} ) );

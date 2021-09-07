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
import { formatPhoneNumber } from '../Services/String';
import PhoneNumberBox from '../Elements/PhoneNumberBox';

const fieldTypeGuid: Guid = '6B1908EC-12A2-463A-A7BD-970CE0FAF097';

export default registerFieldType(fieldTypeGuid, defineComponent({
    name: 'PhoneNumber',
    components: {
        PhoneNumberBox
    },
    props: getFieldTypeProps(),
    data() {
        return {
            internalValue: ''
        };
    },
    computed: {
        safeValue (): string
        {
            return formatPhoneNumber( this.modelValue || '' );
        },
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
<PhoneNumberBox v-if="isEditMode" v-model="internalValue" v-bind="configAttributes" />
<span v-else>{{ safeValue }}</span>`
}));

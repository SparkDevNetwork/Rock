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
import DropDownList, { DropDownListOption } from '../Elements/DropDownList';

const fieldTypeGuid: Guid = '2E28779B-4C76-4142-AE8D-49EA31DDB503';

export default registerFieldType(fieldTypeGuid, defineComponent({
    name: 'GenderField',
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
        displayValue(): string {
            if (this.internalValue === '0') {
                return 'Unknown';
            }
            else if (this.internalValue === '1') {
                return 'Male';
            }
            else if (this.internalValue === '2') {
                return 'Female';
            }
            else {
                return '';
            }
        },
        dropDownListOptions(): DropDownListOption[] {
            return [
                { key: '0', text: 'Unknown', value: '0' },
                { key: '1', text: 'Male', value: '1' },
                { key: '2', text: 'Female', value: '2' }
            ] as DropDownListOption[];
        }
    },
    watch: {
        internalValue() {
            this.$emit('update:modelValue', this.internalValue);
        },
        modelValue: {
            immediate: true,
            handler() {
                this.internalValue = this.modelValue || '';
            }
        }
    },
    template: `
<DropDownList v-if="isEditMode" v-model="internalValue" :options="dropDownListOptions" />
<span v-else>{{ displayValue }}</span>`
}));

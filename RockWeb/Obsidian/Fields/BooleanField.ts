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
import { asYesNoOrNull, asTrueFalseOrNull } from '../Services/Boolean';
import DropDownList, { DropDownListOption } from '../Elements/DropDownList';

const fieldTypeGuid: Guid = '1EDAFDED-DFE6-4334-B019-6EECBA89E05A';

export default registerFieldType(fieldTypeGuid, defineComponent({
    name: 'BooleanField',
    components: {
        DropDownList
    },
    props: getFieldTypeProps(),
    data() {
        const trueVal = asTrueFalseOrNull(true);
        const falseVal = asTrueFalseOrNull(false);
        const yesVal = asYesNoOrNull(true);
        const noVal = asYesNoOrNull(false);

        return {
            internalValue: '',
            dropDownListOptions: [
                { key: falseVal, text: noVal, value: falseVal },
                { key: trueVal, text: yesVal, value: trueVal }
            ] as DropDownListOption[]
        };
    },
    computed: {
        valueAsYesNoOrNull() {
            return asYesNoOrNull(this.modelValue);
        }
    },
    watch: {
        internalValue() {
            this.$emit('update:modelValue', this.internalValue);
        },
        modelValue: {
            immediate: true,
            handler() {
                this.internalValue = asTrueFalseOrNull(this.modelValue) || '';
            }
        }
    },
    template: `
<DropDownList v-if="isEditMode" v-model="internalValue" :options="dropDownListOptions" />
<span v-else>{{ valueAsYesNoOrNull }}</span>`
}));

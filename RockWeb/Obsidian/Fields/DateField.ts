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
import { defineComponent } from '../Vendor/Vue/vue.js';
import { Guid } from '../Util/Guid.js';
import { getFieldTypeProps, registerFieldType } from './Index.js';
import { asDateString } from '../Services/Date.js';
import DatePicker from '../Elements/DatePicker.js';

const fieldTypeGuid: Guid = '6B6AA175-4758-453F-8D83-FCD8044B5F36';

export default registerFieldType(fieldTypeGuid, defineComponent({
    name: 'DateField',
    components: {
        DatePicker
    },
    props: getFieldTypeProps(),
    data() {
        return {
            internalValue: this.modelValue
        };
    },
    computed: {
        valueAsDateString() {
            return asDateString(this.modelValue);
        }
    },
    watch: {
        internalValue() {
            this.$emit('update:modelValue', this.internalValue);
        }
    },
    template: `
<DatePicker v-if="isEditMode" v-model="internalValue" />
<span v-else>{{ valueAsDateString }}</span>`
}));

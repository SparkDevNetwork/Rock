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

import { asFormattedString, toNumberOrNull } from '../Services/Number';
import { defineComponent, PropType } from 'vue';
import { newGuid } from '../Util/Guid';
import RockFormField from './RockFormField';

export default defineComponent({
    name: 'CurrencyBox',
    components: {
        RockFormField
    },
    props: {
        modelValue: {
            type: Number as PropType<number | null>,
            default: null
        }
    },
    emits: [
        'update:modelValue'
    ],
    data: function () {
        return {
            uniqueId: `rock-currencybox-${newGuid()}`,
            internalValue: ''
        };
    },
    methods: {
        onChange() {
            this.internalValue = asFormattedString(this.modelValue);
        }
    },
    computed: {
        internalNumberValue(): number | null {
            return toNumberOrNull(this.internalValue);
        }
    },
    watch: {
        internalNumberValue() {
            this.$emit('update:modelValue', this.internalNumberValue);
        },
        modelValue: {
            immediate: true,
            handler() {
                if (this.modelValue !== this.internalNumberValue) {
                    this.internalValue = asFormattedString(this.modelValue);
                }
            }
        }
    },
    template: `
<RockFormField
    v-model="internalValue"
    @change="onChange"
    formGroupClasses="rock-currency-box"
    name="currencybox">
    <template #default="{uniqueId, field, errors, disabled, inputGroupClasses}">
        <div class="control-wrapper">
            <div class="input-group" :class="inputGroupClasses">
                <span class="input-group-addon">$</span>
                <input :id="uniqueId" type="text" class="form-control" v-bind="field" :disabled="disabled" />
            </div>
        </div>
    </template>
</RockFormField>`
});

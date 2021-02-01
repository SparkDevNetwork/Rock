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
import { newGuid } from '../Util/Guid.js';

export default defineComponent({
    name: 'CheckBox',
    props: {
        modelValue: {
            type: Boolean,
            required: true
        },
        label: {
            type: String,
            required: true
        }
    },
    emits: [
        'update:modelValue'
    ],
    data: function () {
        return {
            uniqueId: `rock-checkbox-${newGuid()}`,
            internalValue: this.modelValue
        };
    },
    methods: {
        handleInput: function () {
            this.$emit('update:modelValue', this.internalValue);
        }
    },
    watch: {
        value: function () {
            this.internalValue = this.modelValue;
        }
    },
    template:
`<div class="checkbox">
    <label title="">
        <input type="checkbox" v-model="internalValue" />
        <span class="label-text ">{{label}}</span>
    </label>
</div>`
});

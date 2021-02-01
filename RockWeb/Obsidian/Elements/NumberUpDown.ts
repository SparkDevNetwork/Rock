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
import { defineComponent, PropType } from '../Vendor/Vue/vue.js';

export default defineComponent({
    name: 'NumberUpDown',
    props: {
        modelValue: {
            type: Number as PropType<number>,
            required: true
        },
        min: {
            type: Number as PropType<number>,
            default: 1
        },
        max: {
            type: Number as PropType<number>,
            default: 9
        }
    },
    methods: {
        goUp() {
            if (!this.isUpDisabled) {
                this.$emit('update:modelValue', this.modelValue + 1);
            }
        },
        goDown() {
            if (!this.isDownDisabled) {
                this.$emit('update:modelValue', this.modelValue - 1);
            }
        }
    },
    computed: {
        isUpDisabled(): boolean {
            return this.modelValue >= this.max;
        },
        isDownDisabled(): boolean {
            return this.modelValue <= this.min;
        }
    },
    template: `
<div class="numberincrement">
    <a @click="goDown" class="numberincrement-down" :class="{disabled: isDownDisabled}" :disabled="isDownDisabled">
        <i class="fa fa-minus "></i>
    </a>
    <span class="numberincrement-value">{{modelValue}}</span>
    <a @click="goUp" class="numberincrement-up" :class="{disabled: isUpDisabled}" :disabled="isUpDisabled">
        <i class="fa fa-plus "></i>
    </a>
</div>`
});

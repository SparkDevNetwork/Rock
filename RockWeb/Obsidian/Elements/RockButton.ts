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
import { defineComponent, PropType } from 'vue';

export enum BtnType {
    default = 'default',
    primary = 'primary',
    danger = 'danger',
    warning = 'warning',
    success = 'success',
    info = 'info',
    link = 'link'
}

export enum BtnSize {
    default = '',
    xs = 'xs',
    sm = 'sm',
    lg = 'lg'
}

export default defineComponent({
    name: 'RockButton',
    props: {
        isLoading: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        loadingText: {
            type: String as PropType<string>,
            default: 'Loading...'
        },
        type: {
            type: String as PropType<string>,
            default: 'button'
        },
        disabled: {
            type: Boolean,
            default: false
        },
        btnType: {
            type: String as PropType<BtnType>,
            default: BtnType.default
        },
        btnSize: {
            type: String as PropType<BtnSize>,
            default: BtnSize.default
        }
    },
    emits: [
        'click'
    ],
    methods: {
        handleClick: function (event: Event) {
            if (!this.isLoading) {
                this.$emit('click', event);
            }
        }
    },
    computed: {
        typeClass(): string {
            return `btn-${this.btnType}`;
        },
        sizeClass(): string {
            if (!this.btnSize) {
                return '';
            }

            return `btn-${this.btnSize}`;
        },
        cssClasses(): string {
            return `btn ${this.typeClass} ${this.sizeClass}`;
        }
    },
    template: `
<button :class="cssClasses" :disabled="isLoading || disabled" @click="handleClick" :type="type">
    <template v-if="isLoading">
        {{loadingText}}
    </template>
    <slot v-else />
</button>`
});

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
        default: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        success: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        info: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        danger: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        warning: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        primary: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        link: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        xs: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        sm: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        lg: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },
    emits: [
        'click'
    ],
    methods: {
        handleClick: function (event: Event) {
            this.$emit('click', event);
        }
    },
    computed: {
        typeClass(): string {
            if (this.danger) {
                return 'btn-danger';
            }

            if (this.warning) {
                return 'btn-warning';
            }

            if (this.success) {
                return 'btn-success';
            }

            if (this.info) {
                return 'btn-info';
            }

            if (this.primary) {
                return 'btn-primary';
            }

            if (this.link) {
                return 'btn-link';
            }

            return 'btn-default';
        },
        sizeClass(): string {
            if (this.xs) {
                return 'btn-xs';
            }

            if (this.sm) {
                return 'btn-sm';
            }

            if (this.lg) {
                return 'btn-lg';
            }

            return '';
        },
        cssClasses(): string {
            return `btn ${this.typeClass} ${this.sizeClass}`;
        }
    },
    template:
`<button :class="cssClasses" :disabled="isLoading || disabled" @click="handleClick" :type="type">
    <template v-if="isLoading">
        {{loadingText}}
    </template>
    <slot v-else />
</button>`
});

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
    name: 'Alert',
    props: {
        dismissible: {
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
        validation: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },
    emits: [
        'dismiss'
    ],
    methods: {
        onDismiss: function () {
            this.$emit('dismiss');
        }
    },
    computed: {
        typeClass(): string {
            if (this.danger) {
                return 'alert-danger';
            }

            if (this.warning) {
                return 'alert-warning';
            }

            if (this.success) {
                return 'alert-success';
            }

            if (this.info) {
                return 'alert-info';
            }

            if (this.primary) {
                return 'alert-primary';
            }

            if (this.validation) {
                return 'alert-validation';
            }

            return 'btn-default';
        },
    },
    template: `
<div class="alert" :class="typeClass">
    <button v-if="dismissible" type="button" class="close" @click="onDismiss">
        <span>&times;</span>
    </button>
    <slot />
</div>`
});

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

import { defineComponent, PropType } from "vue";

/**
 * Displays the UI for the Confirmation Email component in the Communications
 * screen.
 */
export default defineComponent({
    name: "Header",

    props: {
        title: {
            type: String as PropType<string>,
            default: ""
        },

        description: {
            type: String as PropType<string>,
            default: ""
        },

        isSeparatorHidden: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    emits: [
        "update:modelValue"
    ],

    setup() {
        return {
        };
    },

    template: `
<div class="rock-header">
    <div class="d-flex flex-wrap justify-content-between">
        <div>
            <h3 v-if="title" class="title">{{ title }}</h3>
            <p v-if="description" class="description">{{ description }}</p>
        </div>
        <div v-if="$slots.actions" class="section-header-actions align-self-end">
            <slot name="actions" />
        </div>
    </div>

    <hr v-if="!isSeparatorHidden" class="section-header-hr">
</div>
`
});

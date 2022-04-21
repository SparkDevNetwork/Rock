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

import { computed } from "vue";
import { defineComponent, PropType } from "vue";

export default defineComponent({
    name: "FieldFilterContainer",

    props: {
        compareLabel: {
            type: String as PropType<string>
        },
    },

    setup(props, ctx) {
        /** True if the compare column should be displayed. */
        const hasCompareColumn = computed((): boolean => !!ctx.slots.compare || !!props.compareLabel);

        /** True if we have a plain text label to display in the compare column. */
        const hasCompareLabel = computed((): boolean => !!props.compareLabel);

        /** The CSS class to use for the compare column width. */
        const compareColumnClass = computed((): string => {
            if (ctx.slots.compare) {
                return "col-md-4";
            }
            else if (props.compareLabel) {
                return "col-md-2";
            }
            else {
                return "";
            }
        });

        /** The CSS class to use for the value column width. */
        const valueColumnClass = computed((): string => {
            if (ctx.slots.compare) {
                return "col-md-8";
            }
            else if (props.compareLabel) {
                return "col-md-10";
            }
            else {
                return "col-md-12";
            }
        });

        return {
            compareColumnClass,
            hasCompareColumn,
            hasCompareLabel,
            valueColumnClass
        };
    },

    template: `
<div class="row form-row field-criteria">
    <div v-if="hasCompareColumn" :class="compareColumnClass">
        <span v-if="hasCompareLabel" class="data-view-filter-label">{{ compareLabel }}</span>
        <slot v-else name="compare" />
    </div>

    <div :class="valueColumnClass">
        <slot />
    </div>
</div>
`
});

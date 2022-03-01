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

import { computed, PropType } from "vue";
import { defineComponent } from "vue";

export default defineComponent({
    name: "Workflow.FormBuilderDetail.ConfigurableZone",
    components: {
    },

    props: {
        modelValue: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        iconCssClass: {
            type: String as PropType<string>,
            default: "fa fa-gear"
        }
    },

    emits: [
        "configure"
    ],

    setup(props, { emit }) {
        const zoneClasses = computed((): string[] => {
            const classes: string[] = ["configurable-zone"];

            if (props.modelValue) {
                classes.push("active");
            }

            return classes;
        });

        const onActionClick = (): void => {
            emit("configure");
        };

        return {
            onActionClick,
            zoneClasses
        };
    },

    template: `
<div class="configurable-zone" :class="zoneClasses">
    <div class="zone-content-container">
        <div class="zone-content">
            <slot />
        </div>
    </div>

    <div class="zone-actions">
        <slot name="preActions" />
        <i v-if="iconCssClass" :class="iconCssClass + ' fa-fw zone-action'" @click.stop="onActionClick"></i>
        <slot name="postActions" />
    </div>
</div>
`
});

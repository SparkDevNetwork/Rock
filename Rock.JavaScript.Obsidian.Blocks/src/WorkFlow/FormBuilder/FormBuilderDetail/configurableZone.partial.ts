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
        },

        clickBodyToConfigure: {
            type: Boolean as PropType<boolean>,
            default: false
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

        const onBodyActionClick = (): void => {
            if (props.clickBodyToConfigure) {
                emit("configure");
            }
        };

        return {
            onActionClick,
            onBodyActionClick,
            zoneClasses
        };
    },

    template: `
<div :class="zoneClasses">
    <div class="zone-content-container" @click.stop="onBodyActionClick">
        <div class="zone-content">
            <slot />
        </div>
    </div>

    <div class="zone-actions">
        <slot name="preActions" />
        <div v-if="iconCssClass" class="zone-action" @click.stop="onActionClick"><i :class="iconCssClass + ' fa-fw'"></i></div>
        <slot name="postActions" />
    </div>
</div>
`
});

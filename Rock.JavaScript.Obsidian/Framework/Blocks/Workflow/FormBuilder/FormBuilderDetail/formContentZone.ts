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

import { computed, defineComponent, PropType } from "vue";
import ConfigurableZone from "./configurableZone";

export default defineComponent({
    name: "Workflow.FormBuilderDetail.FormContentZone",

    components: {
        ConfigurableZone
    },

    props: {
        modelValue: {
            type: String as PropType<string>,
            required: true
        },

        placeholder: {
            type: String as PropType<string>,
            required: true
        },

        isActive: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        iconCssClass: {
            type: String as PropType<string>,
            default: "fa fa-pencil"
        }
    },

    emits: [
        "configure"
    ],

    setup(props, { emit }) {
        /** True if we have custom content to render. */
        const hasContent = computed((): boolean => !!props.modelValue);

        /**
         * A string that represents the render-safe content. This makes sure
         * that any broken or non-ended elements get ended.
         */
        const safeContent = computed((): string => {
            if (!props.modelValue) {
                return "";
            }

            const div = document.createElement("div");
            div.innerHTML = props.modelValue;

            return div.innerHTML;
        });

        /**
         * Event handler for when the configure button is clicked.
         */
        const onConfigure = (): void => emit("configure");

        return {
            hasContent,
            onConfigure,
            safeContent
        };
    },

    template: `
<ConfigurableZone :modelValue="isActive" :iconCssClass="iconCssClass" @configure="onConfigure">
    <div class="zone-body">
        <div v-if="hasContent" style="min-height: 24px;" v-html="safeContent"></div>
        <div v-else class="text-center text-muted">{{ placeholder }}</div>
    </div>
</ConfigurableZone>
`
});

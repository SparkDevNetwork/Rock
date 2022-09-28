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
import InlineSwitch from "./inlineSwitch";
import TransitionVerticalCollapse from "./transitionVerticalCollapse";
import { useVModelPassthrough } from "@Obsidian/Utility/component";
import SectionHeader from "./sectionHeader";

/**
 * Displays the UI for the Confirmation Email component in the Communications
 * screen.
 */
export default defineComponent({
    name: "SectionContainer",

    components: {
        SectionHeader,
        InlineSwitch,
        TransitionVerticalCollapse
    },

    props: {
        modelValue: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        toggleText: {
            type: String as PropType<string>,
            default: ""
        },

        title: {
            type: String as PropType<string>,
            default: ""
        },

        description: {
            type: String as PropType<string>,
            default: ""
        }
    },

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        const enabled = useVModelPassthrough(props, "modelValue", emit);

        /**
         * True if the content should be visible. Content is visible if we either
         * do not have an enable button or the enable button is on.
         */
        const showContent = computed((): boolean => enabled.value || !props.toggleText);

        return {
            enabled,
            showContent
        };
    },

    template: `
<div class="section-container well">
    <div class="section-header">
        <div class="section-header-content">
            <SectionHeader :title="title" :description="description" :isSeparatorHidden="true">
                <template #actions><slot name="actions" /></template>
            </SectionHeader>
        </div>

        <div v-if="toggleText" class="section-header-toggle align-self-end">
            <InlineSwitch v-model="enabled" :label="toggleText" />
        </div>
    </div>

    <TransitionVerticalCollapse>
        <div v-if="showContent">
            <hr class="section-header-hr">
            <slot />
        </div>
    </TransitionVerticalCollapse>
</div>
`
});

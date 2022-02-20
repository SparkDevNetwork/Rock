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

import { computed, defineComponent, PropType, ref, watch } from "vue";
import InlineSwitch from "../../../../Elements/inlineSwitch";
import TransitionVerticalCollapse from "../../../../Elements/transitionVerticalCollapse";

/**
 * Displays the UI for the Confirmation Email component in the Communications
 * screen.
 */
export default defineComponent({
    name: "Workflow.FormBuilderDetail.SettingsWell",

    components: {
        InlineSwitch,
        TransitionVerticalCollapse
    },

    props: {
        modelValue: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        hasEnable: {
            type: Boolean as PropType<boolean>,
            default: false
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
        /** True if the section is enabled and the rest of the UI should be shown. */
        const enabled = ref(props.modelValue);

        /**
         * True if the content should be visible. Content is visible if we either
         * do not have an enable button or the enable button is on.
         */
        const showContent = computed((): boolean => enabled.value || !props.hasEnable);

        // Watch for changes in our modelValue and then update all our internal values.
        watch(() => props.modelValue, () => {
            enabled.value = props.modelValue;
        });

        // Watch for changes on any of our internal values and then update the modelValue.
        watch([enabled], () => {
            emit("update:modelValue", enabled.value);
        });

        return {
            enabled,
            showContent
        };
    },

    template: `
<div class="well">
    <div class="d-flex">
        <div style="flex-grow: 1;">
            <h3 v-if="title">{{ title }}</h3>
            <p v-if="description">{{ description }}</p>
        </div>

        <div v-if="hasEnable" style="align-self: end;">
            <InlineSwitch v-model="enabled" label="Enable" />
        </div>
    </div>

    <TransitionVerticalCollapse>
        <div v-if="showContent">
            <hr />

            <slot />
        </div>
    </TransitionVerticalCollapse>
</div>
`
});

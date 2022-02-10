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
import { defineComponent, PropType, ref, watch } from "vue";
import RockButton from "../Elements/rockButton";
import { trackModalState } from "../Util/page";

export default defineComponent({
    name: "Modal",

    components: {
        RockButton
    },

    props: {
        modelValue: {
            type: Boolean as PropType<boolean>,
            required: true
        },

        title: {
            type: String as PropType<string>,
            default: ""
        },

        subtitle: {
            type: String as PropType<string>,
            default: ""
        }
    },

    setup(props, { emit }) {
        /** Used to determine if shaking should be currently performed. */
        const isShaking = ref(false);

        /**
         * Event handler for when one of the close buttons is clicked.
         */
        const onClose = (): void => {
            emit("update:modelValue", false);
        };

        /**
         * Event handler for when the scrollable is clicked.
         */
        const onScrollableClick = (): void => {
            // If we aren't already shaking, start shaking to let the user know
            // they are doing something not allowed.
            if (!isShaking.value) {
                isShaking.value = true;
                setTimeout(() => isShaking.value = false, 1000);
            }
        };

        // If we are starting visible, then update the modal tracking.
        if (props.modelValue) {
            trackModalState(true);
        }

        // Watch for changes in our visiblity and update the modal tracking.
        watch(() => props.modelValue, () => trackModalState(props.modelValue));

        return {
            isShaking,
            onScrollableClick,
            onClose
        };
    },

    template: `
<teleport to="body" v-if="modelValue">
    <div>
        <div class="modal-backdrop" style="z-index: 1060;"></div>

        <div @click.stop="onScrollableClick" class="modal-scrollable" style="z-index: 1060;">
            <div @click.stop
                class="modal container modal-content rock-modal rock-modal-frame modal-overflow"
                :class="{'animated shake': isShaking}"
                aria-hidden="false"
                tabindex="-1"
                role="dialog"
                style="display: block; margin-top: 0px;">
                <div class="modal-header">
                    <button @click="onClose" class="close" aria-hidden="true" type="button">&times;</button>
                    <template v-if="title">
                        <h3 class="modal-title">{{ title }}</h3>
                        <small v-if="subtitle">{{ subtitle }}</small>
                    </template>
                    <slot v-else name="header" />
                </div>

                <div class="modal-body">
                    <slot />
                </div>

                <div class="modal-footer">
                    <a @click.prevent="onClose" class="btn btn-link">Cancel</a>
                    <slot name="customButtons" />
                </div>
            </div>
        </div>
    </div>
</teleport>
`
});

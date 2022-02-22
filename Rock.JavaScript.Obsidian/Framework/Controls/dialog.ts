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
import { computed, defineComponent, nextTick, PropType, ref, watch } from "vue";
import RockButton from "../Elements/rockButton";
import { trackModalState } from "../Util/page";

export default defineComponent({
    name: "Dialog",

    components: {
        RockButton
    },

    props: {
        modelValue: {
            type: Boolean as PropType<boolean>,
            required: true
        },

        dismissible: {
            type: Boolean as PropType<boolean>,
            default: true
        }
    },

    setup(props, { emit, slots }) {
        const doShake = ref(false);
        const modalDiv = ref<HTMLElement | null>(null);

        const hasHeader = computed(() => !!slots.header);

        const close = (): void => {
            emit("update:modelValue", false);
        };

        const shake = (): void => {
            if (!doShake.value) {
                doShake.value = true;
                setTimeout(() => doShake.value = false, 1000);
            }
        };

        const centerOnScreen = (): void => {
            nextTick(() => {
                if (!modalDiv.value) {
                    return;
                }

                const height = modalDiv.value.offsetHeight;
                const margin = height / 2;
                modalDiv.value.style.marginTop = `-${margin}px`;
            });
        };

        if (props.modelValue) {
            trackModalState(true);
        }

        watch(() => props.modelValue, () => {
            trackModalState(props.modelValue);
        });

        return {
            centerOnScreen,
            close,
            doShake,
            hasHeader,
            modalDiv,
            shake
        };
    },

    template: `
<teleport to="body" v-if="modelValue">
    <div>
        <div class="modal-backdrop fade in" style="z-index: 1060;"></div>

        <div @click="shake" class="modal-scrollable" style="z-index: 1060;">
            <div @click.stop ref="modalDiv" class="modal fade in" :class="{'animated shake': doShake}" tabindex="-1" role="dialog" style="display: block;">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div v-if="hasHeader" class="modal-header">
                            <button v-if="dismissible" @click="close" type="button" class="close" style="margin-top: -10px;">×</button>
                            <slot name="header" />
                        </div>
                        <div class="modal-body">
                            <button v-if="!hasHeader && dismissible" @click="close" type="button" class="close" style="margin-top: -10px;">×</button>
                            <slot />
                        </div>
                        <div v-if="$slots.footer" class="modal-footer">
                            <slot name="footer" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</teleport>
`
});

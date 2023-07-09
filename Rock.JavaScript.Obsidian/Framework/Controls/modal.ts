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
import { defineComponent, onBeforeUnmount, PropType, ref, watch } from "vue";
import RockForm from "./rockForm";
import RockButton from "./rockButton";
import RockValidation from "./rockValidation";
import { trackModalState } from "@Obsidian/Utility/page";
import { FormError } from "@Obsidian/Utility/form";

export default defineComponent({
    name: "Modal",

    components: {
        RockButton,
        RockForm,
        RockValidation
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
        },

        cancelText: {
            type: String as PropType<string>,
            default: "Cancel"
        },

        saveText: {
            type: String as PropType<string>,
            default: ""
        },

        saveButtonDisabled: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    emits: {
        "update:modelValue": (_value: boolean) => true,
        save: () => true
    },

    setup(props, { emit }) {
        // #region Values

        const internalModalVisible = ref(props.modelValue);
        const container = ref(document.fullscreenElement ?? document.body);
        const validationErrors = ref<FormError[]>([]);

        /** Used to determine if shaking should be currently performed. */
        const isShaking = ref(false);

        // These values are used to automatically resize the modal if an absolute
        // popup like a DataView picker is used inside the modal body.
        const modalBodyElement = ref<HTMLElement | null>(null);
        const modalBodyPaddingElement = ref<HTMLElement | null>(null);
        let legacyOverflowTimer: NodeJS.Timeout | undefined;
        const overflowMutationObserver: MutationObserver | undefined = MutationObserver ? new MutationObserver(onOverflowMutation) : undefined;
        const overflowResizeObserver: ResizeObserver | undefined = ResizeObserver ? new ResizeObserver(onOverflowResize) : undefined;

        // #endregion

        // #region Functions

        /**
         * Adjusts the height of the modal to fit the content. This handles
         * cases where something like a DataViewPicker is opened up that has
         * an inner popup.
         */
        function adjustOverflowPadding(): void {
            if (modalBodyElement.value && modalBodyPaddingElement.value) {
                if (modalBodyElement.value.scrollHeight !== modalBodyElement.value.clientHeight) {
                    let existingPadding = 0;
                    try {
                        existingPadding = parseFloat(window.getComputedStyle(modalBodyPaddingElement.value, null).getPropertyValue("padding-bottom"));
                    }
                    catch {
                        existingPadding = 0;
                    }

                    const totalPadding = modalBodyElement.value.scrollHeight - modalBodyElement.value.clientHeight;

                    if (existingPadding !== totalPadding) {
                        modalBodyPaddingElement.value.style.paddingBottom = `${totalPadding}px`;
                    }
                }
                else if (modalBodyPaddingElement.value.style.paddingBottom !== "") {
                    modalBodyPaddingElement.value.style.paddingBottom = "";
                }
            }
        }

        /**
         * Timer callback when the observers are not supported by the browser.
         * Check the overflow padding to see if it needs to be adjusted.
         */
        function detectLegacyOverflow(): void {
            adjustOverflowPadding();
            legacyOverflowTimer = setTimeout(detectLegacyOverflow, 250);
        }

        // #endregion

        // #region Event Handlers

        /**
         * Event handler for when one of the close buttons is clicked.
         */
        const onClose = (): void => {
            internalModalVisible.value = false;
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

        /**
         * Event handler for when the form has been submitted.
         */
        const onSubmit = (): void => {
            emit("save");
        };

        /**
         * Event handler for when the visible validation errors have changed.
         * This should trigger us showing these errors in the modal.
         *
         * @param errors The errors that should be displayed.
         */
        const onVisibleValidationChanged = (errors: FormError[]): void => {
            validationErrors.value = errors;
        };

        /**
         * Called when the mutation observer has noticed that the DOM inside
         * our modal has been modified. Update the resize observer to watch
         * those nodes for size changes.
         *
         * @param mutations The mutations that happened since we were last called.
         */
        function onOverflowMutation(mutations: MutationRecord[]): void {
            for (const mutation of mutations) {
                mutation.addedNodes.forEach(node => {
                    if (node instanceof Element) {
                        overflowResizeObserver?.observe(node);
                    }
                });

                mutation.removedNodes.forEach(node => {
                    if (node instanceof Element) {
                        overflowResizeObserver?.unobserve(node);
                    }
                });
            }
        }

        /**
         * Called when the resize observer has noticed that the size of any child
         * nodes have changed. Check if we need to adjust our padding.
         */
        function onOverflowResize(): void {
            adjustOverflowPadding();
        }

        // #endregion

        // Watch for changes in our visiblity.
        watch(() => props.modelValue, () => {
            if (props.modelValue) {
                container.value = document.fullscreenElement || document.body;

                // Clear any old validation errors. They will be updated when
                // the submit button is next clicked.
                validationErrors.value = [];
            }

            internalModalVisible.value = props.modelValue;
        });

        watch(internalModalVisible, () => {
            trackModalState(internalModalVisible.value);
            emit("update:modelValue", internalModalVisible.value);
        });

        // Watch for changes to the body element reference. When it becomes
        // available start observing for changes.
        watch(modalBodyElement, () => {
            if (overflowMutationObserver && overflowResizeObserver) {
                if (modalBodyElement.value) {
                    overflowMutationObserver.observe(modalBodyElement.value, {
                        subtree: true,
                        childList: true
                    });

                    // Start observing initial children.
                    modalBodyElement.value.querySelectorAll("*").forEach(node => {
                        if (node instanceof Element) {
                            overflowResizeObserver.observe(node);
                        }
                    });
                }
                else {
                    overflowMutationObserver?.disconnect();
                    overflowResizeObserver?.disconnect();
                }
            }
            else {
                if (modalBodyElement.value) {
                    legacyOverflowTimer = setTimeout(detectLegacyOverflow, 250);
                }
                else if (legacyOverflowTimer) {
                    clearTimeout(legacyOverflowTimer);
                    legacyOverflowTimer = undefined;
                }
            }
        });

        onBeforeUnmount(() => {
            if (internalModalVisible.value) {
                trackModalState(false);
            }

            if (overflowMutationObserver && overflowResizeObserver) {
                overflowMutationObserver.disconnect();
                overflowResizeObserver.disconnect();
            }

            if (legacyOverflowTimer) {
                clearTimeout(legacyOverflowTimer);
                legacyOverflowTimer = undefined;
            }
        });

        // If we are starting visible, then update the modal tracking.
        if (internalModalVisible.value) {
            trackModalState(true);
        }

        return {
            container,
            internalModalVisible,
            isShaking,
            modalBodyElement,
            modalBodyPaddingElement,
            onClose,
            onScrollableClick,
            onSubmit,
            onVisibleValidationChanged,
            validationErrors
        };
    },

    template: `
<teleport :to="container" v-if="modelValue">
    <div>
        <div class="modal-backdrop" style="z-index: 1050;"></div>

        <div @click.stop="onScrollableClick" class="modal-scrollable" style="z-index: 1050;">
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

                <RockForm @submit="onSubmit" hideErrors @visibleValidationChanged="onVisibleValidationChanged">
                    <div class="modal-body">
                        <RockValidation :errors="validationErrors" />

                        <div ref="modalBodyElement">
                        <slot />
                        </div>

                        <div ref="modalBodyPaddingElement" style="transition: 0.15s padding-bottom"></div>
                    </div>

                    <div class="modal-footer">
                        <RockButton @click="onClose" btnType="link">{{ cancelText }}</RockButton>
                        <RockButton v-if="saveText" type="submit" btnType="primary" :disabled="saveButtonDisabled">{{ saveText }}</RockButton>
                        <slot name="customButtons" />
                    </div>
                </RockForm>
            </div>
        </div>
    </div>
</teleport>
`
});

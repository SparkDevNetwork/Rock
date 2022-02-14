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

import { onBeforeUnmount } from "vue";
import { defineComponent, onMounted, PropType, ref, watch } from "vue";

// Define the browser-specific versions of these functions that older browsers
// implemented before using the standard API.
declare global {
    // eslint-disable-next-line @typescript-eslint/naming-convention
    interface Document {
        mozCancelFullScreen?: () => Promise<void>;
        webkitExitFullscreen?: () => Promise<void>;
        mozFullScreenElement?: Element;
        webkitFullscreenElement?: Element;
    }

    // eslint-disable-next-line @typescript-eslint/naming-convention
    interface HTMLElement {
        mozRequestFullscreen?: () => Promise<void>;
        webkitRequestFullscreen?: () => Promise<void>;
    }
}

/**
 * Request that the window enter true fullscreen mode for the given element.
 * 
 * @param element The element that will be the root of the fullscreen view.
 *
 * @returns A promise that indicates when the operation has completed.
 */
async function requestWindowFullscreen(element: HTMLElement): Promise<boolean> {
    try {
        if (element.requestFullscreen) {
            await element.requestFullscreen();
        }
        else if (element.mozRequestFullscreen) {
            await element.mozRequestFullscreen();
        }
        else if (element.webkitRequestFullscreen) {
            await element.webkitRequestFullscreen();
        }
        else {
            return false;
        }

        return true;
    }
    catch (ex) {
        return new Promise<boolean>((_, reject) => reject(ex));
    }
}

/**
 * Request that the window exit true fullscreen mode.
 *
 * @returns A promise that indicates when the operation has completed.
 */
async function exitWindowFullscreen(): Promise<void> {
    if (document.exitFullscreen) {
        await document.exitFullscreen();
    }
    else if (document.mozCancelFullScreen) {
        await document.mozCancelFullScreen();
    }
    else if (document.webkitExitFullscreen) {
        document.webkitExitFullscreen();
    }
}

/** Indicates the full screen mode options available to our component. */
const enum FullscreenMode {
    /* Not currently in fullscreen mode. */
    None = 0,

    /** Using in-page fullscreen mode. */
    Page = 1,

    /** Using full-window fullscreen mode. */
    Window = 2
}

export default defineComponent({
    name: "Fullscreen",

    props: {
        modelValue: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        isPageOnly: {
            type: Boolean as PropType<boolean>,
            default: true
        }
    },

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        /** The current fullscreen mode this component is in. */
        const fullscreenMode = ref(FullscreenMode.None);

        /** The container element used when transitioning into fullscreen mode. */
        const containerElement = ref<HTMLElement | null>(null);

        /** True if the teleport should be disabled, i.e. leave the content where it is. */
        const teleportDisabled = ref(true);

        /** The style classes to be applied to the container. */
        const containerStyle = ref<Record<string, string>>({});

        /**
         * Attempts to enter fullscreen mode using the currently configured
         * settings.
         */
        const enterFullscreen = async (): Promise<void> => {
            const body = document.getElementsByTagName("body")[0];

            body.classList.add("is-fullscreen");

            // TODO: This can be removed once the "is-fullscreen" class is implemented.
            if (body.style.overflowY === "") {
                body.style.overflowY = "hidden";
            }

            if (props.isPageOnly) {

                // Set the styles so our container floats above all other content
                // and handles its own scrolling.
                containerStyle.value = {
                    position: "fixed",
                    left: "0",
                    top: "0",
                    width: "100%",
                    height: "100%",
                    "overflow-y": "auto",
                    "z-index": "1060",
                    "background-color": "var(--body-background)"
                };

                teleportDisabled.value = false;
                fullscreenMode.value = FullscreenMode.Page;
            }
            else {
                if (containerElement.value) {
                    try {
                        containerStyle.value = {
                            "background-color": "var(--body-background)"
                        };

                        await requestWindowFullscreen(containerElement.value);

                        fullscreenMode.value = FullscreenMode.Window;
                    }
                    catch (ex) {
                        console.warn(ex);
                    }
                }
            }
        };

        /**
         * Attempts to exit from the current fullscreen mode.
         */
        const exitFullscreen = async (): Promise<void> => {
            const body = document.getElementsByTagName("body")[0];

            // TODO: This can be removed once the "is-fullscreen" class is implemented.
            if (body.style.overflowY === "hidden") {
                body.style.overflowY = "";
            }

            body.classList.remove("is-fullscreen");

            if (fullscreenMode.value === FullscreenMode.Page) {
                containerStyle.value = {};

                teleportDisabled.value = true;
                fullscreenMode.value = FullscreenMode.None;
            }
            else if (fullscreenMode.value === FullscreenMode.Window) {
                try {
                    const fullscreenElement = document.fullscreenElement ?? document.mozFullScreenElement ?? document.webkitFullscreenElement;

                    // Only exit full screen if we are currently in it. The user
                    // might have hit escape to exit fullscreen already.
                    if (fullscreenElement) {
                        await exitWindowFullscreen();
                    }

                    containerStyle.value = {};

                    fullscreenMode.value = FullscreenMode.None;
                }
                catch (ex) {
                    console.warn(ex);
                }
            }
        };

        /**
         * Event handler when the window fullscreen mode changes.
         */
        const onFullscreenChange = (): void => {
            const fullscreenElement = document.fullscreenElement ?? document.mozFullScreenElement ?? document.webkitFullscreenElement;

            if (fullscreenMode.value !== FullscreenMode.None && !fullscreenElement) {
                exitFullscreen();
            }
        };

        // Perform final initialization once we are mounted.
        onMounted(() => {
            document.addEventListener("fullscreenchange", onFullscreenChange);

            // Set initial full-screen state if requested.
            if (props.modelValue) {
                enterFullscreen();
            }
        });

        // Before we unmount, we need to attempt to exit fullscreen mode.
        onBeforeUnmount(() => {
            if (fullscreenMode.value !== FullscreenMode.None) {
                try {
                    exitFullscreen();
                }
                catch (ex) {
                    console.warn(ex);
                }
            }

            document.removeEventListener("fullscreenchange", onFullscreenChange);
        });

        // Any time the parent component signals us to change fullscreen mode
        // we need to attempt to match the state.
        watch(() => props.modelValue, () => {
            // Check if we are already in the correct state.
            if (!props.modelValue && fullscreenMode.value === FullscreenMode.None) {
                return;
            }
            else if (props.modelValue && fullscreenMode.value !== FullscreenMode.None) {
                return;
            }

            if (props.modelValue) {
                enterFullscreen();
            }
            else {
                exitFullscreen();
            }
        });

        // Any time our fullscreen mode changes, make sure the parent knows.
        watch(fullscreenMode, () => {
            emit("update:modelValue", fullscreenMode.value !== FullscreenMode.None);
        });

        return {
            containerElement,
            containerStyle,
            teleportDisabled
        };
    },

    template: `
<teleport to="body" :disabled="teleportDisabled">
    <div ref="containerElement" :style="containerStyle">
        <slot />
    </div>
</teleport>
`
});

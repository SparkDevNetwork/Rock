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
import { useVModelPassthrough } from "../Util/component";
import RockButton from "../Elements/rockButton";
import Fullscreen from "../Elements/fullscreen";
import TransitionVerticalCollapse from "../Elements/transitionVerticalCollapse";

export default defineComponent({
    name: "Panel",

    components: {
        Fullscreen,
        RockButton,
        TransitionVerticalCollapse
    },

    // We manually bind the attributes to a child element.
    inheritAttrs: false,

    props: {
        /** True if the panel content is shown when hasCollapse is also true. */
        modelValue: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        /** True if the draw should be open and its content displayed. */
        isDrawerOpen: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        /** True if the panel should be in full screen mode. */
        isFullscreen: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        /** True if the panel is collapsable and shows the collapse button. */
        hasCollapse: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        /** True if the panel can go into full screen mode. */
        hasFullscreen: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        /** True if the panel should use in-page full screen mode. */
        isFullscreenPageOnly: {
            type: Boolean as PropType<boolean>,
            default: true
        },

        /** The type of panel to render. */
        type: {
            type: String as PropType<"default" | "block" | "default" | "primary" | "success" | "info" | "warning" | "danger">,
            default: "default"
        },

        /** The title text to display in the panel, will be overridden by slot usage. */
        title: {
            type: String as PropType<string>,
            default: ""
        },

        /** The Icon CSS class to display in the title, will be overridden by slot usage. */
        titleIconClass: {
            type: String as PropType<string>,
            default: ""
        }
    },

    emits: [
        "update:modelValue",
        "update:isDrawerOpen",
        "update:isFullscreen"
    ],

    setup(props, { emit }) {
        /** The internal state of the collapsable panel content. */
        const internalValue = useVModelPassthrough(props, "modelValue", emit);

        /** True if the draw content should be shown. */
        const isDrawerOpen = useVModelPassthrough(props, "isDrawerOpen", emit);

        /** True if the panel should be shown in full screen mode. */
        const isFullscreen = useVModelPassthrough(props, "isFullscreen", emit);

        /** The HTML Element that is the main panel div. */
        const panelElement = ref<HTMLElement | null>(null);

        /** True if the collapse action should be shown. */
        const hasCollapseAction = computed((): boolean => props.hasCollapse && !isFullscreen.value);

        /** The CSS class names to be applied to the panel. */
        const panelClass = computed((): string[] => {
            const classes = ["panel", "panel-flex"];

            classes.push(`panel-${props.type}`);

            if (isFullscreen.value) {
                classes.push("panel-fullscreen");
            }

            return classes;
        });

        /** The CSS class names to be applied to the panel heading. */
        const panelHeadingClass = computed((): string[] => {
            const classes = ["panel-heading"];

            if (props.hasCollapse) {
                classes.push("clickable");
            }

            return classes;
        });

        /** The tab index for the panel, this allows us to catch the escape key. */
        const panelTabIndex = computed((): string | undefined => isFullscreen.value ? "0" : undefined);

        /** True if the panel body should be displayed. */
        const isPanelOpen = computed((): boolean => !props.hasCollapse || internalValue.value !== false || isFullscreen.value);

        /** Event handler when the drawer expander is clicked. */
        const onDrawerPullClick = (): void => {
            isDrawerOpen.value = !isDrawerOpen.value;
        };

        /** Event handler when the panel heading is clicked. */
        const onPanelHeadingClick = (): void => {
            if (props.hasCollapse) {
                internalValue.value = !isPanelOpen.value;
            }
        };

        /**
         * Event handler for when a key is pressed down inside the panel.
         * 
         * @param ev The event that describes which key was pressed.
         */
        const onPanelKeyDown = (ev: KeyboardEvent): void => {
            if (isFullscreen.value && ev.keyCode === 27) {
                ev.stopImmediatePropagation();

                isFullscreen.value = false;
            }
        };

        /** Event handler when the full-screen button is clicked. */
        const onFullscreenClick = (): void => {
            if (props.hasFullscreen) {
                isFullscreen.value = !isFullscreen.value;
            }
        };

        // Watches for changes to our full screen status and responds accordingly.
        watch(isFullscreen, () => {
            // If we have entered full screen then wait for the UI to update
            // and set focus on the panel. This allows the escape key to work.
            if (isFullscreen.value) {
                nextTick(() => panelElement.value?.focus());
            }
        });

        return {
            hasCollapseAction,
            isDrawerOpen,
            isFullscreen,
            isPanelOpen,
            onDrawerPullClick,
            onFullscreenClick,
            onPanelHeadingClick,
            onPanelKeyDown,
            panelClass,
            panelElement,
            panelHeadingClass,
            panelTabIndex
        };
    },

    template: `
<Fullscreen v-model="isFullscreen" :isPageOnly="isFullscreenPageOnly">
    <div :class="panelClass" ref="panelElement" v-bind="$attrs" :tabIndex="panelTabIndex" @keydown="onPanelKeyDown">
        <v-style>
            .panel.panel-flex {
                display: flex;
                flex-direction: column;
            }

            .panel.panel-flex > .panel-heading {
                display: flex;
                align-items: center;
                padding: 0;
                line-height: 1em;
                min-height: 48px;
            }

            .panel.panel-flex > .panel-heading > .panel-title {
                padding: 0px 24px;
                flex-grow: 1;
            }

            .panel.panel-flex > .panel-heading > .panel-aside {
                padding: 0px 24px 0px 0px;
            }

            .panel.panel-flex > .panel-heading > .panel-action {
                display: flex;
                border-left: 1px solid #ccc;
                align-self: stretch;
                align-items: center;
                width: 48px;
                justify-content: center;
                cursor: pointer;
            }

            .panel.panel-fullscreen {
                margin: 0px;
                position: absolute;
                left: 0;
                top: 0;
                width: 100vw;
                height: 100vh;
            }

            .panel.panel-fullscreen,
            .panel.panel-fullscreen > .panel-heading {
                border-radius: 0px;
            }

            .panel.panel-flex.panel-fullscreen > .panel-body {
                flex-grow: 1;
                position: relative;
                overflow-y: auto;
            }

            .page-fullscreen-capable .panel.panel-block.panel-flex {
                overflow-y: hidden;
            }

            .page-fullscreen-capable .panel.panel-flex.panel-block > .panel-body {
                position: relative;
            }
        </v-style>

        <div :class="panelHeadingClass" @click="onPanelHeadingClick">
            <h1 class="panel-title">
                <slot v-if="$slots.title" name="title" />
                <template v-else>
                    <i v-if="titleIconClass" :class="titleIconClass"></i>
                    {{ title }}
                </template>
            </h1>

            <div class="panel-aside">
                <slot name="titleAside" />

                <template v-if="hasCollapseAction">
                    <i v-if="isPanelOpen" class="fa fa-chevron-up fa-xs ml-2"></i>
                    <i v-else class="fa fa-chevron-down fa-xs ml-2"></i>
                </template>
            </div>

            <slot name="actionAside" />

            <span v-if="hasFullscreen" class="panel-action" @click.prevent.stop="onFullscreenClick">
                <i class="fa fa-expand"></i>
            </span>
        </div>

        <div v-if="$slots.drawer" class="panel-drawer rock-panel-drawer" :class="isDrawerOpen ? 'open' : ''">
            <TransitionVerticalCollapse>
                <div v-show="isDrawerOpen" class="drawer-content">
                    <slot name="drawer" />
                </div>
            </TransitionVerticalCollapse>

            <div class="drawer-pull" @click="onDrawerPullClick">
                <i :class="isDrawerOpen ? 'fa fa-chevron-up fa-xs' : 'fa fa-chevron-down fa-xs'"></i>
            </div>
        </div>

        <TransitionVerticalCollapse>
            <div v-show="isPanelOpen" class="panel-body">
                <slot />
            </div>
        </TransitionVerticalCollapse>
    </div>
</Fullscreen>
`
});

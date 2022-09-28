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
import { useVModelPassthrough } from "@Obsidian/Utility/component";
import RockButton from "./rockButton";
import Fullscreen from "./fullscreen";
import TransitionVerticalCollapse from "./transitionVerticalCollapse";
import { PanelAction } from "@Obsidian/Types/Controls/panelAction";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
declare function $(element: any): any;

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
            default: false
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
        titleIconCssClass: {
            type: String as PropType<string>,
            default: ""
        },

        /** A list of action items to be included in the ellipsis. */
        headerSecondaryActions: {
            type: Array as PropType<PanelAction[]>,
            required: false
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

        const hasHeaderSecondaryActions = computed((): boolean => !!props.headerSecondaryActions && props.headerSecondaryActions.length > 0);
        const isHelpOpen = ref(false);
        const headerSecondaryActionMenu = ref<HTMLElement | null>(null);

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
                classes.push("cursor-pointer");
            }

            return classes;
        });

        /** The tab index for the panel, this allows us to catch the escape key. */
        const panelTabIndex = computed((): string | undefined => isFullscreen.value ? "0" : undefined);

        /** True if the panel body should be displayed. */
        const isPanelOpen = computed((): boolean => !props.hasCollapse || internalValue.value !== false || isFullscreen.value);

        const getHeaderSecondaryActionIconClass = (action: PanelAction): string => {
            if (action.iconCssClass) {
                let iconClass = action.iconCssClass;

                if (action.type !== "default" && action.type !== "link") {
                    iconClass += ` text-${action.type}`;
                }

                return iconClass;
            }
            else {
                return "";
            }
        };

        const getHeaderSecondaryActionItemClass = (action: PanelAction): string => {
            return action.disabled ? "disabled" : "";
        };

        const onIgnoreClick = (): void => { /* Intentionally blank to ignore click. */ };

        /** Event handler when the drawer expander is clicked. */
        const onDrawerPullClick = (): void => {
            isDrawerOpen.value = !isDrawerOpen.value;
        };

        const onHelpClick = (): void => {
            isHelpOpen.value = !isHelpOpen.value;
        };

        /** Event handler when the panel heading is clicked. */
        const onPanelHeadingClick = (): void => {
            if (props.hasCollapse) {
                internalValue.value = !isPanelOpen.value;
            }
        };

        const onPanelExpandClick = (): void => {
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

        /** Event handler for when a secondary action is clicked. */
        const onActionClick = (action: PanelAction, event: Event): void => {
            if (action.disabled) {
                return;
            }

            // Close the drop down since we are hijacking the click event.
            if (headerSecondaryActionMenu.value) {
                $(headerSecondaryActionMenu.value).dropdown("toggle");
            }

            if (action.handler) {
                action.handler(event);
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

        watch(headerSecondaryActionMenu, () => {
            if (headerSecondaryActionMenu.value) {
                $(headerSecondaryActionMenu.value).dropdown();
            }
        });

        return {
            getHeaderSecondaryActionIconClass,
            getHeaderSecondaryActionItemClass,
            hasCollapseAction,
            hasHeaderSecondaryActions,
            headerSecondaryActionMenu,
            isDrawerOpen,
            isFullscreen,
            isHelpOpen,
            isPanelOpen,
            onActionClick,
            onDrawerPullClick,
            onFullscreenClick,
            onHelpClick,
            onIgnoreClick,
            onPanelExpandClick,
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

        <div :class="panelHeadingClass" @click="onPanelHeadingClick">
            <h1 class="panel-title">
                <slot v-if="$slots.title" name="title" />
                <template v-else>
                    <i v-if="titleIconCssClass" :class="titleIconCssClass"></i>
                    {{ title }}
                </template>
            </h1>

            <div class="panel-header-actions" @click.prevent.stop="onIgnoreClick">
                <slot name="headerActions" />

                <span v-if="$slots.helpContent" class="action clickable" @click="onHelpClick">
                    <i class="fa fa-question"></i>
                </span>

                <span v-if="hasFullscreen" class="action clickable" @click="onFullscreenClick">
                    <i class="fa fa-expand"></i>
                </span>

                <template v-if="hasHeaderSecondaryActions">
                    <span class="action clickable" style="position: relative;">
                        <i class="fa fa-ellipsis-v" data-toggle="dropdown" ref="headerSecondaryActionMenu"></i>
                        <ul class="dropdown-menu dropdown-menu-right">
                            <li v-for="action in headerSecondaryActions" :class="getHeaderSecondaryActionItemClass(action)">
                                <a href="#" @click.prevent.stop="onActionClick(action, $event)">
                                    <i :class="getHeaderSecondaryActionIconClass(action)"></i>
                                    {{ action.title }}
                                </a>
                            </li>
                        </ul>
                    </span>
                </template>

                <span v-if="hasCollapseAction" class="action clickable" @click="onPanelExpandClick">
                    <i v-if="isPanelOpen" class="fa fa-chevron-up"></i>
                    <i v-else class="fa fa-chevron-down"></i>
                </span>
            </div>
        </div>

        <div v-if="$slots.subheaderLeft || $slots.subheaderRight" class="panel-sub-header">
            <div class="panel-sub-header-left">
                <slot name="subheaderLeft" />
            </div>

            <div class="panel-sub-header-right">
                <slot name="subheaderRight" />
            </div>
        </div>

        <div v-if="$slots.helpContent" class="panel-help">
            <TransitionVerticalCollapse>
                <div v-show="isHelpOpen" class="help-content">
                    <slot name="helpContent" />
                </div>
            </TransitionVerticalCollapse>
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

                <div v-if="$slots.footerActions || $slots.footerSecondaryActions" class="actions">
                    <div class="footer-actions">
                        <slot name="footerActions" />
                    </div>

                    <div class="footer-secondary-actions">
                        <slot name="footerSecondaryActions" />
                    </div>
                </div>
            </div>
        </TransitionVerticalCollapse>
    </div>
</Fullscreen>
`
});

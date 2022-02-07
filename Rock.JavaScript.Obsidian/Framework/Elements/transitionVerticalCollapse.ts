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
import { defineComponent } from "vue";

type CollapseState = {
    display: string;
    computedPaddingTop: string;
    computedPaddingBottom: string;
};

// This provides animations for doing vertical collapses, such as an expander.
// It will not work correctly if your element has padding-top, padding-bottom
// or height values set in the style tag.
export default defineComponent({
    setup() {
        /**
         * Called before the element begins to enter the DOM.
         * 
         * @param element The element that will be entering the DOM.
         */
        const beforeEnter = (element: HTMLElement): void => {
            // Save any values that will be used during the animation.
            const state: CollapseState = {
                display: element.style.display,
                computedPaddingTop: getComputedStyle(element).paddingTop,
                computedPaddingBottom: getComputedStyle(element).paddingBottom
            };

            element.dataset.transitionCollapseState = JSON.stringify(state);

            // Reset all the styles we will be transitioning unless they already
            // have values (which probably means we are aborting an expand).
            if (!element.style.height) {
                element.style.height = "0px";
            }

            if (!element.style.paddingTop) {
                element.style.paddingTop = "0px";
            }

            if (!element.style.paddingBottom) {
                element.style.paddingBottom = "0px";
            }

            element.style.display = "";
        };

        /**
         * Called when the element has entered the DOM.
         * 
         * @param element The element that has entered the DOM.
         */
        const enter = (element: HTMLElement): void => {
            // Set values that will cause the vertical space to expand.
            requestAnimationFrame(() => {
                const state = JSON.parse(element.dataset.transitionCollapseState ?? "") as CollapseState;
                const verticalPadding = (parseInt(state.computedPaddingTop) || 0) + (parseInt(state.computedPaddingBottom) || 0);

                element.style.height = `${element.scrollHeight + verticalPadding}px`;
                element.style.paddingTop = state.computedPaddingTop;
                element.style.paddingBottom = state.computedPaddingBottom;
            });
        };

        /**
         * Called after the element has entered the DOM and the animation has completed.
         * 
         * @param element The element that entered the DOM.
         */
        const afterEnter = (element: HTMLElement): void => {
            const state = JSON.parse(element.dataset.transitionCollapseState ?? "") as CollapseState;

            // Reset all the explicit styles so they go back to implicit values.
            element.style.height = "";
            element.style.paddingTop = "";
            element.style.paddingBottom = "";
            element.style.display = state.display !== "none" ? state.display : "";

            delete element.dataset.transitionCollapseState;
        };

        /**
         * Called before the element begins to leave the DOM.
         *
         * @param element The element that will be leaving the DOM.
         */
        const beforeLeave = (element: HTMLElement): void => {
            // Set the height explicitely so the CSS animation will trigger.
            element.style.height = `${element.offsetHeight}px`;
        };

        /**
         * Called when the element should begin animation for leaving the DOM.
         *
         * @param element The element that is leaving the DOM.
         */
        const leave = (element: HTMLElement): void => {
            // Set values that will cause the vertical space to collapse.
            requestAnimationFrame(() => {
                element.style.height = "0px";
                element.style.paddingTop = "0px";
                element.style.paddingBottom = "0px";
            });
        };

        /**
         * Called after the element has left the DOM and the animation has completed.
         *
         * @param element The element that left the DOM.
         */
        const afterLeave = (element: HTMLElement): void => {
            // Reset all the explicit styles so they go back to implicit values.
            element.style.height = "";
            element.style.paddingTop = "";
            element.style.paddingBottom = "";
        };

        return {
            afterEnter,
            afterLeave,
            beforeEnter,
            beforeLeave,
            enter,
            leave,
        };
    },

    template: `
    <v-style>
        .vertical-collapse-enter-active,
        .vertical-collapse-leave-active {
            overflow: hidden;
            transition-property: height, padding-top, padding-bottom;
            transition-duration: 0.35s;
            transition-timing-function: ease-in-out;
        }
    </v-style>
<transition
    enter-active-class="vertical-collapse-enter-active"
    leave-active-class="vertical-collapse-leave-active"
    @before-enter="beforeEnter"
    @enter="enter"
    @after-enter="afterEnter"
    @before-leave="beforeLeave"
    @leave="leave"
    @after-leave="afterLeave">
    <slot />
</transition>
`
});


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
import { computed, defineComponent, PropType, ref, watch, watchEffect } from "vue";
import RockFormField from "./rockFormField";

/**
 * A component that provides a rating picker for the user to specify their
 * rating of something.
 */
export default defineComponent({
    name: "Rating",

    components: {
        RockFormField
    },

    /** Defines the read-only properties that are provided by the parent component. */
    props: {
        /** The value provided to use from the parent component. */
        modelValue: {
            type: Number as PropType<number>,
            default: 0
        },

        /** The maximum rating value allowed, this is the number of starts displayed. */
        maxRating: {
            type: Number as PropType<number>,
            default: 5
        }
    },

    setup(props, { emit }) {
        /** The current value selected by the person. */
        const internalValue = ref(props.modelValue);

        /**
         * The current value being hovered by the person or null if no
         * hover operation is happening.
         */
        const hoverValue = ref(null as number | null);

        /** True if the clear button should be visible. */
        const showClear = computed((): boolean => internalValue.value > 0);

        /** Watch for changes in the value we are supposed to be editing. */
        watch(() => props.modelValue, () => internalValue.value = props.modelValue);

        /** Watch for changes in our internal value and emit the new value. */
        watchEffect(() => emit("update:modelValue", internalValue.value));

        /**
         * Set the rating value from an action.
         *
         * @param value The new rating value.
         */
        const setRating = (value: number): void => {
            internalValue.value = value;
        };

        /**
         * Handles the clear selection event from the person.
         * 
         * @param e The event that triggered this handler.
         * 
         * @returns A value indicating if the event has been handled.
         */
        const onClear = (e: Event): boolean => {
            e.preventDefault();

            setRating(0);

            return false;
        };

        /**
         * Gets the CSS class to use for the given rating position.
         * 
         * @param position The rating position being queried.
         */
        const classForRating = (position: number): string => {
            const filledCount = Math.min(props.maxRating, hoverValue.value ?? internalValue.value);

            return position <= filledCount ? "fa fa-rating-selected" : "fa fa-rating-unselected";
        };

        /**
         * Sets the current rating position being hovered.
         * 
         * @param position The position being hovered.
         */
        const setHover = (position: number): void => {
            hoverValue.value = position;
        };

        /**
         * Clears any hover rating position value.
         */
        const clearHover = (): void => {
            hoverValue.value = null;
        };

        return {
            classForRating,
            clearHover,
            hoverValue,
            internalValue,
            onClear,
            setHover,
            setRating,
            showClear
        };
    },

    template: `
<RockFormField
    :modelValue="internalValue"
    formGroupClasses="rock-rating"
    name="rock-rating">
    <template #default="{uniqueId, field}">
        <div class="control-wrapper">
            <div class="rating-input">
                <i v-for="i in maxRating" :key="i" :class="classForRating(i)" @click="setRating(i)" @mouseover="setHover(i)" @mouseleave="clearHover()"></i>
                <a v-if="showClear" class="clear-rating" href="#" v-on:click="onClear" @mouseover="setHover(0)" @mouseleave="clearHover()">
                    <span class="fa fa-remove"></span>
                </a>
            </div>
        </div>
    </template>
</RockFormField>
`
});

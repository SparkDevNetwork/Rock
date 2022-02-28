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
import { computed, defineComponent, PropType, ref } from "vue";
import { useVModelPassthrough } from "../Util/component";

export default defineComponent({
    name: "InlineSlider",

    components: {
    },

    props: {
        modelValue: {
            type: Number as PropType<number>,
            default: 0
        },

        isIntegerOnly: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        min: {
            type: Number as PropType<number>,
            default: 0
        },

        max: {
            type: Number as PropType<number>,
            default: 100
        },

        showValueBar: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        /** Contains our current value reflected in the UI. */
        const internalValue = useVModelPassthrough(props, "modelValue", emit);

        /** The slider track HTML Element in the DOM. */
        const sliderElement = ref<HTMLElement | null>(null);

        /** The computed styles for the thumb knob. */
        const thumbStyle = computed((): Record<string, string> => {
            return {
                left: `${percentValue.value * 100}%`
            };
        });

        /** The percentage representation of the value, between 0 and 1. */
        const percentValue = computed((): number => {
            if (props.min <= props.max) {
                return (internalValue.value - props.min) / (props.max - props.min);
            }

            return 0;
        });

        /** The computed styles for the left slider track. */
        const leftSliderStyle = computed((): Record<string, string> => {
            const value = Math.round(percentValue.value * 10000);

            return {
                flexBasis: `${value / 100}%`
            };
        });

        /** The computed styles for the right slider track. */
        const rightSliderStyle = computed((): Record<string, string> => {
            const value = Math.round(percentValue.value * 10000);

            return {
                flexBasis: `${100 - (value / 100)}%`
            };
        });

        /** True if the minimum value should be shown in the value bar. */
        const showMinValue = computed((): boolean => percentValue.value >= 0.1);

        /** True if the maximum value should be shown in the value bar. */
        const showMaxValue = computed((): boolean => percentValue.value <= 0.9);

        /**
         * Constrains the value to ensure it is valid for our configuration.
         * 
         * @param value The value to be constrained.
         *
         * @returns The value after any constraints have been applied.
         */
        const constrainValue = (value: number): number => {
            if (props.isIntegerOnly) {
                value = Math.round(value);
            }

            if (value < props.min) {
                value = props.min;
            }
            else if (value > props.max) {
                value = props.max;
            }

            return value;
        };

        /**
         * Constrains the internalValue property and if it needs to change then
         * it is updated automatically.
         */
        const constrainInternalValueAndUpdate = (): void => {
            const value = constrainValue(internalValue.value);

            if (value !== internalValue.value) {
                internalValue.value = value;
            }
        };

        /**
         * Calculates the new value from the drag position in the DOM.
         * 
         * @param clientX The position of the event in the DOM.
         */
        const calculateDragValue = (clientX: number): void => {
            if (sliderElement.value) {
                const rect = sliderElement.value.getBoundingClientRect();
                const xPosition = clientX - rect.left;
                const xConstrained = Math.min(Math.max(xPosition, 0), rect.width);
                const percent = xConstrained / rect.width;
                const valueRange = props.max - props.min;

                const value = constrainValue((valueRange * percent) + props.min);

                if (value !== internalValue.value) {
                    internalValue.value = value;
                }
            }
        };

        /**
         * Event handler for when a mouse button is pressed down.
         * 
         * @param ev The event that was triggered.
         */
        const onMouseDown = (ev: MouseEvent): void => {
            ev.preventDefault();
            ev.stopPropagation();

            if (ev.button === 0) {
                calculateDragValue(ev.clientX);
                window.addEventListener("mousemove", onMouseMove);
                window.addEventListener("mouseup", onMouseUp);
            }
        };

        /**
         * Event handler for when a finger has touched the display.
         * 
         * @param ev The event that was triggered.
         */
        const onTouchDown = (ev: TouchEvent): void => {
            ev.preventDefault();
            ev.stopPropagation();

            if (ev.touches.length === 1) {
                calculateDragValue(ev.touches[0].clientX);
                window.addEventListener("touchmove", onTouchMove);
                window.addEventListener("touchup", onTouchUp);
            }
        };

        /**
         * Event handler for when the mouse has moved while we are tracking
         * the position.
         *
         * @param ev The event that was triggered.
         */
        const onMouseMove = (ev: MouseEvent): void => {
            calculateDragValue(ev.clientX);
        };

        /**
         * Event handler for when as mouse button has been released while we
         * are tracking the position.
         */
        const onMouseUp = (): void => {
            window.removeEventListener("mousemove", onMouseMove);
            window.removeEventListener("mouseup", onMouseUp);
        };

        /**
         * Event handler for when a touch has moved while we are tracking the
         * position.
         * 
         * @param ev The event that was triggered.
         */
        const onTouchMove = (ev: TouchEvent): void => {
            calculateDragValue(ev.touches[0].clientX);
        };

        /**
         * Event handler for when a touch has been lifted off the display while
         * we are tracking the position.
         */
        const onTouchUp = (): void => {
            window.removeEventListener("touchmove", onTouchMove);
            window.removeEventListener("touchup", onTouchUp);
        };

        // Force the internal value to be constrained and update if necessary.
        constrainInternalValueAndUpdate();

        return {
            internalValue,
            leftSliderStyle,
            onMouseDown,
            onTouchDown,
            rightSliderStyle,
            sliderElement,
            showMaxValue,
            showMinValue,
            thumbStyle
        };
    },

    template: `
<div style="margin-left: calc(var(--slider-handle-height) / 2); margin-right: calc(var(--slider-handle-height) / 2);">
    <div v-if="showValueBar" class="d-flex" style="position: relative; margin-bottom: 3px;">
        <span v-if="showMinValue" class="text-muted">{{ min }}</span>
        <span style="flex-grow: 1"></span>
        <span v-if="showMaxValue" class="text-muted">{{ max }}</span>

        <span :style="thumbStyle" style="position: absolute;">
            <span style="background: var(--slider-progress-bg); border-radius: 3px; padding: 1px 5px; color: #fff; font-size: 14px; margin-left: -50%; display: block; margin-right: 50%;">
                {{ internalValue }}
            </span>
        </span>
    </div>

    <div ref="sliderElement" class="d-flex" style="height: var(--slider-handle-height); align-items: center; position: relative;" @mousedown="onMouseDown" @touchdown="onTouchDown">
        <span :style="leftSliderStyle" style="background-color: var(--slider-progress-bg); height: var(--slider-height); border-top-left-radius: calc(var(--slider-height) / 2); border-bottom-left-radius: calc(var(--slider-height) / 2); flex-grow: 1;"></span>
        <span :style="rightSliderStyle" style="background-color: var(--slider-bg); height: var(--slider-height); border-top-right-radius: calc(var(--slider-height) / 2); border-bottom-right-radius: calc(var(--slider-height) / 2); flex-grow: 1;"></span>

        <span :style="thumbStyle" style="position: absolute; width: var(--slider-handle-height); height: var(--slider-handle-height); margin-left: calc(0px - calc(var(--slider-handle-height) / 2)); cursor: pointer; background: var(--slider-handle-bg); border: 1px solid var(--slider-handle-border-color); border-radius: var(--slider-handle-height);" @mousedown="onMouseDown" @mousemove="onMouseMove" @mouseup="onMouseUp"></span>
    </div>
</div>
`
});

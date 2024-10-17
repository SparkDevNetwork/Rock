import ShorthandPropertyBase from "./shorthandPropertyBase.partial.obs";
import { computed, ComputedRef, ExtractPropTypes, PropType, reactive, Ref, toRefs, unref, watch, WritableComputedRef } from "vue";
import { DirectionalConstituentProperty, StandardShorthandProps, StandardLengthProps } from "./types.partial";

type ReactiveShorthandProperties = {
    shorthand: WritableComputedRef<string>;
    top: WritableComputedRef<string>;
    bottom: WritableComputedRef<string>;
    right: WritableComputedRef<string>;
    left: WritableComputedRef<string>;
};

export function useReactiveShorthandProperties(component: Ref<InstanceType<typeof ShorthandPropertyBase> | undefined>): ReactiveShorthandProperties {
    const shorthand = computed<string>({
        get(): string {
            return component.value?.shorthand ?? "";
        },
        set(value: string): void {
            if (component.value) {
                component.value.shorthand = value;
            }
        }
    });
    const top = computed<string>({
        get(): string {
            return component.value?.top ?? "";
        },
        set(value: string): void {
            if (component.value) {
                component.value.top = value;
            }
        }
    });
    const bottom = computed<string>({
        get(): string {
            return component.value?.bottom ?? "";
        },
        set(value: string): void {
            if (component.value) {
                component.value.bottom = value;
            }
        }
    });
    const right = computed<string>({
        get(): string {
            return component.value?.right ?? "";
        },
        set(value: string): void {
            if (component.value) {
                component.value.right = value;
            }
        }
    });
    const left = computed<string>({
        get(): string {
            return component.value?.left ?? "";
        },
        set(value: string): void {
            if (component.value) {
                component.value.left = value;
            }
        }
    });

    return {
        shorthand,
        top,
        bottom,
        right,
        left
    };
}

/** The standard component props that should be included when using RockFormField. */
export const standardLengthProps: StandardLengthProps = {
    /**
     * Used as both the shorthand property label and the group label for constituent properties.
     *
     * Set to "" to hide this label.
     */
    label: {
        type: String as PropType<string>,
        required: true
    },

    min: {
        type: Number as PropType<number>,
        default: 0 as const
    },

    max: {
        type: Number as PropType<number>,
        default: 99 as const
    },

    mode: {
        type: String as PropType<"numberUpDown" | "numberBox" | "textBox">,
        default: "numberUpDown" as const
    }
};

/** The standard component props that should be included when using RockFormField. */
export const standardShorthandProps: StandardShorthandProps = {
    element: {
        type: Object as PropType<HTMLElement>,
        required: true
    },

    label: {
        type: String as PropType<string>,
        required: true
    },

    labelTop: {
        type: String as PropType<string>,
        default: "Top" as const
    },

    labelBottom: {
        type: String as PropType<string>,
        default: "Bottom" as const
    },

    labelRight: {
        type: String as PropType<string>,
        default: "Right" as const
    },

    labelLeft: {
        type: String as PropType<string>,
        default: "Left" as const
    },

    showConstituentProperties: {
        type: Boolean as PropType<boolean | DirectionalConstituentProperty | DirectionalConstituentProperty[]>,
        default: false as const
    }
};

/**
 * Configures the basic properties that should be passed to the ShorthandPropertyBase
 * component. The value returned by this function should be used with v-bind on
 * the ShorthandPropertyBase in order to pass all the defined prop values to it.
 *
 * @param props The props of the component that will be using the ShorthandPropertyBase.
 *
 * @returns An object of prop values that can be used with v-bind.
 */
export function useStandardShorthandProps(props: ExtractPropTypes<StandardShorthandProps>): ExtractPropTypes<StandardShorthandProps> {
    const propValues = reactive<ExtractPropTypes<StandardShorthandProps>>({
        label: props.label,
        labelBottom: props.labelBottom,
        labelLeft: props.labelLeft,
        labelRight: props.labelRight,
        labelTop: props.labelTop,
        showConstituentProperties: props.showConstituentProperties,
        element: props.element,
    });

    watch(() => props.label, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.label = value;
        }
    });

    watch(() => props.labelBottom, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.labelBottom = value;
        }
    });

    watch(() => props.labelLeft, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.labelLeft = value;
        }
    });

    watch(() => props.labelRight, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.labelRight = value;
        }
    });

    watch(() => props.labelTop, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.labelTop = value;
        }
    });

    watch(() => props.showConstituentProperties, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.showConstituentProperties = value;
        }
    });

    watch(() => props.element, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.element = value;
        }
    });

    return propValues;
}

/**
 * Configures the basic properties that should be passed to the LengthTypePropertyBase
 * component. The value returned by this function should be used with v-bind on
 * the LengthTypePropertyBase in order to pass all the defined prop values to it.
 *
 * @param props The props of the component that will be using the LengthTypePropertyBase.
 *
 * @returns An object of prop values that can be used with v-bind.
 */
export function useStandardLengthProps(props: ExtractPropTypes<StandardLengthProps>): ExtractPropTypes<StandardLengthProps> {
    const propValues = reactive<ExtractPropTypes<StandardLengthProps>>({
        min: props.min,
        max: props.max,
        mode: props.mode,
        label: props.label,
    });

    watch(() => props.label, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.label = value;
        }
    });

    watch(() => props.max, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.max = value;
        }
    });

    watch(() => props.min, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.min = value;
        }
    });

    watch(() => props.mode, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.mode = value;
        }
    });

    return propValues;
}
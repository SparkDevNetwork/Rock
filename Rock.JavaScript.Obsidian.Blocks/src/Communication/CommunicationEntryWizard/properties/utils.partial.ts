import DirectionalShorthandPropertyBase from "./directionalShorthandPropertyBase.partial.obs";
import { computed, ExtractPropTypes, PropType, reactive, Ref, WritableComputedRef } from "vue";
import { DirectionalConstituentProperty, StandardDirectionalShorthandProps, StandardLengthProps } from "./types.partial";

type ReactiveDirectionalShorthandProperties = {
    shorthand: WritableComputedRef<string>;
    top: WritableComputedRef<string>;
    bottom: WritableComputedRef<string>;
    right: WritableComputedRef<string>;
    left: WritableComputedRef<string>;
};

export function useReactiveDirectionalShorthandProperties(component: Ref<InstanceType<typeof DirectionalShorthandPropertyBase> | undefined>): ReactiveDirectionalShorthandProperties {
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
    element: {
        type: Object as PropType<HTMLElement>,
        required: true
    },

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
export const standardDirectionalShorthandProps: StandardDirectionalShorthandProps = {
    element: {
        type: Object as PropType<HTMLElement>,
        required: true
    },

    /**
     * Used as both the shorthand property label and the group label for constituent properties.
     *
     * Set to "" to hide this label.
     */
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

    min: {
        type: Number as PropType<number>,
        default: 0 as const
    },

    max: {
        type: Number as PropType<number>,
        default: 99 as const
    },

    showConstituentProperties: {
        type: Boolean as PropType<boolean | DirectionalConstituentProperty | DirectionalConstituentProperty[]>,
        default: false as const
    },

    mode: {
        type: String as PropType<"numberUpDown" | "numberBox" | "textBox">,
        default: "numberUpDown" as const
    }
};

/**
 * Configures the basic properties that should be passed to the DirectionalShorthandPropertyBase
 * component. The value returned by this function should be used with v-bind on
 * the DirectionalShorthandPropertyBase in order to pass all the defined prop values to it.
 *
 * @param props The props of the component that will be using the DirectionalShorthandPropertyBase.
 *
 * @returns An object of prop values that can be used with v-bind.
 */
export function useStandardDirectionalShorthandProps(props: ExtractPropTypes<StandardDirectionalShorthandProps>): ExtractPropTypes<StandardDirectionalShorthandProps> {
    const propValues = reactive<ExtractPropTypes<StandardDirectionalShorthandProps>>({
        label: props.label,
        labelBottom: props.labelBottom,
        labelLeft: props.labelLeft,
        labelRight: props.labelRight,
        labelTop: props.labelTop,
        max: props.max,
        min: props.min,
        mode: props.mode,
        showConstituentProperties: props.showConstituentProperties,
        element: props.element,
    });

    // watch([() => props.formGroupClasses, () => props.help, () => props.label, () => props.rules, () => props.validationTitle], () => {
    //     copyStandardRockFormFieldProps(props, propValues);
    // });

    return propValues;
}

/**
 * Configures the basic properties that should be passed to the LengthPropertyBase
 * component. The value returned by this function should be used with v-bind on
 * the LengthPropertyBase in order to pass all the defined prop values to it.
 *
 * @param props The props of the component that will be using the LengthPropertyBase.
 *
 * @returns An object of prop values that can be used with v-bind.
 */
export function useStandardLengthProps(props: ExtractPropTypes<StandardLengthProps>): ExtractPropTypes<StandardLengthProps> {
    const propValues = reactive<ExtractPropTypes<StandardLengthProps>>({
        label: props.label,
        max: props.max,
        min: props.min,
        mode: props.mode,
        element: props.element,
    });

    // watch([() => props.formGroupClasses, () => props.help, () => props.label, () => props.rules, () => props.validationTitle], () => {
    //     copyStandardRockFormFieldProps(props, propValues);
    // });

    return propValues;
}
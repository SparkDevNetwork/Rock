import ShorthandPropertyBase from "./shorthandPropertyBase.partial.obs";
import { computed, ExtractPropTypes, PropType, reactive, Ref, watch, WritableComputedRef } from "vue";
import { DirectionalConstituentProperty, StandardShorthandProps, StandardLengthProps, LengthControlType } from "./types.partial";

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
        required: true as const
    },

    mode: {
        type: Object as PropType<LengthControlType>,
        default: { type: "numberBox", min: 0, max: 99 } as const
    }
};

/** The standard component props that should be included when using RockFormField. */
export const standardShorthandProps: StandardShorthandProps = {
    element: {
        type: Object as PropType<HTMLElement>,
        required: true as const
    },

    label: {
        type: String as PropType<string>,
        required: true as const
    },

    labelShorthand: {
        type: String as PropType<string>,
        default: "" as const
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
    },

    canToggleShowConstituents: {
        type: Boolean as PropType<boolean>,
        default: true as const
    },

    cssClassShorthand: {
        type: String as PropType<string | undefined>
    },

    cssClassTop: {
        type: String as PropType<string | undefined>
    },

    cssClassBottom: {
        type: String as PropType<string | undefined>
    },

    cssClassRight: {
        type: String as PropType<string | undefined>
    },

    cssClassLeft: {
        type: String as PropType<string | undefined>
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
        element: props.element,
        label: props.label,
        labelShorthand: props.labelShorthand,
        labelTop: props.labelTop,
        labelBottom: props.labelBottom,
        labelRight: props.labelRight,
        labelLeft: props.labelLeft,
        showConstituentProperties: props.showConstituentProperties,
        canToggleShowConstituents: props.canToggleShowConstituents,
        cssClassShorthand: props.cssClassShorthand,
        cssClassTop: props.cssClassTop,
        cssClassBottom: props.cssClassBottom,
        cssClassRight: props.cssClassRight,
        cssClassLeft: props.cssClassLeft
    });

    watch(() => props.label, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.label = value;
        }
    });

    watch(() => props.labelShorthand, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.labelShorthand = value;
        }
    });

    watch(() => props.labelTop, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.labelTop = value;
        }
    });

    watch(() => props.labelBottom, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.labelBottom = value;
        }
    });

    watch(() => props.labelRight, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.labelRight = value;
        }
    });

    watch(() => props.labelLeft, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.labelLeft = value;
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

    watch(() => props.canToggleShowConstituents, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.canToggleShowConstituents = value;
        }
    });

    watch(() => props.cssClassShorthand, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.cssClassShorthand = value;
        }
    });

    watch(() => props.cssClassTop, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.cssClassTop = value;
        }
    });

    watch(() => props.cssClassBottom, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.cssClassBottom = value;
        }
    });

    watch(() => props.cssClassRight, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.cssClassRight = value;
        }
    });

    watch(() => props.cssClassLeft, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.cssClassLeft = value;
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
        mode: props.mode,
        label: props.label,
    });

    watch(() => props.label, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.label = value;
        }
    });

    watch(() => props.mode, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.mode = value;
        }
    });

    return propValues;
}

export function hasConstituentProperties(props: ExtractPropTypes<StandardShorthandProps> & { propertyTop: string; propertyBottom: string; propertyRight: string; propertyLeft: string; }): boolean | DirectionalConstituentProperty | DirectionalConstituentProperty[] {
    if (props.showConstituentProperties === false) {
        const top = props.element.style.getPropertyValue(props.propertyTop);
        const bottom = props.element.style.getPropertyValue(props.propertyBottom);
        const right = props.element.style.getPropertyValue(props.propertyRight);
        const left = props.element.style.getPropertyValue(props.propertyLeft);

        if (top || bottom || right || left) {
            return hasAnyDifference(top, bottom, right, left);
        }
    }

    return props.showConstituentProperties;
}

function hasAnyDifference(...strings: string[]): boolean {
    const firstString = strings[0];

    for (let i = 1; i < strings.length; i++) {
        if (strings[i] !== firstString) {
            return true; // Exit early if a difference is found
        }
    }

    return false; // No differences found
}
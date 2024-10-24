import { ExtractPropTypes, PropType, reactive, watch } from "vue";
import { DirectionalConstituentProperty, StandardShorthandProps, StandardLengthProps, LengthControlType, CSSStyleDeclarationKebabKey } from "./types.partial";

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
    mode: {
        type: String as PropType<"inline" | "stylesheet">,
        default: "inline" as const
    },

    element: {
        type: Object as PropType<HTMLElement | undefined>,
        required: false
    },

    styleSheetValues: {
        type: Object as PropType<Partial<Record<CSSStyleDeclarationKebabKey, string>>>,
        required: false
    },

    label: {
        type: String as PropType<string>,
        required: true as const
    },

    shorthandLabel: {
        type: String as PropType<string>,
        default: "" as const
    },

    shorthandCssClass: {
        type: String as PropType<string | undefined>
    },

    topLabel: {
        type: String as PropType<string>,
        default: "Top" as const
    },

    topCssClass: {
        type: String as PropType<string | undefined>
    },

    bottomLabel: {
        type: String as PropType<string>,
        default: "Bottom" as const
    },

    bottomCssClass: {
        type: String as PropType<string | undefined>
    },

    rightLabel: {
        type: String as PropType<string>,
        default: "Right" as const
    },

    rightCssClass: {
        type: String as PropType<string | undefined>
    },

    leftLabel: {
        type: String as PropType<string>,
        default: "Left" as const
    },

    leftCssClass: {
        type: String as PropType<string | undefined>
    },

    showConstituentProperties: {
        type: Boolean as PropType<boolean | DirectionalConstituentProperty | DirectionalConstituentProperty[]>,
        default: false as const
    },

    canToggleShowConstituents: {
        type: Boolean as PropType<boolean>,
        default: true as const
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
        mode: props.mode,
        element: props.element,
        styleSheetValues: props.styleSheetValues,
        label: props.label,
        shorthandLabel: props.shorthandLabel,
        shorthandCssClass: props.shorthandCssClass,
        topLabel: props.topLabel,
        topCssClass: props.topCssClass,
        bottomLabel: props.bottomLabel,
        bottomCssClass: props.bottomCssClass,
        rightLabel: props.rightLabel,
        rightCssClass: props.rightCssClass,
        leftLabel: props.leftLabel,
        leftCssClass: props.leftCssClass,
        showConstituentProperties: props.showConstituentProperties,
        canToggleShowConstituents: props.canToggleShowConstituents
    });

    watch(() => props.mode, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.mode = value;
        }
    });

    watch(() => props.element, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.element = value;
        }
    });

    watch(() => props.styleSheetValues, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.styleSheetValues = value;
        }
    });

    watch(() => props.label, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.label = value;
        }
    });

    watch(() => props.shorthandLabel, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.shorthandLabel = value;
        }
    });

    watch(() => props.shorthandCssClass, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.shorthandCssClass = value;
        }
    });

    watch(() => props.topLabel, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.topLabel = value;
        }
    });

    watch(() => props.topCssClass, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.topCssClass = value;
        }
    });

    watch(() => props.bottomLabel, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.bottomLabel = value;
        }
    });

    watch(() => props.bottomCssClass, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.bottomCssClass = value;
        }
    });

    watch(() => props.rightLabel, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.rightLabel = value;
        }
    });

    watch(() => props.rightCssClass, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.rightCssClass = value;
        }
    });

    watch(() => props.leftLabel, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.leftLabel = value;
        }
    });

    watch(() => props.leftCssClass, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.leftCssClass = value;
        }
    });

    watch(() => props.showConstituentProperties, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.showConstituentProperties = value;
        }
    });

    watch(() => props.canToggleShowConstituents, (value, oldValue) => {
        if (value !== oldValue) {
            propValues.canToggleShowConstituents = value;
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

export function hasConstituentProperties(props: ExtractPropTypes<StandardShorthandProps> & { top: string; bottom: string; right: string; left: string; }): boolean | DirectionalConstituentProperty | DirectionalConstituentProperty[] {
    if (props.showConstituentProperties === false) {
        const top = props.element?.style.getPropertyValue(props.top) ?? "";
        const bottom = props.element?.style.getPropertyValue(props.bottom) ?? "";
        const right = props.element?.style.getPropertyValue(props.right) ?? "";
        const left = props.element?.style.getPropertyValue(props.left) ?? "";

        if (top || bottom || right || left) {
            return hasAnyDifference(top, bottom, right, left);
        }
    }

    return props.showConstituentProperties;
}

export function hasAnyDifference(...strings: string[]): boolean {
    const firstString = strings[0];

    for (let i = 1; i < strings.length; i++) {
        if (strings[i] !== firstString) {
            return true; // Exit early if a difference is found
        }
    }

    return false; // No differences found
}
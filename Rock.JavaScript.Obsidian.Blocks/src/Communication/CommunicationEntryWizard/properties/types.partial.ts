import { PropType } from "vue";

export type BorderStyle = "solid" | "dashed" | "dotted" | "";

export type DirectionalConstituentProperty = "top" | "bottom" | "right" | "left";

export type DirectionalShorthandPropertyMode = "numberUpDown" | "numberBox" | "rangeSlider" | "textBox";

// Utility type to convert a string from camel case to kebab case
type ToKebabCase<S extends string> =
    S extends `${infer T}${infer U}`
    ? U extends Uncapitalize<U>
    ? `${Uncapitalize<T>}${ToKebabCase<U>}`
    : `${Uncapitalize<T>}-${ToKebabCase<Uncapitalize<U>>}`
    : S;

// Filter out numeric keys
type StringOnlyKeys<T> = T extends string ? T : never;

// Apply the filtering and conversion to kebab case
export type CSSStyleDeclarationKebabKey = ToKebabCase<StringOnlyKeys<keyof CSSStyleDeclaration>>;

export type StandardDirectionalShorthandProps = {
    element: {
        type: PropType<HTMLElement>;
        required: true;
    };

    /**
     * Used as both the shorthand property label and the group label for constituent properties.
     *
     * Set to "" to hide this label.
     */
    label: {
        type: PropType<string>;
        required: boolean;
    };

    labelTop: {
        type: PropType<string>;
        default: string;
    };

    labelBottom: {
        type: PropType<string>;
        default: string;
    };

    labelRight: {
        type: PropType<string>;
        default: string;
    };

    labelLeft: {
        type: PropType<string>;
        default: string;
    };

    min: {
        type: PropType<number>;
        default: number;
    };

    max: {
        type: PropType<number>;
        default: number;
    };

    showConstituentProperties: {
        type: PropType<boolean | DirectionalConstituentProperty | DirectionalConstituentProperty[]>;
        default: boolean;
    };

    mode: {
        type: PropType<DirectionalShorthandPropertyMode>;
        default: DirectionalShorthandPropertyMode;
    };
};

export type StandardLengthProps = {
    element: {
        type: PropType<HTMLElement>;
        required: true;
    };

    label: {
        type: PropType<string>;
        required: boolean;
    };

    min: {
        type: PropType<number>;
        default: number;
    };

    max: {
        type: PropType<number>;
        default: number;
    };

    mode: {
        type: PropType<DirectionalShorthandPropertyMode>;
        default: DirectionalShorthandPropertyMode;
    };
};
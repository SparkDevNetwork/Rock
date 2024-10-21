import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { PropType } from "vue";

export type BorderStyle = "solid" | "dashed" | "dotted" | "";

export type DirectionalConstituentProperty = "top" | "bottom" | "right" | "left";

export type LengthControlType =
    | {
        type: "numberUpDown";
        min: number | undefined;
        max: number | undefined;
    }
    | {
        type: "numberBox";
        min?: number | undefined;
        max?: number | undefined;
    }
    | {
        type: "rangeSlider";
        min?: number | undefined;
        max?: number | undefined;
    }
    | { type: "textBox" }
    | { type: "dropDownList", items: ListItemBag[] };

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

export type StandardShorthandProps = {
    element: {
        type: PropType<HTMLElement>;
        required: true;
    };

    label: {
        type: PropType<string>;
        required: boolean;
    };

    labelShorthand: {
        type: PropType<string>;
        default: string;
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

    showConstituentProperties: {
        type: PropType<boolean | DirectionalConstituentProperty | DirectionalConstituentProperty[]>;
        default: boolean;
    };
};

export type StandardLengthProps = {
    label: {
        type: PropType<string>;
        required: boolean;
    };

    mode: {
        type: PropType<LengthControlType>;
        default: LengthControlType;
    };
};
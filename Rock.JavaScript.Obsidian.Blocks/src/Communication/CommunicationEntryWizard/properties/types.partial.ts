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
    mode: {
        type: PropType<"inline" | "stylesheet">;
        default: string;
    };

    /**
     * Only required in "inline" mode.
     */
    element: {
        type: PropType<HTMLElement | undefined>;
        required: boolean;
    };

    /**
     * Only required in "stylesheet" mode.
     */
    styleSheetValues: {
        type: PropType<Partial<Record<CSSStyleDeclarationKebabKey, string>> | undefined>;
        required: boolean;
    };

    label: {
        type: PropType<string>;
        required: boolean;
    };

    shorthandLabel: {
        type: PropType<string>;
        default: string;
    };

    topLabel: {
        type: PropType<string>;
        default: string;
    };

    bottomLabel: {
        type: PropType<string>;
        default: string;
    };

    rightLabel: {
        type: PropType<string>;
        default: string;
    };

    leftLabel: {
        type: PropType<string>;
        default: string;
    };

    showConstituentProperties: {
        type: PropType<boolean | DirectionalConstituentProperty | DirectionalConstituentProperty[]>;
        default: boolean;
    };

    canToggleShowConstituents: {
        type: PropType<boolean>;
        default: boolean;
    };

    shorthandCssClass: {
        type: PropType<string | undefined>;
    };

    topCssClass: {
        type: PropType<string | undefined>;
    };

    bottomCssClass: {
        type: PropType<string | undefined>;
    };

    rightCssClass: {
        type: PropType<string | undefined>;
    };

    leftCssClass: {
        type: PropType<string | undefined>;
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
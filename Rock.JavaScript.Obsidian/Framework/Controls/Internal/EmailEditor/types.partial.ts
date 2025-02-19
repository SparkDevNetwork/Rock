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

import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { PropType } from "vue";
import { Ref } from "vue";
import { Guid } from "@Obsidian/Types";

export type GetProcessedHtmlOptions = {
    isPreview?: boolean;
};

export type SendTimePreference = "now" | "later";

export type EditorComponentTypeName =
    "video"
    | "button"
    | "text"
    | "divider"
    | "message"
    | "image"
    | "code"
    | "rsvp"
    | "section"
    | "one-column-section"   // this is a special component type
    | "two-column-section"   // this is a special component type
    | "three-column-section" // this is a special component type
    | "four-column-section"   // this is a special component type
    | "right-sidebar-section"   // this is a special component type
    | "left-sidebar-section" // this is a special component type
    | "title";

export type ComponentTypeDragStartMessage = {
    type: "COMPONENT_TYPE_DRAG_START";
    componentTypeName: EditorComponentTypeName;
    customHtml?: string | null | undefined;
};

export type ComponentTypeDragLeaveMessage = {
    type: "COMPONENT_TYPE_DRAG_LEAVE";
};

export type ComponentTypeDragDropMessage = {
    type: "COMPONENT_TYPE_DRAG_DROP";
};

export type ComponentTypeDragEndMessage = {
    type: "COMPONENT_TYPE_DRAG_END";
};

export type ComponentTypeDragOverMessage = {
    type: "COMPONENT_TYPE_DRAG_OVER";
    clientX: number;
    clientY: number;
};

export type AccordionManager = {
    register(key: string, isExpanded: Ref<boolean>): void;
};

export type Breakpoint = "xs" | "sm" | "md" | "lg" | "xl" | "unknown";

export type BreakpointHelper = {
    breakpoint: Breakpoint;

    isXs: boolean;
    isSm: boolean;
    isMd: boolean;
    isLg: boolean;
    isXl: boolean;

    isXsOrSmaller: boolean;
    isSmOrSmaller: boolean;
    isMdOrSmaller: boolean;
    isLgOrSmaller: boolean;
    isXlOrSmaller: boolean;

    isXsOrLarger: boolean;
    isSmOrLarger: boolean;
    isMdOrLarger: boolean;
    isLgOrLarger: boolean;
    isXlOrLarger: boolean;
};

export type BinaryFileAttachment = {
    fileName: string;
    binaryFileGuid: Guid;
    url: string;
};

export type BlockActionCallbacks = {
    onSuccess(): void;
    onError(error?: string | null | undefined): void;
};

export type FontPropertyGroupVisibilityOptions = {
    isLineHeightHidden?: boolean | undefined;
    isTextFormatHidden?: boolean | undefined;
    isTextCaseHidden?: boolean | undefined;
    isJustifyTextHidden?: boolean | undefined;
    isColorHidden?: boolean | undefined;
};

export type ContentAreaElements = {
    outerTable: HTMLElement;
    outerTableBody: HTMLElement;
    outerTableTr: HTMLElement;
    outerTableTd: HTMLElement;
    innerTable: HTMLElement;
    innerTableBody: HTMLElement;
    innerTableTr: HTMLElement;
    innerTableTd: HTMLElement;
};

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

export type HorizontalAlignment = "left" | "center" | "right";

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
export type CssStyleDeclarationKebabKey = ToKebabCase<StringOnlyKeys<keyof CSSStyleDeclaration>>;

export type StandardProps = {
    /**
     * *(Required, Reactive)* The CSS style declarations (e.g., { "color": "blue", "background-color": "red", "font-family": "Arial" }).
     */
    cssStyleDeclarations: {
        type: PropType<Partial<Record<CssStyleDeclarationKebabKey, string>>>;
        required: true;
    };

    /**
     * *(Optional, Nonreactive)* The element whose inline styles will be updated.
     */
    element: {
        type: PropType<HTMLElement | undefined>;
    };

    /**
     * The property label.
     *
     * *(Nonreactive)*
     */
    label: {
        type: PropType<string>;
        default: string;
    };

    help: {
        type: PropType<string>;
        default: string;
    };
};

export type StandardShorthandProps = StandardProps & {
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

export type Include<T, U extends T> = U;

function styleKeys<T extends readonly CssStyleDeclarationKebabKey[]>(...key: T): T {
    return key;
}

export const BackgroundColorPropertyCssProperties = [...styleKeys("background-color")] as const;
export type BackgroundColorPropertyCssProperty = typeof BackgroundColorPropertyCssProperties[number];

export const BorderColorPropertyCssProperties = [
    // Shorthand property must be after constituent properties.
    ...styleKeys("border-bottom-color", "border-top-color", "border-left-color", "border-right-color", "border-color")
] as const;
export type BorderColorPropertyCssProperty = typeof BorderColorPropertyCssProperties[number];

export const BorderWidthPropertyCssProperties = [
    // Shorthand property must be after constituent properties.
    ...styleKeys("border-top-width", "border-bottom-width", "border-left-width", "border-right-width", "border-width")
] as const;
export type BorderWidthPropertyCssProperty = typeof BorderWidthPropertyCssProperties[number];

export const BorderRadiusPropertyCssProperties = [
    // Shorthand property must be after constituent properties.
    ...styleKeys("border-top-left-radius", "border-top-right-radius", "border-bottom-left-radius", "border-bottom-right-radius", "border-radius")
] as const;
export type BorderRadiusPropertyCssProperty = typeof BorderRadiusPropertyCssProperties[number];

export const BorderStylePropertyCssProperties = [
    // Shorthand property must be after constituent properties.
    ...styleKeys("border-top-style", "border-bottom-style", "border-left-style", "border-right-style", "border-style")
] as const;
export type BorderStylePropertyCssProperty = typeof BorderStylePropertyCssProperties[number];

export const ColorPropertyCssProperties = [...styleKeys("color")] as const;
export type ColorPropertyCssProperty = typeof ColorPropertyCssProperties[number];

export const FontFamilyPropertyCssProperties = [...styleKeys("font-family")] as const;
export type FontFamilyPropertyCssProperty = typeof FontFamilyPropertyCssProperties[number];

export const FontSizePropertyCssProperties = [...styleKeys("font-size")] as const;
export type FontSizePropertyCssProperty = typeof FontSizePropertyCssProperties[number];

export const FontWeightPropertyCssProperties = [...styleKeys("font-weight")] as const;
export type FontWeightPropertyCssProperty = typeof FontWeightPropertyCssProperties[number];

export const HeightPropertyCssProperties = [...styleKeys("height")] as const;
export type HeightPropertyCssProperty = typeof HeightPropertyCssProperties[number];

export const HorizontalAlignmentMarginPropertyCssProperties = [
    ...styleKeys("margin-right", "margin-left")
] as const;
export type HorizontalAlignmentMarginPropertyCssProperty = typeof HorizontalAlignmentMarginPropertyCssProperties[number];

export const LineHeightPropertyCssProperties = [...styleKeys("line-height")] as const;
export type LineHeightPropertyCssProperty = typeof LineHeightPropertyCssProperties[number];

export const MarginPropertyCssProperties = [
    // Shorthand property must be after constituent properties.
    ...styleKeys("margin-top", "margin-bottom", "margin-right", "margin-left", "margin")
] as const;
export type MarginPropertyCssProperty = typeof MarginPropertyCssProperties[number];

export const PaddingPropertyCssProperties = [
    // Shorthand property must be after constituent properties.
    ...styleKeys("padding-top", "padding-bottom", "padding-right", "padding-left", "padding")
] as const;
export type PaddingPropertyCssProperty = typeof PaddingPropertyCssProperties[number];

export const ShowPropertyCssProperties = [...styleKeys("display")] as const;
export type ShowPropertyCssProperty = typeof ShowPropertyCssProperties[number];

export const TextAlignPropertyCssProperties = [...styleKeys("text-align")] as const;
export type TextAlignPropertyCssProperty = typeof TextAlignPropertyCssProperties[number];

export const TextFormatPropertyCssProperties = [
    ...styleKeys("font-weight", "text-decoration", "font-style")
] as const;
export type TextFormatPropertyCssProperty = typeof TextFormatPropertyCssProperties[number];

export const TextTransformPropertyCssProperties = [...styleKeys("text-transform")] as const;
export type TextTransformPropertyCssProperty = typeof TextTransformPropertyCssProperties[number];

export const WidthPropertyCssProperties = [...styleKeys("width")] as const;
export type WidthPropertyCssProperty = typeof WidthPropertyCssProperties[number];


// #region Property Groups

export const BorderPropertyGroupCssProperties = [...BorderColorPropertyCssProperties, ...BorderWidthPropertyCssProperties, ...BorderStylePropertyCssProperties] as const;
export type BorderPropertyGroupCssProperty = typeof BorderPropertyGroupCssProperties[number];

export const DividerPropertyGroupCssProperties = [...BorderColorPropertyCssProperties, ...BorderStylePropertyCssProperties, ...BorderWidthPropertyCssProperties, ...MarginPropertyCssProperties, ...WidthPropertyCssProperties] as const;
export type DividerPropertyGroupCssProperty = typeof DividerPropertyGroupCssProperties[number];

export const FontPropertyGroupCssProperties = [...FontFamilyPropertyCssProperties, ...FontSizePropertyCssProperties, ...ColorPropertyCssProperties, ...TextFormatPropertyCssProperties, ...LineHeightPropertyCssProperties, ...TextTransformPropertyCssProperties, ...TextAlignPropertyCssProperties] as const;
export type FontPropertyGroupCssProperty = typeof FontPropertyGroupCssProperties[number];

// #endregion

export type StyleSheetMode = {
    styleCssClass: string;
    rulesetCssSelector: string;
};

export type BackgroundFit = "repeat" | "center";

export type BackgroundSize = "original" | "fit-width" | "fit-height";

export type BorderStyle = "solid" | "dashed" | "dotted";

export type TextAlignment = "left" | "center" | "right" | "justify";

export type ValueProvider<T> = {
    value: T;
    dispose: () => void;
};

export type ShorthandValueProvider<T> = {
    shorthandValue: T;
    topValue: T;
    bottomValue: T;
    rightValue: T;
    leftValue: T;
    readonly isDisposed: boolean;
    dispose: () => void;
};

export type ValueConverter<T, U> = {
    /**
     * Converts a source value to a target string value.
     */
    toTarget(source: T): U;
    /**
     * Converts a target value to the source value.
     */
    toSource(target: U): T;
};

export type ValueProviderHooks<T, U> = {
    onSourceValueUpdated?: (sourceValue: T) => void;
    onTargetValueUpdated?: (targetValue: U) => void;
};

export type StyleValueProviderHooks<T, U> = ValueProviderHooks<T, U> & {
    onStyleUpdated?: (styleDeclaration: CSSStyleDeclaration, value: U) => void;
};

export type Shorthand<T> = {
    shorthand?: T;
    top?: T;
    bottom?: T;
    left?: T;
    right?: T;
};

export type ShorthandValueProviderHooks<T, U> = ValueProviderHooks<Shorthand<T>, Shorthand<U>>;

export type ShorthandStyleValueProviderHooks<T, U> = StyleValueProviderHooks<Shorthand<T>, Shorthand<U>>;

export type ValueProviderOptions = {
    element: HTMLElement;
    copyToElements?: HTMLElement[] | null | undefined;
    styleSheetMode?: StyleSheetMode | null | undefined;
};

export type CloneComponentRequest = {
    componentElement: HTMLElement;
};

export type CompleteComponentRequest = {
    componentElement: HTMLElement;
};

export type DeleteComponentRequest = {
    componentElement: HTMLElement;
};

export type ComponentTypeDragStartRequest = {
    componentTypeName: EditorComponentTypeName;
    customHtml?: string | null | undefined;
};

export type ComponentTypeDragLeaveRequest = {
    type: "COMPONENT_TYPE_DRAG_LEAVE_REQUEST";
};

export type ComponentTypeDragOverRequest = {
    clientX: number;
    clientY: number;
};

export type ComponentTypeDragDropRequest = {
    type: "COMPONENT_TYPE_DRAG_DROP_REQUEST";
};

export type ComponentTypeDragEndRequest = {
    type: "COMPONENT_TYPE_DRAG_END_REQUEST";
};
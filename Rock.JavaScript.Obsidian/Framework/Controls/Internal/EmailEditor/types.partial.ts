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

import { Ref } from "vue";
import { Enumerable } from "@Obsidian/Utility/linq";
import { Guid } from "@Obsidian/Types";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { FileAsset } from "@Obsidian/ViewModels/Controls/fileAsset";

/**
 * Represents a set of related table elements used for email components.
 * Ensures structure and maintainability for email layouts.
 */
export type TableElements = {
    /** The main `<table>` element, representing a structured container for content. */
    table: HTMLTableElement;

    /** The `<tbody>` element, grouping the component’s rows. */
    tbody: HTMLTableSectionElement;

    /** The `<tr>` element, defining a row within the table. */
    tr: HTMLTableRowElement;

    /** The `<td>` element, representing a cell that contains the component’s content. */
    td: HTMLTableCellElement;
};

/**
 * Defines the three standard wrapper layers used in all email components.
 * Provides a structured, reusable layout for components like text, images, dividers, and buttons.
 */
export type ComponentStructure = {
    /**
     * The **outermost** structure that defines spacing and alignment.
     * Ensures correct positioning within the email layout.
     * - Controls **simulated margin** via padding.
     * - Controls **horizontal alignment** (left, center, right).
     */
    marginWrapper: TableElements & {
        /**
         * Defines the **component's visual boundaries**.
         * Handles styling and overall structure.
         * - Controls **borders** and **border-radius**.
         * - Defines **the width** of the component.
         */
        borderWrapper: TableElements & {
            /**
             * Manages **internal spacing and layout** within the component.
             * Ensures proper content padding while maintaining a structured layout.
             * - Controls **background color**.
             * - Controls **inner padding** inside the component.
             * - Manages **content positioning** without affecting shape.
             */
            paddingWrapper: TableElements;
        };
    };
};

/** The component type names including special types */
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
    | "title"
    | "row";

/** The component type names. */
export type ComponentTypeName = Exclude<
    EditorComponentTypeName,
    "one-column-section"
    | "two-column-section"
    | "three-column-section"
    | "four-column-section"
    | "right-sidebar-section"
    | "left-sidebar-section"
>;

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

export type HorizontalAlignment = "left" | "center" | "right";

export type LetterCase = "none" | "uppercase" | "lowercase" | "capitalize";

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

export type StyleSheetMode = {
    styleCssClass: string;
    rulesetCssSelector: string;
};

export type BackgroundFit = "repeat" | "center";

export type BackgroundSize = "original" | "fit-width" | "fit-height";

export type BorderStyle = "solid" | "dashed" | "dotted" | "none";

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

export type StyleSheetValueProviderHooks<T, U> = StyleValueProviderHooks<T, U> & {
    onStyleUpdated?: (styleSheet: CSSStyleSheet, styleDeclaration: CSSStyleDeclaration, value: U) => void;
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

export type ReplaceComponentRequest = {
    newComponentElement: HTMLElement;
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

export type StyleSheetElements = {
    elementWindow: Window & typeof globalThis;
    elementDocument: Document;
    styleSheet: CSSStyleSheet;
    styleElement: HTMLStyleElement;
    ruleset?: CSSStyleRule | undefined;
};

export type StyleSheetMediaElements = {
    elementWindow: Window & typeof globalThis;
    elementDocument: Document;
    styleSheet: CSSStyleSheet;
    styleElement: HTMLStyleElement;
    ruleset?: CSSMediaRule | undefined;
};

export class ProviderNotCreatedError extends Error {
    constructor(providerName: string) {
        super(
            `${providerName} has not been created for this document. ` +
            `Make sure create${providerName}() is called before using get${providerName}().`
        );
        this.name = "ProviderNotCreatedError";
    }
}

export class ProviderAlreadyExistsError extends Error {
    constructor(providerName: string) {
        super(
            `${providerName} has already been created for this document. ` +
            `Use get${providerName}() instead.`
        );
        this.name = "ProviderAlreadyExistsError";
    }
}

export type DomWatcher = {
    readonly foundElements: Enumerable<Element>;
    onElementFound(callback: (element: Element) => void): void;
    onElementRemoved(callback: (element: Element) => void): void;
    dispose(): void;
};

export class WeakPair<K extends object, V> {
    private store = new WeakMap<K, V>();
    private currentKey: K | null = null;

    set(key: K, value: V): void {
        // Clear previous key before setting a new one
        if (this.currentKey) {
            this.store.delete(this.currentKey);
        }
        this.store.set(key, value);
        this.currentKey = key;
    }

    get(key: K): V | undefined {
        return this.store.get(key);
    }

    has(key: K): boolean {
        return this.store.has(key);
    }

    delete(key: K): boolean {
        return this.store.delete(key);
    }

    clear(): void {
        if (this.currentKey) {
            this.store.delete(this.currentKey);
            this.currentKey = null;
        }
    }
}

export type ButtonWidthMode = "fitToText" | "full" | "fixed";

export type ButtonWidthModel = {
    mode: ButtonWidthMode | null;
    fixedWidthPx: number | null;
};

export type ComponentMigrationHelper = {
    isMigrationRequired(componentElement: Element): boolean;
    migrate(componentElement: Element): Element;
    readonly latestVersion: string;
};

export type ShorthandModel<T> = {
    /** Top. */
    top: T;
    /** Right. */
    right: T;
    /** Bottom. */
    bottom: T;
    /** Left. */
    left: T;
};

/**
 * Logical model for a border.
 * Used for both local button borders and global button border styling.
 */
export type BorderModel = {
    /** CSS border style. */
    style: ShorthandModel<BorderStyle | null> | null;
    /** Border width. */
    widthPx: ShorthandModel<number | null> | null;
    /** CSS color value, typically hex or rgb(a). */
    color: ShorthandModel<string | null> | null;
};


export type GlobalAdapter<TGlobalProps> = {
    version: string;

    /**
     * Reads global props for this component type from the DOM.
     * For buttons, this typically means parsing canonical style blocks
     * and any document level attributes that affect all buttons.
     */
    readGlobalProps: (emailDocument: Document) => TGlobalProps;

    /**
     * Writes global props for this component type into the DOM.
     * For buttons, this typically means updating canonical style blocks
     * and any document level attributes that affect all buttons.
     */
    writeGlobalProps: (emailDocument: Document, globalProps: TGlobalProps) => void;

    /**
     * Migrates the global DOM structures for this component type.
     */
    migrateGlobalProps: (emailDocument: Document) => void;

    /**
     * Creates a new default global props object for this component type.
     *
     * @param emailDocument
     */
    getDefaultGlobalProps: () => TGlobalProps;

    /**
     * Determines if global props for this component type are needed
     * @param emailDocument
     * @returns
     */
    areGlobalDefaultsNeeded: (emailDocument: Document) => boolean;

    /**
     * Called when a new component of this type is added to the document.
     *
     * @param event The event data for the component addition.
     */
    onComponentAdded(event: GlobalAdapterOnComponentAddedEvent<TGlobalProps>): void;
};

export type GlobalAdapterOnComponentAddedEvent<TGlobalProps> = {
    /**
     * The email document where the component was added.
     */
    emailDocument: Document;

    /**
     * The component element that was added.
     */
    componentElement: HTMLElement;

    /**
     * The component type name that was added.
     */
    componentTypeName: ComponentTypeName;

    /**
     * The current global props for the component type.
     */
    globalProps: TGlobalProps;
};

export type GlobalAdapterSnapshot<TGlobalProps> = Omit<
    GlobalAdapter<TGlobalProps>,
    "migrateGlobalProps" | "getDefaultGlobalProps" | "areGlobalDefaultsNeeded" | "onComponentAdded"
>;

/**
 * Generic component adapter for a single component type.
 * L represents local (per instance) props.
 * G represents global (per component type) props.
 */
export type ComponentAdapter<TLocalProps> = {
    /**
     * Latest DOM version for component instances of this type.
     *
     * Stored per node as data-version.
     */
    version: string;

    /**
     * The component type name that this adapter handles.
     */
    componentTypeName: ComponentTypeName;

    /**
     * Migrates all component instance nodes of this type to the latest DOM version.
     */
    migrateAllComponents: (emailDocument: Document) => void;

    /**
     * Migrates a single component instance node to the latest DOM version.
     *
     * Must update data-component-version on the node.
     */
    migrateComponent: (emailDocument: Document, componentElement: HTMLElement) => HTMLElement;

    /**
     * Reads local (per instance) props from a migrated component node.
     *
     * These props will be bound directly to property panel controls.
     */
    readLocalProps: (componentElement: HTMLElement) => TLocalProps;

    /**
     * Writes local (per instance) props into a migrated component node.
     *
     * Called when any bound property panel control changes.
     */
    writeLocalProps: (componentElement: HTMLElement, localProps: TLocalProps) => void;

    /**
     * Creates a new component element for this component type but does not add it to the document.
     */
    createComponentElement: (emailDocument: Document) => HTMLElement;

    /**
     * Refreshes all local props for instances of this component type in the given email document.
     *
     * Useful when global props change that may affect local props.
     */
    refreshAllComponents(emailDocument: Document): void;
};

export type ComponentAdapterVersion<TLocalProps> = Omit<
    ComponentAdapter<TLocalProps>,
    // Omit functions that do not need implementations for each adapter version;
    // i.e., those that will likely be common and should not be duplicated.
    "migrateComponent" | "componentTypeName" | "migrateAllComponents" | "refreshAllComponents"
>;

/**
 * Local button props that map directly to your Button component property panel.
 */
export type ButtonLocalProps = {
    /**
     * Button text content.
     */
    text: string;

    /**
     * Anchor href value.
     */
    href: string;

    /**
     * Font family for the button text.
     */
    fontFamily: string | null;

    /**
     * Font size in pixels for the button text.
     */
    fontSizePx: number | null;

    /**
     * Indicates if the button text is bold.
     */
    isBold: boolean | null;

    /**
     * Indicates if the button text is underlined.
     */
    isUnderlined: boolean | null;

    /**
     * Indicates if the button text is italicized.
     */
    isItalicized: boolean | null;

    /**
     * The letter case to use for the button text.
     */
    letterCase: LetterCase | null;

    /**
     * Line height for the button text.
     */
    lineHeight: number | null;

    /**
     * Text color for the button label.
     */
    textColor: string | null;

    /**
     * Horizontal alignment of the button.
     */
    horizontalAlignment: HorizontalAlignment | null;

    /**
     * Background color of the button.
     */
    backgroundColor: string | null;

    /**
     * Corner radius in pixels for the button.
     */
    borderRadiusPx: ShorthandModel<number | null> | null;

    /**
     * Logical width for this specific button instance.
     */
    width: ButtonWidthModel | null;

    /**
     * Outer margin applied around the button.
     */
    marginPx: ShorthandModel<number | null> | null;

    /**
     * Inner padding applied inside the button.
     */
    paddingPx: ShorthandModel<number | null> | null;

    /**
     * Border styling applied to the button.
     */
    border: BorderModel | null;
};

/**
 * Global button props that map to the "Button Styling" section
 * of your GlobalPropertyPanel.
 */
export type ButtonGlobalProps = {
    /**
     * Global background color for all buttons.
     */
    backgroundColor: string | null;

    /**
     * Global font family for button text.
     */
    fontFamily: string | null;

    /**
     * Global font size in pixels for button text.
     */
    fontSizePx: number | null;

    /**
     * Global bold setting for button text.
     */
    isBold: boolean | null;

    /**
     * Global underline setting for button text.
     */
    isUnderlined: boolean | null;

    /**
     * Global italic setting for button text.
     */
    isItalicized: boolean | null;

    /**
     * Global letter case for button text.
     */
    letterCase: LetterCase | null;

    /**
     * Global line height for button text.
     */
    lineHeight: number | null;

    /**
     * Global text color for button labels.
     */
    textColor: string | null;

    /**
     * Global border settings for buttons.
     */
    border: BorderModel | null;

    /**
     * Global corner radius in pixels for buttons.
     */
    borderRadiusPx: ShorthandModel<number | null> | null;

    /**
     * Global width policy for buttons.
     */
    width: ButtonWidthModel | null;

    /**
     * Global outer margin for buttons.
     */
    marginPx: ShorthandModel<number | null> | null;

    /**
     * Global inner padding for buttons.
     */
    paddingPx: ShorthandModel<number | null> | null;
};

/**
 * Concrete adapter type alias for the Button component.
 */
export type ButtonComponentAdapter = ComponentAdapter<ButtonLocalProps>;

/**
 * Concrete global adapter type alias for the Button component.
 */
export type ButtonGlobalAdapter = GlobalAdapter<ButtonGlobalProps>;

export type ShorthandPropertyNames = {
    top: CssStyleDeclarationKebabKey;
    right: CssStyleDeclarationKebabKey;
    bottom: CssStyleDeclarationKebabKey;
    left: CssStyleDeclarationKebabKey;
};

/**
 * Local RSVP props that map directly to the RSVP component property panel.
 */
export type RsvpLocalProps = {
    blockPaddingPx: ShorthandModel<number | null> | null;
    blockHorizontalAlignment: HorizontalAlignment | null;
    fontFamily: string | null;
    fontSizePx: number | null;
    isBold: boolean | null;
    isUnderlined: boolean | null;
    isItalicized: boolean | null;
    letterCase: LetterCase | null;
    lineHeight: number | null;
    buttonPaddingPx: ShorthandModel<number | null> | null;
    buttonBorderRadiusPx: ShorthandModel<number | null> | null;
    acceptText: string;
    acceptWidth: ButtonWidthModel | null;
    acceptBackgroundColor: string | null;
    acceptTextColor: string | null;
    isDeclineHidden: boolean;
    declineText: string;
    declineWidth: ButtonWidthModel | null;
    declineBackgroundColor: string | null;
    declineTextColor: string | null;
    rsvpGroupGuid: Guid | null;
    rsvpOccurrenceValue: string | null;
};

/**
 * Concrete adapter type alias for the RSVP component.
 */
export type RsvpComponentAdapter = ComponentAdapter<RsvpLocalProps>;

/**
 * Local divider props that map directly to the Divider component property panel.
 */
export type DividerLocalProps = {
    style: BorderStyle | null;
    thicknessPx: number | null;
    color: string | null;
    /**
     * Added in v17.3-alpha to support margin around the divider.
     */
    widthPercent: number | null;
    /**
     * Added in v17.3-alpha to support horizontal alignment of the divider.
     */
    horizontalAlignment: HorizontalAlignment | null;
    marginPx: ShorthandModel<number | null> | null;
    /**
     * @deprecated Supported for legacy reasons. This would place a horizontal line (`<hr>`) instead of a `<div>` resulting in a second line above the colored divider line.
      */
    isDividedWithLine: boolean;
};

/**
 * Global divider props that map to the "Divider Styling" section of the GlobalPropertyPanel.
 */
export type DividerGlobalProps = {
    style: BorderStyle | null;
    thicknessPx: number | null;
    color: string | null;
    widthPercent: number | null;
    horizontalAlignment: HorizontalAlignment | null;
    marginPx: ShorthandModel<number | null> | null;
};

/**
 * Concrete adapter type alias for the Divider component.
 */
export type DividerComponentAdapter = ComponentAdapter<DividerLocalProps>;

/**
 * Concrete global adapter type alias for the Divider component.
 */
export type DividerGlobalAdapter = GlobalAdapter<DividerGlobalProps>;

export type ResizeMode = "crop" | "pad" | "stretch";

type ImageSizeResponsive = {
    readonly type: "responsive";
};

type ImageSizeOriginal = {
    readonly type: "original";
};

type ImageSizeFixed = {
    readonly type: "fixed";

    /**
     * These values are for backward compatibility with existing terminology,
     * but map to these new values in the image component property panel:
     *
     * - "crop" == "Cover"
     * - "pad" == "Contain"
     * - "stretch" == "Stretch"
     */
    resizeMode: ResizeMode;
    fixedWidthPx: number | null;
    fixedHeightPx: number | null;
};

// Use discriminated union for ImageSize so each type can have its own properties.
export type ImageSizeModel = ImageSizeResponsive | ImageSizeOriginal | ImageSizeFixed;

type ImageSourceFile = {
    readonly type: "file";

    isHighResolution: boolean;
    file: ListItemBag | null;
};

type ImageSourceAsset = {
    readonly type: "asset";

    asset: FileAsset | null;
};

export type ImageSourceModel = ImageSourceFile | ImageSourceAsset;

export type ImageLocalProps = {
    /**
     * The source of the image along with related properties.
     */
    imageSource: ImageSourceModel;

    /**
     * The alt text for the image if it cannot be displayed.
     */
    altText: string;

    /**
     * The href to navigate to when the image is clicked.
     */
    href: string | null;

    /**
     * The image size mode and related properties.
     */
    imageSize: ImageSizeModel;

    /**
     * Horizontal alignment of the image.
     */
    horizontalAlignment: HorizontalAlignment | null;

    /**
     * Corner radius in pixels for the image.
     *
     * Not supported in some versions of Outlook.
     */
    borderRadiusPx: ShorthandModel<number | null> | null;

    /**
     * Outer margin applied around the image.
     */
    marginPx: ShorthandModel<number | null> | null;

    /**
     * Border styling applied to the image.
     */
    border: BorderModel | null;
};

export type ImageComponentAdapter = ComponentAdapter<ImageLocalProps>;

export type BodyGlobalProps = {
    widthPx: number | null;
    backgroundColor: string | null;
    bodyAlignment: HorizontalAlignment | null;
    border: BorderModel | null;
    marginPx: ShorthandModel<number | null> | null;
    paddingPx: ShorthandModel<number | null> | null;
};

export type BodyGlobalAdapter = GlobalAdapter<BodyGlobalProps>;

export type GetHtmlRequest = {
    onSuccess: (response: GetHtmlResponse) => void;
    onError?: ((error: string) => void) | null | undefined;
};

export type GetHtmlResponse = {
    html: string;
    bodyWidth?: number | null | undefined;
};

export type UsageType = "template" | "email";

export type CodeLocalProps = {
    html: string;
    marginPx: ShorthandModel<number | null> | null;
};

export type CodeComponentAdapter = ComponentAdapter<CodeLocalProps>;

export type TitleLocalProps = {
    text: string;
    headingLevel: "h1" | "h2" | "h3" | "h4" | "h5" | "h6";
    fontFamily: string | null;
    fontSizePx: number | null;
    isBold: boolean | null;
    isUnderlined: boolean | null;
    isItalicized: boolean | null;
    letterCase: LetterCase | null;
    textAlignment: TextAlignment | null;
    lineHeight: number | null;
    textColor: string | null;
    paddingPx: ShorthandModel<number | null> | null;
    marginPx: ShorthandModel<number | null> | null;
    border: BorderModel | null;
    borderRadiusPx: ShorthandModel<number | null> | null;
};

export type TitleComponentAdapter = ComponentAdapter<TitleLocalProps>;

export type HeadingLevel = "h1" | "h2" | "h3" | "h4" | "h5" | "h6";

export type TextLocalProps = {
    /**
     * The text content of the component, typically in HTML format.
     */
    html: string;

    fontFamily: string | null;
    fontSizePx: number | null;
    isBold: boolean | null;
    isUnderlined: boolean | null;
    isItalicized: boolean | null;
    letterCase: LetterCase | null;
    textAlignment: TextAlignment | null;
    lineHeight: number | null;
    textColor: string | null;
    backgroundColor: string | null;
    paddingPx: ShorthandModel<number | null> | null;
    marginPx: ShorthandModel<number | null> | null;
    border: BorderModel | null;
    borderRadiusPx: ShorthandModel<number | null> | null;
};

export type TextComponentAdapter = ComponentAdapter<TextLocalProps>;

export type VideoLocalProps = {
    /**
     * The URL to link the video to when clicked.
     */
    href: string | null;

    /**
     * This is the URL used to generate the preview image for the video.
     *
     * It is not directly used in the email, but is stored for reference.
     */
    previewImageGeneratorUrl: string | null;

    /**
     * The file reference for the preview image.
     *
     * This is used to construct the actual image URL in the email.
     */
    previewImageFile: ListItemBag | null;

    /**
     * The alt text for the preview image.
     */
    previewImageAltText: string | null;

    /**
     * Space applied around the video component.
     */
    paddingPx: ShorthandModel<number | null> | null;
};

export type VideoComponentAdapter = ComponentAdapter<VideoLocalProps>;

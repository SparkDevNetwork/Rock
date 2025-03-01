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
import { Enumerable } from "@Obsidian/Utility/linq";

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

export type ButtonWidth = "fitToText" | "full" | "fixed";

export type ButtonWidthValues = { width: ButtonWidth | null | undefined; fixedWidth: number | null | undefined; };
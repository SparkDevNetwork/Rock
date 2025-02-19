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

import {
    inject,
    InjectionKey,
    ref,
    Ref,
    watch
} from "vue";
import {
    AccordionManager,
    BackgroundSize,
    BackgroundFit,
    EditorComponentTypeName,
    CssStyleDeclarationKebabKey,
    ContentAreaElements,
    BorderStyle,
    HorizontalAlignment,
    ShorthandValueProvider,
    ValueConverter,
    ValueProvider,
    ValueProviderHooks,
    StyleSheetMode,
    ShorthandStyleValueProviderHooks,
    StyleValueProviderHooks
} from "./types.partial";
import { isNotNullish, isNullish } from "@Obsidian/Utility/util";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { isHTMLElement } from "@Obsidian/Utility/dom";
import { toGuidOrNull, newGuid } from "@Obsidian/Utility/guid";
import { Enumerable } from "@Obsidian/Utility/linq";
import { splitCase, toTitleCase } from "@Obsidian/Utility/stringUtils";

export const AccordionGroupInjectionKey: InjectionKey<AccordionManager> = Symbol("accordion-group");

/** Uses an accordion group if one is set up. */
export function useAccordionGroup(isExpanded: Ref<boolean>): void {
    const accordionKey = newGuid();
    const group = inject(AccordionGroupInjectionKey);
    group?.register(accordionKey, isExpanded);
}

export function getComponentTypeName(element: HTMLElement): EditorComponentTypeName {
    const classList = [...element.classList];

    const map: Record<string, EditorComponentTypeName> = {
        "component-button": "button",
        "component-code": "code",
        "component-divider": "divider",
        "component-image": "image",
        "component-message": "message",
        "component-rsvp": "rsvp",
        "component-section": "section",
        "component-text": "text",
        "component-title": "title",
        "component-video": "video"
    };

    for (const key in map) {
        if (classList.includes(key)) {
            return map[key];
        }
    }

    throw new Error("Unable to get component type for element");
}

export function getComponentIconHtml(componentTypeName: EditorComponentTypeName): string {
    function createIconElement(iconCssClass: string): string {
        return `<i class="${iconCssClass} fa-lg"></i>`;
    }

    switch (componentTypeName) {
        case "title":
            return createIconElement("fa fa-font");
        case "video":
            return createIconElement("fa fa-play-circle-o");
        case "button":
            return `
<div style="background-color: var(--color-interface-strong); width: 60px; border-radius: var(--border-radius-base);">
    <i class="fa fa-mouse-pointer fa-sm" style="color: var(--color-interface-softest);"></i>
</div>
`;
        case "text":
            return createIconElement("fa fa-align-left");
        case "divider":
            return `
<div class="d-flex flex-column align-items-center" style="gap: var(--spacing-tiny);">
    <div style="width: 42px; height: 10px; background-color: var(--color-interface-soft);"></div>
    <div style="width: 56px; height: 2px; background-color: var(--color-interface-strong);"></div>
    <div style="width: 42px; height: 10px; background-color: var(--color-interface-soft);"></div>
</div>`;
        case "message":
            return createIconElement("fa fa-user");
        case "image":
            return createIconElement("fa fa-image");
        case "code":
            return createIconElement("fa fa-code");
        case "rsvp":
            return createIconElement("fa fa-check-square-o");
        case "section":
            return createIconElement("rk rk-one-column");
        case "one-column-section":
            return `
<div class="d-flex" style="width: 65px; height: 31px;">
    <div style="flex-basis: 100%; background-color: var(--color-interface-soft)"></div>
</div>`;
        case "right-sidebar-section":
            return `
<div class="d-flex" style="gap: var(--spacing-tiny); width: 65px; height: 31px;">
    <div style="flex-basis: 66.666666%; background-color: var(--color-interface-soft)"></div>
    <div style="flex-basis: 33.333333%; background-color: var(--color-interface-soft)"></div>
</div>`;
        case "left-sidebar-section":
            return `
<div class="d-flex" style="gap: var(--spacing-tiny); width: 65px; height: 31px;">
    <div style="flex-basis: 33.333333%; background-color: var(--color-interface-soft)"></div>
    <div style="flex-basis: 66.666666%; background-color: var(--color-interface-soft)"></div>
</div>`;
        case "two-column-section":
            return `
<div class="d-flex" style="gap: var(--spacing-tiny); width: 65px; height: 31px;">
    <div style="flex-basis: 50%; background-color: var(--color-interface-soft)"></div>
    <div style="flex-basis: 50%; background-color: var(--color-interface-soft)"></div>
</div>`;
        case "three-column-section":
            return `
<div class="d-flex" style="gap: var(--spacing-tiny); width: 65px; height: 31px;">
    <div style="flex-basis: 33.333333%; background-color: var(--color-interface-soft)"></div>
    <div style="flex-basis: 33.333333%; background-color: var(--color-interface-soft)"></div>
    <div style="flex-basis: 33.333333%; background-color: var(--color-interface-soft)"></div>
</div>`;
        case "four-column-section":
            return `
<div class="d-flex" style="gap: var(--spacing-tiny); width: 65px; height: 31px;">
    <div style="flex-basis: 25%; background-color: var(--color-interface-soft)"></div>
    <div style="flex-basis: 25%; background-color: var(--color-interface-soft)"></div>
    <div style="flex-basis: 25%; background-color: var(--color-interface-soft)"></div>
    <div style="flex-basis: 25%; background-color: var(--color-interface-soft)"></div>
</div>`;
        default:
            console.warn(`Unable to retrieve the icon for the unknown component type: '${componentTypeName}'. Returning the default icon.`);
            return createIconElement("fa fa-question");
    }
}

export function getComponentTitle(componentTypeName: EditorComponentTypeName): string {
    switch (componentTypeName) {
        case "title":
            return "Title";
        case "video":
            return "Video";
        case "button":
            return "Button";
        case "text":
            return "Paragraph";
        case "divider":
            return "Divider";
        case "message":
            return "Message";
        case "image":
            return "Image";
        case "code":
            return "Lava";
        case "rsvp":
            return "RSVP";
        case "section":
        case "one-column-section":
            return "1";
        case "right-sidebar-section":
        case "left-sidebar-section":
        case "two-column-section":
            return "2";
        case "three-column-section":
            return "3";
        case "four-column-section":
            return "4";
        default:
            console.warn(`Unable to retrieve the title for the unknown component type, '${componentTypeName}'. Returning the default icon.`);
            return toTitleCase(splitCase(componentTypeName).replace("-", " "));
    }
}

/**
 * Add a temporary (runtime) CSS class with this prefix to any element that should be stripped out when the HTML is retrieved via getProcessedHtml().
 */
export const RockRuntimeClassCssClassPrefix = "rock-runtime-class" as const;

/**
 * Add this CSS class to any temporary (runtime) element that should be stripped out when the HTML is retrieved via getProcessedHtml().
 */
export const RockRuntimeElementCssClass = "rock-runtime-element" as const;

/**
 * Add this CSS class to any element that should be editable inline when selected in the email designer.
 */
export const RockCssClassContentEditable = `rock-content-editable` as const;

/**
 * Add this CSS class to any temporary element that wraps other elements, and should be unwrapped when the HTML is retrieved via getProcessedHtml().
 *
 * The wrapped elements will be placed in the DOM in place of the wrapper.
 */
export const RockRuntimeWrapperElementCssClass = "rock-runtime-wrapper-element" as const;

export function createComponentElementPlaceholder(document: Document): HTMLElement {
    const container = document.createElement("div");
    container.classList.add("component-placeholder-container");

    // Line (the horizontal divider)
    const line = document.createElement("div");
    line.classList.add("component-placeholder-line");

    // Pill (the text badge)
    const pill = document.createElement("div");
    pill.classList.add("component-placeholder-pill");
    pill.textContent = "Drag it here";

    // Assemble
    container.appendChild(line);
    container.appendChild(pill);
    container.appendChild(line.cloneNode());

    return container;
}

export function createComponentElement(document: Document, componentTypeName: EditorComponentTypeName): HTMLElement {
    const componentTypeCssClass = `component-${componentTypeName.endsWith("-section") ? "section" : componentTypeName}`;
    const componentElement = document.createElement("div");
    componentElement.classList.add("component", componentTypeCssClass);
    componentElement.dataset.state = "component";

    // Notes:
    //  - Inline styles defined here will be at the component level instead of the global level.
    //  - Ensure inline styles can be edited, if desired. Otherwise, styles added here will not be
    //    editable by the individual using the editor.
    //  - Rock CSS variables should not be used here as these will be added to the HTML email.
    //  - Global style defaults are maintained in the emailIFrame.partial.obs file.
    switch (componentTypeName) {
        case "title":
            componentElement.innerHTML = `<h1 class="${RockCssClassContentEditable}">Title</h1>`;
            break;
        case "video":
            componentElement.innerHTML = `<a href=""><img src="/Assets/Images/video-placeholder.jpg" data-imgcsswidth="full" style="width: 100%;"></a>`;
            break;
        case "button":
            componentElement.classList.add("v2");

            // The styles and attributes here will default each button to have a "Fit To Text" width (see buttonWidthProperty.partial.obs for details).
            componentElement.innerHTML = `<table class="button-outerwrap" border="0" cellpadding="0" cellspacing="0" width="100%" style="min-width: 100%;">
                <tbody>
                    <tr>
                        <td class="button-innerwrap" align="center" valign="top">
                            <table class="button-shell" border="0" cellpadding="0" cellspacing="0" style="max-width: 100%; table-layout: auto;">
                                <tbody>
                                    <tr>
                                        <td class="button-content" align="center" valign="middle">
                                            <a class="button-link ${RockCssClassContentEditable}" href="https://" rel="noopener noreferrer" target="_blank" title="Click Me" style="display: inline-block;">Click Me</a>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </td>
                    </tr>
                </tbody>
            </table>`;
            break;
        case "text":
            // Default component styles.
            componentElement.style.paddingTop = "4px";
            componentElement.style.paddingBottom = "4px";

            componentElement.classList.add(RockCssClassContentEditable);
            componentElement.innerHTML = `<p>Let's see what you have to say!</p>`;
            break;
        case "divider":
            componentElement.innerHTML = `<hr />`;
            break;
        case "message":
            componentElement.classList.add(RockCssClassContentEditable);
            componentElement.innerText = "Message";
            break;
        case "image":
            // Image component needs a line-height of 0 to remove extra space under image.
            componentElement.style.lineHeight = "0";
            componentElement.innerHTML = `<img alt="" src="/Assets/Images/image-placeholder.jpg" data-imgcsswidth="full" style="width: 100%;">`;
            break;
        case "code":
            componentElement.classList.add(RockCssClassContentEditable);
            componentElement.innerHTML = `Add your code here...`;
            break;
        case "rsvp":
            componentElement.innerHTML = `<table class="rsvp-outerwrap" border="0" cellpadding="0" cellspacing="0" width="100%" style="min-width: 100%;">
                <tbody>
                    <tr>
                        <td class="rsvp-innerwrap" align="center" valign="top" style="padding: 0;">
                            <table border="0" cellpadding="0" cellspacing="0">
                                <tbody>
                                    <tr>
                                        <td>
                                            <table class="accept-button-shell" border="0" cellpadding="0" cellspacing="0" style="background-color: #16C98D; border-collapse: separate !important; border-radius: 3px; display: inline-table;">
                                                <tbody>
                                                    <tr>
                                                        <td class="rsvp-accept-content" align="center" valign="middle">
                                                            <a class="rsvp-accept-link ${RockCssClassContentEditable}" href="https://" rel="noopener noreferrer" target="_blank" title="Accept" style="color: #FFFFFF; display: inline-block; font-family: Arial; font-size: 16px; font-weight: bold; letter-spacing: normal; padding: 15px; text-align: center; text-decoration: none;">Accept</a>
                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                        </td>
                                        <td style="padding-left: 10px;">
                                            <table class="decline-button-shell" border="0" cellpadding="0" cellspacing="0" style="background-color: #D4442E; border-collapse: separate !important; border-radius: 3px; display: inline-table;">
                                                <tbody>
                                                    <tr>
                                                        <td class="rsvp-decline-content" align="center" valign="middle">
                                                            <a class="rsvp-decline-link ${RockCssClassContentEditable}" href="https://" rel="noopener noreferrer" target="_blank" title="Decline" style="color: #FFFFFF; display: inline-block; font-family: Arial; font-size: 16px; font-weight: bold; letter-spacing: normal; padding: 15px; text-align: center; text-decoration: none;">Decline</a>
                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </td>
                    </tr>
                </tbody>
            </table>
            <input type="hidden" class="rsvp-group-id">
            <input type="hidden" class="rsvp-occurrence-value">`;
            break;
        case "section":
            // Default component styles.
            componentElement.style.padding = "16px";

            componentElement.innerHTML = `<div class="dropzone"></div>`;
            break;
        case "one-column-section":
            // Default component styles.
            componentElement.style.padding = "16px";

            componentElement.innerHTML = `<table class="row" cellpadding="0" cellspacing="0" border="0" style="border-spacing: 0; table-layout: fixed;" width="100%">
            <tbody>
                <tr>
                    <td class="dropzone columns small-12 start last large-12" valign="top" width="100%"></td>
                </tr>
            </tbody>
        </table>`;
            break;
        case "right-sidebar-section":
            // Default component styles.
            componentElement.style.padding = "16px";

            componentElement.innerHTML = `<table class="row" cellpadding="0" cellspacing="0" border="0" style="border-spacing: 0; table-layout: fixed;" width="100%">
            <tbody>
                <tr>
                    <td class="dropzone columns small-12 start large-8" valign="top" width="66.66666666666666%"></td>
                    <td class="dropzone columns small-12 last large-4" valign="top" width="33.33333333333333%"></td>
                </tr>
            </tbody>
        </table>`;
            break;
        case "left-sidebar-section":
            // Default component styles.
            componentElement.style.padding = "16px";

            componentElement.innerHTML = `<table class="row" cellpadding="0" cellspacing="0" border="0" style="border-spacing: 0; table-layout: fixed;" width="100%">
                <tbody>
                    <tr>
                        <td class="dropzone columns small-12 start large-4" valign="top" width="33.33333333333333%"></td>
                        <td class="dropzone columns small-12 last large-8" valign="top" width="66.66666666666666%"></td>
                    </tr>
                </tbody>
            </table>`;
            break;
        case "two-column-section":
            // Default component styles.
            componentElement.style.padding = "16px";

            componentElement.innerHTML = `<table class="row" cellpadding="0" cellspacing="0" border="0" style="border-spacing: 0; table-layout: fixed;" width="100%">
                <tbody>
                    <tr>
                        <td class="dropzone columns small-12 start large-6" valign="top" width="50%"></td>
                        <td class="dropzone columns small-12 last large-6" valign="top" width="50%"></td>
                    </tr>
                </tbody>
            </table>`;
            break;
        case "three-column-section":
            // Default component styles.
            componentElement.style.padding = "16px";

            componentElement.innerHTML = `<table class="row" cellpadding="0" cellspacing="0" border="0" style="border-spacing: 0; table-layout: fixed;" width="100%">
                <tbody>
                    <tr>
                        <td class="dropzone columns small-12 start large-4" valign="top" width="33.33333333333333%"></td>
                        <td class="dropzone columns small-12 large-4" valign="top" width="33.33333333333333%"></td>
                        <td class="dropzone columns small-12 last large-4" valign="top" width="33.33333333333333%"></td>
                    </tr>
                </tbody>
            </table>`;
            break;
        case "four-column-section":
            // Default component styles.
            componentElement.style.padding = "16px";

            componentElement.innerHTML = `<table class="row" cellpadding="0" cellspacing="0" border="0" style="border-spacing: 0; table-layout: fixed;" width="100%">
                <tbody>
                    <tr>
                        <td class="dropzone columns small-12 start large-3" valign="top" width="25%"></td>
                        <td class="dropzone columns small-12 large-3" valign="top" width="25%"></td>
                        <td class="dropzone columns small-12 large-3" valign="top" width="25%"></td>
                        <td class="dropzone columns small-12 last large-3" valign="top" width="25%"></td>
                    </tr>
                </tbody>
            </table>`;
            break;
        default:
            throw new Error(`Unknown typeName: ${componentTypeName}`);
    }

    return componentElement;
}

export function get<T>(value: T): T {
    return value;
}

export function createCssRuleset(selector: string, declarations: Record<string, string>): string | null {
    if (Object.keys(declarations).length) {
        const validDeclarations = Object.entries(declarations).filter(([_k, v]) => !!v).map(([k, v]) => `${k}: ${v};`);
        if (validDeclarations.length) {
            return `${selector} {\n    ${validDeclarations.join("\n    ")}\n}`;
        }
    }

    return null;
}

/**
 * Finds descendant content area elements within a given HTML element.
 * This function identifies relevant table structures while ignoring specific classes.
 *
 * The outerTable must be a direct child element of the `element` argument.
 *
 * @param {HTMLElement} element - The root element to search within.
 * @returns {Partial<ContentAreaElements>} An object containing references to found table elements.
 *
 * @example
 * const container = document.getElementById("content");
 * const elements = findDescendantContentAreaElements(container);
 * console.log(elements.outerTable); // Logs the first outer table found
 */
export function findDescendantContentAreaElements(element: HTMLElement): Partial<ContentAreaElements> {
    const searchResult: Partial<ContentAreaElements> = {};
    const outerTableCssClassesToIgnore = ["button-outerwrap"] as const;
    searchResult.outerTable = [...element.querySelectorAll(":scope > table")]
        .filter(table =>
            !outerTableCssClassesToIgnore.some(ignoreCssClass => table.classList.contains(ignoreCssClass)) && // Exclude tables with classes
            !outerTableCssClassesToIgnore.some(ignoreCssClass => table.closest(`table.${ignoreCssClass}`))    // Exclude tables nested within a table with classes
        )[0] as HTMLElement // Get the first result
        ?? undefined;

    if (searchResult.outerTable) {
        searchResult.outerTableBody = searchResult.outerTable.querySelector(":scope > tbody") as HTMLElement ?? undefined;
        searchResult.outerTableTr = searchResult.outerTable.querySelector(":scope > tbody > tr, :scope > tr") as HTMLElement ?? undefined;

        if (searchResult.outerTableTr) {
            searchResult.outerTableTd = searchResult.outerTableTr.querySelector("td, th") as HTMLElement ?? undefined;

            if (searchResult.outerTableTd) {
                const innerTableCssClassesToIgnore = ["header"] as const;
                searchResult.innerTable = [...searchResult.outerTableTd.querySelectorAll("table")]
                    .filter(table =>
                        !innerTableCssClassesToIgnore.some(ignoreCssClass => table.classList.contains(ignoreCssClass)) && // Exclude tables with classes
                        !innerTableCssClassesToIgnore.some(ignoreCssClass => table.closest(`table.${ignoreCssClass}`))    // Exclude tables nested within a table with classes
                    )[0] as HTMLElement // Get the first result
                    ?? undefined;

                if (searchResult.innerTable) {
                    searchResult.innerTableBody = searchResult.innerTable.querySelector(":scope > tbody") as HTMLElement ?? undefined;
                    searchResult.innerTableTr = searchResult.innerTable.querySelector(":scope > tbody > tr, :scope > tr") as HTMLElement ?? undefined;

                    if (searchResult.innerTableTr) {
                        searchResult.innerTableTd = searchResult.innerTableTr.querySelector("td, th") as HTMLElement ?? undefined;
                    }
                }
            }
        }
    }

    return searchResult;
}

export function addContentAreaElementsIfMissing(
    element: HTMLElement,
    {
        outerTableCssClass,
        innerTableCssClass
    }: {
        outerTableCssClass?: string,
        innerTableCssClass?: string
    } = {}
): ContentAreaElements {
    const tableElements = findDescendantContentAreaElements(element);

    if (!tableElements.outerTable) {
        // Create all elements since the outermost table is missing.
        // The outer table's primary purposes are:
        // - structural container for child elements
        // - provides alignment, padding, background colors
        // - better supported by email clients than using "margin" or other CSS functionality
        tableElements.outerTable = element.ownerDocument.createElement("table");
        tableElements.outerTable.setAttribute("role", "presentation");
        tableElements.outerTable.setAttribute("width", "100%");
        tableElements.outerTable.setAttribute("cellpadding", "0");
        tableElements.outerTable.setAttribute("cellspacing", "0");
        tableElements.outerTable.style.width = "100%";
        tableElements.outerTable.style.tableLayout = "fixed";
        tableElements.outerTable.style.borderSpacing = "0";
        element.append(tableElements.outerTable);
    }

    if (outerTableCssClass) {
        tableElements.outerTable.classList.add(outerTableCssClass);
    }

    // Store initial children before potentially adding outerTable
    const initialChildren = [...element.children].filter(child => child !== tableElements.outerTable && !child.classList.contains(RockStylesCssClass));

    if (!tableElements.outerTableBody) {
        tableElements.outerTableBody = element.ownerDocument.createElement("tbody");
        tableElements.outerTable.append(tableElements.outerTableBody);
    }

    if (!tableElements.outerTableTr) {
        tableElements.outerTableTr = element.ownerDocument.createElement("tr");
        tableElements.outerTableBody.append(tableElements.outerTableTr);
    }

    if (!tableElements.outerTableTd) {
        tableElements.outerTableTd = element.ownerDocument.createElement("td");
        tableElements.outerTableTr.append(tableElements.outerTableTd);
    }

    if (!tableElements.innerTable) {
        // Inner table.
        tableElements.innerTable = element.ownerDocument.createElement("table");
        tableElements.innerTable.setAttribute("role", "presentation");
        tableElements.innerTable.setAttribute("width", "100%");
        tableElements.innerTable.setAttribute("cellpadding", "0");
        tableElements.innerTable.setAttribute("cellspacing", "0");
        tableElements.innerTable.style.width = "100%";
        tableElements.innerTable.style.borderSpacing = "0";
        tableElements.innerTable.style.tableLayout = "fixed";
        tableElements.outerTableTd.append(tableElements.innerTable);
    }

    if (innerTableCssClass) {
        tableElements.innerTable.classList.add(innerTableCssClass);
    }

    if (!tableElements.innerTableBody) {
        tableElements.innerTableBody = element.ownerDocument.createElement("tbody");
        tableElements.innerTable.append(tableElements.innerTableBody);
    }

    if (!tableElements.innerTableTr) {
        tableElements.innerTableTr = element.ownerDocument.createElement("tr");
        tableElements.innerTableBody.append(tableElements.innerTableTr);
    }

    if (!tableElements.innerTableTd) {
        tableElements.innerTableTd = element.ownerDocument.createElement("td");
        tableElements.innerTableTr.append(tableElements.innerTableTd);

        // Move all the element's children into the inner td element.
        tableElements.innerTableTd.append(...initialChildren);
    }

    return {
        outerTable: tableElements.outerTable,
        outerTableBody: tableElements.outerTableBody,
        outerTableTr: tableElements.outerTableTr,
        outerTableTd: tableElements.outerTableTd,
        innerTable: tableElements.innerTable,
        innerTableBody: tableElements.innerTableBody,
        innerTableTr: tableElements.innerTableTr,
        innerTableTd: tableElements.innerTableTd,
    };
}

/**
 * Determines if the specified shorthand property is set in the style source.
 *
 * @param source The style source to check.
 * @param shorthandProperty The shorthand property to check. (e.g. `margin`)
 * @param longhandProperties The longhand properties that make up the shorthand property. (e.g. `margin-top`, `margin-bottom`, `margin-right`, `margin-left`)
 */
export function isShorthandPropertySet(
    source: HTMLElement | Partial<Record<CssStyleDeclarationKebabKey, string>>,
    shorthandProperty: CssStyleDeclarationKebabKey,
    longhandProperties: CssStyleDeclarationKebabKey[]
): boolean {
    const isHTMLElementSource = isHTMLElement(source);

    const shorthandValue = isHTMLElementSource
        ? source.style.getPropertyValue(shorthandProperty)
        : (source[shorthandProperty] ?? "");

    // Check if shorthand is explicitly mentioned in cssText.
    const isShorthandValuePresent = isHTMLElementSource
        ? source.style.cssText.includes(shorthandProperty)
        : !isNullish(source[shorthandProperty]);

    // If shorthand is explicitly present in cssText (including "")...
    if (isShorthandValuePresent) {
        // ...verify that longhand properties match or are cleared.
        const areLonghandValuesCleared = longhandProperties.every(property => {
            return isHTMLElementSource
                ? source.style.getPropertyValue(property) === ""
                : !source[property];
        });

        if (shorthandValue === "" && areLonghandValuesCleared) {
            // Shorthand is explicitly set to "" and longhand properties are cleared.
            return true;
        }

        // Otherwise, is shorthand set to a valid value.
        return shorthandValue !== "";
    }
    else {
        return false;
    }
}

/**
 * Moves the specified inline styles from the source to the target element.
 *
 * @param source The source from which to move the inline styles.
 * @param targetElement The target element to which to move the inline styles.
 * @param shorthandProperty The shorthand property to move. (e.g. `margin`)
 * @param longhandProperties The longhand properties that make up the shorthand property. (e.g. `margin-top`, `margin-bottom`, `margin-right`, `margin-left`)
 */
export function moveShorthandInlineStyles(source: HTMLElement, targetElement: HTMLElement, shorthandProperty: CssStyleDeclarationKebabKey, longhandProperties: CssStyleDeclarationKebabKey[]): void {
    const isSourceShorthandPropertySet = isShorthandPropertySet(source, shorthandProperty, longhandProperties);

    // If the shorthand is explicitly set, transfer or remove it.
    if (isSourceShorthandPropertySet) {
        // Only copy the shorthand value if the target doesn't have its own value.
        const isTargetShorthandPropertySet = isShorthandPropertySet(targetElement, shorthandProperty, longhandProperties);

        if (!isTargetShorthandPropertySet) {
            const sourceShorthandPropertyValue = source.style.getPropertyValue(shorthandProperty);
            targetElement.style.setProperty(shorthandProperty, sourceShorthandPropertyValue);
        }

        // Remove the shorthand value from the source element.
        source.style.removeProperty(shorthandProperty);

        // No need to check longhand properties if the shorthand is set.
        return;
    }


    // Handle individual longhand properties.
    longhandProperties.forEach(longhandProperty => {
        if (!source.style.cssText.includes(longhandProperty)) {
            // Skip longhand properties that are not explicitly set in the source element.
            return;
        }

        // Move the inline styles to the target element if the property is not already set.
        // Using cssText.includes is safe in this case because longhand names are unique and cannot overlap with shorthand.
        const isTargetLonghandPropertySet = targetElement.style.cssText.includes(longhandProperty);

        if (!isTargetLonghandPropertySet) {
            const sourceLonghandPropertyValue = source.style.getPropertyValue(longhandProperty);
            const sourceLonghandPropertyPriority = source.style.getPropertyPriority(longhandProperty);

            targetElement.style.setProperty(longhandProperty, sourceLonghandPropertyValue, sourceLonghandPropertyPriority);
        }

        // Remove the inline style from the source element.
        source.style.removeProperty(longhandProperty);
    });
}

/**
 * Moves the specified inline styles from the source element to the target element.
 *
 * @param sourceElement The source element from which to move the inline styles.
 * @param targetElement The target element to which to move the inline styles.
 * @param standaloneProperties The inline styles to move. (e.g. `display`, `color`, `opacity`, `z-index`)
 */
export function moveStandaloneInlineStyles(
    sourceElement: HTMLElement,
    targetElement: HTMLElement,
    standaloneProperties: CssStyleDeclarationKebabKey[]
): void {
    standaloneProperties.forEach(property => {
        if (!sourceElement.style.cssText.includes(property)) {
            // Skip if the property is not explicitly set in the source element.
            return;
        }

        if (!targetElement.style.cssText.includes(property)) {
            const sourcePropertyValue = sourceElement.style.getPropertyValue(property);
            const sourcePropertyPriority = sourceElement.style.getPropertyPriority(property);

            // Move the property to the target element.
            targetElement.style.setProperty(property, sourcePropertyValue, sourcePropertyPriority);
        }

        // Remove the property from the source element.
        sourceElement.style.removeProperty(property);
    });
}

/**
 * Copies the specified shorthand inline styles from the source to the target element.
 *
 * @param source The source from which to copy the inline styles.
 * @param targetElement The target element to which to copy the inline styles.
 * @param shorthandProperty The shorthand property to copy. (e.g. `margin`)
 * @param longhandProperties The longhand properties that make up the shorthand property. (e.g. `margin-top`, `margin-bottom`, `margin-right`, `margin-left`)
 */
export function copyShorthandInlineStyles(
    source: HTMLElement | Partial<Record<CssStyleDeclarationKebabKey, string>>,
    targetElement: HTMLElement,
    shorthandProperty: CssStyleDeclarationKebabKey,
    longhandProperties: CssStyleDeclarationKebabKey[]
): void {
    const isHTMLElementSource = isHTMLElement(source);

    const isSourceShorthandPropertySet = isShorthandPropertySet(
        source,
        shorthandProperty,
        longhandProperties
    );

    // If the shorthand is explicitly set, copy it to the target.
    if (isSourceShorthandPropertySet) {
        const sourceShorthandPropertyValue = isHTMLElementSource
            ? source.style.getPropertyValue(shorthandProperty)
            : (source[shorthandProperty] ?? "");

        targetElement.style.setProperty(shorthandProperty, sourceShorthandPropertyValue);

        // No need to check longhand properties if the shorthand is set.
        return;
    }

    // Handle individual longhand properties.
    longhandProperties.forEach(longhandProperty => {
        const isSourceLonghandPropertySet = isHTMLElementSource
            ? source.style.cssText.includes(longhandProperty)
            : !isNullish(source[longhandProperty]);

        if (!isSourceLonghandPropertySet) {
            // Skip longhand properties that are not explicitly set in the source element.
            return;
        }

        const sourceLonghandPropertyValue = isHTMLElementSource
            ? source.style.getPropertyValue(longhandProperty)
            : source[longhandProperty] ?? "";
        const sourceLonghandPropertyPriority = isHTMLElementSource
            ? source.style.getPropertyPriority(longhandProperty)
            : "";

        targetElement.style.setProperty(
            longhandProperty,
            sourceLonghandPropertyValue,
            sourceLonghandPropertyPriority
        );
    });
}

export const stringConverter: ValueConverter<string | null | undefined, string | null> = {
    toTarget(source: string | null | undefined): string | null {
        return source || null;
    },

    toSource(target: string | null): string | null | undefined {
        return target || null;
    }
};

export const pixelConverter: ValueConverter<number | null | undefined, string | null> = {
    toTarget(source: number | null | undefined): string | null {
        return isNullish(source) ? null : `${source}px`;
    },

    toSource(target: string | null): number | null | undefined {
        if (isNullish(target)) {
            return target;
        }
        else if (!target) {
            return null;
        }
        else if (target === "0") {
            return 0;
        }
        else if (target.endsWith("px")) {
            return parseInt(target);
        }
        else {
            try {
                const result = parseInt(target);

                if (isNaN(result)) {
                    return undefined;
                }
                else {
                    return result;
                }
            }
            catch {
                return undefined;
            }
        }
    }
};

export const percentageConverter: ValueConverter<number | null | undefined, string | null> = {
    toTarget(source: number | null | undefined): string | null {
        return isNullish(source) ? null : `${source}%`;
    },

    toSource(target: string | null): number | null | undefined {
        if (isNullish(target)) {
            return target;
        }
        else if (!target) {
            return null;
        }
        else if (!target || target === "0") {
            return 0;
        }
        else if (target.endsWith("%")) {
            return parseInt(target);
        }
        else {
            try {
                return parseInt(target);
            }
            catch {
                return undefined;
            }
        }
    }
};

export const integerConverter: ValueConverter<number | null | undefined, string | null> = {
    toTarget(source: number | null | undefined): string | null {
        return isNullish(source) ? null : `${source}`;
    },

    toSource(target: string | null): number | null | undefined {
        if (isNullish(target)) {
            return target;
        }
        else if (!target) {
            return null;
        }
        else if (target === "0") {
            return 0;
        }
        else {
            try {
                const intValue = parseInt(target);

                if (`${intValue}` === target) {
                    return intValue;
                }
                else {
                    return undefined;
                }
            }
            catch {
                return undefined;
            }
        }
    }
};

export function inlineStyleProvider<T>(
    element: HTMLElement,
    property: CssStyleDeclarationKebabKey,
    converter: ValueConverter<T, string | null>,
    copyToElements?: HTMLElement[] | null | undefined,
    hooks?: StyleValueProviderHooks<T, string | null> | null | undefined
): ValueProvider<T> {
    const initialTargetValue = element.style.getPropertyValue(property);

    hooks?.onTargetValueUpdated?.(initialTargetValue);

    const value = ref<T>(
        converter.toSource(initialTargetValue) as T
    );
    hooks?.onSourceValueUpdated?.(value.value as T);

    const targetElements = [element, ...(copyToElements ?? [])];

    const watcher = watch(value, (newValue) => {
        hooks?.onSourceValueUpdated?.(newValue as T);

        const targetValue = converter.toTarget(newValue as T);

        if (targetValue != null) {
            targetElements.forEach(element => {
                element.style.setProperty(property, targetValue);
                hooks?.onStyleUpdated?.(element.style, targetValue);
            });
        }
        else {
            targetElements.forEach(element => {
                element.style.removeProperty(property);
                hooks?.onStyleUpdated?.(element.style, targetValue);
            });
        }

        hooks?.onTargetValueUpdated?.(targetValue);
    });

    return {
        get value() {
            return value.value as T;
        },
        set value(newValue: T) {
            (value as Ref<T>).value = newValue;
        },
        dispose: () => {
            watcher();
        }
    };
}

export function shorthandInlineStyleProvider<T>(
    element: HTMLElement,
    shorthandProperty: CssStyleDeclarationKebabKey,
    longhandProperties: Record<"top" | "right" | "bottom" | "left", CssStyleDeclarationKebabKey>,
    converter: ValueConverter<T, string | null>,
    copyToElements?: HTMLElement[] | null | undefined,
    hooks?: ShorthandStyleValueProviderHooks<T, string | null> | null | undefined
): ShorthandValueProvider<T> {
    let isDisposed: boolean = false;
    const targetElements = [element, ...(copyToElements ?? [])];
    const initialTargetValue = {
        shorthand: element.style.getPropertyValue(shorthandProperty),
        top: element.style.getPropertyValue(longhandProperties.top),
        bottom: element.style.getPropertyValue(longhandProperties.bottom),
        left: element.style.getPropertyValue(longhandProperties.left),
        right: element.style.getPropertyValue(longhandProperties.right)
    };

    hooks?.onTargetValueUpdated?.(initialTargetValue);

    const shorthandValue = ref<T>(
        converter.toSource(initialTargetValue.shorthand) as T
    );
    const topValue = ref<T>(
        converter.toSource(initialTargetValue.top) as T
    );
    const rightValue = ref<T>(
        converter.toSource(initialTargetValue.right) as T
    );
    const bottomValue = ref<T>(
        converter.toSource(initialTargetValue.bottom) as T
    );
    const leftValue = ref<T>(
        converter.toSource(initialTargetValue.left) as T
    );

    hooks?.onSourceValueUpdated?.({
        shorthand: shorthandValue.value as T,
        top: topValue.value as T,
        bottom: bottomValue.value as T,
        left: leftValue.value as T,
        right: rightValue.value as T,
    });

    const topRightBottomLeftWatcher = watch(
        [topValue, rightValue, bottomValue, leftValue],
        ([newTop, newRight, newBottom, newLeft]) => {
            const targetTop = converter.toTarget(newTop as T);
            const targetBottom = converter.toTarget(newBottom as T);
            const targetLeft = converter.toTarget(newLeft as T);
            const targetRight = converter.toTarget(newRight as T);

            if ([targetTop, targetBottom, targetLeft, targetRight].every((val) => val === null)) {
                targetElements.forEach(element => {
                    element.style.removeProperty(shorthandProperty);

                    hooks?.onStyleUpdated?.(element.style, {
                        shorthand: null,
                        top: null,
                        bottom: null,
                        left: null,
                        right: null,
                    });
                });

                hooks?.onTargetValueUpdated?.({
                    shorthand: null,
                    top: null,
                    bottom: null,
                    left: null,
                    right: null,
                });
            }
        });

    const topWatcher = watch(topValue, (newValue) => {
        hooks?.onSourceValueUpdated?.({
            top: newValue as T
        });

        const targetValue = converter.toTarget(newValue as T);

        if (isNullish(targetValue)) {
            targetElements.forEach(element => {
                element.style.removeProperty(longhandProperties.top);

                hooks?.onStyleUpdated?.(element.style, {
                    top: targetValue
                });
            });
        }
        else {
            targetElements.forEach(element => {
                element.style.setProperty(longhandProperties.top, targetValue);

                hooks?.onStyleUpdated?.(element.style, {
                    top: targetValue
                });
            });
        }

        hooks?.onTargetValueUpdated?.({
            top: targetValue
        });
    });

    const bottomWatcher = watch(bottomValue, (newValue) => {
        hooks?.onSourceValueUpdated?.({
            bottom: newValue as T
        });

        const targetValue = converter.toTarget(newValue as T);

        if (isNullish(targetValue)) {
            targetElements.forEach(element => {
                element.style.removeProperty(longhandProperties.bottom);

                hooks?.onStyleUpdated?.(element.style, {
                    bottom: targetValue
                });
            });
        }
        else {
            targetElements.forEach(element => {
                element.style.setProperty(longhandProperties.bottom, targetValue);

                hooks?.onStyleUpdated?.(element.style, {
                    bottom: targetValue
                });
            });
        }

        hooks?.onTargetValueUpdated?.({
            bottom: targetValue
        });
    });

    const leftWatcher = watch(leftValue, (newValue) => {
        hooks?.onSourceValueUpdated?.({
            left: newValue as T
        });

        const targetValue = converter.toTarget(newValue as T);

        if (isNullish(targetValue)) {
            targetElements.forEach(element => {
                element.style.removeProperty(longhandProperties.left);

                hooks?.onStyleUpdated?.(element.style, {
                    left: targetValue
                });
            });
        }
        else {
            targetElements.forEach(element => {
                element.style.setProperty(longhandProperties.left, targetValue);

                hooks?.onStyleUpdated?.(element.style, {
                    left: targetValue
                });
            });
        }

        hooks?.onTargetValueUpdated?.({
            left: targetValue
        });
    });

    const rightWatcher = watch(rightValue, (newValue) => {
        hooks?.onSourceValueUpdated?.({
            right: newValue as T
        });

        const targetValue = converter.toTarget(newValue as T);

        if (isNullish(targetValue)) {
            targetElements.forEach(element => {
                element.style.removeProperty(longhandProperties.right);

                hooks?.onStyleUpdated?.(element.style, {
                    right: targetValue
                });
            });
        }
        else {
            targetElements.forEach(element => {
                element.style.setProperty(longhandProperties.right, targetValue);

                hooks?.onStyleUpdated?.(element.style, {
                    right: targetValue
                });
            });
        }

        hooks?.onTargetValueUpdated?.({
            right: targetValue
        });
    });

    const shorthandWatcher = watch(shorthandValue, (newValue) => {
        hooks?.onSourceValueUpdated?.({
            shorthand: newValue as T,
        });

        const targetValue = converter.toTarget(newValue as T);

        if (targetValue === null) {
            const sourceValue = converter.toSource(targetValue);
            (topValue as Ref<T>).value = sourceValue;
            (rightValue as Ref<T>).value = sourceValue;
            (bottomValue as Ref<T>).value = sourceValue;
            (leftValue as Ref<T>).value = sourceValue;

            targetElements.forEach(element => {
                // Remove all properties when the shorthand property is cleared.
                element.style.removeProperty(shorthandProperty);
                element.style.removeProperty(longhandProperties.top);
                element.style.removeProperty(longhandProperties.bottom);
                element.style.removeProperty(longhandProperties.left);
                element.style.removeProperty(longhandProperties.right);

                hooks?.onStyleUpdated?.(element.style, {
                    shorthand: targetValue,
                    top: targetValue,
                    bottom: targetValue,
                    left: targetValue,
                    right: targetValue
                });

                hooks?.onTargetValueUpdated?.({
                    shorthand: targetValue,
                    top: targetValue,
                    bottom: targetValue,
                    left: targetValue,
                    right: targetValue,
                });
            });
        }
        else {
            targetElements.forEach(element => {
                element.style.setProperty(shorthandProperty, targetValue);

                hooks?.onStyleUpdated?.(element.style, {
                    shorthand: targetValue
                });
            });

            hooks?.onTargetValueUpdated?.({
                shorthand: targetValue
            });
        }
    });

    return {
        get shorthandValue() {
            return shorthandValue.value as T;
        },
        set shorthandValue(newValue: T) {
            (shorthandValue as Ref<T>).value = newValue;
        },
        get topValue() {
            return topValue.value as T;
        },
        set topValue(newValue: T) {
            (topValue as Ref<T>).value = newValue;
        },
        get rightValue() {
            return rightValue.value as T;
        },
        set rightValue(newValue: T) {
            (rightValue as Ref<T>).value = newValue;
        },
        get bottomValue() {
            return bottomValue.value as T;
        },
        set bottomValue(newValue: T) {
            (bottomValue as Ref<T>).value = newValue;
        },
        get leftValue() {
            return leftValue.value as T;
        },
        set leftValue(newValue: T) {
            (leftValue as Ref<T>).value = newValue;
        },
        get isDisposed(): boolean {
            return isDisposed;
        },
        dispose: () => {
            topRightBottomLeftWatcher();
            topWatcher();
            bottomWatcher();
            leftWatcher();
            rightWatcher();
            shorthandWatcher();
            isDisposed = true;
        },
    };
}

type StyleSheetElements = {
    elementWindow: Window & typeof globalThis;
    elementDocument: Document;
    styleSheet: CSSStyleSheet;
    styleElement: HTMLStyleElement;
    ruleset?: CSSStyleRule | undefined;
};

/**
 * Finds the first CSS style sheet where the <style class> matches the `styleCssClass`
 * and the ruleset selector matches the `cssRulesetSelectors`.
 */
function findElements(element: Element, styleCssClass: string, rulesetCssSelector: string): StyleSheetElements | undefined {
    const elementDocument = element.ownerDocument;
    const elementWindow = elementDocument.defaultView;

    if (elementWindow) {
        const eligibleStyleSheets: Omit<StyleSheetElements, "ruleset">[] = [];

        for (const styleSheet of elementDocument.styleSheets) {
            const styleSheetParentElement = styleSheet.ownerNode;

            if (styleSheetParentElement instanceof elementWindow.HTMLStyleElement
                && styleSheetParentElement.classList.contains(styleCssClass)
                && (element === styleSheetParentElement || element.contains(styleSheetParentElement))
            ) {
                const currentEligibleElements: Omit<StyleSheetElements, "ruleset"> = {
                    elementWindow,
                    elementDocument,
                    styleElement: styleSheetParentElement,
                    styleSheet
                };

                eligibleStyleSheets.push(currentEligibleElements);

                for (const ruleset of styleSheet.cssRules) {
                    if (ruleset instanceof elementWindow.CSSStyleRule) {
                        if (ruleset.selectorText === rulesetCssSelector) {
                            // An exact match was found: <style> element and CSS ruleset selector.
                            return {
                                ...currentEligibleElements,
                                ruleset
                            };
                        }
                    }
                }
            }
        }

        if (eligibleStyleSheets.length) {
            // A <style> was found but a CSS ruleset was not found.
            return eligibleStyleSheets[0];
        }
    }
}

function createElements(element: Element, styleCssClass: string): StyleSheetElements | undefined {
    const elementDocument = element.ownerDocument;
    const elementWindow = elementDocument.defaultView;

    if (elementWindow) {
        // Add the stylesheet since it's missing.
        const styleElement = elementDocument.createElement("style") as HTMLStyleElement;
        styleElement.classList.add(styleCssClass);
        element.append(styleElement);

        return {
            elementDocument,
            elementWindow,
            styleElement,
            styleSheet: styleElement.sheet!
        };
    }
}

function addRuleset(elements: StyleSheetElements, rulesetCssSelector: string): CSSStyleRule {
    if (elements.ruleset) {
        // Skip if the ruleset is already created.
        return elements.ruleset;
    }

    const { styleSheet } = elements;
    const rulesetIndex = styleSheet.insertRule(`${rulesetCssSelector} {}`, styleSheet.cssRules.length);
    const ruleset = styleSheet.cssRules[rulesetIndex] as CSSStyleRule;

    elements.ruleset = ruleset;

    return ruleset;
}

function updateStyleElementTextContent(elements: StyleSheetElements): void {
    const { styleElement, styleSheet } = elements;

    // The previous ruleset change only affects the style in memory.
    // Update the HTML <style> element contents.
    styleElement.textContent = Enumerable.from(styleSheet.cssRules)
        .select(rule => rule.cssText)
        .aggregate((rules, rule, i) => i !== 0 ? `${rules}\n${rule}` : rule, "");
}

/**
 * Creates a ValueProvider for a stylesheet pixel value.
 *
 * @param element The element that owns the stylesheet.
 * @param styleCssClass The CSS class of the stylesheet.
 * @param rulesetCssSelector The CSS selectors of the ruleset.
 * @param property The CSS property to manage.
 * @returns
 */
export function styleSheetProvider<T>(
    element: Element,
    styleCssClass: string,
    rulesetCssSelector: string,
    property: CssStyleDeclarationKebabKey,
    converter: ValueConverter<T, string | null>,
    hooks?: StyleValueProviderHooks<T, string | null> | null | undefined
): ValueProvider<T> {
    const initialElements = findElements(element, styleCssClass, rulesetCssSelector);
    const initialTargetValue = initialElements?.ruleset?.style.getPropertyValue(property) ?? null;

    hooks?.onTargetValueUpdated?.(initialTargetValue);

    const value = ref<T>(
        converter.toSource(initialTargetValue) as T
    );

    hooks?.onSourceValueUpdated?.(value.value as T);

    // Update the stylesheet whenever the value changes.
    const watcher = watch(value, (newValue) => {
        hooks?.onSourceValueUpdated?.(newValue as T);

        let elements = findElements(element, styleCssClass, rulesetCssSelector);

        if (!elements && !isNullish(newValue)) {
            // Only create missing elements if there is a value to be set.
            // This accounts for cases where multiple sets of elements
            // can be created on initialization.
            elements = createElements(element, styleCssClass);
        }

        if (elements) {
            const ruleset = elements.ruleset ?? addRuleset(elements, rulesetCssSelector);
            const targetValue = converter.toTarget(newValue as T);

            if (isNullish(targetValue)) {
                ruleset.style.removeProperty(property);
            }
            else {
                ruleset.style.setProperty(property, targetValue);
            }

            hooks?.onStyleUpdated?.(ruleset.style, targetValue);
            hooks?.onTargetValueUpdated?.(targetValue);

            updateStyleElementTextContent(elements);
        }
    });

    return {
        get value() {
            return value.value as T;
        },
        set value(newValue: T) {
            (value as Ref<T>).value = newValue;
        },
        dispose: () => {
            watcher();
        }
    };
}

/**
 * Creates a ShorthandValueProvider for a stylesheet pixel value.
 *
 * @param element The element that owns the stylesheet.
 * @param styleCssClass The CSS class of the stylesheet.
 * @param rulesetCssSelector The CSS selectors of the ruleset.
 * @param shorthandProperty The shorthand property to manage.
 * @param longhandProperties The longhand properties that make up the shorthand property.
 * @param converter The converter to use for converting values to and from CSS properties.
 * @returns A ShorthandValueProvider for managing the shorthand and longhand properties.
 */
export function shorthandStyleSheetProvider<T extends number | string | boolean | null | undefined>(
    element: Element,
    styleCssClass: string,
    rulesetCssSelector: string,
    shorthandProperty: CssStyleDeclarationKebabKey,
    longhandProperties: Record<"top" | "right" | "bottom" | "left", CssStyleDeclarationKebabKey>,
    converter: ValueConverter<T, string | null>,
    hooks?: ShorthandStyleValueProviderHooks<T, string | null> | null | undefined
): ShorthandValueProvider<T> {
    let isDisposed: boolean = false;
    const initialElements = findElements(element, styleCssClass, rulesetCssSelector);
    const initialTargetValues = {
        shorthand: initialElements?.ruleset?.style.getPropertyValue(shorthandProperty) ?? null,
        top: initialElements?.ruleset?.style.getPropertyValue(longhandProperties.top) ?? null,
        bottom: initialElements?.ruleset?.style.getPropertyValue(longhandProperties.bottom) ?? null,
        right: initialElements?.ruleset?.style.getPropertyValue(longhandProperties.right) ?? null,
        left: initialElements?.ruleset?.style.getPropertyValue(longhandProperties.left) ?? null
    };

    hooks?.onTargetValueUpdated?.(initialTargetValues);

    const shorthandValue = ref<T>(
        converter.toSource(initialTargetValues.shorthand)
    );
    const topValue = ref<T>(
        converter.toSource(initialTargetValues.top)
    );
    const rightValue = ref<T>(
        converter.toSource(initialTargetValues.right)
    );
    const bottomValue = ref<T>(
        converter.toSource(initialTargetValues.bottom)
    );
    const leftValue = ref<T>(
        converter.toSource(initialTargetValues.left)
    );

    hooks?.onSourceValueUpdated?.({
        shorthand: shorthandValue.value as T,
        top: topValue.value as T,
        bottom: bottomValue.value as T,
        left: leftValue.value as T,
        right: rightValue.value as T,
    });

    const topRightBottomLeftWatcher = watch(
        [topValue, rightValue, bottomValue, leftValue],
        ([newTop, newRight, newBottom, newLeft]) => {
            if ([newTop, newRight, newBottom, newLeft].every((val) => isNullish(val))) {
                const elements = findElements(element, styleCssClass, rulesetCssSelector);

                if (elements?.ruleset) {
                    const { ruleset } = elements;

                    ruleset.style.removeProperty(shorthandProperty);

                    hooks?.onStyleUpdated?.(ruleset.style, {
                        shorthand: null
                    });

                    hooks?.onTargetValueUpdated?.({
                        shorthand: null
                    });

                    updateStyleElementTextContent(elements);
                }
            }
        });

    const topWatcher = watch(topValue, (newValue) => {
        hooks?.onSourceValueUpdated?.({
            top: newValue as T
        });

        let elements = findElements(element, styleCssClass, rulesetCssSelector);

        if (!elements && !isNullish(newValue)) {
            // Only create missing elements if there is a value to be set.
            // This accounts for cases where multiple sets of elements
            // can be created on initialization.
            elements = createElements(element, styleCssClass);
        }

        if (elements) {
            const ruleset = elements.ruleset ?? addRuleset(elements, rulesetCssSelector);

            const targetValue = converter.toTarget(newValue as T);

            if (isNullish(targetValue)) {
                ruleset.style.removeProperty(longhandProperties.top);
            }
            else {
                ruleset.style.setProperty(longhandProperties.top, targetValue);
            }

            hooks?.onStyleUpdated?.(ruleset.style, {
                top: targetValue
            });

            hooks?.onTargetValueUpdated?.({
                top: targetValue
            });

            updateStyleElementTextContent(elements);
        }
    });

    const bottomWatcher = watch(bottomValue, (newValue) => {
        hooks?.onSourceValueUpdated?.({
            bottom: newValue as T
        });

        let elements = findElements(element, styleCssClass, rulesetCssSelector);

        if (!elements && !isNullish(newValue)) {
            // Only create missing elements if there is a value to be set.
            // This accounts for cases where multiple sets of elements
            // can be created on initialization.
            elements = createElements(element, styleCssClass);
        }

        if (elements) {
            const ruleset = elements.ruleset ?? addRuleset(elements, rulesetCssSelector);
            const targetValue = converter.toTarget(newValue as T);

            if (isNullish(targetValue)) {
                ruleset.style.removeProperty(longhandProperties.bottom);
            }
            else {
                ruleset.style.setProperty(longhandProperties.bottom, targetValue);
            }

            hooks?.onStyleUpdated?.(ruleset.style, {
                bottom: targetValue
            });

            hooks?.onTargetValueUpdated?.({
                bottom: targetValue
            });

            updateStyleElementTextContent(elements);
        }
    });

    const leftWatcher = watch(leftValue, (newValue) => {
        hooks?.onSourceValueUpdated?.({
            left: newValue as T
        });

        let elements = findElements(element, styleCssClass, rulesetCssSelector);

        if (!elements && !isNullish(newValue)) {
            // Only create missing elements if there is a value to be set.
            // This accounts for cases where multiple sets of elements
            // can be created on initialization.
            elements = createElements(element, styleCssClass);
        }

        if (elements) {
            const ruleset = elements.ruleset ?? addRuleset(elements, rulesetCssSelector);
            const targetValue = converter.toTarget(newValue as T);

            if (isNullish(targetValue)) {
                ruleset.style.removeProperty(longhandProperties.left);
            }
            else {
                ruleset.style.setProperty(longhandProperties.left, targetValue);
            }

            hooks?.onStyleUpdated?.(ruleset.style, {
                left: targetValue
            });

            hooks?.onTargetValueUpdated?.({
                left: targetValue
            });

            updateStyleElementTextContent(elements);
        }
    });

    const rightWatcher = watch(rightValue, (newValue) => {
        hooks?.onSourceValueUpdated?.({
            right: newValue as T
        });

        let elements = findElements(element, styleCssClass, rulesetCssSelector);

        if (!elements && !isNullish(newValue)) {
            // Only create missing elements if there is a value to be set.
            // This accounts for cases where multiple sets of elements
            // can be created on initialization.
            elements = createElements(element, styleCssClass);
        }

        if (elements) {
            const ruleset = elements.ruleset ?? addRuleset(elements, rulesetCssSelector);
            const targetValue = converter.toTarget(newValue as T);

            if (isNullish(targetValue)) {
                ruleset.style.removeProperty(longhandProperties.right);
            }
            else {
                ruleset.style.setProperty(longhandProperties.right, targetValue);
            }

            hooks?.onStyleUpdated?.(ruleset.style, {
                right: targetValue
            });

            hooks?.onTargetValueUpdated?.({
                right: targetValue
            });

            updateStyleElementTextContent(elements);
        }
    });

    const shorthandWatcher = watch(shorthandValue, (newValue) => {
        hooks?.onSourceValueUpdated?.({
            shorthand: newValue as T
        });

        let elements = findElements(element, styleCssClass, rulesetCssSelector);

        if (!elements && !isNullish(newValue)) {
            // Only create missing elements if there is a value to be set.
            // This accounts for cases where multiple sets of elements
            // can be created on initialization.
            elements = createElements(element, styleCssClass);
        }

        if (elements) {
            const ruleset = elements.ruleset ?? addRuleset(elements, rulesetCssSelector);
            const targetValue = converter.toTarget(newValue as T);

            if (isNullish(targetValue)) {
                (topValue as Ref<T>).value = null as T;
                (rightValue as Ref<T>).value = null as T;
                (bottomValue as Ref<T>).value = null as T;
                (leftValue as Ref<T>).value = null as T;

                // Remove all properties when the shorthand property is cleared.
                ruleset.style.removeProperty(shorthandProperty);
                ruleset.style.removeProperty(longhandProperties.top);
                ruleset.style.removeProperty(longhandProperties.bottom);
                ruleset.style.removeProperty(longhandProperties.left);
                ruleset.style.removeProperty(longhandProperties.right);

                hooks?.onStyleUpdated?.(ruleset.style, {
                    shorthand: targetValue,
                    top: targetValue,
                    bottom: targetValue,
                    left: targetValue,
                    right: targetValue
                });
            }
            else {
                ruleset.style.setProperty(shorthandProperty, targetValue);

                hooks?.onStyleUpdated?.(ruleset.style, {
                    shorthand: targetValue
                });
            }

            hooks?.onTargetValueUpdated?.({
                shorthand: targetValue
            });

            updateStyleElementTextContent(elements);
        }
    });

    return {
        get shorthandValue() {
            return shorthandValue.value as T;
        },
        set shorthandValue(newValue: T) {
            (shorthandValue as Ref<T>).value = newValue;
        },
        get topValue() {
            return topValue.value as T;
        },
        set topValue(newValue: T) {
            (topValue as Ref<T>).value = newValue;
        },
        get rightValue() {
            return rightValue.value as T;
        },
        set rightValue(newValue: T) {
            (rightValue as Ref<T>).value = newValue;
        },
        get bottomValue() {
            return bottomValue.value as T;
        },
        set bottomValue(newValue: T) {
            (bottomValue as Ref<T>).value = newValue;
        },
        get leftValue() {
            return leftValue.value as T;
        },
        set leftValue(newValue: T) {
            (leftValue as Ref<T>).value = newValue;
        },
        get isDisposed(): boolean {
            return isDisposed;
        },
        dispose: () => {
            topRightBottomLeftWatcher();
            topWatcher();
            bottomWatcher();
            leftWatcher();
            rightWatcher();
            shorthandWatcher();
            isDisposed = true;
        },
    };
}

export function attributeProvider<T>(
    element: Element,
    attribute: string,
    converter: ValueConverter<T, string | null>,
    copyToElements?: Element[],
    hooks?: ValueProviderHooks<T, string | null>
): ValueProvider<T> {
    const targetElements = [element, ...(copyToElements ?? [])];

    const initialTargetValue = element.getAttribute(attribute);

    hooks?.onTargetValueUpdated?.(initialTargetValue);

    const value = ref<T | null | undefined>(
        converter.toSource(initialTargetValue)
    );

    hooks?.onSourceValueUpdated?.(value.value as T);

    const watcher = watch(value, (newValue) => {
        hooks?.onSourceValueUpdated?.(newValue as T);

        const targetValue = converter.toTarget(newValue as T);

        if (isNullish(targetValue)) {
            targetElements.forEach(element => {
                element.removeAttribute(attribute);
            });
        }
        else {
            targetElements.forEach(element => {
                element.setAttribute(attribute, targetValue);
            });
        }

        hooks?.onTargetValueUpdated?.(targetValue);
    });

    return {
        get value(): T {
            return (value as Ref<T>).value;
        },
        set value(newValue: T) {
            (value as Ref<T>).value = newValue;
        },
        dispose() {
            watcher();
        }
    };
}

export const borderStyleConverter: ValueConverter<BorderStyle | null | undefined, string | null> = {
    toTarget(source: BorderStyle | null | undefined): string | null {
        return stringConverter.toTarget(source);
    },

    toSource(target: string | null): BorderStyle | null | undefined {
        return stringConverter.toSource(target) as BorderStyle | null | undefined;
    }
};

export const horizontalAlignmentConverter: ValueConverter<HorizontalAlignment | "" | null | undefined, string | null> = {
    toTarget: function (source: HorizontalAlignment | "" | null | undefined): string | null {
        return stringConverter.toTarget(source);
    },
    toSource: function (target: string | null): HorizontalAlignment | "" | null | undefined {
        return stringConverter.toSource(target) as HorizontalAlignment | "" | null | undefined;
    }
};

export function createBorderStyleProvider(
    element: HTMLElement,
    copyToElements?: HTMLElement[] | null | undefined,
    styleSheetMode?: StyleSheetMode | null | undefined,
    hooks?: ShorthandStyleValueProviderHooks<BorderStyle | null | undefined, string | null> | null | undefined
): ShorthandValueProvider<string | null | undefined> {
    if (!styleSheetMode) {
        return shorthandInlineStyleProvider(
            element,
            "border-style",
            {
                top: "border-top-style",
                bottom: "border-bottom-style",
                right: "border-right-style",
                left: "border-left-style"
            },
            borderStyleConverter,
            copyToElements,
            hooks
        );
    }
    else {
        return shorthandStyleSheetProvider(
            element,
            styleSheetMode.styleCssClass,
            styleSheetMode.rulesetCssSelector,
            "border-style",
            {
                top: "border-top-style",
                bottom: "border-bottom-style",
                right: "border-right-style",
                left: "border-left-style"
            },
            borderStyleConverter,
            hooks
        );
    }
}

export function createBorderWidthProvider(
    element: HTMLElement,
    copyToElements?: HTMLElement[] | null | undefined,
    styleSheetMode?: StyleSheetMode | null | undefined,
    hooks?: ShorthandStyleValueProviderHooks<number | null | undefined, string | null> | null | undefined
): ShorthandValueProvider<number | null | undefined> {
    if (!styleSheetMode) {
        return shorthandInlineStyleProvider(
            element,
            "border-width",
            {
                top: "border-top-width",
                bottom: "border-bottom-width",
                right: "border-right-width",
                left: "border-left-width"
            },
            pixelConverter,
            copyToElements,
            hooks
        );
    }
    else {
        return shorthandStyleSheetProvider(
            element,
            styleSheetMode.styleCssClass,
            styleSheetMode.rulesetCssSelector,
            "border-width",
            {
                top: "border-top-width",
                bottom: "border-bottom-width",
                right: "border-right-width",
                left: "border-left-width"
            },
            pixelConverter,
            hooks
        );
    }
}

export function createBorderColorProvider(
    element: HTMLElement,
    copyToElements?: HTMLElement[] | null | undefined,
    styleSheetMode?: StyleSheetMode | null | undefined,
    hooks?: ShorthandStyleValueProviderHooks<string | null | undefined, string | null> | null | undefined
): ShorthandValueProvider<string | null | undefined> {
    if (!styleSheetMode) {
        return shorthandInlineStyleProvider(
            element,
            "border-color",
            {
                top: "border-top-color",
                bottom: "border-bottom-color",
                right: "border-right-color",
                left: "border-left-color"
            },
            borderStyleConverter,
            copyToElements,
            hooks
        );
    }
    else {
        return shorthandStyleSheetProvider(
            element,
            styleSheetMode.styleCssClass,
            styleSheetMode.rulesetCssSelector,
            "border-color",
            {
                top: "border-top-color",
                bottom: "border-bottom-color",
                right: "border-right-color",
                left: "border-left-color"
            },
            borderStyleConverter,
            hooks
        );
    }
}

export function createBackgroundImageProvider(
    element: HTMLElement,
    copyToElements: HTMLElement[] | null | undefined
): ValueProvider<ListItemBag | null | undefined> {
    const targetElements = [element, ...(copyToElements ?? [])];
    const value = ref<ListItemBag | null | undefined>(getValue(element));

    function getValue(element: HTMLElement): ListItemBag | null | undefined {
        const backgroundImageGuid = toGuidOrNull(element.dataset["backgroundImageGuid"]);

        if (backgroundImageGuid) {
            return {
                value: backgroundImageGuid,
                text: element.dataset["backgroundImageFileName"]
            };
        }
        else {
            return null;
        }
    }

    const watcher = watch(value, (newValue) => {
        const fileGuid = toGuidOrNull(newValue?.value);

        if (fileGuid) {
            const fileName = newValue?.text;
            const backgroundImageValue = `url('/GetImage.ashx?guid=${fileGuid}')`;

            targetElements.forEach(element => {
                element.style.backgroundImage = backgroundImageValue;
            });

            // These hold data that cannot be easily retrieved from style properties.
            element.dataset["backgroundImageGuid"] = fileGuid ?? "";
            element.dataset["backgroundImageFileName"] = fileName ?? "";
        }
        else {
            targetElements.forEach(element => {
                element.style.backgroundImage = "";
            });

            delete element.dataset["backgroundImageGuid"];
            delete element.dataset["backgroundImageFileName"];
        }
    });

    const provider = {
        get value(): ListItemBag | null | undefined {
            return value.value;
        },
        set value(newValue: ListItemBag | null | undefined) {
            value.value = newValue;
        },
        dispose() {
            watcher();
        }
    };

    return provider;
}

export function createBackgroundSizeProvider(
    element: HTMLElement,
    copyToElements?: HTMLElement[] | null | undefined
): ValueProvider<BackgroundSize | null | undefined> {
    type CSSStyleDeclarationBackground = {
        backgroundColor: string;
        backgroundImage: string;
        backgroundPosition: string;
        backgroundSize: string;
        backgroundRepeat: string;
        backgroundAttachment: string;
        backgroundClip: string;
        backgroundOrigin: string;
    };

    const backgroundSizeFitWidth = "100% auto" as const;
    const backgroundSizeFitHeight = "auto 100%" as const;
    const backgroundSizeOriginal = "auto" as const;

    const targetElements = [element, ...(copyToElements ?? [])];

    // Chromium browsers have an issue with `background-size: <percentage> auto;`
    // when set with `element.style.backgroundSize = "100% auto;"` and `element.style.setProperty("background-size", "100% auto;").
    // Only the "100%" value is stored...
    // We will work around this by updating the entire `style` attribute instead of
    // the individual style properties.

    if (!getCurrentElementValue()) {
        // Default to "fit-width".
        setBackgroundSize(backgroundSizeFitWidth);
    }

    const value = ref<BackgroundSize | null | undefined>(getCurrentElementValue());

    function parseBackgroundShorthand(backgroundString: string): CSSStyleDeclarationBackground[] {
        // Define default values for each sub-property
        const defaultValues: CSSStyleDeclarationBackground = {
            backgroundColor: "",
            backgroundImage: "",
            backgroundPosition: "",
            backgroundSize: "",
            backgroundRepeat: "",
            backgroundAttachment: "",
            backgroundClip: "",
            backgroundOrigin: ""
        };

        // Split the layers by commas
        const layers = backgroundString.split(/\s*,\s*/);

        // Parse each layer
        const parsedLayers = layers.map(layer => parseLayer(layer.trim(), defaultValues));

        return parsedLayers;
    }

    function parseLayer(layerString: string, defaultValues: CSSStyleDeclarationBackground): CSSStyleDeclarationBackground {
        const result: CSSStyleDeclarationBackground = {
            ...defaultValues
        };

        let backgroundClipHandled: boolean = false;

        // Split the layer into tokens
        const tokens = layerString.split(/\s+/);
        let positionAndSizeHandled = false;

        for (let i = 0; i < tokens.length; i++) {
            const token = tokens[i];

            if (isColor(token)) {
                result.backgroundColor = token;
            }
            else if (isUrl(token) || isGradient(token) || token === "none") {
                result.backgroundImage = token;
            }
            else if (isRepeatValue(token)) {
                result.backgroundRepeat = token;
            }
            else if (isAttachmentValue(token)) {
                result.backgroundAttachment = token;
            }
            else if (isBoxValue(token)) {
                if (!backgroundClipHandled) {
                    result.backgroundClip = token;
                    // Default to same value
                    result.backgroundOrigin = token;
                    backgroundClipHandled = true;
                }
                else {
                    result.backgroundOrigin = token;
                }
            }
            else if (token.includes("/")) {
                const [position, size] = token.split("/");
                result.backgroundPosition = position.trim();
                result.backgroundSize = size.trim();
                positionAndSizeHandled = true;
            }
            else if (!positionAndSizeHandled) {
                result.backgroundPosition = token;
                if (i + 1 < tokens.length && tokens[i + 1] === "/") {
                    result.backgroundSize = tokens[i + 2];
                    // Skip position, "/", and size
                    i += 2;
                    positionAndSizeHandled = true;
                }
            }
        }

        return result;
    }

    // Helper functions
    function isColor(value: string): boolean {
        return /^(#(?:[0-9a-fA-F]{3}){1,2}|rgba?\(.*?\)|transparent|[a-z]+)$/.test(value);
    }

    function isUrl(value: string): boolean {
        return /^url\((.*?)\)$/.test(value);
    }

    function isGradient(value: string): boolean {
        return /^(linear-gradient|radial-gradient|conic-gradient)\(.*?\)$/.test(value);
    }

    function isRepeatValue(value: string): boolean {
        return /^(repeat|no-repeat|repeat-x|repeat-y|space|round)$/.test(value);
    }

    function isAttachmentValue(value: string): boolean {
        return /^(scroll|fixed|local)$/.test(value);
    }

    function isBoxValue(value: string): boolean {
        return /^(border-box|padding-box|content-box)$/.test(value);
    }

    function getCurrentElementValue(): BackgroundSize | null | undefined {
        const backgroundSize = getCurrentElementStyleBackgroundSize();
        if (backgroundSize === backgroundSizeFitWidth) {
            return "fit-width";
        }
        else if (backgroundSize === backgroundSizeFitHeight) {
            return "fit-height";
        }
        else if (backgroundSize === backgroundSizeOriginal) {
            return "original";
        }
        else {
            return null;
        }
    }

    function setBackgroundSize(backgroundSize: string): void {
        // Use the removeProperty to figure out how to remove the current value.
        targetElements.forEach(element => {
            element.style.removeProperty("background-size");

            const styleDeclarationsString = element.getAttribute("style");
            if (styleDeclarationsString) {
                const suffix = styleDeclarationsString.endsWith(";") ? " " : "; ";
                element.setAttribute("style", `${styleDeclarationsString}${suffix}background-size: ${backgroundSize};`);
            }
            else {
                element.setAttribute("style", `background-size: ${backgroundSize};`);
            }
        });
    }

    function getCurrentElementStyleBackgroundSize(): string {
        const styleDeclarationsString = element.getAttribute("style");

        if (styleDeclarationsString) {
            type PropertyAndValue = {
                property: string;
                value: string;
            };

            const properties = Enumerable
                .from(styleDeclarationsString.split(";"))
                .select((propertyString): PropertyAndValue | null => {
                    propertyString = propertyString.trim();
                    const firstColonIndex = propertyString.indexOf(":");

                    if (firstColonIndex !== -1 && (firstColonIndex + 1) < propertyString.length) {
                        return {
                            property: propertyString.substring(0, firstColonIndex),
                            value: propertyString.substring(firstColonIndex + 1).trimStart()
                        };
                    }
                    else {
                        return null;
                    }
                })
                .where(property => property?.property === "background" || property?.property === "background-size")
                .ofType(isNotNullish)
                .toList();

            const backgroundSize = properties.lastOrUndefined(property => property.property === "background-size");
            if (backgroundSize?.value) {
                return backgroundSize.value;
            }

            // Try to harvest the background size from the background shorthand property.
            const background = properties.lastOrUndefined(property => property.property === "background");
            if (background?.value) {
                // Parse the background.
                const backgroundStyles = parseBackgroundShorthand(background.value);
                return Enumerable.from(backgroundStyles)
                    .where(style => !!style.backgroundSize)
                    .lastOrDefault()?.backgroundSize ?? "";
            }
        }

        return "";
    }

    const watcher = watch(value, (newValue) => {
        if (newValue === "fit-width") {
            setBackgroundSize(backgroundSizeFitWidth);
        }
        else if (newValue === "fit-height") {
            setBackgroundSize(backgroundSizeFitHeight);
        }
        else if (newValue === "original") {
            setBackgroundSize(backgroundSizeOriginal);
        }
        else {
            targetElements.forEach(element => {
                element.style.removeProperty("background-size");
            });
        }
    });

    const provider = {
        get value(): BackgroundSize | null | undefined {
            return value.value;
        },
        set value(newValue: BackgroundSize | null | undefined) {
            value.value = newValue;
        },
        dispose() {
            watcher();
        }
    };

    return provider;
}

export function createBackgroundFitProvider(
    element: HTMLElement,
    copyToElements?: HTMLElement[] | null | undefined
): ValueProvider<BackgroundFit[] | null | undefined> {
    const targetElements = [element, ...(copyToElements ?? [])];

    const value = ref<BackgroundFit[] | null | undefined>(getValue(element));

    function getValue(element: HTMLElement): BackgroundFit[] | null | undefined {
        const backgroundRepeat = element.style.backgroundRepeat;
        const backgroundPosition = element.style.backgroundPosition;

        const properties: BackgroundFit[] = [];

        if (backgroundRepeat === "repeat") {
            properties.push("repeat");
        }

        if (backgroundPosition === "center center") {
            properties.push("center");
        }

        if (!properties.length) {
            const hasStyleProperty = !!backgroundRepeat || !!backgroundPosition;

            if (hasStyleProperty) {
                // Return an empty array if there is an unrecognized style property
                // so that the property control can display a clear button.
                return [];
            }
            else {
                return null;
            }
        }
        else {
            return properties;
        }
    }

    const watcher = watch(value, (newValue, oldValue) => {
        if (newValue) {
            if (newValue.includes("repeat")) {
                targetElements.forEach(element => {
                    element.style.backgroundRepeat = "repeat";
                });
            }
            else if (oldValue?.includes("repeat")) {
                // The old value was "repeat-repeat" and the user deselected it,
                // so change the style explicitly to "no-repeat".
                targetElements.forEach(element => {
                    element.style.backgroundRepeat = "no-repeat";
                });
            }

            if (newValue.includes("center")) {
                targetElements.forEach(element => {
                    element.style.backgroundPosition = "top center";
                });
            }
            else if (oldValue?.includes("center")) {
                // The old value was "center" and the user deselected it,
                // so change the style explicitly to "top left".
                targetElements.forEach(element => {
                    element.style.backgroundPosition = "top left";
                });
            }
        }
        else {
            // The value was set to null so remove all styles.
            // This can happen when a clear button is clicked.
            targetElements.forEach(element => {
                element.style.removeProperty("background-repeat");
                element.style.removeProperty("background-position");
            });
        }
    });

    const provider = {
        get value(): BackgroundFit[] | null | undefined {
            return value.value;
        },
        set value(newValue: BackgroundFit[] | null | undefined) {
            value.value = newValue;
        },
        dispose() {
            watcher();
        }
    };

    return provider;
}

export function createContentWidthProvider(
    element: HTMLElement,
    converter: ValueConverter<number | null | undefined, string | null>
): ValueProvider<number | null | undefined> {
    // Set the numeric value from the element OR from the input CSS styles.
    const {
        innerTable: initialInnerTable
    } = findDescendantContentAreaElements(element);
    const value = ref<number | null | undefined>(findContentWidth(initialInnerTable));

    function findContentWidth(innerTableElement: HTMLElement | undefined): number | null | undefined {
        const width = innerTableElement?.getAttribute("width") ?? null;
        return pixelConverter.toSource(width)
            ?? percentageConverter.toSource(width)
            ?? integerConverter.toSource(width);
    }

    const watcher = watch(value, (newValue) => {
        if (isNullish(newValue)) {
            const { innerTable } = findDescendantContentAreaElements(element);

            if (innerTable) {
                // The table `width` attribute provides a fallback width that many email clients respect.
                innerTable.setAttribute("width", "100%");

                // Restrict the content to the selected number of pixels maximum.
                innerTable.style.maxWidth = "";

                // `width: 100%` ensures responsive scaling on smaller screens.
                innerTable.style.width = "100%";
            }
        }
        else {
            // Update the inline width style of the table data.
            const { innerTable } = addContentAreaElementsIfMissing(element);

            // The table `width` attribute provides a fallback width that many email clients respect.
            innerTable.setAttribute("width", `${newValue}`);

            // Restrict the content to the selected number of pixels maximum.
            innerTable.style.maxWidth = `${converter.toTarget(newValue)}`;

            // `width: 100%` ensures responsive scaling on smaller screens.
            innerTable.style.width = "100%";
        }
    });

    return {
        get value(): number | null | undefined {
            return value.value;
        },
        set value(newValue: number | null | undefined) {
            value.value = newValue;
        },
        dispose() {
            watcher();
        }
    };
}

export const RockStylesCssClass = "rock-styles" as const;
export const RockBodyInnerContainerCssClass = "rock-body-inner-container" as const;
export const RockBodyOuterContainerCssClass = "rock-body-outer-container" as const;

export function createBodyWidthProvider(
    body: HTMLBodyElement
): ValueProvider<number | null | undefined> {

    const { innerTable } = addContentAreaElementsIfMissing(body, {
        innerTableCssClass: RockBodyInnerContainerCssClass,
        outerTableCssClass: RockBodyOuterContainerCssClass
    });

    // `width: 100%` ensures responsive scaling on smaller screens.
    innerTable.style.width = "100%";

    const innerTableWidthAttributeProvider = attributeProvider(innerTable, "width", stringConverter);
    const innerTableWidthStyleSheetProvider = styleSheetProvider(
        body,
        RockStylesCssClass,
        `.${RockBodyInnerContainerCssClass}`,
        "max-width",
        pixelConverter);

    const value = ref<number | null | undefined>(innerTableWidthStyleSheetProvider.value);

    const watcher = watch(value, (newValue) => {
        if (isNullish(newValue)) {
            innerTableWidthAttributeProvider.value = "100%";
            innerTableWidthStyleSheetProvider.value = null;
        }
        else {
            innerTableWidthAttributeProvider.value = `${newValue}`;
            innerTableWidthStyleSheetProvider.value = newValue;
        }
    });

    return {
        get value(): number | null | undefined {
            return value.value;
        },
        set value(newValue: number | null | undefined) {
            value.value = newValue;
        },
        dispose() {
            watcher();
            innerTableWidthAttributeProvider.dispose();
            innerTableWidthStyleSheetProvider.dispose();
        }
    };
}

export function contentAlignmentProvider(
    element: HTMLElement
): ValueProvider<string | null | undefined> {
    // Set the numeric value from the element OR from the input CSS styles.

    const {
        outerTableTd: initialOuterTableTd
    } = findDescendantContentAreaElements(element);
    const value = ref<string | null | undefined>(findContentAlignment(initialOuterTableTd));

    function findContentAlignment(outerTableTd: HTMLElement | undefined): string | null | undefined {
        return outerTableTd?.getAttribute("align");
    }

    const watcher = watch(value, (newValue) => {
        if (isNullish(newValue)) {
            const { outerTableTd } = findDescendantContentAreaElements(element);

            if (outerTableTd) {
                outerTableTd.removeAttribute("align");
            }
        }
        else {
            const { outerTableTd } = addContentAreaElementsIfMissing(element);
            outerTableTd.setAttribute("align", newValue);
        }
    });

    return {
        get value(): string | null | undefined {
            return value.value;
        },
        set value(newValue: string | null | undefined) {
            value.value = newValue;
        },
        dispose() {
            watcher();
        }
    };
}/**
     * Removes temporary wrapper elements from an element's or document's children.
     *
     * This will place the wrapped elements in place of their associated wrappers,
     * and the wrappers will be removed.
     */
export function removeTemporaryWrappers(element: Document | Element): void {
    element.querySelectorAll(`.${RockRuntimeWrapperElementCssClass}`)
        .forEach((wrapper) => {
            if (wrapper.parentNode) {
                // Move all child nodes of the wrapper to the wrapper's parent.
                while (wrapper.firstChild) {
                    wrapper.parentNode.insertBefore(wrapper.firstChild, wrapper);
                }
            }

            // Remove the now-empty wrapper.
            wrapper.remove();
        });
}

/**
     * Removes temporary elements from a document or element.
     */
export function removeTemporaryElements(element: Document | Element): void {
    element.querySelectorAll(`.${RockRuntimeElementCssClass}`)
        .forEach(el => el.remove());
}

/**
 * Removes temporary CSS classes from the children of a document or element children.
 */
export function removeTemporaryClasses(element: Document | Element): void {
    const selector = `[class*="${RockRuntimeClassCssClassPrefix}"]`;

    element.querySelectorAll(selector)
        .forEach(el => {
            const runtimeCssClasses = Enumerable
                .from(el.classList)
                .where(cssClass => cssClass.startsWith(RockRuntimeClassCssClassPrefix));

            el.classList.remove(...runtimeCssClasses);
        });
}

/**
 * Removes temporary attributes from a element's or document's children.
 */
export function removeTemporaryAttributes(element: Document | Element): void {
    const attributesToRemove: string[] = ["draggable", "contenteditable"];

    element.querySelectorAll(`[${attributesToRemove.join("], [")}]`)
        // Strip the Rock runtime CSS classes from the elements.
        .forEach(el => {
            attributesToRemove.forEach(attr => {
                el.removeAttribute(attr);
            });
        });
}

export const EmptyDropzoneSvgPixelWidth = 103;
export const SmallEmptyClass = `${RockRuntimeClassCssClassPrefix}-small` as const;

export function checkDropzoneSize(rect: DOMRectReadOnly, element: HTMLElement): void {
    if (rect.width < EmptyDropzoneSvgPixelWidth) {
        element.classList.add(SmallEmptyClass);
    }
    else {
        element.classList.remove(SmallEmptyClass);
    }
}

export function applyDefaultValueToProvider<T>(
    valueProvider: ValueProvider<T>,
    defaultValue: T
): ValueProvider<T> {
    if (defaultValue !== undefined && isNullish(valueProvider.value)) {
        valueProvider.value = defaultValue;
    }

    return valueProvider;
}

export const DefaultBodyWidth = 600;
export const DefaultBodyAlignment = "center";

export function createBodyBackgroundColorProvider(
    body: HTMLBodyElement
): ValueProvider<string | null | undefined> {
    // Set the value from the element OR from the input CSS styles.
    addContentAreaElementsIfMissing(body, {
        innerTableCssClass: RockBodyInnerContainerCssClass,
        outerTableCssClass: RockBodyOuterContainerCssClass
    });

    const backgroundColorStyleSheetProvider = styleSheetProvider(
        body,
        RockStylesCssClass,
        `.${RockBodyInnerContainerCssClass}`,
        "background-color",
        stringConverter);

    const value = ref<string | null | undefined>(backgroundColorStyleSheetProvider.value);

    const watcher = watch(value, (newValue) => {
        backgroundColorStyleSheetProvider.value = newValue;
    });

    return {
        get value(): string | null | undefined {
            return value.value;
        },
        set value(newValue: string | null | undefined) {
            value.value = newValue;
        },
        dispose() {
            watcher();
            backgroundColorStyleSheetProvider.dispose();
        }
    };
}

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
    Ref,
} from "vue";
import {
    AccordionManager,
    BorderStyle,
    CssStyleDeclarationKebabKey,
    ContentAreaElements,
    DomWatcher,
    EditorComponentTypeName,
    HorizontalAlignment,
    StyleSheetElements,
    ValueConverter
} from "./types.partial";
import { isDocument, isElement, isHTMLElement } from "@Obsidian/Utility/dom";
import { newGuid } from "@Obsidian/Utility/guid";
import { Enumerable } from "@Obsidian/Utility/linq";
import { splitCase, toTitleCase } from "@Obsidian/Utility/stringUtils";
import { isNullish } from "@Obsidian/Utility/util";

// #region Constants

export const AccordionGroupInjectionKey: InjectionKey<AccordionManager> = Symbol("accordion-group");

export const EmptyDropzoneSvgPixelWidth = 103;
export const DefaultBodyWidth = 600;
export const DefaultBodyAlignment = "center";
export const DefaultBodyColor = "#ffffff";
export const DefaultEmailBackgroundColor = "#e7e7e7";

export const RockStylesCssClass = "rock-styles" as const;

/**
 * The outermost wrapper for the entire email.
 * - **Purpose:** Defines the full email structure.
 * - **Usage:** Apply global styles (e.g., background color).
 * - **Best Practice:** There should be exactly **one** `.email-wrapper` per email.
 */
export const EmailWrapperCssClass = "email-wrapper" as const;

/**
 * Represents a **full-width row** within the email.
 * - **Purpose:** Defines sections with independent background styling.
 * - **Usage:** Can be repeated to create multiple sections.
 * - **Best Practice:** Each `.email-row` should contain exactly **one** `.email-row-content`.
 */
export const EmailRowCssClass = "email-row" as const;

/**
 * The **content container inside a row**, which constrains the width.
 * - **Purpose:** Centers content and limits its width (e.g., `600px`).
 * - **Usage:** Nested inside `.email-row` to maintain content alignment.
 * - **Best Practice:** Wrap all email components within `.email-row-content`.
 */
export const EmailRowContentCssClass = "email-row-content" as const;

/**
 * A wrapper for **individual content components** (e.g., text, buttons, images).
 * - **Purpose:** Ensures each component is properly structured within an email.
 * - **Usage:** Each `div.component-<type>` should wrap exactly **one** `.email-content` table.
 * - **Best Practice:** Apply this class to all modular email components.
 */
export const EmailContentCssClass = "email-content" as const;

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

export const SmallEmptyClass = `${RockRuntimeClassCssClassPrefix}-small` as const;

export const GlobalStylesCssSelectors = {
    backgroundColor: `.${EmailWrapperCssClass}`,

    bodyWidth: `.${EmailRowContentCssClass}`,
    bodyColor: `.${EmailRowContentCssClass}`,
    bodyPadding: `.${EmailRowContentCssClass} > tbody > tr > td, .${EmailRowContentCssClass} > tr > td`,
    bodyAlignment: `.${EmailRowCssClass} > tbody > tr > td, .${EmailRowCssClass} > tr > td`,
    bodyBorderStyling: `.${EmailRowContentCssClass}`,
    bodyMargin: `.${EmailWrapperCssClass} > tbody > tr > td, .${EmailWrapperCssClass} > tr > td`,

    globalTextStyling: `body, .${EmailWrapperCssClass} > tbody > tr > td, .${EmailWrapperCssClass} > tr > td`,

    heading1TextStyling: `.component-title h1`,
    heading1Margin: `.component-title h1`,
    heading1Padding: `.component-title h1`,
    heading1BorderStyling: `.component-title h1`,

    heading2TextStyling: `.component-title h2`,
    heading2Margin: `.component-title h2`,
    heading2Padding: `.component-title h2`,
    heading2BorderStyling: `.component-title h2`,

    heading3TextStyling: `.component-title h3`,
    heading3Margin: `.component-title h3`,
    heading3Padding: `.component-title h3`,
    heading3BorderStyling: `.component-title h3`,

    paragraphTextStyling: `.component-text`,
    paragraphMargin: `.component-text`,

    buttonBackgroundColor: `.component-button .button-link`,
    buttonTextStyling: `.component-button .button-link`,
    buttonCornerRadius: `.component-button .button-content, .component-button .button-link`,
    buttonPadding: `.component-button .button-link`,
    buttonMargin: `.component-button .button-innerwrap`,
    buttonBorderStyling: `.component-button .button-link`,
    buttonWidthValuesShell: `.component-button .button-shell, .component-rsvp .rsvp-button-shell`,
    buttonWidthValuesButton: `.component-button .button-link, .component-rsvp .rsvp-accept-link, .component-rsvp .rsvp-decline-link`,

    dividerStyle: `.component-divider hr`,
    dividerThickness: `.component-divider hr`,
    dividerColor: `.component-divider hr`,
    dividerWidth: `.component-divider hr`,
    dividerHorizontalAlignment: `.component-divider hr`,
    dividerPadding: `.component-divider hr`,
} as const;

// #endregion Constants

// #region Converters

export const numberToStringConverter: ValueConverter<number | null | undefined, string | null> = {
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

// #endregion Converters

// #region Functions

/** Uses an accordion group if one is set up. */
export function useAccordionGroup(isExpanded: Ref<boolean>): void {
    const accordionKey = newGuid();
    const group = inject(AccordionGroupInjectionKey);
    group?.register(accordionKey, isExpanded);
}

export function getComponentTypeName(componentElement: HTMLElement): EditorComponentTypeName {
    const classList = [...componentElement.classList];

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
        "component-video": "video",
        "component-row": "row"
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
        case "row":
            return `
<svg viewBox="0 0 512 512" fill="none" xmlns="http://www.w3.org/2000/svg" style="width: 40px; display: block;">
    <path fill-rule="evenodd" clip-rule="evenodd" d="M48 32H464C490.5 32 512 53.5 512 80V432C512 458.5 490.5 480 464 480H48C21.5 480 0 458.5 0 432V80C0 53.5 21.5 32 48 32ZM464 81H49V431H464V81Z" style="fill: var(--color-interface-strong)"></path>
    <path d="M49 187H464V326H49V187Z" style="fill: var(--color-interface-soft)"></path>
</svg>`;
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
        case "row":
            return "Row";
        default:
            console.warn(`Unable to retrieve the title for the unknown component type, '${componentTypeName}'. Returning the default icon.`);
            return toTitleCase(splitCase(componentTypeName).replace("-", " "));
    }
}

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

/**
 * Creates an `.email-content` table element with provided inner content.
 * Allows customization of the `<td>` cell styles (e.g., padding for sections).
 *
 * @param contentHtml Inner HTML string to place inside the `<td>` cell.
 * @returns A table element with `.email-content` structure.
 */
export function createEmailContentTable(contentHtml: string | Enumerable<Element>): HTMLTableElement {
    const contentTable = document.createElement("table");
    contentTable.classList.add(EmailContentCssClass);
    contentTable.setAttribute("border", "0");
    contentTable.setAttribute("cellpadding", "0");
    contentTable.setAttribute("cellspacing", "0");
    contentTable.setAttribute("width", "100%");
    contentTable.setAttribute("role", "presentation");

    const contentBody = document.createElement("tbody");
    const contentRow = document.createElement("tr");
    const contentCell = document.createElement("td");

    if (typeof contentHtml === "string") {
        contentCell.innerHTML = contentHtml;
    }
    else {
        contentHtml.forEach(el => {
            contentCell.appendChild(el);
        });
    }

    contentRow.appendChild(contentCell);
    contentBody.appendChild(contentRow);
    contentTable.appendChild(contentBody);

    return contentTable;
}

/**
 * Creates `.email-row`, `.email-row-content`, and `.email-content` table elements with provided inner content.
 * Allows customization of the `<td>` cell styles (e.g., padding for sections).
 *
 * @param contentHtml Inner HTML string to place inside the `<td>` cell.
 * @param cellStyle Optional inline styles to apply to the `<td>` (e.g., padding).
 * @returns A table element with `.email-content` structure.
 */
export function createEmailRowTable<T extends Element>(contentHtml: string | Enumerable<T>): HTMLTableElement {
    // Create `.email-row` (full width row)
    const rowTable = document.createElement("table");
    rowTable.classList.add(EmailRowCssClass);
    rowTable.setAttribute("border", "0");
    rowTable.setAttribute("cellpadding", "0");
    rowTable.setAttribute("cellspacing", "0");
    rowTable.setAttribute("width", "100%");
    rowTable.setAttribute("role", "presentation");

    const rowTbody = document.createElement("tbody");
    const rowTr = document.createElement("tr");
    const rowTd = document.createElement("td");
    rowTd.setAttribute("align", "center");

    // Create `.email-row-content` (600px constrained content area)
    const rowContentTable = document.createElement("table");
    rowContentTable.classList.add(EmailRowContentCssClass);
    rowContentTable.setAttribute("border", "0");
    rowContentTable.setAttribute("cellpadding", "0");
    rowContentTable.setAttribute("cellspacing", "0");
    rowContentTable.setAttribute("width", `${DefaultBodyWidth}`);
    rowContentTable.setAttribute("role", "presentation");

    const rowContentTbody = document.createElement("tbody");
    const rowContentTr = document.createElement("tr");
    const rowContentTd = document.createElement("td");

    if (typeof contentHtml === "string") {
        rowContentTd.innerHTML = contentHtml;
    }
    else {
        contentHtml.forEach((el) => {
            rowContentTd.appendChild(el);
        });
    }

    // Build `.email-row-content` structure
    rowContentTr.appendChild(rowContentTd);
    rowContentTbody.appendChild(rowContentTr);
    rowContentTable.appendChild(rowContentTbody);

    // Build `.email-row` structure
    rowTd.appendChild(rowContentTable);
    rowTr.appendChild(rowTd);
    rowTbody.appendChild(rowTr);
    rowTable.appendChild(rowTbody);

    return rowTable;
}

/**
 * Ensures the document body is wrapped in an `.email-wrapper` table.
 * Moves existing body contents into the wrapper if not present and also adds `.email-row` and `.email-row-content` tables.
 *
 * @param document The HTML document to normalize.
 */
export function ensureBodyWrapsEmailWrapper(document: Document): HTMLTableElement {
    const existingWrapper = document.querySelector(`table.${EmailWrapperCssClass}`) as HTMLTableElement;

    if (existingWrapper) {
        console.info("Email wrapper table already exists. Skipping.");
        return existingWrapper;
    }

    // Create `.email-wrapper` table structure
    const wrapperTable = document.createElement("table");
    wrapperTable.classList.add(EmailWrapperCssClass);
    wrapperTable.setAttribute("border", "0");
    wrapperTable.setAttribute("cellpadding", "0");
    wrapperTable.setAttribute("cellspacing", "0");
    wrapperTable.setAttribute("width", "100%");
    wrapperTable.setAttribute("role", "presentation");
    wrapperTable.style.minWidth = "100%";
    wrapperTable.style.height = "100%"; // Forces full-height behavior

    const wrapperTbody = document.createElement("tbody");
    const wrapperRow = document.createElement("tr");
    const wrapperCell = document.createElement("td");
    wrapperCell.setAttribute("align", "center");
    wrapperCell.setAttribute("valign", "top"); // Prevent content from being squashed
    wrapperCell.style.height = "100%"; // Ensures row stretches

    // Create `.email-row` (full width row)

    // Build `.email-wrapper` structure
    Enumerable
        .from([...document.body.children])
        .forEach((el) => {
            wrapperCell.appendChild(el);
        });
    wrapperRow.appendChild(wrapperCell);
    wrapperTbody.appendChild(wrapperRow);
    wrapperTable.appendChild(wrapperTbody);

    // Append the table back to the body
    document.body.appendChild(wrapperTable);

    return wrapperTable;
}

export function ensureComponentWrapsContentTableRecursive(root: Document | Element): void {
    if (!isDocument(root) && root.matches(".component:not(.component-row)")) {
        ensureComponentWrapsContentTable(root);
    }

    Enumerable
        .from(root.querySelectorAll(":scope .component:not(.component-row)"))
        .forEach(childComponent => {
            ensureComponentWrapsContentTable(childComponent);
        });
}

export function ensureComponentWrapsContentTable(componentElement: Element): HTMLTableElement {
    if (!componentElement.classList.contains("component")) {
        throw new Error("Element is not a valid component element.");
    }

    // It's safe to assume HTMLTableElement here because of the selector.
    const existingEmailContentTable = componentElement.querySelector(`:scope > table.${EmailContentCssClass}`) as HTMLTableElement;

    if (existingEmailContentTable) {
        return existingEmailContentTable;
    }

    const emailContentTable = createEmailContentTable(Enumerable.from(componentElement.children));

    componentElement.appendChild(emailContentTable);

    // TODO Need custom handlers to move component-specific styles to the correct table elements.

    return emailContentTable;
}

export function createComponentElement(document: Document, componentTypeName: EditorComponentTypeName): HTMLElement {
    const componentTypeCssClass = `component-${componentTypeName.endsWith("-section") ? "section" : componentTypeName}`;
    const componentElement = document.createElement("div");
    componentElement.classList.add("component", componentTypeCssClass);
    componentElement.dataset.state = "component";

    // Inner HTML for the specific component.
    let componentInnerHtml = "";

    // Notes:
    //  - Inline styles defined here will be at the component level instead of the global level.
    //  - Ensure inline styles can be edited, if desired. Otherwise, styles added here will not be
    //    editable by the individual using the editor.
    //  - Rock CSS variables should not be used here as these will be added to the HTML email.
    //  - Global style defaults are maintained in the emailIFrame.partial.obs file.
    switch (componentTypeName) {
        case "title":
            componentInnerHtml = `<h1 class="${RockCssClassContentEditable}">Title</h1>`;
            break;

        case "video":
            componentInnerHtml = `<a href=""><img src="/Assets/Images/video-placeholder.jpg" data-imgcsswidth="full" style="width: 100%;"></a>`;
            break;

        case "button":
            componentElement.classList.add("v2");

            // The styles and attributes here will default each button to have a "Fit To Text" width (see buttonWidthProperty.partial.obs for details).
            componentInnerHtml = `<table class="button-outerwrap" border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%" style="min-width: 100%;">
                <tbody>
                    <tr>
                        <td class="button-innerwrap" align="center" valign="top">
                            <table class="button-shell" border="0" cellpadding="0" cellspacing="0" role="presentation">
                                <tbody>
                                    <tr>
                                        <td class="button-content" align="center" valign="middle">
                                            <a class="button-link ${RockCssClassContentEditable}" href="https://" rel="noopener noreferrer" title="Click Me">Click Me</a>
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
            componentInnerHtml = `<p class="${RockCssClassContentEditable}">Let's see what you have to say!</p>`;
            break;
        case "divider":
            componentInnerHtml = `<hr />`;
            break;
        case "message":
            componentElement.classList.add(RockCssClassContentEditable);
            componentInnerHtml = "Message";
            break;
        case "image":
            // Image component needs a line-height of 0 to remove extra space under image.
            componentElement.style.lineHeight = "0";
            componentInnerHtml = `<img alt="" src="/Assets/Images/image-placeholder.jpg" data-imgcsswidth="full" style="width: 100%;">`;
            break;
        case "code":
            componentElement.classList.add(RockCssClassContentEditable);
            componentInnerHtml = `Add your code here...`;
            break;
        case "rsvp":
            componentInnerHtml = `<table class="rsvp-outerwrap" border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%" style="min-width: 100%;">
                <tbody>
                    <tr>
                        <td class="rsvp-innerwrap" align="center" valign="top" style="padding: 0;">
                            <table border="0" cellpadding="0" cellspacing="0" role="presentation">
                                <tbody>
                                    <tr>
                                        <td>
                                            <table class="accept-button-shell" border="0" cellpadding="0" cellspacing="0" role="presentation" style="background-color: #16C98D; border-collapse: separate; border-radius: 3px; display: inline-table;">
                                                <tbody>
                                                    <tr>
                                                        <td class="rsvp-accept-content" align="center" valign="middle">
                                                            <a class="rsvp-accept-link ${RockCssClassContentEditable}" href="https://" rel="noopener noreferrer" title="Accept" style="color: #FFFFFF; display: inline-block; font-family: Arial; font-size: 16px; font-weight: bold; letter-spacing: normal; padding: 15px; text-align: center; text-decoration: none; border-bottom-width: 0;">Accept</a>
                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                        </td>
                                        <td style="padding-left: 10px;">
                                            <table class="decline-button-shell" border="0" cellpadding="0" cellspacing="0" role="presentation" style="background-color: #D4442E; border-collapse: separate; border-radius: 3px; display: inline-table;">
                                                <tbody>
                                                    <tr>
                                                        <td class="rsvp-decline-content" align="center" valign="middle">
                                                            <a class="rsvp-decline-link ${RockCssClassContentEditable}" href="https://" rel="noopener noreferrer" title="Decline" style="color: #FFFFFF; display: inline-block; font-family: Arial; font-size: 16px; font-weight: bold; letter-spacing: normal; padding: 15px; text-align: center; text-decoration: none; border-bottom-width: 0;">Decline</a>
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

        // Section Components
        case "section":
        case "one-column-section":
        case "two-column-section":
        case "three-column-section":
        case "four-column-section":
        case "right-sidebar-section":
        case "left-sidebar-section":
            componentInnerHtml = `
                <table class="row" cellpadding="0" cellspacing="0" border="0" role="presentation" style="width: 100%;">
                    <tr>
                        ${getSectionColumns(componentTypeName)}
                    </tr>
                </table>`;
            break;
        case "row":
            componentInnerHtml = `<div class="dropzone"></div>`;
            break;
        default:
            throw new Error(`Unknown typeName: ${componentTypeName}`);
    }

    // Fill cell with inner content
    const contentTable = createEmailContentTable(componentInnerHtml);

    if (componentTypeName !== "row") {
        componentElement.appendChild(contentTable);
    }
    else {
        const rowTable = createEmailRowTable(Enumerable.from([contentTable]));

        componentElement.appendChild(rowTable);
    }

    return componentElement;
}

function getSectionColumns(componentTypeName: EditorComponentTypeName): string {
    switch (componentTypeName) {
        case "one-column-section":
            return `<td class="dropzone columns small-12 start last large-12" valign="top" width="100%"></td>`;
        case "right-sidebar-section":
            return `<td class="dropzone columns small-12 start large-8" valign="top" width="66.666666%"></td>
                    <td class="dropzone columns small-12 last large-4" valign="top" width="33.333333%"></td>`;
        case "left-sidebar-section":
            return `<td class="dropzone columns small-12 start large-4" valign="top" width="33.333333%"></td>
                    <td class="dropzone columns small-12 last large-8" valign="top" width="66.666666%"></td>`;
        case "two-column-section":
            return `<td class="dropzone columns small-12 start large-6" valign="top" width="50%"></td>
                    <td class="dropzone columns small-12 last large-6" valign="top" width="50%"></td>`;
        case "three-column-section":
            return `<td class="dropzone columns small-12 start large-4" valign="top" width="33.333333%"></td>
                    <td class="dropzone columns small-12 large-4" valign="top" width="33.333333%"></td>
                    <td class="dropzone columns small-12 last large-4" valign="top" width="33.333333%"></td>`;
        case "four-column-section":
            return `<td class="dropzone columns small-12 start large-3" valign="top" width="25%"></td>
                    <td class="dropzone columns small-12 large-3" valign="top" width="25%"></td>
                    <td class="dropzone columns small-12 large-3" valign="top" width="25%"></td>
                    <td class="dropzone columns small-12 last large-3" valign="top" width="25%"></td>`;
        default:
            return `<div class="dropzone"></div>`;
    }
}

export function getComponentHostSelector(componentTypeName: EditorComponentTypeName): string {
    switch (componentTypeName) {
        case "row": return ".structure-dropzone";
        case "section": return ".dropzone, .structure-dropzone";
        case "one-column-section": return ".dropzone, .structure-dropzone";
        case "two-column-section": return ".dropzone, .structure-dropzone";
        case "three-column-section": return ".dropzone, .structure-dropzone";
        case "four-column-section": return ".dropzone, .structure-dropzone";
        case "left-sidebar-section": return ".dropzone, .structure-dropzone";
        case "right-sidebar-section": return ".dropzone, .structure-dropzone";
        case "button": return ".dropzone";
        case "code": return ".dropzone";
        case "divider": return ".dropzone";
        case "image": return ".dropzone";
        case "message": return ".dropzone";
        case "rsvp": return ".dropzone";
        case "text": return ".dropzone";
        case "title": return ".dropzone";
        case "video": return ".dropzone";
    }
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
    function filterTableMatches(element: Element): boolean {
        const outerTableCssClassesToIgnore = ["button-outerwrap", "header", "spacer"] as const;

        return !outerTableCssClassesToIgnore.some(ignoreCssClass => element.classList.contains(ignoreCssClass))  // Exclude tables with classes
            && !outerTableCssClassesToIgnore.some(ignoreCssClass => element.closest(`table.${ignoreCssClass}`));    // Exclude tables nested within a table with classes
    }

    // First look for new, and legacy outer table elements that have the .container CSS class.
    searchResult.outerTable = Enumerable
        .from(element.querySelectorAll(":scope table.container"))
        .where(filterTableMatches)
        .ofType(isHTMLElement)
        .firstOrDefault();

    // Look for any table that is a direct descendant of the root element.
    if (!searchResult.outerTable) {
        searchResult.outerTable = Enumerable
            .from(element.querySelectorAll(":scope > table"))
            .where(filterTableMatches)
            .ofType(isHTMLElement)
            .firstOrDefault();
    }

    if (searchResult.outerTable) {
        searchResult.outerTableBody = searchResult.outerTable.querySelector(":scope > tbody") as HTMLElement ?? undefined;
        searchResult.outerTableTr = searchResult.outerTable.querySelector(":scope > tbody > tr, :scope > tr") as HTMLElement ?? undefined;

        if (searchResult.outerTableTr) {
            searchResult.outerTableTd = searchResult.outerTableTr.querySelector("td, th") as HTMLElement ?? undefined;

            if (searchResult.outerTableTd) {
                const innerTableCssClassesToIgnore = ["header", "spacer"] as const;
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
        tableElements.outerTable.setAttribute("role", "presentation");
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
        tableElements.innerTable.setAttribute("role", "presentation");
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

/**
 * Finds the first CSS style sheet where the <style class> matches the `styleCssClass`
 * and the ruleset selector matches the `cssRulesetSelectors`.
 */
export function findElements(element: Element, styleCssClass: string, rulesetCssSelector: string): StyleSheetElements | undefined {
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

export function createElements(element: Element, styleCssClass: string): StyleSheetElements | undefined {
    const elementDocument = element.ownerDocument;
    const elementWindow = elementDocument.defaultView;

    if (elementWindow) {
        // Add the stylesheet since it's missing.
        const styleElement = elementDocument.createElement("style") as HTMLStyleElement;
        styleElement.classList.add(styleCssClass);

        // Add it as the first element.
        element.insertBefore(styleElement, element.firstChild);

        return {
            elementDocument,
            elementWindow,
            styleElement,
            styleSheet: styleElement.sheet!
        };
    }
}

export function addRuleset(elements: StyleSheetElements, rulesetCssSelector: string): CSSStyleRule {
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

export function updateStyleElementTextContent(elements: StyleSheetElements): void {
    const { styleElement, styleSheet } = elements;

    // The previous ruleset change only affects the style in memory.
    // Update the HTML <style> element contents.
    styleElement.textContent = Enumerable.from(styleSheet.cssRules)
        .select(rule => rule.cssText)
        .aggregate((rules, rule, i) => i !== 0 ? `${rules}\n${rule}` : rule, "");
}

export function createDomWatcher(
    root: Document | Element,
    selector: string,
    { includeSelf }: { includeSelf?: string | boolean; } = {}
): DomWatcher {
    const foundElements = new Set<Element>();
    let onFoundCallbacks: ((element: Element) => void)[] = [];
    let onRemovedCallbacks: ((element: Element) => void)[] = [];

    function updateMatches(): void {
        const newMatches = new Set(root.querySelectorAll(selector));

        // Handle newly found elements
        newMatches.forEach((el) => {
            if (!foundElements.has(el)) {
                foundElements.add(el);
                onFoundCallbacks.forEach((cb) => cb(el));
            }
        });

        // Handle removed elements
        foundElements.forEach((el) => {
            if (!newMatches.has(el)) {
                foundElements.delete(el);
                onRemovedCallbacks.forEach((cb) => cb(el));
            }
        });
    }

    const observer = new MutationObserver((mutations) => {
        let shouldRevalidate = false;

        mutations.forEach((mutation) => {
            if (mutation.type === "childList") {
                shouldRevalidate = true; // Always revalidate for added/removed nodes
            }

            if (mutation.type === "attributes") {
                const attrName = mutation.attributeName || "";
                if (attrName === "class" || attrName.startsWith("data-")) {
                    shouldRevalidate = true; // Only revalidate for relevant attributes
                }
            }
        });

        if (shouldRevalidate) {
            updateMatches();
        }
    });

    observer.observe(root, {
        childList: true,
        subtree: true,
        attributes: true
    });

    if (includeSelf) {
        if (isElement(root) && root.matches(typeof includeSelf === "boolean" ? selector : includeSelf)) {
            foundElements.add(root);
        }
    }

    // Initial match check
    updateMatches();

    return {
        get foundElements() {
            return Enumerable.from(foundElements);
        },

        onElementFound(callback: (element: Element) => void): void {
            onFoundCallbacks.push(callback);
            foundElements.forEach(callback);
        },

        onElementRemoved(callback: (element: Element) => void): void {
            onRemovedCallbacks.push(callback);
        },

        dispose(): void {
            observer.disconnect();
            foundElements.clear();
            onFoundCallbacks = [];
            onRemovedCallbacks = [];
        }
    };
}

/**
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

export function checkDropzoneSize(rect: DOMRectReadOnly, element: HTMLElement): void {
    if (rect.width < EmptyDropzoneSvgPixelWidth) {
        element.classList.add(SmallEmptyClass);
    }
    else {
        element.classList.remove(SmallEmptyClass);
    }
}

// #endregion Functions
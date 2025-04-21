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
    ComponentMigrationHelper,
    ComponentTypeName,
    ContentAreaElements,
    DomWatcher,
    EditorComponentTypeName,
    HorizontalAlignment,
    StyleSheetElements,
    ValueConverter,
    ComponentStructure,
    TableElements
} from "./types.partial";
import { isElement, isHTMLElement, isHTMLTableElement } from "@Obsidian/Utility/dom";
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

export function getComponentCssClass(componentTypeName: ComponentTypeName): string {
    return `component-${componentTypeName}`;
}

export function getMarginWrapperTableCssClass(componentTypeName: ComponentTypeName, wrapperSuffix: string = ""): string {
    return `margin-wrapper-for-${componentTypeName}${wrapperSuffix}`;
}

export function getMarginWrapperTableSelector(componentTypeName: ComponentTypeName, wrapperSuffix: string = ""): string {
    return `.${getMarginWrapperTableCssClass(componentTypeName, wrapperSuffix)}`;
}

export function getMarginWrapperCellSelector(componentTypeName: ComponentTypeName, wrapperSuffix: string = ""): string {
    return `.${getMarginWrapperTableCssClass(componentTypeName, wrapperSuffix)} > tbody > tr > td`;
}

export function getBorderWrapperTableCssClass(componentTypeName: ComponentTypeName, wrapperSuffix: string = ""): string {
    return `border-wrapper-for-${componentTypeName}${wrapperSuffix}`;
}

export function getBorderWrapperTableSelector(componentTypeName: ComponentTypeName, wrapperSuffix: string = ""): string {
    return `.${getBorderWrapperTableCssClass(componentTypeName, wrapperSuffix)}`;
}

export function getBorderWrapperCellSelector(componentTypeName: ComponentTypeName, wrapperSuffix: string = ""): string {
    return `.${getBorderWrapperTableCssClass(componentTypeName, wrapperSuffix)} > tbody > tr > td`;
}

export function getPaddingWrapperTableCssClass(componentTypeName: ComponentTypeName, wrapperSuffix: string = ""): string {
    return `padding-wrapper-for-${componentTypeName}${wrapperSuffix}`;
}

export function getPaddingWrapperTableSelector(componentTypeName: ComponentTypeName, wrapperSuffix: string = ""): string {
    return `.${getPaddingWrapperTableCssClass(componentTypeName, wrapperSuffix)}`;
}

export function getPaddingWrapperCellSelector(componentTypeName: ComponentTypeName, wrapperSuffix: string = ""): string {
    return `.${getPaddingWrapperTableCssClass(componentTypeName, wrapperSuffix)} > tbody > tr > td`;
}

type ContentWrapperSupportedComponentType = Extract<ComponentTypeName, "text" | "code">;

export function getContentWrapperCssClass(componentTypeName: ContentWrapperSupportedComponentType, wrapperSuffix: string = ""): string {
    return `content-wrapper-for-${componentTypeName}${wrapperSuffix}`;
}

export function getContentWrapperSelector(componentTypeName: ContentWrapperSupportedComponentType, wrapperSuffix: string = ""): string {
    return `.${getContentWrapperCssClass(componentTypeName, wrapperSuffix)}`;
}

export const GlobalStylesCssSelectors = {
    backgroundColor: `.${EmailWrapperCssClass}`,

    bodyWidth: getBorderWrapperTableSelector("row"),
    bodyColor: getBorderWrapperCellSelector("row"),
    bodyPadding: getPaddingWrapperCellSelector("row"),
    bodyAlignment: getMarginWrapperCellSelector("row"),
    bodyBorderStyling: getBorderWrapperCellSelector("row"),
    bodyMargin: getMarginWrapperCellSelector("row"),

    globalTextStyling: `body, .${EmailWrapperCssClass} > tbody > tr > td`,

    heading1TextStyling: `.component-title h1`,
    heading1Margin: `${getMarginWrapperCellSelector("title", "-h1")}`,
    heading1Padding: `${getPaddingWrapperCellSelector("title", "-h1")}`,
    heading1BorderStyling: `${getBorderWrapperCellSelector("title", "-h1")}`,

    heading2TextStyling: `.component-title h2`,
    heading2Margin: `${getMarginWrapperCellSelector("title", "-h2")}`,
    heading2Padding: `${getPaddingWrapperCellSelector("title", "-h2")}`,
    heading2BorderStyling: `${getBorderWrapperCellSelector("title", "-h2")}`,

    heading3TextStyling: `.component-title h3`,
    heading3Margin: `${getMarginWrapperCellSelector("title", "-h3")}`,
    heading3Padding: `${getPaddingWrapperCellSelector("title", "-h3")}`,
    heading3BorderStyling: `${getBorderWrapperCellSelector("title", "-h3")}`,

    paragraphTextStyling: getContentWrapperSelector("text"),
    paragraphMargin: getMarginWrapperCellSelector("text"),

    buttonBackgroundColor: `.component-button .button-link`,
    buttonTextStyling: `.component-button .button-link`,
    buttonCornerRadius: getBorderWrapperCellSelector("button"),
    // Padding needs to be applied to the anchor element instead of the padding-wrapper cell.
    buttonPadding: `.component-button .button-link`,
    buttonMargin: getMarginWrapperCellSelector("button"),
    buttonBorderStyling: getBorderWrapperCellSelector("button"),
    buttonWidthValuesShell: `.component-button .button-shell, .component-rsvp .rsvp-button-shell`,
    buttonWidthValuesButton: `.component-button .button-link, .component-rsvp .rsvp-accept-link, .component-rsvp .rsvp-decline-link`,

    dividerMargin: getMarginWrapperCellSelector("divider"),
    dividerWidth: getBorderWrapperTableSelector("divider"),
    dividerStyle: getPaddingWrapperCellSelector("divider"),
    dividerThickness: getPaddingWrapperCellSelector("divider"),
    dividerColor: getPaddingWrapperCellSelector("divider"),
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

export function getComponentTypeName(componentElement: Element): ComponentTypeName {
    const classList = [...componentElement.classList];

    const map: Record<string, ComponentTypeName> = {
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
<svg viewBox="70 93 373 326" fill="none" xmlns="http://www.w3.org/2000/svg" style="width: 32px; display: block;">
    <path fill-rule="evenodd" clip-rule="evenodd" d="M104.929 93H407.643C426.926 93 442.571 108.645 442.571 127.929V384.071C442.571 403.355 426.926 419 407.643 419H104.929C85.6451 419 70 403.355 70 384.071V127.929C70 108.645 85.6451 93 104.929 93ZM407.641 128.656H105.655V383.344H407.641V128.656Z" style="fill: var(--color-interface-strong);"></path>
    <path d="M106.141 206H408.141V306H106.141V206Z" style="fill: var(--color-interface-soft)"></path>
</svg>
`;
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
 * @param cellInnerHtml Inner HTML string to place inside the `<td>` cell.
 * @returns A table element with `.email-content` structure.
 */
export function createTable<T extends Element>(cellInnerHtml?: string | Enumerable<T> | T[] | undefined): { table: HTMLTableElement, tbody: HTMLTableSectionElement, tr: HTMLTableRowElement, td: HTMLTableCellElement } {
    const table = document.createElement("table");
    table.setAttribute("border", "0");
    table.setAttribute("cellpadding", "0");
    table.setAttribute("cellspacing", "0");
    table.setAttribute("width", "100%");
    table.setAttribute("role", "presentation");

    const tbody = document.createElement("tbody");
    const tr = document.createElement("tr");
    const td = document.createElement("td");

    if (typeof cellInnerHtml === "string") {
        td.innerHTML = cellInnerHtml;
    }
    else if (cellInnerHtml !== undefined) {
        cellInnerHtml.forEach(el => {
            td.appendChild(el);
        });
    }

    tr.appendChild(td);
    tbody.appendChild(tr);
    table.appendChild(tbody);

    return {
        table,
        tbody,
        tr,
        td
    };
}

/**
 * Creates a standard component structure with margin, border, and padding wrappers.
 * @param componentCssClass - The CSS class for the component.
 * @param componentVersion - The version of the component.
 * @param componentInnerHtml - Optional inner HTML for the component.
 * @returns
 */
export function createComponent(
    componentTypeName: Exclude<EditorComponentTypeName,
        | "one-column-section"
        | "two-column-section"
        | "three-column-section"
        | "four-column-section"
        | "right-sidebar-section"
        | "left-sidebar-section"
    >,
    componentVersion: string,
    componentInnerHtml?: string | Enumerable<Element> | undefined
): ComponentStructure {
    const wrappers = createElementWrappers(componentInnerHtml);

    const marginWrapper = wrappers.marginWrapper;
    marginWrapper.table.classList.add(`margin-wrapper-for-${componentTypeName}`);
    marginWrapper.table.classList.add("component", `component-${componentTypeName}`);
    marginWrapper.table.dataset.state = "component";
    setComponentVersionNumber(marginWrapper.table, componentVersion);

    const borderWrapper = marginWrapper.borderWrapper;
    borderWrapper.table.classList.add(`border-wrapper-for-${componentTypeName}`);

    const paddingWrapper = borderWrapper.paddingWrapper;
    paddingWrapper.table.classList.add(`padding-wrapper-for-${componentTypeName}`);

    return {
        marginWrapper: {
            ...marginWrapper,
            borderWrapper: {
                ...borderWrapper,
                paddingWrapper
            }
        }
    };
}
export function createElementWrappers(
    componentInnerHtml?: string | Enumerable<Element> | undefined
): ComponentStructure {
    const paddingWrapper = createTable(componentInnerHtml);
    paddingWrapper.table.classList.add("padding-wrapper");

    const borderWrapper = createTable([paddingWrapper.table]);
    borderWrapper.table.classList.add("border-wrapper");
    // Important! To ensure border-radius is applied to the border,
    // the table must have border-collapse: separate.
    borderWrapper.table.style.setProperty("border-collapse", "separate", "important");
    // Set overflow hidden so corner radius is applied to the border.
    borderWrapper.td.style.overflow = "hidden";

    const marginWrapper = createTable([borderWrapper.table]);
    marginWrapper.table.classList.add("margin-wrapper");

    return {
        marginWrapper: {
            ...marginWrapper,
            borderWrapper: {
                ...borderWrapper,
                paddingWrapper
            }
        }
    };
}

/**
 * Finds the first descendant set of table elements matching the selector or null if some are not found.
 */
function findTable(element: Element | Document, tableSelector: string): TableElements | null {
    const { table, tbody, tr, td } = findTablePartial(element, tableSelector);

    if (table && tbody && tr && td) {
        return {
            table,
            tbody,
            tr,
            td
        };
    }
    else {
        return null;
    }
}

/**
 * Finds the first descendant set of table elements matching the selector.
 *
 * May return undefined for table elements that aren't found.
 */
function findTablePartial(element: Element | Document, tableSelector: string): Partial<TableElements> {
    let table = (isElement(element) && element.matches(tableSelector))
        ? element
        : (element.querySelector(tableSelector) ?? undefined);

    if (table && !isHTMLTableElement(table)) {
        table = undefined;
    }

    const tbody = (table?.querySelector(":scope > tbody") ?? undefined) as HTMLTableSectionElement | undefined;
    const tr = (tbody?.querySelector(":scope > tr") ?? undefined) as HTMLTableRowElement | undefined;
    const td = (tr?.querySelector(":scope > td") ?? undefined) as HTMLTableCellElement | undefined;

    return {
        table,
        tbody,
        tr,
        td
    };
}

/**
 * Finds and returns the component inner wrappers or null if one or more are missing.
 */
export function findComponentInnerWrappers(componentElement: Element): ComponentStructure | null {
    const componentInnerWrappers = findComponentInnerWrappersPartial(componentElement);

    if (componentInnerWrappers.marginWrapper?.borderWrapper?.paddingWrapper) {
        return componentInnerWrappers as ComponentStructure;
    }
    else {
        // One or more wrappers were missing
        // so return null.
        return null;
    }
}

/**
 * Recursively makes all properties in T (including nested ones) optional.
 */
type DeepPartial<T> = {
    [P in keyof T]?: T[P] extends object ? DeepPartial<T[P]> : T[P];
};

/**
 * Finds and returns the component inner wrappers that can be found.
 */
export function findComponentInnerWrappersPartial(componentElement: Element): DeepPartial<ComponentStructure> {
    // Margin should be the outermost wrapper,
    // then border + background,
    // then padding.
    const marginWrapper = findTable(componentElement, "table.margin-wrapper");

    if (!marginWrapper) {
        return {};
    }

    const borderWrapper = findTable(marginWrapper.table, "table.border-wrapper");

    if (!borderWrapper) {
        return {
            marginWrapper: { ...marginWrapper }
        };
    }

    const paddingWrapper = findTable(borderWrapper.table, "table.padding-wrapper");

    if (!paddingWrapper) {
        return {
            marginWrapper: {
                ...marginWrapper,
                borderWrapper: {
                    ...borderWrapper
                }
            }
        };
    }

    return {
        marginWrapper: {
            ...marginWrapper,
            borderWrapper: {
                ...borderWrapper,
                paddingWrapper
            }
        }
    };
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

export function createComponentElement(document: Document, componentTypeName: EditorComponentTypeName): HTMLElement {

    // Notes:
    //  - Inline styles defined here will be at the component level instead of the global level.
    //  - Ensure inline styles can be edited, if desired. Otherwise, styles added here will not be
    //    editable by the individual using the editor.
    //  - Rock CSS variables should not be used here as these will be added to the HTML email.
    //  - Global style defaults are maintained in the emailIFrame.partial.obs file.
    switch (componentTypeName) {
        case "title": {
            const { latestVersion } = getTitleComponentHelper();
            const componentElements = createComponent(
                "title",
                latestVersion,
                `<h1 class="${RockCssClassContentEditable}" style="margin: 0;">Title</h1>`
            );
            // Initialize the component with the correct classes.
            componentElements.marginWrapper.table.classList.add(getMarginWrapperTableCssClass("title", "-h1"));
            componentElements.marginWrapper.borderWrapper.table.classList.add(getBorderWrapperTableCssClass("title", "-h1"));
            componentElements.marginWrapper.borderWrapper.paddingWrapper.table.classList.add(getPaddingWrapperTableCssClass("title", "-h1"));
            return componentElements.marginWrapper.table;
        }

        case "video": {
            const componentElements = createComponent(
                "video",
                "v2-alpha",
                `<a href=""><img src="/Assets/Images/video-placeholder.jpg" data-imgcsswidth="full" style="width: 100%;"></a>`
            );
            // Image component needs a line-height of 0 to remove extra space under image.
            componentElements.marginWrapper.borderWrapper.paddingWrapper.td.style.lineHeight = "0";
            return componentElements.marginWrapper.table;
        }

        case "button": {
            const componentElements = createComponent(
                "button",
                "v2.1-alpha",
                `<a class="button-link ${RockCssClassContentEditable}" href="https://" rel="noopener noreferrer" title="Click Me" style="text-align: center; display: block;">Click Me</a>`
            );

            componentElements.marginWrapper.table.classList.add("button-outerwrap");
            componentElements.marginWrapper.table.style.minWidth = "100%";
            componentElements.marginWrapper.td.classList.add("button-innerwrap");
            componentElements.marginWrapper.td.setAttribute("align", "center");
            componentElements.marginWrapper.td.setAttribute("valign", "top");
            componentElements.marginWrapper.borderWrapper.table.removeAttribute("width");
            componentElements.marginWrapper.borderWrapper.table.classList.add("button-shell");
            componentElements.marginWrapper.borderWrapper.td.classList.add("button-content");
            componentElements.marginWrapper.borderWrapper.td.setAttribute("align", "center");
            componentElements.marginWrapper.borderWrapper.td.setAttribute("valign", "middle");
            return componentElements.marginWrapper.table;
        }

        case "text": {
            const { latestVersion } = getTextComponentHelper();
            const componentElements = createComponent(
                "text",
                latestVersion,
                // Wrap component in a content-wrapper so the wrapper can be styled.
                // It's important that no whitespace is left around the editable area.
                `<div class="content-wrapper content-wrapper-for-text ${RockCssClassContentEditable}"><p style="margin: 0;">Let's see what you have to say!</p></div>`
            );
            return componentElements.marginWrapper.table;
        }

        case "divider": {
            const { latestVersion } = getDividerComponentHelper();
            const componentElements = createComponent(
                "divider",
                latestVersion
            );
            return componentElements.marginWrapper.table;
        }

        case "message": {
            const componentElements = createComponent(
                "message",
                "v2-alpha",
                "Message"
            );
            return componentElements.marginWrapper.table;
        }

        case "image": {
            const componentElements = createComponent(
                "image",
                "v2-alpha",
                // Use box-sizing: border-box to ensure border is included in image width calculations.
                `<img alt="" src="/Assets/Images/image-placeholder.jpg" data-imgcsswidth="full" style="width: 100%; box-sizing: border-box;">`
            );
            // Image component needs a line-height of 0 to remove extra space under image.
            componentElements.marginWrapper.borderWrapper.paddingWrapper.td.style.lineHeight = "0";
            return componentElements.marginWrapper.table;

        }

        case "code": {
            const componentElements = createComponent(
                "code",
                "v2-alpha",
                // Wrap component in a content-wrapper so the wrapper can be styled.
                // It's important that no whitespace is left around the editable area.
                `<div class="content-wrapper content-wrapper-for-code ${RockCssClassContentEditable}">Add your code here...</div>`
            );
            return componentElements.marginWrapper.table;
        }

        case "rsvp": {
            const div = document.createElement("div");
            div.classList.add("component", "component-rsvp");
            div.dataset.state = "component";
            div.innerHTML =
                `<table class="rsvp-outerwrap" border="0" cellpadding="0" cellspacing="0" role="presentation" width="100%" style="min-width: 100%;">
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
            return div;
        }

        // Section Components
        case "section":
        case "one-column-section":
        case "two-column-section":
        case "three-column-section":
        case "four-column-section":
        case "right-sidebar-section":
        case "left-sidebar-section": {
            const { latestVersion } = getSectionComponentHelper();
            const componentElements = createComponent(
                "section",
                latestVersion,
                `<table class="row section-row" cellpadding="0" cellspacing="0" border="0" role="presentation" style="width: 100%;">
                    <tbody>
                        <tr>
                            ${getSectionColumns(componentTypeName)}
                        </tr>
                    </tbody>
                </table>`
            );
            return componentElements.marginWrapper.table;
        }

        case "row": {
            const componentElements = createComponent(
                "row",
                "v2-alpha",
                `<div class="dropzone"></div>`
            );
            return componentElements.marginWrapper.table;
        }
        default:
            throw new Error(`Unknown typeName: ${componentTypeName}`);
    }
}

function getSectionColumns(componentTypeName: EditorComponentTypeName): string {
    const wrappers = createElementWrappers(`<div class="dropzone"></div>`);
    const wrapperHtml = wrappers.marginWrapper.table.outerHTML;
    switch (componentTypeName) {
        case "right-sidebar-section":
            return `<td class="section-column columns small-12 start large-8" valign="top" width="66.666666%">${wrapperHtml}</td>
                    <td class="section-column columns small-12 last large-4" valign="top" width="33.333333%">${wrapperHtml}</td>`;
        case "left-sidebar-section":
            return `<td class="section-column columns small-12 start large-4" valign="top" width="33.333333%">${wrapperHtml}</td>
                    <td class="section-column columns small-12 last large-8" valign="top" width="66.666666%">${wrapperHtml}</td>`;
        case "two-column-section":
            return `<td class="section-column columns small-12 start large-6" valign="top" width="50%">${wrapperHtml}</td>
                    <td class="section-column columns small-12 last large-6" valign="top" width="50%">${wrapperHtml}</td>`;
        case "three-column-section":
            return `<td class="section-column columns small-12 start large-4" valign="top" width="33.333333%">${wrapperHtml}</td>
                    <td class="section-column columns small-12 large-4" valign="top" width="33.333333%">${wrapperHtml}</td>
                    <td class="section-column columns small-12 last large-4" valign="top" width="33.333333%">${wrapperHtml}</td>`;
        case "four-column-section":
            return `<td class="section-column columns small-12 start large-3" valign="top" width="25%">${wrapperHtml}</td>
                    <td class="section-column columns small-12 large-3" valign="top" width="25%">${wrapperHtml}</td>
                    <td class="section-column columns small-12 large-3" valign="top" width="25%">${wrapperHtml}</td>
                    <td class="section-column columns small-12 last large-3" valign="top" width="25%">${wrapperHtml}</td>`;
        case "one-column-section":
        default:
            return `<td class="section-column columns small-12 start last large-12" valign="top" width="100%">${wrapperHtml}</td>`;
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
    { includeSelf, additionalFilter, additionalProjection }: { includeSelf?: string | boolean; additionalFilter?: (el: Element) => boolean; additionalProjection?: (el: Element) => Element; } = {}
): DomWatcher {
    const foundElements = new Set<Element>();
    let onFoundCallbacks: ((element: Element) => void)[] = [];
    let onRemovedCallbacks: ((element: Element) => void)[] = [];

    function updateMatches(): void {
        let matches = Enumerable.from(root.querySelectorAll(selector));
        if (additionalFilter) {
            matches = matches.where(additionalFilter);
        }

        if (additionalProjection) {
            matches = matches.select(additionalProjection);
        }

        const newMatches = new Set(matches);

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

export function checkDropzoneSize(rect: DOMRectReadOnly, element: Element): void {
    if (rect.width < EmptyDropzoneSvgPixelWidth) {
        element.classList.add(SmallEmptyClass);
    }
    else {
        element.classList.remove(SmallEmptyClass);
    }
}

export function getComponentVersionNumber(componentElement: Element): string | null | undefined {
    if (!isHTMLElement(componentElement)) {
        return null;
    }
    else {
        const version = componentElement.dataset.version ?? "";

        if (isComponentVersionNumber(version)) {
            return version;
        }
        else {
            return null;
        }
    }
}

export function setComponentVersionNumber(componentElement: Element, version: string): void {
    if (!isHTMLElement(componentElement)) {
        console.warn("Unable to set version number on non-HTMLElement");
        return;
    }

    if (!isComponentVersionNumber(version)) {
        console.warn(`Invalid version number: ${version}`);
        return;
    }

    componentElement.dataset.version = version;
}

export function isComponentVersionNumber(str: string): boolean {
    const componentVersionRegex = /^v\d+(\.\d+)?(-[a-zA-Z]+(\.\d+)?)?$/;
    return componentVersionRegex.test(str);
}

/**
 * Compares two component version strings using a **semantic-like** versioning system.
 *
 * Version format: `v<major>[.<minor>][-<phase>.<phaseVersion>]`
 * - `v2` (Major-only version, treated as `v2.0`)
 * - `v2.0` (Stable version)
 * - `v2.1` (Minor update)
 * - `v2.0-alpha.1` (Pre-release: alpha phase, version 1)
 * - `v2.0-beta.2` (Pre-release: beta phase, version 2)
 *
 * **Comparison Rules:**
 * 1. **Major version (`vX` vs `vY`)**: Higher major versions are greater.
 * 2. **Minor version (`vX.Y` vs `vX.Z`)**: If major versions are equal, higher minor versions are greater.
 * 3. **Stable vs. Pre-release (`vX.Y` vs `vX.Y-beta.Z`)**: Stable versions are always greater than pre-releases.
 * 4. **Phase Sorting (`alpha < beta < rc`)**: Pre-release phases are sorted lexicographically.
 * 5. **Phase Version (`alpha.1 < alpha.2`)**: If the phase is the same, numerical phase version is compared.
 *
 * **Examples:**
 * ```typescript
 * compareVersions("v2", "v2.1"); // -1 (v2.0 < v2.1)
 * compareVersions("v2.0-alpha.1", "v2.0"); // -1 (alpha < stable)
 * compareVersions("v2.0-beta.2", "v2.0-beta.10"); // -1 (beta.2 < beta.10)
 * compareVersions("v2.0", "v3"); // -1 (v2 < v3)
 * compareVersions("v10", "v2.1"); // 1 (v10 > v2.1)
 * compareVersions("v2.0-beta.2", "v2.0-alpha.1"); // 1 (beta > alpha)
 * compareVersions("v2.0-beta.2", "v2.0-beta.2"); // 0 (equal)
 * ```
 *
 * @param {string} v1 - First version string (e.g., "v2.0-beta.1", "v2")
 * @param {string} v2 - Second version string (e.g., "v2.0", "v2.1", "v3")
 * @returns {number} Returns:
 * - `-1` if `v1 < v2`
 * - `1` if `v1 > v2`
 * - `0` if they are equal
 */
export function compareComponentVersions(v1: string, v2: string): number {
    function parseVersion(version: string): { major: number; minor: number; phase: string; phaseVersion: number } {
        const match = version.match(/^v(\d+)(?:\.(\d+))?(?:-([a-zA-Z]+)(?:\.(\d+))?)?$/);

        return {
            major: match ? parseInt(match[1]) : 0,
            minor: match?.[2] ? parseInt(match[2]) : 0, // Defaults to 0 if missing
            phase: match?.[3] ?? "", // Phase name (e.g., alpha, beta)
            phaseVersion: match?.[4] ? parseInt(match[4]) : 0 // Defaults to 0 if missing
        };
    }

    const v1Parts = parseVersion(v1);
    const v2Parts = parseVersion(v2);

    return (
        v1Parts.major - v2Parts.major ||  // Compare major version
        v1Parts.minor - v2Parts.minor ||  // Compare minor version
        (v1Parts.phase === v2Parts.phase
            ? v1Parts.phaseVersion - v2Parts.phaseVersion // Only compare phase versions when same phase
            : (!v1Parts.phase ? -1 : !v2Parts.phase ? 1 : v1Parts.phase.localeCompare(v2Parts.phase)))
    );
}

// #endregion Functions

// #region Components

export function getTitleComponentHelper(): ComponentMigrationHelper & {
    getElements(componentElement: Element): ComponentStructure & { readonly headingEl: HTMLHeadElement | null; } | null;
} {
    const latestVersion = "v2-alpha" as const;

    return {
        getElements(componentElement: Element): ComponentStructure & { headingEl: HTMLHeadElement | null; } | null {
            if (!componentElement.classList.contains("component-title")) {
                throw new Error(`Element is not a title component element: ${componentElement.outerHTML}`);
            }

            const wrappers = findComponentInnerWrappers(componentElement);

            if (!wrappers) {
                return null;
            }

            return {
                ...wrappers,

                get headingEl(): HTMLHeadingElement | null {
                    return (wrappers.marginWrapper.borderWrapper.paddingWrapper.td.querySelector("h1, h2, h3, h4, h5, h6") ?? null) as HTMLHeadingElement | null;
                }
            };
        },

        isMigrationRequired(componentElement: Element): boolean {
            if (!componentElement.classList.contains("component-title")) {
                throw new Error(`Element is not a title component element: ${componentElement.outerHTML}`);
            }

            // Title is a new component; no need to migrate.
            return false;
        },

        migrate(oldComponentElement: Element): Element {
            if (!oldComponentElement.classList.contains("component-title")) {
                throw new Error(`Element is not a title component element: ${oldComponentElement.outerHTML}`);
            }

            // Title is a new component; no need to migrate.
            return oldComponentElement;
        },

        get latestVersion(): string {
            return latestVersion;
        }
    };
}

export function getTextComponentHelper(): ComponentMigrationHelper & {
    getElements(componentElement: Element): ComponentStructure & { readonly contentWrapper: HTMLElement | null; } | null;
} {
    const latestVersion = "v2-alpha" as const;

    return {
        getElements(componentElement: Element): ComponentStructure & { readonly contentWrapper: HTMLElement | null; } | null {
            if (!componentElement.classList.contains("component-text")) {
                throw new Error(`Element is not a text component element: ${componentElement.outerHTML}`);
            }

            const wrappers = findComponentInnerWrappers(componentElement);

            if (wrappers) {
                return {
                    ...wrappers,

                    get contentWrapper(): HTMLElement | null {
                        return wrappers.marginWrapper.borderWrapper.paddingWrapper.td.querySelector(getContentWrapperSelector("text")) as HTMLElement | null;
                    }
                };
            }
            else {
                return null;
            }
        },

        isMigrationRequired(componentElement: Element): boolean {
            if (!componentElement.classList.contains("component-text")) {
                throw new Error(`Element is not a text component element: ${componentElement.outerHTML}`);
            }

            const versionNumber = getComponentVersionNumber(componentElement);

            return !versionNumber || compareComponentVersions(versionNumber, latestVersion) < 0;
        },

        migrate(oldComponentElement: Element): Element {
            if (!oldComponentElement.classList.contains("component-text") || !isHTMLElement(oldComponentElement)) {
                throw new Error(`Element is not a text component element: ${oldComponentElement.outerHTML}`);
            }

            if (!this.isMigrationRequired(oldComponentElement)) {
                return oldComponentElement;
            }

            const migrations = [
                function v0ToV2Alpha(componentElement: HTMLElement): HTMLElement {
                    if (getComponentVersionNumber(componentElement)) {
                        return componentElement; // Already migrated
                    }

                    // Create the new root table element
                    const newRoot = document.createElement("table");
                    newRoot.setAttribute("border", "0");
                    newRoot.setAttribute("cellpadding", "0");
                    newRoot.setAttribute("cellspacing", "0");
                    newRoot.setAttribute("width", "100%");
                    newRoot.setAttribute("role", "presentation");
                    newRoot.classList.add("margin-wrapper", "component", "component-text");
                    newRoot.setAttribute("data-state", "component");
                    setComponentVersionNumber(newRoot, "v2-alpha"); // Ensure version tracking

                    // Check for `.js-component-text-wrapper`
                    const oldWrapper = componentElement.querySelector(".js-component-text-wrapper") as HTMLElement;

                    // Start building the inner structure
                    let innerStructure = `
                        <tbody><tr><td>
                            <table border="0" cellpadding="0" cellspacing="0" width="100%" role="presentation" class="border-wrapper" style="border-collapse: separate !important;">
                                <tbody><tr><td`;

                    let padding = "";
                    if (oldWrapper) {
                        // Extract styles from `.js-component-text-wrapper`
                        const backgroundColor = componentElement.style.backgroundColor || "";
                        const borderWidth = oldWrapper.style.borderWidth || "";
                        const borderColor = oldWrapper.style.borderColor || "";
                        const borderStyle = oldWrapper.style.borderStyle || "";
                        padding = oldWrapper.style.padding || "";

                        // Apply inline styles for full "v0" migration
                        let inlineStyle = "";
                        if (backgroundColor) inlineStyle += `background-color: ${backgroundColor}; `;
                        if (borderWidth) inlineStyle += `border-width: ${borderWidth}; `;
                        if (borderColor) inlineStyle += `border-color: ${borderColor}; `;
                        if (borderStyle) inlineStyle += `border-style: ${borderStyle}; `;

                        innerStructure += inlineStyle ? ` style="${inlineStyle.trim()}"` : "";
                    }

                    innerStructure += `>
                                    <table border="0" cellpadding="0" cellspacing="0" width="100%" role="presentation" class="padding-wrapper">
                                        <tbody><tr><td`;

                    if (oldWrapper) {
                        // Apply padding style if `.js-component-text-wrapper` existed
                        innerStructure += padding ? ` style="padding: ${padding};"` : "";
                    }

                    innerStructure += `>
                                            <div class="rock-content-editable"></div>
                                        </td></tr></tbody>
                                    </table>
                                </td></tr></tbody>
                            </table>
                        </td></tr></tbody>
                    `;

                    // Set the new structure
                    newRoot.innerHTML = innerStructure;

                    // Move existing content into `.rock-content-editable`
                    const newContentContainer = newRoot.querySelector(".rock-content-editable") as HTMLElement;
                    if (oldWrapper) {
                        // Move children of `.js-component-text-wrapper`
                        oldWrapper.childNodes.forEach(node => newContentContainer.appendChild(node.cloneNode(true)));
                    }
                    else {
                        // Move children of the original `.component-text` (simple case)
                        componentElement.childNodes.forEach(node => newContentContainer.appendChild(node.cloneNode(true)));
                    }

                    return newRoot;
                }
            ];

            // Run migrations.
            return migrations.reduce((component, migrate) => migrate(component), oldComponentElement);
        },

        get latestVersion(): string {
            return latestVersion;
        }
    };
}

export function getButtonComponentHelper(): {
    getElements(componentElement: Element): ComponentStructure & { readonly linkButton: HTMLAnchorElement | null; } | null;
} {
    return {
        getElements(componentElement: Element): ComponentStructure & { readonly linkButton: HTMLAnchorElement | null; } | null {
            if (!componentElement.classList.contains("component-button")) {
                throw new Error(`Element is not a button component element: ${componentElement.outerHTML}`);
            }

            const wrappers = findComponentInnerWrappers(componentElement);

            if (!wrappers) {
                return null;
            }

            return {
                ...wrappers,

                get linkButton(): HTMLAnchorElement | null {
                    const searchFrom = wrappers.marginWrapper.borderWrapper.paddingWrapper.td;

                    return (searchFrom.querySelector("a.button-link") ?? null) as HTMLAnchorElement | null;
                }
            };
        }
    };
}

export function getCodeComponentHelper(): {
    getElements(componentElement: Element): ComponentStructure & { readonly contentWrapper: HTMLElement | null; } | null
} {
    return {
        getElements(componentElement: Element): ComponentStructure & { readonly contentWrapper: HTMLElement | null; } | null {
            if (!componentElement.classList.contains("component-code")) {
                throw new Error(`Element is not a code component element: ${componentElement.outerHTML}`);
            }

            const wrappers = findComponentInnerWrappers(componentElement);

            if (wrappers) {
                return {
                    ...wrappers,

                    get contentWrapper(): HTMLElement | null {
                        const searchFromElement = wrappers.marginWrapper.borderWrapper.paddingWrapper.td;
                        return (searchFromElement.querySelector(getContentWrapperSelector("code")) ?? null) as HTMLElement | null;
                    }
                };
            }

            return wrappers;
        }
    };
}

export function getDividerComponentHelper(): ComponentMigrationHelper & {
    getElements(componentElement: Element): ComponentStructure | null;
} {
    const latestVersion = "v2-alpha" as const;

    return {
        getElements(componentElement: Element): ComponentStructure | null {
            if (!componentElement.classList.contains("component-divider")) {
                throw new Error(`Element is not a divider component element: ${componentElement.outerHTML}`);
            }

            return findComponentInnerWrappers(componentElement);
        },

        isMigrationRequired(componentElement: Element): boolean {
            if (!componentElement.classList.contains("component-divider")) {
                throw new Error(`Element is not a divider component element: ${componentElement.outerHTML}`);
            }

            const versionNumber = getComponentVersionNumber(componentElement);
            return !versionNumber || compareComponentVersions(versionNumber, latestVersion) < 0;
        },

        migrate(componentElement: Element): Element {
            if (!componentElement.classList.contains("component-divider")) {
                throw new Error(`Element is not a divider component element: ${componentElement.outerHTML}`);
            }

            if (!this.isMigrationRequired(componentElement)) {
                // The component is already at the latest version.
                return componentElement;
            }

            const migrations = [
                function v0ToV2Alpha(componentElement: Element): Element {
                    if (getComponentVersionNumber(componentElement)) {
                        // Already migrated since v0 didn't have a version number.
                        return componentElement;
                    }

                    // Create the new root table element
                    const newRoot = document.createElement("table");
                    newRoot.setAttribute("border", "0");
                    newRoot.setAttribute("cellpadding", "0");
                    newRoot.setAttribute("cellspacing", "0");
                    newRoot.setAttribute("width", "100%");
                    newRoot.setAttribute("role", "presentation");
                    newRoot.classList.add("margin-wrapper", "component", "component-divider");
                    newRoot.setAttribute("data-state", "component");
                    setComponentVersionNumber(newRoot, "v2-alpha"); // Ensure version tracking

                    // Extract the existing divider element (`<hr>` or `<div>`)
                    const oldDivider = componentElement.querySelector("hr, div") as HTMLElement;
                    if (!oldDivider) {
                        throw new Error("No divider found in Divider component.");
                    }

                    // Determine if this was an `<hr>` or `<div>`
                    const isHr = oldDivider.tagName.toLowerCase() === "hr";

                    // Extract styles from the old divider
                    let height = oldDivider.style.height;
                    if (isHr && !height) {
                        height = "1px"; // Default to 1px if no height is specified
                    }
                    const backgroundColor = oldDivider.style.backgroundColor || "transparent"; // If missing, keep transparent
                    const marginTop = oldDivider.style.marginTop || "";
                    const marginBottom = oldDivider.style.marginBottom || "";

                    // Define border properties based on element type
                    const borderWidth = `${height} 0px 0px`; // Top border uses height, others are 0px
                    const borderColor = isHr ? `${backgroundColor} transparent transparent` : "transparent";
                    const borderStyle = isHr ? "solid none none" : "solid"; // `solid` is optional for div

                    // Create the new inner structure
                    newRoot.innerHTML = `
                        <tbody><tr><td ${marginTop || marginBottom ? `style="${marginTop ? `padding-top: ${marginTop};` : ""} ${marginBottom ? `padding-bottom: ${marginBottom};` : ""}"` : ""}>
                            <table border="0" cellpadding="0" cellspacing="0" width="100%" role="presentation" class="border-wrapper" style="border-collapse: separate !important;">
                                <tbody><tr><td>
                                    <table border="0" cellpadding="0" cellspacing="0" width="100%" role="presentation" class="padding-wrapper">
                                        <tbody><tr><td style="border-width: ${borderWidth};
                                                             border-color: ${borderColor};
                                                             ${isHr ? `border-style: ${borderStyle};` : ""}">
                                        </td></tr></tbody>
                                    </table>
                                </td></tr></tbody>
                            </table>
                        </td></tr></tbody>
                    `;

                    return newRoot;
                }
            ];

            return migrations.reduce((component, migrate) => migrate(component), componentElement);
        },

        get latestVersion(): string {
            return latestVersion;
        }
    };
}

export function getRowComponentHelper(): { getElements(componentElement: Element): ComponentStructure | null } {
    return {
        getElements(componentElement: Element): ComponentStructure | null {
            if (!componentElement.classList.contains("component-row")) {
                throw new Error(`Element is not a row component element: ${componentElement.outerHTML}`);
            }

            const wrappers = findComponentInnerWrappers(componentElement);

            if (!wrappers) {
                return null;
            }

            return wrappers;
        }
    };
}

type SectionComponentVersion = "v0" | "v2-alpha";
export function getSectionComponentHelper(): ComponentMigrationHelper & {
    getElements(componentElement: Element): ComponentStructure & { readonly rowWrapper: HTMLTableElement | null; } | null;
} {
    const latestVersion: SectionComponentVersion = "v2-alpha" as const;

    function getVersion(version: SectionComponentVersion): SectionComponentVersion {
        return version;
    }

    return {
        getElements(componentElement: Element): ComponentStructure & { readonly rowWrapper: HTMLTableElement | null; } | null {
            if (!componentElement.classList.contains("component-section")) {
                throw new Error(`Element is not a section component element: ${componentElement.outerHTML}`);
            }

            const wrappers = findComponentInnerWrappers(componentElement);

            if (!wrappers) {
                return null;
            }

            return {
                ...wrappers,

                get rowWrapper(): HTMLTableElement | null {
                    return wrappers.marginWrapper.borderWrapper.paddingWrapper.td.querySelector("table.section-row") ?? null;
                }
            };
        },

        isMigrationRequired(componentElement: Element): boolean {
            if (!componentElement.classList.contains("component-section")) {
                throw new Error(`Element is not a section component element: ${componentElement.outerHTML}`);
            }

            const versionNumber = getComponentVersionNumber(componentElement);

            if (!versionNumber) {
                return true;
            }

            const comparison = compareComponentVersions(versionNumber, latestVersion);

            if (comparison < 0) {
                return true;
            }

            return false;
        },

        migrate(oldComponentElement: Element): Element {
            const componentVersion = getComponentVersionNumber(oldComponentElement);

            // Check if the component is already at the latest version.
            if (componentVersion && compareComponentVersions(componentVersion, latestVersion) === 0) {
                return oldComponentElement;
            }

            // These are in order from oldest to newest; new migrations should be added at the end.
            const migrations = [
                function v0ToV2Alpha(componentElement: Element): Element {
                    if (getComponentVersionNumber(componentElement) || !isHTMLElement(componentElement)) {
                        // The old component element didn't have a version number
                        // so if there is any version number at all, it's already migrated.
                        return componentElement;
                    }

                    // Create the new root table element.
                    const newRoot = document.createElement("table");
                    newRoot.setAttribute("border", "0");
                    newRoot.setAttribute("cellpadding", "0");
                    newRoot.setAttribute("cellspacing", "0");
                    newRoot.setAttribute("width", "100%");
                    newRoot.setAttribute("role", "presentation");
                    newRoot.classList.add("margin-wrapper", "component", "component-section");
                    newRoot.setAttribute("data-state", "component");

                    // Ensure version tracking.
                    setComponentVersionNumber(newRoot, getVersion("v2-alpha"));

                    // Extract inline styles from the old component.
                    const backgroundColor = componentElement.style.backgroundColor || "";
                    const padding = componentElement.style.padding || "";
                    const borderRadius = componentElement.style.borderRadius || "";
                    const borderStyle = componentElement.style.borderStyle || "";
                    const borderWidth = componentElement.style.borderWidth || "";
                    const borderColor = componentElement.style.borderColor || "";

                    // Create the inner structure.
                    newRoot.innerHTML = `
                        <tbody>
                            <tr><td>
                                <table border="0" cellpadding="0" cellspacing="0" width="100%" role="presentation" class="border-wrapper" style="border-collapse: separate !important;">
                                    <tbody><tr><td style="${backgroundColor ? `background-color: ${backgroundColor};` : ""}
                                                    ${borderRadius ? `border-radius: ${borderRadius};` : ""}
                                                    ${borderStyle ? `border-style: ${borderStyle};` : ""}
                                                    ${borderWidth ? `border-width: ${borderWidth};` : ""}
                                                    ${borderColor ? `border-color: ${borderColor};` : ""}">
                                        <table border="0" cellpadding="0" cellspacing="0" width="100%" role="presentation" class="padding-wrapper">
                                            <tbody><tr><td ${padding ? `style="padding: ${padding};"` : ""}>
                                                <table class="row section-row" cellpadding="0" cellspacing="0" border="0" role="presentation" style="width: 100%;">
                                                    <tbody><tr></tr></tbody>
                                                </table>
                                            </td></tr></tbody>
                                        </table>
                                    </td></tr></tbody>
                                </table>
                            </td></tr>
                        </tbody>
                    `;

                    // Find old columns (dropzones and spacers)
                    const oldColumns = Array.from(componentElement.querySelectorAll(".dropzone, .spacer"));
                    const nonSpacerColumns = oldColumns.filter(col => !col.classList.contains("spacer")).length;
                    const newColumnsRow = newRoot.querySelector(".section-row tbody tr") as HTMLElement;

                    // Calculate "large-[n]" values ensuring the sum is 12
                    const calculatedLarge = Math.floor(12 / nonSpacerColumns);
                    const remainder = 12 - (calculatedLarge * nonSpacerColumns);
                    const largeValues = new Array(nonSpacerColumns).fill(calculatedLarge);
                    for (let i = 0; i < remainder; i++) {
                        largeValues[i]++;
                    }

                    // Process each column
                    oldColumns.forEach((oldColumn, index) => {
                        const oldColumnElement = oldColumn as HTMLElement;

                        if (oldColumnElement.classList.contains("spacer")) {
                            // Copy spacers exactly
                            const spacer = document.createElement("td");
                            spacer.className = "spacer";
                            spacer.setAttribute("width", oldColumnElement.getAttribute("width") || "8px");
                            spacer.setAttribute("style", oldColumnElement.getAttribute("style") || "width: 8px; min-width: 8px; font-size: 0px; line-height: 0; padding: 0px;");
                            spacer.innerHTML = "&nbsp;";

                            // Append the spacer to the new row
                            newColumnsRow.appendChild(spacer);
                        }
                        else {
                            const width = oldColumnElement.getAttribute("width") || `${(100 / nonSpacerColumns).toFixed(3)}%`;
                            const existingLargeClass = Array.from(oldColumnElement.classList).find(cls => cls.startsWith("large-"));
                            const largeClass = existingLargeClass || `large-${largeValues[index]}`;

                            // Extract styles
                            const dropzoneBackgroundColor = oldColumnElement.style.backgroundColor || "";
                            const dropzoneBorderRadius = oldColumnElement.style.borderRadius || "";
                            const dropzoneBorderStyle = oldColumnElement.style.borderStyle || "";
                            const dropzoneBorderWidth = oldColumnElement.style.borderWidth || "";
                            const dropzoneBorderColor = oldColumnElement.style.borderColor || "";
                            const dropzonePadding = oldColumnElement.style.padding || "";

                            // Create new column
                            const newColumn = document.createElement("td");
                            newColumn.className = `dropzone columns small-12 section-column ${largeClass}`;
                            newColumn.setAttribute("valign", "top");
                            newColumn.setAttribute("width", width);

                            // Construct column content
                            newColumn.innerHTML = `
    <table border="0" cellpadding="0" cellspacing="0" width="100%" role="presentation" class="margin-wrapper" align="center">
        <tbody><tr><td>
            <table border="0" cellpadding="0" cellspacing="0" width="100%" role="presentation" class="border-wrapper" style="border-collapse: separate !important;">
                <tbody><tr><td style="${dropzoneBackgroundColor ? `background-color: ${dropzoneBackgroundColor};` : ""}
                                ${dropzoneBorderRadius ? `border-radius: ${dropzoneBorderRadius};` : ""}
                                ${dropzoneBorderStyle ? `border-style: ${dropzoneBorderStyle};` : ""}
                                ${dropzoneBorderWidth ? `border-width: ${dropzoneBorderWidth};` : ""}
                                ${dropzoneBorderColor ? `border-color: ${dropzoneBorderColor};` : ""}">
                    <table border="0" cellpadding="0" cellspacing="0" width="100%" role="presentation" class="padding-wrapper">
                        <tbody><tr><td ${dropzonePadding ? `style="padding: ${dropzonePadding};"` : ""}>
                            <div class="dropzone"></div>
                        </td></tr></tbody>
                    </table>
                </td></tr></tbody>
            </table>
        </td></tr></tbody>
    </table>
`;

                            // Create a new dropzone and move existing content into it
                            const newDropzone = newColumn.querySelector(".dropzone") as HTMLElement;
                            oldColumn.childNodes.forEach(node => newDropzone.appendChild(node.cloneNode(true)));

                            // Append the new column to the row
                            newColumnsRow.appendChild(newColumn);
                        }
                    });

                    return newRoot;
                }
            ];

            // Run migrations.
            return migrations.reduce((component, migrate) => migrate(component), oldComponentElement);
        },

        get latestVersion(): string {
            return latestVersion;
        }
    };
}

export function getImageComponentHelper(componentElement: Element): ComponentStructure | null {
    if (!componentElement.classList.contains("component-image")) {
        throw new Error(`Element is not a image component element: ${componentElement.outerHTML}`);
    }

    return findComponentInnerWrappers(componentElement);
}

export function getVideoComponentHelper(componentElement: Element): ComponentStructure | null {
    if (!componentElement.classList.contains("component-video")) {
        throw new Error(`Element is not a video component element: ${componentElement.outerHTML}`);
    }

    return findComponentInnerWrappers(componentElement);
}

// #endregion Components
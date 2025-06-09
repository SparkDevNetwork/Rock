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

import { Enumerable } from "./linq";

/**
 * Get a unique CSS selector for any DOM element.
 */
export function getUniqueCssSelector(el: Element): string {
    const path: string[] = [];
    let parent: Element | null = el.parentNode as Element;

    while (parent) {
        path.unshift(`${el.tagName}:nth-child(${([] as Element[]).indexOf.call(parent.children, el) + 1})`);
        el = parent;
        parent = el.parentNode as Element;
    }
    return `${path.join(" > ")}`.toLowerCase();
}

export function* getAncestors(element: Element): IterableIterator<Element> {
    let parent = element.parentElement;

    while (parent) {
        yield parent;
        parent = parent.parentElement;
    }
}

/**
 * Scrolls the start of an element to the top of the window.
 *
 * This accounts for fixed elements.
 */
export function scrollElementStartToTop(element: Element): void {
    const isWithinScrolledContainer = Enumerable
        .from(getAncestors(element))
        .any(ancestorElement => {
            const { overflowY } = getComputedStyle(ancestorElement);
            const isScrollable = overflowY === "scroll" || overflowY === "auto";

            return isScrollable && ancestorElement.scrollHeight > ancestorElement.clientHeight;
        });

    if (isWithinScrolledContainer) {
        element.scrollIntoView({
            block: "start",
            behavior: "smooth"
        });
    }
    else {
        // Get the element's current position and size.
        const rect = element.getBoundingClientRect();

        const desiredLeft = rect.left < 0 ? 0 : rect.left;
        let desiredTop = 0;

        // Check if a fixed element is at the desired point.
        // If so, account for its size when calculating the scroll offset.
        const elementsAtDesiredLocation = document.elementsFromPoint(desiredLeft + 1, desiredTop + 1);
        const fixedElementAtDesiredLocation = elementsAtDesiredLocation.find(el => getComputedStyle(el).position === "fixed");

        if (fixedElementAtDesiredLocation) {
            // Adjust scroll to account for fixed element height.
            const { height: fixedHeight } = fixedElementAtDesiredLocation.getBoundingClientRect();
            desiredTop = fixedHeight;
        }

        window.scrollBy({
            top: rect.top - desiredTop,
            behavior: "smooth"
        });
    }
}

/**
 * Finds the direct child of `element` containing the `descendantElement`.
 * @param element The element to be searched.
 * @param descendantElement The descendant element to search for.
 */
export function findDirectChildContaining(element: HTMLElement, descendantElement: HTMLElement): HTMLElement | null {
    let current: Element | null = descendantElement;

    while (current && current !== element) {
        if (current.parentElement === element && isHTMLElement(current)) {
            // Found the direct child of element that contains descendantElement.
            return current;
        }

        // Move up the DOM tree.
        current = current.parentElement;
    }

    // No direct child was found.
    return null;
}

/**
 * Finds the nearest element to a specific coordinate in the y-direction.
 * @param iframe
 * @param x The X position (in pixels) to search.
 * @param y The Y position (in pixels) to search.
 * @param selector The CSS selector used to find the matching element.
 * @param xRange The horizontal distance (in pixels) left and right of the `xPosition` to search for an element.
 * @param yRange The vertical distance (in pixels) above and below the `yPosition` to search for an element.
 * @returns
 */
export function findNearestIFrameElementFromPoint(
    iframe: HTMLIFrameElement,
    x: number,
    y: number,
    selector: string,
    xRange: number = 10,
    yRange: number = 10
): { element: HTMLElement | null, isAbove: boolean, isBelow: boolean, isLeft: boolean, isRight: boolean } {
    const iframeDoc = iframe.contentDocument || iframe.contentWindow?.document;
    if (!iframeDoc) {
        console.error("Unable to access iframe content.");
        return { element: null, isAbove: false, isBelow: false, isLeft: false, isRight: false };
    }

    const iframeRect = iframe.getBoundingClientRect();

    let closestElement: Element | null = null;
    let isAbove: boolean = false;
    let isBelow: boolean = false;
    let isLeft: boolean = false;
    let isRight: boolean = false;
    let smallestDistance = Infinity;

    // Search within the specified xRange and yRange.
    for (let dx = -xRange; dx <= xRange; dx += 1) {
        for (let dy = -yRange; dy <= yRange; dy += 1) {
            Enumerable.from(iframeDoc.elementsFromPoint(x + dx, y + dy))
                .where(element => element.matches(selector))
                .ofType(isHTMLElement)
                .forEach(element => {
                    const rect = element.getBoundingClientRect();

                    // Calculate the distance from the current position to the element.
                    const elementX = rect.left - iframeRect.left;
                    const elementY = rect.top - iframeRect.top;
                    const distance = Math.sqrt(
                        Math.pow(elementX - x, 2) + Math.pow(elementY - y, 2)
                    );

                    // Update the closest element if this one is nearer.
                    if (distance < smallestDistance) {
                        smallestDistance = distance;

                        closestElement = element;
                        isAbove = dy < 0;
                        isBelow = dy > 0;
                        isLeft = dx < 0;
                        isRight = dx > 0;
                    }
                });
        }
    }

    return {
        element: closestElement,
        isAbove,
        isBelow,
        isLeft,
        isRight
    };
}

/**
 * Gets the window associated with an element.
 */
export function getWindow(el: Element): (Window & typeof globalThis) | null {
    return el.ownerDocument.defaultView;
}

/**
 * Determines whether the argument is an Element.
 */
export function isElement(el: unknown): el is Element {
    // This handles context mismatch when checking iframe elements.
    const elWindow = el?.["ownerDocument"]?.["defaultView"] as (Window & typeof globalThis);

    return el instanceof Element || (!!elWindow && (el instanceof elWindow.Element));
}

/**
 * Determines whether the argument is an HTMLElement.
 */
export function isHTMLElement(el: unknown): el is HTMLElement {
    // This handles context mismatch when checking iframe elements.
    const elWindow = el?.["ownerDocument"]?.["defaultView"] as (Window & typeof globalThis);

    // Check if the element is an instance of HTMLElement in the current window context.
    return el instanceof HTMLElement

        // Check if the element is an instance of HTMLElement in the window context of the element (applies to iframe elements).
        || (!!elWindow && (el instanceof elWindow.HTMLElement))

        // Check if the element has HTMLElement properties (applies to disconnected elements without an ancestor window).
        || (
            typeof el === "object"
            && el !== null
            && typeof (el as HTMLElement).tagName === "string"
            && typeof (el as HTMLElement).classList === "object"
        );
}

/**
 * Determines whether the argument is an HTMLTableElement.
 */
export function isHTMLTableElement(el: unknown): el is HTMLTableElement {
    // This handles context mismatch when checking iframe elements.
    const elWindow = el?.["ownerDocument"]?.["defaultView"] as (Window & typeof globalThis);

    return el instanceof HTMLTableElement || (!!elWindow && (el instanceof elWindow.HTMLTableElement));
}

/**
 * Determines whether the argument is an HTMLAnchorElement.
 */
export function isHTMLAnchorElement(el: unknown): el is HTMLAnchorElement {
    // This handles context mismatch when checking iframe elements.
    const elWindow = el?.["ownerDocument"]?.["defaultView"] as (Window & typeof globalThis);

    return !!elWindow && el instanceof elWindow.HTMLAnchorElement;
}

/**
 * Determines whether the argument is a Document.
 */
export function isDocument(doc: unknown): doc is Document {
    // This handles context mismatch when checking iframe elements.
    const docWindow = doc?.["defaultView"] as (Window & typeof globalThis);

    return !!docWindow && doc instanceof docWindow.Document;
}

/**
 * Determines whether the argument is a MouseEvent.
 * @param event
 * @returns
 */
export function isMouseEvent(event: unknown): event is MouseEvent {
    // This handles context mismatch when checking iframe elements.
    const eventWindow = event?.["view"] as (Window & typeof globalThis);

    return !!eventWindow && event instanceof eventWindow.MouseEvent;
}

/**
 * Determines whether the argument is a TouchEvent.
 * @param event
 * @returns
 */
export function isTouchEvent(event: unknown): event is TouchEvent {
    // This handles context mismatch when checking iframe elements.
    const eventWindow = event?.["view"] as (Window & typeof globalThis);

    // TouchEvent may not be supported in some browsers.
    return !!eventWindow?.TouchEvent && event instanceof eventWindow.TouchEvent;
}

/**
 * Removes white space content from an element.
 *
 * This is important for applying :empty styles.
 */
export function removeWhiteSpaceFromElement(element: Element, selector?: string): void {
    if ((!selector || element.matches(selector))
        && element.childNodes.length
        && Enumerable.from(element.childNodes).all(n => n.nodeType === Node.TEXT_NODE)
        && !element.textContent?.trim()
    ) {
        element.innerHTML = "";
    }
}

/**
 * Removes white space content from an element and its children.
 *
 * This is important for applying :empty styles.
 */
export function removeWhiteSpaceFromElementAndChildElements(element: Element, selector?: string): void {
    removeWhiteSpaceFromChildElements(element, selector);
    removeWhiteSpaceFromElement(element, selector);
}

/**
 * Removes white space content from an element's children.
 *
 * This is important for applying :empty styles.
 */
export function removeWhiteSpaceFromChildElements(element: Document | Element, selector?: string): void {
    // Clear white space from elements matching the selector so :empty styles gets applied.
    if (selector) {
        element.querySelectorAll(selector)
            .forEach(el => {
                removeWhiteSpaceFromElement(el, selector);
            });
    }
    else {
        Enumerable.from(element.children)
            .forEach(el => {
                removeWhiteSpaceFromElement(el, selector);
            });
    }
}
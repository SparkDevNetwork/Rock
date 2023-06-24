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

import { Directive, DirectiveBinding } from "vue";

/**
 * The last known position values for a drag scroll instance host element.
 */
interface IDragScrollPosition {
    left: number,
    top: number,
    x: number,
    y: number
}

/**
 * The scroll behaviors to be preserved, disabled and reinstated for a drag scroll instance host element.
 */
interface IPreserveScrollBehavior {
    scrollBehavior: string,
    scrollSnapType: string
}

const styleProp = {
    cursor: "cursor",
    scrollBehavior: "scroll-behavior",
    scrollSnapType: "scroll-snap-type",
    userSelect: "user-select"
};

const modifiers = {
    left: "left",
    middle: "middle",
    right: "right"
};

const dataDragScrollId = "dragScrollId";
const noDragScrollClass = "js-no-drag-scroll";

let hostElement: HTMLElement;
let startingPosition: IDragScrollPosition;
let preserveScrollBehavior: IPreserveScrollBehavior;

let scrollableElementX: HTMLElement | null;
let scrollableElementY: HTMLElement | null;

// #region Directive Instance Helpers

/**
 * The options that can be used when defining a drag scroll instance.
 */
export interface IDragScrollOptions {
    /** The unique identifier for this drag scroll instance. Recommended to be a Guid. */
    id: string
}

/**
 * Service to handle a specific drag scroll instance; each instance may have different modifiers, Etc.
 */
class DragScrollService {
    /** The options for this drag scroll instance. */
    private readonly options: IDragScrollOptions;

    /** The modifiers - if any - for this drag scroll instance. */
    private readonly modifiers: Record<string, boolean>;

    constructor(options: IDragScrollOptions, modifiers: Record<string, boolean>) {
        this.options = options;
        this.modifiers = modifiers;
    }

    /**
     * Determines whether the button that raised the `MouseEvent` is allowed,
     * according to any modifiers assigned to this drag scroll instance.
     *
     * @param ev An object describing the event.
     * @returns Whether the button that raised the `MouseEvent` is allowed
     */
    public isMouseButtonAllowed(ev: MouseEvent): boolean {
        if (!Object.keys(this.modifiers).length) {
            return true;
        }

        // https://developer.mozilla.org/en-US/docs/Web/API/MouseEvent/button#value
        switch (ev.button) {
            case 0:
                return this.modifiers[modifiers.left];
            case 1:
                return this.modifiers[modifiers.middle];
            case 2:
                return this.modifiers[modifiers.right];
            default:
                return false;
        }
    }
}

/**
 * The drag scroll services that are currently in use on the page.
 */
const knownServices: Record<string, DragScrollService> = {};

/**
 * Creates a service to represent this drag scroll instance and adds it to the collection of known services.
 *
 * @param options The options for this drag scroll instance.
 * @param modifiers The modifiers for this drag scroll instance.
 */
function createService(options: IDragScrollOptions, modifiers: Record<string, boolean>): void {
    knownServices[options.id] = new DragScrollService(options, modifiers);
}

/**
 * Gets the service matching the specified identifier.
 *
 * @param identifier The unique identifier of the service to get.
 * @returns The service matching the specified identifier.
 */
function getService(identifier: string): DragScrollService | undefined {
    return knownServices[identifier];
}

/**
 * Removes the specified service from the collection of known services.
 *
 * @param identifier The unique identifier of the service to destroy.
 */
function destroyService(identifier: string): void {
    delete knownServices[identifier];
}

// #endregion

// #region Functions

/**
 * Determines whether drag scroll should be activated based on this drag scroll instance's
 * modifiers and the target element that raised the `MouseEvent`.
 *
 * @param ev An object describing the event triggering drag scroll behavior.
 * @returns Whether drag scroll should be activated.
 */
function shouldScroll(ev: MouseEvent): boolean {
    const id = hostElement.dataset[dataDragScrollId];
    if (!id) {
        return false;
    }

    const service = getService(id);
    if (!service) {
        return false;
    }

    if (!service.isMouseButtonAllowed(ev)) {
        return false;
    }

    const noScrollElement = (ev.target as HTMLElement)?.closest(`.${noDragScrollClass}`);
    return !noScrollElement;
}

/**
 * Sets "grabbing" styles on the host element and takes note of scroll behaviors that should be
 * preserved, disabled and later reinstated on the element.
 *
 * Note that we need to disable certain scroll and user-selection behaviors while actively
 * drag-scrolling, for a more pleasant individual experience.
 */
function setGrabbingStyles(): void {

    /*
        8/2/2023 - JPH

        There appears to be a browser bug (in Chrome, at least) where the cursor doesn't actually
        change until the mouse is moved. BUT good news: this only happens when dev tools are open.
        If you come across this behavior and happen to have dev tools open, try closing them to
        see if that resolves it before spending time chasing this issue.

        Reason: mouse cursor change is delayed / only changes after mouse is moved, causing the
        "grab" and "grabbing" styles to be backwards when using the drag scroll directive.
    */

    hostElement.style.setProperty(styleProp.cursor, "grabbing");
    hostElement.style.setProperty(styleProp.userSelect, "none");

    const style = window.getComputedStyle(hostElement);
    preserveScrollBehavior = {
        scrollBehavior: style.getPropertyValue(styleProp.scrollBehavior),
        scrollSnapType: style.getPropertyValue(styleProp.scrollSnapType)
    };

    if (preserveScrollBehavior.scrollBehavior) {
        hostElement.style.setProperty(styleProp.scrollBehavior, "auto");
    }

    if (preserveScrollBehavior.scrollSnapType) {
        hostElement.style.setProperty(styleProp.scrollSnapType, "none");
    }
}

/**
 * Determines whether the provided element is scrollable along the X axis.
 *
 * @param el The element to inspect.
 * @returns Whether the provided element is scrollable along the X axis.
 */
function isScrollableX(el: HTMLElement): boolean {
    const hasScrollableContent = el.scrollWidth > el.clientWidth;
    const overflowStyle = window.getComputedStyle(el).overflowX;
    const isOverflowHidden = overflowStyle.startsWith("hidden");

    return hasScrollableContent && !isOverflowHidden;
}

/**
 * Recursively searches ancestor elements to find the first one that is scrollable along the X axis.
 *
 * @param el The element whose anscestors should be searched.
 * @returns The first ancestor element that is scrollable along the X axis, if any.
 */
function getFirstScrollableAncestorX(el: HTMLElement | null): HTMLElement | null {
    if (!el || isScrollableX(el)) {
        return el;
    }

    return getFirstScrollableAncestorX(el.parentElement);
}

/**
 * Determines whether the provided element is scrollable along the Y axis.
 *
 * @param el The element to inspect.
 * @returns Whether the provided element is scrollable along the Y axis.
 */
function isScrollableY(el: HTMLElement): boolean {
    const hasScrollableContent = el.scrollHeight > el.clientHeight;
    const overflowStyle = window.getComputedStyle(el).overflowY;
    const isOverflowHidden = overflowStyle.startsWith("hidden");

    return hasScrollableContent && !isOverflowHidden;
}

/**
 * Recursively searches ancestor elements to find the first one that is scrollable along the Y axis.
 *
 * @param el The element whose anscestors should be searched.
 * @returns The first ancestor element that is scrollable along the Y axis.
 */
function getFirstScrollableAncestorY(el: HTMLElement | null): HTMLElement | null {
    if (!el || isScrollableY(el)) {
        return el;
    }

    return getFirstScrollableAncestorY(el.parentElement);
}

/**
 * Removes "grabbing" styles and reinstates any scroll behaviors that were previously disabled
 * on the host element.
 */
function removeGrabbingStyles(): void {

    /*
        8/2/2023 - JPH

        There appears to be a browser bug (in Chrome, at least) where the cursor doesn't actually
        change until the mouse is moved. BUT good news: this only happens when dev tools are open.
        If you come across this behavior and happen to have dev tools open, try closing them to
        see if that resolves it before spending time chasing this issue.

        Reason: mouse cursor change is delayed / only changes after mouse is moved, causing the
        "grab" and "grabbing" styles to be backwards when using the drag scroll directive.
    */

    hostElement.style.setProperty(styleProp.cursor, "grab");
    hostElement.style.removeProperty(styleProp.userSelect);

    if (preserveScrollBehavior.scrollBehavior) {
        hostElement.style.setProperty(styleProp.scrollBehavior, preserveScrollBehavior.scrollBehavior);
    }

    if (preserveScrollBehavior.scrollSnapType) {
        hostElement.style.setProperty(styleProp.scrollSnapType, preserveScrollBehavior.scrollSnapType);
    }
}

// #endregion

// #region Event Handlers

/**
 * Handles the mouse down event.
 *
 * @param this The element that raised the event.
 * @param event An object describing the event.
 */
function onMouseDown(this: HTMLElement, event: MouseEvent): void {
    hostElement = this;

    if (!shouldScroll(event)) {
        return;
    }

    // https://developer.mozilla.org/en-US/docs/Web/API/MouseEvent/button#value
    if (event.button === 1) {
        event.preventDefault();
    }
    else if (event.button === 2) {
        hostElement.addEventListener("contextmenu", preventContextMenu);
    }

    setGrabbingStyles();

    scrollableElementX = null;
    scrollableElementY = null;

    startingPosition = {
        left: hostElement.scrollLeft,
        top: hostElement.scrollTop,
        x: event.clientX,
        y: event.clientY
    };

    document.addEventListener("mousemove", onMouseMove);
    document.addEventListener("mouseup", onMouseUp);
}

/**
 * Prevents the context menu from being displayed.
 *
 * @param this The element that raised the event.
 * @param event An object describing the event.
 */
function preventContextMenu(this: HTMLElement, event: MouseEvent): void {
    event.preventDefault();
}

/**
 * Handles the mouse move event.
 *
 * @param this The document.
 * @param event An object describing the event.
 */
function onMouseMove(this: Document, event: MouseEvent): void {
    if (!scrollableElementX) {
        if (isScrollableX(hostElement)) {
            scrollableElementX = hostElement;
        }
        else {
            scrollableElementX = getFirstScrollableAncestorX(hostElement.parentElement);
            startingPosition.left = scrollableElementX?.scrollLeft ?? 0;
        }
    }

    if (scrollableElementX) {
        const deltaX = event.clientX - startingPosition.x;
        const newX = startingPosition.left - deltaX;
        scrollableElementX.scrollLeft = newX;
    }

    if (!scrollableElementY) {
        if (isScrollableY(hostElement)) {
            scrollableElementY = hostElement;
        }
        else {
            scrollableElementY = getFirstScrollableAncestorY(hostElement.parentElement);
            startingPosition.top = scrollableElementY?.scrollTop ?? 0;
        }
    }

    if (scrollableElementY) {
        const deltaY = event.clientY - startingPosition.y;
        const newY = startingPosition.top - deltaY;
        scrollableElementY.scrollTop = newY;
    }
}

/**
 * Handles the mouse up event.
 *
 * @param this The document.
 * @param ev An object describing the event.
 */
function onMouseUp(this: Document, _event: MouseEvent): void {
    removeGrabbingStyles();

    document.removeEventListener("mousemove", onMouseMove);
    document.removeEventListener("mouseup", onMouseUp);
}

// #endregion

/**
 * A directive to enable "drag-scroll" behavior for the host element.
 *
 * If the host element itself is not scrollable in the X or Y direction, its ancestors will be searched
 * to find the first one that is scrollable in each direction.
 *
 * The following [mouse button] modifiers may be used:
 *  "left"
 *  "middle"
 *  "right"
 */
export const DragScroll: Directive<HTMLElement, IDragScrollOptions> = {
    mounted(el: HTMLElement, binding: DirectiveBinding) {
        if (!binding?.value?.id) {
            console.error("DragScroll must have a valid identifier.");
            return;
        }

        el.dataset[dataDragScrollId] = binding.value.id;
        createService(binding.value, binding.modifiers);

        el.style.setProperty(styleProp.cursor, "grab");
        el.addEventListener("mousedown", onMouseDown);
    },

    unmounted(el: HTMLElement, binding: DirectiveBinding) {
        if (binding?.value?.id) {
            destroyService(binding.value.id);
        }

        el.style.removeProperty(styleProp.cursor);
        el.style.removeProperty(styleProp.userSelect);

        el.removeEventListener("mousedown", onMouseDown);
        el.removeEventListener("contextmenu", preventContextMenu);

        document.removeEventListener("mousemove", onMouseMove);
        document.removeEventListener("mouseup", onMouseUp);
    }
};

/**
 * A directive to disable "drag-scroll" behavior for the host element and its descendants.
 *
 * Use this directive on any descendants of an element that has the `DragScroll` directive applied,
 * in order to opt-out of the "drag-scroll" behavior if this element (or any of its descendants)
 * are the target of the `MouseEvent`.
 */
export const NoDragScroll: Directive<HTMLElement> = {
    mounted(el: HTMLElement) {
        el.classList.add(noDragScrollClass);
    },

    unmounted(el: HTMLElement) {
        el.classList.remove(noDragScrollClass);
    }
};

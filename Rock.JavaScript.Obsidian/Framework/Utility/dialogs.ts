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

import { Guid } from "@Obsidian/Types";
import { ICancellationToken } from "./cancellation";
import { trackModalState } from "./page";

// eslint-disable-next-line @typescript-eslint/no-explicit-any, @typescript-eslint/naming-convention
declare const Rock: any;

/** The options that describe the dialog. */
export type DialogOptions = {
    /** The text to display inside the dialog. */
    message: string;

    /** A list of buttons to display, rendered left to right. */
    buttons: ButtonOptions[];

    /**
     * An optional container element for the dialog. If not specified then one
     * will be chosen automatically.
     */
    container?: string | Element;

    /**
     * An optional cancellation token that will dismiss the dialog automatically
     * and return `cancel` as the button clicked.
     */
    cancellationToken?: ICancellationToken;
};

/** The options that describe a single button in the dialog. */
export type ButtonOptions = {
    /** The key that uniquely identifies this button. */
    key: string;

    /** The text to display in the button. */
    label: string;

    /** The CSS classes to assign to the button, such as `btn btn-primary`. */
    className: string;
};

/**
 * Creates a dialog to display a message.
 *
 * @param body The body content to put in the dialog.
 * @param footer The footer content to put in the dialog.
 *
 * @returns An element that should be added to the body.
 */
function createDialog(body: HTMLElement | HTMLElement[], footer: HTMLElement | HTMLElement[] | undefined): HTMLElement {
    // Create the scrollable container that will act as a backdrop for the dialog.
    const scrollable = document.createElement("div");
    scrollable.classList.add("modal-scrollable");
    scrollable.style.zIndex = "1060";

    // Create the modal that will act as a container for the outer content.
    const modal = document.createElement("div");
    scrollable.appendChild(modal);
    modal.classList.add("modal", "fade");
    modal.tabIndex = -1;
    modal.setAttribute("role", "dialog");
    modal.setAttribute("aria-hidden", "false");
    modal.style.display = "block";

    // Create the inner dialog of the modal.
    const modalDialog = document.createElement("div");
    modal.appendChild(modalDialog);
    modalDialog.classList.add("modal-dialog");

    // Create the container for the inner content.
    const modalContent = document.createElement("div");
    modalDialog.appendChild(modalContent);
    modalContent.classList.add("modal-content");

    // Create the container for the body content.
    const modalBody = document.createElement("div");
    modalContent.appendChild(modalBody);
    modalBody.classList.add("modal-body");

    // Add all the body elements to the body.
    if (Array.isArray(body)) {
        for (const el of body) {
            modalBody.appendChild(el);
        }
    }
    else {
        modalBody.appendChild(body);
    }

    // If we have any footer content then create a footer.
    if (footer && (!Array.isArray(footer) || footer.length > 0)) {
        const modalFooter = document.createElement("div");
        modalContent.appendChild(modalFooter);
        modalFooter.classList.add("modal-footer");

        // Add all the footer elements to the footer.
        if (Array.isArray(footer)) {
            for (const el of footer) {
                modalFooter.appendChild(el);
            }
        }
        else {
            modalFooter.appendChild(footer);
        }
    }

    // Add a click handler to the background so the user gets feedback
    // that they can't just click away from the dialog.
    scrollable.addEventListener("click", () => {
        modal.classList.remove("animated", "shake");
        setTimeout(() => {
            modal.classList.add("animated", "shake");
        }, 0);
    });

    return scrollable;
}

/**
 * Construct a standard close button to be placed in the dialog.
 *
 * @returns A button element.
 */
function createCloseButton(): HTMLButtonElement {
    const closeButton = document.createElement("button");
    closeButton.classList.add("close");
    closeButton.type = "button";
    closeButton.style.marginTop = "-10px";
    closeButton.innerHTML = "&times;";

    return closeButton;
}

/**
 * Creates a standard backdrop element to be placed in the window.
 *
 * @returns An element to show that the background is not active.
 */
function createBackdrop(): HTMLElement {
    const backdrop = document.createElement("div");
    backdrop.classList.add("modal-backdrop");
    backdrop.style.zIndex = "1050";

    return backdrop;
}

/**
 * Shows a dialog modal. This is meant to look and behave like the standard
 * Rock.dialog.* functions, but this handles fullscreen mode whereas the old
 * methods do not.
 *
 * @param options The options that describe the dialog to be shown.
 *
 * @returns The key of the button that was clicked, or "cancel" if the cancel button was clicked.
 */
export function showDialog(options: DialogOptions): Promise<string> {
    return new Promise<string>(resolve => {
        let timer: NodeJS.Timeout | null = null;
        const container = document.fullscreenElement || document.body;
        const body = document.createElement("div");
        body.innerText = options.message;

        const buttons: HTMLElement[] = [];

        /**
         * Internal function to handle clearing the dialog and resolving the
         * promise.
         *
         * @param result The result to return in the promise.
         */
        function clearDialog(result: string): void {
            // This acts as a way to ensure only a single clear request happens.
            if (timer !== null) {
                return;
            }

            // The timout is used as a fallback in case we don't get the
            // transition end event.
            timer = setTimeout(() => {
                backdrop.remove();
                dialog.remove();
                trackModalState(false);

                resolve(result);
            }, 1000);

            modal.addEventListener("transitionend", () => {
                if (timer) {
                    clearTimeout(timer);
                }

                backdrop.remove();
                dialog.remove();
                trackModalState(false);

                resolve(result);
            });

            modal.classList.remove("in");
            backdrop.classList.remove("in");
        }

        // Add in all the buttons specified.
        for (const button of options.buttons) {
            const btn = document.createElement("button");
            btn.classList.value = button.className;
            btn.type = "button";
            btn.innerText = button.label;
            btn.addEventListener("click", () => {
                clearDialog(button.key);
            });
            buttons.push(btn);
        }

        // Construct the close (cancel) button.
        const closeButton = createCloseButton();
        closeButton.addEventListener("click", () => {
            clearDialog("cancel");
        });

        const dialog = createDialog([closeButton, body], buttons);
        const backdrop = createBackdrop();

        const modal = dialog.querySelector(".modal") as HTMLElement;

        // Do final adjustments to the elements and add to the body.
        trackModalState(true);
        container.appendChild(dialog);
        container.appendChild(backdrop);
        modal.style.marginTop = `-${modal.offsetHeight / 2.0}px`;

        // Show the backdrop and the modal.
        backdrop.classList.add("in");
        modal.classList.add("in");

        // Handle dismissal of the dialog by cancellation token.
        options.cancellationToken?.onCancellationRequested(() => {
            clearDialog("cancel");
        });
    });
}

/**
 * Shows an alert message that requires the user to acknowledge.
 *
 * @param message The message text to be displayed.
 *
 * @returns A promise that indicates when the dialog has been dismissed.
 */
export async function alert(message: string): Promise<void> {
    await showDialog({
        message,
        buttons: [
            {
                key: "ok",
                label: "OK",
                className: "btn btn-primary"
            }
        ]
    });
}

/**
 * Shows a confirmation dialog that consists of OK and Cancel buttons. The
 * user will be required to click one of these two buttons.
 *
 * @param message The message to be displayed inside the dialog.
 *
 * @returns A promise that indicates when the dialog has been dismissed. The
 * value will be true if the OK button was clicked or false otherwise.
 */
export async function confirm(message: string): Promise<boolean> {
    const result = await showDialog({
        message,
        buttons: [
            {
                key: "ok",
                label: "OK",
                className: "btn btn-primary"
            },
            {
                key: "cancel",
                label: "Cancel",
                className: "btn btn-default"
            }
        ]
    });

    return result === "ok";
}

/**
 * Shows a delete confirmation dialog that consists of OK and Cancel buttons.
 * The user will be required to click one of these two buttons. The message
 * is standardized.
 *
 * @param nameText The name of type that will be deleted.
 *
 * @returns A promise that indicates when the dialog has been dismissed. The
 * value will be true if the OK button was clicked or false otherwise.
 */
export function confirmDelete(typeName: string, additionalMessage?: string): Promise<boolean> {
    let message = `Are you sure you want to delete this ${typeName}?`;

    if (additionalMessage) {
        message += ` ${additionalMessage}`;
    }

    return confirm(message);
}

/**
 * Shows the security dialog for the given entity.
 *
 * @param entityTypeIdKey The identifier of the entity's type.
 * @param entityIdKey The identifier of the entity to secure.
 * @param entityTitle The title of the entity. This is used to construct the modal title.
 */
export function showSecurity(entityTypeIdKey: Guid | string | number, entityIdKey: Guid | string | number, entityTitle: string = "Item"): void {
    Rock.controls.modal.show(undefined, `/Secure/${entityTypeIdKey}/${entityIdKey}?t=Secure ${entityTitle}&pb=&sb=Done`);
}

/**
 * Shows the child pages for the given page.
 * @param pageId The page identifier
 */
export function showChildPages(pageId: Guid | string | number): void {
    Rock.controls.modal.show(undefined, `/pages/${pageId}?t=Child Pages&amp;pb=&amp;sb=Done`);
}
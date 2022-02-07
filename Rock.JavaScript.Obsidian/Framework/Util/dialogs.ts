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

type Bootbox = {
    dialog: (options: BootboxOptions) => void;
};

type BootboxOptions = {
    message: string;
    buttons: Record<string, ButtonOptions>;
};

type ButtonOptions = {
    label: string;
    className: string;
    callback?: () => void;
};

declare global {
    /* eslint-disable-next-line */
    var bootbox: Bootbox;
}

/**
 * Shows an alert message that requires the user to acknowledge.
 * 
 * @param message The message text to be displayed.
 *
 * @returns A promise that indicates when the dialog has been dismissed.
 */
export function alert(message: string): Promise<void> {
    return new Promise<void>(resolve => {
        bootbox.dialog({
            message,
            buttons: {
                ok: {
                    label: "OK",
                    className: "btn-primary",
                    callback: () => resolve()
                }
            }
        });
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
export function confirm(message: string): Promise<boolean> {
    return new Promise<boolean>(resolve => {
        bootbox.dialog({
            message,
            buttons: {
                ok: {
                    label: "OK",
                    className: "btn-primary",
                    callback: function () {
                        resolve(true);
                    }
                },
                cancel: {
                    label: "Cancel",
                    className: "btn-default",
                    callback: function () {
                        resolve(false);
                    }
                }
            }
        });
    });
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

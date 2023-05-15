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

import mitt, { Emitter } from "mitt";

// NOTE: Much of the logic for this was taken from VSCode's MIT licensed version:
// https://github.com/microsoft/vscode/blob/342394d1e7d43d3324dc2ede1d634cffd52ba159/src/vs/base/common/cancellation.ts

/**
 * A cancellation token can be used to instruct some operation to run but abort
 * if a certain condition is met.
 */
export interface ICancellationToken {
    /**
     * A flag signalling is cancellation has been requested.
     */
    readonly isCancellationRequested: boolean;

    /**
     * Registers a listener for when cancellation has been requested. This event
     * only ever fires `once` as cancellation can only happen once. Listeners
     * that are registered after cancellation will be called (next event loop run),
     * but also only once.
     *
     * @param listener The function to be called when the token has been cancelled.
     */
    onCancellationRequested(listener: () => void): void;
}

function shortcutCancelledEvent(listener: () => void): void {
    window.setTimeout(listener, 0);
}

/**
 * Determines if something is a cancellation token.
 *
 * @param thing The thing to be checked to see if it is a cancellation token.
 *
 * @returns true if the @{link thing} is a cancellation token, otherwise false.
 */
export function isCancellationToken(thing: unknown): thing is ICancellationToken {
    if (thing === CancellationTokenNone || thing === CancellationTokenCancelled) {
        return true;
    }
    if (thing instanceof MutableToken) {
        return true;
    }
    if (!thing || typeof thing !== "object") {
        return false;
    }
    return typeof (thing as ICancellationToken).isCancellationRequested === "boolean"
        && typeof (thing as ICancellationToken).onCancellationRequested === "function";
}

/**
 * A cancellation token that will never be in a cancelled state.
 */
export const CancellationTokenNone = Object.freeze<ICancellationToken>({
    isCancellationRequested: false,
    onCancellationRequested() {
        // Intentionally blank.
    }
});

/**
 * A cancellation token that is already in a cancelled state.
 */
export const CancellationTokenCancelled = Object.freeze<ICancellationToken>({
    isCancellationRequested: true,
    onCancellationRequested: shortcutCancelledEvent
});

/**
 * Internal implementation of a cancellation token that starts initially as
 * active but can later be switched to a cancelled state.
 */
class MutableToken implements ICancellationToken {
    private isCancelled: boolean = false;
    private emitter: Emitter<Record<string, unknown>> | null = null;

    /**
     * Cancels the token and fires any registered event listeners.
     */
    public cancel(): void {
        if (!this.isCancelled) {
            this.isCancelled = true;
            if (this.emitter) {
                this.emitter.emit("cancel", undefined);
                this.emitter = null;
            }
        }
    }

    // #region ICancellationToken implementation

    get isCancellationRequested(): boolean {
        return this.isCancelled;
    }

    onCancellationRequested(listener: () => void): void {
        if (this.isCancelled) {
            return shortcutCancelledEvent(listener);
        }

        if (!this.emitter) {
            this.emitter = mitt();
        }

        this.emitter.on("cancel", listener);
    }

    // #endregion
}

/**
 * Creates a source instance that can be used to trigger a cancellation
 * token into the cancelled state.
 */
export class CancellationTokenSource {
    /** The token that can be passed to functions. */
    private internalToken?: ICancellationToken = undefined;

    /**
     * Creates a new instance of {@link CancellationTokenSource}.
     *
     * @param parent The parent cancellation token that will also cancel this source.
     */
    constructor(parent?: ICancellationToken) {
        if (parent) {
            parent.onCancellationRequested(() => this.cancel());
        }
    }

    /**
     * The cancellation token that can be used to determine when the task
     * should be cancelled.
     */
    get token(): ICancellationToken {
        if (!this.internalToken) {
            // be lazy and create the token only when
            // actually needed
            this.internalToken = new MutableToken();
        }

        return this.internalToken;
    }

    /**
     * Moves the token into a cancelled state.
     */
    cancel(): void {
        if (!this.internalToken) {
            // Save an object creation by returning the default cancelled
            // token when cancellation happens before someone asks for the
            // token.
            this.internalToken = CancellationTokenCancelled;

        }
        else if (this.internalToken instanceof MutableToken) {
            // Actually cancel the existing token.
            this.internalToken.cancel();
        }
    }
}

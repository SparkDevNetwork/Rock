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
import { newGuid } from "./guid";
import { inject, nextTick, provide } from "vue";

const suspenseSymbol = Symbol("RockSuspense");

/**
 * Defines the interface for a provider of suspense monitoring. These are used
 * to track asynchronous operations that components may be performing so the
 * watching component can perform an operation once all pending operations
 * have completed.
 */
export interface ISuspenseProvider {
    /**
     * Adds a new operation identified by the promise. When the promise
     * either resolves or fails the operation is considered completed.
     *
     * @param operation The promise that represents the operation.
     */
    addOperation(operation: Promise<unknown>): void;

    /**
     * Notes that an asynchronous operation has started on a child component.
     *
     * @param key The key that identifies the operation.
     */
    startAsyncOperation(key: Guid): void;

    /**
     * Notes that an asynchrounous operation has completed on a child component.
     *
     * @param key The key that was previously passed to startAsyncOperation.
     */
    completeAsyncOperation(key: Guid): void;
}

/**
 * A basic provider that handles the guts of a suspense provider. This can be
 * used by components that need to know when child components have completed
 * their work.
 */
export class BasicSuspenseProvider implements ISuspenseProvider {
    private readonly operationKey: Guid;

    private readonly parentProvider: ISuspenseProvider | undefined;

    private readonly pendingOperations: Guid[];

    private finishedHandlers: (() => void)[];

    /**
     * Creates a new suspense provider.
     *
     * @param parentProvider The parent suspense provider that will be notified of pending operations.
     */
    constructor(parentProvider: ISuspenseProvider | undefined) {
        this.operationKey = newGuid();
        this.parentProvider = parentProvider;
        this.pendingOperations = [];
        this.finishedHandlers = [];
    }

    /**
     * Called when all pending operations are complete. Notifies all handlers
     * that the pending operations have completed as well as the parent provider.
     */
    private allOperationsComplete(): void {
        // Wait until the next Vue tick in case a new async operation started.
        // This can happen, for example, with defineAsyncComponent(). It will
        // complete its async operation (loading the JS file) and then the
        // component defined in the file might start an async operation. This
        // prevents us from completing too soon.
        nextTick(() => {
            // Verify nothing started a new asynchronous operation while we
            // we waiting for the next tick.
            if (this.pendingOperations.length !== 0) {
                return;
            }

            // Notify all pending handlers that all operations completed.
            for (const handler of this.finishedHandlers) {
                handler();
            }
            this.finishedHandlers = [];

            // Notify the parent that our own pending operation has completed.
            if (this.parentProvider) {
                this.parentProvider.completeAsyncOperation(this.operationKey);
            }
        });
    }

    /**
     * Adds a new operation identified by the promise. When the promise
     * either resolves or fails the operation is considered completed.
     *
     * @param operation The promise that represents the operation.
     */
    public addOperation(operation: Promise<unknown>): void {
        const operationKey = newGuid();

        this.startAsyncOperation(operationKey);

        operation.then(() => this.completeAsyncOperation(operationKey))
            .catch(() => this.completeAsyncOperation(operationKey));
    }

    /**
     * Notes that an asynchronous operation has started on a child component.
     *
     * @param key The key that identifies the operation.
     */
    public startAsyncOperation(key: Guid): void {
        this.pendingOperations.push(key);

        // If this is the first operation we started, notify the parent provider.
        if (this.pendingOperations.length === 1 && this.parentProvider) {
            this.parentProvider.startAsyncOperation(this.operationKey);
        }
    }

    /**
     * Notes that an asynchrounous operation has completed on a child component.
     *
     * @param key The key that was previously passed to startAsyncOperation.
     */
    public completeAsyncOperation(key: Guid): void {
        const index = this.pendingOperations.indexOf(key);

        if (index !== -1) {
            this.pendingOperations.splice(index, 1);
        }

        // If this was the last operation then send notifications.
        if (this.pendingOperations.length === 0) {
            this.allOperationsComplete();
        }
    }

    /**
     * Checks if this provider has any asynchronous operations that are still
     * pending completion.
     *
     * @returns true if there are pending operations; otherwise false.
     */
    public hasPendingOperations(): boolean {
        return this.pendingOperations.length > 0;
    }

    /**
     * Adds a new handler that is called when all pending operations have been
     * completed. This is a fire-once, meaning the callback will only be called
     * when the current pending operations have completed. If new operations
     * begin after the callback is executed it will not be called again unless
     * it is added with this method again.
     *
     * @param callback The function to call when all pending operations have completed.
     */
    public addFinishedHandler(callback: () => void): void {
        this.finishedHandlers.push(callback);
    }
}

/**
 * Provides a new suspense provider to any child components.
 *
 * @param provider The provider to make available to child components.
 */
export function provideSuspense(provider: ISuspenseProvider): void {
    provide(suspenseSymbol, provider);
}

/**
 * Uses the current suspense provider that was defined by any parent component.
 *
 * @returns The suspense provider if one was defined; otherwise undefined.
 */
export function useSuspense(): ISuspenseProvider | undefined {
    return inject<ISuspenseProvider | undefined>(suspenseSymbol, undefined);
}


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

/**
 * Returns a promise that completes after the specified number of milliseconds
 * have ellapsed.
 * 
 * @param ms The number of milliseconds to wait.
 * 
 * @returns A promise that completes after the interval has ellapsed.
 */
export function sleep(ms: number): Promise<void> {
    return new Promise<void>(resolve => {
        setTimeout(resolve, ms);
    });
}

/**
 * Checks if the value is a promise to return a value. This is used to check
 * if a function that could have returned either a value or a promise for a
 * value returned a promise.
 * 
 * @param obj The object to be tested if it is a promise.
 *
 * @returns True if the object is a promise.
 */
export function isPromise<T>(obj: PromiseLike<T> | T): obj is PromiseLike<T> {
    return !!obj && (typeof obj === "object" || typeof obj === "function") && typeof (obj as Record<string, unknown>).then === "function";
}

/**
 * A class that provides a way to defer execution via await until some
 * external trigger happens.
 */
export class PromiseCompletionSource<T = void> {
    private internalPromise: Promise<T>;

    private internalResolve: (T) => void = () => { /* Intentionally blank. */ };

    private internalReject: (reason?: unknown) => void = () => { /* Intentionally blank. */ };

    constructor() {
        this.internalPromise = new Promise<T>((resolve, reject) => {
            this.internalResolve = resolve;
            this.internalReject = reject;
        });
    }

    /** The promise that can be awaited. */
    public get promise(): Promise<T> {
        return this.internalPromise;
    }

    /**
     * Resolves the promise with the given value.
     * 
     * @param value The value to be returned by the await call.
     */
    public resolve(value: T): void {
        this.internalResolve(value);
    }

    /**
     * Rejects the promise and throws the reason as an error.
     * 
     * @param reason The reason to be thrown by the await call.
     */
    public reject(reason?: unknown): void {
        this.internalReject(reason);
    }
}

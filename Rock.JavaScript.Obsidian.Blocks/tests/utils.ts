import { DOMWrapper } from "@vue/test-utils";

export type WaitForOptions = {
    /** The number of milliseconds at most to wait. Default is 1000ms. */
    timeout?: number;

    /**
     * The interval between calls to your callback, though your callback
     * will be called before the first wait. Default is 50ms.
     */
    interval?: number;
};

/**
 * Wait until the callback completes without error.
 *
 * @param callback The callback that will be repeatedly called until it no longer throws an error.
 * @param options Optional set of options that configure the operation.
 *
 * @returns The result of the callback.
 */
export function waitFor<T>(callback: () => (T | Promise<T>), options?: WaitForOptions): Promise<T> {
    // Justification: This is a special case where we need to return instantly
    // so that the promise works as a promise, but we then need to internally
    // wait for the callback to not throw an error.
    // eslint-disable-next-line no-async-promise-executor
    return new Promise<T>(async (resolve, reject) => {
        let lastError: unknown = undefined;
        let promiseStatus: "waiting" | "done" = "waiting";
        let checkTimer: number | undefined = undefined;

        setTimeout(() => {
            clearTimeout(checkTimer);

            if (promiseStatus !== "done") {
                promiseStatus = "done";

                if (lastError !== undefined) {
                    reject(lastError);
                }
                else {
                    reject(new Error("Timed out in waitFor."));
                }
            }
        }, options?.timeout ?? 1000);

        runCheck();

        function runCheck(): void {
            try {
                const result = callback();

                if (promiseStatus !== "done") {
                    promiseStatus = "done";

                    resolve(result);
                }
            }
            catch (e) {
                if (promiseStatus !== "done") {
                    lastError = e;
                    checkTimer = window.setTimeout(runCheck, options?.interval ?? 50);
                }
            }
        }
    });
}

export function findAllMatching<T extends Element = Element>(parent: Omit<DOMWrapper<Element>, "exists">, selector: string, filter: (node: DOMWrapper<T>) => boolean): DOMWrapper<T>[] {
    return parent.findAll(selector).filter(n => filter(n as DOMWrapper<T>)) as DOMWrapper<T>[];
}

export function getMatching<T extends Element = Element>(parent: Omit<DOMWrapper<Element>, "exists">, selector: string, filter: (node: DOMWrapper<T>) => boolean): DOMWrapper<T> {
    const matching = findAllMatching(parent, selector, filter);

    if (matching.length === 0) {
        throw new Error(`No element matched filter.`);
    }

    return matching[0] as DOMWrapper<T>;
}

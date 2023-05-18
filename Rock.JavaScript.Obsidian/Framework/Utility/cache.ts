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

import { RockDateTime } from "./rockDateTime";

//
type CacheEntry<T> = {
    value: T;
    expiration: number;
};

/**
* Stores the value using the given key. The cache will expire at the expiration or in
* 1 minute if none is provided
* @param key
* @param value
* @param expiration
*/
function set<T>(key: string, value: T, expirationDT: RockDateTime | null = null): void {
    let expiration: number;

    if (expirationDT) {
        expiration = expirationDT.toMilliseconds();
    }
    else {
        // Default to one minute
        expiration = RockDateTime.now().addMinutes(1).toMilliseconds();
    }

    const cache: CacheEntry<T> = { expiration, value };
    const cacheJson = JSON.stringify(cache);
    sessionStorage.setItem(key, cacheJson);
}

/**
 * Gets a stored cache value if there is one that has not yet expired.
 * @param key
 */
function get<T>(key: string): T | null {
    const cacheJson = sessionStorage.getItem(key);

    if (!cacheJson) {
        return null;
    }

    const cache = JSON.parse(cacheJson) as CacheEntry<T>;

    if (!cache || !cache.expiration) {
        return null;
    }

    if (cache.expiration < RockDateTime.now().toMilliseconds()) {
        return null;
    }

    return cache.value;
}

const promiseCache: Record<string, Promise<unknown> | undefined> = {};

/**
 * Since Promises can't be cached, we need to store them in memory until we get the result back. This wraps
 * a function in another function that returns a promise and...
 * - If there's a cached result, return it
 * - Otherwise if there's a cached Promise, return it
 * - Otherwise call the given function and cache it's promise and return it. Once the the Promise resolves, cache its result
 *
 * @param key Key for identifying the cached values
 * @param fn Function that returns a Promise that we want to cache the value of
 *
 */
function cachePromiseFactory<T>(key: string, fn: () => Promise<T>, expiration: RockDateTime | null = null): () => Promise<T> {
    return async function (): Promise<T> {
        // If it's cached, grab it
        const cachedResult = get<T>(key);
        if (cachedResult) {
            return cachedResult;
        }

        // If it's not cached yet but we've already started fetching it
        // (it's not cached until we receive the results), return the existing Promise
        if (promiseCache[key]) {
            return promiseCache[key] as Promise<T>;
        }

        // Not stored anywhere, so fetch it and save it on the stored Promise for the next call
        promiseCache[key] = fn();

        // Once it's resolved, cache the result
        promiseCache[key]?.then((result) => {
            set(key, result, expiration);
            delete promiseCache[key];
            return result;
        }).catch((e: Error) => {
            // Something's wrong, let's get rid of the stored promise, so we can try again.
            delete promiseCache[key];
            throw e;
        });

        return promiseCache[key] as Promise<T>;
    };
}


export default {
    set,
    get,
    cachePromiseFactory: cachePromiseFactory
};

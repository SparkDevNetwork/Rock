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
type CacheEntry<T> = {
    value: T;
    expiration: Date | string;
};

/**
* Stores the value using the given key. The cache will expire at the expiration or in
* 1 minute if none is provided
* @param key
* @param value
* @param expiration
*/
function set<T>(key: string, value: T, expiration: Date | null = null) {
    if (!expiration) {
        // Default to one minute
        const now = new Date();
        const oneMinute = 60000; // One minute: 1 * 60 * 1000
        expiration = new Date(now.getTime() + oneMinute);
    }

    const cache: CacheEntry<T> = { expiration, value };
    const cacheJson = JSON.stringify(cache);
    sessionStorage.setItem(key, cacheJson);
}

/**
 * Gets a stored cache value if there is one that has not yet expired.
 * @param key
 */
function get<T>(key: string) {
    const cacheJson = sessionStorage.getItem(key);

    if (!cacheJson) {
        return null;
    }

    const cache = JSON.parse(cacheJson) as CacheEntry<T>;

    if (!cache || !cache.expiration) {
        return null;
    }

    const expiration = new Date(cache.expiration);

    if (!expiration || expiration < new Date()) {
        return null;
    }

    return cache.value;
}

export default {
    set,
    get
};
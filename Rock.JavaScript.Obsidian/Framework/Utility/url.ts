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

import { Ref, watch } from "vue";

/**
 * Is the value a valid URL?
 * @param val
 */
export function isUrl(val: unknown): boolean {
    if (typeof val === "string") {
        // https://www.regextester.com/1965
        // Modified from link above to support urls like "http://localhost:6229/Person/1/Edit" (Url does not have a period)
        const re = /^(http[s]?:\/\/)?[^\s(["<,>]*\.?[^\s[",><]*$/;
        return re.test(val);
    }

    return false;
}

/**
 * Make the URL safe to use for redirects.
 *
 * - A URL on this site is returned unchanged.
 * - A URL on another site keeps its path and query, but on this site.
 * - Anything else (e.g. javascript:, or a URL that can't be parsed) is "/".
 *
 * Allowed Redirect Domain(s) aren't checked here. To redirect to another
 * site, validate the URL on the server with SiteCache.IsSafeRedirectUrl()
 * instead.
 *
 * @param url The URL to redirect to.
 *
 * @returns A URL that is safe to assign to window.location.href.
 */
export function makeUrlRedirectSafe(url: string): string {
    let u: URL;

    try {
        // Resolve the URL exactly as the browser will when it's assigned to
        // window.location.href.
        u = new URL(url, document.baseURI);
    }
    catch {
        return "/";
    }

    // Only web pages are safe; this rejects javascript:, data:, etc.
    if (u.protocol !== "http:" && u.protocol !== "https:") {
        return "/";
    }

    if (u.origin === window.location.origin) {
        return url;
    }

    // Keep the path but stay on this site. Prefixing the origin keeps a path
    // like "//unsafe.com" from being read as another site.
    return `${window.location.origin}${u.pathname}${u.search}`;
}

/**
 * Keep a list of named Refs synchronized with URL query parameters in the address of the same names.
 * If there are already query parameters in the URL with those names, the Refs will be assigned those
 * values. This will also watch those Refs for changes and update the query parameters to reflect
 * those changes.
 *
 * @param refs An object where the keys represent the query parameters keys to keep synchronized with
 * and the values are the Refs those query parameters are synched with.
 */
export function syncRefsWithQueryParams(refs: Record<string, Ref>): void {
    // Get current query parameters
    const params = new URLSearchParams(window.location.search);

    Object.entries(refs).forEach(([key, ref]: [string, Ref]) => {
        let param = null;

        // try to get the decoded parameter value
        try {
            param = JSON.parse(decodeURI(params.get(key) ?? ""));
        }
        catch (e) { /* just leave the param as null */ }

        // If we found a value, set the Ref to it
        if (param != null) {
            ref.value = param;
        }

        // keep URL params up-to-date with changes to this Ref
        watch(ref, updater(key));
    });

    //
    function updater(key) {
        return (value) => {
            params.set(key, encodeURI(JSON.stringify(value)));

            history.replaceState(null, "", "?" + params.toString());
        };
    }
}

/**
 * Removes query parameters from the current URL and replaces the state in history.
 *
 * @param queryParamKeys The string array of query parameter keys to remove from the current URL.
 */
export function removeCurrentUrlQueryParams(...queryParamKeys: string[]): (string | null)[] {
    return removeUrlQueryParams(window.location.href, ...queryParamKeys);
}

/**
 * Removes query parameters from the current URL and replaces the state in history.
 *
 * @param url The URL from which to remove the query parameters.
 * @param queryParamKeys The string array of query parameter keys to remove from the current URL.
 */
export function removeUrlQueryParams(url: string | URL, ...queryParamKeys: string[]): (string | null)[] {
    if (!queryParamKeys || !queryParamKeys.length) {
        return [];
    }

    if (typeof url === "string") {
        url = new URL(url);
    }

    const queryParams = url.searchParams;

    const removedQueryParams: (string | null)[] = [];

    for (let i = 0; i < queryParamKeys.length; i++) {
        const queryParamKey = queryParamKeys[i];
        removedQueryParams.push(queryParams.get(queryParamKey));
        queryParams.delete(queryParamKey);
    }

    window.history.replaceState(null, "", url);

    return removedQueryParams;
}
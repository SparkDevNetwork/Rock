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
import { isNullish } from "./util";
import { asBooleanOrNull, asTrueOrFalseString } from "./booleanUtils";
import { toNumberOrNull } from "./numberUtils";

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
 * The types of query parameters that can be synchronized with Refs using the
 * syncRefsWithQueryParams function.
 */
export type QueryParamType = "string" | "number" | "boolean" | "json";

/**
 * Defines the binding between a Ref and a query parameter, including the query
 * parameter name and expected type for proper parsing and serialization.
 */
export type QueryParamBinding = {
    /**
     * The query parameter name in the URL.
     */
    param: string;

    /**
     * The Ref to keep synchronized with the query parameter.
     */
    ref: Ref;

    /**
     * The expected type of the query parameter for parsing/serialization.
     */
    type: QueryParamType;
};

/**
 * Keep a list of named Refs synchronized with URL query parameters. If there
 * are already query parameters in the URL with matching parameter names, the
 * Refs will be assigned those values. This will also watch those Refs for
 * changes and update the query parameters to reflect those changes.
 *
 * @param bindings The list of bindings that define which Refs to sync with
 * which query parameters, and how to parse/serialize them.
 */
export function syncRefsWithQueryParams(bindings: QueryParamBinding[]): void {
    let isReplaceStateScheduled = false;

    /**
     * Prevent multiple calls to history.replaceState in the same tick by
     * scheduling it to run in a microtask and ignoring any additional calls
     * until it runs.
     */
    function scheduleReplaceState(): void {
        if (isReplaceStateScheduled) {
            return;
        }

        isReplaceStateScheduled = true;

        queueMicrotask(() => {
            try {
                const qs = params.toString();
                const { pathname, search, hash } = window.location;
                const nextUrl = qs ? `${pathname}?${qs}${hash}` : `${pathname}${hash}`;
                const currentUrl = `${pathname}${search}${hash}`;

                if (nextUrl !== currentUrl) {
                    history.replaceState(null, "", nextUrl);
                }
            }
            finally {
                isReplaceStateScheduled = false;
            }
        });
    }

    // Get current query parameters.
    const params = new URLSearchParams(window.location.search);

    // Loop through bindings. If there is a query parameter in the URL that
    // matches a binding's param, set the ref to that value (after parsing it
    // to the correct type). Then watch the ref for changes and update the
    // query parameter when it changes.
    for (const binding of bindings) {
        const { param, ref, type } = binding;

        // If we find a value in the URL, set the ref to it.
        const raw = params.get(param);
        if (!isNullish(raw)) {
            const parsed = parse(raw, type);
            if (!isNullish(parsed)) {
                ref.value = parsed;
            }
            else {
                // Don't change the ref value if we can't parse the URL value.
            }
        }

        // Keep URL params up-to-date with changes to this ref.
        watch(ref, (value) => {
            if (isNullish(value)) {
                params.delete(param);
            }
            else {
                const serialized = serialize(value, type);
                if (isNullish(serialized)) {
                    params.delete(param);
                }
                else {
                    params.set(param, serialized);
                }
            }

            scheduleReplaceState();
        });
    }

    /**
     * Parse the raw string value from the URL into the correct type based on
     * the provided QueryParamType.
     *
     * @param raw The raw string value from the URL.
     * @param type The type to which the raw value should be parsed.
     * @returns The parsed value or null if parsing fails.
     */
    function parse(raw: string, type: QueryParamType): unknown | null {
        switch (type) {
            case "string":
                return raw;

            case "boolean": {
                return asBooleanOrNull(raw);
            }

            case "number": {
                return toNumberOrNull(raw);
            }

            case "json": {
                try {
                    return JSON.parse(raw);
                }
                catch {
                    return null;
                }
            }
        }
    }

    /**
     * Serialize the value to a string that can be stored in the URL based on
     * the provided QueryParamType. If the value cannot be serialized to the
     * specified type, null is returned.
     *
     * @param value The value to be serialized.
     * @param type The type to which the value should be serialized.
     * @returns The serialized string or null if serialization fails.
     */
    function serialize(value: unknown, type: QueryParamType): string | null {
        switch (type) {
            case "string":
                return typeof value === "string"
                    ? value
                    : null;

            case "boolean": return typeof value === "boolean"
                ? asTrueOrFalseString(value)
                : null;

            case "number":
                return typeof value === "number" && Number.isFinite(value)
                    ? String(value)
                    : null;

            case "json": {
                try {
                    return JSON.stringify(value);
                }
                catch {
                    return null;
                }
            }
        }
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
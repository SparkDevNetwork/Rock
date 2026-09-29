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

import { inject } from "vue";
import { HttpResult } from "@Obsidian/Types/Utility/http";
import { doApiCall } from "./http";

/**
 * The header a Lava endpoint checks for cross-site request forgery protection.
 * Protection is enabled by default on every endpoint, so a request without this
 * header is answered with a 401.
 */
const crossSiteForgeryHeaderKey = "X-Helix-CSRF-Protection";

/**
 * The version segment of the Lava application route. It is part of the route
 * itself and is not related to the application or endpoint being addressed.
 */
const routeVersion = "1";

/** Options that change how a single endpoint call is made. */
export type LavaAppInvokeOptions = {
    /** The HTTP method. Endpoints are keyed by slug AND method, so this selects which endpoint. */
    method?: "GET" | "POST";
};

/** A bound Lava application that can invoke its endpoints by name. */
export type LavaApp = {
    invoke: <T>(endpointSlug: string, data?: Record<string, unknown>, options?: LavaAppInvokeOptions) => Promise<HttpResult<T>>;
};

/**
 * Calls a single endpoint on a Lava application.
 *
 * @param applicationSlug The slug of the Lava application that owns the endpoint.
 * @param endpointSlug The slug of the endpoint to call.
 * @param data The values to send. Query parameters for GET, the request body for POST.
 * @param options The options that change how the call is made.
 * @param parentTrace The page-render traceparent, sent so the call links to the page view's trace.
 *
 * @returns The result of the call, in the same shape a block action returns.
 */
async function invokeEndpoint<T>(applicationSlug: string, endpointSlug: string, data?: Record<string, unknown>, options?: LavaAppInvokeOptions, parentTrace?: string | null): Promise<HttpResult<T>> {
    const method = options?.method ?? "POST";
    const url = `/api/v2/lava-app/${routeVersion}/${applicationSlug}/${endpointSlug}`;

    const headers: Record<string, string> = {
        [crossSiteForgeryHeaderKey]: "true"
    };

    if (parentTrace) {
        headers["traceparent"] = parentTrace;
    }

    const result = await doApiCall<T>(
        method,
        url,
        method === "GET" ? data : undefined,
        method === "GET" ? undefined : data,
        { headers });

    // An endpoint whose content type is still text/html arrives as a string even
    // when its template emits JSON, so parse it rather than handing the caller
    // raw text it cannot use.
    if (typeof result.data === "string") {
        try {
            return {
                ...result,
                data: JSON.parse(result.data) as T
            };
        }
        catch (e) {
            return {
                statusCode: result.statusCode,
                data: null,
                isSuccess: false,
                isError: true,
                errorMessage: `The response from the '${endpointSlug}' endpoint was not valid JSON: ${e}`
            };
        }
    }

    return result;
}

/**
 * Binds a Lava application so its endpoints can be invoked by name, much like
 * invoking a block action.
 *
 * Must be called during component setup: it injects the hosting block's
 * page-render trace so every invocation links back to the page view in
 * observability. Outside a block tree the trace is simply absent and calls
 * still work.
 *
 * @param applicationSlug The slug of the Lava application to bind.
 *
 * @returns An object whose invoke function calls the application's endpoints.
 *
 * @example
 * const lavaApp = useLavaApp("giving-dashboard");
 * const summary = await lavaApp.invoke<SummaryBag>("summary");
 */
export function useLavaApp(applicationSlug: string): LavaApp {
    const parentTrace = inject<string | null>("blockParentTrace", null);

    return {
        invoke: <T>(endpointSlug: string, data?: Record<string, unknown>, options?: LavaAppInvokeOptions): Promise<HttpResult<T>> => {
            return invokeEndpoint<T>(applicationSlug, endpointSlug, data, options, parentTrace);
        }
    };
}

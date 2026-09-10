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

import { InvokeBlockActionFunc } from "@Obsidian/Types/Utility/block";
import { HttpBodyData, HttpFunctions, HttpMethod, HttpResult, HttpUrlParams } from "@Obsidian/Types/Utility/http";

/**
 * The block action that answers each check-in endpoint. Anything the flow
 * reaches for that is not listed here fails loudly rather than silently
 * returning nothing, which is what keeps checkout and family search from
 * appearing on a page that never offered them.
 */
const blockActionNames: Record<string, string> = {
    FamilyMembers: "GetFamilyMembers",
    AttendeeOpportunities: "GetAttendeeOpportunities",
    SaveAttendance: "SaveAttendance",
    ConfirmAttendance: "ConfirmAttendance"
};

/**
 * The one endpoint that carries its session guid in the url rather than a body,
 * and the only one reached by a verb other than POST.
 */
const pendingAttendanceEndpoint = "pendingattendance";

/**
 * Gets the endpoint name from a check-in API url, ignoring any query string.
 */
function getEndpointName(url: string): string {
    return url.split("?")[0].split("/").pop() ?? "";
}

/**
 * What the adapter reports back to the block about the calls passing through
 * it. The imported screens make their own server calls without announcing
 * them, so this is the only place those can be observed.
 */
export type CheckInHttpAdapterCallbacks = {
    /**
     * Called with whether attendance is staged on the server awaiting
     * confirmation. Reported from here because the flow's sessions are
     * immutable clones, so the one the block holds when a check-in is
     * abandoned may predate the save.
     */
    onPendingAttendanceChanged?: (isStaged: boolean) => void;

    /**
     * Called with whether a check-in request is in flight, so the block can
     * stand down the navigation that would otherwise move the flow out from
     * under an unfinished call.
     */
    onBusyChanged?: (isBusy: boolean) => void;
};

/**
 * Creates the HTTP functions that let the check-in flow run against this
 * block's actions instead of the check-in REST API. The REST controller denies
 * anonymous callers, and an individual checking themselves in is one.
 *
 * @param invokeBlockAction The block action invoker to route calls through.
 * @param callbacks What the adapter reports back to the block.
 *
 * @returns The HTTP functions to construct a check-in session with.
 */
export function createCheckInHttpAdapter(invokeBlockAction: InvokeBlockActionFunc, callbacks?: CheckInHttpAdapterCallbacks): HttpFunctions {
    let inFlightCount = 0;

    /**
     * Runs a block action, reporting the flow as busy for as long as it and
     * any other call started alongside it are outstanding.
     *
     * @param actionName The block action to run.
     * @param data The parameters to pass to the action.
     *
     * @returns The result of the block action.
     */
    async function invoke<T>(actionName: string, data: Record<string, unknown>): Promise<HttpResult<T>> {
        inFlightCount += 1;
        callbacks?.onBusyChanged?.(true);

        try {
            return await invokeBlockAction<T>(actionName, data);
        }
        finally {
            inFlightCount -= 1;

            if (inFlightCount === 0) {
                callbacks?.onBusyChanged?.(false);
            }
        }
    }
    /**
     * Refuses an endpoint this block does not answer, naming it so the failure
     * can be traced rather than passing silently as an empty result.
     *
     * @param method The HTTP method that was attempted.
     * @param url The check-in API url that was attempted.
     */
    function unsupported<T>(method: string, url: string): Promise<HttpResult<T>> {
        throw new Error(`Mobile check-in cannot handle ${method} ${url}.`);
    }

    /**
     * Posts to the block action that stands in for a check-in endpoint.
     *
     * @param url The check-in API url the flow is posting to.
     * @param _params Unused, since no mapped endpoint takes url parameters.
     * @param data The request body, forwarded to the action as its options.
     *
     * @returns The result of the block action.
     */
    async function post<T>(url: string, _params?: HttpUrlParams, data?: HttpBodyData): Promise<HttpResult<T>> {
        const actionName = blockActionNames[getEndpointName(url)];

        if (!actionName) {
            return unsupported<T>("POST", url);
        }

        const result = await invoke<T>(actionName, { options: data });

        if (result.isSuccess && actionName === "SaveAttendance") {
            callbacks?.onPendingAttendanceChanged?.(true);
        }
        else if (result.isSuccess && actionName === "ConfirmAttendance") {
            callbacks?.onPendingAttendanceChanged?.(false);
        }

        return result;
    }

    /**
     * Discards a check-in session's pending attendance, reading the session
     * guid off the end of the url since this endpoint carries no body.
     *
     * @param url The check-in API url the flow is deleting.
     *
     * @returns The result of the block action.
     */
    async function doDelete<T>(url: string): Promise<HttpResult<T>> {
        const segments = url.split("?")[0].split("/");
        const sessionGuid = segments.pop() ?? "";

        if (segments.pop()?.toLowerCase() !== pendingAttendanceEndpoint) {
            return unsupported<T>("DELETE", url);
        }

        return await invoke<T>("DeletePendingAttendance", { sessionGuid });
    }

    /**
     * Routes a check-in API call to the block action that answers it.
     *
     * @param method The HTTP method the flow is using.
     * @param url The check-in API url the flow is calling.
     * @param params The url parameters, which no mapped endpoint uses.
     * @param data The request body, where the method carries one.
     *
     * @returns The result of the block action.
     */
    async function doApiCall<T>(method: HttpMethod, url: string, params?: HttpUrlParams, data?: HttpBodyData): Promise<HttpResult<T>> {
        if (method === "POST") {
            return await post<T>(url, params, data);
        }

        if (method === "DELETE") {
            return await doDelete<T>(url);
        }

        return unsupported<T>(method, url);
    }

    return {
        doApiCall,

        doStreamingApiCall<T>(method: HttpMethod, url: string): Promise<HttpResult<ReadableStream<T>>> {
            return unsupported<ReadableStream<T>>(method, url);
        },

        get<T>(url: string): Promise<HttpResult<T>> {
            return unsupported<T>("GET", url);
        },

        post
    };
}

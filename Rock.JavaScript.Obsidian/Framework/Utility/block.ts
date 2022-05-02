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
import { HttpBodyData, HttpResult, HttpUrlParams } from "./http";
import { inject, ref, Ref } from "vue";
import { RockDateTime } from "./rockDateTime";

export type ConfigurationValues = Record<string, unknown>;

export type BlockConfig = {
    blockFileUrl: string;
    rootElement: Element;
    blockGuid: Guid;
    configurationValues: ConfigurationValues;
};

export type InvokeBlockActionFunc = <T>(actionName: string, data?: HttpBodyData) => Promise<HttpResult<T>>;

export type BlockHttpGet = <T>(url: string, params?: HttpUrlParams) => Promise<HttpResult<T>>;

export type BlockHttpPost = <T>(url: string, params?: HttpUrlParams, data?: HttpBodyData) => Promise<HttpResult<T>>;

export type BlockHttp = {
    get: BlockHttpGet;
    post: BlockHttpPost;
};



// TODO: Change these to use symbols

/**
 * Maps the block configuration values to the expected type.
 * 
 * @returns The configuration values for the block.
 */
export function useConfigurationValues<T>(): T {
    const result = inject<T>("configurationValues");

    if (result === undefined) {
        throw "Attempted to access block configuration outside of a RockBlock.";
    }

    return result;
}

/**
 * Gets the function that will be used to invoke block actions.
 *
 * @returns An instance of @see {@link InvokeBlockActionFunc}.
 */
export function useInvokeBlockAction(): InvokeBlockActionFunc {
    const result = inject<InvokeBlockActionFunc>("invokeBlockAction");

    if (result === undefined) {
        throw "Attempted to access block action invocation outside of a RockBlock.";
    }

    return result;
}

/**
 * Use a security grant token value provided by the server. This returns a reference
 * to the actual value and will automatically handle renewing the token and updating
 * the value.
 * 
 * @param token The token provided by the server.
 *
 * @returns A reference to the token that will be updated automatically when it has been renewed.
 */
export function useSecurityGrantToken(token: string | null | undefined): { token: Ref<string | null> } {
    // Use || so that an empty string gets converted to null.
    const tokenRef = ref(token || null);
    const invokeBlockAction = useInvokeBlockAction();

    // Internal function to renew the token and re-schedule renewal.
    const renewToken = async (): Promise<void> => {
        const result = await invokeBlockAction<string>("RenewSecurityGrantToken");

        if (result.isSuccess && result.data) {
            tokenRef.value = result.data;

            scheduleRenewal();
        }
    };

    // Internal function to schedule renewal based on the expiration date in
    // the existing token. Renewal happens 15 minutes before expiration.
    const scheduleRenewal = (): void => {
        // No token, nothing to do.
        if (tokenRef.value === null) {
            return;
        }

        const segments = tokenRef.value?.split(";");

        // Token not in expected format.
        if (segments.length !== 3 || segments[0] !== "1") {
            return;
        }

        const expiresDateTime = RockDateTime.parseISO(segments[1]);

        // Could not parse expiration date and time.
        if (expiresDateTime === null) {
            return;
        }

        const renewTimeout = expiresDateTime.addMinutes(-15).toMilliseconds() - RockDateTime.now().toMilliseconds();

        // Renewal request would be in the past, ignore.
        if (renewTimeout < 0) {
            return;
        }

        // Schedule the renewal task to happen 15 minutes before expiration.
        setTimeout(renewToken, renewTimeout);
    };

    scheduleRenewal();

    return {
        token: tokenRef
    };
}

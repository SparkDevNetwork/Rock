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
import { SecurityGrant } from "@Obsidian/Types/Utility/block";
import { ExtendedRef } from "@Obsidian/Types/Utility/component";
import { DetailBlockBox } from "@Obsidian/ViewModels/Blocks/detailBlockBox";
import { HttpBodyData, HttpResult, HttpUrlParams } from "./http";
import { inject, provide, Ref, ref, watch } from "vue";
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

// #region Security Grants

const securityGrantSymbol = Symbol();

/**
 * Use a security grant token value provided by the server. This returns a reference
 * to the actual value and will automatically handle renewing the token and updating
 * the value. This function is meant to be used by blocks. Controls should use the
 * useSecurityGrant() function instead.
 * 
 * @param token The token provided by the server.
 *
 * @returns A reference to the security grant that will be updated automatically when it has been renewed.
 */
export function getSecurityGrant(token: string | null | undefined): SecurityGrant {
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

/**
 * Provides the security grant to child components to use in their API calls.
 * 
 * @param grant The grant ot provide to child components.
 */
export function provideSecurityGrant(grant: SecurityGrant): void {
    provide(securityGrantSymbol, grant);
}

/**
 * Uses a previously provided security grant token by a parent component.
 * This function is meant to be used by controls that need to obtain a security
 * grant from a parent component.
 *
 * @returns A string reference that contains the security grant token.
 */
export function useSecurityGrantToken(): Ref<string | null> {
    const grant = inject<SecurityGrant>(securityGrantSymbol);

    return grant ? grant.token : ref(null);
}

// #endregion

// #region Extended References

/** An emit object that conforms to having a propertyChanged event. */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type PropertyChangedEmitFn<E extends "propertyChanged"> = E extends Array<infer EE> ? (event: EE, ...args: any[]) => void : (event: E, ...args: any[]) => void;

/**
 * Watches for changes to the given Ref objects and emits a special event to
 * indicate that a given property has changed.
 * 
 * @param propertyRefs The ExtendedRef objects to watch for changes.
 * @param emit The emit function for the component.
 */
export function watchPropertyChanges<E extends "propertyChanged">(propertyRefs: ExtendedRef<unknown>[], emit: PropertyChangedEmitFn<E>): void {
    for (const propRef of propertyRefs) {
        watch(propRef, () => {
            if (propRef.context.propertyName) {
                emit("propertyChanged", propRef.context.propertyName);
            }
        });
    }
}

/**
 * Requests an updated attribute list from the server based on the
 * current UI selections made.
 *
 * @param bag The entity bag that will be used to determine current property values
 * and then updated with the new attributes and values.
 * @param validProperties The properties that are considered valid on the bag when
 * the server will read the bag.
 * @param invokeBlockAction The function to use when calling the block action.
 */
export async function refreshDetailAttributes<TEntityBag>(bag: Ref<TEntityBag>, validProperties: string[], invokeBlockAction: InvokeBlockActionFunc): Promise<void> {
    const data: DetailBlockBox<unknown, unknown> = {
        entity: bag.value,
        isEditable: true,
        validProperties: validProperties
    };

    const result = await invokeBlockAction<DetailBlockBox<Record<string, unknown>, unknown>>("RefreshAttributes", {
        box: data
    });

    if (result.isSuccess) {
        if (result.statusCode === 200 && result.data && bag.value) {
            const newBag: TEntityBag = {
                ...bag.value,
                attributes: result.data.entity?.attributes,
                attributeValues: result.data.entity?.attributeValues
            };

            bag.value = newBag;
        }
    }
}

// #endregion Extended Refs

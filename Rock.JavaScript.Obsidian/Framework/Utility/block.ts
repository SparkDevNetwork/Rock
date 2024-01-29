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

import { BlockEvent, InvokeBlockActionFunc, SecurityGrant } from "@Obsidian/Types/Utility/block";
import { IBlockPersonPreferencesProvider, IPersonPreferenceCollection } from "@Obsidian/Types/Core/personPreferences";
import { ExtendedRef } from "@Obsidian/Types/Utility/component";
import { DetailBlockBox } from "@Obsidian/ViewModels/Blocks/detailBlockBox";
import { inject, provide, Ref, ref, watch } from "vue";
import { RockDateTime } from "./rockDateTime";
import { Guid } from "@Obsidian/Types";
import { HttpBodyData, HttpPostFunc, HttpResult } from "@Obsidian/Types/Utility/http";
import { BlockActionContextBag } from "@Obsidian/ViewModels/Blocks/blockActionContextBag";

const blockReloadSymbol = Symbol();
const configurationValuesChangedSymbol = Symbol();

// TODO: Change these to use symbols

/**
 * Maps the block configuration values to the expected type.
 *
 * @returns The configuration values for the block.
 */
export function useConfigurationValues<T>(): T {
    const result = inject<Ref<T>>("configurationValues");

    if (result === undefined) {
        throw "Attempted to access block configuration outside of a RockBlock.";
    }

    return result.value;
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
 * Creates a function that can be provided to the block that allows calling
 * block actions.
 *
 * @private This should not be used by plugins.
 *
 * @param post The function to handle the post operation.
 * @param pageGuid The unique identifier of the page.
 * @param blockGuid The unique identifier of the block.
 * @param pageParameters The parameters to include with the block action calls.
 *
 * @returns A function that can be used to provide the invoke block action.
 */
export function createInvokeBlockAction(post: HttpPostFunc, pageGuid: Guid, blockGuid: Guid, pageParameters: Record<string, string>): InvokeBlockActionFunc {
    async function invokeBlockAction<T>(actionName: string, data: HttpBodyData | undefined = undefined, actionContext: BlockActionContextBag | undefined = undefined): Promise<HttpResult<T>> {
        let context: BlockActionContextBag = {};

        if (actionContext) {
            context = {...actionContext};
        }

        context.pageParameters = pageParameters;

        return await post<T>(`/api/v2/BlockActions/${pageGuid}/${blockGuid}/${actionName}`, undefined, {
            __context: context,
            ...data
        });
    }

    return invokeBlockAction;
}

/**
 * Provides the reload block callback function for a block. This is an internal
 * method and should not be used by plugins.
 *
 * @param callback The callback that will be called when a block wants to reload itself.
 */
export function provideReloadBlock(callback: () => void): void {
    provide(blockReloadSymbol, callback);
}

/**
 * Gets a function that can be called when a block wants to reload itself.
 *
 * @returns A function that will cause the block component to be reloaded.
 */
export function useReloadBlock(): () => void {
    return inject<() => void>(blockReloadSymbol, () => {
        // Intentionally blank, do nothing by default.
    });
}

/**
 * Provides the data for a block to be notified when its configuration values
 * have changed. This is an internal method and should not be used by plugins.
 *
 * @returns An object with an invoke and reset function.
 */
export function provideConfigurationValuesChanged(): { invoke: () => void, reset: () => void } {
    const callbacks: (() => void)[] = [];

    provide(configurationValuesChangedSymbol, callbacks);

    return {
        invoke: (): void => {
            for (const c of callbacks) {
                c();
            }
        },

        reset: (): void => {
            callbacks.splice(0, callbacks.length);
        }
    };
}

/**
 * Registered a function to be called when the block configuration values have
 * changed.
 *
 * @param callback The function to be called when the configuration values have changed.
 */
export function onConfigurationValuesChanged(callback: () => void): void {
    const callbacks = inject<(() => void)[]>(configurationValuesChangedSymbol);

    if (callbacks !== undefined) {
        callbacks.push(callback);
    }
}


/**
 * A type that returns the keys of a child property.
 */
type ChildKeys<T extends Record<string, unknown>, PropertyName extends string> = keyof NonNullable<T[PropertyName]> & string;

/**
 * A valid properties box that uses the specified name for the content bag.
 */
type ValidPropertiesBox<PropertyName extends string> = {
    validProperties?: string[] | null;
} & {
        [P in PropertyName]?: Record<string, unknown> | null;
    };

/**
 * Sets the a value for a custom settings box. This will set the value and then
 * add the property name to the list of valid properties.
 *
 * @param box The box whose custom setting value will be set.
 * @param propertyName The name of the custom setting property to set.
 * @param value The new value of the custom setting.
 */
export function setCustomSettingsBoxValue<T extends ValidPropertiesBox<"settings">, S extends NonNullable<T["settings"]>, K extends ChildKeys<T, "settings">>(box: T, propertyName: K, value: S[K]): void {
    if (!box.settings) {
        box.settings = {} as Record<string, unknown>;
    }

    box.settings[propertyName] = value;

    if (!box.validProperties) {
        box.validProperties = [];
    }

    if (!box.validProperties.includes(propertyName)) {
        box.validProperties.push(propertyName);
    }
}

/**
 * Sets the a value for a property box. This will set the value and then
 * add the property name to the list of valid properties.
 *
 * @param box The box whose property value will be set.
 * @param propertyName The name of the property on the bag to set.
 * @param value The new value of the property.
 */
export function setPropertiesBoxValue<T extends ValidPropertiesBox<"bag">, S extends NonNullable<T["bag"]>, K extends ChildKeys<T, "bag">>(box: T, propertyName: K, value: S[K]): void {
    if (!box.bag) {
        box.bag = {} as Record<string, unknown>;
    }

    box.bag[propertyName] = value;

    if (!box.validProperties) {
        box.validProperties = [];
    }

    if (!box.validProperties.includes(propertyName)) {
        box.validProperties.push(propertyName);
    }
}

/**
 * Dispatches a block event to the document.
 *
 * @param eventName The name of the event to be dispatched.
 * @param eventData The custom data to be attached to the event.
 *
 * @returns true if preventDefault() was called on the event, otherwise false.
 */
export function dispatchBlockEvent(eventName: string, blockGuid: Guid, eventData?: unknown): boolean {
    const ev = new CustomEvent(eventName, {
        cancelable: true,
        detail: {
            guid: blockGuid,
            data: eventData
        }
    });

    return document.dispatchEvent(ev);
}

/**
 * Tests if the given event is a custom block event. This does not ensure
 * that the event data is the correct type, only the event itself.
 *
 * @param event The event to be tested.
 *
 * @returns true if the event is a block event.
 */
export function isBlockEvent<TData = undefined>(event: Event): event is CustomEvent<BlockEvent<TData>> {
    return "guid" in event && "data" in event;
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
    let renewalTimeout: NodeJS.Timeout | null = null;

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
        // Cancel any existing renewal timer.
        if (renewalTimeout !== null) {
            clearTimeout(renewalTimeout);
            renewalTimeout = null;
        }

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
        renewalTimeout = setTimeout(renewToken, renewTimeout);
    };

    scheduleRenewal();

    return {
        token: tokenRef,
        updateToken(newToken) {
            tokenRef.value = newToken || null;
            scheduleRenewal();
        }
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

// #region Block Guid

const blockGuidSymbol = Symbol("block-guid");

/**
 * Provides the block unique identifier to all child components.
 * This is an internal method and should not be used by plugins.
 *
 * @param blockGuid The unique identifier of the block.
 */
export function provideBlockGuid(blockGuid: string): void {
    provide(blockGuidSymbol, blockGuid);
}

/**
 * Gets the unique identifier of the current block in this component chain.
 *
 * @returns The unique identifier of the block.
 */
export function useBlockGuid(): Guid | undefined {
    return inject<Guid>(blockGuidSymbol);
}

// #endregion

// #region Person Preferences

const blockPreferenceProviderSymbol = Symbol();

/** An no-op implementation of {@link IPersonPreferenceCollection}. */
const emptyPreferences: IPersonPreferenceCollection = {
    getValue(): string {
        return "";
    },
    setValue(): void {
        // Intentionally empty.
    },
    getKeys(): string[] {
        return [];
    },
    containsKey(): boolean {
        return false;
    },
    save(): Promise<void> {
        return Promise.resolve();
    },
    withPrefix(): IPersonPreferenceCollection {
        return emptyPreferences;
    }
};

const emptyPreferenceProvider: IBlockPersonPreferencesProvider = {
    blockPreferences: emptyPreferences,
    getGlobalPreferences() {
        return Promise.resolve(emptyPreferences);
    },
    getEntityPreferences() {
        return Promise.resolve(emptyPreferences);
    }
};

/**
 * Provides the person preferences provider that will be used by components
 * to access the person preferences associated with their block.
 *
 * @private This is an internal method and should not be used by plugins.
 *
 * @param blockGuid The unique identifier of the block.
 */
export function providePersonPreferences(provider: IBlockPersonPreferencesProvider): void {
    provide(blockPreferenceProviderSymbol, provider);
}

/**
 * Gets the person preference provider that can be used to access block
 * preferences as well as other preferences.
 *
 * @returns An object that implements {@link IBlockPersonPreferencesProvider}.
 */
export function usePersonPreferences(): IBlockPersonPreferencesProvider {
    return inject<IBlockPersonPreferencesProvider>(blockPreferenceProviderSymbol)
        ?? emptyPreferenceProvider;
}

// #endregion

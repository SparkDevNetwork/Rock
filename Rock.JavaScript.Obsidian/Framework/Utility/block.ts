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
import { ValidPropertiesBox } from "@Obsidian/ViewModels/Utility/validPropertiesBox";
import { IEntity } from "@Obsidian/ViewModels/entity";
import { debounce } from "./util";
import { BrowserBus, useBrowserBus } from "./browserBus";

const blockReloadSymbol = Symbol();
const configurationValuesChangedSymbol = Symbol();
const staticContentSymbol = Symbol("static-content");
const blockBrowserBusSymbol = Symbol("block-browser-bus");

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
 * Gets the function that will return the URL for a block action.
 *
 * @returns A function that can be called to determine the URL for a block action.
 */
export function useBlockActionUrl(): (actionName: string) => string {
    const result = inject<(actionName: string) => string>("blockActionUrl");

    if (result === undefined) {
        throw "Attempted to access block action URL outside of a RockBlock.";
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
export function createInvokeBlockAction(post: HttpPostFunc, pageGuid: Guid, blockGuid: Guid, pageParameters: Record<string, string>, interactionGuid: Guid): InvokeBlockActionFunc {
    async function invokeBlockAction<T>(actionName: string, data: HttpBodyData | undefined = undefined, actionContext: BlockActionContextBag | undefined = undefined): Promise<HttpResult<T>> {
        let context: BlockActionContextBag = {};

        if (actionContext) {
            context = { ...actionContext };
        }

        context.pageParameters = pageParameters;
        context.interactionGuid = interactionGuid;

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
 * Provides the static content that the block provided on the server.
 *
 * @param content The static content from the server.
 */
export function provideStaticContent(content: Ref<Node[]>): void {
    provide(staticContentSymbol, content);
}

/**
 * Gets the static content that was provided by the block on the server.
 *
 * @returns A string of HTML content or undefined.
 */
export function useStaticContent(): Node[] {
    const content = inject<Ref<Node[]>>(staticContentSymbol);

    if (!content) {
        return [];
    }

    return content.value;
}

/**
 * Provides the browser bus configured to publish messages for the current
 * block.
 *
 * @param bus The browser bus.
 */
export function provideBlockBrowserBus(bus: BrowserBus): void {
    provide(blockBrowserBusSymbol, bus);
}

/**
 * Gets the browser bus configured for use by the current block. If available
 * this will be properly configured to publish messages with the correct block
 * and block type. If this is called outside the context of a block then a
 * generic use {@link BrowserBus} will be returned.
 *
 * @returns An instance of {@link BrowserBus}.
 */
export function useBlockBrowserBus(): BrowserBus {
    return inject<BrowserBus>(blockBrowserBusSymbol, () => useBrowserBus(), true);
}


/**
 * A type that returns the keys of a child property.
 */
type ChildKeys<T extends Record<string, unknown>, PropertyName extends string> = keyof NonNullable<T[PropertyName]> & string;

/**
 * A valid properties box that uses the specified name for the content bag.
 */
type ValidPropertiesSettingsBox = {
    validProperties?: string[] | null;
} & {
    settings?: Record<string, unknown> | null;
};

/**
 * Sets the a value for a custom settings box. This will set the value and then
 * add the property name to the list of valid properties.
 *
 * @param box The box whose custom setting value will be set.
 * @param propertyName The name of the custom setting property to set.
 * @param value The new value of the custom setting.
 */
export function setCustomSettingsBoxValue<T extends ValidPropertiesSettingsBox, S extends NonNullable<T["settings"]>, K extends ChildKeys<T, "settings">>(box: T, propertyName: K, value: S[K]): void {
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
export function setPropertiesBoxValue<T extends Record<string, unknown>, K extends keyof T & string>(box: ValidPropertiesBox<T>, propertyName: K, value: T[K]): void {
    if (!box.bag) {
        box.bag = {} as Record<string, unknown> as T;
    }

    box.bag[propertyName] = value;

    if (!box.validProperties) {
        box.validProperties = [];
    }

    if (!box.validProperties.some(p => p.toLowerCase() === propertyName.toLowerCase())) {
        box.validProperties.push(propertyName);
    }
}

/**
 * Dispatches a block event to the document.
 *
 * @deprecated Do not use this function anymore, it will be removed in the future.
 * Use the BrowserBus instead.
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

// #region Entity Detail Blocks

const entityTypeNameSymbol = Symbol("EntityTypeName");
const entityTypeGuidSymbol = Symbol("EntityTypeGuid");

type UseEntityDetailBlockOptions = {
    /** The block configuration. */
    blockConfig: Record<string, unknown>;

    /**
     * The entity that will be used by the block, this will cause the
     * onPropertyChanged logic to be generated.
     */
    entity?: Ref<ValidPropertiesBox<IEntity>>;
};

type UseEntityDetailBlockResult = {
    /** The onPropertyChanged handler for the edit panel. */
    onPropertyChanged?(propertyName: string): void;
};

/**
 * Performs any framework-level initialization of an entity detail block.
 *
 * @param options The options to use when initializing the detail block logic.
 *
 * @returns An object that contains information which can be used by the block.
 */
export function useEntityDetailBlock(options: UseEntityDetailBlockOptions): UseEntityDetailBlockResult {
    const securityGrant = getSecurityGrant(options.blockConfig.securityGrantToken as string);

    provideSecurityGrant(securityGrant);

    if (options.blockConfig.entityTypeName) {
        provideEntityTypeName(options.blockConfig.entityTypeName as string);
    }

    if (options.blockConfig.entityTypeGuid) {
        provideEntityTypeGuid(options.blockConfig.entityTypeGuid as Guid);
    }

    const entity = options.entity;

    const result: Record<string, unknown> = {};

    if (entity) {
        const invokeBlockAction = useInvokeBlockAction();
        const refreshAttributesDebounce = debounce(() => refreshEntityDetailAttributes(entity, invokeBlockAction), undefined, true);

        result.onPropertyChanged = (propertyName: string): void => {
            // If we don't have any qualified attribute properties or this property
            // is not one of them then do nothing.
            if (!options.blockConfig.qualifiedAttributeProperties || !(options.blockConfig.qualifiedAttributeProperties as string[]).some(n => n.toLowerCase() === propertyName.toLowerCase())) {
                return;
            }

            refreshAttributesDebounce();
        };
    }

    return result;
}

/**
 * Provides the entity type name to child components.
 *
 * @param name The entity type name in PascalCase, such as `GroupMember`.
 */
export function provideEntityTypeName(name: string): void {
    provide(entityTypeNameSymbol, name);
}

/**
 * Gets the entity type name provided from a parent component.
 *
 * @returns The entity type name in PascalCase, such as `GroupMember` or undefined.
 */
export function useEntityTypeName(): string | undefined {
    return inject<string | undefined>(entityTypeNameSymbol, undefined);
}

/**
 * Provides the entity type unique identifier to child components.
 *
 * @param guid The entity type unique identifier.
 */
export function provideEntityTypeGuid(guid: Guid): void {
    provide(entityTypeGuidSymbol, guid);
}

/**
 * Gets the entity type unique identifier provided from a parent component.
 *
 * @returns The entity type unique identifier or undefined.
 */
export function useEntityTypeGuid(): Guid | undefined {
    return inject<string | undefined>(entityTypeGuidSymbol, undefined);
}

// #endregion

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
 * @param box The valid properties box that will be used to determine current
 * property values and then updated with the new attributes and values.
 * @param invokeBlockAction The function to use when calling the block action.
 */
async function refreshEntityDetailAttributes<TEntityBag extends IEntity>(box: Ref<ValidPropertiesBox<TEntityBag>>, invokeBlockAction: InvokeBlockActionFunc): Promise<void> {
    const result = await invokeBlockAction<ValidPropertiesBox<TEntityBag>>("RefreshAttributes", {
        box: box.value
    });

    if (result.isSuccess) {
        if (result.statusCode === 200 && result.data && box.value) {
            const newBox: ValidPropertiesBox<TEntityBag> = {
                ...box.value,
                bag: {
                    ...box.value.bag as TEntityBag,
                    attributes: result.data.bag?.attributes,
                    attributeValues: result.data.bag?.attributeValues
                }
            };

            box.value = newBox;
        }
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

// #region Block and BlockType Guid

const blockGuidSymbol = Symbol("block-guid");
const blockTypeGuidSymbol = Symbol("block-type-guid");

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

/**
 * Provides the block type unique identifier to all child components.
 * This is an internal method and should not be used by plugins.
 *
 * @param blockTypeGuid The unique identifier of the block type.
 */
export function provideBlockTypeGuid(blockTypeGuid: string): void {
    provide(blockTypeGuidSymbol, blockTypeGuid);
}

/**
 * Gets the block type unique identifier of the current block in this component
 * chain.
 *
 * @returns The unique identifier of the block type.
 */
export function useBlockTypeGuid(): Guid | undefined {
    return inject<Guid>(blockTypeGuidSymbol);
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
    },
    on(): void {
        // Intentionally empty.
    },
    off(): void {
        // Intentionally empty.
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

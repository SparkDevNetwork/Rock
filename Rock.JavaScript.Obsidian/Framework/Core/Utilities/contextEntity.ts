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

import { areEqual } from "@Obsidian/Utility/guid";
import { useHttp } from "@Obsidian/Utility/http";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { useStore } from "@Obsidian/PageState";
import { useBlockBrowserBus } from "@Obsidian/Utility/block";
import { ContextEntityItemBag } from "@Obsidian/ViewModels/Cms/contextEntityItemBag";
import { PageMessages } from "@Obsidian/Utility/browserBus";
import { ContextEntityChangedData } from "@Obsidian/Types/Utility/browserBus";
import { Guid } from "@Obsidian/Types";

const http = useHttp();
const store = useStore();
const contextEntities = loadContextEntities();

/**
 * Loads the context entities from the server.
 *
 * @returns A promise that resolves to an array of context entity items.
 */
async function loadContextEntities(): Promise<ContextEntityItemBag[]> {
    const result = await http.get<ContextEntityItemBag[]>("/api/v2/utilities/ContextEntities", undefined);

    if (result.isSuccess && result.data) {
        return result.data;
    }
    else {
        console.error("Failed to load context entities", result.errorMessage);
        return [];
    }
}

/**
 * Provides functions for accessing and setting context entities.
 */
class ContextEntityProvider {
    private bus = useBlockBrowserBus();

    /**
     * Gets the names of all context entity types, which are the full C# class name.
     *
     * @returns A promise that resolves to an array of context entity type names.
     */
    public async getTypeNames(): Promise<string[]> {
        const items = await contextEntities;

        return items
            .filter(item => !!item.entityType)
            .map(item => item.entityType!);
    }

    /**
     * Gets the unique identifiers of all context entity types.
     *
     * @returns A promise that resolves to an array of context entity type unique identifiers.
     */
    public async getTypeGuids(): Promise<string[]> {
        const items = await contextEntities;

        return items.map(item => item.entityTypeGuid);
    }

    /**
     * Gets a context entity by its type. This can be either the full class name or
     * the unique identifier.
     *
     * @param entityType The type of the context entity.
     *
     * @returns A promise that resolves to the context entity or undefined if not found.
     */
    public async get(entityType: string | Guid): Promise<ListItemBag | undefined> {
        const items = await contextEntities;

        const item = items.find(item => item.entityType === entityType || areEqual(item.entityTypeGuid, entityType));

        if (!item) {
            return undefined;
        }

        return {
            value: item.entityGuid,
            text: item.entityName
        };
    }

    /**
     * Sets a context entity for the current session. If the context is null or
     * undefined, the context entity will be removed.
     *
     * @param entityType The type of the context entity as either the full class name or the unique identifier.
     * @param context The context entity to set. Only the `value` property is required.
     * @param pageSpecific Whether the context entity is page-specific.
     *
     * @returns A promise that resolves when the context entity is set.
     */
    public async set(entityType: string | Guid, context: ListItemBag | null | undefined, pageSpecific: boolean = false): Promise<void> {
        if (!context) {
            return this.remove(entityType, pageSpecific);
        }

        const items = await contextEntities;
        const url = `/api/v2/utilities/ContextEntities/${entityType}/${context.value}`;
        const parameters = {
            pageKey: pageSpecific ? store.state.pageGuid : undefined
        };

        const result = await http.post<ContextEntityItemBag>(url, parameters);

        if (result.isSuccess && result.data) {
            const oldItemIndex = items.findIndex(item => item.entityType === entityType || areEqual(item.entityTypeGuid, entityType));

            if (oldItemIndex !== -1) {
                items[oldItemIndex] = result.data;
            }
            else {
                items.push(result.data);
            }

            this.bus.publish(PageMessages.ContextEntityChanged, <ContextEntityChangedData>{
                entityType: result.data.entityType,
                entityTypeGuid: result.data.entityTypeGuid
            });
        }
        else {
            console.error("Failed to set context entity", result.errorMessage);
        }
    }

    /**
     * Removes a context entity for the current session.
     *
     * @param entityType The type of the context entity as either the full class name or the unique identifier.
     * @param pageSpecific Whether the context entity is page-specific.
     */
    public async remove(entityType: string | Guid, pageSpecific: boolean = false): Promise<void> {
        const items = await contextEntities;
        const url = `/api/v2/utilities/ContextEntities/${entityType}`;
        const parameters = {
            pageKey: pageSpecific ? store.state.pageGuid : undefined
        };

        const result = await http.doApiCall("DELETE", url, parameters);

        if (result.isSuccess) {
            const oldItemIndex = items.findIndex(item => item.entityType === entityType || areEqual(item.entityTypeGuid, entityType));

            if (oldItemIndex !== -1) {
                const oldItem = items[oldItemIndex];
                items.splice(oldItemIndex, 1);

                this.bus.publish(PageMessages.ContextEntityChanged, <ContextEntityChangedData>{
                    entityType: oldItem.entityType,
                    entityTypeGuid: oldItem.entityTypeGuid
                });
            }
        }
        else {
            console.error("Failed to remove context entity", result.errorMessage);
        }
    }
}

/**
 * Provides functions for accessing and setting context entities.
 *
 * @returns An object with functions for accessing and setting context entities.
 */
export function useContextEntities(): ContextEntityProvider {
    return new ContextEntityProvider();
}

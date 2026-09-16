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

import { HttpResult } from "@Obsidian/Types/Utility/http";
import { ContentChannelItemBag } from "@Obsidian/ViewModels/Blocks/Cms/ContentChannelItemDetail/contentChannelItemBag";
import { DeleteSlugRequestBag } from "@Obsidian/ViewModels/Blocks/Cms/ContentChannelItemDetail/deleteSlugRequestBag";
import { GetUniqueSlugRequestBag } from "@Obsidian/ViewModels/Blocks/Cms/ContentChannelItemDetail/getUniqueSlugRequestBag";
import { SaveSlugRequestBag } from "@Obsidian/ViewModels/Blocks/Cms/ContentChannelItemDetail/saveSlugRequestBag";
import { SaveSlugResponseBag } from "@Obsidian/ViewModels/Blocks/Cms/ContentChannelItemDetail/saveSlugResponseBag";
import { GridDataBag } from "@Obsidian/ViewModels/Core/Grid/gridDataBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ValidPropertiesBox } from "@Obsidian/ViewModels/Utility/validPropertiesBox";

/**
 * Keys for navigation URLs provided by the block's server-side configuration.
 */
export const enum NavigationUrlKey {
    /**
     * URL for the parent page of this block.
     */
    ParentPage = "ParentPage"
}

/**
 * Determines how a child content channel item is added.
 */
export const enum AddChildMode {
    /**
     * Create and add a brand-new child item.
     */
    New = "new",

    /**
     * Select and add an existing item as a child.
     */
    Existing = "existing"
}

/**
 * Response from the GetUniqueSlug block action: a channel-unique slug candidate.
 */
export type GetUniqueSlugResponse = {
    slug?: string | null;
};

/**
 * Response from the RefreshItemGlobalKey block action: a regenerated Item Global Key candidate.
 */
export type RefreshItemGlobalKeyResponse = {
    itemGlobalKey?: string | null;
};

/**
 * Typed invoker for the Content Channel Item Detail block's `[BlockAction]`
 * methods. Each method wraps one server action with its request shape and
 * response type so call sites stay free of magic strings and casts.
 *
 * @see useInvokeContentChannelItemDetailBlockAction
 */
export type ContentChannelItemDetailBlockActionInvoker = {
    /** Enters edit mode by fetching the editable bag for the given item. */
    edit(key: string | null | undefined): Promise<HttpResult<ValidPropertiesBox<ContentChannelItemBag>>>;

    /** Persists the edit bag. Response data is a redirect URL (string) on create, otherwise the saved box. */
    save(box: ValidPropertiesBox<ContentChannelItemBag>): Promise<HttpResult<ValidPropertiesBox<ContentChannelItemBag> | string>>;

    /** Deletes the item. Response data is the redirect URL. */
    delete(key: string): Promise<HttpResult<string>>;

    /** Persists a slug immediately for an existing item. Returns the saved Id and normalized slug. */
    saveSlug(request: SaveSlugRequestBag): Promise<HttpResult<SaveSlugResponseBag>>;

    /** Deletes a persisted slug for an existing item. No response body. */
    deleteSlug(request: DeleteSlugRequestBag): Promise<HttpResult<void>>;

    /** Returns a channel-unique slug candidate without persisting it. */
    getUniqueSlug(request: GetUniqueSlugRequestBag): Promise<HttpResult<GetUniqueSlugResponse>>;

    /** Returns a regenerated Item Global Key candidate from the title without persisting it. */
    refreshItemGlobalKey(key: string, title: string): Promise<HttpResult<RefreshItemGlobalKeyResponse>>;

    /** Re-downloads the item from the Content Library and returns the refreshed box. */
    redownload(key: string): Promise<HttpResult<ValidPropertiesBox<ContentChannelItemBag>>>;

    /** Loads the child-items grid rows for the given item. */
    getChildItemsGridData(key: string): Promise<HttpResult<GridDataBag>>;

    /** Loads the read-only parent-items grid rows for the given item. */
    getParentItemsGridData(key: string): Promise<HttpResult<GridDataBag>>;

    /** Links an existing item as a child. Response data is a status string. */
    addExistingChildItem(key: string, childItemKey: string): Promise<HttpResult<string>>;

    /** Unlinks a child without deleting the item. Response data is a status string. */
    removeChildAssociation(key: string, childItemKey: string): Promise<HttpResult<string>>;

    /** Hard-deletes a child item. Response data is a status string. */
    deleteChildItem(key: string, childItemKey: string): Promise<HttpResult<string>>;

    /** Reorders a child item before the given sibling (null appends last). */
    reorderChildItem(key: string, childItemKey: string, beforeChildItemKey: string | null): Promise<HttpResult<string>>;

    /** Loads the eligible existing-child options for the picked channel. */
    getAddChildItemOptions(key: string, channelKey: string): Promise<HttpResult<ListItemBag[]>>;

    /** Resolves the drill URL for a clicked related row. Response data is the navigation URL. */
    navigateToRelatedItem(key: string, selectedItemKey: string): Promise<HttpResult<string>>;

    /** Resolves the Add URL for a new child in the picked channel. Response data is the navigation URL. */
    navigateToNewChildItem(key: string, channelKey: string): Promise<HttpResult<string>>;
};

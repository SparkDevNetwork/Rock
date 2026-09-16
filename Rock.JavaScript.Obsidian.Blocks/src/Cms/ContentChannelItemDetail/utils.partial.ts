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

import { useInvokeBlockAction } from "@Obsidian/Utility/block";
import { ContentChannelItemDetailBlockActionInvoker } from "./types.partial";

/**
 * Creates a typed invoker for the Content Channel Item Detail block's
 * `[BlockAction]` methods. Wraps `useInvokeBlockAction` so call sites stay free
 * of magic strings, request-shape boilerplate, and response-type casts.
 *
 * @see ContentChannelItemDetailBlockActionInvoker for the contract each method exposes.
 */
export function useInvokeContentChannelItemDetailBlockAction(): ContentChannelItemDetailBlockActionInvoker {
    const invokeBlockAction = useInvokeBlockAction();

    return {
        edit(key) {
            return invokeBlockAction("Edit", { key });
        },
        save(box) {
            return invokeBlockAction("Save", { box });
        },
        delete(key) {
            return invokeBlockAction("Delete", { key });
        },
        saveSlug(request) {
            return invokeBlockAction("SaveSlug", { request });
        },
        deleteSlug(request) {
            return invokeBlockAction("DeleteSlug", { request });
        },
        getUniqueSlug(request) {
            return invokeBlockAction("GetUniqueSlug", { request });
        },
        refreshItemGlobalKey(key, title) {
            return invokeBlockAction("RefreshItemGlobalKey", { key, title });
        },
        redownload(key) {
            return invokeBlockAction("Redownload", { key });
        },
        getChildItemsGridData(key) {
            return invokeBlockAction("GetChildItemsGridData", { key });
        },
        getParentItemsGridData(key) {
            return invokeBlockAction("GetParentItemsGridData", { key });
        },
        addExistingChildItem(key, childItemKey) {
            return invokeBlockAction("AddExistingChildItem", { key, childItemKey });
        },
        removeChildAssociation(key, childItemKey) {
            return invokeBlockAction("RemoveChildAssociation", { key, childItemKey });
        },
        deleteChildItem(key, childItemKey) {
            return invokeBlockAction("DeleteChildItem", { key, childItemKey });
        },
        reorderChildItem(key, childItemKey, beforeChildItemKey) {
            return invokeBlockAction("ReorderChildItem", { key, childItemKey, beforeChildItemKey });
        },
        getAddChildItemOptions(key, channelKey) {
            return invokeBlockAction("GetAddChildItemOptions", { key, channelKey });
        },
        navigateToRelatedItem(key, selectedItemKey) {
            return invokeBlockAction("NavigateToRelatedItem", { key, selectedItemKey });
        },
        navigateToNewChildItem(key, channelKey) {
            return invokeBlockAction("NavigateToNewChildItem", { key, channelKey });
        }
    };
}

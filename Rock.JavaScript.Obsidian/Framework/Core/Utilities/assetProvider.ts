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

/**
 * This file contains a group of utility functions for traversing a tree of Asset Providers and
 * their folders. The objects in the tree are TreeItemBags with the folder's/provider's name as the
 * `text` property and the `value` property is in the form `${assetProviderId},${folderPath}(,True)?`.
 * If it's the root of an asset provider, it has the ",True" part appended to the end, otherwise it
 * ends after the folder path. Also, if an asset provider doesn't have a Root Folder specified in
 * it's configuration, it's key will just be a numeric ID, followed by 2 commas, and "True", e.g.
 * "3,,True". If a root folder is specified, though, it will show, e.g. "3,Assets/,True".
 *
 * A folder of an asset provider will not have the ",True" prefix, but will include the entire path
 * from the root to its folder name. Below is a sample simplified JSON structure demonstrating what
 * this looks like:
 *
[{
    "value": "1,~/Content/,True",                          // ~/Content is the specified root folder
    "text": "Local Content",
    "children": [{
        "value": "1,~/Content/ExternalSite/",
        "text": "ExternalSite",
        "children": [
            {
                "value": "1,~/Content/ExternalSite/Icons/",
                "text": "Icons",
                "children": null
            }
        ]
    },
    {
        "value": "1,~/Content/InternalSite/",
        "text": "InternalSite",
        "children": null
    }]
},
{
    "value": "3,,True",                                                         // No Root specified
    "text": "Sample AWS S3 Bucket",
    "children": [{
        "value": "3,Test/",
        "text": "Test",
        "children": null
    }]
}]
 */

import { TreeItemBag } from "@Obsidian/ViewModels/Utility/treeItemBag";

const assetProviderSuffixRegex = /,True$/;

/**
 * Search through a tree of TreeItemBags for a folder with the specified key (the `value` property).
 * When it's found, return an object containing:
 * 1. The TreeItemBag of the folder
 * 2. The TreeItemBag of the parent folder
 * 3. The index of the folder in the parent's `children` array
 *
 * By returning all 3 of these, it allows this function to be used for multiple applications: find
 * a folder in order to insert a new child into it, find the parent and index in order to be able to
 * delete the folder, etc.
 *
 * @param {Array<TreeItemBag>} tree The tree to search through.
 * @param {string} searchName The key of the folder to find.
 *
 * @returns {Object} An object containing the TreeItemBag of the folder, the TreeItemBag of the parent, and the index of the folder in the parent's `children` array.
 */
export function findFolder(tree: Array<TreeItemBag>, searchName: string | null, parent: TreeItemBag | null = null): { folder: TreeItemBag | null, parent: TreeItemBag | null, index: number } {
    if (!searchName || !searchName.trim()) {
        return { folder: null, parent: null, index: -1 };
    }

    let folder: TreeItemBag | null = null;
    let index = -1;

    const trueSearchName = searchName.replace(assetProviderSuffixRegex, "");

    for (const item of tree) {
        index++;

        const itemName = item.value?.replace(assetProviderSuffixRegex, "") as string;

        // If this is the item we're searching for, then return all the information we have
        if (itemName == trueSearchName) {
            folder = item;
            return { folder, parent, index };
        }

        if (trueSearchName.startsWith(itemName)) {
            // This item is a parent of the item we're searching for, so keep searching deeper
            const result = findFolder(item.children ?? [], trueSearchName, item);

            if (result.folder) {
                return result;
            }
        }
    }

    // If we got here, then the folder wasn't found, so give a "blank" value
    return { folder: null, parent: null, index: -1 };
}

/**
 * Comparator function for sorting an array of TreeItemBags by the folder name
 * in a case insensitive manner.
 */
export function folderNameComparator(a: TreeItemBag, b: TreeItemBag): number {
    if (a.text!.toLocaleLowerCase() < b.text!.toLocaleLowerCase()) {
        return -1;
    }
    else if (a.text!.toLocaleLowerCase() > b.text!.toLocaleLowerCase()) {
        return 1;
    }
    else if (a < b) {
        return -1;
    }
    else if (a > b) {
        return 1;
    }
    return 0;
}

/**
 * Verify that a folder name is valid.
 */
export function isValidFolderName(name: string): boolean {
    return /^[^*/><?\\\\|:,~]+$/.test(name) && name.trim() !== "";
}
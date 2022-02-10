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

import { TreeItem } from "../treeItem";

/**
 * The methods that must be implemented by tree item providers. These methods
 * provide the TreeItem objects to be displayed when lazy loading is being used.
 */
export interface ITreeItemProvider {
    /**
     * Get the root items to be displayed in the tree list.
     *
     * @returns A collection of TreeItem objects, optionally wrapped in a Promise
     * if the loading is being performed asynchronously.
     */
    getRootItems(): Promise<TreeItem[]> | TreeItem[];

    /**
     * Get the child items of the given tree item.
     * 
     * @param item The parent item whose children should be loaded.
     *
     * @returns A collection of TreeItem objects, optionally wrapped in a Promise
     * if the loading is being performed asynchronously.
     */
    getChildItems(item: TreeItem): Promise<TreeItem[]> | TreeItem[];
}

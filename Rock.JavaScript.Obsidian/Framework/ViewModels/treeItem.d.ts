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

export type TreeItem = {
    /**
     * The generic identifier of this item.
     */
    value: string | null;

    /**
     * The text that should be displayed to identify this item.
     */
    text: string | null;

    /**
     * A value indicating whether this instance is a folder.
     * A folder is an item that is intended to hold child items. This is a
     * distinction from the hasChildren propertyu which specifies if this item
     * currently has children or not.
     */
    isFolder: boolean;

    /**
     * True if this instance has children; otherwise, false.
     */
    hasChildren: boolean;

    /**
     * The icon CSS class.
     */
    iconCssClass: string | null;

    /**
     * True if this instance is active; otherwise, false.
     */
    isActive: boolean;

    /**
     * The child tree items of this item. A value of null indicates that the
     * children should be lazy loaded by the caller.
     */
    children: TreeItem[] | null;
};

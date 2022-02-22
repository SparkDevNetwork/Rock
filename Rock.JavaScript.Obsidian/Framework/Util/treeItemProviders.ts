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

import { Guid } from "../Util/guid";
import { post } from "../Util/http";
import { ITreeItemProvider } from "../ViewModels/Controls/treeList";
import { TreeItem } from "../ViewModels/treeItem";

type ChildTreeItemOptions = {
    /**
     * The parent unique identifier whose children are to be retrieved. If
     * undefined or null then the root items are being requested.
     */
    parentGuid?: Guid | null;

    /**
     * True if items should be loaded; False if just categories should be
     * loaded.
     */
    getCategorizedItems?: boolean;

    /**
     * The entity type unique identifier to limit the results to.
     */
    entityTypeGuid?: Guid | null;

    /**
     * The entity qualifier that is used to filter category results. If set
     * then the category EntityTypeQualifierColumn must match this value.
     */
    entityTypeQualifierColumn?: string | null;

    /**
     * The entity qualifier value that is used to filter category results. If
     * both this and entityTypeQualifierColumn are not blank then the category
     * EntityTypeQualifierValue property must match this value.
     */
    entityTypeQualifierValue?: string | null;

    /**
     * Indicates whether entity items without a name should be included in the
     * results. Only applies if getCategorizedItems is true.
     */
    includeUnnamedentityItems?: boolean;

    /**
     * Indicates whether categories that have no child categories and no items
     * should be included.
     */
    includeCategoriesWithoutChildren?: boolean;

    /**
     * The default icon CSS class to use for items that do not specify their
     * own IconCssClass value.
     */
    defaultIconCssClass?: string | null;

    /**
     * Indicates whether inactive items should be included in the results. If
     * the entity type does not support the IsActive property then this value
     * wil be ignored.
     */
    includeInactiveItems?: boolean;

    /**
     * Indicates whether child categories and items are loaded automatically.
     * If true then all descendant categories will be loaded along with the
     * items if getCategorizedItems is also true. This results in the children
     * property of the results being null to indicate they must be loaded
     * on demand.
     */
    lazyLoad?: boolean;
};

/**
 * Tree Item Provider for retrieving categories from the server and displaying
 * them inside a tree list.
 */
export class CategoryTreeItemProvider implements ITreeItemProvider {
    /**
     * The root category to start pulling categories from. Set to undefined to
     * begin with any category that does not have a parent.
     */
    public rootCategoryGuid?: Guid;

    /**
     * The entity type unique identifier to restrict results to. Set to undefined
     * to include all categories, regardless of entity type.
     */
    public entityTypeGuid?: Guid;

    /**
     * The value that must match in the category EntityTypeQualifierColumn
     * property. Set to undefined or an empty string to ignore.
     */
    public entityTypeQualifierColumn?: string;

    /**
     * The value that must match in the category EntityTypeQualifierValue
     * property.
     */
    public entityTypeQualifierValue?: string;

    /**
     * Gets the child items from the server.
     * 
     * @param parentGuid The parent item whose children are retrieved.
     *
     * @returns A collection of TreeItem objects as an asynchronous operation.
     */
    private async getItems(parentGuid?: Guid | null): Promise<TreeItem[]> {
        const options: ChildTreeItemOptions = {
            parentGuid: parentGuid,
            entityTypeGuid: this.entityTypeGuid,
            entityTypeQualifierColumn: this.entityTypeQualifierColumn,
            entityTypeQualifierValue: this.entityTypeQualifierValue,
            lazyLoad: false
        };

        const response = await post<TreeItem[]>("/api/v2/Controls/CategoryPicker/childTreeItems", {}, options);

        if (response.isSuccess && response.data) {
            return response.data;
        }
        else {
            console.log("Error", response.errorMessage);
            return [];
        }
    }

    /**
     * @inheritdoc
     */
    async getRootItems(): Promise<TreeItem[]> {
        return await this.getItems(this.rootCategoryGuid);
    }

    /**
     * @inheritdoc
     */
    async getChildItems(item: TreeItem): Promise<TreeItem[]> {
        return this.getItems(item.value);
    }
}


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
import { emptyGuid, toGuidOrNull } from "./guid";
import { useHttp } from "./http";
import { SiteType } from "@Obsidian/Enums/Cms/siteType";
import { TreeItemBag } from "@Obsidian/ViewModels/Utility/treeItemBag";
import { CategoryPickerChildTreeItemsOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/categoryPickerChildTreeItemsOptionsBag";
import { AccountPickerGetChildrenOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/accountPickerGetChildrenOptionsBag";
import { LocationItemPickerGetActiveChildrenOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/locationItemPickerGetActiveChildrenOptionsBag";
import { DataViewPickerGetDataViewsOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/dataViewPickerGetDataViewsOptionsBag";
import { WorkflowTypePickerGetWorkflowTypesOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/workflowTypePickerGetWorkflowTypesOptionsBag";
import { PagePickerGetChildrenOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/pagePickerGetChildrenOptionsBag";
import { PagePickerGetSelectedPageHierarchyOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/pagePickerGetSelectedPageHierarchyOptionsBag";
import { ConnectionRequestPickerGetChildrenOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/connectionRequestPickerGetChildrenOptionsBag";
import { GroupPickerGetChildrenOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/groupPickerGetChildrenOptionsBag";
import { MergeTemplatePickerGetMergeTemplatesOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/mergeTemplatePickerGetMergeTemplatesOptionsBag";
import { MergeTemplateOwnership } from "@Obsidian/Enums/Controls/mergeTemplateOwnership";
import { MetricCategoryPickerGetChildrenOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/metricCategoryPickerGetChildrenOptionsBag";
import { MetricItemPickerGetChildrenOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/metricItemPickerGetChildrenOptionsBag";
import { RegistrationTemplatePickerGetChildrenOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/registrationTemplatePickerGetChildrenOptionsBag";
import { ReportPickerGetChildrenOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/reportPickerGetChildrenOptionsBag";
import { SchedulePickerGetChildrenOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/schedulePickerGetChildrenOptionsBag";
import { WorkflowActionTypePickerGetChildrenOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/workflowActionTypePickerGetChildrenOptionsBag";
import { MergeFieldPickerGetChildrenOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/mergeFieldPickerGetChildrenOptionsBag";
import { AssetManagerGetRootFoldersOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/assetManagerGetRootFoldersOptionsBag";
import { AssetManagerBaseOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/assetManagerBaseOptionsBag";
import { UniversalItemTreePickerOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/UniversalItemTreePickerOptionsBag";
import { flatten } from "./arrayUtils";
import { toNumberOrNull } from "./numberUtils";

/**
 * The methods that must be implemented by tree item providers. These methods
 * provide the TreeItem objects to be displayed when lazy loading is being used.
 */
export interface ITreeItemProvider {
    /**
     * Get the root items to be displayed in the tree list.
     *
     * @param expandToValues The values that should be auto-expanded to. This will contain
     * the nodes that should be visible when the data is returned, they should not be
     * expanded themselves, only any ancestor nodes.
     *
     * @returns A collection of TreeItem objects, optionally wrapped in a Promise
     * if the loading is being performed asynchronously.
     */
    getRootItems(expandToValues: string[]): Promise<TreeItemBag[]> | TreeItemBag[];

    /**
     * Get the child items of the given tree item.
     *
     * @param item The parent item whose children should be loaded.
     *
     * @returns A collection of TreeItem objects, optionally wrapped in a Promise
     * if the loading is being performed asynchronously.
     */
    getChildItems(item: TreeItemBag): Promise<TreeItemBag[]> | TreeItemBag[];

    /**
     * Checks if the item can be selected by the individual. This function
     * is optional.
     *
     * @param item The item that is about to be selected.
     * @param isSelectable True if the tree view considers the item selectable.
     *
     * @returns A boolean that determines the final selectable state of the item.
     */
    canSelectItem?(item: TreeItemBag, isSelectable: boolean): boolean;
}

/**
 * Tree Item Provider for retrieving categories from the server and displaying
 * them inside a tree list.
 */
export class CategoryTreeItemProvider implements ITreeItemProvider {
    /** The HTTP client for making API requests. */
    private readonly http = useHttp();

    /**
     * Whether to include the categorized entity items beneath each category.
     * Set to undefined or false to show categories only.
     */
    public getCategorizedItems?: boolean;

    /**
     * Whether to include inactive items. Set to undefined or false to hide them.
     */
    public includeInactiveItems?: boolean;

    /**
     * Whether to include entity items that have a blank name. Set to undefined
     * or false to hide them.
     */
    public includeUnnamedEntityItems?: boolean;

    /**
     * The categories to exclude from the tree, by Guid. Set to undefined or
     * empty to exclude nothing.
     */
    public excludedCategoryGuids?: Guid[];

    /**
     * The icon CSS class applied to any item that has no icon of its own. Set
     * to undefined to leave items without an icon.
     */
    public defaultIconCssClass?: string;

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
     * The security grant token that will be used to request additional access
     * to the category list.
     */
    public securityGrantToken?: string | null;

    /**
     * Gets the child items from the server.
     *
     * @param parentGuid The parent item whose children are retrieved.
     *
     * @returns A collection of TreeItem objects as an asynchronous operation.
     */
    private async getItems(parentGuid?: Guid | null): Promise<TreeItemBag[]> {
        const options: CategoryPickerChildTreeItemsOptionsBag = {
            parentGuid: parentGuid,
            entityTypeGuid: this.entityTypeGuid,
            entityTypeQualifierColumn: this.entityTypeQualifierColumn,
            entityTypeQualifierValue: this.entityTypeQualifierValue,
            lazyLoad: false,
            securityGrantToken: this.securityGrantToken,

            getCategorizedItems: this.getCategorizedItems ?? false,
            includeCategoriesWithoutChildren: true,
            includeInactiveItems: this.includeInactiveItems ?? false,
            includeUnnamedEntityItems: this.includeUnnamedEntityItems ?? false,
        };

        const response = await this.http.post<TreeItemBag[]>("/api/v2/Controls/CategoryPickerChildTreeItems", {}, options);

        if (response.isSuccess && response.data) {
            return this.applyClientOptions(response.data);
        }
        else {
            console.log("Error", response.errorMessage);
            return [];
        }
    }

    /**
     * Applies the exclude-categories and default-icon options the server endpoint
     * does not handle: drops excluded category nodes and fills in a fallback icon.
     *
     * @param items The items returned from the server.
     *
     * @returns The filtered and decorated items.
     */
    private applyClientOptions(items: TreeItemBag[]): TreeItemBag[] {
        const excludedSet = new Set((this.excludedCategoryGuids ?? []).map(guid => guid.toLowerCase()));

        const filtered = excludedSet.size > 0
            ? items.filter(item => !item.value || !excludedSet.has(item.value.toLowerCase()))
            : items;

        for (const item of filtered) {
            if (this.defaultIconCssClass && !item.iconCssClass) {
                item.iconCssClass = this.defaultIconCssClass;
            }

            if (item.children && item.children.length > 0) {
                item.children = this.applyClientOptions(item.children);
            }
        }

        return filtered;
    }

    /**
     * @inheritdoc
     */
    async getRootItems(): Promise<TreeItemBag[]> {
        return await this.getItems(this.rootCategoryGuid);
    }

    /**
     * @inheritdoc
     */
    async getChildItems(item: TreeItemBag): Promise<TreeItemBag[]> {
        return this.getItems(item.value);
    }
}

/**
 * Tree Item Provider for retrieving locations from the server and displaying
 * them inside a tree list.
 */
export class LocationTreeItemProvider implements ITreeItemProvider {
    /** The HTTP client for making API requests. */
    private readonly http = useHttp();

    /**
     * The security grant token that will be used to request additional access
     * to the category list.
     */
    public securityGrantToken?: string | null;

    /**
     * The root location to restrict the tree to.
     */
    public rootLocationGuid?: Guid | null;

    /**
     * Determines if inactive locations should be included in the results.
     */
    public includeInactive: boolean = false;

    /**
     * Gets the child items from the server.
     *
     * @param parentGuid The parent item whose children are retrieved.
     * @param expandToValues The values that should be auto-expanded to.
     *
     * @returns A collection of TreeItem objects as an asynchronous operation.
     */
    private async getItems(parentGuid?: Guid | null, expandToValues?: string[]): Promise<TreeItemBag[]> {
        const options: LocationItemPickerGetActiveChildrenOptionsBag = {
            guid: toGuidOrNull(parentGuid) ?? emptyGuid,
            rootLocationGuid: this.rootLocationGuid ?? emptyGuid,
            expandToValues: expandToValues,
            securityGrantToken: this.securityGrantToken,
            includeInactive: this.includeInactive,
        };
        const url = "/api/v2/Controls/LocationItemPickerGetActiveChildren";
        const response = await this.http.post<TreeItemBag[]>(url, undefined, options);

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
    async getRootItems(expandToValues: string[]): Promise<TreeItemBag[]> {
        return await this.getItems(null, expandToValues);
    }

    /**
     * @inheritdoc
     */
    async getChildItems(item: TreeItemBag): Promise<TreeItemBag[]> {
        return this.getItems(item.value);
    }
}

/**
 * Tree Item Provider for retrieving data views from the server and displaying
 * them inside a tree list.
 */
export class DataViewTreeItemProvider implements ITreeItemProvider {
    /** The HTTP client for making API requests. */
    private readonly http = useHttp();

    /**
     * The entity type unique identifier to restrict results to. Set to undefined
     * to include all categories, regardless of entity type.
     */
    public entityTypeGuid?: Guid;

    /**
     * The security grant token that will be used to request additional access
     * to the category list.
     */
    public securityGrantToken?: string | null;

    /**
     * The flag sets whether only persisted data view should be shown by the picker
     */
    public displayPersistedOnly: boolean = false;

    /**
     * Gets the child items from the server.
     *
     * @param parentGuid The parent item whose children are retrieved.
     *
     * @returns A collection of TreeItem objects as an asynchronous operation.
     */
    private async getItems(parentGuid?: Guid | null): Promise<TreeItemBag[]> {
        const options: DataViewPickerGetDataViewsOptionsBag = {
            parentGuid,
            getCategorizedItems: true,
            includeCategoriesWithoutChildren: false,
            entityTypeGuidFilter: this.entityTypeGuid,
            lazyLoad: false,
            securityGrantToken: this.securityGrantToken,
            displayPersistedOnly: this.displayPersistedOnly,
            includeUnnamedEntityItems: false,
        };

        const response = await this.http.post<TreeItemBag[]>("/api/v2/Controls/DataViewPickerGetDataViews", {}, options);

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
    async getRootItems(): Promise<TreeItemBag[]> {
        return await this.getItems();
    }

    /**
     * @inheritdoc
     */
    async getChildItems(item: TreeItemBag): Promise<TreeItemBag[]> {
        return this.getItems(item.value);
    }
}

/**
 * Tree Item Provider for retrieving categories from the server and displaying
 * them inside a tree list.
 */
export class WorkflowTypeTreeItemProvider implements ITreeItemProvider {
    /** The HTTP client for making API requests. */
    private readonly http = useHttp();

    /**
     * The entity type unique identifier to restrict results to. Set to undefined
     * to include all categories, regardless of entity type.
     */
    public includeInactiveItems?: boolean;

    /**
     * The security grant token that will be used to request additional access
     * to the category list.
     */
    public securityGrantToken?: string | null;

    /**
     * Gets the child items from the server.
     *
     * @param parentGuid The parent item whose children are retrieved.
     *
     * @returns A collection of TreeItem objects as an asynchronous operation.
     */
    private async getItems(parentGuid?: Guid | null): Promise<TreeItemBag[]> {
        const options: WorkflowTypePickerGetWorkflowTypesOptionsBag = {
            parentGuid,
            includeInactiveItems: this.includeInactiveItems ?? false,
            securityGrantToken: this.securityGrantToken,

            getCategorizedItems: false,
            includeCategoriesWithoutChildren: false,
            includeUnnamedEntityItems: false,
            lazyLoad: false,
        };

        const response = await this.http.post<TreeItemBag[]>("/api/v2/Controls/WorkflowTypePickerGetWorkflowTypes", {}, options);

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
    async getRootItems(): Promise<TreeItemBag[]> {
        return await this.getItems();
    }

    /**
     * @inheritdoc
     */
    async getChildItems(item: TreeItemBag): Promise<TreeItemBag[]> {
        return this.getItems(item.value);
    }
}


/**
 * Tree Item Provider for retrieving pages from the server and displaying
 * them inside a tree list.
 */
export class PageTreeItemProvider implements ITreeItemProvider {
    /** The HTTP client for making API requests. */
    private readonly http = useHttp();

    /**
     * The security grant token that will be used to request additional access
     * to the category list.
     */
    public securityGrantToken?: string | null;

    /**
     * List of GUIDs or pages to exclude from the list.
     */
    public hidePageGuids?: Guid[] | null;

    /**
     * Currently selected page
     */
    public selectedPageGuids?: Guid[] | null;

    /**
     * The site type to limit the results.
     */
    public siteType?: SiteType | null;

    /**
     * The unique identifier of the root page whose
     * children are to be enumerated.
     */
    public rootPageGuid?: Guid | null;

    /**
     * Gets the child items of the given parent (or root if no parent given) from the server.
     *
     * @param parentGuid The parent item whose children are retrieved.
     * @param expandToValues The values that should be auto-expanded to.
     *
     * @returns A collection of TreeItem objects as an asynchronous operation.
     */
    private async getItems(parentGuid?: Guid | null, expandToValues?: string[]): Promise<TreeItemBag[]> {
        let result: TreeItemBag[];

        const options: PagePickerGetChildrenOptionsBag = {
            guid: toGuidOrNull(parentGuid) ?? emptyGuid,
            rootPageGuid: this.rootPageGuid ?? null,
            hidePageGuids: this.hidePageGuids ?? [],
            securityGrantToken: this.securityGrantToken,
            siteType: this.siteType,
            expandToValues
        };
        const url = "/api/v2/Controls/PagePickerGetChildren";
        const response = await this.http.post<TreeItemBag[]>(url, undefined, options);

        if (response.isSuccess && response.data) {
            result = response.data;
        }
        else {
            console.log("Error", response.errorMessage);
            return [];
        }

        // If we're getting child nodes or if there is no selected page
        if (parentGuid || !this.selectedPageGuids) {
            return result;
        }

        // If we're getting the root elements and we have a selected page, we also want to grab
        // all the parent pages so we can pre-load the entire hierarchy to the selected page
        return this.getHierarchyToSelectedPage(result);
    }

    /**
     * Get the hierarchical list of parent pages of the selectedPageGuid
     *
     * @returns A list of GUIDs of the parent pages
     */
    private async getParentList(): Promise<Guid[]> {
        const options: PagePickerGetSelectedPageHierarchyOptionsBag = {
            selectedPageGuids: this.selectedPageGuids,
            securityGrantToken: this.securityGrantToken
        };
        const url = "/api/v2/Controls/PagePickerGetSelectedPageHierarchy";
        const response = await this.http.post<Guid[]>(url, undefined, options);

        if (response.isSuccess && response.data) {
            return response.data;
        }
        else {
            console.log("Error", response.errorMessage);
            return [];
        }
    }

    /**
     * Fill in pages to the depth of the selected page
     *
     * @param rootLayer The bottom layer of pages that we'll build depth upon
     *
     * @return The augmented `rootLayer` with the child pages
     */
    private async getHierarchyToSelectedPage(rootLayer: TreeItemBag[]): Promise<TreeItemBag[]> {
        const parents = await this.getParentList();

        if (!parents || parents.length === 0) {
            // Selected page has no parents, so we're done.
            return rootLayer;
        }

        const childLists = await Promise.all(parents.map(guid => this.getItems(guid)));
        const allPages = rootLayer.concat(flatten(childLists));

        parents.forEach((parentGuid, i) => {
            const parentPage: TreeItemBag | undefined = allPages.find(page => page.value === parentGuid);
            if (parentPage) {
                parentPage.children = childLists[i];
            }
        });

        return rootLayer;
    }


    /**
     * @inheritdoc
     */
    async getRootItems(expandToValues: string[]): Promise<TreeItemBag[]> {
        return await this.getItems(null, expandToValues);
    }

    /**
     * @inheritdoc
     */
    async getChildItems(item: TreeItemBag): Promise<TreeItemBag[]> {
        return this.getItems(item.value);
    }
}


/**
 * Tree Item Provider for retrieving connection requests from the server and displaying
 * them inside a tree list.
 */
export class ConnectionRequestTreeItemProvider implements ITreeItemProvider {
    /** The HTTP client for making API requests. */
    private readonly http = useHttp();

    /**
     * The security grant token that will be used to request additional access
     * to the category list.
     */
    public securityGrantToken?: string | null;

    /**
     * Gets the child items from the server.
     *
     * @param parentGuid The parent item whose children are retrieved.
     * @param expandToValues The values that should be auto-expanded to.
     *
     * @returns A collection of TreeItem objects as an asynchronous operation.
     */
    private async getItems(parentGuid?: Guid | null, expandToValues?: string[]): Promise<TreeItemBag[]> {
        const options: ConnectionRequestPickerGetChildrenOptionsBag = {
            parentGuid,
            securityGrantToken: this.securityGrantToken,
            expandToValues
        };
        const url = "/api/v2/Controls/ConnectionRequestPickerGetChildren";
        const response = await this.http.post<TreeItemBag[]>(url, undefined, options);

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
    async getRootItems(expandToValues: string[]): Promise<TreeItemBag[]> {
        return await this.getItems(null, expandToValues);
    }

    /**
     * @inheritdoc
     */
    async getChildItems(item: TreeItemBag): Promise<TreeItemBag[]> {
        return this.getItems(item.value);
    }
}


/**
 * Tree Item Provider for retrieving financial accounts from the server and
 * displaying them inside a tree list.
 */
export class FinancialAccountTreeItemProvider implements ITreeItemProvider {
    /** The HTTP client for making API requests. */
    private readonly http = useHttp();

    /** The security grant token that will be used to request additional access to the account list. */
    public securityGrantToken: string | null = null;

    /** Whether to include inactive accounts. */
    public includeInactive: boolean = false;

    /** Whether to display each account's public name instead of its internal name. */
    public displayPublicName: boolean = false;

    /**
     * Gets the child items from the server.
     *
     * @param parentGuid The parent account whose children are retrieved, or null for the root accounts.
     *
     * @returns A collection of TreeItem objects as an asynchronous operation.
     */
    private async getItems(parentGuid: Guid | null = null): Promise<TreeItemBag[]> {
        const options: AccountPickerGetChildrenOptionsBag = {
            parentGuid: toGuidOrNull(parentGuid) ?? emptyGuid,
            includeInactive: this.includeInactive,
            displayPublicName: this.displayPublicName,
            loadFullTree: false,
            securityGrantToken: this.securityGrantToken
        };
        const url = "/api/v2/Controls/AccountPickerGetChildren";
        const response = await this.http.post<TreeItemBag[]>(url, undefined, options);

        if (response.isSuccess && response.data) {
            return response.data;
        }
        else {
            console.error("Error fetching accounts from server", response.errorMessage);
            return [];
        }
    }

    /**
     * @inheritdoc
     */
    async getRootItems(): Promise<TreeItemBag[]> {
        return await this.getItems(null);
    }

    /**
     * @inheritdoc
     */
    async getChildItems(item: TreeItemBag): Promise<TreeItemBag[]> {
        return this.getItems(toGuidOrNull(item.value));
    }
}


/**
 * Tree Item Provider for retrieving groups from the server and displaying
 * them inside a tree list.
 */
export class GroupTreeItemProvider implements ITreeItemProvider {
    /** The HTTP client for making API requests. */
    private readonly http = useHttp();

    /** The security grant token that will be used to request additional access to the group list. */
    public securityGrantToken: string | null = null;

    /** GUID of the group you want to use as the root. */
    public rootGroupGuid: Guid | null = null;

    /** List of group types GUIDs to limit to groups of those types. */
    public includedGroupTypeGuids: Guid[] = [];

    /** List of group type GUIDs to exclude when no include list is set. */
    public excludedGroupTypeGuids: Guid[] = [];

    /** When true, show no groups by default. */
    public excludeAllByDefault: boolean = false;

    /** Whether to include inactive groups or not. */
    public includeInactiveGroups: boolean = false;

    /** Whether to limit to only groups that have scheduling enabled. */
    public limitToSchedulingEnabled: boolean = false;

    /** Whether to limit to only groups that have RSVPs enabled. */
    public limitToRSVPEnabled: boolean = false;

    /** Whether to limit results to security-role groups only. */
    public limitToSecurityRoleGroups: boolean = false;

    /** Optional campus Guid used to filter groups by campus. */
    public campusGuid: Guid | null = null;

    /** When a campus filter is set, whether to also include groups that have no campus. */
    public includeNoCampus: boolean = false;

    /** Whether to limit results to public groups only. */
    public limitToPublic: boolean = false;

    /**
     * The count mode to attach to each node: 0 = None, 1 = Child Groups,
     * 2 = Group Members. Counts are written to TreeItemBag.childCount.
     */
    public countsType: number = 0;

    /**
     * Gets the child items from the server.
     *
     * @param parentGuid The parent item whose children are retrieved.
     * @param expandToValues The values that should be auto-expanded to.
     *
     * @returns A collection of TreeItem objects as an asynchronous operation.
     */
    private async getItems(parentGuid: Guid | null = null, expandToValues?: string[]): Promise<TreeItemBag[]> {
        const options: GroupPickerGetChildrenOptionsBag = {
            guid: parentGuid,
            rootGroupGuid: this.rootGroupGuid,
            includedGroupTypeGuids: this.includedGroupTypeGuids,
            excludedGroupTypeGuids: this.excludedGroupTypeGuids,
            excludeAllByDefault: this.excludeAllByDefault,
            includeInactiveGroups: this.includeInactiveGroups,
            limitToSchedulingEnabled: this.limitToSchedulingEnabled,
            limitToRSVPEnabled: this.limitToRSVPEnabled,
            limitToSecurityRoleGroups: this.limitToSecurityRoleGroups,
            campusGuid: this.campusGuid,
            includeNoCampus: this.includeNoCampus,
            limitToPublic: this.limitToPublic,
            countsType: this.countsType,
            securityGrantToken: this.securityGrantToken,
            expandToValues
        };
        const url = "/api/v2/Controls/GroupPickerGetChildren";
        const response = await this.http.post<TreeItemBag[]>(url, undefined, options);

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
    async getRootItems(expandToValues: string[]): Promise<TreeItemBag[]> {
        return await this.getItems(null, expandToValues);
    }

    /**
     * @inheritdoc
     */
    async getChildItems(item: TreeItemBag): Promise<TreeItemBag[]> {
        return this.getItems(item.value);
    }
}


/**
 * Tree Item Provider for retrieving merge templates from the server and displaying
 * them inside a tree list.
 */
export class MergeTemplateTreeItemProvider implements ITreeItemProvider {
    /** The HTTP client for making API requests. */
    private readonly http = useHttp();

    /** The security grant token that will be used to request additional access to the group list. */
    public securityGrantToken: string | null = null;

    /** Filter for which merge templates to include in results: Global, Public, or Both */
    public mergeTemplateOwnership: MergeTemplateOwnership = MergeTemplateOwnership.Global;

    /**
     * Gets the child items from the server.
     *
     * @param parentGuid The parent item whose children are retrieved.
     * @param expandToValues The values that should be auto-expanded to.
     *
     * @returns A collection of TreeItem objects as an asynchronous operation.
     */
    private async getItems(parentGuid: Guid | null = null, expandToValues?: string[]): Promise<TreeItemBag[]> {
        const options: MergeTemplatePickerGetMergeTemplatesOptionsBag = {
            parentGuid,
            mergeTemplateOwnership: (toNumberOrNull(this.mergeTemplateOwnership) ?? 0) as MergeTemplateOwnership,
            securityGrantToken: this.securityGrantToken,
            expandToValues
        };
        const url = "/api/v2/Controls/MergeTemplatePickerGetMergeTemplates";
        const response = await this.http.post<TreeItemBag[]>(url, undefined, options);

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
    async getRootItems(expandToValues: string[]): Promise<TreeItemBag[]> {
        return await this.getItems(null, expandToValues);
    }

    /**
     * @inheritdoc
     */
    async getChildItems(item: TreeItemBag): Promise<TreeItemBag[]> {
        return this.getItems(item.value);
    }
}


/**
 * Tree Item Provider for retrieving merge templates from the server and displaying
 * them inside a tree list.
 */
export class MetricCategoryTreeItemProvider implements ITreeItemProvider {
    /** The HTTP client for making API requests. */
    private readonly http = useHttp();

    /** The security grant token that will be used to request additional access to the group list. */
    public securityGrantToken: string | null = null;

    /**
     * Gets the child items from the server.
     *
     * @param parentGuid The parent item whose children are retrieved.
     * @param expandToValues The values that should be auto-expanded to.
     *
     * @returns A collection of TreeItem objects as an asynchronous operation.
     */
    private async getItems(parentGuid: Guid | null = null, expandToValues?: string[]): Promise<TreeItemBag[]> {
        const options: MetricCategoryPickerGetChildrenOptionsBag = {
            parentGuid,
            securityGrantToken: this.securityGrantToken,
            expandToValues
        };
        const url = "/api/v2/Controls/MetricCategoryPickerGetChildren";
        const response = await this.http.post<TreeItemBag[]>(url, undefined, options);

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
    async getRootItems(expandToValues: string[]): Promise<TreeItemBag[]> {
        return await this.getItems(null, expandToValues);
    }

    /**
     * @inheritdoc
     */
    async getChildItems(item: TreeItemBag): Promise<TreeItemBag[]> {
        return this.getItems(item.value);
    }
}

/**
 * Tree Item Provider for retrieving merge templates from the server and displaying
 * them inside a tree list.
 */
export class MetricItemTreeItemProvider implements ITreeItemProvider {
    /** The HTTP client for making API requests. */
    private readonly http = useHttp();

    /** The security grant token that will be used to request additional access to the group list. */
    public securityGrantToken: string | null = null;

    /** A list of category GUIDs to filter the results */
    public includeCategoryGuids: Guid[] | null = null;

    /**
     * Gets the child items from the server.
     *
     * @param parentGuid The parent item whose children are retrieved.
     *
     * @returns A collection of TreeItem objects as an asynchronous operation.
     */
    private async getItems(parentGuid: Guid | null = null): Promise<TreeItemBag[]> {
        const options: MetricItemPickerGetChildrenOptionsBag = {
            parentGuid,
            includeCategoryGuids: this.includeCategoryGuids,
            securityGrantToken: this.securityGrantToken
        };
        const url = "/api/v2/Controls/MetricItemPickerGetChildren";
        const response = await this.http.post<TreeItemBag[]>(url, undefined, options);

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
    async getRootItems(): Promise<TreeItemBag[]> {
        return await this.getItems(null);
    }

    /**
     * @inheritdoc
     */
    async getChildItems(item: TreeItemBag): Promise<TreeItemBag[]> {
        return this.getItems(item.value);
    }
}


/**
 * Tree Item Provider for retrieving registration templates from the server and displaying
 * them inside a tree list.
 */
export class RegistrationTemplateTreeItemProvider implements ITreeItemProvider {
    /** The HTTP client for making API requests. */
    private readonly http = useHttp();

    /** The security grant token that will be used to request additional access to the group list. */
    public securityGrantToken: string | null = null;

    /**
     * Gets the child items from the server.
     *
     * @param parentGuid The parent item whose children are retrieved.
     * @param expandToValues The values that should be auto-expanded to.
     *
     * @returns A collection of TreeItem objects as an asynchronous operation.
     */
    private async getItems(parentGuid: Guid | null = null, expandToValues?: string[]): Promise<TreeItemBag[]> {
        const options: RegistrationTemplatePickerGetChildrenOptionsBag = {
            parentGuid,
            securityGrantToken: this.securityGrantToken,
            expandToValues
        };
        const url = "/api/v2/Controls/RegistrationTemplatePickerGetChildren";
        const response = await this.http.post<TreeItemBag[]>(url, undefined, options);

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
    async getRootItems(expandToValues: string[]): Promise<TreeItemBag[]> {
        return await this.getItems(null, expandToValues);
    }

    /**
     * @inheritdoc
     */
    async getChildItems(item: TreeItemBag): Promise<TreeItemBag[]> {
        return this.getItems(item.value);
    }
}


/**
 * Tree Item Provider for retrieving reports from the server and displaying
 * them inside a tree list.
 */
export class ReportTreeItemProvider implements ITreeItemProvider {
    /** The HTTP client for making API requests. */
    private readonly http = useHttp();

    /** The security grant token that will be used to request additional access to the group list. */
    public securityGrantToken: string | null = null;

    /** A list of category GUIDs to filter the results. */
    public includeCategoryGuids: Guid[] | null = null;

    /** Guid of an Entity Type to filter results by the reports that relate to this entity type. */
    public entityTypeGuid: Guid | null = null;

    /**
     * Gets the child items from the server.
     *
     * @param parentGuid The parent item whose children are retrieved.
     * @param expandToValues The values that should be auto-expanded to.
     *
     * @returns A collection of TreeItem objects as an asynchronous operation.
     */
    private async getItems(parentGuid: Guid | null = null, expandToValues?: string[]): Promise<TreeItemBag[]> {
        const options: ReportPickerGetChildrenOptionsBag = {
            parentGuid,
            includeCategoryGuids: this.includeCategoryGuids,
            entityTypeGuid: this.entityTypeGuid,
            securityGrantToken: this.securityGrantToken,
            expandToValues
        };
        const url = "/api/v2/Controls/ReportPickerGetChildren";
        const response = await this.http.post<TreeItemBag[]>(url, undefined, options);

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
    async getRootItems(expandToValues: string[]): Promise<TreeItemBag[]> {
        return await this.getItems(null, expandToValues);
    }

    /**
     * @inheritdoc
     */
    async getChildItems(item: TreeItemBag): Promise<TreeItemBag[]> {
        return this.getItems(item.value);
    }
}


/**
 * Tree Item Provider for retrieving reports from the server and displaying
 * them inside a tree list.
 */
export class ScheduleTreeItemProvider implements ITreeItemProvider {
    /** The HTTP client for making API requests. */
    private readonly http = useHttp();

    /** The security grant token that will be used to request additional access to the group list. */
    public securityGrantToken: string | null = null;

    /** Whether to include inactive schedules in the results. */
    public includeInactive: boolean = false;

    /** Whether to exclude private schedules in the results. */
    public includePublicOnly: boolean = false;

    /** The category GUIDs to filter schedules by. */
    public includeCategoryGuids: Guid[] | null = null;

    /**
     * When true, restrict the results to schedules eligible for check-in
     * (schedules whose CheckInStartOffsetMinutes is set). Leaf items outside
     * that set are pruned from the tree server-side.
     */
    public includeCheckInSchedulesOnly: boolean = false;

    /**
     * Gets the child items from the server.
     *
     * @param parentGuid The parent item whose children are retrieved.
     * @param expandToValues The values that should be auto-expanded to.
     *
     * @returns A collection of TreeItem objects as an asynchronous operation.
     */
    private async getItems(parentGuid: Guid | null = null, expandToValues?: string[]): Promise<TreeItemBag[]> {
        const options: SchedulePickerGetChildrenOptionsBag = {
            parentGuid,
            includeInactiveItems: this.includeInactive,
            includePublicItemsOnly: this.includePublicOnly,
            includeCategoryGuids: this.includeCategoryGuids,
            includeCheckInSchedulesOnly: this.includeCheckInSchedulesOnly,
            securityGrantToken: this.securityGrantToken,
            expandToValues
        };
        const url = "/api/v2/Controls/SchedulePickerGetChildren";
        const response = await this.http.post<TreeItemBag[]>(url, undefined, options);

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
    async getRootItems(expandToValues: string[]): Promise<TreeItemBag[]> {
        return await this.getItems(null, expandToValues);
    }

    /**
     * @inheritdoc
     */
    async getChildItems(item: TreeItemBag): Promise<TreeItemBag[]> {
        return this.getItems(item.value);
    }
}


/**
 * Tree Item Provider for retrieving reports from the server and displaying
 * them inside a tree list.
 */
export class WorkflowActionTypeTreeItemProvider implements ITreeItemProvider {
    /** The HTTP client for making API requests. */
    private readonly http = useHttp();

    /** The security grant token that will be used to request additional access to the group list. */
    public securityGrantToken: string | null = null;

    /** Whether to include inactive schedules in the results. */
    public includeInactive: boolean = false;

    /**
     * Gets the child items from the server.
     *
     * @param parentGuid The parent item whose children are retrieved.
     * @param expandToValues The values that should be auto-expanded to.
     *
     * @returns A collection of TreeItem objects as an asynchronous operation.
     */
    private async getItems(parentId: string | null = null, expandToValues?: string[]): Promise<TreeItemBag[]> {
        const options: WorkflowActionTypePickerGetChildrenOptionsBag = {
            parentId: toNumberOrNull(parentId) ?? 0,
            securityGrantToken: this.securityGrantToken,
            expandToValues
        };
        const url = "/api/v2/Controls/WorkflowActionTypePickerGetChildren";
        const response = await this.http.post<TreeItemBag[]>(url, undefined, options);

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
    async getRootItems(expandToValues: string[]): Promise<TreeItemBag[]> {
        return await this.getItems(null, expandToValues);
    }

    /**
     * @inheritdoc
     */
    async getChildItems(item: TreeItemBag): Promise<TreeItemBag[]> {
        return this.getItems(item.value);
    }
}

/**
     * Tree Item Provider for retrieving merge fields from the server and displaying
     * them inside a tree list.
     */
export class MergeFieldTreeItemProvider implements ITreeItemProvider {
    /** The HTTP client for making API requests. */
    private readonly http = useHttp();

    /**
     * The security grant token that will be used to request additional access
     * to the category list.
     */
    public securityGrantToken?: string | null;

    /**
     * Currently selected page
     */
    public selectedIds?: string[] | string | null;

    /**
     * Root Level Merge Fields
     */
    public additionalFields: string = "";

    /**
     * Gets the child items of the given parent (or root if no parent given) from the server.
     *
     * @param parentId The parent item whose children are retrieved.
     *
     * @returns A collection of TreeItem objects as an asynchronous operation.
     */
    private async getItems(parentId?: string | null): Promise<TreeItemBag[]> {
        let result: TreeItemBag[];

        const options: MergeFieldPickerGetChildrenOptionsBag = {
            id: parentId || "0",
            additionalFields: this.additionalFields
        };
        const url = "/api/v2/Controls/MergeFieldPickerGetChildren";
        const response = await this.http.post<TreeItemBag[]>(url, undefined, options);

        if (response.isSuccess && response.data) {
            result = response.data;
        }
        else {
            console.log("Error", response.errorMessage);
            return [];
        }

        // If we're getting child nodes or if there is no selected page
        if (parentId || !this.selectedIds || this.selectedIds.length === 0) {
            return result;
        }

        // If we're getting the root elements and we have a selected page, we also want to grab
        // all the parent pages so we can pre-load the entire hierarchy to the selected page
        return this.getHierarchyToSelectedMergeField(result);
    }

    /**
     * Fill in pages to the depth of the selected page
     *
     * @param rootLayer The bottom layer of pages that we'll build depth upon
     *
     * @return The augmented `rootLayer` with the child pages
     */
    private async getHierarchyToSelectedMergeField(rootLayer: TreeItemBag[]): Promise<TreeItemBag[]> {
        const parents = this.getParentList();

        if (!parents || parents.length === 0) {
            // Selected page has no parents, so we're done.
            return rootLayer;
        }

        const childLists = await Promise.all(parents.map(id => this.getItems(id)));
        const allMergeFields = rootLayer.concat(flatten(childLists));

        parents.forEach((parentGuid, i) => {
            const parentMergeField: TreeItemBag | undefined = allMergeFields.find(page => page.value === parentGuid);
            if (parentMergeField) {
                parentMergeField.children = childLists[i];
            }
        });

        return rootLayer;
    }

    /**
     * Get the hierarchical list of parent merge fields of the selected merge fields
     *
     * @returns A list of IDs of the parent merge fields
     */
    private getParentList(): string[] | null {
        if (!this.selectedIds || this.selectedIds.length === 0) {
            return null;
        }

        // If it's a single selection, grab the parents by splitting on "|",
        // e.g. "Grand|Parent|Child" will give ["Grand", "Parent"] as the parents
        if (typeof this.selectedIds == "string") {
            return this.splitSelectionIntoParents(this.selectedIds);
        }

        // Not null/empty nor a single selection, so must be an array of selections
        return flatten(this.selectedIds.map(sel => this.splitSelectionIntoParents(sel)));
    }

    /**
     * Split the given selected ID up and get a list of the parent IDs
     *
     * @param selection a string denoted one of the selected values
     */
    private splitSelectionIntoParents(selection: string): string[] {
        const parentIds: string[] = [];

        // grab the parents by splitting on "|",
        // e.g. "Grand|Parent|Child" will give ["Grand", "Parent"] as the parents
        const splitList = selection.split("|");
        splitList.pop();

        // Now we need to make sure each item further in the list contains it's parents' names
        // e.g. ["Grand", "Parent"] => ["Grand", "Grand|Parent"]
        while (splitList.length >= 1) {
            parentIds.unshift(splitList.join("|"));
            splitList.pop();
        }

        return parentIds;
    }


    /**
     * @inheritdoc
     */
    async getRootItems(): Promise<TreeItemBag[]> {
        return await this.getItems();
    }

    /**
     * @inheritdoc
     */
    async getChildItems(item: TreeItemBag): Promise<TreeItemBag[]> {
        return this.getItems(item.value);
    }
}


/**
 * Tree Item Provider for Asset Storage Provider folders from the server and displaying
 * them inside a tree list.
 */
export class AssetManagerTreeItemProvider implements ITreeItemProvider {
    /** The HTTP client for making API requests. */
    private readonly http = useHttp();

    /** List of folders that are currently expanded in the tree list. */
    public openFolders: Set<string> = new Set();
    public selectedFolder: string | null = "";
    public enableAssetManager = false;
    public enableFileManager = false;
    public encryptedRootFolder = "";
    public securityGrantToken = "";
    public userSpecificRoot = false;

    /**
     * @inheritdoc
     */
    async getRootItems(): Promise<TreeItemBag[]> {
        const options: AssetManagerGetRootFoldersOptionsBag = {
            expandedFolders: this.openFolders.size > 0 ? Array.from(this.openFolders) : null,
            selectedFolder: this.selectedFolder,
            enableAssetManager: this.enableAssetManager,
            enableFileManager: this.enableFileManager,
            rootFolder: this.encryptedRootFolder,
            securityGrantToken: this.securityGrantToken,
            userSpecificRoot: this.userSpecificRoot
        };
        const url = "/api/v2/Controls/AssetManagerGetRootFolders";
        const response = await this.http.post<{ tree: TreeItemBag[], updatedExpandedFolders: string[] }>(url, undefined, options);

        if (response.isSuccess && response.data) {
            this.openFolders = new Set(response.data.updatedExpandedFolders);
            return response.data.tree;
        }
        else {
            console.error("Error Fetching Root Asset Manager Items", response.errorMessage);
            return [];
        }
    }

    /**
     * @inheritdoc
     */
    async getChildItems(item: TreeItemBag): Promise<TreeItemBag[]> {
        const options: AssetManagerBaseOptionsBag = {
            assetFolderId: item.value,
            securityGrantToken: this.securityGrantToken
        };
        const url = "/api/v2/Controls/AssetManagerGetChildren";
        const response = await this.http.post<TreeItemBag[]>(url, undefined, options);

        if (response.isSuccess && response.data) {
            return response.data;
        }
        else {
            console.error("Error Fetching Asset Manager Children", response.errorMessage);
            return [];
        }
    }
}

/**
 * Tree Item Provider for retrieving adaptive messages from the server and displaying
 * them inside a tree list.
 */
export class AdaptiveMessageTreeItemProvider implements ITreeItemProvider {
    /** The HTTP client for making API requests. */
    private readonly http = useHttp();

    /**
     * The security grant token that will be used to request additional access
     * to the category list.
     */
    public securityGrantToken?: string | null;

    /**
     * Gets the child items from the server.
     *
     * @param parentGuid The parent item whose children are retrieved.
     * @param expandToValues The values that should be auto-expanded to.
     *
     * @returns A collection of TreeItem objects as an asynchronous operation.
     */
    private async getItems(parentGuid?: Guid | null, expandToValues?: string[]): Promise<TreeItemBag[]> {
        const options: UniversalItemTreePickerOptionsBag = {
            parentValue: parentGuid,
            securityGrantToken: this.securityGrantToken,
            expandToValues
        };

        const response = await this.http.post<TreeItemBag[]>("/api/v2/Controls/AdaptiveMessagePickerGetAdaptiveMessages", {}, options);

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
    async getRootItems(expandToValues: string[]): Promise<TreeItemBag[]> {
        return await this.getItems(null, expandToValues);
    }

    /**
     * @inheritdoc
     */
    async getChildItems(item: TreeItemBag): Promise<TreeItemBag[]> {
        return this.getItems(item.value);
    }
}

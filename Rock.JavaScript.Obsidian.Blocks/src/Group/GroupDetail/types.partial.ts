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
import { CanDeleteRequestBag } from "@Obsidian/ViewModels/Blocks/Group/GroupDetail/canDeleteRequestBag";
import { CanDeleteResponseBag } from "@Obsidian/ViewModels/Blocks/Group/GroupDetail/canDeleteResponseBag";
import { CopyGroupRequestBag } from "@Obsidian/ViewModels/Blocks/Group/GroupDetail/copyGroupRequestBag";
import { FamilyMemberLocationBag } from "@Obsidian/ViewModels/Blocks/Group/GroupDetail/familyMemberLocationBag";
import { GroupBag } from "@Obsidian/ViewModels/Blocks/Group/GroupDetail/groupBag";
import { GroupRequirementOptionsBag } from "@Obsidian/ViewModels/Blocks/Group/GroupDetail/groupRequirementOptionsBag";
import { GroupTypeOptionsBag } from "@Obsidian/ViewModels/Blocks/Group/GroupDetail/groupTypeOptionsBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ValidPropertiesBox } from "@Obsidian/ViewModels/Utility/validPropertiesBox";

/**
 * Keys for the outbound navigation URL map populated by the C# block in
 * `GetBoxNavigationUrls`. Each URL contains the literal `((Key))` token
 * which the Vue layer substitutes with the active group's IdKey at
 * render time per the IdKey policy in 00-architecture.md.
 */
export const enum NavigationUrlKey {
    AttendancePage = "AttendancePage",
    GroupSchedulerPage = "GroupSchedulerPage",
    GroupRSVPPage = "GroupRSVPPage",
    GroupPlacementPage = "GroupPlacementPage",
    GroupMapPage = "GroupMapPage",
    GroupHistoryPage = "GroupHistoryPage",
    FundraisingProgressPage = "FundraisingProgressPage",
    RegistrationInstancePage = "RegistrationInstancePage",
    EventItemOccurrencePage = "EventItemOccurrencePage",
    ContentItemPage = "ContentItemPage"
}

/**
 * Discriminator keys for the `CanDeleteEntity` block action. Each grid
 * that stages child entities in the bag passes the matching key with the
 * row's Guid so the server can dispatch to the right `Service<T>.CanDelete`
 * before the row is filtered out of the bag. Values must exactly match
 * the `EntityKey` constants in `GroupDetail.cs`.
 */
export const enum EntityKey {
    GroupRequirement = "GroupRequirement",
    GroupMemberWorkflowTrigger = "GroupMemberWorkflowTrigger",
    GroupSync = "GroupSync",
    GroupLocation = "GroupLocation"
}

/**
 * Resolution returned by the archive-children prompt. `false` is a full
 * cancel (Escape, X, backdrop click); the two `archive*` discriminators
 * route to the matching block action.
 */
export type ArchivePromptResult = "archive" | "archiveWithChildren" | false;

/**
 * Shape returned by the `Edit` block action. Carries the editable bag
 * plus the per-GroupType options the edit panel needs on first paint
 * (section visibility, GroupType-pinned dropdown sources).
 */
export type GroupDetailEditResponse = {
    bag: GroupBag;
    validProperties: string[];
    groupTypeOptions: GroupTypeOptionsBag;
};

/**
 * Typed invoker for the Group Detail block's `[BlockAction]` methods. Each
 * method wraps a single server action with the correct request shape and
 * response type so call sites stay free of magic strings and casts.
 *
 * @see useInvokeGroupDetailBlockAction
 */
export type GroupDetailBlockActionInvoker = {
    /**
     * Enters edit mode by fetching the editable bag for the given group.
     * The response carries the bag plus the `validProperties` allowlist the
     * panel uses to gate partial saves.
     */
    edit(key: string): Promise<HttpResult<GroupDetailEditResponse>>;

    /**
     * Persists the edit bag on the server.
     */
    save(box: ValidPropertiesBox<GroupBag> | null | undefined): Promise<HttpResult<ValidPropertiesBox<GroupBag> | string>>;

    /**
     * Deletes the group. Response `data` is the redirect URL the caller
     * navigates to on success.
     */
    delete(key: string): Promise<HttpResult<string>>;

    /**
     * Archives the group without cascading to descendants. Response `data`
     * is the redirect URL.
     */
    archive(key: string): Promise<HttpResult<string>>;

    /**
     * Archives the group and cascades to its descendants. Response `data`
     * is the redirect URL.
     */
    archiveWithChildren(key: string): Promise<HttpResult<string>>;

    /**
     * Copies the group (and optionally its descendants) per the supplied
     * `CopyGroupRequestBag`. Response `data` is the redirect URL pointing
     * at the new group.
     */
    copy(bag: CopyGroupRequestBag): Promise<HttpResult<string>>;

    /**
     * Server-side delete-eligibility check for a child entity staged on the
     * bag (group requirement, sync rule, workflow trigger, or location). The
     * server dispatches to the matching `Service<T>.CanDelete` based on the
     * request's `EntityKey`. Returns null `data` on transport failure; the
     * response's `canDelete` boolean and optional `errorMessage` carry the
     * domain-level verdict.
     */
    canDeleteEntity(request: CanDeleteRequestBag): Promise<HttpResult<CanDeleteResponseBag>>;

    /**
     * Loads the Group Requirement modal's dropdown sources scoped to the
     * given GroupType. Returns the eligible requirement types plus the
     * date-typed group attributes available for "Due Date Group Attribute".
     */
    getGroupRequirementOptions(groupTypeId: number | null): Promise<HttpResult<GroupRequirementOptionsBag>>;

    /**
     * Loads the SystemCommunication dropdown options for the Group Sync
     * modal's Welcome / Exit Communication pickers. No request payload.
     */
    getGroupSyncOptions(): Promise<HttpResult<ListItemBag[]>>;

    /**
     * Refreshes the per-GroupType options that drive the edit panel's
     * section visibility, dropdown sources, and pinned-by-GroupType fields
     * when the user changes the Group Type mid-edit. Returns the full
     * options bag for the picked GroupType.
     */
    getGroupTypeOptions(groupTypeId: number): Promise<HttpResult<GroupTypeOptionsBag>>;

    /**
     * Resolves whether the picked parent group is active. Drives the
     * "selected parent group is inactive" warning under the Parent Group
     * picker. Response `data` is the IsActive boolean; null on transport
     * failure (the watcher falls back to true).
     */
    getParentGroupInfo(parentGroupKey: string): Promise<HttpResult<boolean>>;

    /**
     * Re-filters the Add-mode Group Type dropdown to the GroupTypes that
     * the picked parent group's GroupType permits as children.
     */
    getAllowedChildGroupTypes(parentGroupKey: string): Promise<HttpResult<ListItemBag[]>>;

    /**
     * Lazily loads the Member-tab dropdown options (family addresses of
     * existing group members) for the Group Location modal. Keyed on the
     * group's IdKey since this can't be staged on the initial edit bag
     * without expensive per-row family-location joins.
     */
    getFamilyMemberLocationOptions(groupKey: string): Promise<HttpResult<FamilyMemberLocationBag[]>>;
};

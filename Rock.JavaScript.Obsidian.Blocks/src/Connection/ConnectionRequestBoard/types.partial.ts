/* eslint-disable @typescript-eslint/naming-convention */

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

import { InjectionKey, Ref } from "vue";
import { ConnectionState } from "@Obsidian/Enums/Connection/connectionState";
import { GroupMemberStatus } from "@Obsidian/Enums/Group/groupMemberStatus";
import { CampusBag } from "@Obsidian/ViewModels/Blocks/Core/CampusDetail/campusBag";

// #region v1 API Request View Models

/**
 * The information needed to request connection status view models.
 * For v1 API Endpoint: https://github.com/SparkDevNetwork/Rock/blob/1e1558e2d222df7a3f7bcf36ff4e419a7e9b7a64/Rock.Rest/Controllers/ConnectionRequestsController.Partial.cs#L268
 */
export interface IConnectionStatusViewModelsRequest {
    campusId?: number | null,
    connectorPersonAliasId?: number | null,
    requesterPersonAliasId?: number | null,
    minDate?: string | null,
    maxDate?: string | null,
    delimitedStatusIds?: string | null,
    delimitedConnectionStates?: string | null,
    delimitedLastActivityTypeIds?: string | null,
    statusIconsTemplate?: string | null,
    sortProperty?: number | null,
    maxRequestsPerStatus?: number | null,
    pastDueOnly: boolean
}

// #endregion v1 API Request View Models

// #region v1 API Response View Models

/**
 * Information about a connection status (columns).
 * Representation of: https://github.com/SparkDevNetwork/Rock/blob/1e1558e2d222df7a3f7bcf36ff4e419a7e9b7a64/Rock/Model/Connection/ConnectionRequest/ConnectionStatusViewModel.cs#L25
 */
export interface IConnectionStatusViewModel {
    Id: number;
    Name?: string | null;
    HighlightColor?: string | null;
    RequestCount: number;
    Requests?: IConnectionRequestViewModel[] | null;
}

/**
 * Information about a connection request (cards & grid rows).
 * Representation of: https://github.com/SparkDevNetwork/Rock/blob/1e1558e2d222df7a3f7bcf36ff4e419a7e9b7a64/Rock/Model/Connection/ConnectionRequest/ConnectionRequestViewModel.cs#L28
 */
export interface IConnectionRequestViewModel {
    Id: number;
    PlacementGroupId?: number | null;
    PlacementGroupRoleId?: number | null;
    PlacementGroupMemberStatus?: GroupMemberStatus | null;
    PlacementGroupRoleName?: string | null;
    Comments?: string | null;
    PersonId: number;
    PersonAliasId: number;
    PersonEmail?: string | null;
    PersonNickName?: string | null;
    PersonLastName?: string | null;
    PersonPhotoId?: number | null;
    PersonPhones?: IPhoneViewModel[] | null;
    Campus?: CampusBag | null;
    CampusId?: number | null;
    CampusName?: string | null;
    CampusCode?: string | null;
    ConnectorPhotoId?: number | null;
    ConnectorPersonNickName?: string | null;
    ConnectorPersonLastName?: string | null;
    ConnectorPersonId?: number | null;
    ConnectorPersonAliasId?: number | null;
    StatusId: number;
    ConnectionOpportunityId: number;
    ConnectionTypeId: number;
    StatusName?: string | null;
    StatusHighlightColor?: string | null;
    IsStatusCritical: boolean;
    ActivityCount: number;
    LastActivityDate?: string | null;
    DateOpened?: string | null;
    GroupName?: string | null;
    LastActivityTypeName?: string | null;
    LastActivityTypeId?: number | null;
    Order: number;
    ConnectionState: ConnectionState;
    IsAssignedToYou: boolean;
    IsCritical: boolean;
    IsIdle: boolean;
    IsUnassigned: boolean;
    FollowupDate?: string | null;
    StatusIconsHtml?: string | null;
    CanConnect: boolean;
    CanCurrentUserEdit: boolean;
    RequestAttributes?: string | null;
    StateLabel?: string | null;
    StatusLabelClass?: string | null;
    ActivityCountText?: string | null;
    LastActivityText?: string | null;
    ConnectorPersonFullname?: string | null;
    PersonFullname?: string | null;
    PersonPhotoUrl?: string | null;
    ConnectorPhotoUrl?: string | null;
    CampusHtml?: string | null;
    DaysSinceOpening?: number | null;
    DaysSinceOpeningShortText?: string | null;
    DaysOrWeeksSinceOpeningText?: string | null;
    DaysSinceOpeningLongText?: string | null;
    DaysSinceLastActivity?: number | null;
    DaysSinceLastActivityShortText?: string | null;
    DaysSinceLastActivityLongText?: string | null;
    GroupNameWithRoleAndStatus?: string | null;
}

/**
 * Information about a person's phone number.
 * Representation of: https://github.com/SparkDevNetwork/Rock/blob/1e1558e2d222df7a3f7bcf36ff4e419a7e9b7a64/Rock/Model/Connection/ConnectionRequest/ConnectionRequestViewModel.cs#L607
 */
export interface IPhoneViewModel {
    PhoneType?: string | null;
    FormattedPhoneNumber?: string | null;
    IsMessagingEnabled: boolean;
}

// #endregion v1 API Response View Models

/**
 * An injection key to provide whether request security is enabled.
 */
export const IsRequestSecurityEnabled: InjectionKey<Ref<boolean>> = Symbol("is-request-security-enabled");

/**
 * Keys for page parameters.
 */
export const enum PageParameterKey {
    CampusId = "CampusId",
    ConnectionOpportunityId = "ConnectionOpportunityId",
    ConnectionRequestId = "ConnectionRequestId"
}

/**
 * The request modal mode options available in the Connection Request Board block.
 */
export const enum RequestModalMode {
    /** An existing connection request is being viewed. */
    View = 0,
    /** A connection request is being added or edited. */
    AddEdit = 1
}

/**
 * The request modal "view" sub-mode options available in the Connection Request Board block.
 */
export const enum RequestModalViewSubMode {
    /** An existing connection request is being viewed. */
    View = 0,
    /** A existing connection request's connection request activity is being added or edited. */
    AddEditActivity = 1,
    /** An existing connection request is being transferred. */
    Transfer = 2,
    /** The individual is searching for an alternative connection opportunity to which an existing connection request can be transferred. */
    TransferSearch = 3
}

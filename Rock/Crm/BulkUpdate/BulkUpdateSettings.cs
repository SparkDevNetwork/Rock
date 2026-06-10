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

using System.Collections.Generic;

using Rock.ViewModels.Blocks.Crm.BulkUpdate;
using Rock.Web.Cache;

namespace Rock.Crm.BulkUpdate
{
    /// <summary>
    /// The inputs and authorization context required by
    /// <see cref="BulkUpdateProcessor"/> to apply a bulk update.
    /// </summary>
    internal class BulkUpdateSettings
    {
        /// <summary>
        /// Gets or sets the bulk update payload submitted by the client. This carries the
        /// list of persons to update, the per-field "apply or leave alone" toggles, and
        /// the scalar values to write.
        /// </summary>
        public BulkUpdateBag Bag { get; set; }

        /// <summary>
        /// Gets or sets the alias identifier of the user running the bulk update. Used for
        /// per-user operations such as Following add/remove and for audit columns when the
        /// background thread cannot resolve the current person from the ambient context.
        /// </summary>
        public int? CurrentPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user can write the
        /// Connection Status field. Pre-computed by the block from
        /// <c>BlockCache.IsAuthorized( EDIT_CONNECTION_STATUS )</c>. The processor skips
        /// Connection Status writes when this is <c>false</c>.
        /// </summary>
        public bool CanEditConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user can write the Record
        /// Status field (including Inactive Reason and Inactive Reason Note). Pre-computed
        /// by the block from <c>BlockCache.IsAuthorized( EDIT_RECORD_STATUS )</c>.
        /// </summary>
        public bool CanEditRecordStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user can write the Record
        /// Source field. Pre-computed by the block from
        /// <c>BlockCache.IsAuthorized( EDIT_RECORD_SOURCE )</c>.
        /// </summary>
        public bool CanEditRecordSource { get; set; }

        /// <summary>
        /// Gets or sets the set of Person attributes that may be bulk-updated, keyed by
        /// the attribute's <c>Key</c>. Resolved by the block from the admin-configured
        /// <c>AttributeCategories</c> setting and pre-filtered to those the current user
        /// can <c>EDIT</c>. The processor uses this as the security fence
        /// for the Person Attributes pipeline — keys submitted in
        /// <see cref="BulkUpdateBag.PersonAttributes"/> that aren't in this dictionary
        /// are dropped silently.
        /// </summary>
        public Dictionary<string, AttributeCache> AuthorizedPersonAttributes { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the NoteType the bulk update is authorized to
        /// add notes against. Resolved by the block from
        /// <see cref="BulkUpdateBag.NoteUpdate"/> at Save time after verifying the type
        /// targets <c>Person</c>, is user-selectable, and is <c>EDIT</c>-authorized for
        /// the current user. The processor skips the Note pipeline entirely when this is
        /// <c>null</c>.
        /// </summary>
        public int? AuthorizedNoteTypeId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the Tag the bulk update is authorized to add or
        /// remove against the selected persons. Resolved by the block from
        /// <see cref="BulkUpdateBag.TagUpdate"/> at Save time after verifying the tag
        /// targets <c>Person</c>, is owned by the current user (when personal) or
        /// organizational, and is <c>TAG</c>-authorized. The processor skips the Tag
        /// pipeline entirely when this is <c>null</c>.
        /// </summary>
        public int? AuthorizedTagId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the Group the bulk update is authorized to add
        /// members to, remove members from, or update memberships on. Resolved by the
        /// block from <see cref="BulkUpdateBag.GroupUpdate"/> at Save time after verifying
        /// the current user has <c>EDIT</c> or <c>MANAGE_MEMBERS</c> on the group. The
        /// processor skips the Group pipeline entirely when this is <c>null</c>.
        /// </summary>
        public int? AuthorizedGroupId { get; set; }

        /// <summary>
        /// Gets or sets the set of group-member attributes that may be bulk-updated for
        /// the authorized group, keyed by the attribute's <c>Key</c>. Resolved by the
        /// block from the group's group type, restricted to attributes flagged
        /// <c>ShowOnBulk</c>. The processor uses this as the security fence for member
        /// attribute writes in the Group pipeline — keys submitted in
        /// <see cref="BulkUpdateGroupBag.MemberAttributes"/> that aren't in this dictionary
        /// are dropped silently. The group-level <c>EDIT</c> / <c>MANAGE_MEMBERS</c> check
        /// behind <see cref="AuthorizedGroupId"/> is the authorization gate; member
        /// attributes have no separate per-attribute authorization.
        /// </summary>
        public Dictionary<string, AttributeCache> AuthorizedGroupMemberAttributes { get; set; }

        /// <summary>
        /// Gets or sets the identifiers of the WorkflowTypes the bulk update is authorized
        /// to launch for the selected persons. Resolved by the block from
        /// <see cref="BulkUpdateBag.PostUpdateWorkflowTypeGuids"/> at Save time, restricted
        /// to the admin-configured Workflow Types block setting and intersected with the
        /// current user's <c>Authorization.VIEW</c>. The processor enqueues one launch
        /// transaction per identifier; an empty or <c>null</c> list skips the Workflow
        /// pipeline entirely.
        /// </summary>
        public List<int> AuthorizedWorkflowTypeIds { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the StepType the bulk update is authorized to add,
        /// remove, or modify steps against. Resolved by the block from
        /// <see cref="BulkUpdateBag.StepUpdate"/> at Save time after verifying the current
        /// user has <c>Authorization.EDIT</c> or <c>MANAGE_STEPS</c> on the step type. The
        /// processor skips the Step pipeline entirely when this is <c>null</c>.
        /// </summary>
        public int? AuthorizedStepTypeId { get; set; }

        /// <summary>
        /// Gets or sets the set of Step attributes that may be bulk-updated for the
        /// authorized step type, keyed by the attribute's <c>Key</c>. Resolved by the block
        /// from the step type's attributes, restricted to those flagged <c>ShowOnBulk</c>.
        /// The processor uses this as the security fence for step attribute writes in the
        /// Step pipeline — keys submitted in <see cref="BulkUpdateStepBag.StepAttributes"/>
        /// that aren't in this dictionary are dropped silently. The step-level
        /// <c>EDIT</c> / <c>MANAGE_STEPS</c> check behind <see cref="AuthorizedStepTypeId"/>
        /// is the authorization gate; step attributes have no separate per-attribute
        /// authorization.
        /// </summary>
        public Dictionary<string, AttributeCache> AuthorizedStepAttributes { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of concurrent worker tasks. <c>null</c> or
        /// non-positive values fall back to <see cref="System.Environment.ProcessorCount"/>,
        /// capped at 64.
        /// </summary>
        public int? TaskCount { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of persons in each processing batch.
        /// <c>null</c> or non-positive values let the scheduler partition the work.
        /// </summary>
        public int? BatchSize { get; set; }
    }
}

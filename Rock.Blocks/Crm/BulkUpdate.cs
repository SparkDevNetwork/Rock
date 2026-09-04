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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Rock.Attribute;
using Rock.Configuration;
using Rock.Crm.BulkUpdate;
using Rock.Model;
using Rock.Net;
using Rock.RealTime;
using Rock.RealTime.Topics;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks.Crm.BulkUpdate;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm
{
    /// <summary>
    /// Used for updating information about several individuals at once.
    /// </summary>
    [DisplayName( "Bulk Update" )]
    [Category( "CRM" )]
    [Description( "Used for updating information about several individuals at once." )]
    [IconCssClass( "ti ti-truck" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [SecurityAction( Authorization.EDIT_CONNECTION_STATUS, "The roles and/or users that can edit the connection status for the selected persons." )]
    [SecurityAction( Authorization.EDIT_RECORD_STATUS, "The roles and/or users that can edit the record status for the selected persons." )]
    [SecurityAction( Authorization.EDIT_RECORD_SOURCE, "The roles and/or users that can edit the record source for the selected persons." )]

    #region Block Attributes

    [AttributeCategoryField(
        "Attribute Categories",
        Description = "The person attribute categories to display and allow bulk updating.",
        AllowMultiple = true,
        EntityTypeName = "Rock.Model.Person",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.AttributeCategories )]

    [WorkflowTypeField(
        "Workflow Types",
        Description = "The workflows to make available for bulk updating.",
        AllowMultiple = true,
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.WorkflowTypes )]

    [IntegerField(
        "Task Count",
        Description = "The number of concurrent tasks to use when performing updates. If left blank then it will be determined automatically.",
        DefaultIntegerValue = 0,
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.TaskCount )]

    [IntegerField(
        "Batch Size",
        Description = "The maximum number of items in each processing batch. If not specified, this value will be automatically determined.",
        DefaultIntegerValue = 0,
        IsRequired = false,
        Order = 3,
        Key = AttributeKey.BatchSize )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "694803AC-CD14-4DA9-911E-C0015EFEEA8C" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "42DE5EEC-0F58-41B8-9B81-AF6535C0B9A6" )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.BULK_UPDATE )]
    public class BulkUpdate : RockBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string Set = "Set";
        }

        private static class AttributeKey
        {
            public const string AttributeCategories = "AttributeCategories";
            public const string WorkflowTypes = "WorkflowTypes";
            public const string TaskCount = "TaskCount";
            public const string BatchSize = "BatchSize";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the current user can edit the connection status field
        /// for the persons being bulk-updated. True when the user has <see cref="Authorization.ADMINISTRATE"/>
        /// or the per-field <see cref="Authorization.EDIT_CONNECTION_STATUS"/> security action.
        /// </summary>
        private bool CanEditConnectionStatus =>
            BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson )
            || BlockCache.IsAuthorized( Authorization.EDIT_CONNECTION_STATUS, RequestContext.CurrentPerson );

        /// <summary>
        /// Gets a value indicating whether the current user can edit the record status field
        /// for the persons being bulk-updated. True when the user has <see cref="Authorization.ADMINISTRATE"/>
        /// or the per-field <see cref="Authorization.EDIT_RECORD_STATUS"/> security action.
        /// </summary>
        private bool CanEditRecordStatus =>
            BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson )
            || BlockCache.IsAuthorized( Authorization.EDIT_RECORD_STATUS, RequestContext.CurrentPerson );

        /// <summary>
        /// Gets a value indicating whether the current user can edit the record source field
        /// for the persons being bulk-updated. True when the user has <see cref="Authorization.ADMINISTRATE"/>
        /// or the per-field <see cref="Authorization.EDIT_RECORD_SOURCE"/> security action.
        /// </summary>
        private bool CanEditRecordSource =>
            BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson )
            || BlockCache.IsAuthorized( Authorization.EDIT_RECORD_SOURCE, RequestContext.CurrentPerson );

        /// <summary>
        /// Gets the <see cref="Person"/> entity type identifier, used by the Note and Tag
        /// pipelines to confirm a submitted note type / tag targets people.
        /// </summary>
        private int PersonEntityTypeId => EntityTypeCache.Get( typeof( Person ) ).Id;

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var attributeCategories = GetAttributeCategories( out var duplicateAttributeNames );

            return new BulkUpdateOptionsBag
            {
                CanEditConnectionStatus = CanEditConnectionStatus,
                CanEditRecordStatus = CanEditRecordStatus,
                CanEditRecordSource = CanEditRecordSource,
                UpdatePersons = GetUpdatePersons(),
                WorkflowTypeOptions = GetWorkflowTypeOptions(),
                AttributeCategories = attributeCategories,
                NoteTypeOptions = GetNoteTypeOptions(),
                TagOptions = GetTagOptions(),
                StepProgramOptions = GetStepProgramOptions(),
                AttributeConfigurationWarning = GetDuplicateAttributeWarning( duplicateAttributeNames )
            };
        }

        /// <summary>
        /// Resolves the security fence for the Person Attributes pipeline. Returns the set
        /// of Person attributes the bulk update is authorized to write, keyed by attribute
        /// <c>Key</c>: those that live in a category enumerated by the admin via
        /// <see cref="AttributeKey.AttributeCategories"/> AND that the current user can
        /// <see cref="Authorization.EDIT"/>. The processor intersects what the client
        /// submitted against this set; anything outside it is dropped silently.
        /// </summary>
        private Dictionary<string, AttributeCache> ResolveAuthorizedPersonAttributes()
        {
            var authorized = new Dictionary<string, AttributeCache>();

            var categoryGuids = GetAttributeValue( AttributeKey.AttributeCategories )
                .SplitDelimitedValues()
                .AsGuidList();

            if ( categoryGuids.Count == 0 )
            {
                return authorized;
            }

            var categoryIds = new HashSet<int>();
            foreach ( var guid in categoryGuids )
            {
                var category = CategoryCache.Get( guid );
                if ( category != null )
                {
                    categoryIds.Add( category.Id );
                }
            }

            if ( categoryIds.Count == 0 )
            {
                return authorized;
            }

            foreach ( var attribute in AttributeCache.All() )
            {
                if ( !attribute.IsActive )
                {
                    continue;
                }

                if ( attribute.CategoryIds == null || !attribute.CategoryIds.Any( id => categoryIds.Contains( id ) ) )
                {
                    continue;
                }

                if ( !attribute.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    continue;
                }

                authorized[attribute.Key] = attribute;
            }

            return authorized;
        }

        /// <summary>
        /// Resolves the security fence for the Note pipeline. Returns the
        /// <see cref="NoteType.Id"/> the bulk update is authorized to add notes against,
        /// or <c>null</c> when the client did not submit a Note update, submitted a
        /// blank text, named a non-existent / non-<c>Person</c> / non-selectable note
        /// type, or one the current user is not <c>EDIT</c>-authorized on.
        /// </summary>
        private int? ResolveAuthorizedNoteTypeId( BulkUpdateBag bag )
        {
            var noteTypeGuidString = bag?.NoteUpdate?.NoteType?.Value;
            if ( noteTypeGuidString.IsNullOrWhiteSpace()
                || string.IsNullOrWhiteSpace( bag.NoteUpdate.NoteText ) )
            {
                return null;
            }

            var noteTypeGuid = noteTypeGuidString.AsGuidOrNull();
            if ( !noteTypeGuid.HasValue )
            {
                return null;
            }

            var noteType = NoteTypeCache.Get( noteTypeGuid.Value );
            if ( noteType == null || !noteType.UserSelectable )
            {
                return null;
            }

            if ( noteType.EntityTypeId != PersonEntityTypeId )
            {
                return null;
            }

            if ( !noteType.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return null;
            }

            return noteType.Id;
        }

        /// <summary>
        /// Resolves the security fence for the Tag pipeline. Returns the
        /// <see cref="Tag.Id"/> the bulk update is authorized to add or remove against
        /// the selected persons, or <c>null</c> when the client did not submit a Tag
        /// update, named a tag that does not exist / is not a Person tag / is owned by a
        /// different user, or one the current user is not <c>TAG</c>-authorized on.
        /// </summary>
        private int? ResolveAuthorizedTagId( BulkUpdateBag bag )
        {
            var tagGuid = bag?.TagUpdate?.Tag?.Value.AsGuidOrNull();
            if ( !tagGuid.HasValue )
            {
                return null;
            }

            var currentPerson = RequestContext.CurrentPerson;
            if ( currentPerson == null )
            {
                return null;
            }

            var tag = new TagService( RockContext ).Get( tagGuid.Value );
            if ( tag == null )
            {
                return null;
            }

            if ( tag.EntityTypeId != PersonEntityTypeId )
            {
                return null;
            }

            // Personal tags must be owned by the current user; organizational tags
            // (OwnerPersonAliasId == null) pass through.
            if ( tag.OwnerPersonAliasId.HasValue
                && !currentPerson.Aliases.Any( a => a.Id == tag.OwnerPersonAliasId.Value ) )
            {
                return null;
            }

            if ( !tag.IsAuthorized( Authorization.TAG, currentPerson ) )
            {
                return null;
            }

            return tag.Id;
        }

        /// <summary>
        /// Determines whether the current user may manage members of the group: the gate the
        /// Group pipeline fence and the group pickers all share (<c>EDIT</c> or
        /// <c>MANAGE_MEMBERS</c> on the group).
        /// </summary>
        private bool IsAuthorizedForGroup( Rock.Model.Group group )
        {
            return group.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
                || group.IsAuthorized( Authorization.MANAGE_MEMBERS, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Resolves the security fence for the Group pipeline. Returns the
        /// <see cref="Group.Id"/> the bulk update is authorized to operate on, or
        /// <c>null</c> when the client did not submit a Group update, named a group that
        /// does not exist, or one the current user lacks <c>EDIT</c> and
        /// <c>MANAGE_MEMBERS</c> on. Mirrors the authorization gate already enforced by
        /// the <see cref="GetGroupRoles"/> and <see cref="GetGroupMemberAttributes"/>
        /// block actions.
        /// </summary>
        private int? ResolveAuthorizedGroupId( BulkUpdateBag bag )
        {
            var groupGuid = bag?.GroupUpdate?.Group?.Value.AsGuidOrNull();
            if ( !groupGuid.HasValue )
            {
                return null;
            }

            var group = new GroupService( RockContext ).Get( groupGuid.Value );
            if ( group == null )
            {
                return null;
            }

            if ( !IsAuthorizedForGroup( group ) )
            {
                return null;
            }

            return group.Id;
        }

        /// <summary>
        /// Resolves the security fence for group-member attribute writes. Returns the
        /// <c>ShowOnBulk</c> member attributes eligible for the authorized group, keyed by
        /// attribute <c>Key</c>. Returns an empty dictionary when no group is authorized.
        /// The group-level authorization behind <paramref name="authorizedGroupId"/> is the
        /// gate; member attributes carry no separate per-attribute authorization.
        /// </summary>
        private Dictionary<string, AttributeCache> ResolveAuthorizedGroupMemberAttributes( int? authorizedGroupId )
        {
            if ( !authorizedGroupId.HasValue )
            {
                return new Dictionary<string, AttributeCache>();
            }

            var group = new GroupService( RockContext ).Get( authorizedGroupId.Value );

            return GetBulkGroupMemberAttributes( group )
                .ToDictionary( a => a.Key, a => a );
        }

        /// <summary>
        /// Resolves the group-member attributes eligible for bulk update on the supplied
        /// group, restricted to those flagged <c>ShowOnBulk</c>. Uses
        /// <c>GroupMember.LoadAttributes</c> so the set includes unqualified,
        /// <c>GroupId</c>-qualified, group-type, and inherited group-type member attributes,
        /// not just attributes qualified by the group's own <c>GroupTypeId</c>. Both the
        /// picker (<see cref="GetGroupMemberAttributes"/>) and the server-side fence
        /// (<see cref="ResolveAuthorizedGroupMemberAttributes"/>) flow through this method.
        /// </summary>
        /// <param name="group">The authorized group, or <c>null</c>.</param>
        /// <returns>The bulk-eligible group-member attributes; empty when the group is null.</returns>
        private List<AttributeCache> GetBulkGroupMemberAttributes( Rock.Model.Group group )
        {
            if ( group == null )
            {
                return new List<AttributeCache>();
            }

            /*
                5/28/2026 - MSE

                Resolve member attributes through a transient GroupMember + LoadAttributes
                (the standard Rock pattern; see GroupPlacement, SignUpRegister) instead of a
                flat AllForEntityType<GroupMember>() filter. Only LoadAttributes is
                inheritance-aware, so the flat filter silently dropped unqualified,
                GroupId-qualified, and parent-group-type member attributes.

                Group and GroupId are both set deliberately: Group supplies the GroupTypeId
                for the inheritance walk, GroupId enables GroupId-qualified matching.
            */
            var groupMember = new GroupMember
            {
                Group = group,
                GroupId = group.Id
            };
            groupMember.LoadAttributes( RockContext );

            return groupMember.Attributes.Values
                .Where( a => a.ShowOnBulk )
                .ToList();
        }

        /// <summary>
        /// Resolves the security fence for the Workflow pipeline. Returns the
        /// <see cref="WorkflowType.Id"/> values the bulk update is authorized to launch:
        /// those enumerated by the admin via <see cref="AttributeKey.WorkflowTypes"/> AND
        /// that the current user can <see cref="Authorization.VIEW"/>. The processor enqueues
        /// one launch per identifier; anything the client submits outside this set is dropped.
        /// </summary>
        private List<int> ResolveAuthorizedWorkflowTypeIds( BulkUpdateBag bag )
        {
            var authorized = new List<int>();

            if ( bag?.PostUpdateWorkflowTypeGuids == null || bag.PostUpdateWorkflowTypeGuids.Count == 0 )
            {
                return authorized;
            }

            var configuredGuids = new HashSet<Guid>(
                GetAttributeValue( AttributeKey.WorkflowTypes ).SplitDelimitedValues().AsGuidList() );

            foreach ( var workflowTypeGuid in bag.PostUpdateWorkflowTypeGuids.Distinct() )
            {
                if ( !configuredGuids.Contains( workflowTypeGuid ) )
                {
                    continue;
                }

                var workflowType = WorkflowTypeCache.Get( workflowTypeGuid );
                if ( workflowType == null )
                {
                    continue;
                }

                if ( workflowType.IsActive == false )
                {
                    continue;
                }

                if ( !workflowType.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                {
                    continue;
                }

                authorized.Add( workflowType.Id );
            }

            return authorized;
        }

        /// <summary>
        /// Determines whether the current user may manage steps of the step type: the gate the
        /// Step pipeline fence and the step attribute picker share (<c>EDIT</c> or
        /// <c>MANAGE_STEPS</c> on the step type).
        /// </summary>
        private bool IsAuthorizedForStepType( StepTypeCache stepType )
        {
            return stepType.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
                || stepType.IsAuthorized( Authorization.MANAGE_STEPS, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Resolves the security fence for the Step pipeline. Returns the
        /// <see cref="StepType.Id"/> the bulk update is authorized to add, remove, or modify
        /// steps against, or <c>null</c> when the client did not submit a Step update, named
        /// a step type that does not exist, or one the current user lacks both
        /// <c>EDIT</c> and <c>MANAGE_STEPS</c> on. Mirrors the authorization gate already
        /// enforced by the <see cref="GetStepAttributes"/> block action.
        /// </summary>
        private int? ResolveAuthorizedStepTypeId( BulkUpdateBag bag )
        {
            var stepTypeGuid = bag?.StepUpdate?.StepType?.Value.AsGuidOrNull();
            if ( !stepTypeGuid.HasValue )
            {
                return null;
            }

            var stepType = StepTypeCache.Get( stepTypeGuid.Value );
            if ( stepType == null )
            {
                return null;
            }

            if ( !IsAuthorizedForStepType( stepType ) )
            {
                return null;
            }

            return stepType.Id;
        }

        /// <summary>
        /// Resolves the security fence for step attribute writes. Returns the
        /// <c>ShowOnBulk</c> step attributes eligible for the authorized step type, keyed by
        /// attribute <c>Key</c>. Returns an empty dictionary when no step type is authorized.
        /// Resolution matches the <see cref="GetStepAttributes"/> picker so the fence and the
        /// attributes carry no separate per-attribute authorization.
        /// </summary>
        private Dictionary<string, AttributeCache> ResolveAuthorizedStepAttributes( int? authorizedStepTypeId )
        {
            if ( !authorizedStepTypeId.HasValue )
            {
                return new Dictionary<string, AttributeCache>();
            }

            return GetBulkStepAttributes( authorizedStepTypeId.Value )
                .ToDictionary( a => a.Key, a => a );
        }

        /// <summary>
        /// Resolves the Step attributes eligible for bulk update against the supplied step
        /// type: those flagged <c>ShowOnBulk</c> that are either global (no entity-type
        /// qualifier) or qualified to this <c>StepTypeId</c>, excluding the system
        /// <c>Order</c> / <c>Active</c> attributes. The qualifier rule also surfaces
        /// unqualified (global) Step attributes. Both the <see cref="GetStepAttributes"/>
        /// picker and the <see cref="ResolveAuthorizedStepAttributes"/> write fence resolve
        /// through this one method.
        /// </summary>
        private static List<AttributeCache> GetBulkStepAttributes( int stepTypeId )
        {
            return AttributeCache.AllForEntityType<Step>()
                .Where( a =>
                    ( string.IsNullOrEmpty( a.EntityTypeQualifierColumn )
                        || ( a.EntityTypeQualifierColumn.Equals( "StepTypeId", StringComparison.OrdinalIgnoreCase )
                            && ( string.IsNullOrEmpty( a.EntityTypeQualifierValue )
                                || a.EntityTypeQualifierValue.Equals( stepTypeId.ToString() ) ) ) )
                    && a.Key != "Order"
                    && a.Key != "Active"
                    && a.ShowOnBulk )
                .ToList();
        }

        /// <summary>
        /// Gets the list the initial list of persons to update based on the entity set page parameter.
        /// </summary>
        /// <returns>The list of persons to update.</returns>
        private List<BulkUpdatePersonBag> GetUpdatePersons()
        {
            var setKey = PageParameter( PageParameterKey.Set );
            if ( setKey.IsNullOrWhiteSpace() )
            {
                return new List<BulkUpdatePersonBag>();
            }

            var entitySetService = new EntitySetService( RockContext );
            var entitySet = entitySetService.Get( setKey, !PageCache.Layout.Site.DisablePredictableIds );
            if ( entitySet == null )
            {
                return new List<BulkUpdatePersonBag>();
            }

            /*
                5/27/2026 - MSE

                Materialize the Person entities before projecting to the bag because
                PhotoUrl is a computed property that EF cannot translate to SQL.
                Persons without a primary alias are silently skipped; in practice
                every Person has one after its SaveHook runs.
            */
            return entitySetService.GetEntityQuery<Person>( entitySet.Id )
                .ToList()
                .Where( p => p.PrimaryAliasGuid.HasValue )
                .Select( p => new BulkUpdatePersonBag
                {
                    PersonAliasGuid = p.PrimaryAliasGuid.Value,
                    FullName = $"{p.NickName} {p.LastName}",
                    PhotoUrl = p.PhotoUrl
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the workflow type options.
        /// </summary>
        /// <returns>The list of workflow type options.</returns>
        private List<ListItemBag> GetWorkflowTypeOptions()
        {
            var guids = GetAttributeValue( AttributeKey.WorkflowTypes ).SplitDelimitedValues().AsGuidList();

            var workflowTypeOptions = new List<ListItemBag>();
            foreach ( var guid in guids )
            {
                var workflowType = WorkflowTypeCache.Get( guid );
                if ( workflowType != null )
                {
                    if ( workflowType.IsActive == false )
                    {
                        continue;
                    }

                    if ( !workflowType.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                    {
                        continue;
                    }

                    workflowTypeOptions.Add( workflowType.ToListItemBag() );
                }
            }

            return workflowTypeOptions.OrderBy( wt => wt.Text ).ToList();
        }

        /// <summary>
        /// Gets the attribute categories.
        /// </summary>
        /// <param name="duplicateAttributeNames">
        /// Outputs the distinct names of any attributes that were configured in more than
        /// one selected category and de-duplicated to a single category. Empty when there
        /// were no duplicates.
        /// </param>
        /// <returns>The list of bulk update attribute categories.</returns>
        private List<BulkUpdateAttributeCategoryBag> GetAttributeCategories( out List<string> duplicateAttributeNames )
        {
            var guids = GetAttributeValue( AttributeKey.AttributeCategories ).SplitDelimitedValues().AsGuidList();

            var categories = new List<BulkUpdateAttributeCategoryBag>();

            foreach ( var guid in guids )
            {
                var category = CategoryCache.Get( guid );
                if ( category == null )
                {
                    continue;
                }

                var attributesOfCategory = new AttributeService( RockContext ).GetByCategoryId( category.Id, false )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name )
                    .ToList()
                    .Where( a => a.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                    .Select( a => PublicAttributeHelper.GetPublicAttributeForEdit( AttributeCache.Get( a.Id ) ) )
                    .ToList();

                categories.Add( new BulkUpdateAttributeCategoryBag
                {
                    Name = category.Name,
                    Description = category.Description,
                    Guid = category.Guid,
                    IconCssClass = category.IconCssClass,
                    Attributes = attributesOfCategory
                } );
            }

            var result = categories.OrderBy( c => c.Name ).ToList();

            /*
                 6/9/26 - MSE

                 An attribute can belong to more than one category in Rock, but Bulk Update
                 edits each attribute by its key into a single value, so showing it under
                 two categories would let a user enter conflicting values (last one wins).
                 De-duplicate: keep each attribute in the first category it appears in (by
                 name) and drop it from later ones. The dropped names are returned so the
                 caller can surface a non-blocking notice to administrators.

                 Reason: Avoid ambiguous duplicate attribute editing without breaking valid configs.
            */
            var seenAttributeGuids = new HashSet<Guid>();
            var duplicateNames = new List<string>();
            foreach ( var category in result )
            {
                var keptAttributes = new List<PublicAttributeBag>();

                foreach ( var attribute in category.Attributes )
                {
                    if ( seenAttributeGuids.Add( attribute.AttributeGuid ) )
                    {
                        keptAttributes.Add( attribute );
                    }
                    else if ( !duplicateNames.Contains( attribute.Name ) )
                    {
                        duplicateNames.Add( attribute.Name );
                    }
                }

                category.Attributes = keptAttributes;
            }

            duplicateAttributeNames = duplicateNames;
            return result;
        }

        /// <summary>
        /// Builds the non-blocking notice shown to block administrators when one or more
        /// attributes were configured in multiple selected categories and therefore
        /// de-duplicated to a single category. Returns <c>null</c> when there are no
        /// duplicates or the current user cannot administrate the block, since the notice
        /// is only actionable by someone who can change the block's category configuration.
        /// </summary>
        /// <param name="duplicateAttributeNames">The distinct names of the de-duplicated attributes.</param>
        /// <returns>The notice text, or <c>null</c> when nothing should be shown.</returns>
        private string GetDuplicateAttributeWarning( List<string> duplicateAttributeNames )
        {
            if ( duplicateAttributeNames.Count == 0 )
            {
                return null;
            }

            if ( !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return null;
            }

            var subject = duplicateAttributeNames.Count == 1 ? "attribute is" : "attributes are";
            return $"The following {subject} configured in more than one selected category and shown only once, " +
                $"in the first category alphabetically: {string.Join( ", ", duplicateAttributeNames )}.";
        }

        /// <summary>
        /// Gets the note type options.
        /// </summary>
        /// <returns>The list of note type options.</returns>
        private List<ListItemBag> GetNoteTypeOptions()
        {
            var noteTypeOptions = new List<ListItemBag>();

            var noteTypes = NoteTypeCache.GetByEntity( PersonEntityTypeId, string.Empty, string.Empty, true );

            foreach ( var noteType in noteTypes )
            {
                if ( !noteType.UserSelectable || !noteType.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    continue;
                }

                noteTypeOptions.Add( noteType.ToListItemBag() );
            }

            return noteTypeOptions.OrderBy( nt => nt.Text ).ToList();
        }

        /// <summary>
        /// Gets the tag options.
        /// </summary>
        /// <returns>The list of tag options.</returns>
        private List<ListItemBag> GetTagOptions()
        {
            var personEntityTypeId = PersonEntityTypeId;
            var currentPerson = RequestContext.CurrentPerson;

            if ( currentPerson == null )
            {
                return new List<ListItemBag>();
            }

            var currentPersonAliasIds = currentPerson.Aliases.Select( a => a.Id ).ToList();

            return new TagService( RockContext ).Queryable().AsNoTracking()
                .Where( t => t.EntityTypeId == personEntityTypeId &&
                             ( t.OwnerPersonAliasId == null || currentPersonAliasIds.Contains( t.OwnerPersonAliasId.Value ) ) )
                .OrderByDescending( t => t.OwnerPersonAliasId.HasValue )
                .ThenBy( t => t.Name )
                .ToList()
                .Where( t => t.IsAuthorized( Authorization.TAG, currentPerson ) )
                .Select( t => new ListItemBag
                {
                    Value = t.Guid.ToString(),
                    Text = t.Name,
                    Category = t.OwnerPersonAliasId == null ? "Organization Tags" : "Personal Tags"
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the step program options.
        /// </summary>
        /// <returns>The list of step program options with their step types and statuses.</returns>
        private List<BulkUpdateStepProgramBag> GetStepProgramOptions()
        {
            return new StepProgramService( RockContext ).Queryable().AsNoTracking()
                .Where( p => p.IsActive )
                .OrderBy( p => p.Order )
                .ThenBy( p => p.Name )
                .Select( p => new BulkUpdateStepProgramBag
                {
                    StepProgram = new ListItemBag
                    {
                        Value = p.Guid.ToString(),
                        Text = p.Name
                    },
                    StepTypes = p.StepTypes
                        .Where( t => t.IsActive )
                        .OrderBy( t => t.Order )
                        .ThenBy( t => t.Name )
                        .Select( t => new ListItemBag
                        {
                            Value = t.Guid.ToString(),
                            Text = t.Name
                        } )
                        .ToList(),
                    StepStatuses = p.StepStatuses
                        .Where( s => s.IsActive )
                        .OrderBy( s => s.Order )
                        .ThenBy( s => s.Name )
                        .Select( s => new ListItemBag
                        {
                            Value = s.Guid.ToString(),
                            Text = s.Name
                        } )
                        .ToList()
                } )
                .ToList();
        }

        /// <summary>
        /// Determines whether the payload requests at least one update. Mirrors the client's
        /// pre-submit check so a no-op bag (e.g. from a non-UI caller) is rejected rather than
        /// spinning up a background run that applies nothing and then reports a false success.
        /// </summary>
        /// <param name="bag">The bulk update save payload.</param>
        /// <returns><c>true</c> when any field, attribute, note, group, tag, step, or workflow action is present.</returns>
        private static bool HasAnyUpdateAction( BulkUpdateBag bag )
        {
            return ( bag.UpdatedFields?.Values.Any( isUpdating => isUpdating ) ?? false )
                || ( bag.PersonAttributes?.Count > 0 )
                || !string.IsNullOrWhiteSpace( bag.NoteUpdate?.NoteText )
                || bag.GroupUpdate?.Group != null
                || bag.TagUpdate?.Tag != null
                || bag.StepUpdate?.StepType != null
                || ( bag.PostUpdateWorkflowTypeGuids?.Any() ?? false );
        }

        /// <summary>
        /// Determines whether anything in the payload survives the security fences and will
        /// actually be applied. <see cref="HasAnyUpdateAction"/> only proves the client asked
        /// for something; this proves the request is not entirely composed of operations the
        /// user is not authorized for (an unowned tag, an inaccessible group/step, etc.).
        /// Rejecting an all-fenced-out payload here avoids spinning up a run that touches
        /// nothing and then reports a phantom "successfully updated".
        /// </summary>
        /// <param name="bag">The bulk update save payload.</param>
        /// <param name="settings">The settings holding the already-resolved authorization fences.</param>
        /// <returns><c>true</c> when at least one field, authorized attribute, note, tag, group, step, or workflow will be applied.</returns>
        private static bool HasApplicableWork( BulkUpdateBag bag, BulkUpdateSettings settings )
        {
            var hasFieldWork = bag.UpdatedFields?.Values.Any( isUpdating => isUpdating ) ?? false;

            var hasAttributeWork = bag.PersonAttributes?.Count > 0
                && settings.AuthorizedPersonAttributes != null
                && bag.PersonAttributes.Keys.Any( key => settings.AuthorizedPersonAttributes.ContainsKey( key ) );

            return hasFieldWork
                || hasAttributeWork
                || settings.AuthorizedNoteTypeId.HasValue
                || settings.AuthorizedTagId.HasValue
                || settings.AuthorizedGroupId.HasValue
                || settings.AuthorizedStepTypeId.HasValue
                || ( settings.AuthorizedWorkflowTypeIds?.Count > 0 );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Retrieves the details for a specific person to add to the update list.
        /// </summary>
        /// <param name="personAliasGuid">The person's primary alias unique identifier (as emitted by the PersonPicker).</param>
        /// <returns>A block action result containing the person bag.</returns>
        [BlockAction]
        public BlockActionResult GetUpdatePerson( Guid personAliasGuid )
        {
            if ( personAliasGuid == Guid.Empty )
            {
                return ActionBadRequest( "Invalid person identifier." );
            }

            var person = new PersonAliasService( RockContext ).Queryable()
                .Where( pa => pa.Guid == personAliasGuid )
                .Select( pa => pa.Person )
                .FirstOrDefault();

            if ( person == null )
            {
                return ActionNotFound();
            }

            return ActionOk( new BulkUpdatePersonBag
            {
                PersonAliasGuid = personAliasGuid,
                FullName = $"{person.NickName} {person.LastName}",
                PhotoUrl = person.PhotoUrl
            } );
        }

        /// <summary>
        /// Gets the projected graduation year for the specified grade DefinedValue.
        /// The grade's offset is applied to the system's current graduation year.
        /// </summary>
        /// <param name="gradeValueGuid">The grade DefinedValue unique identifier.</param>
        /// <returns>A block action result containing the projected graduation year.</returns>
        [BlockAction]
        public BlockActionResult GetGraduationYearFromGrade( Guid gradeValueGuid )
        {
            if ( gradeValueGuid == Guid.Empty )
            {
                return ActionBadRequest( "A valid grade identifier is required." );
            }

            var gradeValue = DefinedValueCache.Get( gradeValueGuid );
            if ( gradeValue == null )
            {
                return ActionNotFound();
            }

            var offset = gradeValue.Value.AsIntegerOrNull();
            if ( !offset.HasValue )
            {
                return ActionNotFound( "Grade has no valid offset." );
            }

            return ActionOk( PersonService.GetCurrentGraduationYear() + offset.Value );
        }

        /// <summary>
        /// Gets the group roles available for the specified group, ordered by role
        /// order then name.
        /// </summary>
        /// <param name="groupGuid">The unique identifier of the group.</param>
        /// <returns>A block action result containing the group type guid and a list of role options.</returns>
        [BlockAction]
        public BlockActionResult GetGroupRoles( Guid groupGuid )
        {
            if ( groupGuid == Guid.Empty )
            {
                return ActionBadRequest( "A valid group identifier is required." );
            }

            var group = new GroupService( RockContext ).Get( groupGuid );

            if ( group == null )
            {
                return ActionNotFound();
            }

            if ( !IsAuthorizedForGroup( group ) )
            {
                return ActionForbidden( "Not authorized to access this group." );
            }

            var groupType = GroupTypeCache.Get( group.GroupTypeId );

            if ( groupType == null )
            {
                return ActionNotFound();
            }

            var roles = groupType.Roles
                .OrderBy( r => r.Order )
                .ThenBy( r => r.Name )
                .Select( r => new ListItemBag
                {
                    Value = r.Guid.ToString(),
                    Text = r.Name
                } )
                .ToList();

            return ActionOk( new GroupRolesResponseBag
            {
                GroupTypeGuid = groupType.Guid.ToString(),
                Roles = roles
            } );
        }

        /// <summary>
        /// Saves the bulk update operation. Validates the request, instantiates the
        /// processor, and runs it on a background task. Progress is streamed to the
        /// caller's RealTime <see cref="ITaskActivityProgress"/> topic.
        /// </summary>
        /// <param name="bag">The bulk update save payload.</param>
        /// <param name="sessionId">
        /// The RealTime connection identifier (the client's <c>topic.connectionId</c>) used to
        /// route progress events back to the originating browser.
        /// </param>
        /// <returns>A block action result indicating whether the run was started.</returns>
        [BlockAction]
        public BlockActionResult Save( BulkUpdateBag bag, string sessionId )
        {
            if ( bag?.UpdatePersons == null || !bag.UpdatePersons.Any() )
            {
                return ActionBadRequest( "At least one person must be selected." );
            }

            if ( !HasAnyUpdateAction( bag ) )
            {
                return ActionBadRequest( "Select at least one update to apply." );
            }

            if ( string.IsNullOrWhiteSpace( sessionId ) )
            {
                return ActionBadRequest( "A real-time connection is required. Please wait a moment and try again." );
            }

            var authorizedGroupId = ResolveAuthorizedGroupId( bag );
            var authorizedStepTypeId = ResolveAuthorizedStepTypeId( bag );

            var settings = new BulkUpdateSettings
            {
                Bag = bag,
                CurrentPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId,
                CanEditConnectionStatus = CanEditConnectionStatus,
                CanEditRecordStatus = CanEditRecordStatus,
                CanEditRecordSource = CanEditRecordSource,
                AuthorizedPersonAttributes = ResolveAuthorizedPersonAttributes(),
                AuthorizedNoteTypeId = ResolveAuthorizedNoteTypeId( bag ),
                AuthorizedTagId = ResolveAuthorizedTagId( bag ),
                AuthorizedGroupId = authorizedGroupId,
                AuthorizedGroupMemberAttributes = ResolveAuthorizedGroupMemberAttributes( authorizedGroupId ),
                AuthorizedWorkflowTypeIds = ResolveAuthorizedWorkflowTypeIds( bag ),
                AuthorizedStepTypeId = authorizedStepTypeId,
                AuthorizedStepAttributes = ResolveAuthorizedStepAttributes( authorizedStepTypeId ),
                TaskCount = GetAttributeValue( AttributeKey.TaskCount ).AsIntegerOrNull(),
                BatchSize = GetAttributeValue( AttributeKey.BatchSize ).AsIntegerOrNull()
            };

            // The client asked for something (HasAnyUpdateAction, above), but if every
            // requested action was dropped by a security fence there is nothing left to do.
            // Reject it here rather than running a no-op that would report a phantom success.
            if ( !HasApplicableWork( bag, settings ) )
            {
                return ActionBadRequest( "None of the selected updates could be applied. You may not have permission for them, or they are no longer available." );
            }

            // Captured on the request thread; re-established on the background thread below so
            // the bulk update's writes record the initiating user in their audit columns.
            var capturedRequestContext = RequestContext;

            Task.Run( async () =>
            {
                /*
                    5/27/2026 - MSE

                    Small delay so the browser can finish rendering the progress UI and
                    subscribe to the RealTime topic before the first progress event fires.
                    Matches the pattern in SampleData and BulkImportTool.
                */
                await Task.Delay( 1000 );

                TaskActivityProgress progress = null;

                try
                {
                    var taskChannelName = $"BulkUpdate:{Guid.NewGuid()}";
                    var topic = RealTimeHelper.GetTopicContext<ITaskActivityProgress>();

                    await topic.Channels.AddToChannelAsync( sessionId, taskChannelName );

                    var progressReporter = topic.Clients.Channel( taskChannelName );
                    progress = new TaskActivityProgress( progressReporter, "Bulk Update" )
                    {
                        StartNotificationDelayMilliseconds = 0
                    };
                    progress.StartTask( $"Updating {bag.UpdatePersons.Count} {( bag.UpdatePersons.Count == 1 ? "person" : "people" )}..." );

                    /*
                        5/28/2026 - MSE

                        When Rock saves an entity it automatically fills the CreatedByPersonAliasId
                        and ModifiedByPersonAliasId columns with whoever
                        DbContext.GetCurrentPersonAliasId() reports as the current person (the
                        BulkInsert and SaveAttributeValues paths do the same). That lookup only
                        works inside a live request. This processor runs on a background Task after
                        the Save action has already returned, so the lookup finds no current person
                        and those columns would be saved with no one recorded.

                        Re-establishing the captured RockRequestContext restores it, matching the
                        request pipeline's own accessor-set pattern (RockPage / ServiceScopeHandler)
                        and the fake HttpContext the legacy WebForms block used
                        (GetDataContextForCurrentUser).

                        This must run before Process() starts its Parallel.ForEach: the AsyncLocal
                        set here flows into the workers, where the framework resolves the user on
                        every write.

                        Reason: Records changed by the bulk update process are credited to the person
                        who ran it, instead of being left without an author.
                    */
                    if ( RockApp.Current.GetRequiredService<IRockRequestContextAccessor>() is RockRequestContextAccessor requestContextAccessor )
                    {
                        requestContextAccessor.RockRequestContext = capturedRequestContext;
                    }

                    var processor = new BulkUpdateProcessor( settings, progress );

                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                    var result = processor.Process();
                    stopwatch.Stop();

                    var statusMessage = BuildFinalStatusMessage( result, stopwatch.Elapsed );
                    progress.StopTask( statusMessage, result.Errors, null, result );
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                    progress?.StopTask( "The bulk update failed.", new[] { ex.Message } );
                }
                finally
                {
                    progress?.Dispose();
                }
            } );

            return ActionOk();
        }

        /// <summary>
        /// Builds the user-facing summary line shown when the bulk update completes. Each
        /// outcome is reported on its own so the three cases are never conflated: fully
        /// updated; had at least one requested change that could not be applied (other
        /// changes may have committed); and not updated at all.
        /// </summary>
        private static string BuildFinalStatusMessage( BulkUpdateResultBag result, TimeSpan elapsed )
        {
            if ( result.TotalCount == 0 )
            {
                // Nothing resolved to update (e.g. every selected record was deleted or
                // merged after selection), so there is no success to report.
                return "No individuals were updated.";
            }

            var elapsedSuffix = $" ({elapsed.TotalSeconds:0.0}s)";
            var hasRunLevelErrors = result.Errors != null && result.Errors.Count > 0;

            // Everyone was fully updated.
            if ( result.IssuesCount == 0 && result.FailedCount == 0 )
            {
                if ( !hasRunLevelErrors )
                {
                    return $"Successfully updated {result.SuccessCount} {PersonWord( result.SuccessCount )}.{elapsedSuffix}";
                }

                // Every person committed, but a follow-up step (e.g. a workflow launch) failed.
                // These errors are not per-person, so report them separately from the people.
                var errorWord = result.Errors.Count == 1 ? "error" : "errors";
                return $"Successfully updated {result.SuccessCount} {PersonWord( result.SuccessCount )}. {result.Errors.Count} follow-up {errorWord} occurred.{elapsedSuffix}";
            }

            // Report each bucket on its own so a partial update (some changes applied) is
            // never conflated with a record that was not touched at all.
            var clauses = new List<string>();

            if ( result.SuccessCount > 0 )
            {
                clauses.Add( $"successfully updated {result.SuccessCount} {PersonWord( result.SuccessCount )}" );
            }

            if ( result.IssuesCount > 0 )
            {
                clauses.Add( $"{result.IssuesCount} {PersonWord( result.IssuesCount )} had changes that could not be applied" );
            }

            if ( result.FailedCount > 0 )
            {
                clauses.Add( $"{result.FailedCount} {PersonWord( result.FailedCount )} could not be updated" );
            }

            // Capitalize the first word so the joined sentence starts cleanly.
            clauses[0] = char.ToUpper( clauses[0][0] ) + clauses[0].Substring( 1 );

            return $"{string.Join( "; ", clauses )}.{elapsedSuffix}";
        }

        /// <summary>
        /// Returns "person" or "people" so the count reads with correct grammar.
        /// </summary>
        /// <param name="count">The number the noun must agree with.</param>
        /// <returns>"person" when <paramref name="count"/> is 1, otherwise "people".</returns>
        private static string PersonWord( int count )
        {
            return count == 1 ? "person" : "people";
        }

        /// <summary>
        /// Gets the group member attributes for the specified group.
        /// </summary>
        /// <param name="groupGuid">The unique identifier of the group.</param>
        /// <returns>A block action result containing a list of attribute options.</returns>
        [BlockAction]
        public BlockActionResult GetGroupMemberAttributes( Guid groupGuid )
        {
            if ( groupGuid == Guid.Empty )
            {
                return ActionBadRequest( "A valid group identifier is required." );
            }

            var group = new GroupService( RockContext ).Get( groupGuid );

            if ( group == null )
            {
                return ActionNotFound();
            }

            if ( !IsAuthorizedForGroup( group ) )
            {
                return ActionForbidden( "Not authorized to access this group." );
            }

            var attributes = GetBulkGroupMemberAttributes( group )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .Select( a => PublicAttributeHelper.GetPublicAttributeForEdit( a ) )
                .ToList();

            return ActionOk( attributes );
        }

        /// <summary>
        /// Gets the step attributes for the specified step type.
        /// </summary>
        /// <param name="stepTypeGuid">The unique identifier of the step type.</param>
        /// <returns>A block action result containing a list of attribute options.</returns>
        [BlockAction]
        public BlockActionResult GetStepAttributes( Guid stepTypeGuid )
        {
            if ( stepTypeGuid == Guid.Empty )
            {
                return ActionBadRequest( "A valid step type identifier is required." );
            }

            var stepType = StepTypeCache.Get( stepTypeGuid );

            if ( stepType == null )
            {
                return ActionNotFound();
            }

            if ( !IsAuthorizedForStepType( stepType ) )
            {
                return ActionForbidden( "Not authorized to access this step type." );
            }

            var attributes = GetBulkStepAttributes( stepType.Id )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .Select( a => PublicAttributeHelper.GetPublicAttributeForEdit( a ) )
                .ToList();

            return ActionOk( attributes );
        }

        #endregion Block Actions
    }
}

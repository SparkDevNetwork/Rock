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
using System.Data.Entity.Spatial;
using System.Linq;
using System.Linq.Expressions;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Engagement.SignUp.SignUpProjectDetail;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Engagement.SignUp
{
    /// <summary>
    /// Displays the details of a sign-up project (a group whose type is, or inherits from, the
    /// sign-up group type) and manages its opportunities.
    /// </summary>
    [DisplayName( "Sign-Up Project Detail" )]
    [Category( "Engagement > Sign-Up" )]
    [Description( "Displays details about the scheduled opportunities for a given project group." )]
    [IconCssClass( "ti ti-clipboard-text" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Sign-Up Opportunity Attendee List Page",
        Key = AttributeKey.SignUpOpportunityAttendeeListPage,
        Description = "Page used for viewing all the group members for the selected sign-up opportunity. If set, a view attendees button will show for each opportunity.",
        IsRequired = false,
        Order = 0 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "7D91B66B-FC19-4D2B-941B-DA423B71891D" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "24D3E8EA-3720-42B2-B9E2-20DF33405D0B" )]
    [Rock.SystemGuid.BlockTypeGuid( "69F5C6BD-7A22-42FE-8285-7C8E586E746A" )]
    public class SignUpProjectDetail : RockEntityDetailBlockType<Rock.Model.Group, SignUpProjectDetailBag>, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string ExpandedIds = "ExpandedIds";
            public const string GroupId = "GroupId";
            public const string LocationId = "LocationId";
            public const string ParentGroupId = "ParentGroupId";
            public const string ScheduleId = "ScheduleId";
        }

        private static class AttributeKey
        {
            public const string SignUpOpportunityAttendeeListPage = "SignUpOpportunityAttendeeListPage";
        }

        /// <summary>
        /// The keys of the group attributes this block handles explicitly.
        /// </summary>
        private static class GroupAttributeKey
        {
            public const string ProjectType = "ProjectType";
        }

        private static class NavigationUrlKey
        {
            public const string SignUpOpportunityAttendeeListPage = "SignUpOpportunityAttendeeListPage";
        }

        /// <summary>
        /// The messages displayed when the block cannot show or save a project.
        /// </summary>
        private static class ValidationMessage
        {
            public const string GroupNotFound = "The selected group does not exist or it has been archived.";
            public const string InvalidGroupType = "The selected group is not of a type that can be edited as a sign-up group.";
            public const string ParentGroupNotFound = "The selected parent group does not exist or it has been archived.";
            public const string ScheduleRequired = "Schedule is required.";
            public const string LocationRequired = "Location is required.";
            public const string CampusRequired = "Campus is required.";
            public const string ProjectTypeRequired = "Project Type is required.";
            public const string ProjectTypeInvalid = "Project Type is invalid.";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<SignUpProjectDetailBag, SignUpProjectDetailOptionsBag>();
            var entity = GetInitialEntity();

            SetBoxInitialEntityState( box, entity );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( entity );

            return box;
        }

        /// <inheritdoc/>
        protected override Rock.Model.Group GetInitialEntity()
        {
            var entity = GetInitialEntity<Rock.Model.Group, GroupService>( RockContext, PageParameterKey.GroupId );

            ApplyNewProjectDefaults( entity, new GroupService( RockContext ) );

            return entity;
        }

        /// <summary>
        /// Applies the default values of a project that is being added: the parent group, the
        /// values copied from the parent group and the group type when the parent group leaves
        /// no choice. Existing projects are left untouched.
        /// </summary>
        /// <param name="entity">The project being added.</param>
        /// <param name="groupService">The service used to load the parent group.</param>
        private void ApplyNewProjectDefaults( Rock.Model.Group entity, GroupService groupService )
        {
            if ( entity == null || entity.Id != 0 )
            {
                return;
            }

            var parentGroup = GetParentGroupForNewProject( groupService );
            if ( parentGroup == null )
            {
                return;
            }

            entity.ParentGroup = parentGroup;
            entity.ParentGroupId = parentGroup.Id;
            entity.CampusId = parentGroup.CampusId;
            entity.ReminderSystemCommunicationId = parentGroup.ReminderSystemCommunicationId;
            entity.ReminderOffsetDays = parentGroup.ReminderOffsetDays;
            entity.ReminderAdditionalDetails = parentGroup.ReminderAdditionalDetails;
            entity.ConfirmationAdditionalDetails = parentGroup.ConfirmationAdditionalDetails;

            // Keep the parent's group type when the parent allows it as a child type. Otherwise
            // a single allowed type is selected automatically and several allowed types leave
            // the choice to the person.
            var allowedGroupTypes = GetAllowedGroupTypes( parentGroup );
            if ( allowedGroupTypes.Any( gt => gt.Id == parentGroup.GroupTypeId ) )
            {
                entity.GroupTypeId = parentGroup.GroupTypeId;
            }
            else if ( allowedGroupTypes.Count == 1 )
            {
                entity.GroupTypeId = allowedGroupTypes[0].Id;
            }

            // A new project starts with the project type of its parent.
            parentGroup.LoadAttributes( RockContext );
            entity.LoadAttributes( RockContext );
            entity.SetAttributeValue( GroupAttributeKey.ProjectType, parentGroup.GetAttributeValue( GroupAttributeKey.ProjectType ) );
        }

        /// <summary>
        /// Gets the parent group of a project that is being added: the group identified by the
        /// ParentGroupId page parameter, or the root sign-up groups group when that parameter is
        /// missing or does not identify a group.
        /// </summary>
        /// <param name="groupService">The service used to load the parent group.</param>
        /// <returns>The parent group, or <c>null</c> when neither group exists.</returns>
        private Rock.Model.Group GetParentGroupForNewProject( GroupService groupService )
        {
            var parentGroupKey = PageParameter( PageParameterKey.ParentGroupId );
            var parentGroup = parentGroupKey.IsNotNullOrWhiteSpace()
                ? groupService.Get( parentGroupKey, !PageCache.Layout.Site.DisablePredictableIds )
                : null;

            return parentGroup ?? groupService.Get( Rock.SystemGuid.Group.GROUP_SIGNUP_GROUPS.AsGuid() );
        }

        /// <summary>
        /// Gets the sign-up group types the parent group allows as child group types, ordered by
        /// name.
        /// </summary>
        /// <param name="parentGroup">The parent group.</param>
        /// <returns>The allowed sign-up group types.</returns>
        private List<GroupTypeCache> GetAllowedGroupTypes( Rock.Model.Group parentGroup )
        {
            var parentGroupType = parentGroup != null ? GroupTypeCache.Get( parentGroup.GroupTypeId ) : null;
            if ( parentGroupType == null )
            {
                return new List<GroupTypeCache>();
            }

            return parentGroupType.ChildGroupTypes
                .Where( gt => SignUpOpportunityHelper.IsSignUpGroupType( gt.Id ) )
                .OrderBy( gt => gt.Name )
                .ToList();
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or ErrorMessage
        /// properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="entity">The project to be displayed.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<SignUpProjectDetailBag, SignUpProjectDetailOptionsBag> box, Rock.Model.Group entity )
        {
            if ( entity == null )
            {
                box.ErrorMessage = ValidationMessage.GroupNotFound;
                return;
            }

            if ( entity.Id != 0 )
            {
                if ( !SignUpOpportunityHelper.IsSignUpGroupType( entity.GroupTypeId ) )
                {
                    box.ErrorMessage = ValidationMessage.InvalidGroupType;
                }
                else if ( !entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( Rock.Model.Group.FriendlyTypeName );
                }
                else
                {
                    box.Entity = GetEntityBagForView( entity );
                    box.IsEditable = IsAuthorizedToEdit( entity );
                }
            }
            else if ( entity.ParentGroup == null )
            {
                box.ErrorMessage = ValidationMessage.ParentGroupNotFound;
            }
            else if ( !IsAuthorizedToEdit( entity ) )
            {
                box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Rock.Model.Group.FriendlyTypeName );
            }
            else
            {
                // A project that is being added opens in edit mode.
                box.Entity = GetEntityBagForEdit( entity );
                box.IsEditable = true;
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the box options required for the component to render the view.
        /// </summary>
        /// <param name="entity">The project to be displayed.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private SignUpProjectDetailOptionsBag GetBoxOptions( Rock.Model.Group entity )
        {
            var options = new SignUpProjectDetailOptionsBag
            {
                ProjectTypes = GetProjectTypeOptions(),
                ReminderSystemCommunications = GetReminderSystemCommunicationOptions(),
                GroupTypeOptions = BuildGroupTypeOptions( entity?.GroupTypeId ?? 0 )
            };

            var isNewProject = entity != null && entity.Id == 0 && entity.ParentGroup != null;
            if ( isNewProject )
            {
                var allowedGroupTypes = GetAllowedGroupTypes( entity.ParentGroup );

                options.AllowedGroupTypes = allowedGroupTypes
                    .Select( gt => new ListItemBag { Value = gt.Id.ToString(), Text = gt.Name } )
                    .ToList();

                if ( !allowedGroupTypes.Any() )
                {
                    options.AllowedGroupTypesWarning = $"The {entity.ParentGroup.Name} group does not allow any sign-up group types as child groups.";
                }

                options.AddModeCancelUrl = GetParentGroupPageUrl( entity.ParentGroup.Id );
            }

            return options;
        }

        /// <summary>
        /// Gets the project type defined values as options for the project type radio button
        /// list. The value of each option is the defined value unique identifier, which is how
        /// the project type is stored in the group attribute.
        /// </summary>
        /// <returns>The project type options.</returns>
        private List<ListItemBag> GetProjectTypeOptions()
        {
            var projectTypeDefinedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PROJECT_TYPE.AsGuid() );

            return ( projectTypeDefinedType?.DefinedValues ?? new List<DefinedValueCache>() )
                .ToListItemBagList();
        }

        /// <summary>
        /// Gets the system communications of the sign-up group confirmation category as options
        /// for the reminder communication drop down list.
        /// </summary>
        /// <returns>The reminder system communication options.</returns>
        private List<ListItemBag> GetReminderSystemCommunicationOptions()
        {
            var categoryId = CategoryCache.GetId( Rock.SystemGuid.Category.SYSTEM_COMMUNICATION_SIGNUP_GROUP_CONFIRMATION.AsGuid() );
            if ( !categoryId.HasValue )
            {
                return new List<ListItemBag>();
            }

            var signUpCategoryId = categoryId.Value;

            return new SystemCommunicationService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( c => c.CategoryId == signUpCategoryId )
                .OrderBy( c => c.Title )
                .Select( c => new { c.Guid, c.Title } )
                .ToList()
                .Select( c => new ListItemBag { Value = c.Guid.ToString(), Text = c.Title } )
                .ToList();
        }

        /// <summary>
        /// Gets the URL of this page for the specified parent group, keeping the expanded
        /// identifiers of the group tree so it keeps its state. Used when cancelling the addition
        /// of a project and after deleting a project.
        /// </summary>
        /// <param name="parentGroupId">The identifier of the parent group to display, or <c>null</c> to display no group.</param>
        /// <returns>The URL of this page.</returns>
        private string GetParentGroupPageUrl( int? parentGroupId )
        {
            var queryParams = new Dictionary<string, string>();

            if ( parentGroupId.HasValue && parentGroupId.Value > 0 )
            {
                queryParams[PageParameterKey.GroupId] = IdHasher.Instance.GetHash( parentGroupId.Value );
            }

            var expandedIds = PageParameter( PageParameterKey.ExpandedIds );
            if ( expandedIds.IsNotNullOrWhiteSpace() )
            {
                queryParams[PageParameterKey.ExpandedIds] = expandedIds;
            }

            // Existing parameters are skipped so the stale group identifier of the current URL
            // does not reload the current project.
            return this.GetCurrentPageUrl( queryParams, skipExistingParameters: true );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var urls = new Dictionary<string, string>();

            if ( GetAttributeValue( AttributeKey.SignUpOpportunityAttendeeListPage ).IsNotNullOrWhiteSpace() )
            {
                urls[NavigationUrlKey.SignUpOpportunityAttendeeListPage] = this.GetLinkedPageUrl( AttributeKey.SignUpOpportunityAttendeeListPage, new Dictionary<string, string>
                {
                    { PageParameterKey.GroupId, "((GroupId))" },
                    { PageParameterKey.LocationId, "((LocationId))" },
                    { PageParameterKey.ScheduleId, "((ScheduleId))" }
                } );
            }

            return urls;
        }

        /// <summary>
        /// Builds the options that depend on the specified group type.
        /// </summary>
        /// <param name="groupTypeId">The identifier of the group type.</param>
        /// <returns>The group type options, empty when the group type does not exist.</returns>
        private SignUpProjectGroupTypeOptionsBag BuildGroupTypeOptions( int groupTypeId )
        {
            var options = new SignUpProjectGroupTypeOptionsBag
            {
                InheritedMemberAttributes = new List<SignUpProjectInheritedMemberAttributeBag>(),
                InheritedGroupRequirements = new List<SignUpProjectInheritedRequirementBag>(),
                GroupRoleOptions = new List<ListItemBag>()
            };

            var groupType = groupTypeId > 0 ? GroupTypeCache.Get( groupTypeId ) : null;
            if ( groupType == null )
            {
                return options;
            }

            options.GroupTypeName = groupType.Name;
            options.GroupTypeUrl = GetGroupTypeUrl( groupType );
            options.RequiresCampus = groupType.GroupsRequireCampus;
            options.IsRecordSourceVisible = groupType.AllowGroupSpecificRecordSource;
            options.AllowSpecificGroupMemberAttributes = groupType.AllowSpecificGroupMemberAttributes;
            options.EnableSpecificGroupRequirements = groupType.EnableSpecificGroupRequirements;
            options.AllowedScheduleTypes = groupType.AllowedScheduleTypes;
            options.LocationSelectionMode = groupType.LocationSelectionMode;
            options.InheritedMemberAttributes = BuildInheritedMemberAttributes( groupType );
            options.InheritedGroupRequirements = BuildInheritedGroupRequirements( groupType );
            options.GroupRoleOptions = ( groupType.Roles ?? new List<GroupTypeRoleCache>() )
                .OrderBy( r => r.Order )
                .ThenBy( r => r.Name )
                .ToListItemBagList();

            return options;
        }

        /// <summary>
        /// Gets the URL of the detail page of the specified group type,
        /// preferring the GroupType entity type's <c>LinkUrlLavaTemplate</c>
        /// (the admin-set path) and falling back to <c>~/GroupType/{id}</c>.
        /// </summary>
        /// <param name="groupType">The group type.</param>
        /// <returns>The URL of the group type detail page.</returns>
        private string GetGroupTypeUrl( GroupTypeCache groupType )
        {
            var urlTemplate = EntityTypeCache.Get( typeof( GroupType ) )?.LinkUrlLavaTemplate;

            if ( urlTemplate.IsNotNullOrWhiteSpace() )
            {
                var url = urlTemplate.ResolveMergeFields( new Dictionary<string, object>
                {
                    ["Entity"] = groupType
                } );

                if ( url.IsNotNullOrWhiteSpace() )
                {
                    return RequestContext.ResolveRockUrl( url );
                }
            }

            return RequestContext.ResolveRockUrl( $"~/GroupType/{groupType.Id}" );
        }

        /// <summary>
        /// Gets the specified group type followed by each group type it inherits from, protected
        /// against inheritance cycles.
        /// </summary>
        /// <param name="groupType">The group type to start from.</param>
        /// <returns>The group type inheritance chain.</returns>
        private List<GroupTypeCache> GetGroupTypeInheritanceChain( GroupTypeCache groupType )
        {
            var chain = new List<GroupTypeCache>();
            var current = groupType;
            if ( current == null )
            {
                return chain;
            }

            var visitedGroupTypeIds = new HashSet<int>();
            do
            {
                if ( !visitedGroupTypeIds.Add( current.Id ) )
                {
                    break;
                }

                chain.Add( current );

                current = current.InheritedGroupTypeId.HasValue
                    ? GroupTypeCache.Get( current.InheritedGroupTypeId.Value )
                    : null;
            } while ( current != null );

            return chain;
        }

        /// <summary>
        /// Builds the member attributes defined on the specified group type or on any group type
        /// it inherits from. Members of a project inherit all of them.
        /// </summary>
        /// <param name="groupType">The group type of the project.</param>
        /// <returns>The inherited member attributes, in inheritance order.</returns>
        private List<SignUpProjectInheritedMemberAttributeBag> BuildInheritedMemberAttributes( GroupTypeCache groupType )
        {
            var inheritedAttributes = new List<SignUpProjectInheritedMemberAttributeBag>();
            var groupTypeChain = GetGroupTypeInheritanceChain( groupType );

            if ( !groupTypeChain.Any() )
            {
                return inheritedAttributes;
            }

            // One query for the whole inheritance chain, grouped per group type below in chain order.
            var qualifierValues = groupTypeChain.Select( gt => gt.Id.ToString() ).ToList();
            var attributes = new AttributeService( RockContext ).GetByEntityTypeId( new GroupMember().TypeId, false )
                .AsNoTracking()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "GroupTypeId", StringComparison.OrdinalIgnoreCase )
                    && qualifierValues.Contains( a.EntityTypeQualifierValue ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .Select( a => new
                {
                    a.Name,
                    a.Description,
                    a.Key,
                    a.Guid,
                    a.EntityTypeQualifierValue
                } )
                .ToList();

            foreach ( var inheritedGroupType in groupTypeChain )
            {
                var qualifierValue = inheritedGroupType.Id.ToString();
                var inheritedGroupTypeUrl = GetGroupTypeUrl( inheritedGroupType );

                inheritedAttributes.AddRange( attributes
                    .Where( a => a.EntityTypeQualifierValue == qualifierValue )
                    .Select( a => new SignUpProjectInheritedMemberAttributeBag
                    {
                        Name = a.Name,
                        Description = a.Description,
                        Key = a.Key,
                        Guid = a.Guid,
                        InheritedFromGroupTypeName = inheritedGroupType.Name,
                        InheritedFromGroupTypeUrl = inheritedGroupTypeUrl
                    } ) );
            }

            return inheritedAttributes;
        }

        /// <summary>
        /// Builds the group requirements defined on the specified group type. Only the
        /// requirements of the project's own group type apply to its members, so group types it
        /// inherits from are not included.
        /// </summary>
        /// <param name="groupType">The group type of the project.</param>
        /// <returns>The group type requirements, ordered by name.</returns>
        private List<SignUpProjectInheritedRequirementBag> BuildInheritedGroupRequirements( GroupTypeCache groupType )
        {
            var groupTypeId = groupType.Id;
            var groupTypeUrl = GetGroupTypeUrl( groupType );

            return new GroupRequirementService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Include( r => r.GroupRequirementType )
                .Include( r => r.GroupRole )
                .Where( r => r.GroupTypeId.HasValue && r.GroupTypeId.Value == groupTypeId )
                .ToList()
                .Select( r => new SignUpProjectInheritedRequirementBag
                {
                    Guid = r.Guid,
                    Name = r.GroupRequirementType?.Name ?? string.Empty,
                    GroupRoleName = r.GroupRole?.Name ?? string.Empty,
                    AppliesToAgeClassification = r.AppliesToAgeClassification,
                    MustMeetRequirementToAddMember = r.MustMeetRequirementToAddMember,
                    InheritedFromGroupTypeName = groupType.Name,
                    InheritedFromGroupTypeUrl = groupTypeUrl
                } )
                .OrderBy( r => r.Name )
                .ToList();
        }

        /// <summary>
        /// Builds the group requirement type options for the group requirement modal. Each option
        /// carries the type's due date type so the modal can display the matching due date field.
        /// </summary>
        /// <returns>The group requirement type options, ordered by name.</returns>
        private List<SignUpProjectRequirementTypeBag> BuildGroupRequirementTypeOptions()
        {
            return new GroupRequirementTypeService( RockContext )
                .Queryable()
                .AsNoTracking()
                .OrderBy( t => t.Name )
                .Select( t => new { t.Guid, t.Name, t.DueDateType } )
                .ToList()
                .Select( t => new SignUpProjectRequirementTypeBag
                {
                    Value = t.Guid.ToString(),
                    Text = t.Name,
                    DueDateType = t.DueDateType
                } )
                .ToList();
        }

        /// <summary>
        /// Builds the options of the due date group attribute drop down list of the group
        /// requirement modal: every date or date time attribute of groups of the specified type.
        /// </summary>
        /// <param name="groupTypeId">The identifier of the project's group type.</param>
        /// <returns>The date attribute options.</returns>
        private List<ListItemBag> BuildGroupDateAttributeOptions( int? groupTypeId )
        {
            if ( !groupTypeId.HasValue || groupTypeId.Value <= 0 )
            {
                return new List<ListItemBag>();
            }

            var dateFieldTypeIds = new HashSet<int>();
            var dateFieldTypeId = FieldTypeCache.GetId( Rock.SystemGuid.FieldType.DATE.AsGuid() );
            var dateTimeFieldTypeId = FieldTypeCache.GetId( Rock.SystemGuid.FieldType.DATE_TIME.AsGuid() );

            if ( dateFieldTypeId.HasValue )
            {
                dateFieldTypeIds.Add( dateFieldTypeId.Value );
            }

            if ( dateTimeFieldTypeId.HasValue )
            {
                dateFieldTypeIds.Add( dateTimeFieldTypeId.Value );
            }

            // Loading the attributes of a group of this type includes the attributes inherited
            // from the group types it inherits from.
            var group = new Rock.Model.Group { GroupTypeId = groupTypeId.Value };
            group.LoadAttributes( RockContext );

            return group.Attributes.Values
                .Where( a => dateFieldTypeIds.Contains( a.FieldTypeId ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .Select( a => new ListItemBag { Value = a.Guid.ToString(), Text = a.Name } )
                .ToList();
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The project to be represented as a bag.</param>
        /// <returns>A <see cref="SignUpProjectDetailBag"/> that represents the project.</returns>
        private SignUpProjectDetailBag GetCommonEntityBag( Rock.Model.Group entity )
        {
            if ( entity.Attributes == null )
            {
                entity.LoadAttributes( RockContext );
            }

            var groupType = entity.GroupTypeId > 0 ? GroupTypeCache.Get( entity.GroupTypeId ) : null;
            var campus = entity.CampusId.HasValue ? CampusCache.Get( entity.CampusId.Value ) : null;
            var projectType = GetProjectType( entity );

            return new SignUpProjectDetailBag
            {
                IdKey = entity.IdKey,
                Name = entity.Name,
                Description = entity.Description,
                IsActive = entity.IsActive,
                IsSystem = entity.IsSystem,
                CanEdit = IsAuthorizedToEdit( entity ),
                CanAdministrate = entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ),
                CanSchedule = IsAuthorizedToSchedule( entity ),
                GroupTypeId = entity.GroupTypeId > 0 ? entity.GroupTypeId : ( int? ) null,
                GroupTypeName = groupType?.Name,
                Campus = campus?.ToListItemBag(),
                CampusName = campus?.Name,
                ProjectTypeValue = projectType?.Guid.ToString(),
                ProjectTypeName = projectType?.Value
            };
        }

        /// <inheritdoc/>
        protected override SignUpProjectDetailBag GetEntityBagForView( Rock.Model.Group entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.RequirementSummaries = GetRequirementSummaries( entity );
            bag.Opportunities = GetOpportunities( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        /// <inheritdoc/>
        protected override SignUpProjectDetailBag GetEntityBagForEdit( Rock.Model.Group entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );
            var projectTypeAttribute = entity.Attributes.GetValueOrNull( GroupAttributeKey.ProjectType );

            bag.IsProjectTypeVisible = projectTypeAttribute != null;
            bag.IsProjectTypeRequired = projectTypeAttribute?.IsRequired ?? false;
            bag.ProjectTypeHelpText = projectTypeAttribute?.Description;
            bag.GroupMemberRecordSource = entity.GroupMemberRecordSourceValueId.HasValue
                ? DefinedValueCache.Get( entity.GroupMemberRecordSourceValueId.Value )?.ToListItemBag()
                : null;
            bag.ReminderSystemCommunication = GetSystemCommunicationListItem( entity.ReminderSystemCommunicationId );
            bag.ReminderOffsetDays = entity.ReminderOffsetDays;
            bag.ReminderAdditionalDetails = entity.ReminderAdditionalDetails;
            bag.ConfirmationAdditionalDetails = entity.ConfirmationAdditionalDetails;

            /*
                9/9/26 - MSE

                The "Project Type" attribute is excluded from the attribute values
                container and edited through its own radio button list. Its value
                decides whether the reminder fields are shown, and the defined value
                field type cannot render as radio buttons. Every other group attribute
                is edited through the attribute values container.

                Reason: The project type drives the visibility of the reminder fields.
            */
            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true, attributeFilter: a => a.Key != GroupAttributeKey.ProjectType );

            bag.MemberAttributes = LoadMemberAttributes( entity, EntityTypeCache.Get<GroupMember>().Id );
            bag.MemberOpportunityAttributes = LoadMemberAttributes( entity, EntityTypeCache.Get<GroupMemberAssignment>().Id );

            if ( bag.CanAdministrate )
            {
                bag.GroupRequirements = LoadGroupRequirements( entity );
            }

            return bag;
        }

        /// <summary>
        /// Gets the project type defined value selected for the project.
        /// </summary>
        /// <param name="entity">The project, with its attributes loaded.</param>
        /// <returns>The project type defined value, or <c>null</c> when none is selected.</returns>
        private DefinedValueCache GetProjectType( Rock.Model.Group entity )
        {
            var projectTypeGuid = entity.GetAttributeValue( GroupAttributeKey.ProjectType ).AsGuidOrNull();

            return projectTypeGuid.HasValue ? DefinedValueCache.Get( projectTypeGuid.Value ) : null;
        }

        /// <summary>
        /// Determines whether the specified project type attribute value is the in-person project
        /// type, which is the only project type that sends reminders.
        /// </summary>
        /// <param name="projectTypeValue">The project type attribute value.</param>
        /// <returns><c>true</c> if the project type is in-person; otherwise, <c>false</c>.</returns>
        private static bool GetIsProjectTypeInPerson( string projectTypeValue )
        {
            return projectTypeValue.AsGuidOrNull() == Rock.SystemGuid.DefinedValue.PROJECT_TYPE_IN_PERSON.AsGuid();
        }

        /// <summary>
        /// Gets the validation message for the project's Project Type attribute, or <c>null</c>
        /// when the value is acceptable. The attribute is edited outside the attribute values
        /// container, so it is not covered by <c>SetPublicAttributeValues</c>.
        /// </summary>
        /// <param name="entity">The project, with its attributes loaded.</param>
        /// <returns>The validation message, or <c>null</c>.</returns>
        private static string GetProjectTypeValidationMessage( Rock.Model.Group entity )
        {
            var projectTypeAttribute = entity.Attributes?.GetValueOrNull( GroupAttributeKey.ProjectType );
            if ( projectTypeAttribute == null )
            {
                return null;
            }

            var projectTypeValue = entity.GetAttributeValue( GroupAttributeKey.ProjectType );
            var isKnownProjectType = IsKnownProjectType( projectTypeValue );

            if ( projectTypeValue.IsNotNullOrWhiteSpace() && !isKnownProjectType )
            {
                return ValidationMessage.ProjectTypeInvalid;
            }

            if ( projectTypeAttribute.IsRequired && !isKnownProjectType )
            {
                return ValidationMessage.ProjectTypeRequired;
            }

            return null;
        }

        /// <summary>
        /// Determines whether the specified value is a defined value of the Project Type
        /// defined type.
        /// </summary>
        /// <param name="projectTypeValue">The project type attribute value.</param>
        /// <returns><c>true</c> if the value is a project type; otherwise, <c>false</c>.</returns>
        private static bool IsKnownProjectType( string projectTypeValue )
        {
            var projectTypeGuid = projectTypeValue.AsGuidOrNull();
            if ( !projectTypeGuid.HasValue )
            {
                return false;
            }

            return DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PROJECT_TYPE.AsGuid() )
                ?.DefinedValues?.Any( dv => dv.Guid == projectTypeGuid.Value ) == true;
        }

        /// <summary>
        /// Gets the system communication as a list item.
        /// </summary>
        /// <param name="systemCommunicationId">The identifier of the system communication.</param>
        /// <returns>The list item, or <c>null</c> when the system communication does not exist.</returns>
        private ListItemBag GetSystemCommunicationListItem( int? systemCommunicationId )
        {
            if ( !systemCommunicationId.HasValue )
            {
                return null;
            }

            var systemCommunication = new SystemCommunicationService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( c => c.Id == systemCommunicationId.Value )
                .Select( c => new { c.Guid, c.Title } )
                .FirstOrDefault();

            return systemCommunication != null
                ? new ListItemBag { Value = systemCommunication.Guid.ToString(), Text = systemCommunication.Title }
                : null;
        }

        /// <summary>
        /// Gets the summaries of the group requirements that apply to the project: those of its
        /// group type followed by its own, each ordered by requirement type name.
        /// </summary>
        /// <param name="entity">The project.</param>
        /// <returns>The requirement summaries.</returns>
        private List<SignUpProjectRequirementSummaryBag> GetRequirementSummaries( Rock.Model.Group entity )
        {
            var groupId = entity.Id;
            var groupTypeId = entity.GroupTypeId;

            return new GroupRequirementService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( r =>
                    ( r.GroupTypeId.HasValue && r.GroupTypeId.Value == groupTypeId )
                    || ( r.GroupId.HasValue && r.GroupId.Value == groupId ) )
                .Select( r => new
                {
                    IsGroupTypeRequirement = r.GroupTypeId.HasValue,
                    Name = r.GroupRequirementType.Name,
                    r.MustMeetRequirementToAddMember
                } )
                .ToList()
                .Where( r => r.Name.IsNotNullOrWhiteSpace() )
                .OrderByDescending( r => r.IsGroupTypeRequirement )
                .ThenBy( r => r.Name )
                .Select( r => new SignUpProjectRequirementSummaryBag
                {
                    Name = r.Name,
                    IsLoginRequiredToRegister = r.MustMeetRequirementToAddMember
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the opportunities (group location schedules) of the project along with their
        /// configuration and participant counts.
        /// </summary>
        /// <param name="entity">The project.</param>
        /// <returns>The project's opportunities.</returns>
        private List<SignUpOpportunityBag> GetOpportunities( Rock.Model.Group entity )
        {
            var groupId = entity.Id;

            var groupLocationSchedules = new GroupLocationService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gl => gl.GroupId == groupId )
                .SelectMany( gl => gl.Schedules, ( gl, s ) => new
                {
                    GroupLocationId = gl.Id,
                    gl.Location,
                    Schedule = s,
                    Config = gl.GroupLocationScheduleConfigs.FirstOrDefault( c => c.ScheduleId == s.Id )
                } )
                .ToList();

            // One query for the participant counts of all opportunities, matched to their
            // opportunity by location and schedule below.
            var participantCounts = new GroupMemberAssignmentService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gma =>
                    gma.GroupMember.GroupId == groupId
                    && !gma.GroupMember.Person.IsDeceased
                    && gma.LocationId.HasValue
                    && gma.ScheduleId.HasValue )
                .GroupBy( gma => new { gma.LocationId, gma.ScheduleId } )
                .Select( g => new
                {
                    LocationId = g.Key.LocationId.Value,
                    ScheduleId = g.Key.ScheduleId.Value,
                    Count = g.Count()
                } )
                .ToList()
                .ToDictionary( c => GetOpportunityKey( c.LocationId, c.ScheduleId ), c => c.Count );

            var now = RockDateTime.Now;

            return groupLocationSchedules
                .Select( gls =>
                {
                    var schedule = gls.Schedule;
                    var location = gls.Location;
                    var nextStartDateTime = schedule.NextStartDateTime;
                    var lastStartDateTime = nextStartDateTime.HasValue ? null : GetLastStartDateTime( schedule, now );
                    var locationPickerMode = Rock.Model.GroupType.GetGroupLocationPickerMode( location );

                    return new SignUpOpportunityBag
                    {
                        GroupLocationIdKey = IdHasher.Instance.GetHash( gls.GroupLocationId ),
                        LocationIdKey = location.IdKey,
                        ScheduleIdKey = schedule.IdKey,
                        Name = gls.Config?.ConfigurationName,
                        NextOrLastStartDateTime = ( nextStartDateTime ?? lastStartDateTime )?.ToRockDateTimeOffset(),
                        FriendlyDateTime = schedule.ToFriendlyScheduleText( true ),
                        FriendlyLocation = location.ToString( true ),
                        SlotsMinimum = gls.Config?.MinimumCapacity,
                        SlotsDesired = gls.Config?.DesiredCapacity,
                        SlotsMaximum = gls.Config?.MaximumCapacity,
                        SlotsFilled = participantCounts.GetValueOrDefault( GetOpportunityKey( location.Id, schedule.Id ), 0 ),
                        IsUpcoming = nextStartDateTime.HasValue && nextStartDateTime.Value >= now,
                        ScheduleType = schedule.ScheduleType,
                        ICalendarContent = schedule.ScheduleType == ScheduleType.Custom ? schedule.iCalendarContent : null,
                        NamedSchedule = schedule.ScheduleType == ScheduleType.Named ? schedule.ToListItemBag() : null,
                        LocationPickerMode = locationPickerMode,
                        Location = GetLocationPickerValue( location, locationPickerMode ),
                        ReminderAdditionalDetails = gls.Config?.ReminderAdditionalDetails,
                        ConfirmationAdditionalDetails = gls.Config?.ConfirmationAdditionalDetails
                    };
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the key that identifies an opportunity by its location and schedule.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <returns>The opportunity key.</returns>
        private static string GetOpportunityKey( int locationId, int scheduleId )
        {
            return $"{locationId}|{scheduleId}";
        }

        /// <summary>
        /// Gets the last start date time of a schedule that has no upcoming start date time, so
        /// past opportunities can be sorted by date.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        /// <param name="now">The current date time.</param>
        /// <returns>The last start date time, or <c>null</c> when it cannot be determined.</returns>
        private static DateTime? GetLastStartDateTime( Schedule schedule, DateTime now )
        {
            /*
                9/9/26 - MSE

                The occurrences are only enumerated from the schedule's effective start
                date up to now. Enumerating from now onward returns nothing for a schedule
                that has already ended, and enumerating to the end of time would never
                finish for an inactive schedule that recurs indefinitely.

                Reason: Give past opportunities a sortable date without an unbounded enumeration.
            */
            if ( !schedule.EffectiveStartDate.HasValue || schedule.EffectiveStartDate.Value >= now )
            {
                return null;
            }

            var lastStartDateTime = schedule.GetScheduledStartTimes( schedule.EffectiveStartDate.Value, now ).LastOrDefault();

            return lastStartDateTime != default ? lastStartDateTime : ( DateTime? ) null;
        }

        /// <summary>
        /// Gets the location in the shape the location picker expects for the specified picker
        /// mode: a list item for named locations, an address for address locations and Well-Known
        /// Text for points and polygons.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="locationPickerMode">The picker mode that matches the location.</param>
        /// <returns>The location picker value, or <c>null</c> when the location cannot be edited.</returns>
        private static object GetLocationPickerValue( Location location, GroupLocationPickerMode locationPickerMode )
        {
            switch ( locationPickerMode )
            {
                case GroupLocationPickerMode.Named:
                    return new ListItemBag { Value = location.Guid.ToString(), Text = location.ToString( true ) };

                case GroupLocationPickerMode.Address:
                    return new AddressControlBag
                    {
                        Street1 = location.Street1,
                        Street2 = location.Street2,
                        City = location.City,
                        State = location.State,
                        Locality = location.County,
                        PostalCode = location.PostalCode,
                        Country = location.Country
                    };

                case GroupLocationPickerMode.Point:
                    return location.GeoPoint?.AsText();

                case GroupLocationPickerMode.Polygon:
                    return location.GeoFence?.AsText();

                default:
                    return null;
            }
        }

        /// <summary>
        /// Resolves the location emitted by the location picker to a location entity, creating
        /// address, point and polygon locations that do not exist yet.
        /// </summary>
        /// <param name="locationValue">The location as emitted by the location picker.</param>
        /// <param name="locationPickerMode">The mode the location picker was in.</param>
        /// <param name="locationService">The service used to load or create the location.</param>
        /// <returns>The location, or <c>null</c> when the value does not describe a location.</returns>
        private static Location ResolveLocation( object locationValue, GroupLocationPickerMode locationPickerMode, LocationService locationService )
        {
            if ( locationValue == null )
            {
                return null;
            }

            switch ( locationPickerMode )
            {
                case GroupLocationPickerMode.Named:
                {
                    // The picker emits a list item whose value is the location's unique
                    // identifier. Round-trip through JSON so the payload shape does not
                    // depend on the serializer.
                    var listItem = locationValue.ToJson().FromJsonOrNull<ListItemBag>();
                    var locationGuid = listItem?.Value.AsGuidOrNull();

                    return locationGuid.HasValue ? locationService.Get( locationGuid.Value ) : null;
                }

                case GroupLocationPickerMode.Address:
                {
                    var address = locationValue.ToJson().FromJsonOrNull<AddressControlBag>();
                    if ( address == null || ( address.Street1.IsNullOrWhiteSpace() && address.City.IsNullOrWhiteSpace() ) )
                    {
                        return null;
                    }

                    var addressLocation = new Location
                    {
                        Street1 = address.Street1,
                        Street2 = address.Street2,
                        City = address.City,
                        State = address.State,
                        County = address.Locality,
                        PostalCode = address.PostalCode,
                        Country = address.Country
                    };

                    if ( !LocationService.ValidateLocationAddressRequirements( addressLocation, out _ ) )
                    {
                        return null;
                    }

                    return locationService.Get(
                        address.Street1,
                        address.Street2,
                        address.City,
                        address.State,
                        address.Locality,
                        address.PostalCode,
                        address.Country,
                        new GetLocationArgs
                        {
                            VerifyLocation = false,
                            ValidateLocation = false
                        } );
                }

                case GroupLocationPickerMode.Point:
                {
                    var wellKnownText = locationValue as string ?? locationValue.ToString();
                    if ( wellKnownText.IsNullOrWhiteSpace() )
                    {
                        return null;
                    }

                    DbGeography point;
                    try
                    {
                        point = DbGeography.FromText( wellKnownText );
                    }
                    catch
                    {
                        // Intentionally ignored: the picker emits invalid text for an
                        // empty or partial selection, which is treated as no location.
                        return null;
                    }

                    return locationService.GetByGeoPoint( point );
                }

                case GroupLocationPickerMode.Polygon:
                {
                    var wellKnownText = locationValue as string ?? locationValue.ToString();
                    if ( wellKnownText.IsNullOrWhiteSpace() )
                    {
                        return null;
                    }

                    DbGeography fence;
                    try
                    {
                        fence = DbGeography.PolygonFromText( wellKnownText, DbGeography.DefaultCoordinateSystemId );
                    }
                    catch
                    {
                        // Intentionally ignored: the picker emits invalid text for an
                        // empty or partial selection, which is treated as no location.
                        return null;
                    }

                    return locationService.GetByGeoFence( fence );
                }

                default:
                    return null;
            }
        }

        /// <summary>
        /// Loads the attributes of the specified entity type that are defined for the members of
        /// the project. New projects have none because the attributes are qualified by the
        /// project's identifier.
        /// </summary>
        /// <param name="entity">The project.</param>
        /// <param name="entityTypeId">The identifier of the entity type the attributes belong to.</param>
        /// <returns>The editable attributes, in display order.</returns>
        private List<PublicEditableAttributeBag> LoadMemberAttributes( Rock.Model.Group entity, int entityTypeId )
        {
            if ( entity.Id == 0 )
            {
                return new List<PublicEditableAttributeBag>();
            }

            var qualifierValue = entity.Id.ToString();

            return new AttributeService( RockContext ).GetByEntityTypeId( entityTypeId, true )
                .AsNoTracking()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "GroupId", StringComparison.OrdinalIgnoreCase )
                    && a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList()
                .ConvertAll( a => PublicAttributeHelper.GetPublicEditableAttribute( a ) );
        }

        /// <summary>
        /// Saves the attributes of the specified entity type that are defined for the members of
        /// the project. Attributes that were removed are deleted and the remaining attributes are
        /// ordered as displayed.
        /// </summary>
        /// <param name="entityTypeId">The identifier of the entity type the attributes belong to.</param>
        /// <param name="qualifierValue">The identifier of the project as a string.</param>
        /// <param name="attributes">The attributes as edited.</param>
        private void SaveMemberAttributes( int entityTypeId, string qualifierValue, List<PublicEditableAttributeBag> attributes )
        {
            if ( attributes == null )
            {
                return;
            }

            const string qualifierColumn = "GroupId";

            var attributeService = new AttributeService( RockContext );
            var existingAttributes = attributeService.GetByEntityTypeQualifier( entityTypeId, qualifierColumn, qualifierValue, true ).ToList();

            var remainingAttributeGuids = attributes.Select( a => a.Guid ).ToList();
            foreach ( var existingAttribute in existingAttributes.Where( a => !remainingAttributeGuids.Contains( a.Guid ) ) )
            {
                attributeService.Delete( existingAttribute );
                RockContext.SaveChanges();
            }

            // The incoming attributes are already sorted in the correct order.
            var attributeOrder = 0;
            foreach ( var attributeBag in attributes )
            {
                var attribute = Helper.SaveAttributeEdits( attributeBag, entityTypeId, qualifierColumn, qualifierValue, RockContext );
                if ( attribute != null )
                {
                    attribute.Order = attributeOrder++;
                }
            }
        }

        /// <summary>
        /// Loads the group requirements defined specifically for the project. New projects have
        /// none.
        /// </summary>
        /// <param name="entity">The project.</param>
        /// <returns>The project's group requirements, ordered by requirement type name.</returns>
        private List<SignUpProjectRequirementBag> LoadGroupRequirements( Rock.Model.Group entity )
        {
            if ( entity.Id == 0 )
            {
                return new List<SignUpProjectRequirementBag>();
            }

            var groupId = entity.Id;

            return new GroupRequirementService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Include( r => r.GroupRequirementType )
                .Include( r => r.GroupRole )
                .Include( r => r.AppliesToDataView )
                .Include( r => r.DueDateAttribute )
                .Where( r => r.GroupId.HasValue && r.GroupId.Value == groupId )
                .ToList()
                .Select( r => new SignUpProjectRequirementBag
                {
                    Guid = r.Guid,
                    GroupRequirementType = r.GroupRequirementType.ToListItemBag(),
                    Role = r.GroupRole != null ? new ListItemBag { Value = r.GroupRole.Guid.ToString(), Text = r.GroupRole.Name } : null,
                    AppliesToAgeClassification = r.AppliesToAgeClassification,
                    AppliesToDataView = r.AppliesToDataView.ToListItemBag(),
                    AllowLeadersToOverride = r.AllowLeadersToOverride,
                    MustMeetRequirementToAddMember = r.MustMeetRequirementToAddMember,
                    DueDateType = r.GroupRequirementType?.DueDateType ?? DueDateType.Immediate,
                    DueDateStaticDate = r.DueDateStaticDate?.ToRockDateTimeOffset(),
                    DueDateAttribute = r.DueDateAttribute != null
                        ? new ListItemBag { Value = r.DueDateAttribute.Guid.ToString(), Text = r.DueDateAttribute.Name }
                        : null
                } )
                .OrderBy( r => r.GroupRequirementType?.Text )
                .ToList();
        }

        /// <summary>
        /// Gets the message describing the first pair of group requirements that share the same
        /// requirement type and role, or <c>null</c> when there is no such pair.
        /// </summary>
        /// <param name="requirements">The group requirements as edited.</param>
        /// <returns>The duplicate message, or <c>null</c>.</returns>
        private static string GetDuplicateGroupRequirementMessage( List<SignUpProjectRequirementBag> requirements )
        {
            var duplicate = ( requirements ?? new List<SignUpProjectRequirementBag>() )
                .Where( r => r != null )
                .GroupBy( r => new { Type = r.GroupRequirementType?.Value, Role = r.Role?.Value } )
                .FirstOrDefault( g => g.Count() > 1 )
                ?.First();

            if ( duplicate == null )
            {
                return null;
            }

            var roleText = duplicate.Role != null ? $" for group role {duplicate.Role.Text}" : string.Empty;

            return $"This group already has a group requirement of {duplicate.GroupRequirementType?.Text}{roleText}.";
        }

        /// <summary>
        /// Saves the group requirements defined specifically for the project. Requirements that
        /// were removed are deleted, existing requirements are updated and new requirements are
        /// added.
        /// </summary>
        /// <param name="entity">The project, which must already have an identifier.</param>
        /// <param name="requirements">The group requirements as edited.</param>
        private void SaveGroupRequirements( Rock.Model.Group entity, List<SignUpProjectRequirementBag> requirements )
        {
            var groupRequirementService = new GroupRequirementService( RockContext );
            var groupId = entity.Id;

            var requirementBags = ( requirements ?? new List<SignUpProjectRequirementBag>() )
                .Where( r => r != null )
                .ToList();

            foreach ( var requirementBag in requirementBags.Where( r => r.Guid == Guid.Empty ) )
            {
                requirementBag.Guid = Guid.NewGuid();
            }

            var existingRequirements = groupRequirementService
                .Queryable()
                .Where( r => r.GroupId.HasValue && r.GroupId.Value == groupId )
                .ToList();

            var remainingGuids = requirementBags.Select( r => r.Guid ).ToHashSet();
            foreach ( var existingRequirement in existingRequirements.Where( r => !remainingGuids.Contains( r.Guid ) ).ToList() )
            {
                groupRequirementService.Delete( existingRequirement );
            }

            foreach ( var requirementBag in requirementBags )
            {
                var requirement = existingRequirements.FirstOrDefault( r => r.Guid == requirementBag.Guid );
                if ( requirement == null )
                {
                    requirement = new GroupRequirement
                    {
                        Guid = requirementBag.Guid,
                        GroupId = groupId
                    };

                    groupRequirementService.Add( requirement );
                }

                requirement.GroupRequirementTypeId = requirementBag.GroupRequirementType?.GetEntityId<GroupRequirementType>( RockContext ) ?? 0;
                requirement.GroupRoleId = requirementBag.Role?.GetEntityId<GroupTypeRole>( RockContext );
                requirement.MustMeetRequirementToAddMember = requirementBag.MustMeetRequirementToAddMember;
                requirement.AppliesToAgeClassification = requirementBag.AppliesToAgeClassification;
                requirement.AppliesToDataViewId = requirementBag.AppliesToDataView?.GetEntityId<DataView>( RockContext );
                requirement.AllowLeadersToOverride = requirementBag.AllowLeadersToOverride;
                requirement.DueDateStaticDate = requirementBag.DueDateType == DueDateType.ConfiguredDate
                    ? requirementBag.DueDateStaticDate?.DateTime
                    : null;
                requirement.DueDateAttributeId = requirementBag.DueDateType == DueDateType.GroupAttribute
                    ? requirementBag.DueDateAttribute?.GetEntityId<Rock.Model.Attribute>( RockContext )
                    : null;
            }
        }

        /// <summary>
        /// Determines whether the current person is authorized to edit and delete the project.
        /// A project that is being added and has no group type yet inherits nothing from a group
        /// type, so the group types the parent allows are probed instead.
        /// </summary>
        /// <param name="entity">The project.</param>
        /// <returns><c>true</c> if the current person may edit the project; otherwise, <c>false</c>.</returns>
        private bool IsAuthorizedToEdit( Rock.Model.Group entity )
        {
            if ( entity == null || entity.IsSystem )
            {
                return false;
            }

            if ( entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return true;
            }

            var isNewProjectAwaitingGroupType = entity.Id == 0
                && entity.GroupTypeId <= 0
                && entity.ParentGroup != null;

            if ( !isNewProjectAwaitingGroupType )
            {
                return false;
            }

            var originalGroupTypeId = entity.GroupTypeId;

            try
            {
                foreach ( var allowedGroupType in GetAllowedGroupTypes( entity.ParentGroup ) )
                {
                    entity.GroupTypeId = allowedGroupType.Id;

                    if ( entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                    {
                        return true;
                    }
                }

                return false;
            }
            finally
            {
                entity.GroupTypeId = originalGroupTypeId;
            }
        }

        /// <summary>
        /// Determines whether the current person is authorized to add, edit and delete the
        /// project's opportunities. Because sign-ups are a special usage of groups, people with
        /// schedule authorization may manage opportunities.
        /// </summary>
        /// <param name="entity">The project.</param>
        /// <returns><c>true</c> if the current person may manage opportunities; otherwise, <c>false</c>.</returns>
        private bool IsAuthorizedToSchedule( Rock.Model.Group entity )
        {
            return entity != null
                && ( entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
                    || entity.IsAuthorized( Authorization.SCHEDULE, RequestContext.CurrentPerson ) );
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out Rock.Model.Group entity, out BlockActionResult error )
        {
            var groupService = new GroupService( RockContext );
            error = null;

            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                entity = groupService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    error = ActionBadRequest( ValidationMessage.GroupNotFound );
                    return false;
                }

                if ( !SignUpOpportunityHelper.IsSignUpGroupType( entity.GroupTypeId ) )
                {
                    error = ActionBadRequest( ValidationMessage.InvalidGroupType );
                    return false;
                }
            }
            else
            {
                entity = new Rock.Model.Group();
                groupService.Add( entity );

                ApplyNewProjectDefaults( entity, groupService );

                if ( entity.ParentGroup == null )
                {
                    error = ActionBadRequest( ValidationMessage.ParentGroupNotFound );
                    return false;
                }
            }

            if ( !IsAuthorizedToEdit( entity ) )
            {
                error = ActionBadRequest( $"Not authorized to edit {Rock.Model.Group.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( Rock.Model.Group entity, ValidPropertiesBox<SignUpProjectDetailBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            // The group type can only be chosen while adding a project.
            if ( entity.Id == 0 )
            {
                box.IfValidProperty( nameof( box.Bag.GroupTypeId ), () =>
                {
                    if ( box.Bag.GroupTypeId > 0 )
                    {
                        entity.GroupTypeId = box.Bag.GroupTypeId.Value;
                    }
                } );
            }

            var groupType = entity.GroupTypeId > 0 ? GroupTypeCache.Get( entity.GroupTypeId ) : null;

            box.IfValidProperty( nameof( box.Bag.Campus ), () =>
            {
                var campusGuid = box.Bag.Campus?.Value.AsGuidOrNull();
                entity.CampusId = campusGuid.HasValue ? CampusCache.Get( campusGuid.Value )?.Id : null;

                // When a campus is required but none was selected, the picker was hidden
                // because there is only one campus, so use that campus.
                if ( !entity.CampusId.HasValue && groupType?.GroupsRequireCampus == true )
                {
                    entity.CampusId = CampusCache.SingleCampusId;
                }
            } );

            box.IfValidProperty( nameof( box.Bag.GroupMemberRecordSource ), () =>
            {
                var recordSourceGuid = box.Bag.GroupMemberRecordSource?.Value.AsGuidOrNull();
                entity.GroupMemberRecordSourceValueId = groupType?.AllowGroupSpecificRecordSource == true && recordSourceGuid.HasValue
                    ? DefinedValueCache.Get( recordSourceGuid.Value )?.Id
                    : null;
            } );

            // The attributes depend on the group type, so they are loaded after it is set.
            entity.LoadAttributes( RockContext );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ), () =>
            {
                if ( box.Bag.AttributeValues != null )
                {
                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
                }
            } );

            var hasProjectTypeAttribute = entity.Attributes.ContainsKey( GroupAttributeKey.ProjectType );

            if ( hasProjectTypeAttribute )
            {
                box.IfValidProperty( nameof( box.Bag.ProjectTypeValue ),
                    () => entity.SetAttributeValue( GroupAttributeKey.ProjectType, box.Bag.ProjectTypeValue ) );
            }

            // Reminders are only sent for in-person projects.
            var isProjectTypeInPerson = hasProjectTypeAttribute && GetIsProjectTypeInPerson( entity.GetAttributeValue( GroupAttributeKey.ProjectType ) );

            if ( isProjectTypeInPerson )
            {
                box.IfValidProperty( nameof( box.Bag.ReminderSystemCommunication ),
                    () => entity.ReminderSystemCommunicationId = box.Bag.ReminderSystemCommunication?.GetEntityId<SystemCommunication>( RockContext ) );

                box.IfValidProperty( nameof( box.Bag.ReminderOffsetDays ),
                    () => entity.ReminderOffsetDays = box.Bag.ReminderOffsetDays );

                box.IfValidProperty( nameof( box.Bag.ReminderAdditionalDetails ),
                    () => entity.ReminderAdditionalDetails = box.Bag.ReminderAdditionalDetails );
            }
            else
            {
                entity.ReminderSystemCommunicationId = null;
                entity.ReminderOffsetDays = null;
                entity.ReminderAdditionalDetails = null;
            }

            box.IfValidProperty( nameof( box.Bag.ConfirmationAdditionalDetails ),
                () => entity.ConfirmationAdditionalDetails = box.Bag.ConfirmationAdditionalDetails );

            return true;
        }

        /// <summary>
        /// Gets the sign-up project whose opportunities are being managed, verifying that it is a
        /// sign-up project and that the current person may manage its opportunities.
        /// </summary>
        /// <param name="key">The identifier key of the project.</param>
        /// <param name="error">Contains the action error result when <c>null</c> is returned.</param>
        /// <returns>The project, or <c>null</c> when it cannot be managed.</returns>
        private Rock.Model.Group GetProjectForOpportunityAction( string key, out BlockActionResult error )
        {
            error = null;

            var group = new GroupService( RockContext ).Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( group == null )
            {
                error = ActionBadRequest( ValidationMessage.GroupNotFound );
                return null;
            }

            if ( !SignUpOpportunityHelper.IsSignUpGroupType( group.GroupTypeId ) )
            {
                error = ActionBadRequest( ValidationMessage.InvalidGroupType );
                return null;
            }

            if ( !IsAuthorizedToSchedule( group ) )
            {
                error = ActionForbidden( "You are not authorized to edit the opportunities of this Sign-Up Project." );
                return null;
            }

            return group;
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var key = pageReference.GetPageParameter( PageParameterKey.GroupId );

            if ( key.IsNullOrWhiteSpace() )
            {
                return new BreadCrumbResult { BreadCrumbs = new List<IBreadCrumb>() };
            }

            var id = key.AsIntegerOrNull();
            var guid = key.AsGuidOrNull();
            var isAddPath = ( id.HasValue && id.Value == 0 )
                || ( guid.HasValue && guid.Value == Guid.Empty );

            if ( isAddPath )
            {
                var addCrumb = new BreadCrumbLink( "New Sign-Up Project", new PageReference( pageReference.PageId, 0 ) );
                return new BreadCrumbResult { BreadCrumbs = new List<IBreadCrumb> { addCrumb } };
            }

            var project = new GroupService( RockContext ).GetSelect( key, g => new { g.Name, g.Id }, !PageCache.Layout.Site.DisablePredictableIds );
            if ( project == null )
            {
                return new BreadCrumbResult { BreadCrumbs = new List<IBreadCrumb>() };
            }

            var pageParameters = new Dictionary<string, string>
            {
                [PageParameterKey.GroupId] = IdHasher.Instance.GetHash( project.Id )
            };
            var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageParameters );
            var breadCrumb = new BreadCrumbLink( project.Name, breadCrumbPageRef );

            return new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb> { breadCrumb }
            };
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the box that will contain all the information needed to begin the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the project to be edited.</param>
        /// <returns>A box that contains the project and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<SignUpProjectDetailBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Saves the project contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new project bag to be used when returning to view mode, or the URL to redirect to after creating a new project.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<SignUpProjectDetailBag> box )
        {
            if ( box?.Bag == null )
            {
                return ActionBadRequest( "Invalid request." );
            }

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var isNew = entity.Id == 0;

            var parentGroup = entity.ParentGroup;
            if ( parentGroup == null )
            {
                return ActionBadRequest( ValidationMessage.ParentGroupNotFound );
            }

            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // The group type chosen for a new project may change who is authorized to edit it.
            if ( !IsAuthorizedToEdit( entity ) )
            {
                return ActionBadRequest( EditModeMessage.NotAuthorizedToEdit( Rock.Model.Group.FriendlyTypeName ) );
            }

            if ( entity.GroupTypeId <= 0 )
            {
                return ActionBadRequest( "Group Type is required." );
            }

            var groupType = GroupTypeCache.Get( entity.GroupTypeId );
            if ( groupType == null || !SignUpOpportunityHelper.IsSignUpGroupType( groupType.Id ) )
            {
                return ActionBadRequest( ValidationMessage.InvalidGroupType );
            }

            if ( !GetAllowedGroupTypes( parentGroup ).Any( gt => gt.Id == groupType.Id ) )
            {
                return ActionBadRequest( $"The {parentGroup.Name} group does not allow child groups with a {groupType.Name} group type." );
            }

            // The campus picker enforces this on the client; enforce it here as well so a
            // direct request cannot save a project without a campus when the type requires one.
            if ( groupType.GroupsRequireCampus && !entity.CampusId.HasValue )
            {
                return ActionBadRequest( ValidationMessage.CampusRequired );
            }

            var projectTypeValidationMessage = GetProjectTypeValidationMessage( entity );
            if ( projectTypeValidationMessage.IsNotNullOrWhiteSpace() )
            {
                return ActionBadRequest( projectTypeValidationMessage );
            }

            if ( !entity.IsValid )
            {
                return ActionBadRequest( string.Join( " ", entity.ValidationResults.Select( r => r.ErrorMessage ) ) );
            }

            var canAdministrate = entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );
            var shouldSaveGroupRequirements = canAdministrate && box.IsValidProperty( nameof( box.Bag.GroupRequirements ) );

            if ( shouldSaveGroupRequirements )
            {
                var duplicateRequirementMessage = GetDuplicateGroupRequirementMessage( box.Bag.GroupRequirements );
                if ( duplicateRequirementMessage.IsNotNullOrWhiteSpace() )
                {
                    return ActionBadRequest( duplicateRequirementMessage );
                }
            }

            RockContext.WrapTransaction( () =>
            {
                // Save the project first so a new project has the identifier its requirements
                // and member attributes reference.
                RockContext.SaveChanges();

                entity.SaveAttributeValues( RockContext );

                if ( shouldSaveGroupRequirements )
                {
                    SaveGroupRequirements( entity, box.Bag.GroupRequirements );
                }

                var qualifierValue = entity.Id.ToString();

                box.IfValidProperty( nameof( box.Bag.MemberAttributes ),
                    () => SaveMemberAttributes( EntityTypeCache.Get<GroupMember>().Id, qualifierValue, box.Bag.MemberAttributes ) );

                box.IfValidProperty( nameof( box.Bag.MemberOpportunityAttributes ),
                    () => SaveMemberAttributes( EntityTypeCache.Get<GroupMemberAssignment>().Id, qualifierValue, box.Bag.MemberOpportunityAttributes ) );

                RockContext.SaveChanges();
            } );

            if ( isNew )
            {
                // Keep the expanded identifiers so the group tree keeps its state when the new
                // project is displayed.
                var queryParams = new Dictionary<string, string>
                {
                    [PageParameterKey.GroupId] = entity.IdKey
                };

                var expandedIds = PageParameter( PageParameterKey.ExpandedIds );
                if ( expandedIds.IsNotNullOrWhiteSpace() )
                {
                    queryParams[PageParameterKey.ExpandedIds] = expandedIds;
                }

                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( queryParams ) );
            }

            // Reload the attributes so the view reflects the saved values.
            entity.LoadAttributes( RockContext );

            var refreshedBag = GetEntityBagForView( entity );

            return ActionOk( new ValidPropertiesBox<SignUpProjectDetailBag>
            {
                Bag = refreshedBag,
                ValidProperties = refreshedBag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Deletes the specified project along with the custom schedules of its opportunities.
        /// </summary>
        /// <param name="key">The identifier of the project to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var groupService = new GroupService( RockContext );
            var entity = groupService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( ValidationMessage.GroupNotFound );
            }

            if ( !SignUpOpportunityHelper.IsSignUpGroupType( entity.GroupTypeId ) )
            {
                return ActionBadRequest( ValidationMessage.InvalidGroupType );
            }

            if ( !IsAuthorizedToEdit( entity ) )
            {
                return ActionBadRequest( $"Not authorized to delete {Rock.Model.Group.FriendlyTypeName}." );
            }

            if ( !groupService.CanDelete( entity, out var errorMessage, includeSecondLvl: true ) )
            {
                return ActionBadRequest( errorMessage );
            }

            var parentGroupId = entity.ParentGroupId;
            var groupId = entity.Id;
            var scheduleService = new ScheduleService( RockContext );

            // Delete the project's own custom schedule when no other group uses it. Named
            // schedules are shared and are left in place.
            if ( entity.ScheduleId.HasValue )
            {
                var groupSchedule = scheduleService.Get( entity.ScheduleId.Value );
                if ( groupSchedule != null && groupSchedule.ScheduleType != ScheduleType.Named )
                {
                    var isUsedByOtherGroup = groupService.Queryable().Any( g => g.ScheduleId == groupSchedule.Id && g.Id != groupId );
                    if ( !isUsedByOtherGroup )
                    {
                        scheduleService.Delete( groupSchedule );
                    }
                }
            }

            // The custom schedules of the opportunities are deleted after the project so their
            // group location schedules no longer reference them.
            var opportunitySchedules = new GroupLocationService( RockContext )
                .Queryable()
                .Where( gl => gl.GroupId == groupId )
                .SelectMany( gl => gl.Schedules )
                .ToList();

            groupService.Delete( entity );

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();

                foreach ( var schedule in opportunitySchedules )
                {
                    if ( schedule.ScheduleType != ScheduleType.Named && scheduleService.CanDelete( schedule, out _ ) )
                    {
                        scheduleService.Delete( schedule );
                    }
                }

                RockContext.SaveChanges();
            } );

            return ActionOk( GetParentGroupPageUrl( parentGroupId ) );
        }

        /// <summary>
        /// Gets the options that depend on the specified group type. Called when the group type of
        /// a project that is being added changes.
        /// </summary>
        /// <param name="groupTypeId">The identifier of the selected group type.</param>
        /// <returns>The group type options.</returns>
        [BlockAction]
        public BlockActionResult GetGroupTypeOptions( int groupTypeId )
        {
            if ( !TryGetInitialEntityForEdit( out var actionError ) )
            {
                return actionError;
            }

            if ( groupTypeId <= 0 )
            {
                return ActionBadRequest( "Group Type is required." );
            }

            var groupType = GroupTypeCache.Get( groupTypeId );
            if ( groupType == null )
            {
                return ActionBadRequest( "Group Type not found." );
            }

            if ( !SignUpOpportunityHelper.IsSignUpGroupType( groupType.Id ) )
            {
                return ActionBadRequest( "The selected group type is not a sign-up group type." );
            }

            return ActionOk( BuildGroupTypeOptions( groupType.Id ) );
        }

        /// <summary>
        /// Gets the options of the group requirement modal: the group requirement types and the
        /// date attributes of groups of the specified type.
        /// </summary>
        /// <param name="groupTypeId">The identifier of the project's group type.</param>
        /// <returns>The group requirement options.</returns>
        [BlockAction]
        public BlockActionResult GetGroupRequirementOptions( int? groupTypeId )
        {
            if ( !TryGetInitialEntityForEdit( out var actionError ) )
            {
                return actionError;
            }

            return ActionOk( new SignUpProjectRequirementOptionsBag
            {
                GroupRequirementTypes = BuildGroupRequirementTypeOptions(),
                GroupAttributes = BuildGroupDateAttributeOptions( groupTypeId )
            } );
        }

        /// <summary>
        /// Verifies that the project identified by the page parameters exists and may be edited by
        /// the current person. Used by the actions that provide edit options.
        /// </summary>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the project may be edited; otherwise, <c>false</c>.</returns>
        private bool TryGetInitialEntityForEdit( out BlockActionResult error )
        {
            error = null;

            var entity = GetInitialEntity();
            if ( entity == null )
            {
                error = ActionBadRequest( ValidationMessage.GroupNotFound );
                return false;
            }

            if ( !IsAuthorizedToEdit( entity ) )
            {
                error = ActionBadRequest( $"Not authorized to edit {Rock.Model.Group.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Adds or updates an opportunity (a schedule at a location) of the specified project.
        /// </summary>
        /// <param name="key">The identifier of the project.</param>
        /// <param name="bag">The opportunity as edited. The identifier keys are empty when adding an opportunity.</param>
        /// <returns>The refreshed list of the project's opportunities.</returns>
        [BlockAction]
        public BlockActionResult SaveOpportunity( string key, SignUpOpportunityBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest( "Invalid request." );
            }

            var group = GetProjectForOpportunityAction( key, out var actionError );
            if ( group == null )
            {
                return actionError;
            }

            var groupType = GroupTypeCache.Get( group.GroupTypeId );

            var isCustomScheduleAllowed = groupType.AllowedScheduleTypes.HasFlag( ScheduleType.Custom );
            var isNamedScheduleAllowed = groupType.AllowedScheduleTypes.HasFlag( ScheduleType.Named );
            if ( !isCustomScheduleAllowed && !isNamedScheduleAllowed )
            {
                return ActionBadRequest( $"The {groupType.Name} group type does not allow Custom or Named Group Schedule Options. Please enable at least one of these types to edit opportunities." );
            }

            var editableLocationModes = GroupLocationPickerMode.Address | GroupLocationPickerMode.Named | GroupLocationPickerMode.Point | GroupLocationPickerMode.Polygon;
            if ( ( groupType.LocationSelectionMode & editableLocationModes ) == GroupLocationPickerMode.None )
            {
                return ActionBadRequest( $"The {groupType.Name} group type does not allow any Location Selection Modes. Please enable at least one mode to edit opportunities." );
            }

            var scheduleService = new ScheduleService( RockContext );
            var locationService = new LocationService( RockContext );
            var groupLocationService = new GroupLocationService( RockContext );
            var groupMemberAssignmentService = new GroupMemberAssignmentService( RockContext );

            var errorMessages = new List<string>();

            // Validate the schedule. Only custom and named schedules are supported for opportunities.
            var newScheduleType = bag.ScheduleType;
            string newICalendarContent = null;
            int? newScheduleId = null;

            if ( newScheduleType == ScheduleType.Custom )
            {
                if ( !isCustomScheduleAllowed )
                {
                    errorMessages.Add( $"The {groupType.Name} group type does not allow custom schedules." );
                }

                newICalendarContent = bag.ICalendarContent;

                var calendarEvent = newICalendarContent.IsNotNullOrWhiteSpace()
                    ? InetCalendarHelper.CreateCalendarEvent( newICalendarContent )
                    : null;

                if ( calendarEvent?.DtStart == null )
                {
                    errorMessages.Add( ValidationMessage.ScheduleRequired );
                }
            }
            else if ( newScheduleType == ScheduleType.Named )
            {
                if ( !isNamedScheduleAllowed )
                {
                    errorMessages.Add( $"The {groupType.Name} group type does not allow named schedules." );
                }

                var namedScheduleGuid = bag.NamedSchedule?.Value.AsGuidOrNull();
                newScheduleId = namedScheduleGuid.HasValue ? scheduleService.GetId( namedScheduleGuid.Value ) : null;

                if ( !newScheduleId.HasValue )
                {
                    errorMessages.Add( ValidationMessage.ScheduleRequired );
                }
            }
            else
            {
                errorMessages.Add( ValidationMessage.ScheduleRequired );
            }

            // Validate the location.
            Location newLocation = null;
            var locationPickerMode = bag.LocationPickerMode;

            if ( ( editableLocationModes & locationPickerMode ) != locationPickerMode || locationPickerMode == GroupLocationPickerMode.None )
            {
                errorMessages.Add( ValidationMessage.LocationRequired );
            }
            else if ( !groupType.LocationSelectionMode.HasFlag( locationPickerMode ) )
            {
                errorMessages.Add( $"The {groupType.Name} group type does not allow {locationPickerMode.ConvertToString()} locations." );
            }
            else
            {
                newLocation = ResolveLocation( bag.Location, locationPickerMode, locationService );

                if ( newLocation == null )
                {
                    errorMessages.Add( ValidationMessage.LocationRequired );
                }
            }

            if ( errorMessages.Any() )
            {
                return ActionBadRequest( string.Join( " ", errorMessages ) );
            }

            var editGroupLocationId = IdHasher.Instance.GetId( bag.GroupLocationIdKey ) ?? 0;
            var editScheduleId = IdHasher.Instance.GetId( bag.ScheduleIdKey ) ?? 0;

            // A named schedule can only be used once per location within a project.
            if ( newScheduleType == ScheduleType.Named )
            {
                var namedScheduleId = newScheduleId.Value;
                var newLocationId = newLocation.Id;

                var isDuplicate = groupLocationService
                    .Queryable()
                    .AsNoTracking()
                    .Where( gl => gl.GroupId == group.Id && gl.LocationId == newLocationId )
                    .SelectMany( gl => gl.Schedules, ( gl, s ) => new { GroupLocationId = gl.Id, ScheduleId = s.Id } )
                    .Any( gls => gls.ScheduleId == namedScheduleId
                        && !( gls.GroupLocationId == editGroupLocationId && gls.ScheduleId == editScheduleId ) );

                if ( isDuplicate )
                {
                    return ActionBadRequest( $"A {group.Name} opportunity already exists for the selected Location & Schedule combination. Please edit the existing opportunity or choose a different Location and/or Schedule to add a new opportunity." );
                }
            }

            group.LoadAttributes( RockContext );
            var isProjectTypeInPerson = GetIsProjectTypeInPerson( group.GetAttributeValue( GroupAttributeKey.ProjectType ) );

            var newOpportunityName = bag.Name;
            var newMinimumAttendance = bag.SlotsMinimum;
            var newDesiredAttendance = bag.SlotsDesired;
            var newMaximumAttendance = bag.SlotsMaximum;
            var newReminderAdditionalDetails = isProjectTypeInPerson ? bag.ReminderAdditionalDetails : null;
            var newConfirmationAdditionalDetails = bag.ConfirmationAdditionalDetails;

            GroupLocation groupLocationToSave = null;
            GroupLocation existingGroupLocation = null;
            var shouldDeleteExistingGroupLocation = false;

            Schedule existingSchedule = null;
            var shouldDeleteExistingSchedule = false;

            Schedule newSchedule = null;
            var shouldAddNewScheduleConfig = true;
            var shouldDeleteExistingScheduleConfig = false;

            var assignmentsToReassign = new List<GroupMemberAssignment>();

            // Local function to consistently query for a group location based on different criteria.
            GroupLocation GetExistingGroupLocation( Expression<Func<GroupLocation, bool>> whereExpression )
            {
                return groupLocationService
                    .Queryable()
                    .Include( gl => gl.Schedules )
                    .Include( gl => gl.GroupLocationScheduleConfigs )
                    .FirstOrDefault( whereExpression );
            }

            // Local function to find the group location whose group and location match the new
            // opportunity, or to create and add a new group location.
            GroupLocation GetNewOrMatchingGroupLocation()
            {
                var groupLocation = GetExistingGroupLocation( gl => gl.GroupId == group.Id && gl.LocationId == newLocation.Id );
                if ( groupLocation == null )
                {
                    groupLocation = new GroupLocation { GroupId = group.Id, LocationId = newLocation.Id };
                    groupLocationService.Add( groupLocation );
                }

                return groupLocation;
            }

            // Local function to add the new schedule to the appropriate group location.
            void AddNewSchedule( GroupLocation groupLocation )
            {
                if ( newScheduleType == ScheduleType.Custom )
                {
                    newSchedule = new Schedule { iCalendarContent = newICalendarContent };
                }
                else
                {
                    newSchedule = scheduleService.Get( newScheduleId.Value );
                }

                groupLocation.Schedules.Add( newSchedule );
                shouldDeleteExistingScheduleConfig = true;
            }

            if ( editGroupLocationId > 0 && editScheduleId > 0 )
            {
                existingGroupLocation = GetExistingGroupLocation( gl => gl.Id == editGroupLocationId && gl.GroupId == group.Id );
                existingSchedule = existingGroupLocation?.Schedules.FirstOrDefault( s => s.Id == editScheduleId );

                // An opportunity that no longer exists is added again.
                if ( existingSchedule == null )
                {
                    existingGroupLocation = null;
                }
            }

            if ( existingGroupLocation == null )
            {
                // Add a new opportunity.
                groupLocationToSave = GetNewOrMatchingGroupLocation();
                AddNewSchedule( groupLocationToSave );
            }
            else
            {
                // Edit an existing opportunity. Group member assignments are tied to a specific
                // schedule and location combination, so when either changes the assignments
                // must be moved to the new combination.
                var shouldReassignMembers = false;
                var groupLocationChanged = newLocation.Id != existingGroupLocation.LocationId;

                if ( groupLocationChanged )
                {
                    // The location changed, so find another group location matching this
                    // project and the new location, or create a new one.
                    groupLocationToSave = GetNewOrMatchingGroupLocation();
                    shouldReassignMembers = true;

                    existingGroupLocation.Schedules.Remove( existingSchedule );
                    AddNewSchedule( groupLocationToSave );

                    // The non-named schedule of the previous location is no longer used by
                    // this opportunity, so delete it once nothing else uses it.
                    if ( existingSchedule.ScheduleType != ScheduleType.Named )
                    {
                        shouldDeleteExistingSchedule = true;
                    }
                }
                else
                {
                    groupLocationToSave = existingGroupLocation;

                    if ( newScheduleType != existingSchedule.ScheduleType )
                    {
                        // The schedule type changed from what was previously saved.
                        existingGroupLocation.Schedules.Remove( existingSchedule );
                        shouldReassignMembers = true;

                        // As long as nothing else is using the old non-named schedule, delete it.
                        if ( existingSchedule.ScheduleType != ScheduleType.Named )
                        {
                            shouldDeleteExistingSchedule = true;
                        }

                        AddNewSchedule( existingGroupLocation );
                    }
                    else
                    {
                        var shouldUpdateExistingScheduleConfig = false;

                        if ( newScheduleType == ScheduleType.Custom )
                        {
                            // Update the custom schedule in place; no need to reassign members.
                            existingSchedule.iCalendarContent = newICalendarContent;
                            shouldUpdateExistingScheduleConfig = true;
                        }
                        else if ( newScheduleId.Value != existingSchedule.Id )
                        {
                            // The named schedule changed from what was previously saved.
                            existingGroupLocation.Schedules.Remove( existingSchedule );
                            shouldReassignMembers = true;

                            AddNewSchedule( existingGroupLocation );
                        }
                        else
                        {
                            shouldUpdateExistingScheduleConfig = true;
                        }

                        if ( shouldUpdateExistingScheduleConfig )
                        {
                            var config = existingGroupLocation.GroupLocationScheduleConfigs.FirstOrDefault( c => c.ScheduleId == editScheduleId );
                            if ( config != null )
                            {
                                shouldAddNewScheduleConfig = false;

                                config.ConfigurationName = newOpportunityName;
                                config.MinimumCapacity = newMinimumAttendance;
                                config.DesiredCapacity = newDesiredAttendance;
                                config.MaximumCapacity = newMaximumAttendance;
                                config.ReminderAdditionalDetails = newReminderAdditionalDetails;
                                config.ConfirmationAdditionalDetails = newConfirmationAdditionalDetails;
                            }
                        }
                    }
                }

                if ( shouldReassignMembers )
                {
                    var existingLocationId = existingGroupLocation.LocationId;

                    assignmentsToReassign = groupMemberAssignmentService
                        .Queryable()
                        .Where( gma =>
                            gma.ScheduleId == editScheduleId
                            && gma.LocationId == existingLocationId
                            && gma.GroupMember.GroupId == group.Id )
                        .ToList();
                }

                if ( groupLocationChanged )
                {
                    // The existing schedule was removed from the existing group location, so
                    // delete that group location when it has no schedules left. Deleting it also
                    // deletes its schedule configs.
                    if ( !existingGroupLocation.Schedules.Any() )
                    {
                        shouldDeleteExistingGroupLocation = true;
                    }
                    else
                    {
                        shouldDeleteExistingScheduleConfig = true;
                    }
                }

                if ( shouldDeleteExistingScheduleConfig )
                {
                    // Delete this config explicitly, as its group location may remain.
                    var existingConfig = existingGroupLocation.GroupLocationScheduleConfigs.FirstOrDefault( c => c.ScheduleId == editScheduleId );
                    if ( existingConfig != null )
                    {
                        existingGroupLocation.GroupLocationScheduleConfigs.Remove( existingConfig );
                    }
                }
            }

            RockContext.WrapTransaction( () =>
            {
                // Initial save to get the identifiers of the added entities and to release the FK
                // constraints tied to the entities being updated or deleted.
                RockContext.SaveChanges();

                if ( newSchedule != null )
                {
                    if ( shouldAddNewScheduleConfig )
                    {
                        groupLocationToSave.GroupLocationScheduleConfigs.Add( new GroupLocationScheduleConfig
                        {
                            GroupLocationId = groupLocationToSave.Id,
                            ScheduleId = newSchedule.Id,
                            ConfigurationName = newOpportunityName,
                            MinimumCapacity = newMinimumAttendance,
                            DesiredCapacity = newDesiredAttendance,
                            MaximumCapacity = newMaximumAttendance,
                            ReminderAdditionalDetails = newReminderAdditionalDetails,
                            ConfirmationAdditionalDetails = newConfirmationAdditionalDetails
                        } );
                    }

                    /*
                        9/9/26 - MSE

                        The assignments are moved to the new location and schedule in place
                        rather than deleted and re-added, so they keep their identifiers and
                        the values of the member opportunity attributes stored against them.

                        Reason: Preserve attendee attribute values when an opportunity is moved.
                    */
                    if ( assignmentsToReassign.Any() )
                    {
                        foreach ( var assignment in assignmentsToReassign )
                        {
                            assignment.LocationId = newLocation.Id;
                            assignment.ScheduleId = newSchedule.Id;
                        }

                        // Persist the moved assignments so the previous schedule is no longer
                        // referenced when its deletion is checked below.
                        RockContext.SaveChanges();
                    }
                }

                if ( shouldDeleteExistingSchedule && scheduleService.CanDelete( existingSchedule, out _ ) )
                {
                    scheduleService.Delete( existingSchedule );
                }

                if ( shouldDeleteExistingGroupLocation && groupLocationService.CanDelete( existingGroupLocation, out _ ) )
                {
                    groupLocationService.Delete( existingGroupLocation );
                }

                // Follow-up save for the added, updated and deleted entities.
                RockContext.SaveChanges();
            } );

            return ActionOk( GetOpportunities( group ) );
        }

        /// <summary>
        /// Deletes an opportunity of the specified project along with its participants.
        /// </summary>
        /// <param name="key">The identifier of the project.</param>
        /// <param name="locationKey">The identifier key of the opportunity's location.</param>
        /// <param name="scheduleKey">The identifier key of the opportunity's schedule.</param>
        /// <returns>The refreshed list of the project's opportunities.</returns>
        [BlockAction]
        public BlockActionResult DeleteOpportunity( string key, string locationKey, string scheduleKey )
        {
            var group = GetProjectForOpportunityAction( key, out var actionError );
            if ( group == null )
            {
                return actionError;
            }

            var locationId = IdHasher.Instance.GetId( locationKey ) ?? 0;
            var scheduleId = IdHasher.Instance.GetId( scheduleKey ) ?? 0;

            if ( locationId <= 0 || scheduleId <= 0 )
            {
                return ActionBadRequest( "The selected opportunity does not exist." );
            }

            SignUpOpportunityHelper.DeleteOpportunity( RockContext, group.Id, locationId, scheduleId );

            return ActionOk( GetOpportunities( group ) );
        }

        #endregion Block Actions
    }
}

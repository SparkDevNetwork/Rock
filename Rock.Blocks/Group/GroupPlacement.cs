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
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

using Rock.Attribute;
using Rock.Blocks.Types.Mobile.Crm;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks.Group.GroupPlacement;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

using static Rock.Web.UI.RegistrationInstanceBlock;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Displays the details of a particular group.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Group Placement" )]
    [Category( "Group" )]
    [Description( "Block to manage group placements" )]
    //[SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [RegistrationTemplateField(
        "Registration Template",
        Description = "If provided, this Registration Template will override any Registration Template specified in a URL parameter.",
        Key = AttributeKey.RegistrationTemplate,
        Order = 0,
        IsRequired = false
        )]

    [LinkedPage(
        "Group Detail Page",
        Key = AttributeKey.GroupDetailPage,
        DefaultValue = Rock.SystemGuid.Page.GROUP_VIEWER,
        Order = 1
        )]

    [LinkedPage(
        "Group Member Detail Page",
        Key = AttributeKey.GroupMemberDetailPage,
        DefaultValue = Rock.SystemGuid.Page.GROUP_MEMBER_DETAIL_GROUP_VIEWER,
        Order = 2
        )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "0AA9BF5D-D72C-41DB-9719-253CE2500122" )]
    [Rock.SystemGuid.BlockTypeGuid( "5FA6DDB6-2A99-4882-8E49-562781D69ECA" )]
    public class GroupPlacement : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string RegistrationTemplate = "RegistrationTemplate";
            public const string GroupDetailPage = "GroupDetailPage";
            public const string GroupMemberDetailPage = "GroupMemberDetailPage";
        }

        private static class PreferenceKey
        {
            public const string PlacementConfigurationJSONRegistrationInstanceId = "PlacementConfigurationJSON_RegistrationInstanceId_{0}";
            public const string PlacementConfigurationJSONRegistrationTemplateId = "PlacementConfigurationJSON_RegistrationTemplateId_{0}";
            public const string RegistrantAttributeFilterRegistrationInstanceId = "RegistrantAttributeFilter_RegistrationInstanceId_{0}";
            public const string RegistrantAttributeFilterRegistrationTemplateId = "RegistrantAttributeFilter_RegistrationTemplateId_{0}";
            public const string GroupAttributeFilterGroupTypeId = "GroupAttributeFilter_GroupTypeId_{0}";
            public const string GroupMemberAttributeFilterGroupTypeId = "GroupMemberAttributeFilter_GroupTypeId_{0}";
        }

        private static class PageParameterKey
        {
            public const string RegistrantId = "RegistrantId";
            public const string RegistrationInstanceId = "RegistrationInstanceId";
            public const string RegistrationTemplateId = "RegistrationTemplateId";
            public const string RegistrationTemplatePlacementId = "RegistrationTemplatePlacementId";
            public const string PromptForTemplatePlacement = "PromptForTemplatePlacement";
        }

        #endregion Keys

        #region Classes

        private class InstancePlacementGroupPerson
        {
            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            /// <value>
            /// The person identifier.
            /// </value>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the registration instance identifier.
            /// </summary>
            /// <value>
            /// The registration instance identifier.
            /// </value>
            public int RegistrationInstanceId { get; set; }

            public int GroupId { get; set; }
        }

        private class TemplatePlacementGroupPerson
        {
            public int PersonId { get; set; }

            public int GroupId { get; set; }
        }

        private class GroupPlacementResult
        {
            public int GroupId { get; set; }
            public string GroupName { get; set; }
            public int? GroupCapacity { get; set; }
            public int GroupTypeId { get; set; }
            public int GroupOrder { get; set; }
            public int PersonId { get; set; }
            public int GroupRoleId { get; set; }
            public int GroupMemberId { get; set; }
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            GroupPlacementInitializationBox box = GetInitializationBox();

            return box;
        }

        private GroupPlacementInitializationBox GetInitializationBox()
        {
            GroupPlacementInitializationBox box = new GroupPlacementInitializationBox();
            box.PlacementGroupType = new PlacementGroupTypeBag();
            box.SelectedPlacement = new PlacementBag();
            box.Title = "Group Placement"; // TODO - convert to proper title

            var registrationTemplatePlacementId = GetIdFromPageParameter( PageParameterKey.RegistrationTemplatePlacementId );
            var registrationInstanceId = GetIdFromPageParameter( PageParameterKey.RegistrationInstanceId );

            var registrationTemplateService = new RegistrationTemplateService( RockContext );
            var registrationInstanceService = new RegistrationInstanceService( RockContext );

            int? registrationTemplateId;
            Guid? registrationTemplateGuid = GetAttributeValue( AttributeKey.RegistrationTemplate ).AsGuidOrNull();
            if ( registrationTemplateGuid.HasValue )
            {
                registrationTemplateId = registrationTemplateService.GetId( registrationTemplateGuid.Value );
            }
            else
            {
                registrationTemplateId = GetIdFromPageParameter( PageParameterKey.RegistrationTemplateId );
            }

            // in case a specific registrant is specified
            int? registrantId = GetIdFromPageParameter( PageParameterKey.RegistrantId );

            if ( registrantId.HasValue )
            {
                //hfRegistrantId.Value = registrantId.ToString(); TODO - come back to hidden field functionality
                registrationInstanceId = new RegistrationRegistrantService( RockContext ).GetSelect( registrantId.Value, s => s.Registration.RegistrationInstanceId );
            }

            RegistrationInstance registrationInstance = null;
            if ( registrationInstanceId.HasValue )
            {
                //hfRegistrationInstanceId.Value = registrationInstanceId.ToString(); TODO - come back to hidden field functionality
                registrationInstance = registrationInstanceService.Get( registrationInstanceId.Value );
                if ( registrationInstance != null )
                {
                    registrationTemplateId = registrationInstance.RegistrationTemplateId;
                }
            }

            // make sure a valid RegistrationTemplate specified (or determined from RegistrationInstanceId )
            RegistrationTemplate registrationTemplate;

            if ( registrationTemplateId.HasValue )
            {
                registrationTemplate = registrationTemplateService.Get( registrationTemplateId.Value );
            }
            else
            {
                registrationTemplate = null;
            }

            if ( registrationTemplate == null )
            {
                box.ErrorMessage = "Invalid Registration Template";
                return box;
            }

            RegistrationTemplatePlacement registrationTemplatePlacement = null;
            int? registrationTemplatePlacementGroupTypeId = null;
            if ( registrationTemplatePlacementId.HasValue )
            {
                registrationTemplatePlacement = new RegistrationTemplatePlacementService( RockContext ).Get( registrationTemplatePlacementId.Value );

                if ( registrationTemplatePlacement == null || registrationTemplatePlacement.RegistrationTemplateId != registrationTemplateId )
                {
                    // if the registration template placement is for a different registration template, don't use it
                    registrationTemplatePlacement = null;
                    registrationTemplatePlacementId = null;
                }
                else
                {
                    registrationTemplatePlacementGroupTypeId = registrationTemplatePlacement.GroupTypeId;
                    box.PlacementGroupType.Id = registrationTemplatePlacement.GroupTypeId;
                }
            }

            // Binds the selectable placements ( Buses, Cabins, etc. )
            var registrationTemplatePlacements = registrationTemplateService.GetSelect( registrationTemplateId.Value, s => s.Placements ).OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();

            if ( registrationTemplatePlacement == null )
            {
                if ( !registrationTemplatePlacements.Any() )
                {
                    box.ErrorMessage = "No Placement Types available for this registration template.";
                    //rptSelectRegistrationTemplatePlacement.Visible = false;
                    //nbConfigurationError.Text = "No Placement Types available for this registration template.";
                    //nbConfigurationError.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
                    //nbConfigurationError.Visible = true;
                    //pnlView.Visible = false;
                    return box;
                }

                box.PlacementGroupType.Id = registrationTemplatePlacements.First().Id;

                //ReloadPageWithRegistrationTemplatePlacementId( defaultRegistrationTemplatePlacementId );
                //return;
            }

            // TODO -- handle null issue here.
            var groupType = GroupTypeCache.Get( registrationTemplatePlacementGroupTypeId.Value );

            box.RegistrantId = registrantId;
            box.RegistrationInstanceId = registrationInstanceId;
            box.RegistrationTemplateId = registrationTemplateId;
            box.RegistrationTemplatePlacementId = registrationTemplatePlacementId;

            box.PlacementGroupTypeRoles = groupType?.Roles
                .Select( r => new PlacementGroupTypeRoleBag
                {
                    Id = r.Id,
                    Name = r.Name,
                    MaxCount = r.MaxCount,
                    MinCount = r.MinCount,
                    IsLeader = r.IsLeader
                } )
                .ToList();

            if ( !registrationInstanceId.HasValue )
            {
                box.InTemplateMode = true;
                box.Title = $"Group Placement - {registrationTemplate.Name} - {registrationTemplatePlacement.Name}";
            }
            else
            {
                box.InTemplateMode = false;
                box.Title = $"Group Placement - {registrationInstance.Name} - {registrationTemplatePlacement.Name}";
            }

            box.SelectedPlacement.IconCSSClass = registrationTemplatePlacement?.IconCssClass;
            box.SelectedPlacement.Name = registrationTemplatePlacement?.Name;
            box.IsPlacementAllowingMultiple = registrationTemplatePlacement?.AllowMultiplePlacements ?? false; // todo this shouldn't need a null check.
            box.PlacementGroupType.IconCSSClass = groupType?.IconCssClass;
            box.PlacementGroupType.Name = groupType?.GroupTerm?.Pluralize();

            var placementConfigurationSettingOptions = new PlacementConfigurationSettingOptionsBag();
            placementConfigurationSettingOptions.RegistrantAttributes = GetRegistrantAttributesAsListItems( registrationTemplateId );
            Rock.Model.Group fakeGroup;
            placementConfigurationSettingOptions.GroupAttributes = GetGroupAttributesAsListItems( registrationTemplatePlacementGroupTypeId, out fakeGroup );
            placementConfigurationSettingOptions.GroupMemberAttributes = GetGroupMemberAttributesAsListItems( fakeGroup );

            if ( box.InTemplateMode )
            {
                // Get Registration instances... TODO - Optimize to only query once and get both variations of instances.

                var registrationTemplateInstances = registrationInstanceService.Queryable()
                    .Where( a => a.RegistrationTemplateId == registrationTemplateId )
                    .OrderBy( a => a.Name )
                    .Select( a => new { a.Id, a.Name } )
                    .ToList();

                placementConfigurationSettingOptions.RegistrationInstances = registrationTemplateInstances
                    .Select( i => new ListItemBag
                    {
                        Value = i.Id.ToString(),
                        Text = i.Name
                    } )
                    .ToList();
            }

            box.PlacementConfigurationSettingOptions = placementConfigurationSettingOptions;

            var placementConfiguration = GetPlacementConfiguration( registrationInstanceId, registrationTemplateId );
            HashSet<int> displayedAttributeIds = null;
            HashSet<int> groupAttributeIds = null;
            HashSet<int> groupMemberAttributeIds = null;
            AttributeFiltersBag attributeFiltersBag = new AttributeFiltersBag();

            if ( placementConfiguration.SourceAttributesToDisplay?.Any() == true )
            {
                displayedAttributeIds = placementConfiguration.SourceAttributesToDisplay
                    .Select( id => id.AsIntegerOrNull() )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToHashSet();

                // TODO - guarantee that registrationTemplateId is not null
                RegistrationRegistrant fakeRegistrant = new RegistrationRegistrant
                {
                    RegistrationTemplateId = registrationTemplateId.Value
                };

                fakeRegistrant.LoadAttributes();

                // TODO - Maybe use our method instead.
                attributeFiltersBag.RegistrantAttributesForFilters = fakeRegistrant.GetPublicAttributesForEdit( GetCurrentPerson(), true, attributeFilter: a => displayedAttributeIds.Contains( a.Id ) );
                attributeFiltersBag.RegistrantAttributeValuesForFilters = fakeRegistrant.GetPublicAttributeValuesForEdit( GetCurrentPerson(), true, attributeFilter: a => displayedAttributeIds.Contains( a.Id ) );
            }

            if ( placementConfiguration.GroupAttributesToDisplay?.Any() == true )
            {
                groupAttributeIds = placementConfiguration.GroupAttributesToDisplay
                    .Select( id => id.AsIntegerOrNull() )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToHashSet();

                fakeGroup.LoadAttributes();

                attributeFiltersBag.GroupAttributesForFilters = fakeGroup.GetPublicAttributesForEdit( GetCurrentPerson(), true, attributeFilter: a => groupAttributeIds.Contains( a.Id ) );
                attributeFiltersBag.GroupAttributeValuesForFilters = fakeGroup.GetPublicAttributeValuesForEdit( GetCurrentPerson(), true, attributeFilter: a => groupAttributeIds.Contains( a.Id ) );
            }

            if ( placementConfiguration.GroupMemberAttributesToDisplay?.Any() == true )
            {
                groupMemberAttributeIds = placementConfiguration.GroupMemberAttributesToDisplay
                    .Select( id => id.AsIntegerOrNull() )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToHashSet();

                // TODO - fakeGroup may be null
                var fakeGroupMember = new GroupMember
                {
                    Group = fakeGroup,
                    GroupId = fakeGroup.Id
                };

                fakeGroupMember.LoadAttributes();

                attributeFiltersBag.GroupMemberAttributesForFilters = fakeGroupMember.GetPublicAttributesForEdit( GetCurrentPerson(), true, attributeFilter: a => groupMemberAttributeIds.Contains( a.Id ) );
                attributeFiltersBag.GroupMemberAttributeValuesForFilters = fakeGroupMember.GetPublicAttributeValuesForEdit( GetCurrentPerson(), true, attributeFilter: a => groupMemberAttributeIds.Contains( a.Id ) );
            }

            box.AttributeFilters = attributeFiltersBag;

            if ( box.PlacementGroupType.Id.HasValue && registrationTemplatePlacementId.HasValue )
            {
                //box.PlacementGroups = GetPlacementGroups( registrationTemplatePlacementId.Value, registrationInstanceId, registrationTemplateId, box.PlacementGroupType.Id.Value );
                // TODO - add logic for cases where we just get a single registrant
                // TODO - add logic for Included Registration Instances
                var placementGroupDetails = GetPlacementGroupDetails( registrationTemplatePlacementId.Value, registrationInstanceId, registrationTemplateId.Value );
                var placementPeople = GetPlacementPeople( registrationTemplateId.Value, registrationInstanceId, registrantId );

                // Probably very inefficient.
                GroupMemberService groupMemberService = new GroupMemberService( RockContext );

                var personLookup = placementPeople.ToDictionary( p => p.PersonId );

                // Populate the placement groups with members
                var placementGroups = placementGroupDetails.Select( group => new PlacementGroupBag
                {
                    GroupId = group.GroupId,
                    GroupName = group.GroupName,
                    GroupOrder = group.GroupOrder,
                    GroupTypeId = group.GroupTypeId,
                    GroupCapacity = group.GroupCapacity,
                    Attributes = GroupCache.Get( group.GroupId ).GetPublicAttributesForView( GetCurrentPerson(), true, attributeFilter: a => groupAttributeIds.Contains( a.Id ) ),
                    AttributeValues = GroupCache.Get( group.GroupId ).GetPublicAttributeValuesForView( GetCurrentPerson(), true, attributeFilter: a => groupAttributeIds.Contains( a.Id ) ),
                    GroupMembers = group.GroupMembers
                        .Where( gm => personLookup.ContainsKey( gm.Person.PersonId ) )
                        .Select( gm =>
                        {
                            var person = personLookup[gm.Person.PersonId];

                            return new GroupMemberBag
                            {
                                GroupMemberId = gm.GroupMemberId,
                                GroupRoleId = gm.GroupRoleId,
                                Attributes = GetGroupMemberAttributes( gm.GroupMemberId, groupMemberAttributeIds, groupMemberService ),
                                AttributeValues = GetGroupMemberAttributeValues( gm.GroupMemberId, groupMemberAttributeIds, groupMemberService ),
                                Person = new PersonBag
                                {
                                    PersonId = person.PersonId,
                                    FirstName = person.FirstName,
                                    Nickname = person.Nickname,
                                    LastName = person.LastName,
                                    Gender = person.Gender,
                                    PhotoUrl = person.PhotoUrl,
                                    Registrants = person.Registrants
                                }
                            };
                        } )
                        .ToList()
                } ).ToList();

                // Populate the people that need to be placed.
                var placedPersonIds = placementGroups
                    .SelectMany( g => g.GroupMembers )
                    .Select( m => m.Person.PersonId )
                    .ToHashSet();

                var peopleToPlace = placementPeople
                    .Where( p => !placedPersonIds.Contains( p.PersonId ) )
                    .ToList();

                box.PlacementGroups = placementGroups;
                box.PeopleToPlace = peopleToPlace;
            }

            return box;
        }

        private List<PersonBag> GetPlacementPeople( int registrationTemplateId, int? registrationInstanceId, int? registrantId )
        {
            var registrationRegistrantService = new RegistrationRegistrantService( RockContext );
            var registrationRegistrantQuery = registrationRegistrantService.Queryable();
            var placementConfiguration = GetPlacementConfiguration( registrationInstanceId, registrationTemplateId );

            HashSet<int> includedRegistrationInstanceIds = null;
            if ( placementConfiguration.IncludedRegistrationInstanceIds?.Any() == true )
            {
                includedRegistrationInstanceIds = placementConfiguration.IncludedRegistrationInstanceIds?.Select( id => id.AsIntegerOrNull() )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToHashSet();
            }

            if ( registrationInstanceId.HasValue )
            {
                registrationRegistrantQuery = registrationRegistrantQuery.Where( a => a.Registration.RegistrationInstanceId == registrationInstanceId.Value );
            }
            else if ( includedRegistrationInstanceIds != null )
            {
                registrationRegistrantQuery = registrationRegistrantQuery.Where( a => includedRegistrationInstanceIds.Contains( a.Registration.RegistrationInstanceId ) );
            }
            else
            {
                var instanceIds = new RegistrationInstanceService( RockContext )
                    .Queryable()
                    .Where( i => i.RegistrationTemplateId == registrationTemplateId )
                    .Select( i => i.Id );

                registrationRegistrantQuery = registrationRegistrantQuery.Where( rr => instanceIds.Contains( rr.Registration.RegistrationInstanceId ) );
            }

            if ( registrantId.HasValue )
            {
                registrationRegistrantQuery = registrationRegistrantQuery.Where( a => a.Id == registrantId.Value );
            }

            HashSet<int> displayedAttributeIds = null;

            if ( placementConfiguration.SourceAttributesToDisplay?.Any() == true )
            {
                displayedAttributeIds = placementConfiguration.SourceAttributesToDisplay
                    .Select( id => id.AsIntegerOrNull() )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToHashSet();
            }

            var registrants = registrationRegistrantQuery
                .Where( r => r.PersonAlias != null && r.PersonAlias.Person != null )
                .Select( r => new
                {
                    Registrant = r,
                    r.PersonAlias.Person,
                    RegistrantId = r.Id,
                    RegistrationInstanceId = r.Registration.RegistrationInstance.Id,
                    RegistrationInstanceName = r.Registration.RegistrationInstance.Name,
                } )
                .ToList();

            // TODO - Consider Grouping in SQL
            // TODO - Handle Fees

            var people = registrants
                .GroupBy( r => r.Person.Id )
                .Select( g => new PersonBag
                {
                    PersonId = g.Key,
                    FirstName = g.First().Person.FirstName,
                    Nickname = g.First().Person.NickName,
                    LastName = g.First().Person.LastName,
                    Gender = g.First().Person.Gender,
                    PhotoUrl = g.First().Person.PhotoUrl,
                    Registrants = g.Select( r => new RegistrantBag
                    {
                        RegistrantId = r.RegistrantId,
                        RegistrationInstanceId = r.RegistrationInstanceId,
                        RegistrationInstanceName = r.RegistrationInstanceName,
                        Attributes = GetRegistrantAttributes( r.Registrant, displayedAttributeIds ),
                        AttributeValues = GetRegistrantAttributeValues( r.Registrant, displayedAttributeIds )
                    } ).ToList()
                } )
                .ToList();

            return people;
        }

        private List<PlacementGroupDetailsBag> GetPlacementGroupDetails( int registrationTemplatePlacementId, int? registrationInstanceId, int registrationTemplateId )
        {
            int registrationTemplatePlacementEntityTypeId = EntityTypeCache.Get<Rock.Model.RegistrationTemplatePlacement>().Id;
            int registrationInstanceEntityTypeId = EntityTypeCache.Get<Rock.Model.RegistrationInstance>().Id;
            int groupEntityTypeId = EntityTypeCache.Get<Rock.Model.Group>().Id;
            string placementMode;
            List<GroupPlacementResult> groupPlacementResults;

            // If registration instance id has a value than we are in instance mode
            if ( registrationInstanceId.HasValue )
            {
                placementMode = "InstanceMode";

                groupPlacementResults = RockContext.Database.SqlQuery<GroupPlacementResult>(
                    $@"EXEC [dbo].[spGetGroupPlacementDetails] 
                        @{nameof( registrationTemplatePlacementEntityTypeId )}, 
                        @{nameof( registrationInstanceEntityTypeId )}, 
                        @{nameof( groupEntityTypeId )}, 
                        @{nameof( registrationTemplateId )}, 
                        @{nameof( registrationInstanceId )}, 
                        @{nameof( registrationTemplatePlacementId )}, 
                        @{nameof( placementMode )}",
                    new SqlParameter( nameof( registrationTemplatePlacementEntityTypeId ), registrationTemplatePlacementEntityTypeId ),
                    new SqlParameter( nameof( registrationInstanceEntityTypeId ), registrationInstanceEntityTypeId ),
                    new SqlParameter( nameof( groupEntityTypeId ), groupEntityTypeId ),
                    new SqlParameter( nameof( registrationTemplateId ), registrationTemplateId ),
                    new SqlParameter( nameof( registrationInstanceId ), registrationInstanceId.Value ),
                    new SqlParameter( nameof( registrationTemplatePlacementId ), registrationTemplatePlacementId ),
                    new SqlParameter( nameof( placementMode ), placementMode )
                ).ToList();
            }
            else
            {
                placementMode = "TemplateMode";

                groupPlacementResults = RockContext.Database.SqlQuery<GroupPlacementResult>(
                    $@"EXEC [dbo].[spGetGroupPlacementDetails] 
                        @{nameof( registrationTemplatePlacementEntityTypeId )}, 
                        @{nameof( registrationInstanceEntityTypeId )}, 
                        @{nameof( groupEntityTypeId )}, 
                        @{nameof( registrationTemplateId )},
                        @{nameof( registrationInstanceId )}, 
                        @{nameof( registrationTemplatePlacementId )}, 
                        @{nameof( placementMode )}",
                    new SqlParameter( nameof( registrationTemplatePlacementEntityTypeId ), registrationTemplatePlacementEntityTypeId ),
                    new SqlParameter( nameof( registrationInstanceEntityTypeId ), registrationInstanceEntityTypeId ),
                    new SqlParameter( nameof( groupEntityTypeId ), groupEntityTypeId ),
                    new SqlParameter( nameof( registrationTemplateId ), registrationTemplateId ),
                    new SqlParameter( nameof( registrationInstanceId ), DBNull.Value ),
                    new SqlParameter( nameof( registrationTemplatePlacementId ), registrationTemplatePlacementId ),
                    new SqlParameter( nameof( placementMode ), placementMode )
                ).ToList();
            }

            var placementConfiguration = GetPlacementConfiguration( registrationInstanceId, registrationTemplateId );
            HashSet<int> groupIdsToDisplay = new HashSet<int>();

            if ( placementConfiguration.SourceAttributesToDisplay?.Any() == true )
            {
                groupIdsToDisplay = placementConfiguration.SourceAttributesToDisplay
                    .Select( id => id.AsIntegerOrNull() )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToHashSet();
            }

            var placementGroupDetails = groupPlacementResults
                .GroupBy( r => new { r.GroupId, r.GroupName, r.GroupCapacity, r.GroupTypeId, r.GroupOrder } )
                .Select( g => new PlacementGroupDetailsBag
                {
                    GroupId = g.Key.GroupId,
                    GroupName = g.Key.GroupName,
                    GroupCapacity = g.Key.GroupCapacity,
                    GroupTypeId = g.Key.GroupTypeId,
                    GroupOrder = g.Key.GroupOrder,
                    GroupMembers = g.Select( x => new GroupMemberBag
                    {
                        GroupMemberId = x.GroupMemberId,
                        GroupRoleId = x.GroupRoleId,
                        Person = new PersonBag
                        {
                            PersonId = x.PersonId
                        }
                    } ).ToList()
                } ).ToList();

            return placementGroupDetails;
        }

        private List<ListItemBag> GetRegistrantAttributesAsListItems( int? registrationTemplateId )
        {
            var listItems = new List<ListItemBag>();

            if ( !registrationTemplateId.HasValue )
            {
                return listItems;
            }

            using ( var rockContext = new RockContext() )
            {
                var templateForms = new RegistrationTemplateService( rockContext )
                    .GetSelect( registrationTemplateId.Value, t => t.Forms )?
                    .ToList();

                if ( templateForms == null )
                {
                    return listItems;
                }

                foreach ( var form in templateForms )
                {
                    var fields = form.Fields
                        .OrderBy( f => f.Order )
                        .Where( f => f.FieldSource == RegistrationFieldSource.RegistrantAttribute && f.AttributeId.HasValue )
                        .ToList();

                    foreach ( var field in fields )
                    {
                        var attribute = AttributeCache.Get( field.AttributeId.Value );
                        if ( attribute != null )
                        {
                            listItems.Add( new ListItemBag
                            {
                                Text = attribute.Name,
                                Value = attribute.Id.ToString()
                            } );
                        }
                    }
                }
            }

            return listItems;
        }

        private List<ListItemBag> GetGroupAttributesAsListItems( int? registrationTemplatePlacementGroupTypeId, out Rock.Model.Group fakeGroup )
        {
            var listItems = new List<ListItemBag>();

            if ( !registrationTemplatePlacementGroupTypeId.HasValue )
            {
                fakeGroup = null;
                return listItems;
            }

            fakeGroup = new Rock.Model.Group { GroupTypeId = registrationTemplatePlacementGroupTypeId.Value };
            fakeGroup.LoadAttributes();

            listItems = fakeGroup.Attributes
                .Select( a => a.Value )
                .Where( a => a != null )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .Select( a => new ListItemBag
                {
                    Text = a.Name,
                    Value = a.Id.ToString()
                } )
                .ToList();

            return listItems;
        }

        public List<ListItemBag> GetGroupMemberAttributesAsListItems( Rock.Model.Group group )
        {
            var listItems = new List<ListItemBag>();

            if ( group == null )
            {
                return listItems;
            }

            var fakeGroupMember = new GroupMember
            {
                Group = group,
                GroupId = group.Id
            };

            fakeGroupMember.LoadAttributes();

            listItems = fakeGroupMember.GetAuthorizedAttributes( Authorization.VIEW, GetCurrentPerson() )
                .Select( a => a.Value )
                .Where( a => a != null )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .Select( a => new ListItemBag
                {
                    Text = a.Name,
                    Value = a.Id.ToString()
                } )
                .ToList();

            return listItems;
        }

        //public List<PersonBag> LegacyGetPlacementPeople( int registrationTemplateId, int? registrationInstanceId, int? registrantId, int registrationTemplatePlacementId )
        //{
        //    /*
        //     * 1. Get the Registration Template Placements using the registrationTemplateId.
        //     * 2. Get the Registration Instances
        //     *    - First check for a registrationInstanceId
        //     *    - Else we get all registration instances underneath the registrationTemplate.
        //     *    - If includedRegistrationIds has a value than we filter our registration instances to be whatever is included in that hash set.
        //     * 3. Use GetRegistrationTemplatePlacementPlacementGroups to select Groups from our Registration Template Placements.
        //     *    - We can get our Group Members from this.
        //     * 4. Use GetRegistrationInstancePlacementGroupsByPlacement to select Groups from our Registration Instance Placements.
        //     *    - We can get our Group Members from this.
        //     * 
        //     */


        //    var registrationRegistrantService = new RegistrationRegistrantService( RockContext );
        //    var registrationRegistrantQuery = registrationRegistrantService.Queryable();
        //    var placementConfiguration = GetPlacementConfiguration( registrationInstanceId, registrationTemplateId );

        //    HashSet<int> includedRegistrationInstanceIds = null;
        //    if (placementConfiguration.IncludedRegistrationInstanceIds?.Any() == true)
        //    {
        //        includedRegistrationInstanceIds = placementConfiguration.IncludedRegistrationInstanceIds
        //            ?.Select( id => id.AsIntegerOrNull() )
        //            .Where( id => id.HasValue )
        //            .Select( id => id.Value )
        //            .ToHashSet();
        //    }

        //    registrationRegistrantQuery = registrationRegistrantQuery
        //        .Where( a => a.Registration.RegistrationInstance.RegistrationTemplateId == registrationTemplateId );

        //    if ( registrationInstanceId.HasValue )
        //    {
        //        registrationRegistrantQuery = registrationRegistrantQuery.Where( a => a.Registration.RegistrationInstanceId == registrationInstanceId.Value );
        //    }
        //    else if ( includedRegistrationInstanceIds != null )
        //    {
        //        registrationRegistrantQuery = registrationRegistrantQuery.Where( a => includedRegistrationInstanceIds.Contains( a.Registration.RegistrationInstanceId ) );
        //    }

        //    if ( registrantId.HasValue )
        //    {
        //        registrationRegistrantQuery = registrationRegistrantQuery.Where( a => a.Id == registrantId.Value );
        //    }

        //    var registrationTemplatePlacement = new RegistrationTemplatePlacementService( RockContext ).Get( registrationTemplatePlacementId );

        //    registrationRegistrantQuery = registrationRegistrantQuery.Where( a => a.OnWaitList == false );

        //    registrationRegistrantQuery = registrationRegistrantQuery.OrderBy( a => a.PersonAlias.Person.LastName ).ThenBy( a => a.PersonAlias.Person.NickName );

        //    var registrationTemplatePlacementService = new RegistrationTemplatePlacementService( RockContext );
        //    var registrationInstanceService = new RegistrationInstanceService( RockContext );

        //    var templatePlacementsByPerson = registrationTemplatePlacementService
        //        .GetRegistrationTemplatePlacementPlacementGroups( registrationTemplatePlacement )
        //        .SelectMany( g => g.Members )
        //        .GroupBy( m => m.PersonId )
        //        .ToDictionary(
        //            g => g.Key,
        //            g => g.Select( m => (m.GroupId, m.GroupRoleId) ).ToList()
        //        );

        //    //IQueryable<InstancePlacementGroupPerson> allInstancesPlacementGroupInfoQuery = null;

        //    if ( !registrationInstanceId.HasValue && ( placementConfiguration.IncludedRegistrationInstanceIds == null || !placementConfiguration.IncludedRegistrationInstanceIds.Any() ) )
        //    {
        //        includedRegistrationInstanceIds = new RegistrationTemplateService( RockContext ).GetSelect( registrationTemplateId, s => s.Instances.Select( i => i.Id ) ).ToHashSet();
        //    }

        //    var instancePlacementsByPersonAndInstance = new Dictionary<(int PersonId, int RegistrationInstanceId), List<(int GroupId, int GroupRoleId)>>();

        //    if ( registrationInstanceId.HasValue )
        //    {
        //        var instance = registrationInstanceService.Get( registrationInstanceId.Value );

        //        var groupMembers = registrationInstanceService
        //            .GetRegistrationInstancePlacementGroupsByPlacement( instance, registrationTemplatePlacementId )
        //            .Where( g => g.GroupTypeId == registrationTemplatePlacement.GroupTypeId )
        //            .SelectMany( g => g.Members )
        //            .Select( m => new
        //            {
        //                m.PersonId,
        //                m.GroupId,
        //                m.GroupRoleId,
        //                RegistrationInstanceId = registrationInstanceId.Value
        //            } )
        //            .ToList();

        //        instancePlacementsByPersonAndInstance = groupMembers
        //            .GroupBy( x => (x.PersonId, x.RegistrationInstanceId) )
        //            .ToDictionary(
        //                g => g.Key,
        //                g => g.Select( x => (x.GroupId, x.GroupRoleId) ).ToList()
        //            );
        //    }
        //    else if ( includedRegistrationInstanceIds != null )
        //    {
        //        var combinedGroupMembers = new List<(int PersonId, int RegistrationInstanceId, int GroupId, int GroupRoleId)>();

        //        foreach ( var instanceId in includedRegistrationInstanceIds )
        //        {
        //            var instance = registrationInstanceService.Get( instanceId );

        //            var groupMembers = registrationInstanceService
        //                .GetRegistrationInstancePlacementGroupsByPlacement( instance, registrationTemplatePlacementId )
        //                .Where( g => g.GroupTypeId == registrationTemplatePlacement.GroupTypeId )
        //                .SelectMany( g => g.Members )
        //                .Select( m => new
        //                {
        //                    m.PersonId,
        //                    m.GroupId,
        //                    m.GroupRoleId,
        //                    RegistrationInstanceId = instanceId
        //                } )
        //                .ToList()
        //                .Select( x => (x.PersonId, x.RegistrationInstanceId, x.GroupId, x.GroupRoleId) );

        //            combinedGroupMembers.AddRange( groupMembers );
        //        }

        //        instancePlacementsByPersonAndInstance = combinedGroupMembers
        //            .GroupBy( x => (x.PersonId, x.RegistrationInstanceId) )
        //            .ToDictionary(
        //                g => g.Key,
        //                g => g.Select( x => (x.GroupId, x.GroupRoleId) ).ToList()
        //            );
        //    }

        //    //if ( allInstancesPlacementGroupInfoQuery == null )
        //    //{
        //    //    throw new ArgumentNullException( "Registration Instance(s) must be specified" );
        //    //}

        //    // TODO - handle null instances ^^

        //    var registrationRegistrantPlacementList = registrationRegistrantQuery
        //        .ToList()
        //        .Select( r =>
        //        {
        //            var templateGroupTuples = templatePlacementsByPerson.TryGetValue( r.PersonAlias.PersonId, out var tGroupTuples )
        //                ? tGroupTuples
        //                : new List<(int GroupId, int GroupRoleId)>();

        //            var instanceKey = (r.PersonAlias.PersonId, r.Registration.RegistrationInstanceId);
        //            var instanceGroupTuples = instancePlacementsByPersonAndInstance.TryGetValue( instanceKey, out var iGroupTuples )
        //                ? iGroupTuples
        //                : new List<(int GroupId, int GroupRoleId)>();

        //            var placedGroups = templateGroupTuples
        //                .Concat( instanceGroupTuples )
        //                .GroupBy( p => p.GroupId )
        //                .Select( g => new GroupPlacementMappingBag
        //                {
        //                    GroupId = g.Key,
        //                    GroupRoleId = g.First().GroupRoleId
        //                } )
        //                .ToList();

        //            return new
        //            {
        //                Registrant = r,
        //                Person = r.PersonAlias.Person,
        //                RegistrationInstance = r.Registration.RegistrationInstance,
        //                GroupPlacementMapping = placedGroups,
        //                AlreadyPlacedInGroup = placedGroups.Any()
        //            };
        //        } )
        //        .ToList();

        //    HashSet<int> displayedAttributeIds = null;

        //    if ( placementConfiguration.SourceAttributesToDisplay?.Any() == true )
        //    {
        //        displayedAttributeIds = placementConfiguration.SourceAttributesToDisplay
        //            .Select( id => id.AsIntegerOrNull() )
        //            .Where( id => id.HasValue )
        //            .Select( id => id.Value )
        //            .ToHashSet();
        //    }

        //    var personBagList = registrationRegistrantPlacementList
        //        .Select( x => new PersonBag
        //        {
        //            PersonId = x.Person.Id,
        //            RegistrationInstanceId = x.RegistrationInstance.Id,
        //            AlreadyPlacedInGroup = x.AlreadyPlacedInGroup,
        //            GroupPlacementMapping = x.GroupPlacementMapping,
        //            FirstName = x.Person.FirstName,
        //            Nickname = x.Person.NickName,
        //            LastName = x.Person.LastName,
        //            PhotoUrl = x.Person.PhotoUrl,
        //            Gender = x.Person.Gender,
        //            RegistrantId = x.Registrant.Id,
        //            RegistrationInstanceName = x.RegistrationInstance.Name,
        //            Attributes = GetRegistrantAttributes( x.Registrant, displayedAttributeIds ),
        //            AttributeValues = GetRegistrantAttributeValues( x.Registrant, displayedAttributeIds ),
        //            //Fees = new Dictionary<string, string>(),
        //        } )
        //        .ToList();

        //    return personBagList;
        //}

        private Dictionary<string, PublicAttributeBag> GetGroupMemberAttributes( int groupMemberId, HashSet<int> displayedAttributeIds, GroupMemberService groupMemberService )
        {
            if ( displayedAttributeIds == null )
            {
                // don't spend time loading attributes if there aren't any to be displayed
                return null;
            }

            var groupMember = groupMemberService.Get( groupMemberId );

            if ( groupMember.AttributeValues == null )
            {
                groupMember.LoadAttributes();
            }

            var attributes = groupMember.GetPublicAttributesForView( GetCurrentPerson(), true, attributeFilter: a => displayedAttributeIds.Contains( a.Id ) );

            return attributes;
        }

        private Dictionary<string, string> GetGroupMemberAttributeValues( int groupMemberId, HashSet<int> displayedAttributeIds, GroupMemberService groupMemberService )
        {
            if ( displayedAttributeIds == null )
            {
                // don't spend time loading attributes if there aren't any to be displayed
                return null;
            }

            var groupMember = groupMemberService.Get( groupMemberId );

            if ( groupMember.AttributeValues == null )
            {
                groupMember.LoadAttributes();
            }

            var attributeValues = groupMember.GetPublicAttributeValuesForView( GetCurrentPerson(), true, attributeFilter: a => displayedAttributeIds.Contains( a.Id ) );

            return attributeValues;
        }

        private Dictionary<string, PublicAttributeBag> GetRegistrantAttributes( RegistrationRegistrant registrant, HashSet<int> displayedAttributeIds )
        {
            if ( displayedAttributeIds == null )
            {
                // don't spend time loading attributes if there aren't any to be displayed
                return null;
            }

            if ( registrant.AttributeValues == null )
            {
                registrant.LoadAttributes();
            }

            var attributes = registrant.GetPublicAttributesForView( GetCurrentPerson(), true, attributeFilter: a => displayedAttributeIds.Contains( a.Id ) );

            return attributes;
        }

        private Dictionary<string, string> GetRegistrantAttributeValues( RegistrationRegistrant registrant, HashSet<int> displayedAttributeIds )
        {
            if ( displayedAttributeIds == null )
            {
                // don't spend time loading attributes if there aren't any to be displayed
                return null;
            }

            if ( registrant.AttributeValues == null )
            {
                registrant.LoadAttributes();
            }

            var attributeValues = registrant.GetPublicAttributeValuesForView( GetCurrentPerson(), true, attributeFilter: a => displayedAttributeIds.Contains( a.Id ) );

            return attributeValues;
        }

        /// <summary>
        /// Gets the placement configuration.
        /// </summary>
        /// <returns></returns>
        private PlacementConfigurationSettingsBag GetPlacementConfiguration( int? registrationInstanceId, int? registrationTemplateId )
        {
            if ( _placementConfiguration == null )
            {
                var preferences = GetBlockPersonPreferences();

                string placementConfigurationJSON = string.Empty;
                if ( registrationInstanceId.HasValue )
                {
                    placementConfigurationJSON = preferences.GetValue( string.Format( PreferenceKey.PlacementConfigurationJSONRegistrationInstanceId, registrationInstanceId.Value ) );
                }
                else if ( registrationTemplateId.HasValue )
                {
                    placementConfigurationJSON = preferences.GetValue( string.Format( PreferenceKey.PlacementConfigurationJSONRegistrationTemplateId, registrationTemplateId.Value ) );
                }

                _placementConfiguration = placementConfigurationJSON.FromJsonOrNull<PlacementConfigurationSettingsBag>() ?? new PlacementConfigurationSettingsBag();
            }

            return _placementConfiguration;
        }

        private PlacementConfigurationSettingsBag _placementConfiguration = null;

        #endregion Methods

        #region Helper Methods

        /// <summary>
        /// Gets the ID from the page parameter accounting for idKeys (Obsidian) or plain ids (Webforms).
        /// </summary>
        /// <param name="pageParameterKey">The key of the page parameter.</param>
        /// <returns>The ID as an integer if found; otherwise, null.</returns>
        private int? GetIdFromPageParameter( string pageParameterKey )
        {
            var idParam = PageParameter( pageParameterKey );
            return Rock.Utility.IdHasher.Instance.GetId( idParam ) ?? idParam.AsIntegerOrNull();
        }

        #endregion Helper Methods

        #region Block Actions



        #endregion Block Actions
    }
}

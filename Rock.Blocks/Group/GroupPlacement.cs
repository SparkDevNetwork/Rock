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
using System.Data.SqlClient;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.ViewModels.Blocks.Group.GroupPlacement;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

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
            public const string PersonAttributeFilterRegistrationInstanceId = "PersonAttributeFilter_RegistrationInstanceId_{0}";
            public const string PersonAttributeFilterRegistrationTemplateId = "PersonAttributeFilter_RegistrationTemplateId_{0}";
            public const string GroupAttributeFilterGroupTypeId = "GroupAttributeFilter_GroupTypeId_{0}";
            public const string GroupMemberAttributeFilterGroupTypeId = "GroupMemberAttributeFilter_GroupTypeId_{0}";
            public const string RegistrantFeeItemValuesForFiltersJSONRegistrationInstanceId = "RegistrantFeeItemValuesForFiltersJSON_RegistrationInstanceId_{0}";
            public const string RegistrantFeeItemValuesForFiltersJSONRegistrationTemplateId = "RegistrantFeeItemValuesForFiltersJSON_RegistrationTemplateId_{0}";
        }

        private static class PageParameterKey
        {
            public const string RegistrantId = "RegistrantId";
            public const string RegistrationInstanceId = "RegistrationInstanceId";
            public const string RegistrationTemplateId = "RegistrationTemplateId";
            public const string RegistrationTemplatePlacementId = "RegistrationTemplatePlacementId";
            public const string PromptForTemplatePlacement = "PromptForTemplatePlacement";
            public const string ReturnUrl = "ReturnUrl";
        }

        private static class NavigationUrlKey
        {
            public const string GroupDetailPage = "GroupDetailPage";
            public const string GroupMemberDetailPage = "GroupMemberDetailPage";
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
            public int? PersonId { get; set; }
            public int? GroupRoleId { get; set; }
            public int? GroupMemberId { get; set; }
        }

        private class DestinationGroupResult
        {
            public int GroupId { get; set; }
            public string GroupName { get; set; }
            public int? GroupCapacity { get; set; }
            public int GroupTypeId { get; set; }
            public int GroupOrder { get; set; }
            public bool IsShared { get; set; }
        }

        private class PlacementPeopleResult
        {
            // Group Member info
            public int? GroupId { get; set; }
            public int? GroupTypeId { get; set; }
            public int? GroupMemberId { get; set; }
            public int? GroupRoleId { get; set; }

            // Person info
            public int PersonId { get; set; }
            public string FirstName { get; set; }
            public string NickName { get; set; }
            public string LastName { get; set; }
            public Gender Gender { get; set; }
            public int? PhotoId { get; set; }
            public int Age { get; set; }
            public int? RecordTypeValueId { get; set; }
            public AgeClassification AgeClassification { get; set; }

            // Registrant info
            public int? RegistrantId { get; set; }
            public string RegistrationInstanceName { get; set; }
            public int? RegistrationInstanceId { get; set; }
            public DateTime? CreatedDateTime { get; set; }
            // TODO - Add Fees
            public string FeeName { get; set; }
            public string Option { get; set; }
            public int? Quantity { get; set; }
            public decimal? Cost { get; set; }
            public RegistrationFeeType? FeeType { get; set; }
            public int? FeeItemId { get; set; }
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
            box.NavigationUrls = GetBoxNavigationUrls();
            box.BackPageUrl = GetBackPageUrl();

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

            // TODO - optimize fetching attributes (I'm getting this data 3 different ways currently)
            box.GroupAttributes = fakeGroup.GetPublicAttributesForEdit( GetCurrentPerson(), true );

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
            HashSet<int> displayedAttributeIds = new HashSet<int>();
            HashSet<int> groupAttributeIds = new HashSet<int>();
            HashSet<int> groupMemberAttributeIds = new HashSet<int>();
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

            if ( placementConfiguration.AreFeesDisplayed )
            {
                var feeItemList = new RegistrationTemplateFeeService( RockContext )
                    .Queryable()
                    .SelectMany( fee => fee.FeeItems.Select( item => new ListItemBag
                    {
                        Value = item.Id.ToString(),
                        Text = fee.Name + " - " + item.Name
                    } ) )
                    .ToList();

                attributeFiltersBag.RegistrantFeeItemsForFilters = feeItemList;
            }

            box.AttributeFilters = attributeFiltersBag;

            //if ( box.PlacementGroupType.Id.HasValue && registrationTemplatePlacementId.HasValue )
            //{
            //    //box.PlacementGroups = GetPlacementGroups( registrationTemplatePlacementId.Value, registrationInstanceId, registrationTemplateId, box.PlacementGroupType.Id.Value );
            //    // TODO - add logic for cases where we just get a single registrant
            //    // TODO - add logic for Included Registration Instances


            //    var placementGroupDetails = GetPlacementGroupDetails( registrationTemplatePlacementId.Value, registrationInstanceId, registrationTemplateId.Value );

            //    RockContext.SqlLogging( true );
            //    var placementPeople = GetPlacementPeople( registrationTemplateId.Value, registrationInstanceId, registrantId );
            //    RockContext.SqlLogging( false );
            //    // Probably very inefficient.
            //    GroupMemberService groupMemberService = new GroupMemberService( RockContext );

            //    var groupMemberIds = placementGroupDetails
            //        .SelectMany( g => g.GroupMembers )
            //        .Where( gm => gm.GroupMemberId != null )
            //        .Select( gm => gm.GroupMemberId )
            //        .Distinct()
            //        .ToList();

            //    var personLookup = placementPeople.ToDictionary( p => p.PersonId );

            //    // Populate the placement groups with members
            //    var placementGroups = placementGroupDetails.Select( group => new PlacementGroupBag
            //    {
            //        GroupId = group.GroupId,
            //        GroupName = group.GroupName,
            //        GroupOrder = group.GroupOrder,
            //        GroupTypeId = group.GroupTypeId,
            //        GroupCapacity = group.GroupCapacity,
            //        Attributes = GetGroupAttributes( group.GroupId, group.GroupTypeId, groupAttributeIds ),
            //        AttributeValues = GetGroupAttributeValues( group.GroupId, group.GroupTypeId, groupAttributeIds ),
            //        GroupMembers = group.GroupMembers
            //            .Where( gm => gm.GroupMemberId.HasValue && gm.Person.PersonId.HasValue && personLookup.ContainsKey( gm.Person.PersonId ) )
            //            .Select( gm =>
            //            {
            //                var person = personLookup[gm.Person.PersonId.Value];

            //                return new GroupMemberBag
            //                {
            //                    GroupMemberId = gm.GroupMemberId,
            //                    GroupRoleId = gm.GroupRoleId,
            //                    Attributes = GetGroupMemberAttributes( gm.GroupMemberId.Value, group.GroupTypeId, groupMemberAttributeIds ),
            //                    AttributeValues = GetGroupMemberAttributeValues( gm.GroupMemberId.Value, group.GroupTypeId, groupMemberAttributeIds ),
            //                    Person = new PersonBag
            //                    {
            //                        PersonId = person.PersonId,
            //                        FirstName = person.FirstName,
            //                        Nickname = person.Nickname,
            //                        LastName = person.LastName,
            //                        Gender = person.Gender,
            //                        PhotoUrl = person.PhotoUrl,
            //                        Registrants = person.Registrants
            //                    }
            //                };
            //            } )
            //            .ToList()
            //    } ).ToList();

            //    // Populate the people that need to be placed.
            //    var placedPersonIds = placementGroups
            //        .SelectMany( g => g.GroupMembers )
            //        .Select( m => m.Person.PersonId )
            //        .ToHashSet();

            //    var peopleToPlace = placementPeople
            //        .Where( p => !placedPersonIds.Contains( p.PersonId ) )
            //        .ToList();

            //    box.PlacementGroups = placementGroups;
            //    box.PeopleToPlace = peopleToPlace;
            //}

            return box;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.GroupDetailPage] = this.GetLinkedPageUrl( AttributeKey.GroupDetailPage, new Dictionary<string, string> { ["GroupId"] = "((Key))", ["autoEdit"] = "true", ["returnUrl"] = this.GetCurrentPageUrl() } ),
                [NavigationUrlKey.GroupMemberDetailPage] = this.GetLinkedPageUrl( AttributeKey.GroupMemberDetailPage, new Dictionary<string, string> { ["GroupMemberId"] = "((Key))", ["autoEdit"] = "true", ["returnUrl"] = this.GetCurrentPageUrl() } ),
            };
        }

        /// <summary>
        /// Gets the "Back" page URL.
        /// </summary>
        private string GetBackPageUrl()
        {
            var returnUrl = this.PageParameter( PageParameterKey.ReturnUrl );
            if ( returnUrl.IsNotNullOrWhiteSpace() )
            {
                return returnUrl;
            }

            return string.Empty;
        }

        //private List<PersonBag> GetPlacementPeople( int registrationTemplateId, int? registrationInstanceId, int? registrantId )
        //{
        //    var registrationRegistrantService = new RegistrationRegistrantService( RockContext );
        //    var registrationRegistrantQuery = registrationRegistrantService.Queryable();
        //    var placementConfiguration = GetPlacementConfiguration( registrationInstanceId, registrationTemplateId );

            //    HashSet<int> includedRegistrationInstanceIds = null;
            //    if ( placementConfiguration.IncludedRegistrationInstanceIds?.Any() == true )
            //    {
            //        includedRegistrationInstanceIds = placementConfiguration.IncludedRegistrationInstanceIds?.Select( id => id.AsIntegerOrNull() )
            //            .Where( id => id.HasValue )
            //            .Select( id => id.Value )
            //            .ToHashSet();
            //    }

            //    if ( registrationInstanceId.HasValue )
            //    {
            //        registrationRegistrantQuery = registrationRegistrantQuery.Where( a => a.Registration.RegistrationInstanceId == registrationInstanceId.Value );
            //    }
            //    else if ( includedRegistrationInstanceIds != null )
            //    {
            //        registrationRegistrantQuery = registrationRegistrantQuery.Where( a => includedRegistrationInstanceIds.Contains( a.Registration.RegistrationInstanceId ) );
            //    }
            //    else
            //    {
            //        var instanceIds = new RegistrationInstanceService( RockContext )
            //            .Queryable()
            //            .Where( i => i.RegistrationTemplateId == registrationTemplateId )
            //            .Select( i => i.Id );

            //        registrationRegistrantQuery = registrationRegistrantQuery.Where( rr => instanceIds.Contains( rr.Registration.RegistrationInstanceId ) );
            //    }

            //    if ( registrantId.HasValue )
            //    {
            //        registrationRegistrantQuery = registrationRegistrantQuery.Where( a => a.Id == registrantId.Value );
            //    }

            //    HashSet<int> displayedAttributeIds = null;

            //    if ( placementConfiguration.SourceAttributesToDisplay?.Any() == true )
            //    {
            //        displayedAttributeIds = placementConfiguration.SourceAttributesToDisplay
            //            .Select( id => id.AsIntegerOrNull() )
            //            .Where( id => id.HasValue )
            //            .Select( id => id.Value )
            //            .ToHashSet();
            //    }

            //    var registrants = registrationRegistrantQuery
            //        .Where( r => r.PersonAlias != null && r.PersonAlias.Person != null )
            //        .Select( r => new
            //        {
            //            PersonId = r.PersonAlias.Person.Id,
            //            r.PersonAlias.Person.FirstName,
            //            r.PersonAlias.Person.NickName,
            //            r.PersonAlias.Person.LastName,
            //            r.PersonAlias.Person.Gender,
            //            r.PersonAlias.Person.PhotoId,
            //            r.PersonAlias.Person.Age,
            //            PersonRecordTypeValueId = r.PersonAlias.Person.RecordTypeValueId,
            //            r.PersonAlias.Person.AgeClassification,
            //            RegistrantId = r.Id,
            //            r.CreatedDateTime,
            //            r.RegistrationTemplateId,
            //            RegistrationInstanceId = r.Registration.RegistrationInstance.Id,
            //            RegistrationInstanceName = r.Registration.RegistrationInstance.Name,
            //        } )
            //        .ToList();

            //    // TODO - Consider Grouping in SQL
            //    // TODO - Handle Fees

            //    var people = registrants
            //        .GroupBy( r => r.PersonId )
            //        .Select( g => new PersonBag
            //        {
            //            PersonId = g.Key,
            //            FirstName = g.First().FirstName,
            //            Nickname = g.First().NickName,
            //            LastName = g.First().LastName,
            //            Gender = g.First().Gender,
            //            PhotoUrl = Rock.Model.Person.GetPersonPhotoUrl( $"{g.First().NickName.Truncate( 1, false )}{g.First().LastName.Truncate( 1, false )}", g.First().PhotoId, g.First().Age, g.First().Gender, g.First().PersonRecordTypeValueId, g.First().AgeClassification ),
            //            Registrants = g.Select( r => new RegistrantBag
            //            {
            //                RegistrantId = r.RegistrantId,
            //                RegistrationInstanceId = r.RegistrationInstanceId,
            //                CreatedDateTime = r.CreatedDateTime,
            //                RegistrationInstanceName = r.RegistrationInstanceName,
            //                Attributes = GetRegistrantAttributes(r.RegistrantId, r.RegistrationTemplateId, displayedAttributeIds ),
            //                AttributeValues = GetRegistrantAttributeValues( r.RegistrantId, r.RegistrationTemplateId, displayedAttributeIds )
            //            } ).ToList()
            //        } )
            //        .ToList();

            //    return people;
            //}

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


        private Dictionary<string, PublicAttributeBag> GetGroupAttributes( int groupId, int groupTypeId, HashSet<int> displayedAttributeIds )
        {
            if ( displayedAttributeIds == null )
            {
                // don't spend time loading attributes if there aren't any to be displayed
                return null;
            }

            Rock.Model.Group group = new Rock.Model.Group
            {
                Id = groupId,
                GroupTypeId = groupTypeId,
            };

            if ( group.AttributeValues == null )
            {
                group.LoadAttributes();
            }

            var attributes = group.GetPublicAttributesForView( GetCurrentPerson(), true, attributeFilter: a => displayedAttributeIds.Contains( a.Id ) );

            return attributes;
        }

        private Dictionary<string, string> GetGroupAttributeValues( int groupId, int groupTypeId, HashSet<int> displayedAttributeIds )
        {
            if ( displayedAttributeIds == null )
            {
                // don't spend time loading attributes if there aren't any to be displayed
                return null;
            }

            Rock.Model.Group group = new Rock.Model.Group
            {
                Id = groupId,
                GroupTypeId = groupTypeId,
            };

            if ( group.AttributeValues == null )
            {
                group.LoadAttributes();
            }

            var attributeValues = group.GetPublicAttributeValuesForView( GetCurrentPerson(), true, attributeFilter: a => displayedAttributeIds.Contains( a.Id ) );

            return attributeValues;
        }
        private Dictionary<string, PublicAttributeBag> GetGroupMemberAttributes( int? groupMemberId, int? groupTypeId, HashSet<int> displayedAttributeIds )
        {
            if ( displayedAttributeIds == null || !groupMemberId.HasValue || !groupTypeId.HasValue )
            {
                // don't spend time loading attributes if there aren't any to be displayed
                return null;
            }

            GroupMember groupMember = new GroupMember
            {
                Id = groupMemberId.Value,
                GroupTypeId = groupTypeId.Value,
            };

            if ( groupMember.AttributeValues == null )
            {
                groupMember.LoadAttributes();
            }

            var attributes = groupMember.GetPublicAttributesForView( GetCurrentPerson(), true, attributeFilter: a => displayedAttributeIds.Contains( a.Id ) );

            return attributes;
        }

        private Dictionary<string, string> GetGroupMemberAttributeValues( int? groupMemberId, int? groupTypeId, HashSet<int> displayedAttributeIds )
        {
            if ( displayedAttributeIds == null || !groupMemberId.HasValue || !groupTypeId.HasValue )
            {
                // don't spend time loading attributes if there aren't any to be displayed
                return null;
            }

            GroupMember groupMember = new GroupMember
            {
                Id = groupMemberId.Value,
                GroupTypeId = groupTypeId.Value,
            };

            if ( groupMember.AttributeValues == null )
            {
                groupMember.LoadAttributes();
            }

            var attributeValues = groupMember.GetPublicAttributeValuesForView( GetCurrentPerson(), true, attributeFilter: a => displayedAttributeIds.Contains( a.Id ) );

            return attributeValues;
        }

        private Dictionary<string, PublicAttributeBag> GetRegistrantAttributes( int registrantId, int registrationTemplateId, HashSet<int> displayedAttributeIds )
        {
            if ( displayedAttributeIds == null )
            {
                // don't spend time loading attributes if there aren't any to be displayed
                return null;
            }

            RegistrationRegistrant registrant = new RegistrationRegistrant
            {
                Id = registrantId,
                RegistrationTemplateId = registrationTemplateId,
            };

            if ( registrant.AttributeValues == null )
            {
                registrant.LoadAttributes();
            }

            var attributes = registrant.GetPublicAttributesForView( GetCurrentPerson(), true, attributeFilter: a => displayedAttributeIds.Contains( a.Id ) );

            return attributes;
        }

        private Dictionary<string, string> GetRegistrantAttributeValues( int registrantId, int registrationTemplateId, HashSet<int> displayedAttributeIds )
        {
            if ( displayedAttributeIds == null )
            {
                // don't spend time loading attributes if there aren't any to be displayed
                return null;
            }

            RegistrationRegistrant registrant = new RegistrationRegistrant
            {
                Id = registrantId,
                RegistrationTemplateId = registrationTemplateId,
            };

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

        private List<string> GetRegistrantFeeItemValuesForFilters( int? registrationInstanceId, int? registrationTemplateId )
        {
            var preferences = GetBlockPersonPreferences();

            string registrantFeeItemValuesJSON = string.Empty;
            if ( registrationInstanceId.HasValue )
            {
                registrantFeeItemValuesJSON = preferences.GetValue( string.Format( PreferenceKey.RegistrantFeeItemValuesForFiltersJSONRegistrationInstanceId, registrationInstanceId.Value ) );
            }
            else if ( registrationTemplateId.HasValue )
            {
                registrantFeeItemValuesJSON = preferences.GetValue( string.Format( PreferenceKey.RegistrantFeeItemValuesForFiltersJSONRegistrationTemplateId, registrationTemplateId.Value ) );
            }

            return registrantFeeItemValuesJSON.FromJsonOrNull<List<string>>() ?? new List<string>();
        }

        private bool CanPlacePeople( List<int?> personIds, int registrationTemplatePlacementId, int groupId, out string errorMessage )
        {
            RegistrationTemplatePlacementService registrationTemplatePlacementService = new RegistrationTemplatePlacementService( RockContext );

            var registrationTemplatePlacement = registrationTemplatePlacementService.Get( registrationTemplatePlacementId );

            if ( registrationTemplatePlacement == null )
            {
                errorMessage = "Specified RegistrationTemplatePlacement not found.";
                return false;
            }

            var group = new GroupService( RockContext ).Get( groupId );

            if ( group == null )
            {
                errorMessage = "Specified Group not found.";
                return false;
            }

            var peopleInfo = new PersonService( RockContext )
                .Queryable()
                .Where( p => personIds.Contains( p.Id ) )
                .Select( p => new
                {
                    p.Id,
                } )
                .ToList();

            if ( !peopleInfo.Any() )
            {
                errorMessage = "No valid people to place found.";
                return false;
            }

            if ( registrationTemplatePlacement.GroupTypeId != group.GroupTypeId )
            {
                errorMessage = "Specified group's group type does not match the group type of the registration placement.";
                return false;
            }

            // TODO - Logic to check if allow multiple conditions are met.

            errorMessage = string.Empty;
            return true;
        }

        private bool IsGroupRoleCapacityAvailable( int roleId, int groupId, int pendingGroupMemberCount, out string errorMessage )
        {
            var group = new GroupService( RockContext ).Get( groupId );
            var role = GroupTypeCache.Get( group.GroupTypeId )?.Roles.FirstOrDefault( r => r.Id == roleId );

            if ( role?.MaxCount is int maxCount )
            {
                var currentCount = new GroupMemberService( RockContext )
                    .Queryable()
                    .Count( gm => gm.GroupId == groupId && gm.GroupRoleId == roleId );

                if ( currentCount + pendingGroupMemberCount > maxCount )
                {
                    errorMessage = $"The number of {role.Name.Pluralize().ToLower()} for this group is above its maximum allowed limit of {maxCount} active members.";
                    return false;
                }
            }
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Determines whether [is valid parent group] [the specified parent group].
        /// </summary>
        /// <param name="parentGroup">The parent group.</param>
        /// <param name="childGroupTypeId">The child group type identifier.</param>
        /// <returns>
        ///   <c>true</c> if [is valid parent group] [the specified parent group]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsValidParentGroup( Rock.Model.Group parentGroup, int childGroupTypeId )
        {
            bool isValidParentGroup = true;

            if ( parentGroup != null )
            {
                var parentGroupGroupType = GroupTypeCache.Get( parentGroup.GroupTypeId );
                isValidParentGroup = parentGroupGroupType.AllowAnyChildGroupType || parentGroupGroupType.ChildGroupTypes.Any( a => a.Id == childGroupTypeId );
            }

            return isValidParentGroup;
        }

        /// <summary>
        /// Determines whether [is valid existing group] [the specified selected group].
        /// </summary>
        /// <param name="selectedGroup">The selected group.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns>
        ///   <c>true</c> if [is valid existing group] [the specified selected group]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsValidExistingGroup( Rock.Model.Group selectedGroup, int groupTypeId )
        {
            return selectedGroup?.GroupTypeId == groupTypeId;
        }

        /// <summary>
        /// Determines whether [has valid child groups] [the specified parent group identifier].
        /// </summary>
        /// <param name="parentGroupId">The parent group identifier.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if [has valid child groups] [the specified parent group identifier]; otherwise, <c>false</c>.
        /// </returns>
        private bool HasValidChildGroups( int parentGroupId, int groupTypeId, out string errorMessage )
        {
            var childPlacementGroups = new GroupService( new RockContext() ).Queryable().Where( a => a.ParentGroupId == parentGroupId && a.IsActive == true ).ToList();
            if ( childPlacementGroups.Count() == 0 )
            {
                errorMessage = "The selected parent group does not have any active child groups.";
                return false;
            }

            List<Rock.Model.Group> invalidGroups = new List<Rock.Model.Group>();
            foreach ( var childPlacementGroup in childPlacementGroups )
            {
                if ( !IsValidExistingGroup( childPlacementGroup, groupTypeId ) )
                {
                    invalidGroups.Add( childPlacementGroup );
                }
            }

            if ( invalidGroups.Any() )
            {
                var groupType = GroupTypeCache.Get( groupTypeId );
                errorMessage = string.Format( "The child groups of this parent group must be {0} groups.", groupType );
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        private Dictionary<string, ListItemBag> GetFormattedFeesForRegistrant(
            IEnumerable<PlacementPeopleResult> personGroup,
            int registrantId )
        {
            return personGroup
                .Where( f => f.RegistrantId == registrantId && f.Quantity > 0 && f.FeeItemId.HasValue )
                .ToDictionary(
                    f => f.FeeItemId.Value.ToString(),
                    f =>
                    {
                        var feeLabel = f.FeeType == RegistrationFeeType.Multiple && f.Option.IsNotNullOrWhiteSpace()
                            ? $"{f.FeeName}-{f.Option}"
                            : f.Option;
                        var costText = f.Quantity > 1
                            ? $"{f.Quantity} at {f.Cost?.FormatAsCurrency()}" // TODO - check for null
                            : ( ( f.Quantity ?? 0 ) * ( f.Cost ?? 0 ) ).FormatAsCurrency();

                        return new ListItemBag
                        {
                            Text = feeLabel,
                            Value = costText
                        };
                    }
                );
        }

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

        [BlockAction]
        public BlockActionResult GetPlacementPeople( int? registrationTemplateId, int? registrationInstanceId, int? registrationTemplatePlacementId )
        {
            int registrationTemplatePlacementEntityTypeId = EntityTypeCache.Get<Rock.Model.RegistrationTemplatePlacement>().Id;
            int registrationInstanceEntityTypeId = EntityTypeCache.Get<Rock.Model.RegistrationInstance>().Id;
            int groupEntityTypeId = EntityTypeCache.Get<Rock.Model.Group>().Id;
            string placementMode;
            List<PlacementPeopleResult> placementPeopleResults;

            if ( !registrationTemplateId.HasValue || !registrationTemplatePlacementId.HasValue )
            {
                // TODO - refactor how ids are retrieved.
                return ActionBadRequest("Could not find Registration Template Id");
            }

            var placementConfiguration = GetPlacementConfiguration( registrationInstanceId, registrationTemplateId );
            var includedRegistrationInstanceIds = ( placementConfiguration.IncludedRegistrationInstanceIds?.Any() == true )
                ? string.Join( ",", placementConfiguration.IncludedRegistrationInstanceIds )
                : string.Empty;
            var includeFees = placementConfiguration.AreFeesDisplayed;
            var includedFeeItemIdsFromPreferences = GetRegistrantFeeItemValuesForFilters( registrationInstanceId, registrationTemplateId );
            var includedFeeItemIds = string.Join( ",", includedFeeItemIdsFromPreferences );

            // If registration instance id has a value than we are in instance mode
            if ( registrationInstanceId.HasValue )
            {
                placementMode = "InstanceMode";

                placementPeopleResults = RockContext.Database.SqlQuery<PlacementPeopleResult>(
                    $@"EXEC [dbo].[spGetGroupPlacementPeople] 
                        @{nameof( registrationTemplatePlacementEntityTypeId )}, 
                        @{nameof( registrationInstanceEntityTypeId )}, 
                        @{nameof( groupEntityTypeId )}, 
                        @{nameof( registrationTemplateId )}, 
                        @{nameof( registrationInstanceId )}, 
                        @{nameof( registrationTemplatePlacementId )}, 
                        @{nameof( placementMode )},
                        @{nameof( includedRegistrationInstanceIds )},
                        @{nameof( includeFees )},
                        @{nameof( includedFeeItemIds )}",
                    new SqlParameter( nameof( registrationTemplatePlacementEntityTypeId ), registrationTemplatePlacementEntityTypeId ),
                    new SqlParameter( nameof( registrationInstanceEntityTypeId ), registrationInstanceEntityTypeId ),
                    new SqlParameter( nameof( groupEntityTypeId ), groupEntityTypeId ),
                    new SqlParameter( nameof( registrationTemplateId ), registrationTemplateId ),
                    new SqlParameter( nameof( registrationInstanceId ), registrationInstanceId.Value ),
                    new SqlParameter( nameof( registrationTemplatePlacementId ), registrationTemplatePlacementId ),
                    new SqlParameter( nameof( placementMode ), placementMode ),
                    new SqlParameter( nameof( includedRegistrationInstanceIds ), includedRegistrationInstanceIds ),
                    new SqlParameter( nameof( includeFees ), includeFees ),
                    new SqlParameter( nameof( includedFeeItemIds ), includedFeeItemIds )
                ).ToList();
            }
            else
            {
                placementMode = "TemplateMode";

                placementPeopleResults = RockContext.Database.SqlQuery<PlacementPeopleResult>(
                    $@"EXEC [dbo].[spGetGroupPlacementPeople] 
                        @{nameof( registrationTemplatePlacementEntityTypeId )}, 
                        @{nameof( registrationInstanceEntityTypeId )}, 
                        @{nameof( groupEntityTypeId )}, 
                        @{nameof( registrationTemplateId )}, 
                        @{nameof( registrationInstanceId )}, 
                        @{nameof( registrationTemplatePlacementId )}, 
                        @{nameof( placementMode )},
                        @{ nameof( includedRegistrationInstanceIds )},
                        @{nameof( includeFees )},
                        @{nameof( includedFeeItemIds )}",
                    new SqlParameter( nameof( registrationTemplatePlacementEntityTypeId ), registrationTemplatePlacementEntityTypeId ),
                    new SqlParameter( nameof( registrationInstanceEntityTypeId ), registrationInstanceEntityTypeId ),
                    new SqlParameter( nameof( groupEntityTypeId ), groupEntityTypeId ),
                    new SqlParameter( nameof( registrationTemplateId ), registrationTemplateId ),
                    new SqlParameter( nameof( registrationInstanceId ), DBNull.Value ),
                    new SqlParameter( nameof( registrationTemplatePlacementId ), registrationTemplatePlacementId ),
                    new SqlParameter( nameof( placementMode ), placementMode ),
                    new SqlParameter( nameof( includedRegistrationInstanceIds ), includedRegistrationInstanceIds ),
                    new SqlParameter( nameof( includeFees ), includeFees ),
                    new SqlParameter( nameof( includedFeeItemIds ), includedFeeItemIds )
                ).ToList();
            }

            HashSet<int> displayedRegistrantAttributeIds = null;
            HashSet<int> displayedGroupMemberAttributeIds = null;

            if ( placementConfiguration.SourceAttributesToDisplay?.Any() == true )
            {
                displayedRegistrantAttributeIds = placementConfiguration.SourceAttributesToDisplay
                    .Select( id => id.AsIntegerOrNull() )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToHashSet();
            }

            if ( placementConfiguration.GroupMemberAttributesToDisplay?.Any() == true )
            {
                displayedGroupMemberAttributeIds = placementConfiguration.GroupMemberAttributesToDisplay
                    .Select( id => id.AsIntegerOrNull() )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToHashSet();
            }

            var placementPeopleBag = new PlacementPeopleBag
            {
                TempGroups = new List<PlacementGroupBag>(),
                PeopleToPlace = new List<PersonBag>()
            };

            // Group flat results by PersonId
            var peopleGroups = placementPeopleResults
                .GroupBy( r => r.PersonId )
                .ToList();

            foreach ( var personGroup in peopleGroups )
            {
                var firstResult = personGroup.First();

                var personBag = new PersonBag
                {
                    PersonId = firstResult.PersonId,
                    FirstName = firstResult.FirstName,
                    Nickname = firstResult.NickName,
                    LastName = firstResult.LastName,
                    Gender = firstResult.Gender,
                    PhotoUrl = Rock.Model.Person.GetPersonPhotoUrl(
                        $"{firstResult.NickName.Truncate( 1, false )}{firstResult.LastName.Truncate( 1, false )}",
                        firstResult.PhotoId,
                        firstResult.Age,
                        firstResult.Gender,
                        firstResult.RecordTypeValueId,
                        firstResult.AgeClassification
                    ),
                    Registrants = personGroup
                        .Where( r => r.RegistrantId.HasValue )
                        .DistinctBy( r => r.RegistrantId )
                        .Select( r => new RegistrantBag
                        {
                            RegistrantId = r.RegistrantId.Value,
                            RegistrationInstanceName = r.RegistrationInstanceName,
                            RegistrationInstanceId = r.RegistrationInstanceId ?? 0,
                            CreatedDateTime = r.CreatedDateTime?.ToRockDateTimeOffset(),
                            Fees = GetFormattedFeesForRegistrant(personGroup, r.RegistrantId.Value),
                            Attributes = GetRegistrantAttributes( r.RegistrantId.Value, registrationTemplateId.Value, displayedRegistrantAttributeIds ),
                            AttributeValues = GetRegistrantAttributeValues( r.RegistrantId.Value, registrationTemplateId.Value, displayedRegistrantAttributeIds )
                        } )
                        .ToList()
                };

                // If GroupId exists, put them in TempGroups
                if ( firstResult.GroupId.HasValue )
                {
                    var groupBag = placementPeopleBag.TempGroups.FirstOrDefault( g => g.GroupId == firstResult.GroupId );

                    if ( groupBag == null )
                    {
                        groupBag = new PlacementGroupBag
                        {
                            GroupId = firstResult.GroupId.Value,
                            GroupMembers = new List<GroupMemberBag>()
                        };
                        placementPeopleBag.TempGroups.Add( groupBag );
                    }

                    var groupMemberBag = new GroupMemberBag
                    {
                        GroupMemberId = firstResult.GroupMemberId,
                        GroupRoleId = firstResult.GroupRoleId,
                        Attributes = GetGroupMemberAttributes( firstResult.GroupMemberId, firstResult.GroupTypeId, displayedGroupMemberAttributeIds ),
                        AttributeValues = GetGroupMemberAttributeValues( firstResult.GroupMemberId, firstResult.GroupTypeId, displayedGroupMemberAttributeIds ),
                        Person = personBag
                    };

                    groupBag.GroupMembers.Add( groupMemberBag );
                }
                else
                {
                    // No GroupId > Add directly to PeopleToPlace
                    placementPeopleBag.PeopleToPlace.Add( personBag );
                }
            }

            return ActionOk( placementPeopleBag );

            //return placementGroupDetails;
        }

        [BlockAction]
        public BlockActionResult GetDestinationGroups( int? registrationTemplateId, int? registrationInstanceId, int? registrationTemplatePlacementId )
        {
            int registrationTemplatePlacementEntityTypeId = EntityTypeCache.Get<Rock.Model.RegistrationTemplatePlacement>().Id;
            int registrationInstanceEntityTypeId = EntityTypeCache.Get<Rock.Model.RegistrationInstance>().Id;
            int groupEntityTypeId = EntityTypeCache.Get<Rock.Model.Group>().Id;
            string placementMode;
            List<DestinationGroupResult> destinationGroupResults;

            if ( !registrationTemplatePlacementId.HasValue )
            {
                // TODO - refactor how ids are retrieved.
                return ActionBadRequest( "Could not find Registration Template Id" );
            }

            var placementConfiguration = GetPlacementConfiguration( registrationInstanceId, registrationTemplateId );
            var includedRegistrationInstanceIds = ( placementConfiguration.IncludedRegistrationInstanceIds?.Any() == true )
                ? string.Join( ",", placementConfiguration.IncludedRegistrationInstanceIds )
                : string.Empty;

            // If registration instance id has a value than we are in instance mode
            if ( registrationInstanceId.HasValue )
            {
                placementMode = "InstanceMode";

                destinationGroupResults = RockContext.Database.SqlQuery<DestinationGroupResult>(
                    $@"EXEC [dbo].[spGetDestinationGroups] 
                        @{nameof( registrationTemplatePlacementEntityTypeId )}, 
                        @{nameof( registrationInstanceEntityTypeId )}, 
                        @{nameof( groupEntityTypeId )}, 
                        @{nameof( registrationTemplateId )}, 
                        @{nameof( registrationInstanceId )}, 
                        @{nameof( registrationTemplatePlacementId )}, 
                        @{nameof( placementMode )},
                        @{ nameof( includedRegistrationInstanceIds )}",
                    new SqlParameter( nameof( registrationTemplatePlacementEntityTypeId ), registrationTemplatePlacementEntityTypeId ),
                    new SqlParameter( nameof( registrationInstanceEntityTypeId ), registrationInstanceEntityTypeId ),
                    new SqlParameter( nameof( groupEntityTypeId ), groupEntityTypeId ),
                    new SqlParameter( nameof( registrationTemplateId ), registrationTemplateId ),
                    new SqlParameter( nameof( registrationInstanceId ), registrationInstanceId.Value ),
                    new SqlParameter( nameof( registrationTemplatePlacementId ), registrationTemplatePlacementId ),
                    new SqlParameter( nameof( placementMode ), placementMode ),
                    new SqlParameter( nameof( includedRegistrationInstanceIds ), includedRegistrationInstanceIds )
                ).ToList();
            }
            else
            {
                placementMode = "TemplateMode";

                destinationGroupResults = RockContext.Database.SqlQuery<DestinationGroupResult>(
                    $@"EXEC [dbo].[spGetDestinationGroups] 
                        @{nameof( registrationTemplatePlacementEntityTypeId )}, 
                        @{nameof( registrationInstanceEntityTypeId )}, 
                        @{nameof( groupEntityTypeId )}, 
                        @{nameof( registrationTemplateId )}, 
                        @{nameof( registrationInstanceId )}, 
                        @{nameof( registrationTemplatePlacementId )}, 
                        @{nameof( placementMode )},
                        @{nameof( includedRegistrationInstanceIds )}",
                    new SqlParameter( nameof( registrationTemplatePlacementEntityTypeId ), registrationTemplatePlacementEntityTypeId ),
                    new SqlParameter( nameof( registrationInstanceEntityTypeId ), registrationInstanceEntityTypeId ),
                    new SqlParameter( nameof( groupEntityTypeId ), groupEntityTypeId ),
                    new SqlParameter( nameof( registrationTemplateId ), registrationTemplateId ),
                    new SqlParameter( nameof( registrationInstanceId ), DBNull.Value ),
                    new SqlParameter( nameof( registrationTemplatePlacementId ), registrationTemplatePlacementId ),
                    new SqlParameter( nameof( placementMode ), placementMode ),
                    new SqlParameter( nameof( includedRegistrationInstanceIds ), includedRegistrationInstanceIds )
                ).ToList();
            }

            HashSet<int> groupIdsToDisplay = new HashSet<int>();

            if ( placementConfiguration.GroupAttributesToDisplay?.Any() == true )
            {
                groupIdsToDisplay = placementConfiguration.GroupAttributesToDisplay
                    .Select( id => id.AsIntegerOrNull() )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToHashSet();
            }

            var placementGroupBags = destinationGroupResults
                .GroupBy( g => g.GroupId )
                .Select( g => new PlacementGroupBag
                {
                    GroupId = g.Key,
                    GroupName = g.First().GroupName,
                    GroupCapacity = g.First().GroupCapacity,
                    GroupTypeId = g.First().GroupTypeId,
                    GroupOrder = g.First().GroupOrder,
                    IsShared = g.First().IsShared,
                    Attributes = GetGroupAttributes( g.Key, g.First().GroupTypeId, groupIdsToDisplay ),
                    AttributeValues = GetGroupAttributeValues( g.Key, g.First().GroupTypeId, groupIdsToDisplay )
                } ).ToList();

            return ActionOk( placementGroupBags );
        }

        [BlockAction]
        public BlockActionResult AddGroupMembersToGroup( List<GroupMemberBag> pendingGroupMembers, int registrationTemplatePlacementId, int groupId )
        {
            var personIds = pendingGroupMembers
                .Where( gm => gm?.Person != null )
                .Select( gm => gm.Person.PersonId )
                .ToList();
            string errorMessage = string.Empty;

            var canPlacePeople = CanPlacePeople( personIds, registrationTemplatePlacementId, groupId, out errorMessage );

            if ( !canPlacePeople )
            {
                return ActionBadRequest( errorMessage );
            }

            var groupMemberService = new GroupMemberService( RockContext );
            var memberPairs = new List<(GroupMemberBag Bag, GroupMember Entity)>();

            // Handle edge case where adding multiple group members could exceed the max count and throw an exception
            if (
                pendingGroupMembers.Count > 1 &&
                pendingGroupMembers[0].GroupRoleId.HasValue &&
                !IsGroupRoleCapacityAvailable( pendingGroupMembers[0].GroupRoleId.Value, groupId, pendingGroupMembers.Count, out errorMessage )
            )
            {
                return ActionBadRequest( errorMessage );
            }

            foreach ( var bag in pendingGroupMembers )
            {
                if ( !bag.Person.PersonId.HasValue && !bag.GroupRoleId.HasValue )
                {
                    continue;
                }

                var groupMember = new GroupMember
                {
                    GroupId = groupId,
                    GroupRoleId = bag.GroupRoleId.Value,
                    PersonId = bag.Person.PersonId.Value,
                    GroupMemberStatus = GroupMemberStatus.Active,
                };

                // TODO - may need to populate attributes here

                if ( !groupMember.IsValid )
                {
                    errorMessage = groupMember.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                    return ActionBadRequest( errorMessage );
                }


                groupMemberService.Add( groupMember );

                memberPairs.Add( (bag, groupMember) );
            }

            RockContext.SaveChanges();

            foreach ( var (bag, entity) in memberPairs )
            {
                bag.GroupMemberId = entity.Id;
            }

            return ActionOk( pendingGroupMembers );
        }

        [BlockAction]
        public BlockActionResult RemoveGroupMemberFromGroup( int groupMemberId, int groupId )
        {
            var groupMemberService = new GroupMemberService( RockContext );
            var groupMember = groupMemberService.Get( groupMemberId );

            if ( groupMember == null || groupMember.GroupId != groupId )
            {
                return ActionBadRequest( "The specified Group Member was not found." );
            }

            groupMemberService.Delete( groupMember );
            RockContext.SaveChanges();
            return ActionOk();
        }

        [BlockAction]
        public BlockActionResult AddPlacementGroup( AddGroupBag addGroupBag, int groupTypeId, int? registrationInstanceId, int? registrationTemplatePlacementId )
        {
            List<Rock.Model.Group> placementGroups;
            var groupService = new GroupService( RockContext );

            if ( addGroupBag.SelectedGroupOption == "Add New Group" )
            {
                var newPlacementGroup = new Rock.Model.Group
                {
                    Name = addGroupBag.GroupName,
                    GroupCapacity = addGroupBag.GroupCapacity,
                    Description = addGroupBag.GroupDescription,
                    GroupTypeId = groupTypeId,
                };

                if ( addGroupBag.ParentGroupForNewGroup != null && addGroupBag.ParentGroupForNewGroup.Value.IsNotNullOrWhiteSpace() )
                {
                    var parentGroup = groupService.Get( addGroupBag.ParentGroupForNewGroup.Value.AsGuid() );
                    if ( parentGroup != null )
                    {
                        if ( !IsValidParentGroup( parentGroup, groupTypeId ) )
                        {
                            var groupType = GroupTypeCache.Get( groupTypeId );
                            var errorMessage = string.Format( "The selected parent group doesn't allow adding {0} child groups", groupType );
                            return ActionBadRequest( errorMessage );
                        }

                        newPlacementGroup.ParentGroupId = parentGroup.Id;
                        newPlacementGroup.ParentGroup = parentGroup;
                    }
                }

                if ( addGroupBag.GroupCampus != null )
                {
                    var campus = CampusCache.Get( addGroupBag.GroupCampus.Value );
                    if ( campus != null )
                    {
                        newPlacementGroup.CampusId = campus.Id;
                    }
                }

                groupService.Add( newPlacementGroup );

                if ( !newPlacementGroup.IsAuthorized( Rock.Security.Authorization.EDIT, GetCurrentPerson() ) )
                {
                    return ActionBadRequest( "You are not authorized to save group with the selected group type and/or parent group." );
                }

                RockContext.SaveChanges();

                newPlacementGroup.LoadAttributes( RockContext );
                newPlacementGroup.SetPublicAttributeValues( addGroupBag.NewGroupAttributeValues, GetCurrentPerson(), enforceSecurity: true );
                newPlacementGroup.SaveAttributeValues( RockContext );

                newPlacementGroup.SaveAttributeValues();
                placementGroups = new List<Rock.Model.Group>
                {
                    newPlacementGroup
                };
            }
            else if ( addGroupBag.SelectedGroupOption == "Add Existing Group(s)" )
            {
                placementGroups = new List<Rock.Model.Group>();
                var existingGroups = addGroupBag.ExistingGroupsToAdd;

                if ( existingGroups == null )
                {
                    return ActionBadRequest( "Please select an existing group." );
                }

                foreach ( var group in existingGroups )
                {
                    if ( group.Value.IsNullOrWhiteSpace() )
                    {
                        continue;
                    }

                    var existingPlacementGroup = groupService.Get( group.Value.AsGuid() );
                    if ( existingPlacementGroup == null )
                    {
                        continue;
                    }

                    if ( groupTypeId != existingPlacementGroup.GroupTypeId )
                    {
                        var groupType = GroupTypeCache.Get( groupTypeId );
                        var errorMessage = string.Format( "The selected groups must be {0} groups", groupType );
                        return ActionBadRequest( errorMessage );
                    }

                    placementGroups.Add( existingPlacementGroup );
                }
            }
            else
            {
                var parentGroupGuid = addGroupBag.ParentGroupForChildren?.Value?.AsGuidOrNull();
                var parentGroup = parentGroupGuid.HasValue ? groupService.Get( parentGroupGuid.Value ) : null;

                if ( parentGroup == null )
                {
                    return ActionBadRequest( "Please select a parent group to add the groups." );
                }

                string errorMessage;
                if ( !HasValidChildGroups( parentGroup.Id, groupTypeId, out errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                var existingPlacementGroups = groupService.Queryable().Where( a => a.ParentGroupId == parentGroup.Id && a.IsActive == true ).ToList();
                placementGroups = existingPlacementGroups;
            }

            // TODO - update with groups placement logic
            var registrationInstanceService = new RegistrationInstanceService( RockContext );
            var registrationTemplatePlacementService = new RegistrationTemplatePlacementService( RockContext );

            foreach ( var placementGroup in placementGroups )
            {
                if ( registrationInstanceId.HasValue )
                {
                    var registrationInstance = registrationInstanceService.Get( registrationInstanceId.Value );
                    registrationInstanceService.GetRegistrationInstancePlacementGroupsByPlacement( registrationInstanceId.Value, registrationTemplatePlacementId.Value );

                    // in RegistrationInstanceMode
                    registrationInstanceService.AddRegistrationInstancePlacementGroup( registrationInstance, placementGroup, registrationTemplatePlacementId.Value );
                }
                else if ( registrationTemplatePlacementId.HasValue )
                {
                    var registrationTemplatePlacement = registrationTemplatePlacementService.Get( registrationTemplatePlacementId.Value );
                    registrationTemplatePlacementService.AddRegistrationTemplatePlacementPlacementGroup( registrationTemplatePlacement, placementGroup );
                }
            }

            RockContext.SaveChanges();

            // TODO - return the added groups
            return ActionOk();
        }

        [BlockAction]
        public BlockActionResult DetachPlacementGroup( DetachGroupBag detachGroupBag )
        {
            var group = new GroupService( RockContext ).Get( detachGroupBag.GroupId );

            if ( group == null )
            {
                return ActionNotFound( "Specified group not found." );
            }

            if (!detachGroupBag.RegistrationTemplatePlacementId.HasValue)
            {
                return ActionNotFound( "Specified registration template placement not found." );
            }

            if ( detachGroupBag.RegistrationInstanceId.HasValue )
            {
                var registrationInstanceService = new RegistrationInstanceService( RockContext );
                var registrationInstance = registrationInstanceService.Get( detachGroupBag.RegistrationInstanceId.Value );

                if ( registrationInstance == null )
                {
                    return ActionNotFound( "Specified registration instance not found." );
                }

                registrationInstanceService.DeleteRegistrationInstancePlacementGroup( registrationInstance, group, detachGroupBag.RegistrationTemplatePlacementId.Value );
            }
            else
            {
                var registrationTemplatePlacementService = new RegistrationTemplatePlacementService( RockContext );
                var registrationTemplatePlacement = registrationTemplatePlacementService.Get( detachGroupBag.RegistrationTemplatePlacementId.Value );

                if ( registrationTemplatePlacement == null )
                {
                    return ActionNotFound( "Specified registration template placement not found." );
                }

                registrationTemplatePlacementService.DeleteRegistrationTemplatePlacementPlacementGroup( registrationTemplatePlacement, group );
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        [BlockAction]
        public BlockActionResult DeletePlacementGroup( int groupId )
        {
            var groupService = new GroupService( RockContext );
            var group = groupService.Get( groupId );

            if ( group == null )
            {
                return ActionNotFound( "Specified group not found." );
            }

            if ( !group.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) )
            {
                return ActionBadRequest( "You are not authorized to delete this group." );
            }

            if ( !groupService.CanDelete( group, out string errorMessage, true ) )
            {
                return ActionBadRequest( errorMessage );
            }

            groupService.Delete( group );

            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion Block Actions
    }
}

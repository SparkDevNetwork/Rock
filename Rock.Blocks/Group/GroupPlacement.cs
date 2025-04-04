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
using System.Linq.Expressions;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Utility;
using Rock.ViewModels.Blocks.Group.GroupPlacement;
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

        private class InstancePlacementGroupPersonId
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

            if ( registrationInstanceId.HasValue )
            {
                //hfRegistrationInstanceId.Value = registrationInstanceId.ToString(); TODO - come back to hidden field functionality
                var registrationInstance = registrationInstanceService.Get( registrationInstanceId.Value );
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

            var groupType = GroupTypeCache.Get( registrationTemplatePlacementGroupTypeId.Value );

            box.SelectedPlacement.IconCSSClass = registrationTemplatePlacement?.IconCssClass;
            box.SelectedPlacement.Name = registrationTemplatePlacement?.Name;
            box.PlacementGroupType.IconCSSClass = groupType?.IconCssClass;
            box.PlacementGroupType.Name = groupType?.GroupTerm?.Pluralize();

            if ( box.PlacementGroupType.Id.HasValue && registrationTemplatePlacementId.HasValue )
            {
                box.PlacementGroups = GetPlacementGroups( registrationTemplatePlacementId.Value, registrationInstanceId, registrationTemplateId, box.PlacementGroupType.Id.Value );
                if ( registrationTemplateId.HasValue )
                {
                    box.PlacementPeople = GetPlacementPeople( registrationTemplateId.Value, registrationInstanceId, registrantId, registrationTemplatePlacementId.Value );
                }
            }

            return box;
        }

        public List<PersonBag> GetPlacementPeople( int registrationTemplateId, int? registrationInstanceId, int? registrantId, int registrationTemplatePlacementId )
        {
            var registrationRegistrantService = new RegistrationRegistrantService( RockContext );
            var registrationRegistrantQuery = registrationRegistrantService.Queryable();

            registrationRegistrantQuery = registrationRegistrantQuery
                .Where( a => a.Registration.RegistrationInstance.RegistrationTemplateId == registrationTemplateId );

            if ( registrationInstanceId.HasValue )
            {
                registrationRegistrantQuery = registrationRegistrantQuery.Where( a => a.Registration.RegistrationInstanceId == registrationInstanceId.Value );
            }

            if ( registrantId.HasValue )
            {
                registrationRegistrantQuery = registrationRegistrantQuery.Where( a => a.Id == registrantId.Value );
            }

            var registrationTemplatePlacement = new RegistrationTemplatePlacementService( RockContext ).Get( registrationTemplatePlacementId );

            registrationRegistrantQuery = registrationRegistrantQuery.Where( a => a.OnWaitList == false );

            registrationRegistrantQuery = registrationRegistrantQuery.OrderBy( a => a.PersonAlias.Person.LastName ).ThenBy( a => a.PersonAlias.Person.NickName );

            var registrationTemplatePlacementService = new RegistrationTemplatePlacementService( RockContext );
            var registrationInstanceService = new RegistrationInstanceService( RockContext );

            var registrationTemplatePlacementGroupsPersonIdQuery = registrationTemplatePlacementService.GetRegistrationTemplatePlacementPlacementGroups( registrationTemplatePlacement ).SelectMany( a => a.Members ).Select( a => a.PersonId );

            IQueryable<InstancePlacementGroupPersonId> allInstancesPlacementGroupInfoQuery = null;
            int[] registrationTemplateInstanceIds = Array.Empty<int>();

            if ( !registrationInstanceId.HasValue )
            {
                registrationTemplateInstanceIds = new RegistrationTemplateService( RockContext ).GetSelect( registrationTemplateId, s => s.Instances.Select( i => i.Id ) ).ToArray();
            }

            if ( registrationInstanceId.HasValue )
            {
                allInstancesPlacementGroupInfoQuery =
                    registrationInstanceService.GetRegistrationInstancePlacementGroupsByPlacement(
                        registrationInstanceService.Get( registrationInstanceId.Value ),
                        registrationTemplatePlacementId )
                        .Where( a => a.GroupTypeId == registrationTemplatePlacement.GroupTypeId )
                        .SelectMany( a => a.Members ).Select( a => a.PersonId )
                        .Select( s => new InstancePlacementGroupPersonId
                        {
                            PersonId = s,
                            RegistrationInstanceId = registrationInstanceId.Value
                        } );
            }
            else if ( registrationTemplateInstanceIds?.Any() == true )
            {
                foreach ( var registrationInstanceIdTemp in registrationTemplateInstanceIds )
                {
                    var instancePlacementGroupInfoQuery = registrationInstanceService.GetRegistrationInstancePlacementGroupsByPlacement(
                        registrationInstanceService.Get( registrationInstanceIdTemp ),
                        registrationTemplatePlacementId )
                    .Where( a => a.GroupTypeId == registrationTemplatePlacement.GroupTypeId )
                    .SelectMany( a => a.Members ).Select( a => a.PersonId )
                    .Select( s => new InstancePlacementGroupPersonId
                    {
                        PersonId = s,
                        RegistrationInstanceId = registrationInstanceIdTemp
                    } );

                    if ( allInstancesPlacementGroupInfoQuery == null )
                    {
                        allInstancesPlacementGroupInfoQuery = instancePlacementGroupInfoQuery;
                    }
                    else
                    {
                        allInstancesPlacementGroupInfoQuery = allInstancesPlacementGroupInfoQuery.Union( instancePlacementGroupInfoQuery );
                    }
                }
            }

            if ( allInstancesPlacementGroupInfoQuery == null )
            {
                throw new ArgumentNullException( "Registration Instance(s) must be specified" );
            }

            var registrationRegistrantPlacementQuery = registrationRegistrantQuery.Select( r => new
            {
                Registrant = r,
                r.PersonAlias.Person,
                r.Registration.RegistrationInstance,
                AlreadyPlacedInGroup =
                    registrationTemplatePlacementGroupsPersonIdQuery.Contains( r.PersonAlias.PersonId )
                    || allInstancesPlacementGroupInfoQuery.Any( x => x.RegistrationInstanceId == r.Registration.RegistrationInstanceId && x.PersonId == r.PersonAlias.PersonId )
            } );

            var registrationRegistrantPlacementList = registrationRegistrantPlacementQuery.AsNoTracking().ToList();

            var personBagList = registrationRegistrantPlacementList
                .Select( x => new PersonBag
                {
                    PersonId = x.Person.Id,
                    RegistrationInstanceId = x.RegistrationInstance.Id,
                    AlreadyPlacedInGroup = x.AlreadyPlacedInGroup,
                    FullName = x.Person.FullName,
                    Gender = x.Person.Gender,
                    RegistrantId = x.Registrant.Id,
                    RegistrationInstancename = x.RegistrationInstance.Name,
                    //Fees = new Dictionary<string, string>(),
                } )
                .ToList();

            return personBagList;
        }

        private List<PlacementGroupBag> GetPlacementGroups( int registrationTemplatePlacementId, int? registrationInstanceId, int? registrationTemplateId, int placementGroupTypeId )
        {
            var groupType = GroupTypeCache.Get( placementGroupTypeId );

            //_groupTypeRoles = groupType.Roles.ToList();
            var registrationInstanceService = new RegistrationInstanceService( RockContext );
            var registrationTemplatePlacementService = new RegistrationTemplatePlacementService( RockContext );
            var groupService = new GroupService( RockContext );
            RegistrationTemplatePlacement registrationTemplatePlacement = registrationTemplatePlacementService.Get( registrationTemplatePlacementId );

            List<RegistrationInstance> registrationInstanceList = null;

            if ( registrationInstanceId.HasValue )
            {
                var registrationInstance = registrationInstanceService.Get( registrationInstanceId.Value );
                registrationTemplateId = registrationInstance.RegistrationTemplateId;

                registrationInstanceList = new List<Rock.Model.RegistrationInstance>();
                registrationInstanceList.Add( registrationInstance );
            }
            else if ( registrationTemplateId.HasValue )
            {
                registrationInstanceList = registrationInstanceService
                    .Queryable()
                    .Where( a => a.RegistrationTemplateId == registrationTemplateId.Value )
                    .Select( a => new { Id = a.Id, Name = a.Name } )
                    .AsEnumerable()
                    .Select( a => new RegistrationInstance { Id = a.Id, Name = a.Name } )
                    .OrderBy( a => a.Name )
                    .ToList();
            }

            //var displayedCampusId = GetPlacementConfiguration().DisplayedCampusId;

            var registrationInstancePlacementGroupList = new List<PlacementGroupBag>();
            Dictionary<int, List<int>> placementGroupIdRegistrationInstanceIds = new Dictionary<int, List<int>>();
            if ( registrationInstanceList != null && registrationTemplatePlacement != null )
            {
                foreach ( var registrationInstance in registrationInstanceList )
                {
                    var placementGroupsQry = registrationInstanceService
                        .GetRegistrationInstancePlacementGroupsByPlacement( registrationInstance.Id, registrationTemplatePlacementId )
                        .Where( a => a.GroupTypeId == groupType.Id );

                    //if ( displayedCampusId.HasValue )
                    //{
                    //    placementGroupsQry = placementGroupsQry.Where( a => !a.CampusId.HasValue || a.CampusId == displayedCampusId.Value );
                    //}

                    //placementGroupsQry = ApplyGroupFilter( groupType, groupService, placementGroupsQry );

                    // Only fetch what we actually need for each registration *instance* placement group.
                    var placementGroups = placementGroupsQry
                        .Select( g => new { g.Id, g.Name, g.CampusId, g.GroupCapacity, g.GroupTypeId } )
                        .ToList()
                        .Select( g => new PlacementGroupBag { Id = g.Id, Name = g.Name, CampusId = g.CampusId, GroupCapacity = g.GroupCapacity, GroupTypeId = g.GroupTypeId } );

                    foreach ( var placementGroup in placementGroups )
                    {
                        registrationInstancePlacementGroupList.Add( placementGroup, true );

                        var instanceIds = placementGroupIdRegistrationInstanceIds.GetValueOrDefault( placementGroup.Id, new List<int>() );
                        instanceIds.Add( registrationInstance.Id );
                        placementGroupIdRegistrationInstanceIds[placementGroup.Id] = instanceIds;
                    }
                }
            }

            var registrationTemplatePlacementGroupQuery = registrationTemplatePlacementService.GetRegistrationTemplatePlacementPlacementGroups( registrationTemplatePlacement );

            //if ( displayedCampusId.HasValue )
            //{
            //    registrationTemplatePlacementGroupQuery = registrationTemplatePlacementGroupQuery.Where( a => !a.CampusId.HasValue || a.CampusId == displayedCampusId );
            //}

            //registrationTemplatePlacementGroupQuery = ApplyGroupFilter( groupType, groupService, registrationTemplatePlacementGroupQuery );

            // Only fetch what we actually need for each registration *template* placement group.
            var registrationTemplatePlacementGroupList = registrationTemplatePlacementGroupQuery
                .Select( g => new { g.Id, g.Name, g.CampusId, g.GroupCapacity, g.GroupTypeId } )
                .ToList()
                .Select( g => new PlacementGroupBag { Id = g.Id, Name = g.Name, CampusId = g.CampusId, GroupCapacity = g.GroupCapacity, GroupTypeId = g.GroupTypeId } );

            var registrationTemplatePlacementGroupIds = registrationTemplatePlacementGroupList.Select( a => a.Id ).ToList();
            var placementGroupList = new List<PlacementGroupBag>();
            placementGroupList.AddRange( registrationTemplatePlacementGroupList );
            placementGroupList.AddRange( registrationInstancePlacementGroupList );

            // A placement group could be associated with both the instance and the template. So, we'll make sure the same group isn't listed more than once
            placementGroupList = placementGroupList.DistinctBy( a => a.Id ).ToList();
            placementGroupList = placementGroupList.OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            //var groupIds = placementGroupList.Select( a => a.Id ).ToList();
            //var groupMemberService = new GroupMemberService( RockContext );
            //var placementGroupIdGroupRoleIdCount = groupMemberService
            //   .Queryable()
            //   .Where( a => a.GroupMemberStatus != GroupMemberStatus.Inactive && groupIds.Contains( a.GroupId ) )
            //   .Select( gm => new { GroupId = gm.GroupId, GroupRoleId = gm.GroupRoleId } )
            //   .ToList()
            //   .GroupBy( gm => gm.GroupId )
            //   .ToDictionary( a => a.Key, b => b.GroupBy( a => a.GroupRoleId ).ToDictionary( a => a.Key, a => a.Count() ) );

            return placementGroupList;
        }

        /// <summary>
        /// Shows the details.
        /// </summary>
        protected void ShowDetails()
        {
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

            if ( registrationInstanceId.HasValue )
            {
                //hfRegistrationInstanceId.Value = registrationInstanceId.ToString(); TODO - come back to hidden field functionality
                var registrationInstance = registrationInstanceService.Get( registrationInstanceId.Value );
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
                var errorMessage = "Invalid Registration Template";
                //nbConfigurationError.Text = "Invalid Registration Template";
                //nbConfigurationError.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
                //nbConfigurationError.Visible = true;
                //pnlView.Visible = false;
                return;
            }

            //hfRegistrationTemplateId.Value = registrationTemplateId.ToString(); TODO - come back to hidden field functionality
            //hfBlockId.Value = this.BlockId.ToString(); TODO - come back to hidden field functionality

            //var placementConfiguration = GetPlacementConfiguration(); TODO - seems like this is handled in person preferences.

            //hfOptionsRegistrantPersonDataViewFilterId.Value = placementConfiguration.RegistrantPersonDataViewFilterId.ToString();

            //hfOptionsDisplayedRegistrantAttributeIds.Value = placementConfiguration.DisplayedRegistrantAttributeIds.ToJson();
            //hfOptionsDisplayedGroupMemberAttributeIds.Value = placementConfiguration.DisplayedGroupMemberAttributeIds.ToJson();
            //hfOptionsIncludeFees.Value = placementConfiguration.ShowFees.ToJavaScriptValue();
            //hfOptionsHighlightGenders.Value = placementConfiguration.HighlightGenders.ToJavaScriptValue();
            //hfOptionsDisplayRegistrantAttributes.Value = placementConfiguration.DisplayRegistrantAttributes.ToJavaScriptValue();
            //hfOptionsApplyRegistrantFilters.Value = placementConfiguration.ApplyRegistrantFilters.ToJavaScriptValue();
            //hfOptionsHideFullGroups.Value = placementConfiguration.HideFullGroups.ToJavaScriptValue();
            //hfRegistrationTemplateInstanceIds.Value = placementConfiguration.IncludedRegistrationInstanceIds.ToJson();
            //hfRegistrationTemplateShowInstanceName.Value = placementConfiguration.ShowRegistrationInstanceName.ToJavaScriptValue();

            //hfOptionsFilterFeeId.Value = placementConfiguration.FilterFeeId.ToString();
            //hfOptionsFilterFeeItemIds.Value = placementConfiguration.FilterFeeOptionIds.ToJson();

            //hfRegistrationTemplatePlacementId.Value = registrationTemplatePlacementId.ToString(); TODO - come back to hidden field functionality 

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
                    //PlacementGroupTypeId = registrationTemplatePlacement.GroupTypeId;
                    //hfRegistrationTemplatePlacementGroupTypeId.Value = registrationTemplatePlacementGroupTypeId.ToString(); TODO - come back to hidden field functionality
                    //hfRegistrationTemplatePlacementAllowMultiplePlacements.Value = registrationTemplatePlacement.AllowMultiplePlacements.ToJavaScriptValue(); TODO - come back to hidden field functionality
                }
            }

            //BindDropDowns();

            // Binds the selectable placements ( Buses, Cabins, etc. )
            var registrationTemplatePlacements = registrationTemplateService.GetSelect( registrationTemplateId.Value, s => s.Placements ).OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            // BindRegistrationTemplatePlacementPrompt( registrationTemplatePlacements );

            // The "prompt" is the nav options for different selectable placements ( Buses, Cabins, etc. )
            // if a registrationTemplatePlacement wasn't specified (or wasn't specified initially), show the prompt
            //rcwSelectRegistrationTemplatePlacement.Visible = ( registrationTemplatePlacement == null ) || this.PageParameter( PageParameterKey.PromptForTemplatePlacement ).AsBoolean();

            if ( registrationTemplatePlacement == null )
            {
                if ( !registrationTemplatePlacements.Any() )
                {
                    var errorMessage = "No Placement Types available for this registration template.";
                    //rptSelectRegistrationTemplatePlacement.Visible = false;
                    //nbConfigurationError.Text = "No Placement Types available for this registration template.";
                    //nbConfigurationError.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
                    //nbConfigurationError.Visible = true;
                    //pnlView.Visible = false;
                    return;
                }

                var defaultRegistrationTemplatePlacementId = registrationTemplatePlacements.First().Id;

                //ReloadPageWithRegistrationTemplatePlacementId( defaultRegistrationTemplatePlacementId );
                return;
            }

            if ( registrationTemplatePlacement == null )
            {
                if ( registrationInstanceId.HasValue )
                {
                    var errorMessage = "Invalid Registration Template Placement specified.";
                    //nbConfigurationError.Text = "Invalid Registration Template Placement Specified";
                    //nbConfigurationError.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
                    //nbConfigurationError.Visible = true;
                    //pnlView.Visible = false;
                }
                else
                {
                    // Prompt for registrationTemplatePlacement
                    //rcwSelectRegistrationTemplatePlacement.Visible = true;
                }

                return;
            }

            var groupType = GroupTypeCache.Get( registrationTemplatePlacementGroupTypeId.Value );

            //_placementIconCssClass = registrationTemplatePlacement.GetIconCssClass();

            //lGroupPlacementGroupTypeIconHtml.Text = string.Format( "<i class='{0}'></i>", _placementIconCssClass );
            //lAddPlacementGroupButtonIconHtml.Text = string.Format( "<i class='{0}'></i>", _placementIconCssClass );

            //lGroupPlacementGroupTypeName.Text = groupType.GroupTerm.Pluralize();
            //lAddPlacementGroupButtonText.Text = string.Format( " Add {0}", groupType.GroupTerm );

            //BindPlacementGroupsRepeater();

            //hfGroupDetailUrl.Value = this.LinkedPageUrl( AttributeKey.GroupDetailPage );
            //hfGroupMemberDetailUrl.Value = this.LinkedPageUrl( AttributeKey.GroupMemberDetailPage );
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



        #endregion Block Actions
    }
}

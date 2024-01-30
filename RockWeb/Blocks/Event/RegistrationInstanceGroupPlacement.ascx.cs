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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Event
{
    [DisplayName( "Registration Group Placement" )]
    [Category( "Event" )]
    [Description( "Block to manage group placement for Registration Instances." )]

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

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.EVENT_REGISTRATION_GROUP_PLACEMENT )]
    public partial class RegistrationInstanceGroupPlacement : RegistrationInstanceBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string RegistrationTemplate = "RegistrationTemplate";
            public const string GroupDetailPage = "GroupDetailPage";
            public const string GroupMemberDetailPage = "GroupMemberDetailPage";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string RegistrantId = "RegistrantId";
            public const string RegistrationInstanceId = "RegistrationInstanceId";
            public const string RegistrationTemplateId = "RegistrationTemplateId";
            public const string RegistrationTemplatePlacementId = "RegistrationTemplatePlacementId";
            public const string PromptForTemplatePlacement = "PromptForTemplatePlacement";
        }

        #endregion PageParameterKeys

        #region UserPreferenceKeys

        private static class UserPreferenceKey
        {
            public const string PlacementConfigurationJSON_RegistrationInstanceId = "PlacementConfigurationJSON_RegistrationInstanceId_{0}";
            public const string PlacementConfigurationJSON_RegistrationTemplateId = "PlacementConfigurationJSON_RegistrationTemplateId_{0}";
            public const string RegistrantAttributeFilter_RegistrationInstanceId = "RegistrantAttributeFilter_RegistrationInstanceId_{0}";
            public const string RegistrantAttributeFilter_RegistrationTemplateId = "RegistrantAttributeFilter_RegistrationTemplateId_{0}";
            public const string GroupAttributeFilter_GroupTypeId = "GroupAttributeFilter_GroupTypeId_{0}";
            public const string GroupMemberAttributeFilter_GroupTypeId = "GroupMemberAttributeFilter_GroupTypeId_{0}";
        }

        #endregion UserPreferenceKeys

        #region Properties

        /// <summary>
        /// The placemnent group type identifier.
        /// </summary>
        public int PlacementGroupTypeId
        {
            get
            {
                return ( ViewState[ViewStateKey.PlacementGroupTypeId] as string ).AsInteger();
            }

            set
            {
                ViewState[ViewStateKey.PlacementGroupTypeId] = value.ToString();
            }
        }

        #endregion Properties

        #region ViewStateKeys

        private static class ViewStateKey
        {
            public const string DataFilterJSON = "DataFilterJSON";
            public const string PlacementGroupTypeId = "PlacementGroupTypeId";
        }

        #endregion ViewStateKeys

        #region Classes

        /// <summary>
        ///
        /// </summary>
        private class PlacementConfiguration
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PlacementConfiguration"/> class.
            /// </summary>
            public PlacementConfiguration()
            {
                ShowRegistrationInstanceName = true;
                ExpandRegistrantDetails = true;
                IncludedRegistrationInstanceIds = new int[0];
                DisplayedRegistrantAttributeIds = new int[0];
                DisplayedGroupAttributeIds = new int[0];
                DisplayedGroupMemberAttributeIds = new int[0];
                FilterFeeOptionIds = new int[0];
            }

            /// <summary>
            /// Whether the all Registrant Details should be expanded (from the
            /// </summary>
            /// <value>
            ///   <c>true</c> if [expand registrant details]; otherwise, <c>false</c>.
            /// </value>
            public bool ExpandRegistrantDetails { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [show registration instance name].
            /// Note this setting only applies when in Template Mode
            /// </summary>
            /// <value>
            ///   <c>true</c> if [show registration instance name]; otherwise, <c>false</c>.
            /// </value>
            public bool ShowRegistrationInstanceName { get; set; }

            /// <summary>
            /// Gets or sets the included registration instance ids.
            /// Note this setting only applies when in Template Mode
            /// </summary>
            /// <value>
            /// The included registration instance ids.
            /// </value>
            public int[] IncludedRegistrationInstanceIds { get; set; }

            /// <summary>
            /// Gets or sets the displayed campus identifier.
            /// </summary>
            /// <value>
            /// The displayed campus identifier.
            /// </value>
            public int? DisplayedCampusId { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [highlight genders].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [highlight genders]; otherwise, <c>false</c>.
            /// </value>
            public bool HighlightGenders { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [show fees].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [show fees]; otherwise, <c>false</c>.
            /// </value>
            public bool ShowFees { get; set; }

            /// <summary>
            /// Gets or sets the displayed registrant attribute ids.
            /// </summary>
            /// <value>
            /// The displayed registrant attribute ids.
            /// </value>
            public int[] DisplayedRegistrantAttributeIds { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [display registrant attributes].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [display registrant attributes]; otherwise, <c>false</c>.
            /// </value>
            public bool DisplayRegistrantAttributes { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [apply registrant filters].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [apply registrant filters]; otherwise, <c>false</c>.
            /// </value>
            public bool ApplyRegistrantFilters { get; set; }

            /// <summary>
            /// Gets or sets the registrant data view filter (Person DataViewFilter)
            /// </summary>
            /// <value>
            /// The registrant data view filter identifier.
            /// </value>
            public int? RegistrantPersonDataViewFilterId { get; set; }

            /// <summary>
            /// Gets or sets the displayed group attribute ids.
            /// </summary>
            /// <value>
            /// The displayed group attribute ids.
            /// </value>
            public int[] DisplayedGroupAttributeIds { get; set; }

            /// <summary>
            /// Gets or sets the displayed group member attribute ids.
            /// </summary>
            /// <value>
            /// The displayed group member attribute ids.
            /// </value>
            public int[] DisplayedGroupMemberAttributeIds { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [hide full groups].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [hide full groups]; otherwise, <c>false</c>.
            /// </value>
            public bool HideFullGroups { get; set; }

            /// <summary>
            /// Gets or sets the name of the filter fee.
            /// </summary>
            /// <value>
            /// The name of the filter fee.
            /// </value>
            public int? FilterFeeId { get; set; }

            /// <summary>
            /// Gets or sets the filter fee options.
            /// </summary>
            /// <value>
            /// The filter fee options.
            /// </value>
            public int[] FilterFeeOptionIds { get; set; }
        }

        #endregion Classes

        #region Helper classes

        private enum AddPlacementGroupTab
        {
            AddNewGroup,
            AddExistingGroup,
            AddMultipleGroups
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/dragula.min.js", true );
            RockPage.AddScriptLink( "~/Scripts/Rock/Controls/GroupPlacementTool/groupPlacementTool.js" );
            RockPage.AddCSSLink( "~/Themes/Rock/Styles/group-placement.css", true );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetails();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the details.
        /// </summary>
        protected void ShowDetails()
        {
            int? registrationTemplatePlacementId = this.PageParameter( PageParameterKey.RegistrationTemplatePlacementId ).AsIntegerOrNull();

            var rockContext = new RockContext();

            var registrationTemplateService = new RegistrationTemplateService( rockContext );
            var registrationInstanceService = new RegistrationInstanceService( rockContext );

            int? registrationInstanceId = this.PageParameter( PageParameterKey.RegistrationInstanceId ).AsIntegerOrNull();

            int? registrationTemplateId;
            Guid? registrationTemplateGuid = GetAttributeValue( AttributeKey.RegistrationTemplate ).AsGuidOrNull();
            if ( registrationTemplateGuid.HasValue )
            {
                registrationTemplateId = registrationTemplateService.GetId( registrationTemplateGuid.Value );
            }
            else
            {
                registrationTemplateId = this.PageParameter( PageParameterKey.RegistrationTemplateId ).AsIntegerOrNull();
            }

            // in case a specific registrant is specified
            int? registrantId = this.PageParameter( PageParameterKey.RegistrantId ).AsIntegerOrNull();

            if ( registrantId.HasValue )
            {
                hfRegistrantId.Value = registrantId.ToString();
                registrationInstanceId = new RegistrationRegistrantService( rockContext ).GetSelect( registrantId.Value, s => s.Registration.RegistrationInstanceId );
            }

            if ( registrationInstanceId.HasValue )
            {
                hfRegistrationInstanceId.Value = registrationInstanceId.ToString();
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
                nbConfigurationError.Text = "Invalid Registration Template";
                nbConfigurationError.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
                nbConfigurationError.Visible = true;
                pnlView.Visible = false;
                return;
            }

            hfRegistrationTemplateId.Value = registrationTemplateId.ToString();
            hfBlockId.Value = this.BlockId.ToString();

            var placementConfiguration = GetPlacementConfiguration();

            hfOptionsRegistrantPersonDataViewFilterId.Value = placementConfiguration.RegistrantPersonDataViewFilterId.ToString();

            hfOptionsDisplayedRegistrantAttributeIds.Value = placementConfiguration.DisplayedRegistrantAttributeIds.ToJson();
            hfOptionsDisplayedGroupMemberAttributeIds.Value = placementConfiguration.DisplayedGroupMemberAttributeIds.ToJson();
            hfOptionsIncludeFees.Value = placementConfiguration.ShowFees.ToJavaScriptValue();
            hfOptionsHighlightGenders.Value = placementConfiguration.HighlightGenders.ToJavaScriptValue();
            hfOptionsDisplayRegistrantAttributes.Value = placementConfiguration.DisplayRegistrantAttributes.ToJavaScriptValue();
            hfOptionsApplyRegistrantFilters.Value = placementConfiguration.ApplyRegistrantFilters.ToJavaScriptValue();
            hfOptionsHideFullGroups.Value = placementConfiguration.HideFullGroups.ToJavaScriptValue();
            hfRegistrationTemplateInstanceIds.Value = placementConfiguration.IncludedRegistrationInstanceIds.ToJson();
            hfRegistrationTemplateShowInstanceName.Value = placementConfiguration.ShowRegistrationInstanceName.ToJavaScriptValue();

            hfOptionsFilterFeeId.Value = placementConfiguration.FilterFeeId.ToString();
            hfOptionsFilterFeeItemIds.Value = placementConfiguration.FilterFeeOptionIds.ToJson();

            hfRegistrationTemplatePlacementId.Value = registrationTemplatePlacementId.ToString();

            RegistrationTemplatePlacement registrationTemplatePlacement = null;
            int? registrationTemplatePlacementGroupTypeId = null;
            if ( registrationTemplatePlacementId.HasValue )
            {
                registrationTemplatePlacement = new RegistrationTemplatePlacementService( rockContext ).Get( registrationTemplatePlacementId.Value );

                if ( registrationTemplatePlacement == null || registrationTemplatePlacement.RegistrationTemplateId != registrationTemplateId )
                {
                    // if the registration template placement is for a different registration template, don't use it
                    registrationTemplatePlacement = null;
                    registrationTemplatePlacementId = null;
                }
                else
                {
                    registrationTemplatePlacementGroupTypeId = registrationTemplatePlacement.GroupTypeId;
                    PlacementGroupTypeId = registrationTemplatePlacement.GroupTypeId;
                    hfRegistrationTemplatePlacementGroupTypeId.Value = registrationTemplatePlacementGroupTypeId.ToString();
                    hfRegistrationTemplatePlacementAllowMultiplePlacements.Value = registrationTemplatePlacement.AllowMultiplePlacements.ToJavaScriptValue();
                }
            }

            BindDropDowns();

            var registrationTemplatePlacements = registrationTemplateService.GetSelect( registrationTemplateId.Value, s => s.Placements ).OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            BindRegistrationTemplatePlacementPrompt( registrationTemplatePlacements );

            // if a registrationTemplatePlacement wasn't specified (or wasn't specified initially), show the prompt
            rcwSelectRegistrationTemplatePlacement.Visible = ( registrationTemplatePlacement == null ) || this.PageParameter( PageParameterKey.PromptForTemplatePlacement ).AsBoolean();

            if ( registrationTemplatePlacement == null )
            {
                if ( !registrationTemplatePlacements.Any() )
                {
                    rptSelectRegistrationTemplatePlacement.Visible = false;
                    nbConfigurationError.Text = "No Placement Types available for this registration template.";
                    nbConfigurationError.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
                    nbConfigurationError.Visible = true;
                    pnlView.Visible = false;
                    return;
                }

                var defaultRegistrationTemplatePlacementId = registrationTemplatePlacements.First().Id;

                ReloadPageWithRegistrationTemplatePlacementId( defaultRegistrationTemplatePlacementId );
                return;
            }

            if ( registrationTemplatePlacement == null )
            {
                if ( registrationInstanceId.HasValue )
                {
                    nbConfigurationError.Text = "Invalid Registration Template Placement Specified";
                    nbConfigurationError.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
                    nbConfigurationError.Visible = true;
                    pnlView.Visible = false;
                }
                else
                {
                    // Prompt for registrationTemplatePlacement
                    rcwSelectRegistrationTemplatePlacement.Visible = true;
                }

                return;
            }

            var groupType = GroupTypeCache.Get( registrationTemplatePlacementGroupTypeId.Value );

            _placementIconCssClass = registrationTemplatePlacement.GetIconCssClass();

            lGroupPlacementGroupTypeIconHtml.Text = string.Format( "<i class='{0}'></i>", _placementIconCssClass );
            lAddPlacementGroupButtonIconHtml.Text = string.Format( "<i class='{0}'></i>", _placementIconCssClass );

            lGroupPlacementGroupTypeName.Text = groupType.GroupTerm.Pluralize();
            lAddPlacementGroupButtonText.Text = string.Format( " Add {0}", groupType.GroupTerm );

            BindPlacementGroupsRepeater();

            hfGroupDetailUrl.Value = this.LinkedPageUrl( AttributeKey.GroupDetailPage );
            hfGroupMemberDetailUrl.Value = this.LinkedPageUrl( AttributeKey.GroupMemberDetailPage );
        }

        /// <summary>
        /// Gets the placement configuration.
        /// </summary>
        /// <returns></returns>
        private PlacementConfiguration GetPlacementConfiguration()
        {
            if ( _placementConfiguration == null )
            {
                int? registrationInstanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
                int registrationTemplateId = hfRegistrationTemplateId.Value.AsInteger();
                var preferences = GetBlockPersonPreferences();

                string placementConfigurationJSON;
                if ( registrationInstanceId.HasValue )
                {
                    placementConfigurationJSON = preferences.GetValue( string.Format( UserPreferenceKey.PlacementConfigurationJSON_RegistrationInstanceId, registrationInstanceId.Value ) );
                }
                else
                {
                    placementConfigurationJSON = preferences.GetValue( string.Format( UserPreferenceKey.PlacementConfigurationJSON_RegistrationTemplateId, registrationTemplateId ) );
                }

                _placementConfiguration = placementConfigurationJSON.FromJsonOrNull<PlacementConfiguration>() ?? new PlacementConfiguration();
            }

            return _placementConfiguration;
        }

        private PlacementConfiguration _placementConfiguration = null;

        /// <summary>
        /// Saves the placement configuration.
        /// </summary>
        /// <param name="placementConfiguration">The placement configuration.</param>
        private void SavePlacementConfiguration( PlacementConfiguration placementConfiguration )
        {
            int? registrationInstanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
            int registrationTemplateId = hfRegistrationTemplateId.Value.AsInteger();

            string placementConfigurationJSON = placementConfiguration.ToJson();
            var preferences = GetBlockPersonPreferences();
            if ( registrationInstanceId.HasValue )
            {
                preferences.SetValue( string.Format( UserPreferenceKey.PlacementConfigurationJSON_RegistrationInstanceId, registrationInstanceId.Value ), placementConfigurationJSON );
            }
            else
            {
                preferences.SetValue( string.Format( UserPreferenceKey.PlacementConfigurationJSON_RegistrationTemplateId, registrationTemplateId ), placementConfigurationJSON );
            }

            preferences.Save();

            ShowDetails();
        }

        /// <summary>
        /// Binds the drop downs.
        /// </summary>
        private void BindDropDowns()
        {
            bgAddNewOrExistingPlacementGroup.Items.Clear();
            bgAddNewOrExistingPlacementGroup.BindToEnum<AddPlacementGroupTab>();
        }

        /// <summary>
        /// Binds the prompt for the Registration Template Placement,
        /// Returns false if there are no registration templates for the current RegistrationTemplate,
        /// </summary>
        /// <returns></returns>
        private void BindRegistrationTemplatePlacementPrompt( List<RegistrationTemplatePlacement> registrationTemplatePlacements )
        {
            rptSelectRegistrationTemplatePlacement.DataSource = registrationTemplatePlacements;
            rptSelectRegistrationTemplatePlacement.DataBind();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptSelectRegistrationTemplatePlacement control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptSelectRegistrationTemplatePlacement_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            RegistrationTemplatePlacement registrationTemplatePlacement = e.Item.DataItem as RegistrationTemplatePlacement;
            if ( registrationTemplatePlacement == null )
            {
                return;
            }

            var additionalQueryParameters = new Dictionary<string, string>();
            additionalQueryParameters.Add( PageParameterKey.RegistrationTemplatePlacementId, registrationTemplatePlacement.Id.ToString() );
            additionalQueryParameters.Add( PageParameterKey.PromptForTemplatePlacement, true.ToString() );

            var pageUrl = GetCurrentPageUrl( additionalQueryParameters );
            var currentRegistrationTemplatePlacementId = hfRegistrationTemplatePlacementId.Value.AsIntegerOrNull();
            string activeClass = string.Empty;
            if ( currentRegistrationTemplatePlacementId.HasValue )
            {
                if ( currentRegistrationTemplatePlacementId == registrationTemplatePlacement.Id )
                {
                    activeClass = "active";
                }
            }
            else
            {
                if ( e.Item.ItemIndex == 0 )
                {
                    activeClass = "active";
                }
            }

            var lTabHtml = e.Item.FindControl( "lTabHtml" ) as Literal;
            string lTabHtmlFormat =
@"
<li class='{0}'>
   <a href = '{1}'><i class='{2}'></i> {3}</a>
</li>
";

            lTabHtml.Text = string.Format(
                lTabHtmlFormat,
                activeClass, // {0}
                pageUrl, // {1}
                registrationTemplatePlacement.GetIconCssClass(),  // {2}
                registrationTemplatePlacement.Name );  // {3}
        }

        private Dictionary<int, List<int>> _placementGroupIdRegistrationInstanceIds;
        private List<int> _registrationTemplatePlacementGroupIds;
        private Dictionary<int, Dictionary<int,int>> _placementGroupIdGroupRoleIdCount;

        /// <summary>
        /// Binds the placement groups repeater.
        /// </summary>
        private void BindPlacementGroupsRepeater()
        {
            var registrationTemplatePlacementId = hfRegistrationTemplatePlacementId.Value.AsInteger();
            var registrationInstanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
            var registrationTemplateId = hfRegistrationTemplateId.Value.AsIntegerOrNull();

            var groupType = GroupTypeCache.Get( hfRegistrationTemplatePlacementGroupTypeId.Value.AsInteger() );

            _groupTypeRoles = groupType.Roles.ToList();
            var rockContext = new RockContext();
            var registrationInstanceService = new RegistrationInstanceService( rockContext );
            var registrationTemplatePlacementService = new RegistrationTemplatePlacementService( rockContext );
            var groupService = new GroupService( rockContext );
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

            var displayedCampusId = GetPlacementConfiguration().DisplayedCampusId;

            var registrationInstancePlacementGroupList = new List<Group>();
            _placementGroupIdRegistrationInstanceIds = new Dictionary<int, List<int>>();
            if ( registrationInstanceList != null && registrationTemplatePlacement != null )
            {
                foreach ( var registrationInstance in registrationInstanceList )
                {
                    var placementGroupsQry = registrationInstanceService
                        .GetRegistrationInstancePlacementGroupsByPlacement( registrationInstance.Id, registrationTemplatePlacementId )
                        .Where( a => a.GroupTypeId == groupType.Id );

                    if ( displayedCampusId.HasValue )
                    {
                        placementGroupsQry = placementGroupsQry.Where( a => !a.CampusId.HasValue || a.CampusId == displayedCampusId.Value );
                    }

                    placementGroupsQry = ApplyGroupFilter( groupType, groupService, placementGroupsQry );

                    // Only fetch what we actually need for each registration *instance* placement group.
                    var placementGroups = placementGroupsQry
                        .Select( g => new { g.Id, g.Name, g.CampusId, g.GroupCapacity, g.GroupTypeId } )
                        .ToList()
                        .Select( g => new Group { Id = g.Id, Name = g.Name, CampusId = g.CampusId, GroupCapacity = g.GroupCapacity, GroupTypeId = g.GroupTypeId } );

                    foreach ( var placementGroup in placementGroups )
                    {
                        registrationInstancePlacementGroupList.Add( placementGroup, true );

                        var instanceIds = _placementGroupIdRegistrationInstanceIds.GetValueOrDefault( placementGroup.Id, new List<int>() );
                        instanceIds.Add( registrationInstance.Id );
                        _placementGroupIdRegistrationInstanceIds[placementGroup.Id] = instanceIds;
                    }
                }
            }

            var registrationTemplatePlacementGroupQuery = registrationTemplatePlacementService.GetRegistrationTemplatePlacementPlacementGroups( registrationTemplatePlacement );
            if ( displayedCampusId.HasValue )
            {
                registrationTemplatePlacementGroupQuery = registrationTemplatePlacementGroupQuery.Where( a => !a.CampusId.HasValue || a.CampusId == displayedCampusId );
            }

            registrationTemplatePlacementGroupQuery = ApplyGroupFilter( groupType, groupService, registrationTemplatePlacementGroupQuery );

            // Only fetch what we actually need for each registration *template* placement group.
            var registrationTemplatePlacementGroupList = registrationTemplatePlacementGroupQuery
                .Select( g => new { g.Id, g.Name, g.CampusId, g.GroupCapacity, g.GroupTypeId } )
                .ToList()
                .Select( g => new Group { Id = g.Id, Name = g.Name, CampusId = g.CampusId, GroupCapacity = g.GroupCapacity, GroupTypeId = g.GroupTypeId } );

            _registrationTemplatePlacementGroupIds = registrationTemplatePlacementGroupList.Select( a => a.Id ).ToList();
            var placementGroupList = new List<Group>();
            placementGroupList.AddRange( registrationTemplatePlacementGroupList );
            placementGroupList.AddRange( registrationInstancePlacementGroupList );

            // A placement group could be associated with both the instance and the template. So, we'll make sure the same group isn't listed more than once
            placementGroupList = placementGroupList.DistinctBy( a => a.Id ).ToList();
            placementGroupList = placementGroupList.OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            var groupIds = placementGroupList.Select( a => a.Id ).ToList();
            var groupMemberService = new GroupMemberService( rockContext );
            _placementGroupIdGroupRoleIdCount = groupMemberService
               .Queryable()
               .Where( a => a.GroupMemberStatus != GroupMemberStatus.Inactive && groupIds.Contains( a.GroupId ) )
               .Select( gm => new { GroupId = gm.GroupId, GroupRoleId = gm.GroupRoleId } )
               .ToList()
               .GroupBy( gm => gm.GroupId )
               .ToDictionary( a => a.Key, b => b.GroupBy( a => a.GroupRoleId ).ToDictionary( a => a.Key, a => a.Count() ) );

            rptPlacementGroups.DataSource = placementGroupList;
            rptPlacementGroups.DataBind();
        }

        private IQueryable<Group> ApplyGroupFilter( GroupTypeCache groupType, GroupService groupService, IQueryable<Group> placementGroupsQry )
        {
            var preferences = GetBlockPersonPreferences();
            Dictionary<int, string> attributeFilters = preferences.GetValue( string.Format( UserPreferenceKey.GroupAttributeFilter_GroupTypeId, groupType.Id ) ).FromJsonOrNull<Dictionary<int, string>>();
            var parameterExpression = groupService.ParameterExpression;
            Expression groupWhereExpression = null;
            if (attributeFilters != null)
            {
                foreach (var attributeFilter in attributeFilters)
                {
                    var attribute = AttributeCache.Get(attributeFilter.Key);
                    var attributeFilterValues = attributeFilter.Value.FromJsonOrNull<List<string>>();
                    var entityField = EntityHelper.GetEntityFieldForAttribute(attribute);
                    if (entityField != null && attributeFilterValues != null)
                    {
                        var attributeWhereExpression = ExpressionHelper.GetAttributeExpression(groupService, parameterExpression, entityField, attributeFilterValues);
                        if (groupWhereExpression == null)
                        {
                            groupWhereExpression = attributeWhereExpression;
                        }
                        else
                        {
                            groupWhereExpression = Expression.AndAlso(groupWhereExpression, attributeWhereExpression);
                        }
                    }
                }
            }

            if ( groupWhereExpression != null )
            {
                placementGroupsQry = placementGroupsQry.Where( parameterExpression, groupWhereExpression );
            }

            return placementGroupsQry;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            // do nothing
        }

        /// <summary>
        /// Handles the Click event of the btnAddPlacementGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddPlacementGroup_Click( object sender, EventArgs e )
        {
            var newGroup = new Group() { GroupTypeId = hfRegistrationTemplatePlacementGroupTypeId.Value.AsInteger() };

            // set up Attribute Values Container
            avcNewPlacementGroupAttributeValues.AddEditControls( newGroup );

            if ( !newGroup.IsAuthorized( Rock.Security.Authorization.EDIT, this.CurrentPerson ) )
            {
                // if they can't add groups of this type, only show the Add to Existing group option
                bgAddNewOrExistingPlacementGroup.Visible = false;
                bgAddNewOrExistingPlacementGroup.SetValue( AddPlacementGroupTab.AddExistingGroup.ConvertToInt().ToString() );
                pnlAddNewPlacementGroup.Visible = false;
            }
            else
            {
                bgAddNewOrExistingPlacementGroup.SetValue( AddPlacementGroupTab.AddNewGroup.ConvertToInt().ToString() );
            }

            bgAddNewOrExistingPlacementGroup_SelectedIndexChanged( null, null );

            var registrationInstanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
            var registrationTemplatePlacementId = hfRegistrationTemplatePlacementId.Value.AsIntegerOrNull();

            if ( registrationInstanceId.HasValue )
            {
                // in Registration Instance Mode
                mdAddPlacementGroup.Title = "Add Group";
            }
            else if ( registrationTemplatePlacementId.HasValue )
            {
                // in Registration Template Mode
                mdAddPlacementGroup.Title = "Add Shared Group";
            }

            nbNotAllowedToAddGroup.Visible = false;
            nbNewPlacementGroupParentGroupWarning.Visible = false;
            gpNewPlacementGroupParentGroup.SetValue( ( int? ) null );
            tbNewPlacementGroupName.Text = string.Empty;
            cpNewPlacementGroupCampus.SetValue( ( int? ) null );
            nbGroupCapacity.Text = string.Empty;
            tbNewPlacementGroupDescription.Text = string.Empty;

            nbAddExistingPlacementGroupWarning.Visible = false;
            gpAddExistingPlacementGroup.SetValue( ( int? ) null );

            nbNewPlacementGroupParentGroupWarning.Visible = false;

            nbAddExistingPlacementMultipleGroupsWarning.Visible = false;
            gpAddExistingPlacementGroupsFromParent.SetValue( ( int? ) null );

            mdAddPlacementGroup.Show();
        }

        /// <summary>
        /// Handles the Click event of the btnConfiguration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfiguration_Click( object sender, EventArgs e )
        {
            var placementConfiguration = GetPlacementConfiguration();

            mdPlacementConfiguration.Show();
            cpConfigurationCampusPicker.SelectedCampusId = placementConfiguration.DisplayedCampusId;
            var registrationInstanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
            var registrationTemplateId = hfRegistrationTemplateId.Value.AsInteger();

            bool inTemplateMode = registrationInstanceId == null;
            pwRegistrationTemplateConfiguration.Visible = inTemplateMode;

            var rockContext = new RockContext();

            DataViewFilter dataViewFilter = null;

            if ( placementConfiguration.RegistrantPersonDataViewFilterId.HasValue )
            {
                dataViewFilter = new DataViewFilterService( rockContext ).Get( placementConfiguration.RegistrantPersonDataViewFilterId.Value );
            }

            if ( dataViewFilter == null || dataViewFilter.ExpressionType == FilterExpressionType.Filter )
            {
                dataViewFilter = new DataViewFilter();
                dataViewFilter.Guid = new Guid();
                dataViewFilter.ExpressionType = FilterExpressionType.GroupAll;
            }

            CreateFilterControl( dataViewFilter, true, rockContext );

            AddRegistrantAttributeFilters( true );

            if ( inTemplateMode )
            {
                cbShowRegistrationInstanceName.Checked = placementConfiguration.ShowRegistrationInstanceName;

                cblRegistrationInstances.Items.Clear();
                var registrationTemplateInstances = new RegistrationInstanceService( rockContext ).Queryable()
                    .Where( a => a.RegistrationTemplateId == registrationTemplateId )
                    .OrderBy( a => a.Name )
                    .Select( a => new { a.Id, a.Name } ).ToList();

                foreach ( var registrationTemplateInstance in registrationTemplateInstances )
                {
                    cblRegistrationInstances.Items.Add( new ListItem( registrationTemplateInstance.Name, registrationTemplateInstance.Id.ToString() ) );
                }

                cblRegistrationInstances.SetValues( placementConfiguration.IncludedRegistrationInstanceIds );
            }

            cbHighlightGenders.Checked = placementConfiguration.HighlightGenders;
            cbShowFees.Checked = placementConfiguration.ShowFees;

            cblDisplayedRegistrantAttributes.Items.Clear();

            var registrantAttributes = GetRegistrantFormFields().Where( a => a.FieldSource == RegistrationFieldSource.RegistrantAttribute ).Select( a => a.Attribute ).Where( a => a != null ).ToList();

            foreach ( var registrantAttribute in registrantAttributes.OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
            {
                cblDisplayedRegistrantAttributes.Items.Add( new ListItem( registrantAttribute.Name, registrantAttribute.Id.ToString() ) );
            }

            cblDisplayedRegistrantAttributes.SetValues( placementConfiguration.DisplayedRegistrantAttributeIds );
            cbDisplayRegistrantAttributes.Checked = placementConfiguration.DisplayRegistrantAttributes;
            cbApplyRegistrantFilters.Checked = placementConfiguration.ApplyRegistrantFilters;
            var fakeGroup = new Rock.Model.Group { GroupTypeId = hfRegistrationTemplatePlacementGroupTypeId.Value.AsInteger() };
            fakeGroup.LoadAttributes();

            var groupAttributeList = fakeGroup.Attributes.Select( a => a.Value ).ToList();
            cblDisplayedGroupAttributes.Items.Clear();
            foreach ( var groupAttribute in groupAttributeList.OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
            {
                cblDisplayedGroupAttributes.Items.Add( new ListItem( groupAttribute.Name, groupAttribute.Id.ToString() ) );
            }

            cblDisplayedGroupAttributes.SetValues( placementConfiguration.DisplayedGroupAttributeIds );
            AddGroupAttributeFilters( true );

            var fakeGroupMember = new GroupMember() { Group = fakeGroup, GroupId = fakeGroup.Id };
            fakeGroupMember.LoadAttributes();
            var groupMemberAttributeList = fakeGroupMember.Attributes.Select( a => a.Value ).ToList();
            cblDisplayedGroupMemberAttributes.Items.Clear();
            foreach ( var groupMemberAttribute in groupMemberAttributeList.OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
            {
                cblDisplayedGroupMemberAttributes.Items.Add( new ListItem( groupMemberAttribute.Name, groupMemberAttribute.Id.ToString() ) );
            }

            cblDisplayedGroupMemberAttributes.SetValues( placementConfiguration.DisplayedGroupMemberAttributeIds );
            AddGroupMemberAttributeFilters( true );
            cbHideFullGroups.Checked = placementConfiguration.HideFullGroups;

            var registrationTemplateFeeService = new RegistrationTemplateFeeService( new RockContext() );
            var templateFees = registrationTemplateFeeService.Queryable().Where( f => f.RegistrationTemplateId == registrationTemplateId ).ToList();

            ddlFeeName.Items.Clear();
            ddlFeeName.Items.Add( new ListItem() );

            foreach ( var templateFee in templateFees )
            {
                ddlFeeName.Items.Add( new ListItem( templateFee.Name, templateFee.Id.ToString() ) );
            }

            rcwFeeFilters.Visible = templateFees.Any();

            cblFeeOptions.Visible = false;

            if ( placementConfiguration.FilterFeeId.HasValue )
            {
                ddlFeeName.SetValue( placementConfiguration.FilterFeeId.Value );
                ddlFeeName_SelectedIndexChanged( ddlFeeName, null );
                cblFeeOptions.SetValues( placementConfiguration.FilterFeeOptionIds );
            }

            UpdateDisplayedRegistrantFilters();
            UpdateDisplayedGroupFilters();
            UpdateDisplayedGroupMemberFilters();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdPlacementConfiguration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdPlacementConfiguration_SaveClick( object sender, EventArgs e )
        {
            var dataViewFilter = ReportingHelper.GetFilterFromControls( phPersonFilters );

            SaveRegistrantAttributeFilters();
            SaveGroupAttributeFilters();
            SaveGroupMemberAttributeFilters();
            // update Guids since we are creating a new dataFilter and children and deleting the old one
            SetNewDataFilterGuids( dataViewFilter );

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !dataViewFilter.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            var rockContext = new RockContext();
            DataViewFilterService dataViewFilterService = new DataViewFilterService( rockContext );

            var placementConfiguration = GetPlacementConfiguration() ?? new PlacementConfiguration();

            int? dataViewFilterId = placementConfiguration.RegistrantPersonDataViewFilterId;
            if ( dataViewFilterId.HasValue )
            {
                var oldDataViewFilter = dataViewFilterService.Get( dataViewFilterId.Value );
                DeleteDataViewFilter( oldDataViewFilter, dataViewFilterService );
            }

            dataViewFilterService.Add( dataViewFilter );

            rockContext.SaveChanges();

            placementConfiguration.DisplayedCampusId = cpConfigurationCampusPicker.SelectedCampusId;
            placementConfiguration.ShowRegistrationInstanceName = cbShowRegistrationInstanceName.Checked;
            placementConfiguration.IncludedRegistrationInstanceIds = cblRegistrationInstances.SelectedValues.AsIntegerList().ToArray();
            placementConfiguration.HighlightGenders = cbHighlightGenders.Checked;
            placementConfiguration.ShowFees = cbShowFees.Checked;
            placementConfiguration.DisplayedRegistrantAttributeIds = cblDisplayedRegistrantAttributes.SelectedValues.AsIntegerList().ToArray();
            placementConfiguration.DisplayRegistrantAttributes = cbDisplayRegistrantAttributes.Checked;
            placementConfiguration.ApplyRegistrantFilters = cbApplyRegistrantFilters.Checked;
            placementConfiguration.RegistrantPersonDataViewFilterId = dataViewFilter.Id;
            placementConfiguration.DisplayedGroupAttributeIds = cblDisplayedGroupAttributes.SelectedValues.AsIntegerList().ToArray();
            placementConfiguration.HideFullGroups = cbHideFullGroups.Checked;
            placementConfiguration.DisplayedGroupMemberAttributeIds = cblDisplayedGroupMemberAttributes.SelectedValues.AsIntegerList().ToArray();

            placementConfiguration.FilterFeeId = ddlFeeName.SelectedValue.AsIntegerOrNull();
            placementConfiguration.FilterFeeOptionIds = cblFeeOptions.SelectedValuesAsInt.ToArray();

            SavePlacementConfiguration( placementConfiguration );

            mdPlacementConfiguration.Hide();
        }

        private List<GroupTypeRoleCache> _groupTypeRoles;
        private string _placementIconCssClass;

        /// <summary>
        /// Handles the ItemDataBound event of the rptPlacementGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptPlacementGroups_ItemDataBound( object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e )
        {
            var placementGroup = e.Item.DataItem as Group;
            if ( placementGroup == null )
            {
                return;
            }

            var hfPlacementGroupId = e.Item.FindControl( "hfPlacementGroupId" ) as HiddenFieldWithClass;
            hfPlacementGroupId.Value = placementGroup.Id.ToString();

            var hfPlacementGroupCapacity = e.Item.FindControl( "hfPlacementGroupCapacity" ) as HiddenFieldWithClass;
            hfPlacementGroupCapacity.Value = placementGroup.GroupCapacity.ToString();

            var lGroupIconHtml = e.Item.FindControl( "lGroupIconHtml" ) as Literal;
            lGroupIconHtml.Text = string.Format( "<i class='{0}'></i>", _placementIconCssClass );

            var lGroupName = e.Item.FindControl( "lGroupName" ) as Literal;
            lGroupName.Text = placementGroup.Name;

            var hlGroupCampus = e.Item.FindControl( "hlGroupCampus" ) as HighlightLabel;
            if ( placementGroup.CampusId.HasValue )
            {
                hlGroupCampus.Text = CampusCache.Get( placementGroup.CampusId.Value ).Name;
                hlGroupCampus.Visible = true;
            }

            var hlInstanceName = e.Item.FindControl( "hlInstanceName" ) as HighlightLabel;
            var hlRegistrationTemplatePlacementName = e.Item.FindControl( "hlRegistrationTemplatePlacementName" ) as HighlightLabel;

            var groupRegistrationInstanceIds = _placementGroupIdRegistrationInstanceIds.GetValueOrDefault( placementGroup.Id, new List<int>() );
            if ( groupRegistrationInstanceIds.Any() )
            {
                hlInstanceName.Text = new RegistrationInstanceService( new RockContext() ).GetByIds( groupRegistrationInstanceIds ).Select( a => a.Name ).ToList().AsDelimited( ",", "and" );
                hlInstanceName.Visible = true;

                // if there is exactly one registration instance that has this as a placement group, set what instance this group placement is associated with
                // If there are multiple, leave this blank
                if ( groupRegistrationInstanceIds.Count() == 1 )
                {
                    var hfPlacementGroupRegistrationInstanceId = e.Item.FindControl( "hfPlacementGroupRegistrationInstanceId" ) as HiddenFieldWithClass;
                    int groupRegistrationInstanceId = groupRegistrationInstanceIds[0];
                    hfPlacementGroupRegistrationInstanceId.Value = groupRegistrationInstanceId.ToString();
                }
            }

            if ( _registrationTemplatePlacementGroupIds.Contains( placementGroup.Id ) )
            {
                hlRegistrationTemplatePlacementName.Text = "Shared";
                hlRegistrationTemplatePlacementName.Visible = true;

                /* Don't show the detach and delete actions for shared groups since that would affect all the instances */
                var detachPlacementGroup = e.Item.FindControl( "detachPlacementGroup" ) as HtmlAnchor;
                detachPlacementGroup.Visible = false;

                var deleteGroup = e.Item.FindControl( "deleteGroup" ) as HtmlAnchor;
                deleteGroup.Visible = false;

                var actionSeparator = e.Item.FindControl( "actionSeparator" ) as HtmlGenericControl;
                actionSeparator.Visible = false;
            }

            var pnlGroupAttributes = e.Item.FindControl( "pnlGroupAttributes" ) as Panel;

            var displayedAttributes = GetPlacementConfiguration().DisplayedGroupAttributeIds.Select( a => AttributeCache.Get( a ) ).Where( a => a != null ).ToArray();

            if ( displayedAttributes.Any() )
            {
                pnlGroupAttributes.Visible = true;
                var avcGroupAttributes = e.Item.FindControl( "avcGroupAttributes" ) as AttributeValuesContainer;
                avcGroupAttributes.ShowCategoryLabel = false;
                avcGroupAttributes.NumberOfColumns = 2;
                avcGroupAttributes.IncludedAttributes = displayedAttributes;
                placementGroup.LoadAttributes();
                avcGroupAttributes.AddDisplayControls( placementGroup );
                pnlGroupAttributes.Visible = avcGroupAttributes.GetDisplayedAttributes().Any();
            }
            else
            {
                pnlGroupAttributes.Visible = false;
            }

            var rptPlacementGroupRole = e.Item.FindControl( "rptPlacementGroupRole" ) as Repeater;
            if ( !_placementGroupIdGroupRoleIdCount.ContainsKey( placementGroup.Id ) )
            {
                _placementGroupIdGroupRoleIdCount.Add( placementGroup.Id, new Dictionary<int, int>() );
            }

            var groupRoleIdCount = _placementGroupIdGroupRoleIdCount[placementGroup.Id];
            rptPlacementGroupRole.DataSource = _groupTypeRoles.ToDictionary( a => a, a => groupRoleIdCount.ContainsKey( a.Id ) ? groupRoleIdCount[a.Id] : 0 );
            rptPlacementGroupRole.DataBind();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptPlacementGroupRole control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptPlacementGroupRole_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if (e.Item.DataItem == null)
            {
                return;
            }

            var groupTypeRole = (KeyValuePair<GroupTypeRoleCache, int>)e.Item.DataItem;
            var hfGroupTypeRoleId = e.Item.FindControl( "hfGroupTypeRoleId" ) as HiddenFieldWithClass;
            hfGroupTypeRoleId.Value = groupTypeRole.Key.Id.ToString();

            var hfGroupTypeRoleMaxMembers = e.Item.FindControl( "hfGroupTypeRoleMaxMembers" ) as HiddenFieldWithClass;
            hfGroupTypeRoleMaxMembers.Value = groupTypeRole.Key.MaxCount.ToString();

            var lGroupRoleName = e.Item.FindControl( "lGroupRoleName" ) as Literal;
            lGroupRoleName.Text = groupTypeRole.Key.Name.Pluralize();

            var lGroupRoleCount = e.Item.FindControl( "lGroupRoleCount" ) as Literal;
            lGroupRoleCount.Text = groupTypeRole.Value.ToStringSafe();
        }

        #endregion

        /// <summary>
        /// Reloads the page with registration template placement identifier parameter
        /// </summary>
        /// <param name="registrationTemplatePlacementId">The registration template placement identifier.</param>
        private void ReloadPageWithRegistrationTemplatePlacementId( int registrationTemplatePlacementId )
        {
            var additionalQueryParameters = new Dictionary<string, string>();
            additionalQueryParameters.Add( PageParameterKey.RegistrationTemplatePlacementId, registrationTemplatePlacementId.ToString() );
            additionalQueryParameters.Add( PageParameterKey.PromptForTemplatePlacement, true.ToString() );
            NavigateToCurrentPageReference( additionalQueryParameters );
        }

        #region Add Placement Group

        /// <summary>
        /// Handles the SaveClick event of the mdAddPlacementGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdAddPlacementGroup_SaveClick( object sender, EventArgs e )
        {
            List<Group> placementGroups;
            var groupTypeId = hfRegistrationTemplatePlacementGroupTypeId.Value.AsInteger();
            var rockContext = new RockContext();
            nbAddExistingPlacementMultipleGroupsWarning.Visible = false;
            nbAddExistingPlacementGroupWarning.Visible = false;

            if ( bgAddNewOrExistingPlacementGroup.SelectedValue == AddPlacementGroupTab.AddExistingGroup.ConvertToInt().ToString() )
            {
                placementGroups = new List<Group>();
                var existingGroupIds = gpAddExistingPlacementGroup.SelectedValuesAsInt();

                foreach ( var groupId in existingGroupIds )
                {
                    var existingPlacementGroup = new GroupService( rockContext ).Get( groupId );
                    if ( existingPlacementGroup == null )
                    {
                        continue;
                    }

                    if ( groupTypeId != existingPlacementGroup.GroupTypeId )
                    {
                        continue;
                    }

                    placementGroups.Add( existingPlacementGroup );
                }
            }
            else if ( bgAddNewOrExistingPlacementGroup.SelectedValue == AddPlacementGroupTab.AddMultipleGroups.ConvertToInt().ToString() )
            {
                var parentGroupId = gpAddExistingPlacementGroupsFromParent.SelectedValueAsId();
                if ( parentGroupId == null )
                {
                    nbAddExistingPlacementMultipleGroupsWarning.Visible = true;
                    nbAddExistingPlacementMultipleGroupsWarning.Text = "Please select a parent group to add the groups.";
                    return;
                }

                string errorMessage;
                if ( !HasValidChildGroups( parentGroupId.Value, groupTypeId, out errorMessage ) )
                {
                    nbAddExistingPlacementMultipleGroupsWarning.Visible = true;
                    nbAddExistingPlacementMultipleGroupsWarning.Text = errorMessage;
                    return;
                }

                var existingPlacementGroups = new GroupService( rockContext ).Queryable().Where( a => a.ParentGroupId == parentGroupId && a.IsActive == true ).ToList();
                placementGroups = existingPlacementGroups;
            }
            else
            {
                var newPlacementGroup = new Group();
                newPlacementGroup.GroupTypeId = groupTypeId;

                var groupService = new GroupService( rockContext );
                if ( gpNewPlacementGroupParentGroup.GroupId.HasValue )
                {
                    newPlacementGroup.ParentGroupId = gpNewPlacementGroupParentGroup.GroupId;
                    newPlacementGroup.ParentGroup = groupService.Get( newPlacementGroup.ParentGroupId.Value );
                }

                newPlacementGroup.Name = tbNewPlacementGroupName.Text;
                newPlacementGroup.CampusId = cpNewPlacementGroupCampus.SelectedCampusId;
                newPlacementGroup.GroupCapacity = nbGroupCapacity.Text.AsIntegerOrNull();
                newPlacementGroup.Description = tbNewPlacementGroupDescription.Text;

                groupService.Add( newPlacementGroup );

                if ( !newPlacementGroup.IsAuthorized( Rock.Security.Authorization.EDIT, this.CurrentPerson ) )
                {
                    nbNotAllowedToAddGroup.Visible = true;
                    return;
                }

                rockContext.SaveChanges();
                avcNewPlacementGroupAttributeValues.GetEditValues( newPlacementGroup );
                newPlacementGroup.SaveAttributeValues();
                placementGroups = new List<Group>();
                placementGroups.Add( newPlacementGroup );
            }

            var registrationInstanceService = new RegistrationInstanceService( rockContext );
            var registrationTemplatePlacementService = new RegistrationTemplatePlacementService( rockContext );
            var registrationInstanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
            var registrationTemplatePlacementId = hfRegistrationTemplatePlacementId.Value.AsIntegerOrNull();

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

            rockContext.SaveChanges();

            mdAddPlacementGroup.Hide();
            BindPlacementGroupsRepeater();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the bgAddNewOrExistingPlacementGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected void bgAddNewOrExistingPlacementGroup_SelectedIndexChanged( object sender, EventArgs e )
        {
            pnlAddMultipleGroups.Visible = bgAddNewOrExistingPlacementGroup.SelectedValue == AddPlacementGroupTab.AddMultipleGroups.ConvertToInt().ToString();
            pnlAddExistingPlacementGroup.Visible = bgAddNewOrExistingPlacementGroup.SelectedValue == AddPlacementGroupTab.AddExistingGroup.ConvertToInt().ToString();
            pnlAddNewPlacementGroup.Visible = bgAddNewOrExistingPlacementGroup.SelectedValue == AddPlacementGroupTab.AddNewGroup.ConvertToInt().ToString();
        }

        /// <summary>
        /// Handles the SelectItem event of the gpNewPlacementGroupParentGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gpNewPlacementGroupParentGroup_SelectItem( object sender, EventArgs e )
        {
            int groupTypeId = hfRegistrationTemplatePlacementGroupTypeId.Value.AsInteger();
            var selectedParentGroup = new GroupService( new RockContext() ).Get( gpNewPlacementGroupParentGroup.SelectedValue.AsInteger() );
            if ( !IsValidParentGroup( selectedParentGroup, groupTypeId ) )
            {
                var groupType = GroupTypeCache.Get( groupTypeId );
                nbNewPlacementGroupParentGroupWarning.Text = string.Format( "The selected parent group doesn't allow adding {0} child groups", groupType );
                nbNewPlacementGroupParentGroupWarning.Visible = true;
            }
            else
            {
                nbNewPlacementGroupParentGroupWarning.Visible = false;
            }
        }

        /// <summary>
        /// Determines whether [is valid parent group] [the specified parent group].
        /// </summary>
        /// <param name="parentGroup">The parent group.</param>
        /// <param name="childGroupTypeId">The child group type identifier.</param>
        /// <returns>
        ///   <c>true</c> if [is valid parent group] [the specified parent group]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsValidParentGroup( Group parentGroup, int childGroupTypeId )
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
        /// Handles the SelectItem event of the gpAddExistingPlacementGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gpAddExistingPlacementGroup_SelectItem( object sender, EventArgs e )
        {
            int groupTypeId = hfRegistrationTemplatePlacementGroupTypeId.Value.AsInteger();
            var selectedGroupIds = gpAddExistingPlacementGroup.SelectedValuesAsInt();
            var selectedGroups = new GroupService( new RockContext() )
                .Queryable().AsNoTracking()
                .Where( g => selectedGroupIds.Contains( g.Id ) )
                .ToList();

            var invalidGroups = selectedGroups.Where( g => !IsValidExistingGroup( g, groupTypeId ) );

            if ( invalidGroups.Any() )
            {
                var groupType = GroupTypeCache.Get( groupTypeId );
                nbAddExistingPlacementGroupWarning.Text = string.Format( "The selected groups must be {0} groups", groupType );
                nbAddExistingPlacementGroupWarning.Visible = true;
            }
            else
            {
                nbAddExistingPlacementGroupWarning.Visible = false;
            }
        }

        /// <summary>
        /// Handles the SelectItem event of the gpAddExistingPlacementGroupsFromParent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gpAddExistingPlacementGroupsFromParent_SelectItem( object sender, EventArgs e )
        {
            int groupTypeId = hfRegistrationTemplatePlacementGroupTypeId.Value.AsInteger();
            nbAddExistingPlacementMultipleGroupsWarning.Visible = false;
            var parentGroupId = gpAddExistingPlacementGroupsFromParent.SelectedValueAsId();
            if ( parentGroupId == null )
            {
                return;
            }

            string errorMessage;
            if ( !HasValidChildGroups( parentGroupId.Value, groupTypeId, out errorMessage ) )
            {
                nbAddExistingPlacementMultipleGroupsWarning.Visible = true;
                nbAddExistingPlacementMultipleGroupsWarning.Text = errorMessage;
                return;
            }
        }

        /// <summary>
        /// Determines whether [is valid existing group] [the specified selected group].
        /// </summary>
        /// <param name="selectedGroup">The selected group.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns>
        ///   <c>true</c> if [is valid existing group] [the specified selected group]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsValidExistingGroup( Group selectedGroup, int groupTypeId )
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

            List<Group> invalidGroups = new List<Group>();
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

        #endregion Add Placement Group

        #region Person Filter

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var dataViewFilter = DataViewFilter.FromJson( ViewState[ViewStateKey.DataFilterJSON].ToString() );

            if ( dataViewFilter != null )
            {
                CreateFilterControl( dataViewFilter, false, new RockContext() );
            }

            AddRegistrantAttributeFilters( false );
            AddGroupAttributeFilters( false );
            AddGroupMemberAttributeFilters( true );
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns <see langword="null" />.
        /// </returns>
        protected override object SaveViewState()
        {
            var dataViewFilterJson = ReportingHelper.GetFilterFromControls( phPersonFilters ).ToJson();
            ViewState[ViewStateKey.DataFilterJSON] = dataViewFilterJson;

            return base.SaveViewState();
        }

        /// <summary>
        /// Handles the AddFilterClick event of the groupControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void groupControl_AddFilterClick( object sender, EventArgs e )
        {
            FilterGroup groupControl = sender as FilterGroup;
            FilterField filterField = new FilterField();

            filterField.DataViewFilterGuid = Guid.NewGuid();
            groupControl.Controls.Add( filterField );
            filterField.ID = string.Format( "ff_{0}", filterField.DataViewFilterGuid.ToString( "N" ) );

            filterField.FilteredEntityTypeName = groupControl.FilteredEntityTypeName;
            filterField.Expanded = true;

            filterField.DeleteClick += filterControl_DeleteClick;
        }

        /// <summary>
        /// Handles the AddGroupClick event of the groupControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void groupControl_AddGroupClick( object sender, EventArgs e )
        {
            FilterGroup groupControl = sender as FilterGroup;
            FilterGroup childGroupControl = new FilterGroup();
            childGroupControl.DataViewFilterGuid = Guid.NewGuid();
            groupControl.Controls.Add( childGroupControl );
            childGroupControl.ID = string.Format( "fg_{0}", childGroupControl.DataViewFilterGuid.ToString( "N" ) );
            childGroupControl.FilteredEntityTypeName = groupControl.FilteredEntityTypeName;
            childGroupControl.FilterType = FilterExpressionType.GroupAll;

            childGroupControl.AddFilterClick += groupControl_AddFilterClick;
            childGroupControl.AddGroupClick += groupControl_AddGroupClick;
            childGroupControl.DeleteGroupClick += groupControl_DeleteGroupClick;
        }

        /// <summary>
        /// Handles the DeleteClick event of the filterControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void filterControl_DeleteClick( object sender, EventArgs e )
        {
            FilterField fieldControl = sender as FilterField;
            fieldControl.Parent.Controls.Remove( fieldControl );
        }

        /// <summary>
        /// Handles the DeleteGroupClick event of the groupControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void groupControl_DeleteGroupClick( object sender, EventArgs e )
        {
            FilterGroup groupControl = sender as FilterGroup;
            groupControl.Parent.Controls.Remove( groupControl );
        }

        /// <summary>
        /// Creates the filter control.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="setSelection">if set to <c>true</c> [set selection].</param>
        /// <param name="rockContext">The rock context.</param>
        private void CreateFilterControl( DataViewFilter filter, bool setSelection, RockContext rockContext )
        {
            phPersonFilters.Controls.Clear();
            if ( filter != null )
            {
                CreateFilterControl( phPersonFilters, filter, setSelection, rockContext );
            }
        }

        /// <summary>
        /// Creates the filter control.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="setSelection">if set to <c>true</c> [set selection].</param>
        /// <param name="rockContext">The rock context.</param>
        private void CreateFilterControl( Control parentControl, DataViewFilter filter, bool setSelection, RockContext rockContext )
        {
            try
            {
                if ( filter.ExpressionType == FilterExpressionType.Filter )
                {
                    var filterControl = new FilterField();

                    parentControl.Controls.Add( filterControl );
                    filterControl.DataViewFilterGuid = filter.Guid;
                    filterControl.ID = string.Format( "ff_{0}", filterControl.DataViewFilterGuid.ToString( "N" ) );

                    filterControl.FilteredEntityTypeName = typeof( Person ).FullName;

                    if ( filter.EntityTypeId.HasValue )
                    {
                        var entityTypeCache = EntityTypeCache.Get( filter.EntityTypeId.Value, rockContext );
                        if ( entityTypeCache != null )
                        {
                            filterControl.FilterEntityTypeName = entityTypeCache.Name;
                        }
                    }

                    filterControl.Expanded = filter.Expanded;
                    if ( setSelection )
                    {
                        try
                        {
                            filterControl.SetSelection( filter.Selection );
                        }
                        catch ( Exception ex )
                        {
                            this.LogException( new Exception( "Exception setting selection for DataViewFilter: " + filter.Guid, ex ) );
                        }
                    }

                    filterControl.DeleteClick += filterControl_DeleteClick;
                }
                else
                {
                    var groupControl = new FilterGroup();
                    parentControl.Controls.Add( groupControl );
                    groupControl.DataViewFilterGuid = filter.Guid;
                    groupControl.ID = string.Format( "fg_{0}", groupControl.DataViewFilterGuid.ToString( "N" ) );
                    groupControl.FilteredEntityTypeName = typeof( Person ).FullName;
                    groupControl.IsDeleteEnabled = parentControl is FilterGroup;
                    if ( setSelection )
                    {
                        groupControl.FilterType = filter.ExpressionType;
                    }

                    groupControl.AddFilterClick += groupControl_AddFilterClick;
                    groupControl.AddGroupClick += groupControl_AddGroupClick;
                    groupControl.DeleteGroupClick += groupControl_DeleteGroupClick;
                    foreach ( var childFilter in filter.ChildFilters )
                    {
                        CreateFilterControl( groupControl, childFilter, setSelection, rockContext );
                    }
                }
            }
            catch ( Exception ex )
            {
                this.LogException( new Exception( "Exception creating FilterControl for DataViewFilter: " + filter.Guid, ex ) );
            }
        }

        /// <summary>
        /// Sets the new data filter guids.
        /// </summary>
        /// <param name="dataViewFilter">The data view filter.</param>
        private void SetNewDataFilterGuids( DataViewFilter dataViewFilter )
        {
            if ( dataViewFilter != null )
            {
                dataViewFilter.Guid = Guid.NewGuid();
                foreach ( var childFilter in dataViewFilter.ChildFilters )
                {
                    SetNewDataFilterGuids( childFilter );
                }
            }
        }

        /// <summary>
        /// Deletes the data view filter.
        /// </summary>
        /// <param name="dataViewFilter">The data view filter.</param>
        /// <param name="service">The service.</param>
        private void DeleteDataViewFilter( DataViewFilter dataViewFilter, DataViewFilterService service )
        {
            if ( dataViewFilter != null )
            {
                foreach ( var childFilter in dataViewFilter.ChildFilters.ToList() )
                {
                    DeleteDataViewFilter( childFilter, service );
                }

                service.Delete( dataViewFilter );
            }
        }

        #endregion Person Filter

        #region Registrant Filter

        /// <summary>
        /// Adds the registrant attribute filters.
        /// </summary>
        public void AddRegistrantAttributeFilters( bool setValues )
        {
            var registrantAttributeFields = this.GetRegistrantFormFields().Where( a => a.FieldSource == RegistrationFieldSource.RegistrantAttribute ).ToList();
            phRegistrantFilters.Controls.Clear();

            int? registrationInstanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
            int registrationTemplateId = hfRegistrationTemplateId.Value.AsInteger();

            Dictionary<int, string> attributeFilters;
            var preferences = GetBlockPersonPreferences();
            if ( registrationInstanceId.HasValue )
            {
                attributeFilters = preferences.GetValue( string.Format( UserPreferenceKey.RegistrantAttributeFilter_RegistrationInstanceId, registrationInstanceId.Value ) ).FromJsonOrNull<Dictionary<int, string>>();
            }
            else
            {
                attributeFilters = preferences.GetValue( string.Format( UserPreferenceKey.RegistrantAttributeFilter_RegistrationTemplateId, registrationTemplateId ) ).FromJsonOrNull<Dictionary<int, string>>();
            }

            foreach ( var field in registrantAttributeFields )
            {
                var attribute = field.Attribute;

                // Add dynamic filter fields
                var filterFieldControl = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filterRegistrants_" + attribute.Id.ToString(), false, Rock.Reporting.FilterMode.SimpleFilter );

                if ( setValues && attributeFilters != null )
                {
                    var attributeFilterValue = attributeFilters.GetValueOrNull( attribute.Id );
                    if ( attributeFilterValue.IsNotNullOrWhiteSpace() )
                    {
                        var filterValues = attributeFilterValue.FromJsonOrNull<List<string>>();
                        attribute.FieldType.Field.SetFilterValues( filterFieldControl, attribute.QualifierValues, filterValues );
                    }
                }

                if ( filterFieldControl != null )
                {
                    if ( filterFieldControl is IRockControl )
                    {
                        var rockControl = ( IRockControl ) filterFieldControl;
                        rockControl.Label = attribute.Name;
                        rockControl.Help = attribute.Description;
                        phRegistrantFilters.Controls.Add( filterFieldControl );
                    }
                    else
                    {
                        var wrapper = new RockControlWrapper();
                        wrapper.ID = filterFieldControl.ID + "_wrapper";
                        wrapper.Label = attribute.Name;
                        wrapper.Controls.Add( filterFieldControl );
                        phRegistrantFilters.Controls.Add( wrapper );
                    }
                }
            }
        }

        /// <summary>
        /// Saves the registrant attribute filters.
        /// </summary>
        public void SaveRegistrantAttributeFilters()
        {
            var registrantAttributeFields = this.GetRegistrantFormFields().Where( a => a.FieldSource == RegistrationFieldSource.RegistrantAttribute ).ToList();
            Dictionary<int, string> attributeFilters = new Dictionary<int, string>();
            foreach ( var field in registrantAttributeFields )
            {
                var attribute = field.Attribute;
                var filterControl = phRegistrantFilters.FindControl( "filterRegistrants_" + attribute.Id.ToString() );

                if ( filterControl != null && filterControl.Visible )
                {
                    try
                    {
                        var values = attribute.FieldType.Field.GetFilterValues( filterControl, field.Attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                        attributeFilters.Add( attribute.Id, attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson() );
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            int? registrationInstanceId = hfRegistrationInstanceId.Value.AsIntegerOrNull();
            int registrationTemplateId = hfRegistrationTemplateId.Value.AsInteger();
            var preferences = GetBlockPersonPreferences();

            if ( registrationInstanceId.HasValue )
            {
                preferences.SetValue( string.Format( UserPreferenceKey.RegistrantAttributeFilter_RegistrationInstanceId, registrationInstanceId.Value ), attributeFilters.ToJson() );
            }
            else
            {
                preferences.SetValue( string.Format( UserPreferenceKey.RegistrantAttributeFilter_RegistrationTemplateId, registrationTemplateId ), attributeFilters.ToJson() );
            }

            preferences.Save();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblDisplayedRegistrantAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblDisplayedRegistrantAttributes_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateDisplayedRegistrantFilters();
        }

        /// <summary>
        /// Updates the displayed registrant filters.
        /// </summary>
        private void UpdateDisplayedRegistrantFilters()
        {
            var registrantAttributeFields = this.GetRegistrantFormFields().Where( a => a.FieldSource == RegistrationFieldSource.RegistrantAttribute ).ToList();
            var displayedRegistrantAttributes = cblDisplayedRegistrantAttributes.SelectedValuesAsInt;
            foreach ( var field in registrantAttributeFields )
            {
                var attribute = field.Attribute;
                if ( attribute.FieldType.Field.HasFilterControl() )
                {
                    var filterControl = phRegistrantFilters.FindControl( "filterRegistrants_" + attribute.Id.ToString() );
                    var filterControlWrapper = phRegistrantFilters.FindControl( "filterRegistrants_" + attribute.Id.ToString() + "_wrapper" );
                    if ( filterControl != null && filterControlWrapper != null )
                    {
                        filterControl.Visible = displayedRegistrantAttributes.Contains( attribute.Id );
                        if ( filterControlWrapper != null )
                        {
                            filterControlWrapper.Visible = displayedRegistrantAttributes.Contains( attribute.Id );
                        }
                    }
                }
            }

            // hide the registrant filters section if there aren't any visible
            rcwRegistrantFilters.Visible = registrantAttributeFields.Where( a => displayedRegistrantAttributes.Contains( a.AttributeId ) ).Any();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlFeeName control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlFeeName_SelectedIndexChanged( object sender, EventArgs e )
        {
            Populate_cblFeeOptions();
            if ( cblFeeOptions.Items.Count > 1 )
            {
                cblFeeOptions.Visible = true;
            }
            else
            {
                cblFeeOptions.Visible = false;
            }
        }

        /// <summary>
        /// Populates cblFeeOptions with fee options.
        /// </summary>
        private void Populate_cblFeeOptions()
        {
            cblFeeOptions.Items.Clear();

            int? feeId = ddlFeeName.SelectedValue.AsIntegerOrNull();
            if ( feeId.HasValue )
            {
                var feeItems = new RegistrationTemplateFeeItemService( new RockContext() ).Queryable().Where( a => a.RegistrationTemplateFeeId == feeId );

                foreach ( var feeItem in feeItems )
                {
                    cblFeeOptions.Items.Add( new ListItem( feeItem.Name, feeItem.Id.ToString() ) );
                }

                cblFeeOptions.Visible = true;
            }
        }

        #endregion Registrant Filter

        #region Group Filter

        /// <summary>
        /// Adds the group attribute filters.
        /// </summary>
        public void AddGroupAttributeFilters( bool setValues )
        {
            var groupTypeId = PlacementGroupTypeId;
            var fakeGroup = new Rock.Model.Group { GroupTypeId = groupTypeId };
            fakeGroup.LoadAttributes();
            var groupAttributeList = fakeGroup.Attributes.Select( a => a.Value ).ToList();

            phGroupFilters.Controls.Clear();

            var preferences = GetBlockPersonPreferences();
            Dictionary<int, string> attributeFilters = preferences.GetValue( string.Format( UserPreferenceKey.GroupAttributeFilter_GroupTypeId, groupTypeId ) ).FromJsonOrNull<Dictionary<int, string>>();

            foreach ( var attribute in groupAttributeList )
            {
                // Add dynamic filter fields
                var filterFieldControl = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filterGroups_" + attribute.Id.ToString(), false, Rock.Reporting.FilterMode.SimpleFilter );

                if ( setValues && attributeFilters != null )
                {
                    var attributeFilterValue = attributeFilters.GetValueOrNull( attribute.Id );
                    if ( attributeFilterValue.IsNotNullOrWhiteSpace() )
                    {
                        var filterValues = attributeFilterValue.FromJsonOrNull<List<string>>();
                        attribute.FieldType.Field.SetFilterValues( filterFieldControl, attribute.QualifierValues, filterValues );
                    }
                }

                if ( filterFieldControl != null )
                {
                    if ( filterFieldControl is IRockControl )
                    {
                        var rockControl = ( IRockControl ) filterFieldControl;
                        rockControl.Label = attribute.Name;
                        rockControl.Help = attribute.Description;
                        phGroupFilters.Controls.Add( filterFieldControl );
                    }
                    else
                    {
                        var wrapper = new RockControlWrapper();
                        wrapper.ID = filterFieldControl.ID + "_wrapper";
                        wrapper.Label = attribute.Name;
                        wrapper.Controls.Add( filterFieldControl );
                        phGroupFilters.Controls.Add( wrapper );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblDisplayedGroupAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblDisplayedGroupAttributes_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateDisplayedGroupFilters();
        }

        /// <summary>
        /// Updates the displayed group filters.
        /// </summary>
        private void UpdateDisplayedGroupFilters()
        {
            var groupTypeId = hfRegistrationTemplatePlacementGroupTypeId.Value.AsInteger();
            var fakeGroup = new Rock.Model.Group { GroupTypeId = groupTypeId };
            fakeGroup.LoadAttributes();
            var groupAttributeList = fakeGroup.Attributes.Select( a => a.Value ).ToList();
            var displayedGroupAttributes = cblDisplayedGroupAttributes.SelectedValuesAsInt;
            foreach ( var attribute in groupAttributeList )
            {
                if ( attribute.FieldType.Field.HasFilterControl() )
                {
                    var filterControl = phGroupFilters.FindControl( "filterGroups_" + attribute.Id.ToString() );
                    var filterControlWrapper = phGroupFilters.FindControl( "filterGroups_" + attribute.Id.ToString() + "_wrapper" );
                    if ( filterControl != null && filterControlWrapper != null )
                    {
                        filterControl.Visible = displayedGroupAttributes.Contains( attribute.Id );
                        if ( filterControlWrapper != null )
                        {
                            filterControlWrapper.Visible = displayedGroupAttributes.Contains( attribute.Id );
                        }
                    }
                }
            }

            // hide the registrant filters section if there aren't any visible
            rcwGroupFilters.Visible = groupAttributeList.Where( a => displayedGroupAttributes.Contains( a.Id ) ).Any();
        }

        /// <summary>
        /// Saves the group attribute filters.
        /// </summary>
        public void SaveGroupAttributeFilters()
        {
            var groupTypeId = hfRegistrationTemplatePlacementGroupTypeId.Value.AsInteger();
            var fakeGroup = new Rock.Model.Group { GroupTypeId = groupTypeId };
            fakeGroup.LoadAttributes();
            var groupAttributeList = fakeGroup.Attributes.Select( a => a.Value ).ToList();
            Dictionary<int, string> attributeFilters = new Dictionary<int, string>();
            foreach ( var attribute in groupAttributeList )
            {
                var filterControl = phGroupFilters.FindControl( "filterGroups_" + attribute.Id.ToString() );

                if ( filterControl != null && filterControl.Visible )
                {
                    try
                    {
                        var values = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                        attributeFilters.Add( attribute.Id, attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson() );
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( string.Format( UserPreferenceKey.GroupAttributeFilter_GroupTypeId, groupTypeId ), attributeFilters.ToJson() );
            preferences.Save();
        }

        #endregion Group Filter

        #region Group Member Filter

        /// <summary>
        /// Adds the group member attribute filters.
        /// </summary>
        public void AddGroupMemberAttributeFilters( bool setValues )
        {
            var groupTypeId = PlacementGroupTypeId;
            var fakeGroup = new Rock.Model.Group { GroupTypeId = groupTypeId };
            fakeGroup.LoadAttributes();
            var fakeGroupMember = new GroupMember() { Group = fakeGroup, GroupId = fakeGroup.Id };
            fakeGroupMember.LoadAttributes();
            var groupMemberAttributeList = fakeGroupMember.Attributes.Select( a => a.Value ).ToList();

            phGroupMemberFilters.Controls.Clear();

            var preferences = GetBlockPersonPreferences();
            Dictionary<int, string> attributeFilters = preferences.GetValue( string.Format( UserPreferenceKey.GroupMemberAttributeFilter_GroupTypeId, groupTypeId ) ).FromJsonOrNull<Dictionary<int, string>>();

            foreach ( var attribute in groupMemberAttributeList )
            {
                // Add dynamic filter fields
                var filterFieldControl = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filterGroupMembers_" + attribute.Id.ToString(), false, Rock.Reporting.FilterMode.SimpleFilter );

                if ( setValues && attributeFilters != null )
                {
                    var attributeFilterValue = attributeFilters.GetValueOrNull( attribute.Id );
                    if ( attributeFilterValue.IsNotNullOrWhiteSpace() )
                    {
                        var filterValues = attributeFilterValue.FromJsonOrNull<List<string>>();
                        attribute.FieldType.Field.SetFilterValues( filterFieldControl, attribute.QualifierValues, filterValues );
                    }
                }

                if ( filterFieldControl != null )
                {
                    if ( filterFieldControl is IRockControl )
                    {
                        var rockControl = ( IRockControl ) filterFieldControl;
                        rockControl.Label = attribute.Name;
                        rockControl.Help = attribute.Description;
                        phGroupMemberFilters.Controls.Add( filterFieldControl );
                    }
                    else
                    {
                        var wrapper = new RockControlWrapper();
                        wrapper.ID = filterFieldControl.ID + "_wrapper";
                        wrapper.Label = attribute.Name;
                        wrapper.Controls.Add( filterFieldControl );
                        phGroupMemberFilters.Controls.Add( wrapper );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblDisplayedGroupMemberAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblDisplayedGroupMemberAttributes_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateDisplayedGroupMemberFilters();
        }

        /// <summary>
        /// Updates the displayed group member filters.
        /// </summary>
        private void UpdateDisplayedGroupMemberFilters()
        {
            var groupTypeId = PlacementGroupTypeId;
            var fakeGroup = new Rock.Model.Group { GroupTypeId = groupTypeId };
            fakeGroup.LoadAttributes();
            var fakeGroupMember = new GroupMember() { Group = fakeGroup, GroupId = fakeGroup.Id };
            fakeGroupMember.LoadAttributes();
            var groupMemberAttributeList = fakeGroupMember.Attributes.Select( a => a.Value ).ToList();
            var displayedGroupMemberAttributes = cblDisplayedGroupMemberAttributes.SelectedValuesAsInt;
            foreach ( var attribute in groupMemberAttributeList )
            {
                if ( attribute.FieldType.Field.HasFilterControl() )
                {
                    var filterControl = phGroupMemberFilters.FindControl( "filterGroupMembers_" + attribute.Id.ToString() );
                    var filterControlWrapper = phGroupMemberFilters.FindControl( "filterGroupMembers_" + attribute.Id.ToString() + "_wrapper" );
                    if ( filterControl != null && filterControlWrapper != null )
                    {
                        filterControl.Visible = displayedGroupMemberAttributes.Contains( attribute.Id );
                        if ( filterControlWrapper != null )
                        {
                            filterControlWrapper.Visible = displayedGroupMemberAttributes.Contains( attribute.Id );
                        }
                    }
                }
            }

            // hide the registrant filters section if there aren't any visible
            rcwGroupMemberFilters.Visible = groupMemberAttributeList.Where( a => displayedGroupMemberAttributes.Contains( a.Id ) ).Any();
        }

        /// <summary>
        /// Saves the group member attribute filters.
        /// </summary>
        public void SaveGroupMemberAttributeFilters()
        {
            var groupTypeId = PlacementGroupTypeId;
            var fakeGroup = new Rock.Model.Group { GroupTypeId = groupTypeId };
            fakeGroup.LoadAttributes();
            var fakeGroupMember = new GroupMember() { Group = fakeGroup, GroupId = fakeGroup.Id };
            fakeGroupMember.LoadAttributes();
            var groupMemberAttributeList = fakeGroupMember.Attributes.Select( a => a.Value ).ToList();
            Dictionary<int, string> attributeFilters = new Dictionary<int, string>();
            foreach ( var attribute in groupMemberAttributeList )
            {
                var filterControl = phGroupMemberFilters.FindControl( "filterGroupMembers_" + attribute.Id.ToString() );

                if ( filterControl != null && filterControl.Visible )
                {
                    try
                    {
                        var values = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                        attributeFilters.Add( attribute.Id, attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson() );
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( string.Format( UserPreferenceKey.GroupMemberAttributeFilter_GroupTypeId, groupTypeId ), attributeFilters.ToJson() );
            preferences.Save();
        }

        #endregion Group Member Filter

    }
}
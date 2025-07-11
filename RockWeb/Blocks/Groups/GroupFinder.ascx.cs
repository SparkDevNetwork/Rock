﻿// <copyright>
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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Field.Types;
using Rock.Lava;
using Rock.Model;
using Rock.Reporting;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Groups
{
    /// <summary>
    /// Block for people to find a group that matches their search parameters.
    /// </summary>
    [DisplayName( "Group Finder" )]
    [Category( "Groups" )]
    [Description( "Block for people to find a group that matches their search parameters." )]
    [ContextAware( typeof( Campus ) )]

    #region Block Attributes
    // Linked Pages
    [LinkedPage( "Group Detail Page",
        Description = "The page to navigate to for group details.",
        IsRequired = false,
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.GroupDetailPage )]
    [LinkedPage( "Register Page",
        Description = "The page to navigate to when registering for a group.",
        IsRequired = false,
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.RegisterPage )]

    // Filter Settings
    [GroupTypeField( "Group Type",
        IsRequired = true,
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.GroupType )]
    [GroupTypeField( "Geofenced Group Type",
        IsRequired = false,
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.GeofencedGroupType )]

    [TextField( "CampusLabel",
        IsRequired = true,
        DefaultValue = "Campuses",
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.CampusLabel )]
    [DefinedValueField(
        "Campus Types",
        Key = AttributeKey.CampusTypes,
        Description = "Allows selecting which campus types to filter campuses by.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        AllowMultiple = true )]
    [DefinedValueField(
        "Campus Statuses",
        Key = AttributeKey.CampusStatuses,
        Description = "This allows selecting which campus statuses to filter campuses by.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        AllowMultiple = true )]

    [TextField( "TimeOfDayLabel",
        IsRequired = true,
        DefaultValue = "Time of Day",
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.TimeOfDayLabel )]
    [TextField( "DayOfWeekLabel",
        IsRequired = true,
        DefaultValue = "Day of Week",
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.DayOfWeekLabel )]
    [TextField( "ScheduleFilters",
        IsRequired = false,
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.ScheduleFilters )]
    [BooleanField( "Display Campus Filter",
        IsRequired = false,
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.DisplayCampusFilter )]
    [BooleanField( "Enable Campus Context",
        IsRequired = false,
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.EnableCampusContext )]
    [BooleanField( "Hide Overcapacity Groups",
        Description = "When set to true, groups that are at capacity or whose default GroupTypeRole are at capacity are hidden.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.HideOvercapacityGroups )]
    [AttributeField( "Attribute Filters",
        EntityTypeGuid = Rock.SystemGuid.EntityType.GROUP,
        IsRequired = false,
        AllowMultiple = true,
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.AttributeFilters )]

    // Map Settings
    [BooleanField( "Show Map",
        DefaultBooleanValue = false,
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.ShowMap )]
    [DefinedValueField( "Map Style",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.MAP_STYLES,
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.MAP_STYLE_GOOGLE,
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.MapStyle )]
    [IntegerField( "Map Height",
        IsRequired = false,
        DefaultIntegerValue = 600,
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.MapHeight )]
    [BooleanField( "Show Fence",
        DefaultBooleanValue = false,
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.ShowFence )]
    [ValueListField( "Polygon Colors",
        IsRequired = false,
        DefaultValue = "#f37833|#446f7a|#afd074|#649dac|#f8eba2|#92d0df|#eaf7fc",
        ValuePrompt = "#ffffff",
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.PolygonColors
        )]
    [CodeEditorField( "Map Info",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = AttributeDefaultLava.MapInfo,
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.MapInfo )]

    // Lava Output Settings
    [BooleanField( "Show Lava Output",
        DefaultBooleanValue = false,
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.ShowLavaOutput )]
    [CodeEditorField( "Lava Output",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "",
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.LavaOutput )]

    // Grid Settings
    [BooleanField( "Show Grid",
        DefaultBooleanValue = false,
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.ShowGrid )]
    [BooleanField( "Show Schedule",
        DefaultBooleanValue = false,
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.ShowSchedule )]
    [BooleanField( "Show Proximity",
        DefaultBooleanValue = false,
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.ShowProximity )]
    [BooleanField( "Show Campus",
        DefaultBooleanValue = false,
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.ShowCampus )]
    [BooleanField( "Show Count",
        DefaultBooleanValue = false,
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.ShowCount )]
    [BooleanField( "Show Age",
        DefaultBooleanValue = false,
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.ShowAge )]
    [BooleanField( "Show Description",
        DefaultBooleanValue = true,
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.ShowDescription )]
    [AttributeField( "Attribute Columns",
        Category = AttributeCategory.CustomSetting,
        EntityTypeGuid = Rock.SystemGuid.EntityType.GROUP,
        IsRequired = false,
        AllowMultiple = true,
        Key = AttributeKey.AttributeColumns )]
    [BooleanField( "Sort By Distance",
        DefaultBooleanValue = false,
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.SortByDistance )]
    [TextField( "Page Sizes",
        Description = "To show a dropdown of page sizes, enter a comma delimited list of page sizes. For example: 10,20 will present a drop down with 10,20,All as options with the default as 10",
        IsRequired = false,
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.PageSizes )]
    [BooleanField( "Include Pending",
        DefaultBooleanValue = true,
        Category = AttributeCategory.CustomSetting,
        Key = AttributeKey.IncludePending )]
    [BooleanField( "Load Initial Results",
        Key = AttributeKey.LoadInitialResults,
        Category = AttributeCategory.CustomSetting )]
    [TextField( "Group Type Locations",
        Key = AttributeKey.GroupTypeLocations,
        Category = AttributeCategory.CustomSetting )]
    [IntegerField( "Maximum Zoom Level",
        Key = AttributeKey.MaximumZoomLevel,
        Category = AttributeCategory.CustomSetting )]
    [IntegerField( "Minimum Zoom Level",
        Key = AttributeKey.MinimumZoomLevel,
        Category = AttributeCategory.CustomSetting )]
    [IntegerField( "Initial Zoom Level",
        Key = AttributeKey.InitialZoomLevel,
        Category = AttributeCategory.CustomSetting )]
    [IntegerField( "Marker Zoom Level",
        Key = AttributeKey.MarkerZoomLevel,
        Category = AttributeCategory.CustomSetting )]
    [IntegerField( "Marker Zoom Amount",
        DefaultIntegerValue = 1,
        Key = AttributeKey.MarkerZoomAmount,
        Category = AttributeCategory.CustomSetting )]
    [TextField( "Location Precision Level",
        DefaultValue = "Precise",
        Key = AttributeKey.LocationPrecisionLevel,
        Category = AttributeCategory.CustomSetting )]
    [TextField( "Map Marker",
        Key = AttributeKey.MapMarker,
        Category = AttributeCategory.CustomSetting )]
    [TextField( "Marker Color",
        Key = AttributeKey.MarkerColor,
        Category = AttributeCategory.CustomSetting )]
    #endregion

    [Rock.SystemGuid.BlockTypeGuid( "9F8F2D68-DEEA-4686-810F-AB32923F855E" )]
    public partial class GroupFinder : RockBlockCustomSettings
    {
        private static class AttributeKey
        {
            public const string GroupDetailPage = "GroupDetailPage";
            public const string RegisterPage = "RegisterPage";
            public const string GroupType = "GroupType";
            public const string GeofencedGroupType = "GeofencedGroupType";
            public const string CampusLabel = "CampusLabel";
            public const string CampusTypes = "CampusTypes";
            public const string CampusStatuses = "CampusStatuses";
            public const string TimeOfDayLabel = "TimeOfDayLabel";
            public const string DayOfWeekLabel = "DayOfWeekLabel";
            public const string ScheduleFilters = "ScheduleFilters";
            public const string DisplayCampusFilter = "DisplayCampusFilter";
            public const string EnableCampusContext = "EnableCampusContext";
            public const string HideOvercapacityGroups = "HideOvercapacityGroups";
            public const string AttributeFilters = "AttributeFilters";
            public const string ShowMap = "ShowMap";
            public const string MapStyle = "MapStyle";
            public const string MapHeight = "MapHeight";
            public const string ShowFence = "ShowFence";
            public const string PolygonColors = "PolygonColors";
            public const string MapInfo = "MapInfo";
            public const string ShowLavaOutput = "ShowLavaOutput";
            public const string LavaOutput = "LavaOutput";
            public const string ShowGrid = "ShowGrid";
            public const string ShowSchedule = "ShowSchedule";
            public const string ShowProximity = "ShowProximity";
            public const string ShowCampus = "ShowCampus";
            public const string ShowCount = "ShowCount";
            public const string ShowAge = "ShowAge";
            public const string ShowDescription = "ShowDescription";
            public const string AttributeColumns = "AttributeColumns";
            public const string SortByDistance = "SortByDistance";
            public const string PageSizes = "PageSizes";
            public const string IncludePending = "IncludePending";
            public const string LoadInitialResults = "LoadInitialResults";
            public const string GroupTypeLocations = "GroupTypeLocations";
            public const string MaximumZoomLevel = "MaximumZoomLevel";
            public const string MinimumZoomLevel = "MinimumZoomLevel";
            public const string InitialZoomLevel = "InitialZoomLevel";
            public const string MarkerZoomLevel = "MarkerZoomLevel";
            public const string MarkerZoomAmount = "MarkerZoomAmount";
            public const string LocationPrecisionLevel = "LocationPrecisionLevel";
            public const string MapMarker = "MapMarker";
            public const string MarkerColor = "MarkerColor";
        }

        private static class AttributeDefaultLava
        {
            public const string MapInfo = @"
<h4 class='margin-t-none'>{{ Group.Name }}</h4> 

<div class='margin-b-sm'>
{% for attribute in Group.AttributeValues %}
    <strong>{{ attribute.AttributeName }}:</strong> {{ attribute.ValueFormatted }} <br />
{% endfor %}
</div>

<div class='margin-v-sm'>
{% if Location.FormattedHtmlAddress and Location.FormattedHtmlAddress != '' %}
	{{ Location.FormattedHtmlAddress }}
{% endif %}
</div>

{% if LinkedPages.GroupDetailPage and LinkedPages.GroupDetailPage != '' %}
    <a class='btn btn-xs btn-action margin-r-sm' href='{{ LinkedPages.GroupDetailPage }}?GroupId={{ Group.Id }}'>View {{ Group.GroupType.GroupTerm }}</a>
{% endif %}

{% if LinkedPages.RegisterPage and LinkedPages.RegisterPage != '' %}
    {% if LinkedPages.RegisterPage contains '?' %}
        <a class='btn btn-xs btn-action' href='{{ LinkedPages.RegisterPage }}&GroupGuid={{ Group.Guid }}'>Register</a>
    {% else %}
        <a class='btn btn-xs btn-action' href='{{ LinkedPages.RegisterPage }}?GroupGuid={{ Group.Guid }}'>Register</a>
    {% endif %}
{% endif %}
";
        }

        private static class AttributeCategory
        {
            public const string CustomSetting = "CustomSetting";
        }

        private static class ViewStateKey
        {
            public const string AttributeFilters = "AttributeFilters";
            public const string AttributeColumns = "AttributeColumns";
            public const string GroupTypeLocations = "GroupTypeLocations";
        }

        #region Private Variables
        private Guid _targetPersonGuid = Guid.Empty;
        private Dictionary<string, string> _urlParms = new Dictionary<string, string>();
        #endregion

        #region Properties

        /// <summary>
        /// Gets the settings tool tip.
        /// </summary>
        /// <value>
        /// The settings tool tip.
        /// </value>
        public override string SettingsToolTip
        {
            get
            {
                return "Edit Settings";
            }
        }

        /// <summary>
        /// Gets or sets the attribute filters.
        /// </summary>
        /// <value>
        /// The attribute filters.
        /// </value>
        public List<AttributeCache> AttributeFilters { get; set; }

        /// <summary>
        /// Gets or sets the _ attribute columns.
        /// </summary>
        /// <value>
        /// The _ attribute columns.
        /// </value>
        public List<AttributeCache> AttributeColumns { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            AttributeFilters = ViewState[ViewStateKey.AttributeFilters] as List<AttributeCache>;
            AttributeColumns = ViewState[ViewStateKey.AttributeColumns] as List<AttributeCache>;
            GroupTypeLocations = ViewState[ViewStateKey.GroupTypeLocations] as Dictionary<int, int>;
            if ( GroupTypeLocations == null )
            {
                GroupTypeLocations = new Dictionary<int, int>();
            }

            BuildDynamicControls();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            GroupTypeLocations = GetAttributeValue( AttributeKey.GroupTypeLocations ).FromJsonOrNull<Dictionary<int, int>>();
            var mapMarkerDefinedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.MAP_MARKERS.AsGuid() );
            ddlMapMarker.DefinedTypeId = mapMarkerDefinedType.Id;

            dvpCampusTypes.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.CAMPUS_TYPE ) ).Id;
            dvpCampusStatuses.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.CAMPUS_STATUS ) ).Id;

            gGroups.DataKeyNames = new string[] { "Id" };
            gGroups.Actions.ShowAdd = false;
            gGroups.GridRebind += gGroups_GridRebind;
            gGroups.ShowActionRow = false;
            gGroups.AllowPaging = false;

            this.BlockUpdated += Block_Updated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            this.LoadGoogleMapsApi();

            if ( GroupTypeLocations == null )
            {
                GroupTypeLocations = new Dictionary<int, int>();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            nbNotice.Visible = false;

            if ( Request["PersonGuid"] != null )
            {
                Guid.TryParse( Request["PersonGuid"].ToString(), out _targetPersonGuid );
                _urlParms.Add( "PersonGuid", _targetPersonGuid.ToString() );
            }

            if ( !Page.IsPostBack )
            {
                BindAttributes();
                BuildDynamicControls();

                if ( GetAttributeValue( AttributeKey.EnableCampusContext ).AsBoolean() && GetAttributeValue( AttributeKey.DisplayCampusFilter ).AsBoolean() )
                {
                    // Default campus filter selection to campus context (user can override filter).
                    var campusEntityType = EntityTypeCache.Get( "Rock.Model.Campus" );
                    var contextCampus = RockPage.GetCurrentContext( campusEntityType ) as Campus;

                    if ( contextCampus != null )
                    {
                        cblCampus.SetValue( contextCampus.Id.ToString() );
                    }
                }

                if ( _targetPersonGuid != Guid.Empty )
                {
                    ShowViewForPerson( _targetPersonGuid );
                }
                else
                {
                    ShowView();
                }
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState[ViewStateKey.AttributeFilters] = AttributeFilters;
            ViewState[ViewStateKey.AttributeColumns] = AttributeColumns;
            ViewState[ViewStateKey.GroupTypeLocations] = GroupTypeLocations;

            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the ContentDynamic control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_Updated( object sender, EventArgs e )
        {
            ShowView();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the gtpGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gtpGroupType_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetGroupTypeOptions();
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            SetAttributeValue( AttributeKey.GroupType, gtpGroupType.SelectedValuesAsInt.ToJson() );
            SetAttributeValue( AttributeKey.GeofencedGroupType, gtpGeofenceGroupType.SelectedGroupTypeId.ToString() );

            SetAttributeValue( AttributeKey.DayOfWeekLabel, tbDayOfWeekLabel.Text );
            SetAttributeValue( AttributeKey.TimeOfDayLabel, tbTimeOfDayLabel.Text );
            SetAttributeValue( AttributeKey.CampusLabel, tbCampusLabel.Text );

            var schFilters = new List<string>();
            if ( rblFilterDOW.Visible )
            {
                schFilters.Add( rblFilterDOW.SelectedValue );
                schFilters.Add( cbFilterTimeOfDay.Checked ? "Time" : string.Empty );
            }

            SetAttributeValue( AttributeKey.ScheduleFilters, schFilters.Where( f => f != string.Empty ).ToList().AsDelimited( "," ) );

            SetAttributeValue( AttributeKey.DisplayCampusFilter, cbFilterCampus.Checked.ToString() );
            SetAttributeValue( AttributeKey.EnableCampusContext, cbCampusContext.Checked.ToString() );
            SetAttributeValue( AttributeKey.HideOvercapacityGroups, cbHideOvercapacityGroups.Checked.ToString() );
            SetAttributeValue( AttributeKey.LoadInitialResults, cbLoadInitialResults.Checked.ToString() );
            SetAttributeValue( AttributeKey.GroupTypeLocations, GroupTypeLocations.ToJson() );
            SetAttributeValue( AttributeKey.MaximumZoomLevel, ddlMaxZoomLevel.SelectedValue );
            SetAttributeValue( AttributeKey.MinimumZoomLevel, ddlMinZoomLevel.SelectedValue );
            SetAttributeValue( AttributeKey.InitialZoomLevel, ddlInitialZoomLevel.SelectedValue );
            SetAttributeValue( AttributeKey.MarkerZoomLevel, ddlMarkerZoomLevel.SelectedValue );
            SetAttributeValue( AttributeKey.MarkerZoomAmount, nbMarkerAutoScaleAmount.Text );
            SetAttributeValue( AttributeKey.LocationPrecisionLevel, ddlLocationPrecisionLevel.SelectedValue );
            SetAttributeValue( AttributeKey.MapMarker, ddlMapMarker.SelectedValue );
            SetAttributeValue( AttributeKey.MarkerColor, cpMarkerColor.Text );

            SetAttributeValue( AttributeKey.AttributeFilters, cblAttributes.Items.Cast<ListItem>().Where( i => i.Selected ).Select( i => i.Value ).ToList().AsDelimited( "," ) );

            SetAttributeValue( AttributeKey.ShowMap, cbShowMap.Checked.ToString() );
            SetAttributeValue( AttributeKey.MapStyle, dvpMapStyle.SelectedValue );
            SetAttributeValue( AttributeKey.MapHeight, nbMapHeight.Text );
            SetAttributeValue( AttributeKey.ShowFence, cbShowFence.Checked.ToString() );
            SetAttributeValue( AttributeKey.PolygonColors, vlPolygonColors.Value );
            SetAttributeValue( AttributeKey.MapInfo, ceMapInfo.Text );

            SetAttributeValue( AttributeKey.ShowLavaOutput, cbShowLavaOutput.Checked.ToString() );
            SetAttributeValue( AttributeKey.LavaOutput, ceLavaOutput.Text );

            SetAttributeValue( AttributeKey.ShowGrid, cbShowGrid.Checked.ToString() );
            SetAttributeValue( AttributeKey.ShowSchedule, cbShowSchedule.Checked.ToString() );
            SetAttributeValue( AttributeKey.ShowDescription, cbShowDescription.Checked.ToString() );
            SetAttributeValue( AttributeKey.ShowCampus, cbShowCampus.Checked.ToString() );

            // Convert the select campus type defined value ids to defined value guids to make them compatible with the Defined Values field type
            var selectedCampusTypes = dvpCampusTypes.SelectedValuesAsInt
                  .Select( a => DefinedValueCache.Get( a ) )
                  .Where( a => a != null )
                  .Select( a => a.Guid )
                  .ToList()
                  .AsDelimited( "," );

            SetAttributeValue( AttributeKey.CampusTypes, selectedCampusTypes );

            var selectedCampusStatuses = dvpCampusStatuses.SelectedValuesAsInt
                  .Select( a => DefinedValueCache.Get( a ) )
                  .Where( a => a != null )
                  .Select( a => a.Guid )
                  .ToList()
                  .AsDelimited( "," );

            SetAttributeValue( AttributeKey.CampusStatuses, selectedCampusStatuses );

            SetAttributeValue( AttributeKey.ShowProximity, cbProximity.Checked.ToString() );
            SetAttributeValue( AttributeKey.SortByDistance, cbSortByDistance.Checked.ToString() );
            SetAttributeValue( AttributeKey.PageSizes, tbPageSizes.Text );
            SetAttributeValue( AttributeKey.ShowCount, cbShowCount.Checked.ToString() );
            SetAttributeValue( AttributeKey.ShowAge, cbShowAge.Checked.ToString() );
            SetAttributeValue( AttributeKey.AttributeColumns, cblGridAttributes.Items.Cast<ListItem>().Where( i => i.Selected ).Select( i => i.Value ).ToList().AsDelimited( "," ) );
            SetAttributeValue( AttributeKey.IncludePending, cbIncludePending.Checked.ToString() );

            var ppFieldType = new PageReferenceFieldType();
            SetAttributeValue( AttributeKey.GroupDetailPage, ppFieldType.GetEditValue( ppGroupDetailPage, null ) );
            SetAttributeValue( AttributeKey.RegisterPage, ppFieldType.GetEditValue( ppRegisterPage, null ) );

            SaveAttributeValues();

            mdEdit.Hide();
            pnlEditModal.Visible = false;
            upnlContent.Update();

            BindAttributes();
            BuildDynamicControls();
            ShowView();
        }

        /// <summary>
        /// Handles the Click event of the btnSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSearch_Click( object sender, EventArgs e )
        {
            ShowResults();
        }

        /// <summary>
        /// Handles the Click event of the btnClear control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnClear_Click( object sender, EventArgs e )
        {
            acAddress.SetValues( null );
            BuildDynamicControls();
            ShowView();
        }

        /// <summary>
        /// Handles the RowSelected event of the gGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gGroups_RowSelected( object sender, RowEventArgs e )
        {
            if ( !NavigateToLinkedPage( AttributeKey.GroupDetailPage, "GroupId", e.RowKeyId ) )
            {
                ShowResults();
                ScriptManager.RegisterStartupScript( pnlMap, pnlMap.GetType(), "group-finder-row-selected", "openInfoWindowById(" + e.RowKeyId + ");", true );
            }
        }

        /// <summary>
        /// Handles the Click event of the registerColumn control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void registerColumn_Click( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var group = new GroupService( rockContext ).Get( e.RowKeyId );
                if ( group != null )
                {
                    _urlParms.Add( "GroupGuid", group.Guid.ToString() );
                    if ( !NavigateToLinkedPage( AttributeKey.RegisterPage, _urlParms ) )
                    {
                        ShowResults();
                    }
                }
                else
                {
                    ShowResults();
                }
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gGroups_GridRebind( object sender, EventArgs e )
        {
            ShowResults();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            pnlEditModal.Visible = true;
            upnlContent.Update();
            mdEdit.Show();

            BindGroupTypeLocationGrid();

            var rockContext = new RockContext();
            var groupTypes = new GroupTypeService( rockContext )
                .Queryable().AsNoTracking().OrderBy( t => t.Order ).ToList();

            BindGroupType( gtpGroupType, groupTypes );
            BindGroupType( gtpGeofenceGroupType, groupTypes, AttributeKey.GeofencedGroupType );
            tbCampusLabel.Text = GetAttributeValue( AttributeKey.CampusLabel );
            tbDayOfWeekLabel.Text = GetAttributeValue( AttributeKey.DayOfWeekLabel );
            tbTimeOfDayLabel.Text = GetAttributeValue( AttributeKey.TimeOfDayLabel );

            var scheduleFilters = GetAttributeValue( AttributeKey.ScheduleFilters ).SplitDelimitedValues( false ).ToList();
            if ( scheduleFilters.Contains( "Day" ) )
            {
                rblFilterDOW.SetValue( "Day" );
            }
            else if ( scheduleFilters.Contains( "Days" ) )
            {
                rblFilterDOW.SetValue( "Days" );
            }
            else
            {
                rblFilterDOW.SelectedIndex = 0;
            }

            cbFilterTimeOfDay.Checked = scheduleFilters.Contains( "Time" );

            SetGroupTypeOptions();
            foreach ( string attr in GetAttributeValue( AttributeKey.AttributeFilters ).SplitDelimitedValues() )
            {
                var li = cblAttributes.Items.FindByValue( attr );
                if ( li != null )
                {
                    li.Selected = true;
                }
            }

            ddlMaxZoomLevel.SelectedValue = GetAttributeValue( AttributeKey.MaximumZoomLevel );
            ddlMinZoomLevel.SelectedValue = GetAttributeValue( AttributeKey.MinimumZoomLevel );
            ddlInitialZoomLevel.SelectedValue = GetAttributeValue( AttributeKey.InitialZoomLevel );
            ddlMarkerZoomLevel.SelectedValue = GetAttributeValue( AttributeKey.MarkerZoomLevel );
            nbMarkerAutoScaleAmount.Text = GetAttributeValue( AttributeKey.MarkerZoomAmount );
            ddlLocationPrecisionLevel.SelectedValue = GetAttributeValue( AttributeKey.LocationPrecisionLevel );
            ddlMapMarker.SetValue( GetAttributeValue( AttributeKey.MapMarker ) );
            cpMarkerColor.Text = GetAttributeValue( AttributeKey.MarkerColor );

            cbFilterCampus.Checked = GetAttributeValue( AttributeKey.DisplayCampusFilter ).AsBoolean();
            cbCampusContext.Checked = GetAttributeValue( AttributeKey.EnableCampusContext ).AsBoolean();
            cbHideOvercapacityGroups.Checked = GetAttributeValue( AttributeKey.HideOvercapacityGroups ).AsBoolean();
            cbLoadInitialResults.Checked = GetAttributeValue( AttributeKey.LoadInitialResults ).AsBoolean();

            cbShowMap.Checked = GetAttributeValue( AttributeKey.ShowMap ).AsBoolean();
            dvpMapStyle.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.MAP_STYLES.AsGuid() ).Id;
            dvpMapStyle.SetValue( GetAttributeValue( AttributeKey.MapStyle ) );
            nbMapHeight.Text = GetAttributeValue( AttributeKey.MapHeight );
            cbShowFence.Checked = GetAttributeValue( AttributeKey.ShowFence ).AsBoolean();
            vlPolygonColors.Value = GetAttributeValue( AttributeKey.PolygonColors );
            ceMapInfo.Text = GetAttributeValue( AttributeKey.MapInfo );

            cbShowLavaOutput.Checked = GetAttributeValue( AttributeKey.ShowLavaOutput ).AsBoolean();
            ceLavaOutput.Text = GetAttributeValue( AttributeKey.LavaOutput );

            cbShowGrid.Checked = GetAttributeValue( AttributeKey.ShowGrid ).AsBoolean();
            cbShowSchedule.Checked = GetAttributeValue( AttributeKey.ShowSchedule ).AsBoolean();
            cbShowDescription.Checked = GetAttributeValue( AttributeKey.ShowDescription ).AsBoolean();
            cbShowCampus.Checked = GetAttributeValue( AttributeKey.ShowCampus ).AsBoolean();

            // Convert the defined value guids to ints as required by the control
            var selectedCampusTypes = GetAttributeValue( AttributeKey.CampusTypes )
                        .Split( ',' )
                        .AsGuidList()
                        .Select( a => DefinedValueCache.Get( a ) )
                        .Select( a => a.Id )
                        .ToList();

            dvpCampusTypes.SetValues( selectedCampusTypes );

            var selectedCampusStatueses = GetAttributeValue( AttributeKey.CampusStatuses )
                        .Split( ',' )
                        .AsGuidList()
                        .Select( a => DefinedValueCache.Get( a ) )
                        .Select( a => a.Id )
                        .ToList();

            dvpCampusStatuses.SetValues( selectedCampusStatueses );

            cbProximity.Checked = GetAttributeValue( AttributeKey.ShowProximity ).AsBoolean();
            cbSortByDistance.Checked = GetAttributeValue( AttributeKey.SortByDistance ).AsBoolean();
            tbPageSizes.Text = GetAttributeValue( AttributeKey.PageSizes );
            cbShowCount.Checked = GetAttributeValue( AttributeKey.ShowCount ).AsBoolean();
            cbShowAge.Checked = GetAttributeValue( AttributeKey.ShowAge ).AsBoolean();
            foreach ( string attr in GetAttributeValue( AttributeKey.AttributeColumns ).SplitDelimitedValues() )
            {
                var li = cblGridAttributes.Items.FindByValue( attr );
                if ( li != null )
                {
                    li.Selected = true;
                }
            }

            cbIncludePending.Checked = GetAttributeValue( AttributeKey.IncludePending ).AsBoolean();

            var ppFieldType = new PageReferenceFieldType();
            ppFieldType.SetEditValue( ppGroupDetailPage, null, GetAttributeValue( AttributeKey.GroupDetailPage ) );
            ppFieldType.SetEditValue( ppRegisterPage, null, GetAttributeValue( AttributeKey.RegisterPage ) );

            upnlContent.Update();
        }

        /// <summary>
        /// Binds the group attribute list.
        /// </summary>
        private void SetGroupTypeOptions()
        {
            rblFilterDOW.Visible = false;
            cbFilterTimeOfDay.Visible = false;

            // Rebuild the checkbox list settings for both the filter and display in grid attribute lists
            cblAttributes.Items.Clear();
            cblGridAttributes.Items.Clear();

            if ( gtpGroupType.SelectedValuesAsInt != null )
            {
                var groupTypes = gtpGroupType.SelectedValuesAsInt.Select( id => GroupTypeCache.Get( id ) );

                foreach ( var groupType in groupTypes )
                {
                    if ( groupType != null )
                    {
                        bool hasWeeklyschedule = ( groupType.AllowedScheduleTypes & ScheduleType.Weekly ) == ScheduleType.Weekly;
                        rblFilterDOW.Visible = hasWeeklyschedule;
                        cbFilterTimeOfDay.Visible = hasWeeklyschedule;

                        var group = new Group();
                        group.GroupTypeId = groupType.Id;
                        group.LoadAttributes();
                        foreach ( var attribute in group.Attributes )
                        {
                            if ( attribute.Value.FieldType.Field.HasFilterControl() )
                            {
                                cblAttributes.Items.Add( new ListItem( attribute.Value.Name + string.Format( " ({0})", groupType.Name ), attribute.Value.Guid.ToString() ) );
                            }

                            cblGridAttributes.Items.Add( new ListItem( attribute.Value.Name + string.Format( " ({0})", groupType.Name ), attribute.Value.Guid.ToString() ) );
                        }
                    }
                }
            }

            // Remove location data for any group type that has it but isn't selected.
            if ( GroupTypeLocations != null )
            {
                var groupTypeIdWithLocations = GroupTypeLocations.Keys.ToList();
                foreach ( var groupTypeId in groupTypeIdWithLocations )
                {
                    if ( !gtpGroupType.SelectedValuesAsInt.Contains( groupTypeId ) )
                    {
                        GroupTypeLocations.Remove( groupTypeId );
                    }
                }
            }

            cblAttributes.Visible = cblAttributes.Items.Count > 0;
            cblGridAttributes.Visible = cblAttributes.Items.Count > 0;

            BindGroupTypeLocationGrid();
        }

        private void ShowViewForPerson( Guid targetPersonGuid )
        {
            // check for a specific person in the query string
            Person targetPerson = null;
            Location targetPersonLocation = null;

            targetPerson = new PersonService( new RockContext() ).Queryable().Where( p => p.Guid == targetPersonGuid ).FirstOrDefault();
            targetPersonLocation = targetPerson.GetHomeLocation();

            if ( targetPerson != null )
            {
                lTitle.Text = string.Format( "<h4 class='margin-t-none'>Groups for {0}</h4>", targetPerson.FullName );
                acAddress.SetValues( targetPersonLocation );
                acAddress.Visible = false;
                phFilterControls.Visible = false;
                btnSearch.Visible = false;
                btnClear.Visible = false;

                if ( targetPersonLocation != null && targetPersonLocation.GeoPoint != null )
                {
                    lTitle.Text += string.Format( "<p>Search based on: {0}</p>", targetPersonLocation.ToString() );

                    ShowResults();
                }
                else if ( targetPersonLocation != null )
                {
                    lTitle.Text += string.Format( "<p>The position of the address on file ({0}) could not be determined.</p>", targetPersonLocation.ToString() );
                }
                else
                {
                    lTitle.Text += string.Format( "<p>The person does not have an address on file.</p>" );
                }
            }
        }

        /// <summary>
        /// Shows the view.
        /// </summary>
        private void ShowView()
        {
            // If the groups should be limited by geofence, or the distance should be displayed,
            // then we need to capture the person's address
            Guid? fenceTypeGuid = GetAttributeValue( AttributeKey.GeofencedGroupType ).AsGuidOrNull();
            if ( fenceTypeGuid.HasValue || GetAttributeValue( AttributeKey.ShowProximity ).AsBoolean() )
            {
                acAddress.Visible = true;

                if ( CurrentPerson != null )
                {
                    acAddress.SetValues( CurrentPerson.GetHomeLocation() );
                }

                phFilterControls.Visible = true;
                btnSearch.Visible = true;
            }
            else
            {
                acAddress.Visible = false;

                // Check to see if there are any Schedule or attribute filters.
                string scheduleFilters = GetAttributeValue( AttributeKey.ScheduleFilters );
                bool displayCampusFilter = GetAttributeValue( AttributeKey.DisplayCampusFilter ).AsBoolean();
                bool loadInitialResults = GetAttributeValue( AttributeKey.LoadInitialResults ).AsBoolean();

                if ( !string.IsNullOrWhiteSpace( scheduleFilters ) || AttributeFilters.Any() )
                {
                    phFilterControls.Visible = true;
                    btnSearch.Visible = true;
                }
                else if ( displayCampusFilter && !loadInitialResults )
                {
                    pnlResults.Visible = loadInitialResults && displayCampusFilter;
                }
                else
                {
                    // Hide the search button and show the results immediately since there is 
                    // no filter criteria to be entered
                    phFilterControls.Visible = false;
                    btnSearch.Visible = GetAttributeValue( AttributeKey.DisplayCampusFilter ).AsBoolean();
                    pnlResults.Visible = true;
                }
            }

            if ( !pnlResults.Visible && GetAttributeValue( AttributeKey.LoadInitialResults ).AsBoolean() )
            {
                pnlResults.Visible = true;
            }

            btnClear.Visible = btnSearch.Visible;

            // If we've already displayed results, then re-display them
            if ( pnlResults.Visible )
            {
                ShowResults();
            }
        }

        /// <summary>
        /// Adds the attribute filters.
        /// </summary>
        private void BindAttributes()
        {
            // Parse the attribute filters 
            AttributeFilters = new List<AttributeCache>();
            foreach ( string attr in GetAttributeValue( AttributeKey.AttributeFilters ).SplitDelimitedValues() )
            {
                Guid? attributeGuid = attr.AsGuidOrNull();
                if ( attributeGuid.HasValue )
                {
                    var attribute = AttributeCache.Get( attributeGuid.Value );
                    if ( attribute != null && attribute.FieldType.Field.HasFilterControl() )
                    {
                        AttributeFilters.Add( attribute );
                    }
                }
            }

            // Parse the attribute filters 
            AttributeColumns = new List<AttributeCache>();
            foreach ( string attr in GetAttributeValue( AttributeKey.AttributeColumns ).SplitDelimitedValues() )
            {
                Guid? attributeGuid = attr.AsGuidOrNull();
                if ( attributeGuid.HasValue )
                {
                    var attribute = AttributeCache.Get( attributeGuid.Value );
                    if ( attribute != null )
                    {
                        AttributeColumns.Add( attribute );
                    }
                }
            }
        }

        /// <summary>
        /// Builds the dynamic controls.
        /// </summary>
        private void BuildDynamicControls()
        {
            // Clear attribute filter controls and recreate
            phFilterControls.Controls.Clear();
            var scheduleFilters = GetAttributeValue( AttributeKey.ScheduleFilters ).SplitDelimitedValues().ToList();
            if ( scheduleFilters.Contains( "Days" ) )
            {
                var dowsFilterControl = new RockCheckBoxList();
                dowsFilterControl.ID = "filter_dows";
                dowsFilterControl.Label = GetAttributeValue( AttributeKey.DayOfWeekLabel );
                dowsFilterControl.BindToEnum<DayOfWeek>();
                dowsFilterControl.RepeatDirection = RepeatDirection.Horizontal;

                AddFilterControl( dowsFilterControl, "Days of Week", "The day of week that group meets on." );
            }

            if ( scheduleFilters.Contains( "Day" ) )
            {
                var control = FieldTypeCache.Get( Rock.SystemGuid.FieldType.DAY_OF_WEEK ).Field.FilterControl( null, "filter_dow", false, Rock.Reporting.FilterMode.SimpleFilter );
                string dayOfWeekLabel = GetAttributeValue( AttributeKey.DayOfWeekLabel );
                AddFilterControl( control, dayOfWeekLabel, "The day of week that group meets on." );
            }

            if ( scheduleFilters.Contains( "Time" ) )
            {
                var control = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TIME ).Field.FilterControl( null, "filter_time", false, Rock.Reporting.FilterMode.SimpleFilter );
                string timeOfDayLabel = GetAttributeValue( AttributeKey.TimeOfDayLabel );
                AddFilterControl( control, timeOfDayLabel, "The time of day that group meets." );
            }

            if ( GetAttributeValue( AttributeKey.DisplayCampusFilter ).AsBoolean() )
            {
                var filterCampusTypeIds = GetAttributeValue( AttributeKey.CampusTypes )
                  .SplitDelimitedValues( true )
                  .AsGuidList()
                  .Select( a => DefinedValueCache.Get( a ) )
                  .Where( a => a != null )
                  .Select( a => a.Id )
                  .ToList();

                var filterCampusStatusIds = GetAttributeValue( AttributeKey.CampusStatuses )
                    .SplitDelimitedValues( true )
                    .AsGuidList()
                    .Select( a => DefinedValueCache.Get( a ) )
                    .Where( a => a != null )
                    .Select( a => a.Id )
                    .ToList();

                var campuses = CampusCache.All( includeInactive: false );

                if ( filterCampusTypeIds.Any() )
                {
                    campuses = campuses.Where( c => c.CampusTypeValueId != null && filterCampusTypeIds.Contains( c.CampusTypeValueId.Value ) ).ToList();
                }

                if ( filterCampusStatusIds.Any() )
                {
                    campuses = campuses.Where( c => c.CampusStatusValueId != null && filterCampusStatusIds.Contains( c.CampusStatusValueId.Value ) ).ToList();
                }

                cblCampus.Label = GetAttributeValue( AttributeKey.CampusLabel );
                cblCampus.Visible = true;
                cblCampus.DataSource = campuses.OrderBy( c => c.Order );

                cblCampus.DataBind();
            }
            else
            {
                cblCampus.Visible = false;
            }

            if ( AttributeFilters != null )
            {
                var existingFilters = new HashSet<string>();
                foreach ( var attribute in AttributeFilters )
                {
                    var filterId = $"filter_{attribute.Key}_{attribute.FieldType.Id}";
                    if ( existingFilters.Contains( filterId ) )
                    {
                        continue;
                    }

                    existingFilters.Add( filterId );
                    var control = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, filterId, false, Rock.Reporting.FilterMode.SimpleFilter );
                    if ( control != null )
                    {
                        AddFilterControl( control, attribute.Name, attribute.Description );
                    }
                }
            }

            // Build attribute columns
            foreach ( var column in gGroups.Columns.OfType<AttributeField>().ToList() )
            {
                gGroups.Columns.Remove( column );
            }

            if ( AttributeColumns != null )
            {
                foreach ( var attribute in AttributeColumns )
                {
                    string dataFieldExpression = attribute.Key;
                    bool columnExists = gGroups.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = dataFieldExpression;
                        boundField.AttributeId = attribute.Id;
                        boundField.HeaderText = attribute.Name;

                        var attributeCache = AttributeCache.Get( attribute.Id );
                        if ( attributeCache != null )
                        {
                            boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                        }

                        gGroups.Columns.Add( boundField );
                    }
                }
            }

            // Add Register Column
            foreach ( var column in gGroups.Columns.OfType<EditField>().ToList() )
            {
                gGroups.Columns.Remove( column );
            }

            var registerPage = new PageReference( GetAttributeValue( AttributeKey.RegisterPage ) );

            if ( _targetPersonGuid != Guid.Empty )
            {
                registerPage.Parameters = _urlParms;
            }

            if ( registerPage.PageId > 0 )
            {
                var registerColumn = new EditField();
                registerColumn.ToolTip = "Register";
                registerColumn.HeaderText = "Register";
                registerColumn.Click += registerColumn_Click;
                gGroups.Columns.Add( registerColumn );
            }

            var pageSizes = new List<int>();
            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.PageSizes ) ) )
            {
                pageSizes = GetAttributeValue( AttributeKey.PageSizes ).Split( ',' ).AsIntegerList();
            }

            ddlPageSize.Items.Clear();
            ddlPageSize.Items.AddRange( pageSizes.Select( a => new ListItem( a.ToString(), a.ToString() ) ).ToArray() );
            ddlPageSize.Items.Add( new ListItem( "All", "0" ) );

            if ( pageSizes.Any() )
            {
                // set default PageSize to whatever is first in the PageSize setting
                ddlPageSize.Visible = true;
                ddlPageSize.SelectedValue = pageSizes[0].ToString();
            }
            else
            {
                ddlPageSize.Visible = false;
            }

            // if the SortByDistance is enabled, prevent them from sorting by ColumnClick
            if ( GetAttributeValue( AttributeKey.SortByDistance ).AsBoolean() )
            {
                gGroups.AllowSorting = false;
            }
        }

        private void AddFilterControl( Control control, string name, string description )
        {
            if ( control is IRockControl )
            {
                var rockControl = ( IRockControl ) control;
                rockControl.Label = name;
                rockControl.Help = description;
                phFilterControls.Controls.Add( control );
            }
            else
            {
                var wrapper = new RockControlWrapper();
                wrapper.ID = control.ID + "_wrapper";
                wrapper.Label = name;
                wrapper.Controls.Add( control );
                phFilterControls.Controls.Add( wrapper );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void ShowResults()
        {
            // Get the group types that we're interested in
            var groupTypeIds = GetGroupTypeIds();
            if ( groupTypeIds == null )
            {
                ShowError( "A valid Group Type is required." );
                return;
            }

            gGroups.Columns[1].Visible = GetAttributeValue( AttributeKey.ShowDescription ).AsBoolean();
            gGroups.Columns[2].Visible = GetAttributeValue( AttributeKey.ShowSchedule ).AsBoolean();
            gGroups.Columns[3].Visible = GetAttributeValue( AttributeKey.ShowCount ).AsBoolean();
            gGroups.Columns[4].Visible = GetAttributeValue( AttributeKey.ShowAge ).AsBoolean();

            var includePending = GetAttributeValue( AttributeKey.IncludePending ).AsBoolean();
            bool showProximity = GetAttributeValue( AttributeKey.ShowProximity ).AsBoolean();
            gGroups.Columns[6].Visible = showProximity;  // Distance

            // Get query of groups of the selected group type
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );
            var groupQry = groupService
                .Queryable( "GroupLocations.Location" )
                .Where( g => g.IsActive
                        && groupTypeIds.Contains( g.GroupType.Id )
                        && g.IsPublic );

            var groupParameterExpression = groupService.ParameterExpression;
            var schedulePropertyExpression = Expression.Property( groupParameterExpression, "Schedule" );

            var dowsFilterControl = phFilterControls.FindControl( "filter_dows" ) as RockCheckBoxList;
            if ( dowsFilterControl != null )
            {
                var dows = new List<DayOfWeek>();
                dowsFilterControl.SelectedValuesAsInt.ForEach( i => dows.Add( ( DayOfWeek ) i ) );

                if ( dows.Any() )
                {
                    groupQry = groupQry.Where( g =>
                        g.Schedule.WeeklyDayOfWeek.HasValue &&
                        dows.Contains( g.Schedule.WeeklyDayOfWeek.Value ) );
                }
            }

            var dowFilterControl = phFilterControls.FindControl( "filter_dow" );
            if ( dowFilterControl != null )
            {
                var field = FieldTypeCache.Get( Rock.SystemGuid.FieldType.DAY_OF_WEEK ).Field;

                var filterValues = field.GetFilterValues( dowFilterControl, null, Rock.Reporting.FilterMode.SimpleFilter );
                var expression = field.PropertyFilterExpression( null, filterValues, schedulePropertyExpression, "WeeklyDayOfWeek", typeof( DayOfWeek? ) );
                groupQry = groupQry.Where( groupParameterExpression, expression, null );
            }

            var timeFilterControl = phFilterControls.FindControl( "filter_time" );
            if ( timeFilterControl != null )
            {
                var field = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TIME ).Field;

                var filterValues = field.GetFilterValues( timeFilterControl, null, Rock.Reporting.FilterMode.SimpleFilter );
                var expression = field.PropertyFilterExpression( null, filterValues, schedulePropertyExpression, "WeeklyTimeOfDay", typeof( TimeSpan? ) );
                groupQry = groupQry.Where( groupParameterExpression, expression, null );
            }

            if ( GetAttributeValue( AttributeKey.DisplayCampusFilter ).AsBoolean() )
            {
                var searchCampuses = cblCampus.SelectedValuesAsInt;
                if ( searchCampuses.Count > 0 )
                {
                    groupQry = groupQry.Where( c => searchCampuses.Contains( c.CampusId ?? -1 ) );
                }
                else if ( searchCampuses.Count == 0 && !GetAttributeValue( AttributeKey.LoadInitialResults ).AsBoolean() )
                {
                    // If the campus filter is displayed, none are chosen, and results are not displayed on load,
                    // then prompt the searcher to select a campus.
                    ShowWarning( "Please select at least one campus." );
                    pnlResults.Visible = false;
                    return;
                }
            }
            else if ( GetAttributeValue( AttributeKey.EnableCampusContext ).AsBoolean() )
            {
                // if Campus Context is enabled and the filter is not shown, we need to filter campuses directly.
                var campusEntityType = EntityTypeCache.Get( "Rock.Model.Campus" );
                var contextCampus = RockPage.GetCurrentContext( campusEntityType ) as Campus;

                if ( contextCampus != null )
                {
                    groupQry = groupQry.Where( c => c.CampusId == contextCampus.Id );
                }
            }

            // This hides the groups that are at or over capacity by doing two things:
            // 1) If the group has a GroupCapacity, check that we haven't met or exceeded that.
            // 2) When someone registers for a group on the front-end website, they automatically get added with the group's default
            //    GroupTypeRole. If that role exists and has a MaxCount, check that we haven't met or exceeded it yet.
            if ( GetAttributeValue( AttributeKey.HideOvercapacityGroups ).AsBoolean() )
            {
                groupQry = groupQry.Where(
                    g => g.GroupCapacity == null ||
                    g.Members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active ).Count() < g.GroupCapacity );

                groupQry = groupQry.Where( g =>
                     g.GroupType == null ||
                     g.GroupType.DefaultGroupRole == null ||
                     g.GroupType.DefaultGroupRole.MaxCount == null ||
                     g.Members.Where( m => m.GroupRoleId == g.GroupType.DefaultGroupRole.Id ).Count() < g.GroupType.DefaultGroupRole.MaxCount );
            }

            // Filter query by any configured attribute filters
            if ( AttributeFilters != null && AttributeFilters.Any() )
            {
                var processedAttributes = new HashSet<string>();
                /*
                    07/01/2021 - MSB

                    This section of code creates an expression for each attribute in the search. The attributes from the same
                    Group Type get grouped and &&'d together. Then the grouped Expressions will get ||'d together so that results
                    will be returned across Group Types.

                    If we don't do this, when the Admin adds attributes from two different Group Types and then the user enters data
                    for both attributes they would get no results because Attribute A from Group Type A doesn't exists in Group Type B.
                    
                    Reason: Queries across Group Types
                */
                var filters = new Dictionary<string, Expression>();
                var parameterExpression = groupService.ParameterExpression;

                foreach ( var attribute in AttributeFilters )
                {
                    var filterId = $"filter_{attribute.Key}_{attribute.FieldType.Id}";
                    var queryKey = $"{attribute.EntityTypeQualifierColumn}_{attribute.EntityTypeQualifierValue}";

                    var filterControl = phFilterControls.FindControl( filterId );

                    Expression leftExpression = null;
                    if ( filters.ContainsKey( queryKey ) )
                    {
                        leftExpression = filters[queryKey];
                    }

                    var expression = Rock.Utility.ExpressionHelper.BuildExpressionFromFieldType<Group>( attribute.FieldType.Field, filterControl, attribute, groupService, parameterExpression, FilterMode.SimpleFilter );
                    if ( expression != null )
                    {
                        if ( leftExpression == null )
                        {
                            filters[queryKey] = expression;
                        }
                        else
                        {
                            filters[queryKey] = Expression.And( leftExpression, expression );
                        }
                    }
                }

                if ( filters.Count == 1 )
                {
                    groupQry = groupQry.Where( parameterExpression, filters.FirstOrDefault().Value );
                }
                else if ( filters.Count > 1 )
                {
                    var keys = filters.Keys.ToList();
                    var expression = filters[keys[0]];

                    for ( var i = 1; i < filters.Count; i++ )
                    {
                        expression = Expression.Or( expression, filters[keys[i]] );
                    }

                    groupQry = groupQry.Where( parameterExpression, expression );
                }
            }

            List<GroupLocation> fences = null;
            List<Group> groups = groupQry.ToList();

            groups = groups.Where( g => !GroupTypeLocations.ContainsKey( g.GroupTypeId ) || g.GroupLocations.Any( gl => gl.GroupLocationTypeValueId == GroupTypeLocations[g.GroupTypeId] ) ).ToList();

            // Run query to get list of matching groups
            SortProperty sortProperty = gGroups.SortProperty;
            if ( sortProperty != null )
            {
                groups = groups.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                groups = groups.OrderBy( g => g.Name ).ToList();
            }

            gGroups.Columns[5].Visible = GetAttributeValue( AttributeKey.ShowCampus ).AsBoolean() && groups.Any( g => g.CampusId.HasValue );

            int? fenceGroupTypeId = GetGroupTypeId( GetAttributeValue( AttributeKey.GeofencedGroupType ).AsGuidOrNull() );
            bool showMap = GetAttributeValue( AttributeKey.ShowMap ).AsBoolean();
            bool showFences = showMap && GetAttributeValue( AttributeKey.ShowFence ).AsBoolean();

            var distances = new Dictionary<int, double>();

            // If we care where these groups are located...
            if ( fenceGroupTypeId.HasValue || showMap || showProximity )
            {
                // Get the location for the address entered
                Location personLocation = null;
                if ( fenceGroupTypeId.HasValue || showProximity )
                {
                    personLocation = new LocationService( rockContext )
                        .Get( acAddress.Street1, acAddress.Street2, acAddress.City, acAddress.State, acAddress.PostalCode, acAddress.Country );
                }

                // If showing a map, and person's location was found, save a mapitem for this location
                FinderMapItem personMapItem = null;
                if ( showMap && personLocation != null && personLocation.GeoPoint != null )
                {
                    var infoWindow = string.Format(
                        @"
<div style='width:250px'>
    <div class='clearfix'>
		<strong>Your Location</strong>
        <br/>{0}
    </div>
</div>
",
                        personLocation.FormattedHtmlAddress );

                    personMapItem = new FinderMapItem( personLocation );
                    personMapItem.Name = "Your Location";
                    personMapItem.InfoWindow = HttpUtility.HtmlEncode( infoWindow.Replace( Environment.NewLine, string.Empty ).Replace( "\n", string.Empty ).Replace( "\t", string.Empty ) );
                }

                // Get the locations, and optionally calculate the distance for each of the groups
                var groupLocations = new List<GroupLocation>();
                foreach ( var group in groups )
                {
                    foreach ( var groupLocation in group.GroupLocations
                        .Where( gl => gl.Location.GeoPoint != null ) )
                    {
                        groupLocations.Add( groupLocation );

                        if ( showProximity && personLocation != null && personLocation.GeoPoint != null )
                        {
                            double meters = groupLocation.Location.GeoPoint.Distance( personLocation.GeoPoint ) ?? 0.0D;
                            double miles = meters * Location.MilesPerMeter;

                            // If this group already has a distance calculated, see if this location is closer and if so, use it instead
                            if ( distances.ContainsKey( group.Id ) )
                            {
                                if ( distances[group.Id] < miles )
                                {
                                    distances[group.Id] = miles;
                                }
                            }
                            else
                            {
                                distances.Add( group.Id, miles );
                            }
                        }
                    }
                }

                // If groups should be limited by a geofence
                var fenceMapItems = new List<MapItem>();
                if ( fenceGroupTypeId.HasValue )
                {
                    fences = new List<GroupLocation>();
                    if ( personLocation != null && personLocation.GeoPoint != null )
                    {
                        fences = new GroupLocationService( rockContext )
                            .Queryable( "Group,Location" )
                            .Where( gl => gl.Group.GroupTypeId == fenceGroupTypeId
                                && gl.Location.GeoFence != null
                                && personLocation.GeoPoint.Intersects( gl.Location.GeoFence ) )
                            .ToList();
                    }

                    // Limit the group locations to only those locations inside one of the fences
                    groupLocations = groupLocations
                        .Where( gl => fences.Any( f => gl.Location.GeoPoint.Intersects( f.Location.GeoFence ) ) )
                        .ToList();

                    // Limit the groups to the those that still contain a valid location
                    groups = groups
                        .Where( g =>
                            groupLocations.Any( gl => gl.GroupId == g.Id ) )
                        .ToList();

                    // If the map and fences should be displayed, create a map item for each fence
                    if ( showMap && showFences )
                    {
                        foreach ( var fence in fences )
                        {
                            var mapItem = new FinderMapItem( fence.Location );
                            mapItem.EntityTypeId = EntityTypeCache.Get( "Rock.Model.Group" ).Id;
                            mapItem.EntityId = fence.GroupId;
                            mapItem.Name = fence.Group.Name;
                            fenceMapItems.Add( mapItem );
                        }
                    }
                }

                // if not sorting by ColumnClick and SortByDistance, then sort the groups by distance
                if ( gGroups.SortProperty == null && showProximity && GetAttributeValue( AttributeKey.SortByDistance ).AsBoolean() )
                {
                    // only show groups with a known location, and sort those by distance
                    groups = groups.Where( a => distances.Select( b => b.Key ).Contains( a.Id ) ).ToList();
                    groups = groups.OrderBy( a => distances[a.Id] ).ThenBy( a => a.Name ).ToList();
                }

                // if limiting by PageSize, limit to the top X groups
                int? pageSize = ddlPageSize.SelectedValue.AsIntegerOrNull();
                if ( pageSize.HasValue && pageSize > 0 )
                {
                    groups = groups.Take( pageSize.Value ).ToList();
                }

                // If a map is to be shown
                if ( showMap && groups.Any() )
                {
                    ILavaTemplate lavaTemplate = null;

                    var parseResult = LavaService.ParseTemplate( GetAttributeValue( AttributeKey.MapInfo ) );

                    lavaTemplate = parseResult.Template;

                    // Add mapitems for all the remaining valid group locations
                    var groupMapItems = new List<MapItem>();
                    foreach ( var gl in groupLocations )
                    {
                        var group = groups.Where( g => g.Id == gl.GroupId ).FirstOrDefault();
                        if ( group != null )
                        {
                            // Resolve info window lava template
                            var linkedPageParams = new Dictionary<string, string> { { "GroupId", group.Id.ToString() } };
                            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, CurrentPerson );
                            mergeFields.Add( "Group", gl.Group );
                            mergeFields.Add( "Location", gl.Location );

                            Dictionary<string, object> linkedPages = new Dictionary<string, object>();
                            linkedPages.Add( AttributeKey.GroupDetailPage, LinkedPageRoute( AttributeKey.GroupDetailPage ) );

                            if ( _targetPersonGuid != Guid.Empty )
                            {
                                linkedPages.Add( AttributeKey.RegisterPage, LinkedPageUrl( AttributeKey.RegisterPage, _urlParms ) );
                            }
                            else
                            {
                                linkedPages.Add( AttributeKey.RegisterPage, LinkedPageRoute( AttributeKey.RegisterPage ) );
                            }

                            mergeFields.Add( "LinkedPages", linkedPages );
                            mergeFields.Add( "CampusContext", RockPage.GetCurrentContext( EntityTypeCache.Get( "Rock.Model.Campus" ) ) as Campus );

                            // add collection of allowed security actions
                            Dictionary<string, object> securityActions = new Dictionary<string, object>();
                            securityActions.Add( "View", group.IsAuthorized( Authorization.VIEW, CurrentPerson ) );
                            securityActions.Add( "Edit", group.IsAuthorized( Authorization.EDIT, CurrentPerson ) );
                            securityActions.Add( "Administrate", group.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) );
                            mergeFields.Add( "AllowedActions", securityActions );

                            string infoWindow;

                            var result = LavaService.RenderTemplate( lavaTemplate, mergeFields );

                            infoWindow = result.Text;

                            // Add a map item for group
                            var mapItem = new FinderMapItem( gl.Location );
                            mapItem.EntityTypeId = EntityTypeCache.Get( "Rock.Model.Group" ).Id;
                            mapItem.EntityId = group.Id;
                            mapItem.Name = group.Name;

                            var markerColor = GetAttributeValue( AttributeKey.MarkerColor );
                            if ( markerColor.IsNullOrWhiteSpace() )
                            {
                                mapItem.Color = group.GroupType.GroupTypeColor;
                            }
                            else
                            {
                                mapItem.Color = markerColor;
                            }

                            var locationPrecisionLevel = GetAttributeValue( AttributeKey.LocationPrecisionLevel );
                            switch ( locationPrecisionLevel.ToLower() )
                            {
                                case "narrow":
                                    mapItem.Point.Latitude = mapItem.Point.Latitude != null ? Convert.ToDouble( mapItem.Point.Latitude.Value.ToString( "#.###5" ) ) : ( double? ) null;
                                    mapItem.Point.Longitude = mapItem.Point.Longitude != null ? Convert.ToDouble( mapItem.Point.Longitude.Value.ToString( "#.###5" ) ) : ( double? ) null;
                                    break;
                                case "close":
                                    mapItem.Point.Latitude = mapItem.Point.Latitude != null ? Convert.ToDouble( mapItem.Point.Latitude.Value.ToString( "#.###" ) ) : ( double? ) null;
                                    mapItem.Point.Longitude = mapItem.Point.Longitude != null ? Convert.ToDouble( mapItem.Point.Longitude.Value.ToString( "#.###" ) ) : ( double? ) null;
                                    break;
                                case "wide":
                                    mapItem.Point.Latitude = mapItem.Point.Latitude != null ? Convert.ToDouble( mapItem.Point.Latitude.Value.ToString( "#.##" ) ) : ( double? ) null;
                                    mapItem.Point.Longitude = mapItem.Point.Longitude != null ? Convert.ToDouble( mapItem.Point.Longitude.Value.ToString( "#.##" ) ) : ( double? ) null;
                                    break;
                            }

                            mapItem.InfoWindow = HttpUtility.HtmlEncode( infoWindow.Replace( Environment.NewLine, string.Empty ).Replace( "\n", string.Empty ).Replace( "\t", string.Empty ) );
                            groupMapItems.Add( mapItem );
                        }
                    }

                    // Show the map
                    Map( personMapItem, fenceMapItems, groupMapItems );
                    pnlMap.Visible = true;
                }
                else
                {
                    pnlMap.Visible = false;
                }
            }
            else
            {
                pnlMap.Visible = false;
            }

            // Should a lava output be displayed
            if ( GetAttributeValue( AttributeKey.ShowLavaOutput ).AsBoolean() )
            {
                string template = GetAttributeValue( AttributeKey.LavaOutput );

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, CurrentPerson );
                if ( fences != null )
                {
                    mergeFields.Add( "Fences", fences.Select( f => f.Group ).ToList() );
                }
                else
                {
                    mergeFields.Add( "Fences", new Dictionary<string, object>() );
                }

                mergeFields.Add( "Groups", groups );

                Dictionary<string, object> linkedPages = new Dictionary<string, object>();
                linkedPages.Add( AttributeKey.GroupDetailPage, LinkedPageRoute( AttributeKey.GroupDetailPage ) );

                if ( _targetPersonGuid != Guid.Empty )
                {
                    linkedPages.Add( AttributeKey.RegisterPage, LinkedPageUrl( AttributeKey.RegisterPage, _urlParms ) );
                }
                else
                {
                    linkedPages.Add( AttributeKey.RegisterPage, LinkedPageRoute( AttributeKey.RegisterPage ) );
                }

                mergeFields.Add( "LinkedPages", linkedPages );
                mergeFields.Add( "CampusContext", RockPage.GetCurrentContext( EntityTypeCache.Get( "Rock.Model.Campus" ) ) as Campus );

                lLavaOverview.Text = template.ResolveMergeFields( mergeFields );

                pnlLavaOutput.Visible = true;
            }
            else
            {
                pnlLavaOutput.Visible = false;
            }

            // Should a grid be displayed
            if ( GetAttributeValue( AttributeKey.ShowGrid ).AsBoolean() )
            {
                pnlGrid.Visible = true;

                // Save the groups into the grid's object list since it is not being bound to actual group objects
                gGroups.ObjectList = new Dictionary<string, object>();
                groups.ForEach( g => gGroups.ObjectList.Add( g.Id.ToString(), g ) );

                // Bind the grid
                gGroups.DataSource = groups.Select( g =>
                {
                    var qryMembers = new GroupMemberService( rockContext )
                        .Queryable()
                        .Where( a => a.GroupId == g.Id );

                    if ( includePending )
                    {
                        qryMembers = qryMembers.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active
                            || m.GroupMemberStatus == GroupMemberStatus.Pending );
                    }
                    else
                    {
                        qryMembers = qryMembers.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active );
                    }

                    var groupType = GroupTypeCache.Get( g.GroupTypeId );

                    return new
                    {
                        Id = g.Id,
                        Name = g.Name,
                        GroupTypeName = groupType.Name,
                        GroupOrder = g.Order,
                        GroupTypeOrder = groupType.Order,
                        Description = g.Description,
                        IsSystem = g.IsSystem,
                        IsActive = g.IsActive,
                        GroupRole = string.Empty,
                        DateAdded = DateTime.MinValue,
                        Schedule = g.Schedule?.ToFriendlyScheduleText( true ),
                        MemberCount = qryMembers.Count(),
                        AverageAge = Math.Round( qryMembers.Select( m => new { m.Person.BirthDate, m.Person.DeceasedDate } ).ToList().Select( a => Person.GetAge( a.BirthDate, a.DeceasedDate ) ).Average() ?? 0.0D ),
                        Campus = g.Campus != null ? g.Campus.Name : string.Empty,
                        Distance = distances.Where( d => d.Key == g.Id )
                            .Select( d => d.Value ).FirstOrDefault()
                    };
                } ).ToList();
                gGroups.DataBind();
            }
            else
            {
                pnlGrid.Visible = false;
            }

            // Show the results
            pnlResults.Visible = true;
        }

        /// <summary>
        /// Binds the type of the group.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="groupTypes">The group types.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        private void BindGroupType( GroupTypePicker control, List<GroupType> groupTypes, string attributeName )
        {
            control.GroupTypes = groupTypes;

            var groupTypeGuid = GetAttributeValue( attributeName ).AsIntegerOrNull();
            if ( groupTypeGuid.HasValue )
            {
                var groupType = groupTypes.FirstOrDefault( g => g.Id.Equals( groupTypeGuid.Value ) );
                if ( groupType != null )
                {
                    control.SelectedGroupTypeId = groupType.Id;
                }
            }
        }

        private void BindGroupType( RockListBox control, List<GroupType> groupTypes )
        {
            control.DataSource = groupTypes.Select( t => new { value = t.Id, text = t.Name } );
            control.DataBind();

            var groupTypeIds = GetGroupTypeIds();

            if ( groupTypeIds != null )
            {
                var existingGroupTypeIds = groupTypes.Where( g => groupTypeIds.Any( gt => gt == g.Id ) );
                control.SetValues( groupTypeIds );
            }
        }

        private void BindGroupTypeLocationGrid()
        {
            var groupTypeIds = gtpGroupType.SelectedValuesAsInt;
            if ( groupTypeIds == null )
            {
                groupTypeIds = GetGroupTypeIds();
            }

            gGroupTypeLocation.DataSource = GroupTypeCache.All().Where( gt => groupTypeIds.Contains( gt.Id ) ).ToList();
            gGroupTypeLocation.DataBind();
        }

        private List<int> GetGroupTypeIds()
        {
            var groupTypeGuid = GetAttributeValue( AttributeKey.GroupType ).AsGuidOrNull();

            if ( groupTypeGuid == null )
            {
                return GetAttributeValue( AttributeKey.GroupType ).FromJsonOrNull<List<int>>();
            }

            return new List<int> { GroupTypeCache.Get( groupTypeGuid.Value ).Id };
        }

        private int? GetGroupTypeId( Guid? groupTypeGuid )
        {
            if ( groupTypeGuid.HasValue )
            {
                var groupType = GroupTypeCache.Get( groupTypeGuid.Value );
                if ( groupType != null )
                {
                    return groupType.Id;
                }
            }

            return null;
        }

        /// <summary>
        /// Maps the specified location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="fences">The fences.</param>
        /// <param name="groups">The groups.</param>
        private void Map( MapItem location, List<MapItem> fences, List<MapItem> groups )
        {
            pnlMap.Visible = true;

            string mapStylingFormat = @"
                        <style>
                            #map_wrapper {{
                                height: {0}px;
                            }}

                            #map_canvas {{
                                width: 100%;
                                height: 100%;
                                border-radius: var(--border-radius-base);
                            }}
                        </style>";
            lMapStyling.Text = string.Format( mapStylingFormat, GetAttributeValue( AttributeKey.MapHeight ) );

            // add styling to map
            string styleCode = "null";
            var markerColors = new List<string>();
            string mapId = string.Empty;

            DefinedValueCache dvcMapStyle = DefinedValueCache.Get( GetAttributeValue( AttributeKey.MapStyle ).AsInteger() );
            if ( dvcMapStyle != null )
            {
                var dynamicMapStyle = dvcMapStyle.GetAttributeValue( "DynamicMapStyle" );
                if ( dynamicMapStyle.IsNotNullOrWhiteSpace() )
                {
                    styleCode = dynamicMapStyle;
                }
                mapId = dvcMapStyle.GetAttributeValue( "core_GoogleMapId" );

                var colorsSetting = dvcMapStyle.GetAttributeValue( "Colors" ) ?? string.Empty;
                markerColors = colorsSetting
                    .Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries )
                    .ToList();
                markerColors.ForEach( c => c = c.Replace( "#", string.Empty ) );
            }

            if ( !markerColors.Any() )
            {
                markerColors.Add( "FE7569" );
            }

            string locationColor = markerColors[0].Replace( "#", string.Empty );
            var polygonColorList = GetAttributeValue( AttributeKey.PolygonColors ).Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
            string polygonColors = "\"" + polygonColorList.AsDelimited( "\", \"" ) + "\"";
            string groupColor = ( markerColors.Count > 1 ? markerColors[1] : markerColors[0] ).Replace( "#", string.Empty );

            string latitude = "39.8282";
            string longitude = "-98.5795";
            var orgLocation = GlobalAttributesCache.Get().OrganizationLocation;
            if ( orgLocation != null && orgLocation.GeoPoint != null )
            {
                latitude = orgLocation.GeoPoint.Latitude.ToString();
                longitude = orgLocation.GeoPoint.Longitude.ToString();
            }

            var maxZoomLevel = GetAttributeValue( AttributeKey.MaximumZoomLevel );
            if ( maxZoomLevel.IsNullOrWhiteSpace() )
            {
                maxZoomLevel = "null";
            }

            var minZoomLevel = GetAttributeValue( AttributeKey.MinimumZoomLevel );
            if ( minZoomLevel.IsNullOrWhiteSpace() )
            {
                minZoomLevel = "null";
            }

            var zoom = GetAttributeValue( AttributeKey.InitialZoomLevel );
            if ( zoom.IsNullOrWhiteSpace() )
            {
                zoom = "null";
            }

            var zoomThreshold = GetAttributeValue( AttributeKey.MarkerZoomLevel );
            if ( zoomThreshold.IsNullOrWhiteSpace() )
            {
                zoomThreshold = "null";
            }

            var zoomAmount = GetAttributeValue( AttributeKey.MarkerZoomAmount );
            if ( zoomAmount.IsNullOrWhiteSpace() )
            {
                zoomAmount = "null";
            }

            // write script to page
            string mapScriptFormat = @"

        var locationData = {0};
        var fenceData = {1};
        var groupData = {2}; 
        var markerScale = 1;

        var allMarkers = [];

        var map;
        var bounds = new google.maps.LatLngBounds();
        var infoWindow = new google.maps.InfoWindow();
        var parser = new DOMParser();

        var mapStyle = {3};
        var mapId = '{15}';
        var isMapIdEmpty = !mapId;

        var polygonColorIndex = 0;
        var polygonColors = [{5}];

        var min = .999999;
        var max = 1.000001;
        
        initializeMap();

        function initializeMap() {{

            // Set default map options
            var mapOptions = {{
                 mapTypeId: 'roadmap'
                ,center: new google.maps.LatLng({7}, {8})
                ,maxZoom: {11}
                ,minZoom: {12}
                ,zoom: {9}
            }};

            if (!isMapIdEmpty) {{
                mapOptions.mapId = mapId;
            }} else {{
                mapOptions.styles = mapStyle;
            }}

            // Display a map on the page
            map = new google.maps.Map(document.getElementById('map_canvas'), mapOptions);
            google.maps.event.addListener(map, 'zoom_changed', function() {{
                var zoomThreshold = {13};
                var zoomAmount = {14};

                var zoom = map.getZoom();

                if(!zoomThreshold || !zoomAmount) {{
                    return;
                }}

                var scale = markerScale;
                if ( zoom >= zoomThreshold ) {{
                    let zoomScale = [ 0, 1, 1.00025, 1.0005, 1.001, 1.002, 1.004, 1.008, 1.016, 1.032, 1.064, 1.128, 1.256, 1.512, 2.024, 3.048, 5.096, 9.192, 17.384, 33.769, 66.536]
                    let zoomScaleLastIndex = zoomScale.length - 1;
                    if(zoom > zoomScaleLastIndex)
                    {{
                        zoom = zoomScaleLastIndex;
                    }}

                    scale = (zoomScale[zoom] * zoomAmount );
                }}

                var markerCount = allMarkers.length;
                var updatedMarkers = [];
                for (var i = 0; i < markerCount; i++) {{
                    var marker = allMarkers[i];
                    var updatedMarker;

                    if (isMapIdEmpty) {{
                        var pinImage = {{
                            path: marker.icon.path,
                            fillColor: marker.icon.fillColor,
                            fillOpacity: marker.icon.fillOpacity,
                            strokeColor: marker.icon.strokeColor,
                            strokeWeight: marker.icon.strokeWeight,
                            scale: scale,
                            labelOrigin: marker.icon.labelOrigin,
                            anchor: marker.icon.anchor,
                        }};

                        // Remove marker
  			            marker.setMap(null);

				        // Add marker back
                        updatedMarker = new google.maps.Marker({{
                            position: marker.position,
                            icon: pinImage,
                            map: map,
                            id: marker.id,
                            title: marker.title,
                            info_window: marker.info_window,
                        }});
                    }}
                    else {{
                        const pinSvg = parser.parseFromString(""{10}"",""image/svg+xml"").documentElement;
                        pinSvg.style.fill = marker.icon_color;

                        // Remove marker
  			            marker.marker_element.map = null;

				        // Add marker back
                        updatedMarker = {{
                            id: marker.id,
                            position: marker.position,
                            map: map,
                            title: marker.title,
                            info_window: marker.info_window,
                            icon_color: marker.icon_color,
                            marker_element: new google.maps.marker.AdvancedMarkerElement({{
                                id: marker.id,
                                position: marker.position,
                                map: map,
                                title: marker.title,
                                content: pinSvg,
                            }}),
                            setMap: function(map) {{
                                this.map = map;
                                this.marker_element.map = map;
                            }},
                            getPosition: function() {{
                                return this.position;
                            }},
                            setPosition: function(position) {{
                                this.position = position;
                                this.marker_element.position = position;
                            }}
                        }};
                    }}

                    if ( updatedMarker.info_window != null ) {{ 
                        google.maps.event.addListener(updatedMarker.marker_element || marker, 'click', (function (marker) {{
                            return function () {{
                                openInfoWindow(marker);
                            }}
                        }})(updatedMarker));
                    }}

                    if ( updatedMarker.id && updatedMarker.id > 0 ) {{ 
                        google.maps.event.addListener(updatedMarker.marker_element || marker, 'mouseover', (function (marker) {{
                            return function () {{
                                $(""tr[datakey='"" + marker.id + ""']"").addClass('row-highlight');
                            }}
                        }})(updatedMarker));

                        google.maps.event.addListener(updatedMarker.marker_element || marker, 'mouseout', (function (marker) {{
                            return function () {{
                                $(""tr[datakey='"" + marker.id + ""']"").removeClass('row-highlight');
                            }}
                        }})(updatedMarker));
                    }}

                    updatedMarkers.push(updatedMarker);
                }}

                allMarkers = [...updatedMarkers];
	        }});

            map.setTilt(45);

            if ( locationData != null )
            {{
                var items = addMapItem(0, locationData, '{4}');
                for (var j = 0; j < items.length; j++) {{
                    items[j].setMap(map);
                }}
            }}

            if ( fenceData != null ) {{
                for (var i = 0; i < fenceData.length; i++) {{
                    var items = addMapItem(i, fenceData[i] );
                    for (var j = 0; j < items.length; j++) {{
                        items[j].setMap(map);
                    }}
                }}
            }}

            if ( groupData != null ) {{
                for (var i = 0; i < groupData.length; i++) {{
                    var items = addMapItem(i, groupData[i], groupData[i].Color ? groupData[i].Color : '{6}');
                    for (var j = 0; j < items.length; j++) {{
                        items[j].setMap(map);
                    }}
                }}
            }}

            // adjust any markers that may overlap
            adjustOverlappedMarkers();

            if (!bounds.isEmpty()) {{
                if(mapOptions.zoom || mapOptions.zoom === 0){{
                    map.setCenter(bounds.getCenter());
                }} else {{
                    map.fitBounds(bounds);
                }}
            }}

        }}

        function openInfoWindowById(id) {{ 
            marker = $.grep(allMarkers, function(m) {{ return m.id == id }})[0];
            openInfoWindow(marker);
        }}

        function openInfoWindow(marker) {{
            infoWindow.setContent( $('<div/>').html(marker.info_window).text() );
            if (isMapIdEmpty) {{
                infoWindow.open(map, marker);
            }}
            else {{
                infoWindow.open({{
                    anchor: marker.marker_element,
                    map
                }});
            }}
        }}

        function addMapItem( i, mapItem, color ) {{

            var items = [];

            if (mapItem.Point) {{ 

                var position = new google.maps.LatLng(mapItem.Point.Latitude, mapItem.Point.Longitude);
                bounds.extend(position);

                if (!color) {{
                    color = '#FE7569';
                }}

                if ( color.length > 0 && color.toLowerCase().indexOf('rgb') < 0 && color[0] != '#' )
                {{
                    color = '#' + color;
                }}

                if (isMapIdEmpty) {{
                    var pinImage = {{
                        path: ""{10}"",
                        fillColor: color,
                        fillOpacity: 1,
                        strokeColor: '#000',
                        strokeWeight: 0,
                        scale: markerScale,
                        labelOrigin: new google.maps.Point(0, 0),
                        anchor: new google.maps.Point(0, 0),
                    }};
                    marker = new google.maps.Marker({{
                        id: mapItem.EntityId,
                        position: position,
                        map: map,
                        title: htmlDecode(mapItem.Name),
                        icon: pinImage,
                        info_window: mapItem.InfoWindow,
                    }});
                }}
                else {{
                    const pinSvg = parser.parseFromString(""{10}"",""image/svg+xml"").documentElement;
                    pinSvg.style.fill = color;

                    marker = {{
                        id: mapItem.EntityId,
                        position: position,
                        map: map,
                        title: htmlDecode(mapItem.Name),
                        info_window: mapItem.InfoWindow,
                        icon_color: color,
                        marker_element: new google.maps.marker.AdvancedMarkerElement({{
                            id: mapItem.EntityId,
                            position: position,
                            map: map,
                            title: htmlDecode(mapItem.Name),
                            content: pinSvg
                        }}),
                        setMap: function(map) {{
                            this.map = map;
                        }},
                        getPosition: function() {{
                            return this.position;
                        }},
                        setPosition: function(position) {{
                            this.position = position;
                        }}
                    }}
                }}

                items.push(marker);
                allMarkers.push(marker);

                if ( mapItem.InfoWindow != null ) {{
                    google.maps.event.addListener(marker.marker_element || marker, 'click', (function (marker, i) {{
                        return function () {{
                            openInfoWindow(marker);
                        }}
                    }})(marker, i));
                }}

                if ( mapItem.EntityId && mapItem.EntityId > 0 ) {{ 
                    google.maps.event.addListener(marker.marker_element || marker, 'mouseover', (function (marker, i) {{
                        return function () {{
                            $(""tr[datakey='"" + mapItem.EntityId + ""']"").addClass('row-highlight');
                        }}
                    }})(marker, i));

                    google.maps.event.addListener(marker.marker_element || marker, 'mouseout', (function (marker, i) {{
                        return function () {{
                            $(""tr[datakey='"" + mapItem.EntityId + ""']"").removeClass('row-highlight');
                        }}
                    }})(marker, i));

                }}

            }}

            if (typeof mapItem.PolygonPoints !== 'undefined' && mapItem.PolygonPoints.length > 0) {{

                var polygon;
                var polygonPoints = [];

                $.each(mapItem.PolygonPoints, function(j, point) {{
                    var position = new google.maps.LatLng(point.Latitude, point.Longitude);
                    bounds.extend(position);
                    polygonPoints.push(position);
                }});

                var polygonColor = getNextPolygonColor();

                polygon = new google.maps.Polygon({{
                    paths: polygonPoints,
                    map: map,
                    strokeColor: polygonColor,
                    fillColor: polygonColor
                }});

                items.push(polygon);

                // Get Center
                var polyBounds = new google.maps.LatLngBounds();
                for ( j = 0; j < polygonPoints.length; j++) {{
                    polyBounds.extend(polygonPoints[j]);
                }}

                if ( mapItem.InfoWindow != null ) {{ 
                    google.maps.event.addListener(polygon, 'click', (function (polygon, i) {{
                        return function () {{
                            infoWindow.setContent( $('<div/>').html(mapItem.InfoWindow).text() );
                            infoWindow.setPosition(polyBounds.getCenter());
                            infoWindow.open(map);
                        }}
                    }})(polygon, i));
                }}
            }}

            return items;

        }}
        
        function setAllMap(markers, map) {{
            for (var i = 0; i < markers.length; i++) {{
                markers[i].setMap(map);
            }}
        }}

        function htmlDecode(input) {{
            var e = document.createElement('div');
            e.innerHTML = input;
            return e.childNodes.length === 0 ? """" : e.childNodes[0].nodeValue;
        }}

        function getNextPolygonColor() {{
            var color = 'FE7569';
            if ( polygonColors.length > polygonColorIndex ) {{
                color = polygonColors[polygonColorIndex];
                polygonColorIndex++;
            }} else {{
                color = polygonColors[0];
                polygonColorIndex = 1;
            }}
            return color;
        }}

        function adjustOverlappedMarkers() {{
            
            if (allMarkers.length > 1) {{
                for(i=0; i < allMarkers.length-1; i++) {{
                    var marker1 = allMarkers[i];
                    var pos1 = marker1.getPosition();
                    for(j=i+1; j < allMarkers.length; j++) {{
                        var marker2 = allMarkers[j];
                        var pos2 = marker2.getPosition();
                        if (pos1.equals(pos2)) {{
                            var newLat = pos1.lat() * (Math.random() * (max - min) + min);
                            var newLng = pos1.lng() * (Math.random() * (max - min) + min);
                            marker1.setPosition( new google.maps.LatLng(newLat,newLng) );
                        }}
                    }}
                }}
            }}

        }}
";

            var locationJson = location != null ?
                string.Format( "JSON.parse('{0}')", location.ToJson().Replace( Environment.NewLine, string.Empty ).Replace( "\\", "\\\\" ).EscapeQuotes().Replace( "\x0A", string.Empty ) ) : "null";

            var fencesJson = fences != null && fences.Any() ?
                string.Format( "JSON.parse('{0}')", fences.ToJson().Replace( Environment.NewLine, string.Empty ).Replace( "\\", "\\\\" ).EscapeQuotes().Replace( "\x0A", string.Empty ) ) : "null";

            var groupsJson = groups != null && groups.Any() ?
                string.Format( "JSON.parse('{0}')", groups.ToJson().Replace( Environment.NewLine, string.Empty ).Replace( "\\", "\\\\" ).EscapeQuotes().Replace( "\x0A", string.Empty ) ) : "null";

            var markerDefinedValueId = GetAttributeValue( AttributeKey.MapMarker ).AsIntegerOrNull();
            var marker = "M 0,0 C -2,-20 -10,-22 -10,-30 A 10,10 0 1,1 10,-30 C 10,-22 2,-20 0,0 z";

            if ( markerDefinedValueId != null )
            {
                marker = DefinedValueCache.Get( markerDefinedValueId.Value ).Description;
            }

            if (mapId != "DEFAULT_MAP_ID" )
            {
                var markerFormat = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"50\" height=\"50\" viewBox=\"-10 -40 30 55\"><path d=\"{0}\"/></svg>";
                marker = string.Format( markerFormat, marker ).Replace( "\"", "'" );
            }

            string mapScript = string.Format(
                mapScriptFormat,
                locationJson,       // 0
                fencesJson,         // 1
                groupsJson,         // 2
                styleCode,          // 3
                locationColor,      // 4
                polygonColors,      // 5
                groupColor,         // 6
                latitude,           // 7
                longitude,          // 8
                zoom,               // 9
                marker,             // 10
                maxZoomLevel,       // 11
                minZoomLevel,       // 12
                zoomThreshold,      // 13
                zoomAmount,         // 14
                mapId );            // 15

            ScriptManager.RegisterStartupScript( pnlMap, pnlMap.GetType(), "group-finder-map-script", mapScript, true );
        }

        private void ShowError( string message )
        {
            nbNotice.Heading = "Error";
            nbNotice.NotificationBoxType = NotificationBoxType.Danger;
            ShowMessage( message );
        }

        private void ShowWarning( string message )
        {
            nbNotice.Heading = "Warning";
            nbNotice.NotificationBoxType = NotificationBoxType.Warning;
            ShowMessage( message );
        }

        private void ShowMessage( string message )
        {
            nbNotice.Text = string.Format( "<p>{0}</p>", message );
            nbNotice.Visible = true;
        }

        #endregion

        /// <summary>
        /// A map item class specific to group finder
        /// </summary>
        public class FinderMapItem : MapItem
        {
            /// <summary>
            /// Gets or sets the information window.
            /// </summary>
            /// <value>
            /// The information window.
            /// </value>
            public string InfoWindow { get; set; }

            public string Color { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="FinderMapItem"/> class.
            /// </summary>
            /// <param name="location">The location.</param>
            public FinderMapItem( Location location )
                : base( location )
            {
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlPageSize control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlPageSize_SelectedIndexChanged( object sender, EventArgs e )
        {
            ShowResults();
        }

        protected void gGroupTypeLocation_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var dropDownList = e.Row.ControlsOfTypeRecursive<RockDropDownList>().FirstOrDefault();
            var groupType = e.Row.DataItem as GroupTypeCache;

            if ( dropDownList == null || groupType == null )
            {
                return;
            }

            dropDownList.Attributes.Add( "group-type-id", groupType.Id.ToString() );
            var locationTypeValues = groupType.LocationTypeValues;
            if ( locationTypeValues != null )
            {
                dropDownList.Items.Add( new ListItem( "All", string.Empty ) );
                foreach ( var locationTypeValue in locationTypeValues )
                {
                    dropDownList.Items.Add( new ListItem( locationTypeValue.Value, locationTypeValue.Id.ToString() ) );
                }

                if ( GroupTypeLocations != null && GroupTypeLocations.ContainsKey( groupType.Id ) )
                {
                    dropDownList.SelectedValue = GroupTypeLocations[groupType.Id].ToString();
                }
            }
        }

        private Dictionary<int, int> GroupTypeLocations { get; set; }

        protected void lLocationList_SelectedIndexChanged( object sender, EventArgs e )
        {
            var dropDownList = sender as RockDropDownList;
            if ( dropDownList == null )
            {
                return;
            }

            var groupTypeId = dropDownList.Attributes["group-type-id"].AsIntegerOrNull();
            if ( groupTypeId == null )
            {
                return;
            }

            var groupTypeLocations = GroupTypeLocations;
            if ( groupTypeLocations == null )
            {
                groupTypeLocations = new Dictionary<int, int>();
            }

            var groupTypeLocationId = dropDownList.SelectedValue.AsIntegerOrNull();
            if ( groupTypeLocationId == null )
            {
                groupTypeLocations.Remove( groupTypeId.Value );
            }
            else
            {
                groupTypeLocations.AddOrReplace( groupTypeId.Value, groupTypeLocationId.Value );
            }

            GroupTypeLocations = groupTypeLocations;
        }

        protected void lbShowAdditionalMapSettings_Click( object sender, EventArgs e )
        {
            dMapAdditionalSettings.Visible = !dMapAdditionalSettings.Visible;
        }
    }
}

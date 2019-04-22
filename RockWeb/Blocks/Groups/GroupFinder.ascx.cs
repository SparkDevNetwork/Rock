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
using System.Runtime.Caching;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using DotLiquid;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Field.Types;
using Rock.Model;
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

    // Linked Pages
    [LinkedPage( "Group Detail Page", "The page to navigate to for group details.", false, "", "CustomSetting" )]
    [LinkedPage( "Register Page", "The page to navigate to when registering for a group.", false, "", "CustomSetting" )]

    // Filter Settings
    [GroupTypeField( "Group Type", "", true, "", "CustomSetting" )]
    [GroupTypeField( "Geofenced Group Type", "", false, "", "CustomSetting" )]

    [TextField( "CampusLabel", "", true, "Campuses", "CustomSetting" )]
    [TextField( "TimeOfDayLabel", "", true, "Time of Day", "CustomSetting" )]
    [TextField( "DayOfWeekLabel", "", true, "Day of Week", "CustomSetting" )]
    [TextField( "ScheduleFilters", "", false, "", "CustomSetting" )]
    [BooleanField( "Display Campus Filter", "", false, "CustomSetting" )]
    [BooleanField( "Enable Campus Context", "", false, "CustomSetting" )]
    [BooleanField( "Hide Overcapacity Groups", "When set to true, groups that are at capacity or whose default GroupTypeRole are at capacity are hidden.", true )]
    [AttributeField( Rock.SystemGuid.EntityType.GROUP, "Attribute Filters", "", false, true, "", "CustomSetting" )]

    // Map Settings
    [BooleanField( "Show Map", "", false, "CustomSetting" )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.MAP_STYLES, "Map Style", "", true, false, Rock.SystemGuid.DefinedValue.MAP_STYLE_GOOGLE, "CustomSetting" )]
    [IntegerField( "Map Height", "", false, 600, "CustomSetting" )]
    [BooleanField( "Show Fence", "", false, "CustomSetting" )]
    [ValueListField( "Polygon Colors", "", false, "#f37833|#446f7a|#afd074|#649dac|#f8eba2|#92d0df|#eaf7fc", "#ffffff", null, null, "CustomSetting" )]
    [CodeEditorField( "Map Info", "", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, false, @"
<h4 class='margin-t-none'>{{ Group.Name }}</h4> 

<div class='margin-b-sm'>
{% for attribute in Group.AttributeValues %}
    <strong>{{ attribute.AttributeName }}:</strong> {{ attribute.ValueFormatted }} <br />
{% endfor %}
</div>

<div class='margin-v-sm'>
{% if Location.FormattedHtmlAddress && Location.FormattedHtmlAddress != '' %}
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
", "CustomSetting" )]
    [BooleanField( "Map Info Debug", "", false, "CustomSetting" )]

    // Lava Output Settings
    [BooleanField( "Show Lava Output", "", false, "CustomSetting" )]
    [CodeEditorField( "Lava Output", "", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, false, @"
", "CustomSetting" )]
    [BooleanField( "Lava Output Debug", "", false, "CustomSetting" )]

    // Grid Settings
    [BooleanField( "Show Grid", "", false, "CustomSetting" )]
    [BooleanField( "Show Schedule", "", false, "CustomSetting" )]
    [BooleanField( "Show Proximity", "", false, "CustomSetting" )]
    [BooleanField( "Show Campus", "", false, "CustomSetting" )]
    [BooleanField( "Show Count", "", false, "CustomSetting" )]
    [BooleanField( "Show Age", "", false, "CustomSetting" )]
    [BooleanField( "Show Description", "", true, "CustomSetting" )]
    [AttributeField( Rock.SystemGuid.EntityType.GROUP, "Attribute Columns", "", false, true, "", "CustomSetting" )]
    [BooleanField( "Sort By Distance", "", false, "CustomSetting" )]
    [TextField( "Page Sizes", "To show a dropdown of page sizes, enter a comma delimited list of page sizes. For example: 10,20 will present a drop down with 10,20,All as options with the default as 10", false, "", "CustomSetting" )]

    public partial class GroupFinder : RockBlockCustomSettings
    {
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

            AttributeFilters = ViewState["AttributeFilters"] as List<AttributeCache>;
            AttributeColumns = ViewState["AttributeColumns"] as List<AttributeCache>;

            BuildDynamicControls();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gGroups.DataKeyNames = new string[] { "Id" };
            gGroups.Actions.ShowAdd = false;
            gGroups.GridRebind += gGroups_GridRebind;
            gGroups.ShowActionRow = false;
            gGroups.AllowPaging = false;

            this.BlockUpdated += Block_Updated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            this.LoadGoogleMapsApi();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

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

                if ( GetAttributeValue( "EnableCampusContext" ).AsBoolean() )
                {
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
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["AttributeFilters"] = AttributeFilters;
            ViewState["AttributeColumns"] = AttributeColumns;

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

            SetAttributeValue( "GroupType", GetGroupTypeGuid( gtpGroupType.SelectedGroupTypeId ) );
            SetAttributeValue( "GeofencedGroupType", GetGroupTypeGuid( gtpGeofenceGroupType.SelectedGroupTypeId ) );

            SetAttributeValue( "DayOfWeekLabel", tbDayOfWeekLabel.Text);
            SetAttributeValue( "TimeOfDayLabel", tbTimeOfDayLabel.Text );
            SetAttributeValue( "CampusLabel", tbCampusLabel.Text );

            var schFilters = new List<string>();
            if ( rblFilterDOW.Visible )
            {
                schFilters.Add( rblFilterDOW.SelectedValue );
                schFilters.Add( cbFilterTimeOfDay.Checked ? "Time" : string.Empty );
            }

            SetAttributeValue( "ScheduleFilters", schFilters.Where( f => f != string.Empty ).ToList().AsDelimited( "," ) );

            SetAttributeValue( "DisplayCampusFilter", cbFilterCampus.Checked.ToString() );
            SetAttributeValue( "EnableCampusContext", cbCampusContext.Checked.ToString() );
            SetAttributeValue( "AttributeFilters", cblAttributes.Items.Cast<ListItem>().Where( i => i.Selected ).Select( i => i.Value ).ToList().AsDelimited( "," ) );

            SetAttributeValue( "ShowMap", cbShowMap.Checked.ToString() );
            SetAttributeValue( "MapStyle", dvpMapStyle.SelectedValue );
            SetAttributeValue( "MapHeight", nbMapHeight.Text );
            SetAttributeValue( "ShowFence", cbShowFence.Checked.ToString() );
            SetAttributeValue( "PolygonColors", vlPolygonColors.Value );
            SetAttributeValue( "MapInfo", ceMapInfo.Text );

            SetAttributeValue( "ShowLavaOutput", cbShowLavaOutput.Checked.ToString() );
            SetAttributeValue( "LavaOutput", ceLavaOutput.Text );

            SetAttributeValue( "ShowGrid", cbShowGrid.Checked.ToString() );
            SetAttributeValue( "ShowSchedule", cbShowSchedule.Checked.ToString() );
            SetAttributeValue( "ShowDescription", cbShowDescription.Checked.ToString() );
            SetAttributeValue( "ShowCampus", cbShowCampus.Checked.ToString() );
            SetAttributeValue( "ShowProximity", cbProximity.Checked.ToString() );
            SetAttributeValue( "SortByDistance", cbSortByDistance.Checked.ToString() );
            SetAttributeValue( "PageSizes", tbPageSizes.Text );
            SetAttributeValue( "ShowCount", cbShowCount.Checked.ToString() );
            SetAttributeValue( "ShowAge", cbShowAge.Checked.ToString() );
            SetAttributeValue( "AttributeColumns", cblGridAttributes.Items.Cast<ListItem>().Where( i => i.Selected ).Select( i => i.Value ).ToList().AsDelimited( "," ) );

            var ppFieldType = new PageReferenceFieldType();
            SetAttributeValue( "GroupDetailPage", ppFieldType.GetEditValue( ppGroupDetailPage, null ) );
            SetAttributeValue( "RegisterPage", ppFieldType.GetEditValue( ppRegisterPage, null ) );

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

            pnlMap.Visible = false;
            pnlLavaOutput.Visible = false;
            pnlGrid.Visible = false;
        }

        /// <summary>
        /// Handles the RowSelected event of the gGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gGroups_RowSelected( object sender, RowEventArgs e )
        {
            if (!NavigateToLinkedPage("GroupDetailPage", "GroupId", e.RowKeyId))
            {
                ShowResults();
                ScriptManager.RegisterStartupScript(pnlMap, pnlMap.GetType(), "group-finder-row-selected", "openInfoWindowById("+e.RowKeyId+");", true);
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
                    if ( !NavigateToLinkedPage( "RegisterPage", _urlParms ) )
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

            var rockContext = new RockContext();
            var groupTypes = new GroupTypeService( rockContext )
                .Queryable().AsNoTracking().ToList();

            BindGroupType( gtpGroupType, groupTypes, "GroupType" );
            BindGroupType( gtpGeofenceGroupType, groupTypes, "GeofencedGroupType" );
            string av = GetAttributeValue( "CampusLabel" );
            tbCampusLabel.Text = GetAttributeValue( "CampusLabel" );
            tbDayOfWeekLabel.Text = GetAttributeValue( "DayOfWeekLabel" );
            tbTimeOfDayLabel.Text = GetAttributeValue( "TimeOfDayLabel" );

            var scheduleFilters = GetAttributeValue( "ScheduleFilters" ).SplitDelimitedValues( false ).ToList();
            if ( scheduleFilters.Contains("Day") )
            {
                rblFilterDOW.SetValue( "Day" );
            }
            else if ( scheduleFilters.Contains( "Days"))
            {
                rblFilterDOW.SetValue( "Days" );
            }

            cbFilterTimeOfDay.Checked = scheduleFilters.Contains( "Time" );

            SetGroupTypeOptions();
            foreach ( string attr in GetAttributeValue( "AttributeFilters" ).SplitDelimitedValues() )
            {
                var li = cblAttributes.Items.FindByValue( attr );
                if ( li != null )
                {
                    li.Selected = true;
                }
            }

            cbFilterCampus.Checked = GetAttributeValue( "DisplayCampusFilter" ).AsBoolean();
            cbCampusContext.Checked = GetAttributeValue( "EnableCampusContext" ).AsBoolean();

            cbShowMap.Checked = GetAttributeValue( "ShowMap" ).AsBoolean();
            dvpMapStyle.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.MAP_STYLES.AsGuid() ).Id;
            dvpMapStyle.SetValue( GetAttributeValue( "MapStyle" ) );
            nbMapHeight.Text = GetAttributeValue( "MapHeight" );
            cbShowFence.Checked = GetAttributeValue( "ShowFence" ).AsBoolean();
            vlPolygonColors.Value = GetAttributeValue( "PolygonColors" );
            ceMapInfo.Text = GetAttributeValue( "MapInfo" );

            cbShowLavaOutput.Checked = GetAttributeValue( "ShowLavaOutput" ).AsBoolean();
            ceLavaOutput.Text = GetAttributeValue( "LavaOutput" );

            cbShowGrid.Checked = GetAttributeValue( "ShowGrid" ).AsBoolean();
            cbShowSchedule.Checked = GetAttributeValue( "ShowSchedule" ).AsBoolean();
            cbShowDescription.Checked = GetAttributeValue( "ShowDescription" ).AsBoolean();
            cbShowCampus.Checked = GetAttributeValue( "ShowCampus" ).AsBoolean();
            cbProximity.Checked = GetAttributeValue( "ShowProximity" ).AsBoolean();
            cbSortByDistance.Checked = GetAttributeValue( "SortByDistance" ).AsBoolean();
            tbPageSizes.Text = GetAttributeValue( "PageSizes" );
            cbShowCount.Checked = GetAttributeValue( "ShowCount" ).AsBoolean();
            cbShowAge.Checked = GetAttributeValue( "ShowAge" ).AsBoolean();
            foreach ( string attr in GetAttributeValue( "AttributeColumns" ).SplitDelimitedValues() )
            {
                var li = cblGridAttributes.Items.FindByValue( attr );
                if ( li != null )
                {
                    li.Selected = true;
                }
            }

            var ppFieldType = new PageReferenceFieldType();
            ppFieldType.SetEditValue( ppGroupDetailPage, null, GetAttributeValue( "GroupDetailPage" ) );
            ppFieldType.SetEditValue( ppRegisterPage, null, GetAttributeValue( "RegisterPage" ) );

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

            if ( gtpGroupType.SelectedGroupTypeId.HasValue )
            {
                var groupType = GroupTypeCache.Get( gtpGroupType.SelectedGroupTypeId.Value );
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
                            cblAttributes.Items.Add( new ListItem( attribute.Value.Name, attribute.Value.Guid.ToString() ) );
                        }

                        cblGridAttributes.Items.Add( new ListItem( attribute.Value.Name, attribute.Value.Guid.ToString() ) );
                    }
                }
            }

            cblAttributes.Visible = cblAttributes.Items.Count > 0;
            cblGridAttributes.Visible = cblAttributes.Items.Count > 0;
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
            Guid? fenceTypeGuid = GetAttributeValue( "GeofencedGroupType" ).AsGuidOrNull();
            if ( fenceTypeGuid.HasValue || GetAttributeValue( "ShowProximity" ).AsBoolean() )
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

                // Check to see if there's any filters
                string scheduleFilters = GetAttributeValue( "ScheduleFilters" );
                if ( !string.IsNullOrWhiteSpace( scheduleFilters ) || AttributeFilters.Any() )
                {
                    phFilterControls.Visible = true;
                    btnSearch.Visible = true;
                }
                else
                {
                    // Hide the search button and show the results immediately since there is 
                    // no filter criteria to be entered
                    phFilterControls.Visible = false;
                    btnSearch.Visible = GetAttributeValue( "DisplayCampusFilter" ).AsBoolean();
                    pnlResults.Visible = true;
                }
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
            foreach ( string attr in GetAttributeValue( "AttributeFilters" ).SplitDelimitedValues() )
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
            foreach ( string attr in GetAttributeValue( "AttributeColumns" ).SplitDelimitedValues() )
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
            var scheduleFilters = GetAttributeValue( "ScheduleFilters" ).SplitDelimitedValues().ToList();
            if ( scheduleFilters.Contains( "Days" ) )
            {
                var dowsFilterControl = new RockCheckBoxList();
                dowsFilterControl.ID = "filter_dows";
                dowsFilterControl.Label = GetAttributeValue( "DayOfWeekLabel" );
                dowsFilterControl.BindToEnum<DayOfWeek>();
                dowsFilterControl.RepeatDirection = RepeatDirection.Horizontal;
                
                AddFilterControl( dowsFilterControl, "Days of Week", "The day of week that group meets on." );
            }

            if ( scheduleFilters.Contains( "Day" ) )
            {
                var control = FieldTypeCache.Get( Rock.SystemGuid.FieldType.DAY_OF_WEEK ).Field.FilterControl( null, "filter_dow", false, Rock.Reporting.FilterMode.SimpleFilter );
                string dayOfWeekLabel = GetAttributeValue( "DayOfWeekLabel" );
                AddFilterControl( control, dayOfWeekLabel, "The day of week that group meets on." );
            }

            if ( scheduleFilters.Contains( "Time" ) )
            {
                var control = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TIME ).Field.FilterControl( null, "filter_time", false, Rock.Reporting.FilterMode.SimpleFilter );
                string timeOfDayLabel = GetAttributeValue( "TimeOfDayLabel" );
                AddFilterControl( control, timeOfDayLabel, "The time of day that group meets." );
            }
            
            if ( GetAttributeValue( "DisplayCampusFilter" ).AsBoolean() )
            {

                cblCampus.Label = GetAttributeValue( "CampusLabel" );
                cblCampus.Visible = true;
                cblCampus.DataSource = CampusCache.All().Where( c => c.IsActive == true );
                cblCampus.DataBind();
            }
            else
            {
                cblCampus.Visible = false;
            }

            if ( AttributeFilters != null )
            {
                foreach ( var attribute in AttributeFilters )
                {
                    var control = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filter_" + attribute.Id.ToString(), false, Rock.Reporting.FilterMode.SimpleFilter );
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

            var registerPage = new PageReference( GetAttributeValue( "RegisterPage" ) );

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
            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "PageSizes" ) ) )
            {
                pageSizes = GetAttributeValue( "PageSizes" ).Split( ',' ).AsIntegerList();
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
            if ( GetAttributeValue( "SortByDistance" ).AsBoolean() )
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
            Guid? groupTypeGuid = GetAttributeValue( "GroupType" ).AsGuidOrNull();
            if ( !groupTypeGuid.HasValue )
            {
                ShowError( "A valid Group Type is required." );
                return;
            }

            gGroups.Columns[1].Visible = GetAttributeValue( "ShowDescription" ).AsBoolean();
            gGroups.Columns[2].Visible = GetAttributeValue( "ShowSchedule" ).AsBoolean();
            gGroups.Columns[3].Visible = GetAttributeValue( "ShowCount" ).AsBoolean();
            gGroups.Columns[4].Visible = GetAttributeValue( "ShowAge" ).AsBoolean();

            bool showProximity = GetAttributeValue( "ShowProximity" ).AsBoolean();
            gGroups.Columns[6].Visible = showProximity;  // Distance

            // Get query of groups of the selected group type
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );
            var groupQry = groupService
                .Queryable( "GroupLocations.Location" )
                .Where( g => g.IsActive && g.GroupType.Guid.Equals( groupTypeGuid.Value ) && g.IsPublic );

            var groupParameterExpression = groupService.ParameterExpression;
            var schedulePropertyExpression = Expression.Property( groupParameterExpression, "Schedule" );

            var dowsFilterControl = phFilterControls.FindControl( "filter_dows" ) as RockCheckBoxList;
            if ( dowsFilterControl != null )
            {
                var dows = new List<DayOfWeek>();
                dowsFilterControl.SelectedValuesAsInt.ForEach( i => dows.Add( (DayOfWeek)i ) );

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

            if ( GetAttributeValue( "DisplayCampusFilter" ).AsBoolean() )
            {
                var searchCampuses = cblCampus.SelectedValuesAsInt;
                if ( searchCampuses.Count > 0 )
                {
                    groupQry = groupQry.Where( c => searchCampuses.Contains( c.CampusId ?? -1 ) );
                }
            }

            // This hides the groups that are at or over capacity by doing two things:
            // 1) If the group has a GroupCapacity, check that we haven't met or exceeded that.
            // 2) When someone registers for a group on the front-end website, they automatically get added with the group's default
            //    GroupTypeRole. If that role exists and has a MaxCount, check that we haven't met or exceeded it yet.
            if ( GetAttributeValue( "HideOvercapacityGroups" ).AsBoolean() )
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
                foreach ( var attribute in AttributeFilters )
                {
                    var filterControl = phFilterControls.FindControl( "filter_" + attribute.Id.ToString() );
                    groupQry = attribute.FieldType.Field.ApplyAttributeQueryFilter( groupQry, filterControl, attribute, groupService, Rock.Reporting.FilterMode.SimpleFilter );
                }
            }

            List<GroupLocation> fences = null;
            List<Group> groups = null;

            // Run query to get list of matching groups
            SortProperty sortProperty = gGroups.SortProperty;
            if ( sortProperty != null )
            {
                groups = groupQry.Sort( sortProperty ).ToList();
            }
            else
            {
                groups = groupQry.OrderBy( g => g.Name ).ToList();
            }

            gGroups.Columns[5].Visible = GetAttributeValue( "ShowCampus" ).AsBoolean() && groups.Any( g => g.CampusId.HasValue );

            int? fenceGroupTypeId = GetGroupTypeId( GetAttributeValue( "GeofencedGroupType" ).AsGuidOrNull() );
            bool showMap = GetAttributeValue( "ShowMap" ).AsBoolean();
            bool showFences = showMap && GetAttributeValue( "ShowFence" ).AsBoolean();

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
                            .Where( gl =>
                                gl.Group.GroupTypeId == fenceGroupTypeId &&
                                gl.Location.GeoFence != null &&
                                personLocation.GeoPoint.Intersects( gl.Location.GeoFence ) )
                            .ToList();
                    }

                    // Limit the group locations to only those locations inside one of the fences
                    groupLocations = groupLocations
                        .Where( gl =>
                            fences.Any( f => gl.Location.GeoPoint.Intersects( f.Location.GeoFence ) ) )
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
                if ( gGroups.SortProperty == null && showProximity && GetAttributeValue( "SortByDistance" ).AsBoolean() )
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
                    Template template = Template.Parse( GetAttributeValue( "MapInfo" ) );

                    bool showDebug = UserCanEdit && GetAttributeValue( "MapInfoDebug" ).AsBoolean();
                    lMapInfoDebug.Visible = showDebug;

                    // Add mapitems for all the remaining valid group locations
                    var groupMapItems = new List<MapItem>();
                    foreach ( var gl in groupLocations )
                    {
                        var group = groups.Where( g => g.Id == gl.GroupId ).FirstOrDefault();
                        if ( group != null )
                        {
                            // Resolve info window lava template
                            var linkedPageParams = new Dictionary<string, string> { { "GroupId", group.Id.ToString() } };
                            var mergeFields = new Dictionary<string, object>();
                            mergeFields.Add( "Group", gl.Group );
                            mergeFields.Add( "Location", gl.Location );

                            Dictionary<string, object> linkedPages = new Dictionary<string, object>();
                            linkedPages.Add( "GroupDetailPage", LinkedPageRoute( "GroupDetailPage" ) );

                            if ( _targetPersonGuid != Guid.Empty )
                            {
                                linkedPages.Add( "RegisterPage", LinkedPageUrl( "RegisterPage", _urlParms ) );
                            }
                            else
                            {
                                linkedPages.Add( "RegisterPage", LinkedPageRoute( "RegisterPage" ) );
                            }

                            mergeFields.Add( "LinkedPages", linkedPages );
                            mergeFields.Add( "CampusContext", RockPage.GetCurrentContext( EntityTypeCache.Get( "Rock.Model.Campus" ) ) as Campus );

                            // add collection of allowed security actions
                            Dictionary<string, object> securityActions = new Dictionary<string, object>();
                            securityActions.Add( "View", group.IsAuthorized( Authorization.VIEW, CurrentPerson ) );
                            securityActions.Add( "Edit", group.IsAuthorized( Authorization.EDIT, CurrentPerson ) );
                            securityActions.Add( "Administrate", group.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) );
                            mergeFields.Add( "AllowedActions", securityActions );

                            string infoWindow = template.Render( Hash.FromDictionary( mergeFields ) );

                            if ( showDebug )
                            {
                                lMapInfoDebug.Text = mergeFields.lavaDebugInfo( null, "<span class='label label-info'>Lava used for the map window.</span>", string.Empty );
                                showDebug = false;
                            }

                            // Add a map item for group
                            var mapItem = new FinderMapItem( gl.Location );
                            mapItem.EntityTypeId = EntityTypeCache.Get( "Rock.Model.Group" ).Id;
                            mapItem.EntityId = group.Id;
                            mapItem.Name = group.Name;
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
            if ( GetAttributeValue( "ShowLavaOutput" ).AsBoolean() )
            {
                string template = GetAttributeValue( "LavaOutput" );

                var mergeFields = new Dictionary<string, object>();
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
                linkedPages.Add( "GroupDetailPage", LinkedPageRoute( "GroupDetailPage" ) );

                if ( _targetPersonGuid != Guid.Empty )
                {
                    linkedPages.Add( "RegisterPage", LinkedPageUrl( "RegisterPage", _urlParms ) );
                }
                else
                {
                    linkedPages.Add( "RegisterPage", LinkedPageRoute( "RegisterPage" ) );
                }

                mergeFields.Add( "LinkedPages", linkedPages );
                mergeFields.Add( "CampusContext", RockPage.GetCurrentContext( EntityTypeCache.Get( "Rock.Model.Campus" ) ) as Campus );

                lLavaOverview.Text = template.ResolveMergeFields( mergeFields );

                bool showDebug = UserCanEdit && GetAttributeValue( "LavaOutputDebug" ).AsBoolean();
                lLavaOutputDebug.Visible = showDebug;
                if ( showDebug )
                {
                    lLavaOutputDebug.Text = mergeFields.lavaDebugInfo( null, "<span class='label label-info'>Lava used for the summary info.</span>" );
                }

                pnlLavaOutput.Visible = true;
            }
            else
            {
                pnlLavaOutput.Visible = false;
            }

            // Should a grid be displayed
            if ( GetAttributeValue( "ShowGrid" ).AsBoolean() )
            {
                pnlGrid.Visible = true;

                // Save the groups into the grid's object list since it is not being bound to actual group objects
                gGroups.ObjectList = new Dictionary<string, object>();
                groups.ForEach( g => gGroups.ObjectList.Add( g.Id.ToString(), g ) );

                // Bind the grid
                gGroups.DataSource = groups.Select( g =>
                {
                    var qryMembers = new GroupMemberService( rockContext ).Queryable().Where( a => a.GroupId == g.Id );
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
                        Schedule = g.Schedule,
                        MemberCount = qryMembers.Count(),
                        AverageAge = Math.Round( qryMembers.Select( m => m.Person.BirthDate ).ToList().Select( a => Person.GetAge( a ) ).Average() ?? 0.0D ),
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

            Guid? groupTypeGuid = GetAttributeValue( attributeName ).AsGuidOrNull();
            if ( groupTypeGuid.HasValue )
            {
                var groupType = groupTypes.FirstOrDefault( g => g.Guid.Equals( groupTypeGuid.Value ) );
                if ( groupType != null )
                {
                    control.SelectedGroupTypeId = groupType.Id;
                }
            }
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
        /// Gets the group type unique identifier.
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        private string GetGroupTypeGuid( int? groupTypeId )
        {
            if ( groupTypeId.HasValue )
            {
                var groupType = GroupTypeCache.Get( groupTypeId.Value );
                if ( groupType != null )
                {
                    return groupType.Guid.ToString();
                }
            }

            return string.Empty;
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
            lMapStyling.Text = string.Format( mapStylingFormat, GetAttributeValue( "MapHeight" ) );

            // add styling to map
            string styleCode = "null";
            var markerColors = new List<string>();

            DefinedValueCache dvcMapStyle = DefinedValueCache.Get( GetAttributeValue( "MapStyle" ).AsInteger() );
            if ( dvcMapStyle != null )
            {
                styleCode = dvcMapStyle.GetAttributeValue( "DynamicMapStyle" );
                markerColors = dvcMapStyle.GetAttributeValue( "Colors" )
                    .Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries )
                    .ToList();
                markerColors.ForEach( c => c = c.Replace( "#", string.Empty ) );
            }

            if ( !markerColors.Any() )
            {
                markerColors.Add( "FE7569" );
            }

            string locationColor = markerColors[0].Replace( "#", string.Empty );
            var polygonColorList = GetAttributeValue( "PolygonColors" ).Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
            string polygonColors = "\"" + polygonColorList.AsDelimited( "\", \"" ) + "\"";
            string groupColor = ( markerColors.Count > 1 ? markerColors[1] : markerColors[0] ).Replace( "#", string.Empty );

            string latitude = "39.8282";
            string longitude = "-98.5795";
            string zoom = "4";
            var orgLocation = GlobalAttributesCache.Get().OrganizationLocation;
            if ( orgLocation != null && orgLocation.GeoPoint != null )
            {
                latitude = orgLocation.GeoPoint.Latitude.ToString();
                longitude = orgLocation.GeoPoint.Longitude.ToString();
                zoom = "12";
            }

            // write script to page
            string mapScriptFormat = @"

        var locationData = {0};
        var fenceData = {1};
        var groupData = {2}; 

        var allMarkers = [];

        var map;
        var bounds = new google.maps.LatLngBounds();
        var infoWindow = new google.maps.InfoWindow();

        var mapStyle = {3};

        var polygonColorIndex = 0;
        var polygonColors = [{5}];

        var min = .999999;
        var max = 1.000001;

        initializeMap();

        function initializeMap() {{

            // Set default map options
            var mapOptions = {{
                 mapTypeId: 'roadmap'
                ,styles: mapStyle
                ,center: new google.maps.LatLng({7}, {8})
                ,zoom: {9}
            }};

            // Display a map on the page
            map = new google.maps.Map(document.getElementById('map_canvas'), mapOptions);
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
                    var items = addMapItem(i, groupData[i], '{6}');
                    for (var j = 0; j < items.length; j++) {{
                        items[j].setMap(map);
                    }}
                }}
            }}

            // adjust any markers that may overlap
            adjustOverlappedMarkers();

            if (!bounds.isEmpty()) {{
                map.fitBounds(bounds);
            }}

        }}

        function openInfoWindowById(id) {{
            marker = $.grep(allMarkers, function(m) {{ return m.id == id }})[0];
            openInfoWindow(marker);
        }}

        function openInfoWindow(marker) {{
            infoWindow.setContent( $('<div/>').html(marker.info_window).text() );
            infoWindow.open(map, marker);
        }}

        function addMapItem( i, mapItem, color ) {{

            var items = [];

            if (mapItem.Point) {{ 

                var position = new google.maps.LatLng(mapItem.Point.Latitude, mapItem.Point.Longitude);
                bounds.extend(position);

                if (!color) {{
                    color = 'FE7569'
                }}

                var pinImage = {{
                    path: 'M 0,0 C -2,-20 -10,-22 -10,-30 A 10,10 0 1,1 10,-30 C 10,-22 2,-20 0,0 z',
                    fillColor: '#' + color,
                    fillOpacity: 1,
                    strokeColor: '#000',
                    strokeWeight: 1,
                    scale: 1,
                    labelOrigin: new google.maps.Point(0,-28)
                }};

                marker = new google.maps.Marker({{
                    id: mapItem.EntityId,
                    position: position,
                    map: map,
                    title: htmlDecode(mapItem.Name),
                    icon: pinImage,
                    info_window: mapItem.InfoWindow,
                    label: String.fromCharCode(9679)
                }});
    
                items.push(marker);
                allMarkers.push(marker);

                if ( mapItem.InfoWindow != null ) {{ 
                    google.maps.event.addListener(marker, 'click', (function (marker, i) {{
                        return function () {{
                            openInfoWindow(marker);
                        }}
                    }})(marker, i));
                }}

                if ( mapItem.EntityId && mapItem.EntityId > 0 ) {{ 
                    google.maps.event.addListener(marker, 'mouseover', (function (marker, i) {{
                        return function () {{
                            $(""tr[datakey='"" + mapItem.EntityId + ""']"").addClass('row-highlight');
                        }}
                    }})(marker, i));

                    google.maps.event.addListener(marker, 'mouseout', (function (marker, i) {{
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
                            infoWindow.setContent( mapItem.InfoWindow );
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
                zoom );             // 9

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
    }
}

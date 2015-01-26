// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

    // Block Properties
    [LinkedPage( "Detail Page", "The page to navigate to for group details.", false, "", "", 0 )]
    [LinkedPage( "Register Page", "The page to navigate to when registering for a group.", false, "", "", 1 )]

    // Custom Settings
    [GroupTypeField( "Group Type", "The group type to limit selection to.", true, "", "CustomSetting" )]
    [AttributeField( Rock.SystemGuid.EntityType.GROUP, "Attribute Filters", "The group attributes to display as filters.", false, true, "", "CustomSetting" )]
    [AttributeField( Rock.SystemGuid.EntityType.GROUP, "Attributes in Grid", "The group attributes to display as a column in the results.", false, true, "", "CustomSetting" )]
    [BooleanField( "Show Count", "Should the group member count be displayed in list of groups?", false, "CustomSetting" )]
    [BooleanField( "Show Age", "Should the group member count be displayed in list of groups?", false, "CustomSetting" )]
    [BooleanField( "Show By Proximity", "Should an address input be displayed and the distance displayed to each group?", false, "CustomSetting" )]
    [GroupTypeField( "Geofenced Group Type", "The group type that should be used as a geofence limit when finding groups.", false, "", "CustomSetting" )]

    [BooleanField( "Show Map", "Should a map of the groups be displayed?", false, "CustomSetting" )]
    [BooleanField( "Show Fence", "Should geofence boundary be displayed on the map (Requires a Geofence Group Type)?", false, "CustomSetting" )]
    [IntegerField( "Map Height", "Height of the map in pixels (default value is 600px)", false, 600, "CustomSetting" )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.MAP_STYLES, "Map Style", "The map theme that should be used for styling the map.", true, false, Rock.SystemGuid.DefinedValue.MAP_STYLE_GOOGLE, "CustomSetting" )]
    [ValueListField( "Polygon Colors", "List of colors to use when displaying multiple fences (normally will only have one fence).", false, "#f37833,#446f7a,#afd074,#649dac,#f8eba2,#92d0df,#eaf7fc", "#ffffff", null, null, "CustomSetting" )]
    [CodeEditorField( "Map Info", "Lava template for the info window. To suppress the window provide a blank template.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 200, false, @"
<div style='width:250px'>

    <div class='clearfix'>
        <h4 class='pull-left' style='margin-top: 0;'>{{ GroupName }}</h4> 
        <span class='label label-campus pull-right'>{{ Campus.Name }}</span>
    </div>
    
    <div class='clearfix'>
		{% if Location.Address && Location.Address != '' %}
			<strong>{{ Location.Type }}</strong>
			<br>{{ Location.Address }}
		{% endif %}
		{% if Members.size > 0 %}
			<br>
			<br><strong>{{ GroupType.GroupMemberTerm }}s</strong><br>
			{% for GroupMember in Members -%}
				<div class='clearfix'>
					{% if GroupMember.PhotoUrl != '' %}
						<div class='pull-left' style='padding: 0 5px 2px 0'>
							<img src='{{ GroupMember.PhotoUrl }}&maxheight=50&maxwidth=50'>
						</div>
					{% endif %}
					<a href='{{ GroupMember.ProfilePageUrl }}'>{{ GroupMember.NickName }} {{ GroupMember.LastName }}</a> - {{ GroupMember.Role }}
                    {% if groupTypeGuid != '790E3215-3B10-442B-AF69-616C0DCB998E' and GroupMember.ConnectionStatus != '' %}
				        <br>{{ GroupMember.ConnectionStatus }}
					{% endif %}
					{% if GroupMember.Email != '' %}
						<br>{{ GroupMember.Email }}
					{% endif %}
					{% for Phone in GroupMember.Person.PhoneNumbers %}
						<br>{{ Phone.NumberTypeValue.Value }}: {{ Phone.NumberFormatted }}
					{% endfor %}
				</div>
				<br>
			{% endfor -%}
		{% endif %}
    </div>
    
    {% if GroupType.Guid != '790E3215-3B10-442B-AF69-616C0DCB998E' %}
		<br>
		<a class='btn btn-xs btn-action' href='{{ DetailPageUrl }}'>View {{ GroupType.GroupTerm }}</a>
		<a class='btn btn-xs btn-action' href='{{ RegisterPageUrl }}'>Register</a>
	{% endif %}

</div>
", "CustomSetting" )]

     public partial class GroupFinder : RockBlockCustomSettings
    {

        private List<AttributeCache> _attributeFilters = null;

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

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            BindAttributeFilters();

            gGroups.DataKeyNames = new string[] { "Id" };
            gGroups.Actions.ShowAdd = false;
            gGroups.GridRebind += gGroups_GridRebind;

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

            if ( !Page.IsPostBack )
            {
                ShowView();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the ContentDynamic control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void Block_Updated( object sender, EventArgs e )
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
            BindGroupAttributeList();
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
            SetAttributeValue( "ShowByProximity", cbProximity.Checked.ToString() );
            SetAttributeValue( "AttributeFilters", cblAttributes.Items.Cast<ListItem>().Where( i => i.Selected ).Select( i => i.Value ).ToList().AsDelimited( "," ) );
            SetAttributeValue( "ShowCount", cbShowCount.Checked.ToString() );
            SetAttributeValue( "ShowAge", cbShowAge.Checked.ToString() );
            SetAttributeValue( "GridAttributes", cblGridAttributes.Items.Cast<ListItem>().Where( i => i.Selected ).Select( i => i.Value ).ToList().AsDelimited( "," ) );
            SetAttributeValue( "ShowMap", cbShowMap.Checked.ToString() );
            SetAttributeValue( "MapHeight", nbMapHeight.Text );
            SetAttributeValue( "MapStyle", ddlMapStyle.SelectedValue );
            SetAttributeValue( "ShowFence", cbShowFence.Checked.ToString() );
            SetAttributeValue( "PolygonColors", vlPolygonColors.Value );
            SetAttributeValue( "MapInfo", ceMapInfo.Text );
            SaveAttributeValues();

            mdEdit.Hide();
            pnlEditModal.Visible = false;
            upnlContent.Update();

            BindAttributeFilters();
            ShowView();
        }

        /// <summary>
        /// Handles the Click event of the btnSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSearch_Click( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowSelected event of the gGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gGroups_RowSelected( object sender, RowEventArgs e )
        {

        }

        /// <summary>
        /// Handles the GridRebind event of the gGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void gGroups_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
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

            BindGroupAttributeList();

            var rockContext = new RockContext();
            var groupTypes = new GroupTypeService( rockContext )
                .Queryable().AsNoTracking().ToList();

            BindGroupType( gtpGroupType, groupTypes, "GroupType" );
            BindGroupType( gtpGeofenceGroupType, groupTypes, "GeofencedGroupType" );
            cbProximity.Checked = GetAttributeValue( "ShowByProximity" ).AsBoolean();
            foreach ( string attr in GetAttributeValue( "AttributeFilters" ).SplitDelimitedValues() )
            {
                var li = cblAttributes.Items.FindByValue( attr );
                if ( li != null )
                {
                    li.Selected = true;
                }
            }

            cbShowCount.Checked = GetAttributeValue( "ShowCount" ).AsBoolean();
            cbShowAge.Checked = GetAttributeValue( "ShowAge" ).AsBoolean();
            foreach ( string attr in GetAttributeValue( "GridAttributes" ).SplitDelimitedValues() )
            {
                var li = cblGridAttributes.Items.FindByValue( attr );
                if ( li != null )
                {
                    li.Selected = true;
                }
            }

            ddlMapStyle.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.MAP_STYLES.AsGuid() ) );

            cbShowMap.Checked = GetAttributeValue( "ShowMap" ).AsBoolean();
            nbMapHeight.Text = GetAttributeValue( "MapHeight" );
            ddlMapStyle.SetValue( GetAttributeValue( "MapStyle" ) );
            cbShowFence.Checked = GetAttributeValue( "ShowFence" ).AsBoolean();
            vlPolygonColors.Value = GetAttributeValue( "PolygonColors" );
            ceMapInfo.Text = GetAttributeValue( "MapInfo" );

            upnlContent.Update();
        }

        /// <summary>
        /// Binds the group attribute list.
        /// </summary>
        private void BindGroupAttributeList()
        {
            var group = new Group();
            group.GroupTypeId = gtpGroupType.SelectedGroupTypeId ?? 0;
            group.LoadAttributes();

            cblAttributes.Items.Clear();
            cblGridAttributes.Items.Clear();
            foreach ( var attribute in group.Attributes )
            {
                cblAttributes.Items.Add( new ListItem( attribute.Value.Name, attribute.Value.Guid.ToString() ) );
                cblGridAttributes.Items.Add( new ListItem( attribute.Value.Name, attribute.Value.Guid.ToString() ) );
            }

            cblAttributes.Visible = cblAttributes.Items.Count > 0;
            cblGridAttributes.Visible = cblAttributes.Items.Count > 0;
        }

        /// <summary>
        /// Shows the view.
        /// </summary>
        private void ShowView()
        {
            bool useProximity = GetAttributeValue( "ShowByProximity" ).AsBoolean();

            acAddress.Visible = useProximity;

            if ( pnlResults.Visible )
            {
                BindGrid();
            }
        }

        /// <summary>
        /// Adds the attribute filters.
        /// </summary>
        private void BindAttributeFilters()
        {
            _attributeFilters = new List<AttributeCache>();
            foreach ( string attr in GetAttributeValue( "Attributes" ).SplitDelimitedValues() )
            {
                Guid? attributeGuid = attr.AsGuidOrNull();
                if ( attributeGuid.HasValue )
                {
                    var attribute = AttributeCache.Read( attributeGuid.Value );
                    if ( attribute != null )
                    {
                        _attributeFilters.Add( attribute );
                    }
                }
            }
            
            phAttributeFilters.Controls.Clear();

            foreach ( var attribute in _attributeFilters )
            {
                var control = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filter_" + attribute.Id.ToString() );
                if ( control is IRockControl )
                {
                    var rockControl = (IRockControl)control;
                    rockControl.Label = attribute.Name;
                    rockControl.Help = attribute.Description;
                    phAttributeFilters.Controls.Add( control );
                }
                else
                {
                    var wrapper = new RockControlWrapper();
                    wrapper.ID = control.ID + "_wrapper";
                    wrapper.Label = attribute.Name;
                    wrapper.Controls.Add( control );
                    phAttributeFilters.Controls.Add( wrapper );
                }
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            Guid? groupTypeGuid = GetAttributeValue( "GroupType" ).AsGuidOrNull();
            if ( groupTypeGuid.HasValue )
            {
                bool useProximity = GetAttributeValue( "ShowByProximity" ).AsBoolean();
                
                gGroups.Columns[2].Visible = GetAttributeValue( "ShowCount" ).AsBoolean();
                gGroups.Columns[3].Visible = GetAttributeValue( "ShowAge" ).AsBoolean();
                gGroups.Columns[4].Visible = useProximity;  // Distance

                // Build attribute columns
                foreach ( var column in gGroups.Columns.OfType<AttributeField>().ToList() )
                {
                    gGroups.Columns.Remove( column );
                }
                foreach ( string attr in GetAttributeValue( "GridAttributes" ).SplitDelimitedValues() )
                {
                    Guid? attributeGuid = attr.AsGuidOrNull();
                    if ( attributeGuid.HasValue )
                    {
                        var attribute = AttributeCache.Read( attributeGuid.Value );
                        if ( attribute != null )
                        {
                            string dataFieldExpression = attribute.Key;
                            bool columnExists = gGroups.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                            if ( !columnExists )
                            {
                                AttributeField boundField = new AttributeField();
                                boundField.DataField = dataFieldExpression;
                                boundField.HeaderText = attribute.Name;
                                boundField.SortExpression = string.Empty;

                                var attributeCache = Rock.Web.Cache.AttributeCache.Read( attribute.Id );
                                if ( attributeCache != null )
                                {
                                    boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                                }

                                gGroups.Columns.Add( boundField );
                            }
                        }
                    }
                }

                // Get query of groups of the selected group type
                var rockContext = new RockContext();
                var groupQry = new GroupService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( g => g.GroupType.Guid.Equals( groupTypeGuid.Value ) );

                // Filter query by any configured attribute filters
                if ( _attributeFilters.Any() )
                {
                    var attributeValueService = new AttributeValueService( rockContext );
                    var parameterExpression = attributeValueService.ParameterExpression;

                    foreach ( var attribute in _attributeFilters )
                    {
                        var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                        if ( filterControl != null )
                        {
                            var filterValues = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues );
                            var expression = attribute.FieldType.Field.FilterExpression( attributeValueService, parameterExpression, "Value", filterValues );
                            if ( expression != null )
                            {
                                var attributeValues = attributeValueService
                                    .Queryable().AsNoTracking()
                                    .Where( v => v.Attribute.Id == attribute.Id );

                                attributeValues = attributeValues.Where( parameterExpression, expression, null );

                                groupQry = groupQry.Where( w => attributeValues.Select( v => v.EntityId ).Contains( w.Id ) );
                            }
                        }
                    }
                }

                // If filtering by proximity and a valid geofence group type was specified, limit the groups to those inside
                // the fence that the filtered address belongs to
                bool showMap = GetAttributeValue( "ShowMap" ).AsBoolean();
                bool showFences = showMap && GetAttributeValue( "ShowFence" ).AsBoolean();
                Location location = null;
                var mapFences = new List<MapItem>();
                var groupLocations = new Dictionary<int, Location>();
                var distances = new Dictionary<int, double>();
                if ( useProximity )
                {
                    int? fenceGroupTypeId = GetGroupTypeId( GetAttributeValue( "GeofencedGroupType" ).AsGuidOrNull() );
                    if ( fenceGroupTypeId.HasValue )
                    {
                        // Find the location of the address entered
                        location = new LocationService( rockContext )
                            .Get( acAddress.Street1, acAddress.Street2, acAddress.City,
                                acAddress.State, acAddress.PostalCode, acAddress.Country );
                        if ( location.GeoPoint != null )
                        {
                            // If address was valid, find the fenced group types that include that location (normally would only be one)
                            var groupLocationService = new GroupLocationService( rockContext );
                            foreach ( var fence in groupLocationService
                                .Queryable("Group,Location").AsNoTracking()
                                .Where( gl =>
                                    gl.Group.GroupTypeId == fenceGroupTypeId &&
                                    gl.Location.GeoFence != null &&
                                    location.GeoPoint.Intersects( gl.Location.GeoFence ) )
                                .ToList() )
                            {
                                // Save the fence boundary for displaying on map
                                if ( showFences )
                                {
                                    var mapItem = new MapItem( fence.Location );
                                    mapItem.EntityTypeId = EntityTypeCache.Read( "Rock.Model.Group" ).Id;
                                    mapItem.EntityId = fence.GroupId;
                                    mapItem.Name = fence.Group.Name;
                                    mapFences.Add( mapItem );
                                }

                                // For each fenced group, find the groups inside that fence
                                foreach ( var groupLocation in groupLocationService
                                    .Queryable( "Group,Location" ).AsNoTracking()
                                    .Where( gl =>
                                        gl.Group.GroupType.Guid.Equals( groupTypeGuid.Value ) &&
                                        gl.Location.GeoPoint != null &&
                                        gl.Location.GeoPoint.Intersects( fence.Location.GeoFence ) ) )
                                {
                                    // If this group has already been added, see if this location is closer and if so, use it instead
                                    double meters = groupLocation.Location.GeoPoint.Distance(location.GeoPoint) ?? 0.0D;
                                    double miles = meters / 1609.344;

                                    if ( distances.ContainsKey( groupLocation.GroupId ))
                                    {
                                        if ( distances[groupLocation.GroupId] < miles )
                                        {
                                            distances[groupLocation.GroupId] = miles;
                                            groupLocations.AddOrReplace(groupLocation.GroupId, groupLocation.Location);
                                        }
                                    }
                                    else
                                    {
                                        distances.Add( groupLocation.GroupId, miles );
                                        groupLocations.AddOrReplace(groupLocation.GroupId, groupLocation.Location);
                                    }
                                }
                            }
                        }
                        groupQry = groupQry.Where( g => groupLocations.Keys.Contains( g.Id ) );
                    }
                }

                // Run query to get list of matching groups
                List<Group> groups = null;
                SortProperty sortProperty = gGroups.SortProperty;
                if ( sortProperty != null )
                {
                    groups = groupQry.Sort( sortProperty ).ToList();
                }
                else
                {
                    groups = groupQry.OrderBy( g => g.Name ).ToList();
                }

                if ( showMap )
                {
                    var mapGroups = new List<MapItem>();
                    foreach( var gl in groupLocations )
                    {
                        var group = groups.Where( g => g.Id == gl.Key ).FirstOrDefault();
                        if (group != null)
                        {
                            var mapItem = new MapItem( gl.Value );
                            mapItem.EntityTypeId = EntityTypeCache.Read( "Rock.Model.Group" ).Id;
                            mapItem.EntityId = group.Id;
                            mapItem.Name = group.Name;
                            mapGroups.Add(mapItem);
                        }
                    }

                    Map( location, mapFences, mapGroups );
                }
                else
                {
                    pnlMap.Visible = false;
                }

                gGroups.ObjectList = new Dictionary<string, object>();
                foreach( var group in groups )
                { 
                    gGroups.ObjectList.Add( group.Id.ToString(), group );
                }

                gGroups.DataSource = groups.Select( g => new
                {
                    Id = g.Id,
                    Name = g.Name,
                    GroupTypeName = g.GroupType.Name,
                    GroupOrder = g.Order,
                    GroupTypeOrder = g.GroupType.Order,
                    Description = g.Description,
                    IsSystem = g.IsSystem,
                    IsActive = g.IsActive,
                    GroupRole = string.Empty,
                    DateAdded = DateTime.MinValue,
                    MemberCount = g.Members.Count(),
                    AverageAge = g.Members.Select( m => m.Person ).Average( p => p.Age ),
                    Distance = distances.Where( d => d.Key == g.Id)
                        .Select( d => d.Value ).FirstOrDefault()
                } ).ToList();
                gGroups.DataBind();

                pnlResults.Visible = true;
            }
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
                var groupType = groupTypes.FirstOrDefault( g => g.Guid.Equals(groupTypeGuid.Value));
                if ( groupType != null )
                {
                    control.SelectedGroupTypeId = groupType.Id;
                }
            }
        }

        private int? GetGroupTypeId ( Guid? groupTypeGuid )
        {
            if ( groupTypeGuid.HasValue )
            {
                var groupType = GroupTypeCache.Read( groupTypeGuid.Value );
                if ( groupType != null )
                {
                    return  groupType.Id;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the group type unique identifier.
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        private string GetGroupTypeGuid ( int? groupTypeId )
        {
            if ( groupTypeId.HasValue )
            {
                var groupType = GroupTypeCache.Read( groupTypeId.Value );
                if ( groupType != null )
                {
                    return  groupType.Guid.ToString();
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
        private void Map( Location location, List<MapItem> fences, List<MapItem> groups )
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
                                border-radius: 8px;
                            }}
                        </style>";
            lMapStyling.Text = string.Format( mapStylingFormat, GetAttributeValue( "MapHeight" ) );

            // add styling to map
            string styleCode = "null";
            var markerColors = new List<string>();

            DefinedValueCache dvcMapStyle = DefinedValueCache.Read( GetAttributeValue( "MapStyle" ).AsGuid() );
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
            string groupColor = ( markerColors.Count > 1 ? markerColors[1] : markerColors[0] ).Replace( "#", string.Empty );
            var polygonColorList = GetAttributeValue( "PolygonColors" ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
            string polygonColors = "\"" + polygonColorList.AsDelimited( "\", \"" ) + "\"";

            string template = HttpUtility.HtmlEncode( GetAttributeValue( "MapInfo" ).Replace( Environment.NewLine, string.Empty ).Replace( "\n", string.Empty ) );
            string groupPage = GetAttributeValue( "DetailPage" );
            string registerPage = GetAttributeValue( "RegisterPage" );
            string infoWindowJson = string.Format( @"{{ ""GroupPage"":""{0}"", ""RegisterPage"":""{1}"", ""Template"":""{2}"" }}",
                groupPage, registerPage, template );

            string latitude = "39.8282";
            string longitude = "-98.5795";
            string zoom = "4";
            var orgLocation = GlobalAttributesCache.Read().OrganizationLocation;
            if ( orgLocation != null && orgLocation.GeoPoint != null )
            {
                latitude = orgLocation.GeoPoint.Latitude.ToString();
                longitude = orgLocation.GeoPoint.Longitude.ToString();
                zoom = "12";
            }

            // write script to page
            string mapScriptFormat = @"

        var locationData = JSON.
        var fenceData = JSON.parse('{0}'
        var groupData = JSON.parse('{0}'); 
        var allMarkers = [];
        var groupItems = [];

        var map;
        var bounds = new google.maps.LatLngBounds();
        var infoWindow = new google.maps.InfoWindow();

        var mapStyle = {1};

        var pinShadow = new google.maps.MarkerImage('//chart.apis.google.com/chart?chst=d_map_pin_shadow',
            new google.maps.Size(40, 37),
            new google.maps.Point(0, 0),
            new google.maps.Point(12, 35));

        var polygonColorIndex = 0;
        var polygonColors = [{2}];

        var infoWindowRequest = {3};

        var min = .999999;
        var max = 1.000001;

        initializeMap();

        function initializeMap() {{

            debugger;

            // Set default map options
            var mapOptions = {{
                 mapTypeId: 'roadmap'
                ,styles: mapStyle
                ,center: new google.maps.LatLng({4}, {5})
                ,zoom: {6}
            }};

            // Display a map on the page
            map = new google.maps.Map(document.getElementById('map_canvas'), mapOptions);
            map.setTilt(45);

            for (var i = 0; i < fenceData.length; i++) {{
                var items = addMapItem(i, groupData[i], '{2}');
                for (var j = 0; j < items.length; j++) {{
                    items[j].setMap(map);
                    groupItems.push(items[j]);
                }}
            }}

            for (var i = 0; i < groupData.length; i++) {{
                var items = addMapItem(i, groupData[i], '{2}');
                for (var j = 0; j < items.length; j++) {{
                    items[j].setMap(map);
                    groupItems.push(items[j]);
                }}
            }}

            // adjust any markers that may overlap
            adjustOverlappedMarkers();

            if (!bounds.isEmpty()) {{
                map.fitBounds(bounds);
            }}

        }}

        function addMapItem( i, mapItem, color ) {{

            var items = [];

            if (mapItem.Point) {{ 

                var position = new google.maps.LatLng(mapItem.Point.Latitude, mapItem.Point.Longitude);
                bounds.extend(position);

                if (!color) {{
                    color = 'FE7569'
                }}

                var pinImage = new google.maps.MarkerImage('http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=%E2%80%A2|' + color,
                    new google.maps.Size(21, 34),
                    new google.maps.Point(0,0),
                    new google.maps.Point(10, 34));

                marker = new google.maps.Marker({{
                    position: position,
                    map: map,
                    title: htmlDecode(mapItem.Name),
                    icon: pinImage,
                    shadow: pinShadow
                }});
    
                items.push(marker);
                allMarkers.push(marker);

                google.maps.event.addListener(marker, 'click', (function (marker, i) {{
                    return function () {{
                        $.post( Rock.settings.get('baseUrl') + 'api/Groups/GetMapInfoWindow/' + mapItem.EntityId + '/' + mapItem.LocationId, infoWindowRequest, function( data ) {{
                            infoWindow.setContent( data.Result );
                            infoWindow.open(map, marker);
                        }});
                    }}
                }})(marker, i));

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

            var groupJson = groups.ToJson().Replace( Environment.NewLine, "" ).Replace( "\x0A", "" );
            string mapScript = string.Format( mapScriptFormat,
                groupJson, styleCode, groupColor, infoWindowJson, latitude, longitude, zoom );

            ScriptManager.RegisterStartupScript( pnlMap, pnlMap.GetType(), "group-finder-map-script", mapScript, true );

        }

        #endregion

}

}
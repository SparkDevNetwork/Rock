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
    [AttributeField( Rock.SystemGuid.EntityType.GROUP, "Attributes", "The group attributes to display as filters.", false, true, "", "CustomSetting" )]
    [BooleanField( "Show Count", "Should the group member count be displayed in list of groups?", false, "CustomSetting" )]
    [BooleanField( "Show Age", "Should the group member count be displayed in list of groups?", false, "CustomSetting" )]
    [BooleanField( "Show Map", "Should a map of the groups be displayed?", false, "CustomSetting" )]
    [IntegerField( "Map Height", "The height of the map.", false, 400, "CustomSetting" )]
    [CodeEditorField( "Map Info Lava", "The lava template to use for formatting the group information in the map info window.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 200, false, "", "CustomSetting" )]
    [BooleanField( "Show By Proximity", "Should an address input be displayed and the distance displayed to each group?", false, "CustomSetting" )]
    [GroupTypeField( "Geofenced Group Type", "The group type that should be used as a geofence limit when finding groups.", false, "", "CustomSetting" )]

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

            AddAttributeFilters();

            gGroups.DataKeyNames = new string[] { "Id" };
            gGroups.Actions.ShowAdd = false;
            gGroups.GridRebind += gGroups_GridRebind;

            this.BlockUpdated += Block_Updated;
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
            SetAttributeValue( "Attributes", cblAttributes.Items.Cast<ListItem>().Where( i => i.Selected ).Select( i => i.Value ).ToList().AsDelimited( "," ) );
            SetAttributeValue( "ShowCount", cbShowCount.Checked.ToString() );
            SetAttributeValue( "ShowAge", cbShowAge.Checked.ToString() );
            SetAttributeValue( "ShowMap", cbShowMap.Checked.ToString() );
            SetAttributeValue( "MapHeight", nbMapHeight.Text );
            SetAttributeValue( "MapInfoLava", ceMapInfo.Text );
            SetAttributeValue( "ShowByProximity", cbProximity.Checked.ToString() );
            SetAttributeValue( "GeofencedGroupType", GetGroupTypeGuid( gtpGeofenceGroupType.SelectedGroupTypeId ) );
            SaveAttributeValues();

            mdEdit.Hide();
            pnlEditModal.Visible = false;
            upnlContent.Update();

            ShowView();
        }

        protected void btnSearch_Click( object sender, EventArgs e )
        {
            BindGrid();
        }

        protected void gGroups_RowSelected( object sender, RowEventArgs e )
        {

        }
        
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

            var rockContext = new RockContext();
            var groupTypes = new GroupTypeService( rockContext )
                .Queryable().AsNoTracking().ToList();
            BindGroupType( gtpGroupType, groupTypes, "GroupType" );
            BindGroupType( gtpGeofenceGroupType, groupTypes, "GeofencedGroupType" );

            BindGroupAttributeList();
            foreach ( string attr in GetAttributeValue( "Attributes" ).SplitDelimitedValues() )
            {
                var li = cblAttributes.Items.FindByValue( attr );
                if ( li != null )
                {
                    li.Selected = true;
                }
            }

            cbShowCount.Checked = GetAttributeValue( "ShowCount" ).AsBoolean();
            cbShowAge.Checked = GetAttributeValue( "ShowAge" ).AsBoolean();
            cbShowMap.Checked = GetAttributeValue( "ShowMap" ).AsBoolean();
            nbMapHeight.Text = GetAttributeValue( "MapHeight" );
            ceMapInfo.Text = GetAttributeValue( "MapInfoLava" );
            cbProximity.Checked = GetAttributeValue( "ShowByProximity" ).AsBoolean();

            upnlContent.Update();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void ShowView()
        {
            bool useProximity = GetAttributeValue( "ShowByProximity" ).AsBoolean();

            acAddress.Visible = useProximity;

            if ( pnlGridResults.Visible )
            {
                BindGrid();
            }
        }

        /// <summary>
        /// Adds the attribute filters.
        /// </summary>
        private void AddAttributeFilters()
        {
            phAttributeFilters.Controls.Clear();
            foreach ( var column in gGroups.Columns.OfType<AttributeField>().ToList() )
            {
                gGroups.Columns.Remove( column );
            }

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

                // Add attribute columns
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


        /// <summary>
        /// Binds the group attribute list.
        /// </summary>
        private void BindGroupAttributeList()
        {
            var group = new Group();
            group.GroupTypeId = gtpGroupType.SelectedGroupTypeId ?? 0;
            group.LoadAttributes();

            cblAttributes.Items.Clear();
            foreach ( var attribute in group.Attributes )
            {
                cblAttributes.Items.Add( new ListItem( attribute.Value.Name, attribute.Value.Guid.ToString() ) );
            }

            cblAttributes.Visible = cblAttributes.Items.Count > 0;
        }

        private void BindGrid()
        {
            var rockContext = new RockContext();

            bool useProximity = GetAttributeValue( "ShowByProximity" ).AsBoolean();
                
            gGroups.Columns[2].Visible = GetAttributeValue( "ShowCount" ).AsBoolean();
            gGroups.Columns[3].Visible = GetAttributeValue( "ShowAge" ).AsBoolean();
            gGroups.Columns[4].Visible = useProximity;

            Guid? groupTypeGuid = GetAttributeValue( "GroupType" ).AsGuidOrNull();
            if ( groupTypeGuid.HasValue )
            {
                var groupQry = new GroupService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( g => g.GroupType.Guid.Equals( groupTypeGuid.Value ) );

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

                var groupLocations = new Dictionary<int, Location>();
                var distances = new Dictionary<int, double>();
                if ( useProximity )
                {
                    int? fenceGroupTypeId = GetGroupTypeId( GetAttributeValue( "GeofencedGroupType" ).AsGuidOrNull() );
                    if ( fenceGroupTypeId.HasValue )
                    {
                        var location = new LocationService( rockContext )
                            .Get( acAddress.Street1, acAddress.Street2, acAddress.City,
                                acAddress.State, acAddress.PostalCode, acAddress.Country );
                        if ( location.GeoPoint != null )
                        {
                            var groupLocationService = new GroupLocationService( rockContext );
                            foreach ( var fence in groupLocationService
                                .Queryable().AsNoTracking()
                                .Where( gl =>
                                    gl.Group.GroupTypeId == fenceGroupTypeId &&
                                    gl.Location.GeoFence != null &&
                                    location.GeoPoint.Intersects( gl.Location.GeoFence ) )
                                .Select( gl => gl.Location.GeoFence )
                                .ToList() )
                            {
                                foreach ( var groupLocation in groupLocationService
                                    .Queryable( "Group,Location" ).AsNoTracking()
                                    .Where( gl =>
                                        gl.Group.GroupType.Guid.Equals( groupTypeGuid ) &&
                                        gl.Location.GeoPoint != null &&
                                        gl.Location.GeoPoint.Intersects( fence ) ) )
                                {
                                    // If this group has already been added, see if this location is closer and if so, use it instead
                                    double distance = groupLocation.Location.GeoPoint.Distance(location.GeoPoint) ?? 0.0D;
                                    if ( distances.ContainsKey( groupLocation.GroupId ))
                                    {
                                        if ( distances[groupLocation.GroupId] < distance )
                                        {
                                            distances[groupLocation.GroupId] = distance;
                                            groupLocations.AddOrReplace(groupLocation.GroupId, groupLocation.Location);
                                        }
                                    }
                                    else
                                    {
                                        distances.Add( groupLocation.GroupId, distance);
                                        groupLocations.AddOrReplace(groupLocation.GroupId, groupLocation.Location);
                                    }
                                }
                            }
                        }
                    }
                    groupQry = groupQry.Where( g => groupLocations.Keys.Contains( g.Id ));
                }

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

                pnlGridResults.Visible = true;
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

        #endregion

}

}
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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Plugins.church_ccv.Groups
{
    [DisplayName( "Group Region Utility" )]
    [Category( "CCV > Groups" )]
    [Description( "Reorganizes groups to be under the appropriate parent group based on GeoLocation" )]
    public partial class GroupRegionUtility : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            var allGroupTypes = new GroupTypeService( new RockContext() ).Queryable().OrderBy( a => a.Name ).ToList();
            gtpGroupType.GroupTypes = allGroupTypes.Where( a => a.Name.Contains( "Group" ) ).ToList();
            gtpGeofencingGroupType.GroupTypes = allGroupTypes.Where( a => a.Name.Contains( "Area" ) ).ToList();
            gtpParentGroupType.GroupTypes = allGroupTypes;
            gtpParentForDelete.GroupTypes = allGroupTypes;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !this.IsPostBack )
            {
                var currentGroupId = this.PageParameter( "GroupId" ).AsIntegerOrNull();
                var currentGroup = new GroupService( new RockContext() ).Get( currentGroupId ?? 0 );
                gpCurrentParentArea.SetValue( currentGroup );
                if ( currentGroup != null )
                {
                    gpNewParentArea.RootGroupId = currentGroup.ParentGroupId;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnRun control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRun_Click( object sender, EventArgs e )
        {
            ReorganizeGroups( true );
        }

        /// <summary>
        /// Reorganizes the groups.
        /// </summary>
        /// <param name="preview">if set to <c>true</c> [preview].</param>
        private void ReorganizeGroups( bool preview )
        {
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );

            var currentGroups = groupService.GetAllDescendents( gpCurrentParentArea.SelectedValue.AsInteger() );
            var newParentGroups = groupService.GetAllDescendents( gpNewParentArea.SelectedValue.AsInteger() );
            var groupTypeId = gtpGroupType.SelectedGroupTypeId ?? 0;
            var geofencingGroupTypeId = gtpGeofencingGroupType.SelectedGroupTypeId ?? 0;
            var parentGroupTypeId = gtpParentGroupType.SelectedGroupTypeId ?? 0;
            var groupList = currentGroups.Where( a => a.GroupTypeId == groupTypeId ).ToList();

            var newGeoFenceGroups = newParentGroups.Where( a => a.GroupTypeId == geofencingGroupTypeId );
            var result = new Dictionary<Group, Group>();
            foreach ( var group in groupList )
            {
                var groupLocation = group.GroupLocations.FirstOrDefault();
                if ( groupLocation != null && groupLocation.Location.GeoPoint != null )
                {
                    foreach ( var geofenceGroup in newGeoFenceGroups )
                    {
                        var geoFenceLocation = geofenceGroup.GroupLocations.FirstOrDefault();
                        if ( geoFenceLocation != null && geoFenceLocation.Location.GeoFence != null )
                        {
                            if ( groupLocation.Location.GeoPoint.Intersects( geoFenceLocation.Location.GeoFence ) )
                            {
                                result.Add( group, geofenceGroup );
                            }
                        }
                    }
                }
            }

            var mappedList = result.Select( a => new
            {
                Id = a.Key.Id,
                GroupId = a.Key.Id,
                GroupName = a.Key.Name,
                CurrentParentRegion = this.FirstParentGroupOfType(a.Key, geofencingGroupTypeId),
                CurrentParentGroup = this.FirstParentGroupOfType( a.Key, parentGroupTypeId ),
                NewParentRegion = a.Value,
                NewParentGroup = groupService.GetAllDescendents( a.Value.Id ).Where( x => x.GroupTypeId == parentGroupTypeId ).FirstOrDefault(),
                RegionChanged =  this.FirstParentGroupOfType(a.Key, geofencingGroupTypeId).Id != a.Value.Id
            } ).ToList();

            mappedList = mappedList.OrderByDescending( a => a.RegionChanged ).ToList();

            grdPreview.DataSource = mappedList;

            grdPreview.DataBind();

            if ( preview == false )
            {
                foreach ( var item in mappedList )
                {
                    var group = groupService.Get( item.GroupId );
                    if ( group.ParentGroupId != item.NewParentGroup.Id )
                    {
                        group.ParentGroupId = item.NewParentGroup.Id;
                        rockContext.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// Firsts the type of the parent group of.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="parentGroupTypeId">The parent group type identifier.</param>
        /// <returns></returns>
        private Group FirstParentGroupOfType( Group group, int parentGroupTypeId )
        {
            var parentGroup = group.ParentGroup;
            while (parentGroup != null && parentGroup.GroupTypeId != parentGroupTypeId)
            {
                parentGroup = parentGroup.ParentGroup;
            }

            return parentGroup;
        }

        /// <summary>
        /// Handles the Click event of the btnApply control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnApply_Click( object sender, EventArgs e )
        {
            ReorganizeGroups( false );
        }

        /// <summary>
        /// Handles the Click event of the btnDeleteIfNoChildGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDeleteIfNoChildGroups_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );

            var parentGroupTypeId = gtpParentForDelete.SelectedGroupTypeId ?? 0;
            var childGroups = groupService.GetAllDescendents(gpCurrentParentArea.SelectedValue.AsInteger()).Where(a => a.GroupTypeId == parentGroupTypeId).ToList();
            foreach( var childGroup in childGroups)
            {
                if (!groupService.GetAllDescendents(childGroup.Id).Any())
                {
                    var errorMessage = string.Empty;
                    if (groupService.CanDelete(childGroup, out errorMessage))
                    {
                        groupService.Delete( childGroup );
                    }
                }
            }
            

            rockContext.SaveChanges();
        }
    }
}
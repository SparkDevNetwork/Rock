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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;

namespace RockWeb.Blocks.CheckIn
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Manager" )]
    [Category( "Check-in" )]
    [Description( "Block used to view current check-in counts and locations." )]

    [GroupTypeField( "Check-in Type", key: "GroupTypeTemplate", groupTypePurposeValueGuid: Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE )]
    public partial class Manager : Rock.Web.UI.RockBlock
    {
        #region Fields

        public string CurrentNavItem { get; set; }
        public NavigationData NavData { get; set; }

        #endregion

        #region Properties
        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            CurrentNavItem = ViewState["CurrentNavItem"] as string;
            NavData = ViewState["NavData"] as NavigationData;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

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

            nbGroupTypeWarning.Visible = false;

            if ( !Page.IsPostBack )
            {
                NavData = GetNavigationData();

                CurrentNavItem = "L";   // TODO get from user preference
                BuildNavigationControls();
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
            ViewState["NavData"] = NavData;
            ViewState["CurrentNavItem"] = CurrentNavItem;
            return base.SaveViewState();
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
        }

        protected void lbNavigate_Click( object sender, EventArgs e )
        {
            var lb = sender as LinkButton;
            if (lb != null)
            {
                CurrentNavItem = lb.CommandArgument;
                BuildNavigationControls();
            }
        }

        #endregion

        #region Methods

        #region Get Navigation Data

        private NavigationData GetNavigationData()
        {
            var groupTypeTemplateGuid = this.GetAttributeValue( "GroupTypeTemplate" ).AsGuidOrNull();
            if ( groupTypeTemplateGuid.HasValue )
            {
                NavData = new NavigationData();

                var rockContext = new RockContext();

                // Get the group types
                AddGroupType( GroupTypeCache.Read( groupTypeTemplateGuid.Value ) );

                // Get the groups
                var groupTypeIds = NavData.GroupTypes.Select( t => t.Id ).ToList();
                foreach ( var group in new GroupService( rockContext ).Queryable()
                    .Where( g =>
                        groupTypeIds.Contains( g.GroupTypeId ) &&
                        g.IsActive ) )
                {
                    if ( group.GroupLocations.Any() )
                    {
                        var navGroup = new NavigationGroup( group );
                        navGroup.ChildLocationIds = group.GroupLocations.Select( g => g.LocationId ).ToList();
                        NavData.Groups.Add( navGroup );
                        NavData.GroupTypes.Where( g => g.Id == group.GroupTypeId ).ToList()
                            .ForEach( g => g.ChildGroupIds.Add( group.Id ) );
                    }
                }

                // Remove any grouptype trees without locations
                var emptyGroupTypeIds = NavData.GroupTypes
                    .Where( g => !g.ChildGroupIds.Any() && !g.ChildGroupTypeIds.Any() )
                    .Select( g => g.Id )
                    .ToList();
                while ( emptyGroupTypeIds.Any() )
                {
                    NavData.GroupTypes = NavData.GroupTypes.Where( g => !emptyGroupTypeIds.Contains( g.Id ) ).ToList();
                    NavData.GroupTypes.ForEach( g =>
                        g.ChildGroupTypeIds = g.ChildGroupTypeIds.Where( c => !emptyGroupTypeIds.Contains( c ) ).ToList() );

                    emptyGroupTypeIds = NavData.GroupTypes
                        .Where( g => !g.ChildGroupIds.Any() && !g.ChildGroupTypeIds.Any() )
                        .Select( g => g.Id )
                        .ToList();
                }


                // Get the locations
                var locationIds = NavData.Groups.SelectMany( g => g.ChildLocationIds ).Distinct().ToList();
                foreach ( var location in new LocationService( rockContext ).Queryable( "ParentLocation" )
                    .Where( l => locationIds.Contains( l.Id ) ) )
                {
                    var navLocation = AddLocation( location );
                    navLocation.HasGroups = true;
                }

                // Get the active schedules
                var activeSchedules = new List<int>();
                foreach ( var schedule in new ScheduleService( rockContext ).Queryable()
                    .Where( s => s.CheckInStartOffsetMinutes.HasValue ) )
                {
                    if ( schedule.IsScheduleActive || schedule.IsCheckInActive )
                    {
                        activeSchedules.Add( schedule.Id );
                    }
                }

                // Get the attendance counts
                var dayStart = RockDateTime.Today;
                var now = RockDateTime.Now;
                var groupIds = NavData.Groups.Select( g => g.Id ).ToList();
                foreach ( var itemCount in new AttendanceService( rockContext ).Queryable()
                    .Where( a =>
                        a.ScheduleId.HasValue &&
                        a.GroupId.HasValue &&
                        a.LocationId.HasValue &&
                        a.StartDateTime > dayStart &&
                        a.StartDateTime < now &&
                        a.DidAttend &&
                        activeSchedules.Contains( a.ScheduleId.Value ) &&
                        groupIds.Contains( a.GroupId.Value ) )
                    .GroupBy( a => new
                    {
                        ScheduleId = a.ScheduleId.Value,
                        GroupId = a.GroupId.Value,
                        LocationId = a.LocationId.Value
                    } )
                    .Select( g => new
                    {
                        ScheduleId = g.Key.ScheduleId,
                        GroupId = g.Key.GroupId,
                        LocationId = g.Key.LocationId,
                        Count = g.Count()
                    } ) )
                {
                    AddGroupCount( itemCount.GroupId, itemCount.Count );
                    AddLocationCount( itemCount.LocationId, itemCount.Count );
                }

                return NavData;
            }
            else
            {
                nbGroupTypeWarning.Visible = true;
            }

            return null;

        }

        private void AddGroupType( GroupTypeCache groupType )
        {
            if ( groupType != null && !NavData.GroupTypes.Exists( g => g.Id == groupType.Id ) )
            {
                var navGroupType = new NavigationGroupType( groupType );
                NavData.GroupTypes.Add( navGroupType );

                foreach ( var childGroupType in groupType.ChildGroupTypes )
                {
                    AddGroupType( childGroupType );
                    navGroupType.ChildGroupTypeIds.Add( childGroupType.Id );
                }
            }
        }

        private NavigationLocation AddLocation( Location location )
        {
            if ( location != null )
            {
                var navLocation = NavData.Locations.FirstOrDefault( l => l.Id == location.Id );
                if ( navLocation == null )
                {
                    navLocation = new NavigationLocation( location );
                    NavData.Locations.Add( navLocation );
                }

                if ( location.ParentLocationId.HasValue )
                {
                    navLocation.ParentId = location.ParentLocationId;

                    var parentLocation = NavData.Locations.FirstOrDefault( l => l.Id == location.ParentLocationId );
                    if ( parentLocation == null )
                    {
                        parentLocation = AddLocation( location.ParentLocation );
                    }
                    if ( parentLocation != null )
                    {
                        parentLocation.ChildLocationIds.Add( navLocation.Id );
                    }
                }

                return navLocation;
            }

            return null;
        }

        private void AddGroupCount( int groupId, int count )
        {
            var navGroup = NavData.Groups.FirstOrDefault( g => g.Id == groupId );
            if ( navGroup != null )
            {
                navGroup.CurrentCount += count;
            }
        }

        private void AddGroupTypeCount( int groupTypeId, int count )
        {
            var navGroupType = NavData.GroupTypes.FirstOrDefault( g => g.Id == groupTypeId );
            if ( navGroupType != null )
            {
                navGroupType.CurrentCount += count;
                if ( navGroupType.ParentId.HasValue )
                {
                    AddGroupTypeCount( navGroupType.ParentId.Value, count );
                }
            }
        }

        private void AddLocationCount( int locationId, int count )
        {
            var navLocation = NavData.Locations.FirstOrDefault( g => g.Id == locationId );
            if ( navLocation != null )
            {
                navLocation.CurrentCount += count;
                if ( navLocation.ParentId.HasValue )
                {
                    AddLocationCount( navLocation.ParentId.Value, count );
                }
            }
        }

        #endregion

        private void BuildNavigationControls()
        {
            if (string.IsNullOrWhiteSpace(CurrentNavItem))
            {
                CurrentNavItem = "L"; 
            }

            string itemType = CurrentNavItem.Left(1);
            int? itemId = CurrentNavItem.Length > 1 ? CurrentNavItem.Substring( 1 ).AsIntegerOrNull() : null;
            
            NavigationItem headerItem = null;
            var navItems = new List<NavigationItem>();

            switch ( itemType )
            {
                case "L":
                    {
                        if ( itemId.HasValue )
                        {
                            headerItem = NavData.Locations.FirstOrDefault( l => l.Id == itemId.Value );
                        }

                        NavData.Locations
                            .Where( l => l.ParentId == itemId )
                            .ToList().ForEach( l => navItems.Add( l ) );

                        break;
                    }

                case "T":
                    {
                        if ( itemId.HasValue )
                        {
                            headerItem = NavData.GroupTypes.FirstOrDefault( l => l.Id == itemId.Value );
                        }

                        NavData.GroupTypes
                            .Where( t => t.ParentId == itemId )
                            .ToList().ForEach( t => navItems.Add( t ) );

                        NavData.Groups
                            .Where( g => g.GroupTypeId == itemId )
                            .ToList().ForEach( g => navItems.Add( g ) );

                        break;
                    }

                case "G":
                    {
                        if ( itemId.HasValue )
                        {
                            headerItem = NavData.Groups.FirstOrDefault( l => l.Id == itemId.Value );
                        }

                        var locations = new List<int>();
                        NavData.Groups
                            .Where( g => g.Id == itemId )
                            .ToList()
                            .ForEach( g =>
                                g.ChildLocationIds
                                    .ForEach( l => locations.Add( l ) ) );

                        NavData.Locations
                            .Where( l => locations.Contains(l.Id ) )
                            .ToList().ForEach( l => navItems.Add( l ) );

                        break;
                    }
            }

            pnlNavHeading.Visible = headerItem != null;
            if ( headerItem != null )
            {
                lbNavHeading.CommandArgument = itemType + ( headerItem.ParentId.HasValue ? headerItem.ParentId.Value.ToString() : "" );
                lNavHeading.Text = headerItem.Name;
            }

            rptNavItems.DataSource = navItems
                .OrderBy( i => i.TypeKey )
                .ThenBy( i => i.Order )
                .ThenBy( i => i.Name );
            rptNavItems.DataBind();

        }

        #endregion

        #region Helper Navigation Classes

        [Serializable]
        public class NavigationData
        {
            public List<NavigationLocation> Locations { get; set; }
            public List<NavigationGroupType> GroupTypes { get; set; }
            public List<NavigationGroup> Groups { get; set; }
            public NavigationData()
            {
                Locations = new List<NavigationLocation>();
                GroupTypes = new List<NavigationGroupType>();
                Groups = new List<NavigationGroup>();
            }
        }

        [Serializable]
        public abstract class NavigationItem
        {
            public int? ParentId { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
            public int Order { get; set; }
            public int CurrentCount { get; set; }
            public virtual string TypeKey { get { return ""; } }
        }

        [Serializable]
        public class NavigationLocation : NavigationItem
        {
            public override string TypeKey { get { return "L"; } }
            public bool IsActive { get; set; }
            public List<int> ChildLocationIds { get; set; }
            public bool HasGroups { get; set; }
            public NavigationLocation( Location location )
            {
                Id = location.Id;
                Name = location.Name;
                IsActive = location.IsActive;
                ChildLocationIds = new List<int>();
            }
        }

        [Serializable]
        public class NavigationGroupType : NavigationItem
        {
            public override string TypeKey { get { return "T"; } }
            public List<int> ChildGroupTypeIds { get; set; }
            public List<int> ChildGroupIds { get; set; }
            public NavigationGroupType( GroupTypeCache groupType )
            {
                Id = groupType.Id;
                Name = groupType.Name;
                Order = groupType.Order;
                ChildGroupTypeIds = new List<int>();
                ChildGroupIds = new List<int>();
            }
        }

        [Serializable]
        public class NavigationGroup : NavigationItem
        {
            public override string TypeKey { get { return "G"; } }
            public int GroupTypeId { get; set; }
            public List<int> ChildLocationIds { get; set; }
            public NavigationGroup( Group group )
            {
                Id = group.Id;
                Name = group.Name;
                Order = group.Order;
                GroupTypeId = group.GroupTypeId;
                ChildLocationIds = new List<int>();
            }
        }

        #endregion

}
}
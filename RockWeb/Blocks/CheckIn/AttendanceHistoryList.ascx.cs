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
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Checkin
{
    /// <summary>
    /// Block for displaying the attendance history of a person or a group.
    /// </summary>
    [DisplayName( "Attendance History" )]
    [Category( "Checkin" )]
    [Description( "Block for displaying the attendance history of a person or a group." )]
    [BooleanField( "Filter Attendance By Default", "Sets the default display of Attended to Did Attend instead of [All]", false )]
    [ContextAware]
    public partial class AttendanceHistoryList : RockBlock, ICustomGridColumns
    {
        #region Fields

        private Person _person = null;
        private Group _group = null;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gHistory.GridRebind += gHistory_GridRebind;
            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.ClearFilterClick += RFilter_ClearFilterClick;
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            var contextEntity = this.ContextEntity();
            if ( contextEntity != null )
            {
                if ( contextEntity is Person )
                {
                    _person = contextEntity as Person;
                }
                else if ( contextEntity is Group )
                {
                    _group = contextEntity as Group;
                }
            }

            if ( !Page.IsPostBack )
            {
                bool valid = true;
                int personEntityTypeId = EntityTypeCache.Get( "Rock.Model.Person" ).Id;
                if ( ContextTypesRequired.Any( p => p.Id == personEntityTypeId ) && _person == null )
                {
                    valid = false;
                }

                int batchEntityTypeId = EntityTypeCache.Get( "Rock.Model.Group" ).Id;
                if ( ContextTypesRequired.Any( g => g.Id == batchEntityTypeId ) && _group == null )
                {
                    valid = false;
                }

                if ( valid )
                {
                    rFilter.Visible = true;
                    gHistory.Visible = true;
                    BindFilter();
                    BindGrid();
                }
                else
                {
                    rFilter.Visible = false;
                    gHistory.Visible = false;
                }
            }
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the GridRebind event of the gHistory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gHistory_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            var rockContext = new RockContext();
            switch ( e.Key )
            {
                case "Date Range":
                    e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                    break;

                case "Person":
                    int? personId = e.Value.AsIntegerOrNull();
                    e.Value = null;
                    if ( personId.HasValue )
                    {
                        var person = new PersonService( rockContext ).Get( personId.Value );
                        if ( person != null )
                        {
                            e.Value = person.ToString();
                        }
                    }
                    break;

                case "Group":
                    int? groupId = e.Value.AsIntegerOrNull();
                    e.Value = null;
                    if ( groupId.HasValue )
                    {
                        var group = new GroupService( rockContext ).Get( groupId.Value );
                        if ( group != null )
                        {
                            e.Value = group.ToString();
                        }
                    }
                    break;

                case "Schedule":
                    int? scheduleId = e.Value.AsIntegerOrNull();
                    e.Value = null;
                    if ( scheduleId.HasValue )
                    {
                        var schedule = new ScheduleService( rockContext ).Get( scheduleId.Value );
                        if ( schedule != null )
                        {
                            e.Value = schedule.Name;
                        }
                    }
                    break;

                case "Attended":
                    if ( e.Value == "1" )
                    {
                        e.Value = "Did Attend";
                    }
                    else if ( e.Value == "0" )
                    {
                        e.Value = "Did Not Attend";
                    }
                    else
                    {
                        e.Value = null;
                    }

                    break;

                default:
                    e.Value = null;
                    break;
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Date Range", drpDates.DelimitedValues );
            rFilter.SaveUserPreference( "Person", ppPerson.SelectedValue.ToString() );
            rFilter.SaveUserPreference( "Group", ddlAttendanceGroup.SelectedValue );
            rFilter.SaveUserPreference( "Schedule", spSchedule.SelectedValue );
            rFilter.SaveUserPreference( "Attended", ddlDidAttend.SelectedValue );

            BindGrid();
        }

        protected void RFilter_ClearFilterClick( object sender, EventArgs e )
        {
            rFilter.DeleteUserPreferences();
            BindFilter();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            drpDates.DelimitedValues = rFilter.GetUserPreference( "Date Range" );

            using ( var rockContext = new RockContext() )
            {
                if ( _person != null )
                {
                    ppPerson.Visible = false;

                    if ( _group == null )
                    {
                        ddlAttendanceGroup.Visible = true;
                        ddlAttendanceGroup.Items.Clear();
                        ddlAttendanceGroup.Items.Add( new ListItem() );

                        // only list groups that this person has attended before
                        var groupIdsAttended = new AttendanceService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( a =>
                                a.PersonAlias != null &&
                                a.PersonAlias.PersonId == _person.Id )
                            .Select( a => a.Occurrence.GroupId )
                            .Distinct()
                            .ToList();

                        foreach ( var group in new GroupService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( g => groupIdsAttended.Contains( g.Id ) )
                            .OrderBy( g => g.Name )
                            .Select( g => new { g.Name, g.Id } ).ToList() )
                        {
                            ddlAttendanceGroup.Items.Add( new ListItem( group.Name, group.Id.ToString() ) );
                        }

                        ddlAttendanceGroup.SetValue( rFilter.GetUserPreference( "Group" ).AsIntegerOrNull() );
                    }
                    else
                    {
                        ddlAttendanceGroup.Visible = false;
                    }
                }
                else
                {
                    ppPerson.Visible = true;
                    int? personId = rFilter.GetUserPreference( "Person" ).AsIntegerOrNull();
                    if ( personId.HasValue )
                    {
                        var person = new PersonService( rockContext ).Get( personId.Value );
                        ppPerson.SetValue( person );
                    }

                    ddlAttendanceGroup.Visible = false;
                }
            }

            spSchedule.SetValue( rFilter.GetUserPreference( "Schedule" ).AsIntegerOrNull() );

            string filterValue = rFilter.GetUserPreference( "Attended" );
            var filterAttendance = GetAttributeValue( "FilterAttendanceByDefault" ).AsBoolean();
            if ( string.IsNullOrEmpty( filterValue ) && filterAttendance )
            {
                filterValue = "1";
                rFilter.SaveUserPreference( "Attended", filterValue );
            }

            ddlDidAttend.SetValue( filterValue );
        }

        private List<GroupTypePath> _groupTypePaths;
        private Dictionary<int, string> _locationPaths;

        /// <summary>
        /// Binds the grid.
        /// </summary>
        protected void BindGrid()
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            var qryAttendance = attendanceService.Queryable();

            var personField = gHistory.Columns.OfType<PersonField>().FirstOrDefault();
            if ( personField != null )
            {
                personField.Visible = _person == null;
            }

            var groupField = gHistory.Columns.OfType<RockBoundField>().FirstOrDefault( a => a.HeaderText == "Group" );
            if ( groupField != null )
            {
                groupField.Visible = _group == null;
            }

            if ( _person != null )
            {
                qryAttendance = qryAttendance.Where( a => a.PersonAlias.PersonId == _person.Id );

                if ( _group != null )
                {
                    qryAttendance = qryAttendance.Where( a => a.Occurrence.GroupId == _group.Id );
                }
                else
                {
                    int? groupId = ddlAttendanceGroup.SelectedValueAsInt();
                    if ( groupId.HasValue )
                    {
                        qryAttendance = qryAttendance.Where( a => a.Occurrence.GroupId == groupId.Value );
                    }
                }
            }
            else
            {
                int? personId = ppPerson.SelectedValue;
                if ( personId.HasValue )
                {
                    qryAttendance = qryAttendance.Where( a => a.PersonAlias.PersonId == personId.Value );
                }
            }

            // Filter by Date Range
            if ( drpDates.LowerValue.HasValue )
            {
                qryAttendance = qryAttendance.Where( t => t.StartDateTime >= drpDates.LowerValue.Value );
            }
            if ( drpDates.UpperValue.HasValue )
            {
                DateTime upperDate = drpDates.UpperValue.Value.Date.AddDays( 1 );
                qryAttendance = qryAttendance.Where( t => t.StartDateTime < upperDate );
            }

            // Filter by Schedule
            int? scheduleId = spSchedule.SelectedValue.AsIntegerOrNull();
            if ( scheduleId.HasValue && scheduleId.Value > 0 )
            {
                qryAttendance = qryAttendance.Where( h => h.Occurrence.ScheduleId == scheduleId.Value );
            }

            // Filter by DidAttend
            int? didAttend = ddlDidAttend.SelectedValueAsInt( false );
            if ( didAttend.HasValue )
            {
                if ( didAttend.Value == 1 )
                {
                    qryAttendance = qryAttendance.Where( a => a.DidAttend == true );
                }
                else
                {
                    qryAttendance = qryAttendance.Where( a => a.DidAttend == false );
                }
            }

            var qry = qryAttendance
                .Select( a => new
                {
                    LocationId = a.Occurrence.LocationId,
                    LocationName = a.Occurrence.Location != null ? a.Occurrence.Location.Name : string.Empty,
                    CampusId = a.CampusId,
                    CampusName = a.Campus != null ? a.Campus.Name : string.Empty,
                    ScheduleName = a.Occurrence.Schedule != null ? a.Occurrence.Schedule.Name : string.Empty,
                    Person = a.PersonAlias.Person,
                    GroupName = a.Occurrence.Group != null ? a.Occurrence.Group.Name : string.Empty,
                    GroupTypeId = a.Occurrence.Group != null ? a.Occurrence.Group.GroupTypeId : (int?)null,
                    StartDateTime = a.StartDateTime,
                    EndDateTime = a.EndDateTime,
                    DidAttend = a.DidAttend
                } );

            SortProperty sortProperty = gHistory.SortProperty;
            if ( sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderByDescending( p => p.StartDateTime );
            }

            // build a lookup for _groupTypePaths for OnRowDatabound
            _groupTypePaths = new GroupTypeService( rockContext ).GetAllCheckinGroupTypePaths().ToList();

            // build a lookup for _locationpaths for OnRowDatabound
            _locationPaths = new Dictionary<int, string>();
            var qryLocations = new LocationService( rockContext ).Queryable().Where( a => qry.Any( b => b.LocationId == a.Id ) );
            foreach ( var location in qryLocations )
            {
                var parentLocation = location.ParentLocation;
                var locationNames = new List<string>();
                while ( parentLocation != null )
                {
                    locationNames.Add( parentLocation.Name );
                    parentLocation = parentLocation.ParentLocation;
                }

                string locationPath = string.Empty;
                if ( locationNames.Any() )
                {
                    locationNames.Reverse();
                    locationPath = locationNames.AsDelimited( " > " );
                }

                _locationPaths.AddOrIgnore( location.Id, locationPath );
            }

            gHistory.EntityTypeId = EntityTypeCache.Get<Attendance>().Id;
            gHistory.DataSource = qry.ToList();
            gHistory.DataBind();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gHistory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gHistory_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var dataItem = e.Row.DataItem;
            if ( dataItem != null )
            {
                Literal lGroupName = e.Row.FindControl( "lGroupName" ) as Literal;
                if ( lGroupName != null )
                {
                    int? groupTypeId = dataItem.GetPropertyValue( "GroupTypeId" ) as int?;
                    string groupTypePath = null;
                    if ( groupTypeId.HasValue )
                    {
                        var path = _groupTypePaths.FirstOrDefault( a => a.GroupTypeId == groupTypeId.Value );
                        if ( path != null )
                        {
                            groupTypePath = path.Path;
                        }
                    }

                    lGroupName.Text = string.Format( "{0}<br /><small>{1}</small>", dataItem.GetPropertyValue( "GroupName" ), groupTypePath );
                }

                Literal lLocationName = e.Row.FindControl( "lLocationName" ) as Literal;
                if ( lLocationName != null )
                {
                    int? locationId = dataItem.GetPropertyValue( "LocationId" ) as int?;
                    string locationPath = null;
                    if ( locationId.HasValue )
                    {
                        if ( _locationPaths.ContainsKey( locationId.Value ) )
                        {
                            locationPath = _locationPaths[locationId.Value];
                        }
                    }

                    lLocationName.Text = string.Format( "{0}<br /><small>{1}</small>", dataItem.GetPropertyValue( "LocationName" ), locationPath );
                }
            }
        }

        #endregion
    }
}

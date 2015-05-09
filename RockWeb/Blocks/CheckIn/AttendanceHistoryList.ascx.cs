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
    [ContextAware]
    public partial class AttendanceHistoryList : RockBlock
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
                int personEntityTypeId = EntityTypeCache.Read( "Rock.Model.Person" ).Id;
                if ( ContextTypesRequired.Any( p => p.Id == personEntityTypeId ) && _person == null )
                {
                    valid = false;
                }

                int batchEntityTypeId = EntityTypeCache.Read( "Rock.Model.Group" ).Id;
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
                    var person = new PersonService( rockContext ).Get( e.Value.AsIntegerOrNull() ?? 0 );
                    if ( person != null )
                    {
                        e.Value = person.ToString();
                    }
                    else
                    {
                        e.Value = null;
                    }

                    break;

                case "Group":
                    var group = new GroupService( rockContext ).Get( e.Value.AsIntegerOrNull() ?? 0 );
                    if ( group != null )
                    {
                        e.Value = group.ToString();
                    }
                    else
                    {
                        e.Value = null;
                    }

                    break;

                case "Schedule":
                    var schedule = new ScheduleService( rockContext ).Get( e.Value.AsIntegerOrNull() ?? 0 );
                    if ( schedule != null )
                    {
                        e.Value = schedule.Name;
                    }
                    else
                    {
                        e.Value = null;
                    }

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
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            drpDates.DelimitedValues = rFilter.GetUserPreference( "Date Range" );

            ppPerson.Visible = _person == null;
            ddlAttendanceGroup.Visible = _group == null && _person != null;
            ddlAttendanceGroup.Items.Clear();
            ddlAttendanceGroup.Items.Add( new ListItem() );

            if ( _person != null )
            {
                var rockContext = new RockContext();
                var qryGroup = new GroupService( rockContext ).Queryable();

                var qryPersonAttendance = new AttendanceService( rockContext ).Queryable().Where( a => a.PersonAlias.PersonId == _person.Id );

                // only list groups that this person has attended before
                var groupList = qryGroup.Where( g => qryPersonAttendance.Any( a => a.GroupId == g.Id ) )
                    .OrderBy( a => a.Name )
                    .Select( a => new { a.Name, a.Id } ).ToList();

                foreach ( var group in groupList )
                {
                    ddlAttendanceGroup.Items.Add( new ListItem( group.Name, group.Id.ToString() ) );
                }

                ddlAttendanceGroup.SetValue( rFilter.GetUserPreference( "Group" ).AsIntegerOrNull() );
            }

            spSchedule.SetValue( rFilter.GetUserPreference( "Schedule" ).AsIntegerOrNull() );
        }

        private List<GroupTypePath> _groupTypePaths;

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
            }
            else if ( _group != null )
            {
                qryAttendance = qryAttendance.Where( a => a.GroupId == _group.Id );
            }

            // Filter by Date Range
            var drp = new DateRangePicker();
            drp.DelimitedValues = rFilter.GetUserPreference( "Date Range" );
            if ( drp.LowerValue.HasValue )
            {
                qryAttendance = qryAttendance.Where( t => t.StartDateTime >= drp.LowerValue.Value );
            }

            if ( drp.UpperValue.HasValue )
            {
                DateTime upperDate = drp.UpperValue.Value.Date.AddDays( 1 );
                qryAttendance = qryAttendance.Where( t => t.EndDateTime < upperDate );
            }

            // Filter by Person
            int? personId = ppPerson.SelectedValue;
            if ( personId.HasValue && ppPerson.Visible )
            {
                qryAttendance = qryAttendance.Where( a => a.PersonAlias.PersonId == personId.Value );
            }

            // Filter by Group
            int groupId = ddlAttendanceGroup.SelectedValue.AsInteger();
            if ( groupId > 0 && ddlAttendanceGroup.Visible )
            {
                qryAttendance = qryAttendance.Where( a => a.GroupId == groupId );
            }

            // Filter by Schedule
            int scheduleId = spSchedule.SelectedValue.AsInteger();
            if ( scheduleId > 0 && spSchedule.Visible )
            {
                qryAttendance = qryAttendance.Where( h => h.ScheduleId == scheduleId );
            }

            var qry = qryAttendance
                .Select( a => new
                {
                    LocationName = a.Location.Name,
                    CampusId = a.CampusId,
                    CampusName = a.Campus.Name,
                    ScheduleName = a.Schedule.Name,
                    Person = a.PersonAlias.Person,
                    GroupName = a.Group.Name,
                    GroupTypeId = a.Group.GroupTypeId,
                    StartDateTime = a.StartDateTime,
                    EndDateTime = a.EndDateTime
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

            _groupTypePaths = new GroupTypeService( rockContext ).GetAllCheckinGroupTypePaths().ToList();

            gHistory.EntityTypeId = EntityTypeCache.Read<Attendance>().Id;
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
            }
        }

        #endregion
    }
}
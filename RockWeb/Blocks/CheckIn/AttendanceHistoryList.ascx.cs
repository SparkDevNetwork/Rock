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
using System.Web;
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
        void gHistory_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        protected void gHistory_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow || e.Row.RowType == DataControlRowType.Header )
            {
                if ( _person != null )
                {
                    e.Row.Cells[2].Visible = false;
                }
                else if ( _group != null )
                {
                    e.Row.Cells[3].Visible = false;
                }
            }
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Date Range":
                    e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                    break;

                case "Person":
                    e.Value = ddlPeople.SelectedValue;
                    break;

                case "Group":
                    e.Value = ddlGroups.SelectedValue;
                    break;

                case "Schedule":
                    e.Value = ddlSchedules.SelectedValue;
                    break;
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Date Range", drpDates.DelimitedValues );
            rFilter.SaveUserPreference( "Person", ddlPeople.SelectedValue.ToLower() != "-1" ? ddlPeople.SelectedValue : string.Empty );
            rFilter.SaveUserPreference( "Group", ddlGroups.SelectedValue.ToLower() != "-1" ? ddlGroups.SelectedValue : string.Empty );
            rFilter.SaveUserPreference( "Schedule", ddlSchedules.SelectedValue.ToLower() != "-1" ? ddlSchedules.SelectedValue : string.Empty );
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

            var attendanceService = new AttendanceService( new RockContext() );
            var attendanceQuery = attendanceService.Queryable( "PersonAlias.Person" );
            if ( _person != null )
            {
                attendanceQuery = attendanceQuery.Where( a => a.PersonAlias.PersonId == _person.Id );

                ddlGroups.DataSource = attendanceQuery.Select( a => a.Group ).Distinct().OrderBy( a => a.Name ).ToList();
                ddlGroups.DataBind();
                ddlGroups.Items.Insert( 0, Rock.Constants.All.ListItem );
                ddlGroups.Visible = attendanceQuery.Select( a => a.Group ).Distinct().ToList().Any();
                ddlGroups.SetValue( rFilter.GetUserPreference( "Group" ) );

                ddlPeople.Visible = false;
            }

            if ( _group != null )
            {
                attendanceQuery = attendanceQuery.Where( a => a.GroupId == _group.Id );
                var attendanceList = attendanceQuery.ToList();

                ddlPeople.DataSource = attendanceList.Where( a => a.PersonAlias != null && a.PersonAlias.Person != null)
                    .OrderBy( a => a.PersonAlias.Person.FullName ).Select( a => a.PersonAlias.Person.FullName ).Distinct();
                ddlPeople.DataBind();
                ddlPeople.Items.Insert( 0, Rock.Constants.All.ListItem );
                ddlPeople.Visible = attendanceList.Where( a => a.PersonAlias != null && a.PersonAlias.Person != null )
                    .Select( a => a.PersonAlias.Person.FullName ).Distinct().Any();
                ddlPeople.SetValue( rFilter.GetUserPreference( "Person" ) );

                ddlGroups.Visible = false;
            }

            ddlSchedules.DataSource = attendanceQuery.Where( a => a.Schedule != null ).OrderBy( a => a.Schedule.Name ).Select( a => a.Schedule.Name ).Distinct().ToList();
            ddlSchedules.DataBind();
            ddlSchedules.Items.Insert( 0, Rock.Constants.All.ListItem );
            ddlSchedules.Visible = attendanceQuery.Where( a => a.Schedule != null ).Select( a => a.Schedule.Name ).Distinct().ToList().Any();
            ddlSchedules.SetValue( rFilter.GetUserPreference( "Schedule" ) );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        protected void BindGrid()
        {
            var attendanceService = new AttendanceService( new RockContext() );
            var attendanceQuery = attendanceService.Queryable( "PersonAlias.Person" );
            if ( _person != null )
            {
                attendanceQuery = attendanceQuery.Where( a => a.PersonAlias.PersonId == _person.Id );
            }
            else if ( _group != null )
            {
                attendanceQuery = attendanceQuery.Where( a => a.GroupId == _group.Id );
            }

            var attendanceList = attendanceQuery.ToList();
            var qry = attendanceList.AsQueryable()
                .Select( a => new
                {
                    Location = a.Location,
                    Schedule = a.Schedule,
                    FullName = a.PersonAlias.Person.FullName,
                    Group = a.Group,
                    StartDateTime = a.StartDateTime,
                    EndDateTime = a.EndDateTime
                } );

            // Filter by Date Range
            var drp = new DateRangePicker();
            drp.DelimitedValues = rFilter.GetUserPreference( "Date Range" );
            if ( drp.LowerValue.HasValue )
            {
                qry = qry.Where( t => t.StartDateTime >= drp.LowerValue.Value );
            }
            if ( drp.UpperValue.HasValue )
            {
                DateTime upperDate = drp.UpperValue.Value.Date.AddDays( 1 );
                qry = qry.Where( t => t.EndDateTime < upperDate );
            }

            // Filter by Person
            if ( ddlPeople.SelectedIndex > 0 )
            {
                if ( ddlPeople.SelectedValue.ToLower() != "all" )
                {
                    qry = qry.Where( h => h.FullName == ddlPeople.SelectedValue );
                }
            }

            // Filter by Group
            if ( ddlGroups.SelectedIndex > 0 )
            {
                if ( ddlGroups.SelectedValue.ToLower() != "all" )
                {
                    qry = qry.Where( h => h.Group.Name == ddlGroups.SelectedValue );
                }
            }

            // Filter by Schedule
            if ( ddlSchedules.SelectedIndex > 0 )
            {
                if ( ddlSchedules.SelectedValue.ToLower() != "all" )
                {
                    qry = qry.Where( h => h.Schedule.Name == ddlSchedules.SelectedValue );
                }
            }

            SortProperty sortProperty = gHistory.SortProperty;
            if ( sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderByDescending( p => p.StartDateTime );
            }

            gHistory.EntityTypeId = EntityTypeCache.Read<Attendance>().Id;
            gHistory.DataSource = qry.ToList();
            gHistory.DataBind();
        }

        #endregion
}
}
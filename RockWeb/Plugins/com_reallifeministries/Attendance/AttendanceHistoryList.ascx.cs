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
using Rock.Attribute;

namespace com.reallifeministries.Attendance
{
    /// <summary>
    /// Block for displaying the attendance history of a person or a group.
    /// </summary>
    [DisplayName( "RLM Attendance History" )]
    [Category( "Attendance" )]
    [Description( "Block for displaying the attendance history of a person." )]
    [IntegerField("Limit","The number of records that will display",true,15)]
    [GroupTypesField("Show Group Types","Only display display attendance for these group types")]
    [ContextAware]
    public partial class AttendanceHistoryList : RockBlock
    {
        #region Fields

        private Person _person = null;
        private List<Guid> GroupTypeGuids = new List<Guid>();

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
            }
            var showGroupTypes = GetAttributeValue( "ShowGroupTypes" ).Split( new char[] { ',' } ).Where( s => !String.IsNullOrEmpty(s));
            this.GroupTypeGuids = showGroupTypes.Select(g => g.AsGuid()).ToList();
            
            if ( !Page.IsPostBack )
            {
                bool valid = true;
                int personEntityTypeId = EntityTypeCache.Read( "Rock.Model.Person" ).Id;
                if ( ContextTypesRequired.Any( p => p.Id == personEntityTypeId ) && _person == null )
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

                case "Group":
                    e.Value = ddlGroups.SelectedValue;
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
            rFilter.SaveUserPreference( "Group", ddlGroups.SelectedValue.ToLower() != "-1" ? ddlGroups.SelectedValue : string.Empty );
            
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
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        protected void BindGrid()
        {
            if (_person != null)
            {
                var attendanceService = new AttendanceService( new RockContext() );
                var attendanceQuery = attendanceService.Queryable( "PersonAlias.Person" );

                attendanceQuery = attendanceQuery.Where( a => a.PersonAlias.PersonId == _person.Id );

                if (this.GroupTypeGuids.Count > 0)
                {
                    attendanceQuery = attendanceQuery.Where( a => this.GroupTypeGuids.Contains( a.Group.GroupType.Guid ) );
                }


                var attendanceList = attendanceQuery.ToList();
                var qry = attendanceList.AsQueryable()
                    .Select( a => new
                    {
                        Location = a.Location,
                        FullName = a.PersonAlias.Person.FullName,
                        GroupTypeName = a.Group.GroupType.Name,
                        GroupName = a.Group.Name,
                        Person = a.PersonAlias.Person,
                        Group = a.Group,
                        StartDateTime = a.StartDateTime
                    } );

                // Filter by Date Range
                var drp = new DateRangePicker();
                drp.DelimitedValues = rFilter.GetUserPreference( "Date Range" );
                if (drp.LowerValue.HasValue)
                {
                    qry = qry.Where( t => t.StartDateTime >= drp.LowerValue.Value );
                }
                if (drp.UpperValue.HasValue)
                {
                    DateTime upperDate = drp.UpperValue.Value.Date.AddDays( 1 );
                    qry = qry.Where( t => t.StartDateTime < upperDate );
                }

                // Filter by Group
                if (ddlGroups.SelectedIndex > 0)
                {
                    if (ddlGroups.SelectedValue.ToLower() != "all")
                    {
                        qry = qry.Where( h => h.Group.Name == ddlGroups.SelectedValue );
                    }
                }

                SortProperty sortProperty = gHistory.SortProperty;
                if (sortProperty != null)
                {
                    qry = qry.Sort( sortProperty );
                }
                else
                {
                    qry = qry.OrderByDescending( p => p.StartDateTime );
                }

                var limit = GetAttributeValue( "Limit" ).AsInteger();
                qry = qry.Take( limit );

                gHistory.DataSource = qry.ToList();

                gHistory.DataBind();
            }
        }

        #endregion
}
}
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
using System.ComponentModel;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using com.ccvonline.CommandCenter.Model;
using com.ccvonline.CommandCenter.Data;

namespace RockWeb.Plugins.com_ccvonline.CommandCenter
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "DVR Recording List" )]
    [Category( "CCV > Command Center" )]
    [Description( "Lists all of the Command Center DVR recordings." )]
    [LinkedPage( "Detail Page" )]
    public partial class DVRRecordingList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gRecordings.GridRebind += gRecordings_GridRebind;

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

            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }
        }       

        #endregion

        #region Grid Events

        void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Campus":

                    int campusId = 0;
                    if ( int.TryParse( e.Value, out campusId ) )
                    {
                        var service = new CampusService( new RockContext() );
                        var campus = service.Get( campusId );
                        if ( campus != null )
                        {
                            e.Value = campus.Name;
                        }
                    }

                    break;
            }
        }

        /// <summary>
        /// Handles the Edit event of the gRecordings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gRecordings_Edit( object sender, RowEventArgs e )
        {            
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            var weekendDate = ( (DateTime)e.RowKeyValues[0] );
            parameters.Add( "WeekendDate", weekendDate.ToString() );

            var campusGuid = e.RowKeyValues[1].ToString();
            parameters.Add( "CampusGuid", campusGuid.ToString() );

            var venue = e.RowKeyValues[2];
            parameters.Add( "Venue", venue.ToString() );            

            NavigateToLinkedPage( "DetailPage", parameters );
        }

        /// <summary>
        /// Handles the GridRebind event of the gRecordings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gRecordings_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Campus", cpCampus.SelectedValue != All.Id.ToString() ? cpCampus.SelectedValue : string.Empty );
            rFilter.SaveUserPreference( "From Date", dtStartDate.Text );
            rFilter.SaveUserPreference( "To Date", dtEndDate.Text );
            rFilter.SaveUserPreference( "Venue", ddlVenue.Text );

            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            cpCampus.Campuses = new CampusService( new RockContext() ).Queryable().OrderBy( c => c.Name ).ToList();
            cpCampus.Items.Insert( 0, new ListItem( All.Text, All.IdValue ) );
            cpCampus.SelectedValue = rFilter.GetUserPreference( "Campus" );
            dtStartDate.Text = rFilter.GetUserPreference( "From Date" );
            dtEndDate.Text = rFilter.GetUserPreference( "To Date" );
            ddlVenue.Text = rFilter.GetUserPreference( "Venue" );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var service = new RecordingService( new CommandCenterContext() );
            var sortProperty = gRecordings.SortProperty;

            var queryable = service.Queryable()
                                    .GroupBy( r => 
                                        new {
                                            r.Campus,
                                            r.CampusId,
                                            WeekendDate = DbFunctions.TruncateTime( SqlFunctions.DateAdd( "day", 1 - SqlFunctions.DatePart( "dw", r.Date ) % 7, r.Date ) ),
                                            r.Venue
                                            } )
                                    .Select( g => 
                                        new {
                                            WeekendDate = g.Key.WeekendDate,
                                            Campus = g.Key.Campus,
                                            CampusId = g.Key.CampusId,
                                            CampusGuid = g.Key.Campus.Guid,
                                            Venue = g.Key.Venue,
                                            RecordingCount = g.Select( t => t.Date ).Count()
                                        } );

            int campusId = int.MinValue;
            if ( int.TryParse( rFilter.GetUserPreference( "Campus" ), out campusId ) && campusId > 0 )
            {
                queryable = queryable.Where( r => r.CampusId == campusId );
            }

            DateTime fromDate = DateTime.MinValue;
            if ( DateTime.TryParse( rFilter.GetUserPreference( "From Date" ), out fromDate ) )
            {
                queryable = queryable.Where( r => r.WeekendDate >= fromDate );
            }

            DateTime toDate = DateTime.MinValue;
            if ( DateTime.TryParse( rFilter.GetUserPreference( "To Date" ), out toDate ) )
            {
                queryable = queryable.Where( r => r.WeekendDate <= toDate );
            }

            string venue = rFilter.GetUserPreference( "Venue" );
            if (!string.IsNullOrWhiteSpace( venue ) )
            {
                queryable = queryable.Where( r => r.Venue.StartsWith( venue ) );
            }

            if ( sortProperty != null )
            {
                gRecordings.DataSource = queryable.Sort( sortProperty ).ToList();
            }
            else
            {
                gRecordings.DataSource = queryable.OrderByDescending( t => t.WeekendDate ).ToList();
            }
           
            gRecordings.DataBind();
        }

        #endregion
    }
}
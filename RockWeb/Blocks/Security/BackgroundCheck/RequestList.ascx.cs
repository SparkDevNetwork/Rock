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
using System.Data.Entity;
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
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Security.BackgroundCheck
{
    [DisplayName( "Request List" )]
    [Category( "Security > Background Check" )]
    [Description( "Lists all the background check requests." )]

    public partial class RequestList : RockBlock, ISecondaryBlock
    { 
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            fRequest.ApplyFilterClick += fRequest_ApplyFilterClick;
            fRequest.DisplayFilterValue += fRequest_DisplayFilterValue;
            
            gRequest.DataKeyNames = new string[] { "Id" };
            gRequest.Actions.ShowAdd = false;
            gRequest.IsDeleteEnabled = false;
            gRequest.GridRebind += gRequest_GridRebind;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events (main grid)

        /// <summary>
        /// Handles the ApplyFilterClick event of the fRequest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void fRequest_ApplyFilterClick( object sender, EventArgs e )
        {
            fRequest.SaveUserPreference( "Request Date Range", drpRequestDates.DelimitedValues );
            fRequest.SaveUserPreference( "Response Date Range", drpResponseDates.DelimitedValues );

            BindGrid();
        }

        /// <summary>
        /// Displays the text of the current filters
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void fRequest_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Request Date Range":
                case "Response Date Range":
                    e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                    break;

            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gRequest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gRequest_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            drpRequestDates.DelimitedValues = fRequest.GetUserPreference( "Request Date Range" );
            drpResponseDates.DelimitedValues = fRequest.GetUserPreference( "Response Date Range" );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                var qry = new BackgroundCheckService( rockContext )
                    .Queryable().AsNoTracking();

                // Request Date Range
                var drpRequestDates = new DateRangePicker();
                drpRequestDates.DelimitedValues = fRequest.GetUserPreference( "Request Date Range" );
                if ( drpRequestDates.LowerValue.HasValue )
                {
                    qry = qry.Where( t => t.RequestDate >= drpRequestDates.LowerValue.Value );
                }

                if ( drpRequestDates.UpperValue.HasValue )
                {
                    DateTime upperDate = drpRequestDates.UpperValue.Value.Date.AddDays( 1 );
                    qry = qry.Where( t => t.RequestDate < upperDate );
                }


                // Request Date Range
                var drpResponseDates = new DateRangePicker();
                drpResponseDates.DelimitedValues = fRequest.GetUserPreference( "Response Date Range" );
                if ( drpResponseDates.LowerValue.HasValue )
                {
                    qry = qry.Where( t => t.RequestDate >= drpResponseDates.LowerValue.Value );
                }

                if ( drpResponseDates.UpperValue.HasValue )
                {
                    DateTime upperDate = drpResponseDates.UpperValue.Value.Date.AddDays( 1 );
                    qry = qry.Where( t => t.RequestDate < upperDate );
                }

                SortProperty sortProperty = gRequest.SortProperty;
                if ( sortProperty != null )
                {
                    gRequest.DataSource = qry.Sort( sortProperty ).ToList();
                }
                else
                {
                    gRequest.DataSource = qry.OrderByDescending( d => d.RequestDate ).ToList();
                }
                gRequest.DataBind();
            }
        }

        #endregion

        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }
    }
}
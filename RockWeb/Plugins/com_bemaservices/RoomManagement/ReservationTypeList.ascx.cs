﻿// <copyright>
// Copyright by the BEMA Software Services
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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;

using com.bemaservices.RoomManagement.Model;

namespace RockWeb.Plugins.com_bemaservices.RoomManagement
{
    /// <summary>
    /// Block to display the reservation types.
    /// </summary>
    [DisplayName( "Reservation Type List" )]
    [Category( "BEMA Services > Room Management" )]
    [Description( "Block to display the reservation types." )]
    [LinkedPage( "Detail Page", "Page used to view details of a reservation type." )]
    public partial class ReservationTypeList : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            lbAddReservationType.Visible = UserCanAdministrate;
            rptReservationTypes.ItemCommand += rptReservationTypes_ItemCommand;
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
                GetData();
            }
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
            GetData();
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptReservationTypes control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptReservationTypes_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int? reservationTypeId = e.CommandArgument.ToString().AsIntegerOrNull();
            if ( reservationTypeId.HasValue )
            {
                NavigateToLinkedPage( "DetailPage", "ReservationTypeId", reservationTypeId.Value );
            }

            GetData();
        }

        /// <summary>
        /// Handles the Click event of the lbAddReservationType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddReservationType_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "ReservationTypeId", 0 );
        }
        #endregion

        #region Methods

        /// <summary>
        /// Gets the data.
        /// </summary>
        private void GetData()
        {
            using ( var rockContext = new RockContext() )
            {
                // Get all of the event calendars
                var allReservationTypes = new ReservationTypeService( rockContext ).Queryable()
                    .OrderBy( w => w.Name )
                    .ToList();

                var authorizedReservationTypes = new List<ReservationType>();
                foreach ( var reservationType in allReservationTypes )
                {
                    if ( UserCanEdit || reservationType.IsAuthorized( Authorization.VIEW, CurrentPerson ))
                    {
                        authorizedReservationTypes.Add( reservationType );
                    }
                }

                rptReservationTypes.DataSource = authorizedReservationTypes.ToList();
                rptReservationTypes.DataBind();
            }
        }

        #endregion
    }
}
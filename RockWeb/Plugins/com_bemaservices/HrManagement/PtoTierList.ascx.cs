﻿// <copyright>
// Copyright by BEMA Information Services
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

using com.bemaservices.HrManagement.Model;
using System.Drawing;
using Rock.Field.Types;
using System.Runtime.CompilerServices;

namespace RockWeb.Plugins.com_bemaservices.HrManagement
{
    /// <summary>
    /// Block to display the connection types.
    /// </summary>
    [DisplayName( "PTO Tier List" )]
    [Category( "BEMA Services > HR Management" )]
    [Description( "Block to display the PTO Tiers." )]
    [LinkedPage( "Detail Page", "Page used to view details of a PTO Tier." )]
    public partial class PtoTierList : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            lbAddPtoTier.Visible = UserCanAdministrate;
            rptPtoTiers.ItemCommand += rptPtoTiers_ItemCommand;
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
        /// Handles the ItemCommand event of the rptConnectionTypes control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptPtoTiers_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int? ptoTierId = e.CommandArgument.ToString().AsIntegerOrNull();
            if ( ptoTierId.HasValue )
            {
                NavigateToLinkedPage( "DetailPage", "PtoTierId", ptoTierId.Value );
            }

            GetData();
        }

        /// <summary>
        /// Handles the Click event of the lbAddConnectionType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddPtoTier_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "PtoTierId", 0 );
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
                var allPtoTiers = new PtoTierService( rockContext ).Queryable()
                    .OrderBy( w => w.Name )
                    .ToList();

                /* In case we add security on the entity later 

                var authorizedPtoTiers = new List<ConnectionType>();
                foreach ( var ptoTier in allPtoTiers )
                {
                    if ( UserCanEdit || ptoTier.IsAuthorized( Authorization.VIEW, CurrentPerson ))
                    {
                        authorizedPtoTiers.Add( ptoTier );
                    }
                }

                */
                rptPtoTiers.DataSource = allPtoTiers.ToList(); //authorizedPtoTiers.ToList();
                rptPtoTiers.DataBind();
            }
        }

        #endregion
    }
}
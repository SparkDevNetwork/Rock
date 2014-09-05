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
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Pledge List" )]
    [Category( "Finance" )]
    [Description( "Generic list of all pledges in the system." )]

    [LinkedPage( "Detail Page" )]
    public partial class PledgeList : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gPledges.DataKeyNames = new[] { "id" };
            gPledges.Actions.AddClick += gPledges_Add;
            gPledges.GridRebind += gPledges_GridRebind;
            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;

            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gPledges.Actions.ShowAdd = canAddEditDelete;
            gPledges.IsDeleteEnabled = canAddEditDelete;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Handles the GridRebind event of the gPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gPledges_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gPledges_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "pledgeId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gPledges_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "pledgeId", (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the gPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gPledges_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var pledgeService = new FinancialPledgeService( rockContext );
            var pledge = pledgeService.Get( e.RowKeyId );
            string errorMessage;

            if ( pledge == null )
            {
                return;
            }

            if ( !pledgeService.CanDelete( pledge, out errorMessage ) )
            {
                mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                return;
            }

            pledgeService.Delete( pledge );
            rockContext.SaveChanges();

            BindGrid();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var pledgeService = new FinancialPledgeService( new RockContext() );
            var sortProperty = gPledges.SortProperty;
            var pledges = pledgeService.Queryable();

            if ( ppFilterPerson.PersonId.HasValue )
            {
                pledges = pledges.Where( p => p.PersonAlias.PersonId == ppFilterPerson.PersonId.Value );
            }

            var accountIds = fpFilterAccount.SelectedValuesAsInt().Where( i => i != 0 ).ToList();

            if ( accountIds.Any() )
            {
                pledges = pledges.Where( p => p.AccountId.HasValue && accountIds.Contains( p.AccountId.Value ) );
            }

            gPledges.DataSource = sortProperty != null ? pledges.Sort( sortProperty ).ToList() : pledges.OrderBy( p => p.AccountId ).ToList();
            gPledges.DataBind();
        }
    }
}
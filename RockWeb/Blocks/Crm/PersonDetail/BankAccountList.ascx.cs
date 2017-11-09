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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Lists Bank Accounts for a Person
    /// </summary>
    [DisplayName( "Bank Account List" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Lists bank accounts for a person" )]

    public partial class BankAccountList : PersonBlock, ISecondaryBlock, ICustomGridColumns
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            bool canEdit = IsUserAuthorized( Authorization.EDIT );

            gList.DataKeyNames = new string[] { "Id" };
            gList.Actions.ShowAdd = false;
            gList.IsDeleteEnabled = canEdit;
            gList.GridRebind += gList_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upFinancialGivingProfile );
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
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var financialPersonBankAccountService = new FinancialPersonBankAccountService( rockContext );
            var financialPersonBankAccount = financialPersonBankAccountService.Get( e.RowKeyId );

            if ( financialPersonBankAccount != null )
            {
                string errorMessage;
                if ( !financialPersonBankAccountService.CanDelete( financialPersonBankAccount, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                financialPersonBankAccountService.Delete( financialPersonBankAccount );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var financialPersonBankAccountService = new FinancialPersonBankAccountService( rockContext );
            var qry = financialPersonBankAccountService.Queryable();

            if ( this.Person != null && this.Person.PrimaryAliasId.HasValue )
            {
                qry = qry.Where( a => a.PersonAliasId == this.Person.PrimaryAliasId.Value );

                SortProperty sortProperty = gList.SortProperty;

                if ( sortProperty != null )
                {
                    gList.DataSource = qry.Sort( sortProperty ).ToList();
                }
                else
                {
                    gList.DataSource = qry.OrderBy( s => s.AccountNumberMasked ).ToList();
                }

                gList.DataBind();
            }
        }

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on its page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        #endregion
    }
}

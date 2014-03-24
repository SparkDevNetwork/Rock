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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Block for viewing list of financial accounts
    /// </summary>
    [DisplayName( "Account List" )]
    [Category( "Finance" )]
    [Description( "Block for viewing list of financial accounts." )]
    [LinkedPage( "Detail Page" )]
    public partial class AccountList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            bool canEdit = IsUserAuthorized( Authorization.EDIT );

            rAccountFilter.ApplyFilterClick += rAccountFilter_ApplyFilterClick;
            rAccountFilter.DisplayFilterValue += rAccountFilter_DisplayFilterValue;

            var campusList = new CampusService().Queryable().OrderBy( a => a.Name ).ToList();
            if ( campusList.Count > 0 )
            {
                ddlCampus.Visible = true;
                rGridAccount.Columns[3].Visible = true;
            }

            rGridAccount.DataKeyNames = new string[] { "id" };
            rGridAccount.Actions.ShowAdd = canEdit;
            rGridAccount.Actions.AddClick += rGridAccount_Add;
            rGridAccount.GridReorder += rGridAccount_GridReorder;
            rGridAccount.GridRebind += rGridAccounts_GridRebind;
            rGridAccount.IsDeleteEnabled = canEdit;
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

        #region Events

        /// <summary>
        /// Handles the Add event of the rGridAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rGridAccount_Add( object sender, EventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "accountId", "0" );
            NavigateToLinkedPage( "DetailPage", parms );
        }

        /// <summary>
        /// Handles the Edit event of the rGridAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGridAccount_Edit( object sender, RowEventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "accountId", e.RowKeyValue.ToString() );
            NavigateToLinkedPage( "DetailPage", parms );
        }

        /// <summary>
        /// Handles the Delete event of the rGridAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGridAccount_Delete( object sender, RowEventArgs e )
        {
            var accountService = new FinancialAccountService();
            var account = accountService.Get( (int)e.RowKeyValue );
            if ( account != null )
            {
                string errorMessage;
                if ( !accountService.CanDelete( account, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                accountService.Delete( account, CurrentPersonAlias );
                accountService.Save( account, CurrentPersonAlias );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridReorder event of the rGridAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void rGridAccount_GridReorder( object sender, GridReorderEventArgs e )
        {
            using ( new UnitOfWorkScope() )
            {
                var accounts = GetAccounts();
                if ( accounts != null )
                {
                    new FinancialAccountService().Reorder( accounts.ToList(), e.OldIndex, e.NewIndex, CurrentPersonAlias );
                }

                BindGrid();
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the rGridAccount control.
        /// </summary>
        /// <param name="sendder">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rGridAccounts_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the filter display
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rAccountFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rAccountFilter control.
        /// </summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rAccountFilter_ApplyFilterClick( object Sender, EventArgs e )
        {
            rAccountFilter.SaveUserPreference( "Account Name", txtAccountName.Text );
            rAccountFilter.SaveUserPreference( "Campus", txtAccountName.Text );
            rAccountFilter.SaveUserPreference( "Active", ddlIsActive.SelectedValue );
            rAccountFilter.SaveUserPreference( "Tax Deductible", ddlIsTaxDeductible.SelectedValue );
            BindGrid();
        }

        #endregion

        #region Internal Methods

        private IQueryable<FinancialAccount> GetAccounts()
        {
            var accountService = new FinancialAccountService();
            SortProperty sortProperty = rGridAccount.SortProperty;
            var accountQuery = accountService.Queryable();

            string accountNameFilter = rAccountFilter.GetUserPreference( "Account Name" );
            if ( !string.IsNullOrEmpty( accountNameFilter ) )
            {
                accountQuery = accountQuery.Where( account => account.Name.Contains( accountNameFilter ) );
            }

            string campusFilter = rAccountFilter.GetUserPreference( "Campus" );
            if ( !string.IsNullOrEmpty( campusFilter ) )
            {
                accountQuery = accountQuery.Where( account => account.Campus.Name == campusFilter );
            }

            string activeFilter = rAccountFilter.GetUserPreference( "Active" );
            if ( !string.IsNullOrWhiteSpace( activeFilter ) )
            {
                accountQuery = accountQuery.Where( account => account.IsActive == ( activeFilter == "Yes" ) );
            }

            string taxDeductibleFilter = rAccountFilter.GetUserPreference( "Tax Deductible" );
            if ( !string.IsNullOrWhiteSpace( taxDeductibleFilter ) )
            {
                accountQuery = accountQuery.Where( account => account.IsTaxDeductible == ( taxDeductibleFilter == "Yes" ) );
            }

            accountQuery = accountQuery.OrderBy( a => a.Order );

            return accountQuery;
        }

        /// <summary>
        /// Binds the Account list grid.
        /// </summary>
        private void BindGrid()
        {
            rGridAccount.DataSource = GetAccounts().ToList();
            rGridAccount.DataBind();
        }

        private void BindFilter()
        {
            txtAccountName.Text = rAccountFilter.GetUserPreference( "Account Name" );
            ddlCampus.SelectedValue = rAccountFilter.GetUserPreference( "Campus" );
            ddlIsActive.SelectedValue = rAccountFilter.GetUserPreference( "Active" );
            ddlIsTaxDeductible.SelectedValue = rAccountFilter.GetUserPreference( "Tax Deductible" );
        }

        #endregion
    }
}
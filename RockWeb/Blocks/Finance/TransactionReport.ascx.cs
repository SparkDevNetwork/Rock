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
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Transaction Report" )]
    [Category( "Finance" )]
    [Description( "Block that reports transactions for the currently logged in user with filters." )]
    [TextField( "Transaction Label", "The label to use to describe the transactions (e.g. 'Gifts', 'Donations', etc.)", true, "Gifts", "", 1 )]
    [TextField("Account Label", "The label to use to describe accounts.", true, "Accounts", "", 2)]
    [AccountsField("Accounts", "List of accounts to allow the person to view", false, "", "", 3)]
    public partial class TransactionReport : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
            cblAccounts.Label = GetAttributeValue( "AccountLabel" );
            gTransactions.DataKeyNames = new string[] { "Id" };
            gTransactions.RowItemText = GetAttributeValue( "TransactionLabel" );
            gTransactions.EmptyDataText = string.Format( "No {0} found with the provided criteria.", GetAttributeValue( "TransactionLabel" ).ToLower() );
            gTransactions.GridRebind += gTransactions_GridRebind;
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
                // set default date range
                drpFilterDates.LowerValue = new DateTime( DateTime.Now.Year, 1, 1 );
                drpFilterDates.UpperValue = DateTime.Now;
                
                // load account list
                LoadAccounts();

                BindGrid();
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        /// <summary>
        /// Handles the GridRebind event of the gTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gTransactions_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the bbtnApply control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bbtnApply_Click( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        private void LoadAccounts()
        {
            var rockContext = new RockContext();
            FinancialAccountService accountService = new FinancialAccountService(rockContext);

            List<Guid> selectedAccounts = new List<Guid>(); ;

            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "Accounts" ) ) )
            {
                selectedAccounts = GetAttributeValue( "Accounts" ).Split( ',' ).Select( Guid.Parse ).ToList();
            }

            var accounts = accountService.Queryable()
                                .Where( a => selectedAccounts.Contains( a.Guid ) )
                                .OrderBy( a => a.Order );

            foreach ( var account in accounts.ToList() )
            {
                ListItem checkbox = new ListItem(account.PublicName, account.Id.ToString(), true);
                checkbox.Selected = true;

                cblAccounts.Items.Add( checkbox );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            RockContext rockContext = new RockContext();
            FinancialTransactionService transService = new FinancialTransactionService( rockContext );

            // get list of selected accounts
            List<int> selectedAccountIds = cblAccounts.Items.Cast<ListItem>()
                                            .Where( i => i.Selected == true )
                                            .Select( i => int.Parse( i.Value ) ).ToList();

            var qry = transService.Queryable( "TransactionDetails.Account,FinancialPaymentDetail" )
                        .Where( t => 
                            t.TransactionDetails.Any( d => selectedAccountIds.Contains( d.AccountId ) ) && 
                            t.AuthorizedPersonAlias != null &&
                            t.AuthorizedPersonAlias.Person != null &&
                            t.AuthorizedPersonAlias.Person.GivingGroupId == CurrentPerson.GivingGroupId );

            if (drpFilterDates.LowerValue.HasValue) {
                qry = qry.Where(t => t.TransactionDateTime.Value >= drpFilterDates.LowerValue.Value);
            }

            if ( drpFilterDates.UpperValue.HasValue )
            {
                var lastDate = drpFilterDates.UpperValue.Value.AddDays( 1 ); // add one day to ensure we get all transactions till midnight
                qry = qry.Where( t => t.TransactionDateTime.Value < lastDate ); 
            }

            var txns = qry.ToList();

            // get account totals
            Dictionary<string, decimal> accountTotals = new Dictionary<string, decimal>();

            foreach ( var transaction in txns )
            {
                foreach ( var transactionDetail in transaction.TransactionDetails )
                {
                    if ( accountTotals.Keys.Contains( transactionDetail.Account.Name ) )
                    {
                        accountTotals[transactionDetail.Account.Name] += transactionDetail.Amount;
                    }
                    else
                    {
                        accountTotals.Add( transactionDetail.Account.Name, transactionDetail.Amount );
                    }
                }
            }

            lAccountSummary.Text = string.Empty;
            if ( accountTotals.Count > 0 )
            {
                pnlSummary.Visible = true;
                foreach ( var key in accountTotals.Keys )
                {
                    lAccountSummary.Text += String.Format( "<li>{0}: ${1}</li>", key, accountTotals[key] );
                }
            }
            else
            {
                pnlSummary.Visible = false;
            }

            gTransactions.EntityTypeId = EntityTypeCache.Read<FinancialTransaction>().Id;
            gTransactions.DataSource = txns.Select( t => new
            {
                t.Id,
                t.TransactionDateTime,
                CurrencyType = FormatCurrencyType( t ),
                Summary = FormatSummary( t ),
                t.TotalAmount
            } ).ToList();
            gTransactions.DataBind();

        }

        protected string FormatCurrencyType( FinancialTransaction txn )
        {
            string currencyType = string.Empty;
            string creditCardType = string.Empty;

            if ( txn.FinancialPaymentDetail != null && txn.FinancialPaymentDetail.CurrencyTypeValueId.HasValue )
            {
                int currencyTypeId = txn.FinancialPaymentDetail.CurrencyTypeValueId.Value;

                var currencyTypeValue = DefinedValueCache.Read( currencyTypeId );
                currencyType = currencyTypeValue != null ? currencyTypeValue.Value : string.Empty;


                if ( txn.FinancialPaymentDetail.CreditCardTypeValueId.HasValue )
                {
                    int creditCardTypeId = txn.FinancialPaymentDetail.CreditCardTypeValueId.Value;
                    var creditCardTypeValue = DefinedValueCache.Read( creditCardTypeId );
                    creditCardType = creditCardTypeValue != null ? creditCardTypeValue.Value : string.Empty;

                    return string.Format( "{0} - {1}", currencyType, creditCardType );
                }
            }

            return currencyType;
        }


        private string FormatSummary( FinancialTransaction txn )
        {
            var sb = new StringBuilder();
            foreach ( var transactionDetail in txn.TransactionDetails )
            {
                sb.AppendFormat( "{0} (${1})<br>", transactionDetail.Account, transactionDetail.Amount );
            }
            return sb.ToString();
        }

        #endregion
        
}
}
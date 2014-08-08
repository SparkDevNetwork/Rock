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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// view an existing scheduled transaction.
    /// </summary>
    [DisplayName( "Scheduled Transaction View" )]
    [Category( "Finance" )]
    [Description( "View an existing scheduled transaction." )]

    [LinkedPage("Update Page", "The page used to update in existing scheduled transaction.")]
    public partial class ScheduledTransactionView : RockBlock
    {

        #region Properties

        private Dictionary<int, string> _accountNames = null;
        private Dictionary<int, string> AccountNames
        {
            get
            {
                if ( _accountNames == null )
                {
                    _accountNames = new Dictionary<int, string>();
                    new FinancialAccountService( new RockContext() ).Queryable()
                        .OrderBy( a => a.Order )
                        .Select( a => new { a.Id, a.Name } )
                        .ToList()
                        .ForEach( a => _accountNames.Add( a.Id, a.Name ) );
                }
                return _accountNames;
            }
        }

        #endregion

        #region base control methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            string script = @"
    $('a.js-cancel-txn').click(function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to cancel this scheduled transaction?', function (result) {
            if (result) {
                eval(e.target.href);
            }
        });
    });

    $('a.js-reactivate-txn').click(function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to reactivate this scheduled transaction?', function (result) {
            if (result) {
                eval(e.target.href);
            }
        });
    });
";
            ScriptManager.RegisterStartupScript( lbCancel, lbCancel.GetType(), "update-txn-status", script, true );
        }
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbError.Visible = false;

            if (!Page.IsPostBack)
            {
                ShowView( GetScheduledTransaction() );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbUpdate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbUpdate_Click( object sender, EventArgs e )
        {
            var txn = GetScheduledTransaction();
            if ( txn != null && txn.AuthorizedPerson != null )
            {
                var parms = new Dictionary<string, string>();
                parms.Add( "ScheduledTransactionId", txn.Id.ToString() );
                parms.Add( "Person", txn.AuthorizedPerson.UrlEncodedKey );
                NavigateToLinkedPage( "UpdatePage", parms );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbRefresh control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRefresh_Click( object sender, EventArgs e )
        {
            int? txnId = PageParameter( "Txn" ).AsIntegerOrNull();
            if ( txnId.HasValue )
            {
                var rockContext = new RockContext();
                var txnService = new FinancialScheduledTransactionService( rockContext );
                var txn = txnService.Get( txnId.Value );
                if ( txn != null )
                {
                    string errorMessage = string.Empty;
                    if ( txnService.GetStatus( txn, out errorMessage ) )
                    {
                        rockContext.SaveChanges();
                    }
                    else
                    {
                        ShowErrorMessage( errorMessage );
                    }
                    ShowView( txn );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            int? txnId = PageParameter( "ScheduledTransactionId" ).AsIntegerOrNull();
            if ( txnId.HasValue )
            {
                var rockContext = new RockContext();
                var txnService = new FinancialScheduledTransactionService( rockContext );
                var txn = txnService.Get( txnId.Value );
                if ( txn != null )
                {
                    var gateWay = GetGateway( txn );
                    if ( gateWay != null )
                    {
                        string errorMessage = string.Empty;
                        if ( gateWay.CancelScheduledPayment( txn, out errorMessage ) )
                        {
                            var noteTypeService = new NoteTypeService( rockContext );
                            var noteType = noteTypeService.Get( txn.TypeId, "Note" );

                            var noteService = new NoteService( rockContext );
                            var note = new Note();
                            note.NoteTypeId = noteType.Id;
                            note.EntityId = txn.Id;
                            note.Caption = "Cancelled Transaction";
                            noteService.Add( note );
                        }
                        else
                        {
                            ShowErrorMessage( errorMessage );
                        }

                        txnService.GetStatus(txn, out errorMessage);

                        rockContext.SaveChanges();

                        ShowView( txn );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbReactivate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbReactivate_Click( object sender, EventArgs e )
        {
            int? txnId = PageParameter( "ScheduledTransactionId" ).AsIntegerOrNull();
            if ( txnId.HasValue )
            {
                var rockContext = new RockContext();
                var txnService = new FinancialScheduledTransactionService( rockContext );
                var txn = txnService.Get( txnId.Value );
                if ( txn != null )
                {
                    var gateWay = GetGateway( txn );
                    if ( gateWay != null )
                    {
                        string errorMessage = string.Empty;
                        if ( gateWay.ReactivateScheduledPayment( txn, out errorMessage ) )
                        {
                            var noteTypeService = new NoteTypeService( rockContext );
                            var noteType = noteTypeService.Get( txn.TypeId, "Note" );

                            var noteService = new NoteService( rockContext );
                            var note = new Note();
                            note.NoteTypeId = noteType.Id;
                            note.EntityId = txn.Id;
                            note.Caption = "Reactivated Transaction";
                            noteService.Add( note );
                        }
                        else
                        {
                            ShowErrorMessage( errorMessage );
                        }

                        txnService.GetStatus( txn, out errorMessage );

                        rockContext.SaveChanges();

                        ShowView( txn );
                    }
                }
            }
        }

        #endregion

        #region  Methods

        /// <summary>
        /// Shows the view.
        /// </summary>
        /// <param name="txn">The TXN.</param>
        private void ShowView( FinancialScheduledTransaction txn )
        {
            if ( txn != null )
            {
                hlStatus.Text = txn.IsActive ? "Active" : "Inactive";
                hlStatus.LabelType = txn.IsActive ? LabelType.Success : LabelType.Danger;

                string rockUrlRoot = ResolveRockUrl( "/" );

                var detailsLeft = new DescriptionList()
                    .Add( "Person", txn.AuthorizedPerson != null ? txn.AuthorizedPerson.GetAnchorTag( rockUrlRoot ) : string.Empty );

                var detailsRight = new DescriptionList()
                    .Add( "Amount", ( txn.ScheduledTransactionDetails.Sum( d => (decimal?)d.Amount ) ?? 0.0M ).ToString( "C2" ) )
                    .Add( "Frequency", txn.TransactionFrequencyValue != null ? txn.TransactionFrequencyValue.Name : string.Empty )
                    .Add( "Start Date", txn.StartDate.ToShortDateString() )
                    .Add( "End Date", txn.EndDate.HasValue ? txn.EndDate.Value.ToShortDateString() : string.Empty )
                    .Add( "Next Payment Date", txn.NextPaymentDate.HasValue ? txn.NextPaymentDate.Value.ToShortDateString() : string.Empty )
                    .Add( "Last Status Refresh", txn.LastStatusUpdateDateTime.HasValue ? txn.LastStatusUpdateDateTime.Value.ToString( "g" ) : string.Empty );

                if ( txn.CurrencyTypeValue != null )
                {
                    string currencyType = txn.CurrencyTypeValue.Name;
                    if ( txn.CurrencyTypeValue.Guid.Equals( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() ) )
                    {
                        currencyType += txn.CreditCardTypeValue != null ? ( " - " + txn.CreditCardTypeValue.Name ) : string.Empty;
                    }
                    detailsLeft.Add( "Currency Type", currencyType );
                }

                if ( txn.GatewayEntityType != null )
                {
                    detailsLeft.Add( "Payment Gateway", Rock.Financial.GatewayContainer.GetComponentName( txn.GatewayEntityType.Name ) );
                }

                detailsLeft
                    .Add( "Transaction Code", txn.TransactionCode )
                    .Add( "Schedule Id", txn.GatewayScheduleId );

                lDetailsLeft.Text = detailsLeft.Html;
                lDetailsRight.Text = detailsRight.Html;

                gAccountsView.DataSource = txn.ScheduledTransactionDetails.ToList();
                gAccountsView.DataBind();

                var rockContext = new RockContext();
                var noteType = new NoteTypeService( rockContext ).Get( txn.TypeId, "Note" );
                rptrNotes.DataSource = new NoteService( rockContext ).Get( noteType.Id, txn.Id )
                    .Where( n => n.CreatedDateTime.HasValue )
                    .OrderBy( n => n.CreatedDateTime )
                    .ToList()
                    .Select( n => new
                    {
                        n.Caption,
                        Text = n.Text.ConvertCrLfToHtmlBr(),
                        Person = n.CreatedByPersonAlias.Person.FullName,
                        Date = n.CreatedDateTime.Value.ToShortDateString(),
                        Time = n.CreatedDateTime.Value.ToShortTimeString()
                    } )
                    .ToList();
                rptrNotes.DataBind();

                lbCancel.Visible = txn.IsActive;
                lbReactivate.Visible = !txn.IsActive;
            }
        }

        /// <summary>
        /// Gets the scheduled transaction.
        /// </summary>
        /// <returns></returns>
        private FinancialScheduledTransaction GetScheduledTransaction()
        {
            int? txnId = PageParameter( "ScheduledTransactionId" ).AsIntegerOrNull();
            if (txnId.HasValue)
            {
                var rockContext = new RockContext();
                var service = new FinancialScheduledTransactionService( rockContext );
                return service
                    .Queryable( "ScheduledTransactionDetails,GatewayEntityType,CurrencyTypeValue,CreditCardTypeValue" )
                    .Where( t => t.Id == txnId.Value )
                    .FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// Gets the gateway.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <returns></returns>
        private GatewayComponent GetGateway( FinancialScheduledTransaction scheduledTransaction )
        {
            if ( scheduledTransaction.GatewayEntityType != null )
            {
                Guid gatewayGuid = scheduledTransaction.GatewayEntityType.Guid;
                var gateway = GatewayContainer.GetComponent( gatewayGuid.ToString() );
                if ( gateway != null && gateway.IsActive )
                {
                    return gateway;
                }
            }

            return null;
        }

        /// <summary>
        /// Accounts the name.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        protected string AccountName( int accountId )
        {
            return AccountNames.ContainsKey( accountId ) ? AccountNames[accountId] : "";
        }

        private void ShowErrorMessage(string message)
        {
            nbError.Text = message;
            nbError.Visible = true;
        }

        #endregion

}
}
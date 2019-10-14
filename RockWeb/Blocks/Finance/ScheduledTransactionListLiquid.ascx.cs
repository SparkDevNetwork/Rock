// <copyright>
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Generates a list of scheduled transactions for the current person with edit/transfer and delete buttons.
    /// </summary>
    [DisplayName( "Scheduled Transaction List Lava" )]
    [Category( "Finance" )]
    [Description( "Block that shows a list of scheduled transactions for the currently logged in user with the ability to modify the formatting using a lava template." )]

    [CodeEditorField( "Template",
        Key = AttributeKey.Template,
        Description = "Lava template for the display of the scheduled transactions.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = @"{% include '~~/Assets/Lava/ScheduledTransactionListLiquid.lava'  %}",
        Order = 1 )]

    [LinkedPage(
        "Scheduled Transaction Entry Page",
        Key = AttributeKey.ScheduledTransactionEntryPage,
        Description = "Link to use when adding new transactions.",
        IsRequired = false,
        Order = 4 )]

    [TextField( "Transaction Label",
        Key = AttributeKey.TransactionLabel,
        Description = "The label to use to describe the transaction (e.g. 'Gift', 'Donation', etc.)",
        IsRequired = true,
        DefaultValue = "Gift",
        Order = 5 )]

    [FinancialGatewayField(
        "Gateway Filter",
        Key = AttributeKey.GatewayFilter,
        Description = "When set, causes only scheduled transaction's of a particular gateway to be shown.",
        IsRequired = false,
        Order = 6 )]

    [FinancialGatewayField(
        "Transfer-To Gateway",
        Key = AttributeKey.TransferToGateway,
        Description = "Set this if you want people to transfer their existing scheduled transactions to this new gateway. When set, the Edit button becomes 'Transfer' (default or whatever is set in the 'Transfer Button Text' setting) if the scheduled transaction's gateway does not match the transfer-to gateway.",
        IsRequired = false,
        Order = 7 )]

    [TextField(
        "Transfer Button Text",
        Key = AttributeKey.TransferButtonText,
        Description = "The text to use on the transfer (edit) button which is used when a Transfer-To gateway is set.",
        IsRequired = true,
        DefaultValue = "Transfer",
        Order = 8 )]

    [LinkedPage(
        "Scheduled Transaction Edit Page",
        Key = AttributeKey.ScheduledTransactionEditPage,
        Description = "The page used to update an existing scheduled transaction for Gateways that don't support a hosted payment interface.",
        Order = 9 )]

    [LinkedPage(
        "Scheduled Transaction Edit Page for Hosted Gateways",
        Key = AttributeKey.ScheduledTransactionEditPageHosted,
        Description = "The page used to update an existing scheduled transaction for Gateways that support a hosted payment interface.",
        IsRequired = false,
        Order = 10 )]

    public partial class ScheduledTransactionListLiquid : RockBlock
    {
        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        protected static class AttributeKey
        {
            public const string Template = "Template";
            public const string ScheduledTransactionEditPage = "ScheduledTransactionEditPage";
            public const string ScheduledTransactionEditPageHosted = "ScheduledTransactionEditPageHosted";
            public const string ScheduledTransactionEntryPage = "ScheduledTransactionEntryPage";
            public const string TransactionLabel = "TransactionLabel";
            public const string GatewayFilter = "GatewayFilter";
            public const string TransferToGateway = "TransferToGateway";
            public const string TransferButtonText = "TransferButtonText";
        }

        #region Fields

        /// <summary>
        /// The _transfer to gateway unique identifier is set to non-null if the block setting is set.
        /// </summary>
        private Guid? _transferToGatewayGuid = null;

        /// <summary>
        /// This constant-like value is used to avoid hard-coding the string "transfer" in multiple places.
        /// </summary>
        private const string TRANSFER = "transfer";

        #endregion

        #region Base Control Methods

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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            lbAddScheduledTransaction.Text = string.Format( "Create New {0}", GetAttributeValue( AttributeKey.TransactionLabel ) );
            _transferToGatewayGuid = GetAttributeValue( AttributeKey.TransferToGateway ).AsGuidOrNull();
            lbAddScheduledTransaction.Visible = GetAttributeValue( AttributeKey.ScheduledTransactionEntryPage ).IsNotNullOrWhiteSpace();

            // set initial info
            if ( !IsPostBack )
            {
                ShowContent();
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
            ShowContent();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptScheduledTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptScheduledTransactions_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var transactionSchedule = e.Item.DataItem as FinancialScheduledTransaction;

                HiddenField hfScheduledTransactionId = ( HiddenField ) e.Item.FindControl( "hfScheduledTransactionId" );
                hfScheduledTransactionId.Value = transactionSchedule.Id.ToString();

                // create dictionary for liquid
                Dictionary<string, object> scheduleSummary = new Dictionary<string, object>();
                scheduleSummary.Add( "Id", transactionSchedule.Id );
                scheduleSummary.Add( "Guid", transactionSchedule.Guid );
                scheduleSummary.Add( "StartDate", transactionSchedule.StartDate );
                scheduleSummary.Add( "EndDate", transactionSchedule.EndDate );
                scheduleSummary.Add( "NextPaymentDate", transactionSchedule.NextPaymentDate );

                // If a Transfer-To gateway was set and this transaction is not using that gateway, change the Edit button to a Transfer button
                if ( _transferToGatewayGuid != null && transactionSchedule.FinancialGateway.Guid != _transferToGatewayGuid )
                {
                    Button btnEdit = ( Button ) e.Item.FindControl( "btnEdit" );
                    btnEdit.Text = GetAttributeValue( AttributeKey.TransferButtonText );

                    HiddenField hfTransfer = ( HiddenField ) e.Item.FindControl( "hfTransfer" );
                    hfTransfer.Value = TRANSFER;
                }

                if ( transactionSchedule.NextPaymentDate.HasValue )
                {
                    scheduleSummary.Add( "DaysTillNextPayment", ( transactionSchedule.NextPaymentDate.Value - DateTime.Now ).Days );
                }
                else
                {
                    scheduleSummary.Add( "DaysTillNextPayment", null );
                }

                DateTime? lastPaymentDate = transactionSchedule.Transactions.Max( t => t.TransactionDateTime );
                scheduleSummary.Add( "LastPaymentDate", lastPaymentDate );

                if ( lastPaymentDate.HasValue )
                {
                    scheduleSummary.Add( "DaysSinceLastPayment", ( DateTime.Now - lastPaymentDate.Value ).Days );
                }
                else
                {
                    scheduleSummary.Add( "DaysSinceLastPayment", null );
                }

                scheduleSummary.Add( "PersonName", transactionSchedule.AuthorizedPersonAlias != null && transactionSchedule.AuthorizedPersonAlias.Person != null ? transactionSchedule.AuthorizedPersonAlias.Person.FullName : string.Empty );
                scheduleSummary.Add( "CurrencyType", ( transactionSchedule.FinancialPaymentDetail != null && transactionSchedule.FinancialPaymentDetail.CurrencyTypeValue != null ) ? transactionSchedule.FinancialPaymentDetail.CurrencyTypeValue.Value : string.Empty );
                scheduleSummary.Add( "CreditCardType", ( transactionSchedule.FinancialPaymentDetail != null && transactionSchedule.FinancialPaymentDetail.CreditCardTypeValue != null ) ? transactionSchedule.FinancialPaymentDetail.CreditCardTypeValue.Value : string.Empty );
                scheduleSummary.Add( "AccountNumberMasked", ( transactionSchedule.FinancialPaymentDetail != null ) ? transactionSchedule.FinancialPaymentDetail.AccountNumberMasked ?? string.Empty : string.Empty );
                scheduleSummary.Add( "UrlEncryptedKey", transactionSchedule.UrlEncodedKey );
                scheduleSummary.Add( "Frequency", transactionSchedule.TransactionFrequencyValue.Value );
                scheduleSummary.Add( "FrequencyDescription", transactionSchedule.TransactionFrequencyValue.Description );

                List<Dictionary<string, object>> summaryDetails = new List<Dictionary<string, object>>();
                decimal totalAmount = 0;

                foreach ( FinancialScheduledTransactionDetail detail in transactionSchedule.ScheduledTransactionDetails )
                {
                    Dictionary<string, object> detailSummary = new Dictionary<string, object>();
                    detailSummary.Add( "AccountId", detail.Id );
                    detailSummary.Add( "AccountName", detail.Account.Name );
                    detailSummary.Add( "Amount", detail.Amount );
                    detailSummary.Add( "Summary", detail.Summary );

                    summaryDetails.Add( detailSummary );

                    totalAmount += detail.Amount;
                }

                scheduleSummary.Add( "ScheduledAmount", totalAmount );
                scheduleSummary.Add( "TransactionDetails", summaryDetails );

                Dictionary<string, object> schedule = new Dictionary<string, object>();
                schedule.Add( "ScheduledTransaction", scheduleSummary );

                // merge into content
                Literal lLiquidContent = ( Literal ) e.Item.FindControl( "lLiquidContent" );
                lLiquidContent.Text = GetAttributeValue( AttributeKey.Template ).ResolveMergeFields( schedule );
            }
        }

        /// <summary>
        /// Handles the Click event of the bbtnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bbtnDelete_Click( object sender, EventArgs e )
        {
            BootstrapButton bbtnDelete = ( BootstrapButton ) sender;
            RepeaterItem riItem = ( RepeaterItem ) bbtnDelete.NamingContainer;

            HiddenField hfScheduledTransactionId = ( HiddenField ) riItem.FindControl( "hfScheduledTransactionId" );
            Literal content = ( Literal ) riItem.FindControl( "lLiquidContent" );
            Button btnEdit = ( Button ) riItem.FindControl( "btnEdit" );

            using ( var rockContext = new Rock.Data.RockContext() )
            {
                FinancialScheduledTransactionService fstService = new FinancialScheduledTransactionService( rockContext );
                var currentTransaction = fstService.Get( hfScheduledTransactionId.Value.AsInteger() );
                if ( currentTransaction != null && currentTransaction.FinancialGateway != null )
                {
                    currentTransaction.FinancialGateway.LoadAttributes( rockContext );
                }

                string errorMessage = string.Empty;
                if ( fstService.Cancel( currentTransaction, out errorMessage ) )
                {
                    try
                    {
                        fstService.GetStatus( currentTransaction, out errorMessage );
                    }
                    catch
                    {
                        // Ignore
                    }

                    rockContext.SaveChanges();
                    content.Text = string.Format( "<div class='alert alert-success'>Your recurring {0} has been deleted.</div>", GetAttributeValue( AttributeKey.TransactionLabel ).ToLower() );
                }
                else
                {
                    content.Text = string.Format( "<div class='alert alert-danger'>An error occurred while deleting your scheduled transaction. Message: {0}</div>", errorMessage );
                }
            }

            bbtnDelete.Visible = false;
            btnEdit.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            Button btnEdit = ( Button ) sender;
            RepeaterItem riItem = ( RepeaterItem ) btnEdit.NamingContainer;

            HiddenField hfScheduledTransactionId = ( HiddenField ) riItem.FindControl( "hfScheduledTransactionId" );
            var scheduledTransactionId = hfScheduledTransactionId.Value.AsIntegerOrNull();
            HiddenField hfTransfer = ( HiddenField ) riItem.FindControl( "hfTransfer" );

            if ( !scheduledTransactionId.HasValue )
            {
                return;
            }

            var financialScheduledTransaction = new FinancialScheduledTransactionService( new RockContext() ).Get( scheduledTransactionId.Value );
            if ( financialScheduledTransaction == null )
            {
                return;
            }

            Dictionary<string, string> qryParams = new Dictionary<string, string>();
            qryParams.Add( "ScheduledTransactionId", scheduledTransactionId.Value.ToString() );

            // If this is a transfer, go to the TransactionEntry page/block
            if ( _transferToGatewayGuid != null && hfTransfer.Value == TRANSFER && !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.ScheduledTransactionEntryPage ) ) )
            {
                qryParams.Add( TRANSFER, "true" );
                this.NavigateToLinkedPage( AttributeKey.ScheduledTransactionEntryPage, qryParams );
            }
            else
            {
                if ( financialScheduledTransaction.FinancialGateway.GetGatewayComponent() is IHostedGatewayComponent )
                {
                    NavigateToLinkedPage( AttributeKey.ScheduledTransactionEditPageHosted, qryParams );
                }
                else
                {
                    NavigateToLinkedPage( AttributeKey.ScheduledTransactionEditPage, qryParams );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddScheduledTransaction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddScheduledTransaction_Click( object sender, EventArgs e )
        {
            this.NavigateToLinkedPage( AttributeKey.ScheduledTransactionEntryPage );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the content.
        /// </summary>
        private void ShowContent()
        {
            // get scheduled contributions for current user
            if ( CurrentPerson != null )
            {
                var rockContext = new RockContext();
                var transactionService = new FinancialScheduledTransactionService( rockContext );
                var personService = new PersonService( rockContext );

                // get business giving id
                var givingIds = personService.GetBusinesses( CurrentPerson.Id ).Select( g => g.GivingId ).ToList();

                // add the person's regular giving id
                givingIds.Add( CurrentPerson.GivingId );

                var schedules = transactionService.Queryable()
                    .Include( a => a.ScheduledTransactionDetails.Select( s => s.Account ) )
                    .Where( s => givingIds.Contains( s.AuthorizedPersonAlias.Person.GivingId ) && s.IsActive == true );

                // filter the list if necessary
                var gatewayFilterGuid = GetAttributeValue( AttributeKey.GatewayFilter ).AsGuidOrNull();
                if ( gatewayFilterGuid != null )
                {
                    schedules = schedules.Where( s => s.FinancialGateway.Guid == gatewayFilterGuid );
                }

                rptScheduledTransactions.DataSource = schedules.ToList();
                rptScheduledTransactions.DataBind();

                if ( schedules.Count() == 0 )
                {
                    pnlNoScheduledTransactions.Visible = true;
                    lNoScheduledTransactionsMessage.Text = string.Format( "No {0} currently exist.", GetAttributeValue( AttributeKey.TransactionLabel ).Pluralize().ToLower() );
                }
            }
        }

        #endregion
    }
}
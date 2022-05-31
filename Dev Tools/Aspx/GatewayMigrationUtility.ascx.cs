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
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.AspNet.SignalR;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.MyWell;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Financial Gateway Migration Utility
    /// </summary>
    [DisplayName( "Financial Gateway Migration Utility" )]
    [Category( "Finance" )]
    [Description( "Tool to assist in migrating records from NMI to My Well." )]

    #region Block Attributes

    #endregion Block Attributes
    public partial class GatewayMigrationUtility : RockBlock
    {

        #region UserPreference Keys

        private static class UserPreferenceKey
        {
            public const string MigrateSavedAccountsResultSummary = "MigrateSavedAccountsResultSummary";
            public const string MigrateScheduledTransactionsResultSummary = "MigrateScheduledTransactionsResultSummary";
            public const string MigrateSavedAccountsResultDetails = "MigrateSavedAccountsResultDetails";
            public const string MigrateScheduledTransactionsResultDetails = "MigrateScheduledTransactionsResultDetails";
            public const string MigrateSavedAccountsResultFileURL = "MigrateSavedAccountsResultFileURL";
            public const string MigrateScheduledTransactionsResultFileURL = "MigrateScheduledTransactionsResultFileURL";
        }

        #endregion UserPreference Keys

        /// <summary>
        /// This holds the reference to the RockMessageHub SignalR Hub context.
        /// </summary>
        private IHubContext _hubContext = GlobalHost.ConnectionManager.GetHubContext<RockMessageHub>();

        /// <summary>
        /// Gets the signal r notification key.
        /// </summary>
        /// <value>
        /// The signal r notification key.
        /// </value>
        public string SignalRNotificationKey
        {
            get
            {
                return string.Format( "GatewayMigrationUtility_BlockId:{0}_SessionId:{1}", this.BlockId, Session.SessionID );
            }
        }

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            RockPage.AddScriptLink( "~/Scripts/jquery.signalR-2.2.0.min.js", false );
            gNMIPersonProfiles.GridRebind += GNMIPersonProfiles_GridRebind;
        }

        /// <summary>
        /// Handles the GridRebind event of the GNMIPersonProfiles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        private void GNMIPersonProfiles_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            // The ViewState of the scheduled transaction results might not be correct since the scheduled transaction migration runs in a separate thread
            // So, make sure the results are updated on every post back
            ShowScheduledTransactionResults();

            if ( !Page.IsPostBack )
            {
                ShowDetails();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the details.
        /// </summary>
        protected void ShowDetails()
        {
            var migrateSavedAccountsResultSummary = this.GetBlockUserPreference( UserPreferenceKey.MigrateSavedAccountsResultSummary );
            var migrateSavedAccountsResultDetails = this.GetBlockUserPreference( UserPreferenceKey.MigrateSavedAccountsResultDetails );
            hfMigrateSavedAccountsResultFileURL.Value = this.GetBlockUserPreference( UserPreferenceKey.MigrateSavedAccountsResultFileURL );
            pnlMigrateSavedAccountsResults.Visible = false;
            pnlMigrateScheduledTransactionsResults.Visible = false;

            if ( migrateSavedAccountsResultSummary.IsNotNullOrWhiteSpace() )
            {
                nbMigrateSavedAccountsResults.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Info;
                nbMigrateSavedAccountsResults.Text = "Migrate Saved Accounts has already been run.";
                nbMigrateSavedAccountsResults.Details = string.Format( "<pre>{0}</pre>", migrateSavedAccountsResultDetails.ToString() );
                pnlMigrateSavedAccountsResults.Visible = true;
            }

            hfMigrateScheduledTransactionsResultFileURL.Value = this.GetBlockUserPreference( UserPreferenceKey.MigrateScheduledTransactionsResultFileURL );

            nbMigrateScheduledTransactionsResult.Text = string.Empty;

            ShowScheduledTransactionResults();

            var rockContext = new RockContext();
            var financialGatewayService = new FinancialGatewayService( rockContext );
            var activeGatewayList = financialGatewayService.Queryable().Where( a => a.IsActive == true ).AsNoTracking().ToList();
            var myWellGateways = activeGatewayList.Where( a => a.GetGatewayComponent() is MyWellGateway ).ToList();
            ddlMyWellGateway.Items.Clear();
            foreach ( var myWellGateway in myWellGateways )
            {
                ddlMyWellGateway.Items.Add( new ListItem( myWellGateway.Name, myWellGateway.Id.ToString() ) );
            }

            var nmiGateways = activeGatewayList.Where( a => a.GetGatewayComponent() is Rock.NMI.Gateway ).ToList();
            ddlNMIGateway.Items.Clear();
            foreach ( var nmiGateway in nmiGateways )
            {
                ddlNMIGateway.Items.Add( new ListItem( nmiGateway.Name, nmiGateway.Id.ToString() ) );
            }

            PageReference downloadPaymentsPage = new PageReference( PageCache.GetId( Rock.SystemGuid.Page.DOWNLOAD_PAYMENTS.AsGuid() ) ?? 0 );

            nbScheduledTransactionsDownloadPayments.Text = string.Format( @"Before Migrating Scheduled Transactions:
<ol>
    <li>Go to the <a href='{0}'>Download Payments</a> page.</li>
    <li>Make sure the block settings are set correctly.</li>
    <li>Set Gateway to the NMI Gateway.</li>
    <li>Set Date Range to include today.</li>
    <li>Download Payments</li>
</ol>", downloadPaymentsPage.BuildUrl() );

            BindGrid();
        }

        /// <summary>
        /// Shows the scheduled transaction results.
        /// </summary>
        private void ShowScheduledTransactionResults()
        {
            var migrateScheduledTransactionsResultSummary = this.GetBlockUserPreference( UserPreferenceKey.MigrateScheduledTransactionsResultSummary );
            var migrateScheduledTransactionsResultDetails = this.GetBlockUserPreference( UserPreferenceKey.MigrateScheduledTransactionsResultDetails );
            if ( migrateScheduledTransactionsResultSummary.IsNotNullOrWhiteSpace() )
            {
                nbMigrateScheduledTransactionsResult.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Info;
                nbMigrateScheduledTransactionsResult.Text = "Migrate Scheduled Transactions has already been run.";
                nbMigrateScheduledTransactionsResult.Details = string.Format( "<pre>{0}</pre>", migrateScheduledTransactionsResultDetails.ToString() );
                pnlMigrateScheduledTransactionsResults.Visible = true;
                btnDownloadScheduledTransactionsResultsJSON.Style[HtmlTextWriterStyle.Display] = "";
            }

            var oneTimeFrequencyId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME.AsGuid() );
            var oneTimeScheduledTransactionQry = new FinancialScheduledTransactionService( new RockContext() ).Queryable()
                .Where( a => a.IsActive && a.TransactionFrequencyValueId == oneTimeFrequencyId && !string.IsNullOrEmpty( a.GatewayScheduleId ) )
                .Where( a => a.FinancialGatewayId.HasValue );

            var oneTimeScheduledTransactionCountText = oneTimeScheduledTransactionQry
                .GroupBy( a => a.FinancialGateway.Name )
                .Select( a => new
                {
                    GatewayName = a.Key,
                    TransactionCount = a.Count()
                } ).ToList().Select( a => string.Format( "<li>{0} - {1}</li>", a.GatewayName, a.TransactionCount ) ).ToList().AsDelimited( "" );

            if ( oneTimeScheduledTransactionQry.Any() )
            {
                nbUpdateOneTimeScheduleStatus.Text = string.Format( "Active one-time scheduled transactions remaining: <ul> {0} </ul> ", oneTimeScheduledTransactionCountText );
            }
            else
            {
                nbUpdateOneTimeScheduleStatus.Text = "No one-time scheduled transactions remaining.";
            }
        }

        public Dictionary<int, List<FinancialPersonSavedAccount>> _financialPersonSavedAccountLookupByPersonId = null;
        public Dictionary<int, List<FinancialScheduledTransaction>> _financialScheduledTransactionLookupByPersonId = null;

        /// <summary>
        /// Binds the grid.
        /// </summary>
        public void BindGrid()
        {
            var rockContext = new RockContext();
            var nmiGatewayId = ddlNMIGateway.SelectedValue.AsIntegerOrNull();
            if ( !nmiGatewayId.HasValue )
            {
                return;
            }

            var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );

            var financialPersonSavedAccountsQuery = new FinancialPersonSavedAccountService( rockContext ).Queryable()
                .Where( a => a.FinancialGatewayId == nmiGatewayId && a.PersonAliasId.HasValue );

            var scheduledTransactionsQuery = financialScheduledTransactionService.Queryable()
                .Where( a => a.FinancialGatewayId == nmiGatewayId && a.IsActive )
                .Include( a => a.FinancialGateway );

            var scheduledTransactionsList = scheduledTransactionsQuery.ToList();

            var nmiPersonQuery = new PersonService( rockContext ).Queryable()
                .Where( p =>
                     financialPersonSavedAccountsQuery.Any( sa => sa.PersonAlias.PersonId == p.Id )
                     || scheduledTransactionsQuery.Any( st => st.AuthorizedPersonAlias.PersonId == p.Id )
                    ).AsNoTracking();

            _financialPersonSavedAccountLookupByPersonId = financialPersonSavedAccountsQuery.GroupBy( a => a.PersonAlias.PersonId ).ToDictionary( k => k.Key, v => v.ToList() );
            _financialScheduledTransactionLookupByPersonId = scheduledTransactionsList.GroupBy( a => a.AuthorizedPersonAlias.PersonId ).ToDictionary( k => k.Key, v => v.ToList() );

            if ( gNMIPersonProfiles.SortProperty != null )
            {
                nmiPersonQuery = nmiPersonQuery.Sort( gNMIPersonProfiles.SortProperty );
            }
            else
            {
                nmiPersonQuery = nmiPersonQuery.OrderBy( a => a.LastName ).ThenBy( a => a.NickName );
            }

            gNMIPersonProfiles.SetLinqDataSource( nmiPersonQuery );
            gNMIPersonProfiles.DataBind();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gNMIPersonProfiles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gNMIPersonProfiles_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var person = e.Row.DataItem as Person;
            if ( person == null )
            {
                return;
            }

            Literal lPerson = e.Row.FindControl( "lPerson" ) as Literal;
            Literal lPersonSavedAccounts = e.Row.FindControl( "lPersonSavedAccounts" ) as Literal;
            Literal lPersonScheduledTransactions = e.Row.FindControl( "lPersonScheduledTransactions" ) as Literal;

            lPerson.Text = person.FullNameReversed;
            var personSavedAccounts = _financialPersonSavedAccountLookupByPersonId.GetValueOrNull( person.Id );
            if ( personSavedAccounts != null && personSavedAccounts.Any() )
            {
                lPersonSavedAccounts.Text = personSavedAccounts.Select( a => string.Format( "{0} ( {1} )", a.Name, a.FinancialPaymentDetail != null ? a.FinancialPaymentDetail.AccountNumberMasked : null ) ).ToList().AsDelimited( "<br />" );
            }

            var scheduledTransactions = _financialScheduledTransactionLookupByPersonId.GetValueOrNull( person.Id );
            if ( scheduledTransactions != null && scheduledTransactions.Any() )
            {
                var rowFormat = @"
<div class='row'>
    <div class='col-md-4'>
        {0}
    </div>
    <div class='col-md-4'>
        {1}
    </div>
    <div class='col-md-4'>
        {2}
    </div>
</div>
";
                lPersonScheduledTransactions.Text =
                    string.Format( rowFormat, "<strong>Amount</strong>", "<strong>Frequency</strong>", "<strong>Next Payment</strong>" )
                    +
                    scheduledTransactions.Select( a => string.Format( rowFormat, a.TotalAmount.FormatAsCurrency(), DefinedValueCache.Get( a.TransactionFrequencyValueId ), a.NextPaymentDate.ToShortDateString() ) ).ToList().AsDelimited( "" );
            }
        }

        #endregion

        #region Migrate Saved Accounts Related

        /// <summary>
        /// Handles the FileUploaded event of the fuCustomerVaultImportFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.FileUploaderEventArgs"/> instance containing the event data.</param>
        protected void fuCustomerVaultImportFile_FileUploaded( object sender, Rock.Web.UI.Controls.FileUploaderEventArgs e )
        {
            ShowScheduledTransactionResults();
            btnMigrateSavedAccounts.Enabled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        private class CustomerVaultImportRecord
        {
            /// <summary>
            /// Gets or sets the NMI customer identifier.
            /// </summary>
            /// <value>
            /// The NMI customer identifier.
            /// </value>
            public string NMICustomerId { get; set; }

            /// <summary>
            /// Gets or sets the My Well customer identifier.
            /// </summary>
            /// <value>
            /// The My Well customer identifier.
            /// </value>
            public string MyWellCustomerId { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        private abstract class MigrationResult
        {
            public string MyWellCustomerId { get; set; }
            public int? PersonId { get; set; }
            public string PersonFullName { get; set; }
            public DateTime MigrationDateTime { get; set; }
            public bool DidMigrateSuccessfully { get; set; }
            public string ResultMessage { get; set; }
            public abstract string GetSummaryDetails();
        }

        /// <summary>
        /// 
        /// </summary>
        private class SavedAccountMigrationResult : MigrationResult
        {
            public string NMICustomerId { get; set; }
            public int FinancialPersonSavedAccountId { get; set; }

            public override string GetSummaryDetails()
            {
                return string.Format( "FinancialPersonSavedAccount.Id: {0} NMI CustomerId: '{1}', My Well CustomerId: '{2}', Result: {3} ",
                FinancialPersonSavedAccountId,
                NMICustomerId,
                MyWellCustomerId,
                this.ResultMessage );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class ScheduledTransactionMigrationResult : MigrationResult
        {
            public int ScheduledTransactionId { get; set; }
            public string NMISubscriptionId { get; internal set; }
            public DateTime? OriginalNextPaymentDate { get; internal set; }
            public DateTime? MyWellSubscriptionStartDate { get; internal set; }
            public bool IsNextPaymentDateAdjusted { get; internal set; }

            public override string GetSummaryDetails()
            {
                return string.Format( "ScheduledTransactionId.Id: {0}, NMI SubscriptionId: '{1}', My Well CustomerId: '{2}', Result: {3} ",
                ScheduledTransactionId,
                NMISubscriptionId,
                MyWellCustomerId,
                this.ResultMessage );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnMigrateSavedAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMigrateSavedAccounts_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            var financialGatewayService = new FinancialGatewayService( rockContext );
            var nmiFinancialGatewayID = ddlNMIGateway.SelectedValue.AsInteger();
            var nmiFinancialGateway = financialGatewayService.Get( nmiFinancialGatewayID );
            var nmiGatewayComponent = nmiFinancialGateway.GetGatewayComponent();
            var myWellFinancialGatewayId = ddlMyWellGateway.SelectedValue.AsInteger();
            var myWellFinancialGateway = financialGatewayService.Get( myWellFinancialGatewayId );
            var myWellGatewayComponent = myWellFinancialGateway.GetGatewayComponent() as IHostedGatewayComponent;

            var financialPersonSavedAccountService = new FinancialPersonSavedAccountService( rockContext );
            var nmiPersonSavedAccountQry = financialPersonSavedAccountService.Queryable().Where( a => a.FinancialGatewayId == nmiFinancialGatewayID && a.PersonAliasId.HasValue );
            string logFileUrl = string.Format( "~/App_Data/Logs/GatewayMigrationUtility_MigrateSavedAccounts_{0}.json", RockDateTime.Now.ToString( "yyyyMMddTHHmmss" ) );

            List<int> personIdsToMigrate = gNMIPersonProfiles.SelectedKeys.Select( a => a as int? ).Where( a => a.HasValue ).Select( a => a.Value ).Distinct().ToList();
            if ( personIdsToMigrate.Any() )
            {
                nmiPersonSavedAccountQry = nmiPersonSavedAccountQry.Where( a => personIdsToMigrate.Contains( a.PersonAlias.PersonId ) );
            }

            var nmiPersonSavedAccountList = nmiPersonSavedAccountQry.ToList();

            var nmiPersonSavedAccountListCount = nmiPersonSavedAccountList.Count();

            List<MigrationResult> migrateSavedAccountResultList = new List<MigrationResult>();

            foreach ( var nmiPersonSavedAccount in nmiPersonSavedAccountList )
            {
                SavedAccountMigrationResult migrateSavedAccountResult = new SavedAccountMigrationResult();

                migrateSavedAccountResult.FinancialPersonSavedAccountId = nmiPersonSavedAccount.Id;
                if ( nmiPersonSavedAccount.PersonAlias != null )
                {
                    migrateSavedAccountResult.PersonId = nmiPersonSavedAccount.PersonAlias.PersonId;
                    migrateSavedAccountResult.PersonFullName = nmiPersonSavedAccount.PersonAlias.Person.FullName;
                }
                else
                {
                    migrateSavedAccountResult.PersonId = null;
                    migrateSavedAccountResult.PersonFullName = "(No person record associated with saved account)";
                }

                // NMI Saves NMI CustomerId to ReferenceNumber and leaves GatewayPersonIdentifier blank, but just in case that changes, look if GatewayPersonIdentifier has a value first
                if ( nmiPersonSavedAccount.GatewayPersonIdentifier.IsNotNullOrWhiteSpace() )
                {
                    migrateSavedAccountResult.NMICustomerId = nmiPersonSavedAccount.GatewayPersonIdentifier;
                }
                else
                {
                    migrateSavedAccountResult.NMICustomerId = nmiPersonSavedAccount.ReferenceNumber;
                }

                if ( migrateSavedAccountResult.NMICustomerId.IsNotNullOrWhiteSpace() )
                {
                    /* 2020-01-13 MDP
                       When MyWell transfers Vault Data from NMI to MyWell, it uses the same ID as NMI for the customer_id.
                    */
                    migrateSavedAccountResult.MyWellCustomerId = migrateSavedAccountResult.NMICustomerId;
                }

                migrateSavedAccountResult.MigrationDateTime = RockDateTime.Now;

                if ( migrateSavedAccountResult.NMICustomerId.IsNullOrWhiteSpace() )
                {
                    migrateSavedAccountResult.DidMigrateSuccessfully = false;
                    migrateSavedAccountResult.ResultMessage = string.Format(
                        "Saved Account (FinancialPersonSavedAccount.Guid: {0},  GatewayPersonIdentifier: {1}, ReferenceNumber: {2}) doesn't have an NMI Customer ID reference",
                        nmiPersonSavedAccount.Guid,
                        nmiPersonSavedAccount.GatewayPersonIdentifier,
                        nmiPersonSavedAccount.ReferenceNumber );
                }
                else if ( migrateSavedAccountResult.MyWellCustomerId.IsNullOrWhiteSpace() )
                {
                    // NOTE: NMI Customer IDs created after the Vault import file was created won't have a myWellCustomerId
                    migrateSavedAccountResult.DidMigrateSuccessfully = false;
                    migrateSavedAccountResult.ResultMessage = string.Format(
                        "NMI CustomerId {0} not found in Vault Import file",
                        migrateSavedAccountResult.NMICustomerId );
                }
                else
                {
                    nmiPersonSavedAccount.GatewayPersonIdentifier = migrateSavedAccountResult.MyWellCustomerId;
                    nmiPersonSavedAccount.FinancialGatewayId = myWellFinancialGatewayId;
                    migrateSavedAccountResult.DidMigrateSuccessfully = true;
                    migrateSavedAccountResult.ResultMessage = "Success";
                }

                migrateSavedAccountResultList.Add( migrateSavedAccountResult );
            }

            rockContext.SaveChanges();

            string resultSummary;
            if ( migrateSavedAccountResultList.Where( a => a.DidMigrateSuccessfully == false ).Any() )
            {
                resultSummary = string.Format( "Migrated {0} Saved Accounts with {1} accounts that did not migrate.", migrateSavedAccountResultList.Where( a => a.DidMigrateSuccessfully ).Count(), migrateSavedAccountResultList.Where( a => a.DidMigrateSuccessfully == false ).Count() );
            }
            else
            {
                resultSummary = string.Format( "Migrated {0} Saved Accounts", nmiPersonSavedAccountList.Count(), migrateSavedAccountResultList.Where( a => a.DidMigrateSuccessfully == false ) );
            }

            if ( !nmiPersonSavedAccountList.Any() )
            {
                nbMigrateSavedAccountsResults.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Warning;
                nbMigrateSavedAccountsResults.Text = "No NMI Saved Accounts Found";
            }
            else
            {
                nbMigrateSavedAccountsResults.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Info;
                nbMigrateSavedAccountsResults.Text = resultSummary;
            }

            var migrationDetails = migrateSavedAccountResultList.Select( a => a.GetSummaryDetails() ).ToList().AsDelimited( Environment.NewLine );

            pnlMigrateSavedAccountsResults.Visible = true;
            nbMigrateSavedAccountsResults.Details = string.Format( "<pre>{0}</pre>", migrationDetails );
            this.SetBlockUserPreference( UserPreferenceKey.MigrateSavedAccountsResultSummary, nbMigrateSavedAccountsResults.Text );
            this.SetBlockUserPreference( UserPreferenceKey.MigrateSavedAccountsResultDetails, migrationDetails );

            try
            {
                this.SetBlockUserPreference( UserPreferenceKey.MigrateSavedAccountsResultFileURL, logFileUrl );
                hfMigrateSavedAccountsResultFileURL.Value = logFileUrl;

                string logFile = this.Context.Server.MapPath( logFileUrl );
                File.WriteAllText( logFile, migrateSavedAccountResultList.ToJson( indentOutput: true ) );
            }
            catch
            {
                //
            }
        }

        #endregion Migrate Saved Accounts Related

        #region Migrate Scheduled Transactions

        /// <summary>
        /// 
        /// </summary>
        private class SubscriptionCustomerImportRecord
        {
            /// <summary>
            /// Gets or sets the NMI subscription identifier.
            /// </summary>
            /// <value>
            /// The NMI subscription identifier.
            /// </value>
            public string NMISubscriptionId { get; set; }

            /// <summary>
            /// Gets or sets the My Well customer identifier.
            /// </summary>
            /// <value>
            /// The My Well customer identifier.
            /// </value>
            public string MyWellCustomerId { get; set; }
        }

        /// <summary>
        /// Handles the FileUploaded event of the fuScheduleImportFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.FileUploaderEventArgs"/> instance containing the event data.</param>
        protected void fuScheduleImportFile_FileUploaded( object sender, Rock.Web.UI.Controls.FileUploaderEventArgs e )
        {
            ShowScheduledTransactionResults();
            btnMigrateScheduledTransactions.Enabled = true;
        }

        /// <summary>
        /// Handles the Click event of the btnMigrateScheduledTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMigrateScheduledTransactions_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var financialGatewayService = new FinancialGatewayService( rockContext );
            var nmiFinancialGatewayId = ddlNMIGateway.SelectedValue.AsInteger();
            var nmiFinancialGateway = financialGatewayService.Get( nmiFinancialGatewayId );
            var nmiGatewayComponent = nmiFinancialGateway.GetGatewayComponent();
            var myWellFinancialGatewayId = ddlMyWellGateway.SelectedValue.AsInteger();
            var myWellFinancialGateway = financialGatewayService.Get( myWellFinancialGatewayId );
            var myWellGatewayComponent = myWellFinancialGateway.GetGatewayComponent() as IHostedGatewayComponent;

            var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );

            // Get the ScheduledTransaction with NoTracking. If we need to update it, we'll track it with a different rockContext then save it.
            // Limit to active subscriptions that have a NextPaymentDate (onetime or canceled schedules might not have a NextPaymentDate)
            var scheduledTransactionsQry = financialScheduledTransactionService.Queryable().Where( a => a.FinancialGatewayId == nmiFinancialGatewayId & a.IsActive && a.NextPaymentDate.HasValue );

            List<int> personIdsToMigrate = gNMIPersonProfiles.SelectedKeys.Cast<int>().ToList();
            if ( personIdsToMigrate.Any() )
            {
                scheduledTransactionsQry = scheduledTransactionsQry.Where( a => personIdsToMigrate.Contains( a.AuthorizedPersonAlias.PersonId ) );
            }

            var scheduledTransactions = scheduledTransactionsQry.Include( a => a.ScheduledTransactionDetails ).ToList();

            var earliestMyWellStartDate = myWellGatewayComponent.GetEarliestScheduledStartDate( myWellFinancialGateway );
            var oneTimeFrequencyId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME.AsGuid() );

            string errorMessage;

            List<ScheduledTransactionMigrationResult> scheduledTransactionMigrationResults = new List<ScheduledTransactionMigrationResult>();

            var scheduledTransactionCount = scheduledTransactions.Count();
            var scheduledTransactionProgress = 0;
            string logFileUrl = string.Format( "~/App_Data/Logs/GatewayMigrationUtility_MigrateScheduledTransactions_{0}.json", RockDateTime.Now.ToString( "yyyyMMddTHHmmss" ) );
            hfMigrateScheduledTransactionsResultFileURL.Value = logFileUrl;
            var notificationboxJsHook = ".js-migrate-scheduled-notification";

            // Migrating Scheduled Transactions might take a while. Each migrated Scheduled Payment may take a half second or so to create on the MyWell Gateway.
            var importTask = new Task( () =>
            {
                // wait a little so the browser can render and start listening to events
                Task.Delay( 2000 ).Wait();

                _hubContext.Clients.All.setMigrateScheduledTransactionsButtonVisibility( this.SignalRNotificationKey, false );
                

                foreach ( var scheduledTransaction in scheduledTransactions )
                {
                    scheduledTransactionProgress++;

                    ScheduledTransactionMigrationResult scheduledTransactionMigrationResult = new ScheduledTransactionMigrationResult();
                    scheduledTransactionMigrationResult.MigrationDateTime = RockDateTime.Now;
                    scheduledTransactionMigrationResults.Add( scheduledTransactionMigrationResult );

                    scheduledTransactionMigrationResult.NMISubscriptionId = scheduledTransaction.GatewayScheduleId;
                    if ( scheduledTransactionMigrationResult.NMISubscriptionId.IsNotNullOrWhiteSpace() )
                    {
                        /* 2020-01-13 MDP
                        When MyWell transfers vault data from NMI to MyWell, they will also create CustomerVault records to associate with any scheduled transactions.
                        The reason is that Rock's implementation of NMI didn't associate VaultRecords and Subscriptions, and NMI also doesn't expose the link either.
                        Fortunately we able to link these back together again.  For mapping the new MyWell CustomerVaultId to scheduled transactions, MyWell uses the
                        NMI *SubscriptionId* as the MyWell *CustomerId* (not a typo!). So, since we now will now the CustomerVaultId for each subscription
                        (scheduled transaction), we can create new MyWell subscriptions based off of the data in FinancialScheduledTransaction plus keep an association
                        of VaultId to SubscriptionId both in FinancialScheduledTransaction.TransactionCode and also by using the MyWell API to get the customer_id for each subscription.
                         */

                        scheduledTransactionMigrationResult.MyWellCustomerId = scheduledTransactionMigrationResult.NMISubscriptionId;
                    }

                    scheduledTransactionMigrationResult.ScheduledTransactionId = scheduledTransaction.Id;
                    if ( scheduledTransaction.AuthorizedPersonAlias != null )
                    {
                        scheduledTransactionMigrationResult.PersonId = scheduledTransaction.AuthorizedPersonAlias.PersonId;
                        scheduledTransactionMigrationResult.PersonFullName = scheduledTransaction.AuthorizedPersonAlias.Person.FullName;
                    }
                    else
                    {
                        scheduledTransactionMigrationResult.PersonId = null;
                        scheduledTransactionMigrationResult.PersonFullName = "(No person record associated with saved account)";
                    }

                    if ( scheduledTransactionMigrationResult.MyWellCustomerId == null )
                    {
                        scheduledTransactionMigrationResult.ResultMessage = string.Format(
                            "WARNING: No My Well CustomerId found for Financial Scheduled Transaction with Id: {0} which is associated NMI SubscriptionId: '{1}'",
                            scheduledTransaction.Id,
                            scheduledTransactionMigrationResult.NMISubscriptionId
                        );

                        UpdateProgressMessage( string.Format( "Migrating Scheduled Transactions: {0} of {1}", scheduledTransactionProgress, scheduledTransactionCount ), scheduledTransactionMigrationResult.GetSummaryDetails(), notificationboxJsHook );

                        continue;
                    }

                    UpdateProgressMessage( string.Format( "Migrating Scheduled Transactions: {0} of {1}", scheduledTransactionProgress, scheduledTransactionCount ), scheduledTransactionMigrationResult.GetSummaryDetails(), notificationboxJsHook );

                    /* 2020-01-13 MDP
                       MyWell will return an error if trying to schedule the transaction sooner than UTC Tomorrow (see the details of what this means in the notes of MyWellGateway.GetEarliestScheduledStartDate)
                     */

                    // My Well requires that NextPaymentDate is in the Future (using UTC). That math is done in the gateway implementation...
                    // if the NextPayment null or earlier than whatever My Well considers the earliest start date, see if we can fix that up by calling GetStatus
                    if ( scheduledTransaction.NextPaymentDate == null || scheduledTransaction.NextPaymentDate < earliestMyWellStartDate )
                    {
                        // make sure we have the current data on what NMI thinks the next payment date is
                        financialScheduledTransactionService.GetStatus( scheduledTransaction, out errorMessage );
                        rockContext.SaveChanges();
                    }

                    if ( scheduledTransaction.NextPaymentDate == null )
                    {
                        // Shouldn't happen, but just in case
                        scheduledTransactionMigrationResult.ResultMessage = string.Format(
                            "WARNING: Unknown NextPaymentDate for FinancialScheduledTransaction.Id: {0} NMI SubscriptionId: '{1}'" + Environment.NewLine,
                            scheduledTransaction.Id,
                            scheduledTransactionMigrationResult.NMISubscriptionId
                            );

                        continue;
                    }

                    scheduledTransactionMigrationResult.OriginalNextPaymentDate = scheduledTransaction.NextPaymentDate;

                    if ( scheduledTransaction.NextPaymentDate < earliestMyWellStartDate )
                    {
                        if ( ( scheduledTransaction.NextPaymentDate > RockDateTime.Today ) && earliestMyWellStartDate.Subtract( scheduledTransaction.NextPaymentDate.Value ).TotalDays <= 2 )
                        {
                            /* 2020-01-13 MDP
                             If the NextPaymentDate is after Today but before the Earliest My Well Start Date, it'll be off by less than 24 hrs, so just reschedule it for the Earliest My Well Start Date.
                             For example, if the current datetime is 2020-01-13 5:01PM AZ and the NextPaymentDate is 2020-01-14, and the schedule is 'On the 14th of every month', we'll have to shift it
                             to 'on the 15th of every month' if UTC is a day ahead of the local time.
                            */
                            scheduledTransactionMigrationResult.IsNextPaymentDateAdjusted = true;
                            scheduledTransaction.NextPaymentDate = earliestMyWellStartDate;
                            scheduledTransactionMigrationResult.MyWellSubscriptionStartDate = scheduledTransaction.NextPaymentDate;
                        }
                        else
                        {
                            // if the NextPaymentDate is still too early AFTER getting the most recent status, then we can't safely figure it out, so report it
                            scheduledTransactionMigrationResult.ResultMessage = string.Format(
        "WARNING: NextPaymentDate of {0} for FinancialScheduledTransaction.Id: {1} and NMI SubscriptionId: '{2}' must have a NextPaymentDate of at least {3}." + Environment.NewLine,
        scheduledTransaction.NextPaymentDate,
        scheduledTransaction.Id,
        scheduledTransactionMigrationResult.NMISubscriptionId,
        earliestMyWellStartDate
        );
                            continue;
                        }
                    }

                    // create a subscription in the My Well System, then cancel the one on the NMI system
                    PaymentSchedule paymentSchedule = new PaymentSchedule
                    {
                        TransactionFrequencyValue = DefinedValueCache.Get( scheduledTransaction.TransactionFrequencyValueId ),
                        StartDate = scheduledTransaction.NextPaymentDate.Value,
                        PersonId = scheduledTransaction.AuthorizedPersonAlias.PersonId
                    };

                    scheduledTransactionMigrationResult.MyWellSubscriptionStartDate = paymentSchedule.StartDate;

                    ReferencePaymentInfo referencePaymentInfo = new ReferencePaymentInfo
                    {
                        GatewayPersonIdentifier = scheduledTransactionMigrationResult.MyWellCustomerId,
                        Description = string.Format( "Migrated from NMI SubscriptionID:{0}", scheduledTransactionMigrationResult.NMISubscriptionId ),
                        Amount = scheduledTransaction.TotalAmount
                    };

                    var myWellGateway = ( myWellGatewayComponent as MyWellGateway );
                    string alreadyMigratedMyWellSubscriptionId = null;

                    if ( myWellGateway != null )
                    {
                        var customerMyWellSubscriptions = myWellGateway.SearchCustomerSubscriptions( myWellFinancialGateway, scheduledTransactionMigrationResult.MyWellCustomerId );
                        if ( customerMyWellSubscriptions.TotalCount > 0 && customerMyWellSubscriptions.Data != null )
                        {
                            alreadyMigratedMyWellSubscriptionId = customerMyWellSubscriptions.Data.Where( a => a.Description.Contains( referencePaymentInfo.Description ) ).Select( a => a.Customer.Id ).FirstOrDefault();
                        }
                    }

                    if ( string.IsNullOrEmpty( alreadyMigratedMyWellSubscriptionId ) )
                    {
                        // hasn't already been migrated, so go ahead and migrate it
                        var tempFinancialScheduledTransaction = myWellGatewayComponent.AddScheduledPayment( myWellFinancialGateway, paymentSchedule, referencePaymentInfo, out errorMessage );
                        if ( tempFinancialScheduledTransaction != null )
                        {
                            //////////// This cannot be undone!! 
                            nmiGatewayComponent.CancelScheduledPayment( scheduledTransaction, out errorMessage );

                            scheduledTransaction.TransactionCode = tempFinancialScheduledTransaction.TransactionCode;
                            scheduledTransaction.GatewayScheduleId = tempFinancialScheduledTransaction.GatewayScheduleId;
                            scheduledTransaction.FinancialGatewayId = tempFinancialScheduledTransaction.FinancialGatewayId;
                            scheduledTransaction.IsActive = tempFinancialScheduledTransaction.IsActive;
                            rockContext.SaveChanges();

                            scheduledTransactionMigrationResult.DidMigrateSuccessfully = true;

                            scheduledTransactionMigrationResult.ResultMessage = string.Format(
                                "SUCCESS: Scheduled Transaction migration succeeded. (FinancialScheduledTransaction.Id: {0}, NMI SubscriptionId: '{1}', My Well CustomerId: {2}, My Well SubscriptionId: {3})" + Environment.NewLine,
                                scheduledTransaction.Id,
                                scheduledTransactionMigrationResult.NMISubscriptionId,
                                scheduledTransactionMigrationResult.MyWellCustomerId,
                                scheduledTransaction.GatewayScheduleId
                                );
                        }
                        else
                        {
                            scheduledTransactionMigrationResult.ResultMessage = string.Format(
                                "ERROR: Scheduled Transaction migration failed. ErrorMessage: {0}, FinancialScheduledTransaction.Id: {1}, NMI SubscriptionId: '{2}', My Well CustomerId: {3}" + Environment.NewLine,
                                errorMessage,
                                scheduledTransaction.Id,
                                scheduledTransactionMigrationResult.NMISubscriptionId,
                                scheduledTransactionMigrationResult.MyWellCustomerId
                                );
                        }
                    }
                    else
                    {
                        scheduledTransactionMigrationResult.ResultMessage = string.Format(
                            "INFO: Scheduled Transaction already migrated to My Well. FinancialScheduledTransaction.Id: {0}, NMI SubscriptionId: '{1}', My Well SubscriptionId: '{2}', My Well CustomerId: {3}" + Environment.NewLine,
                            scheduledTransaction.Id,
                            scheduledTransactionMigrationResult.NMISubscriptionId,
                            alreadyMigratedMyWellSubscriptionId,
                            scheduledTransactionMigrationResult.MyWellCustomerId
                            );
                    }
                }
            } );

            string importResult = string.Empty;

            importTask.ContinueWith( ( c ) =>
             {
                 _hubContext.Clients.All.setMigrateScheduledTransactionsButtonVisibility( this.SignalRNotificationKey, true );
                 var migrationDetails = scheduledTransactionMigrationResults.Select( a => a.GetSummaryDetails() ).ToList().AsDelimited( Environment.NewLine );

                 if ( c.Exception != null )
                 {
                     ExceptionLogService.LogException( c.Exception );
                     migrationDetails += string.Format( "EXCEPTION: {0}", c.Exception.Flatten().Message );
                     importResult = "EXCEPTION";
                     UpdateProgressMessage( importResult, migrationDetails, notificationboxJsHook );
                 }
                 else
                 {
                     importResult = "Migrate Scheduled Transactions Completed Successfully";
                     UpdateProgressMessage( importResult, migrationDetails, notificationboxJsHook );
                 }

                 this.SetBlockUserPreference( UserPreferenceKey.MigrateScheduledTransactionsResultSummary, importResult );
                 this.SetBlockUserPreference( UserPreferenceKey.MigrateScheduledTransactionsResultDetails, migrationDetails );

                 try
                 {

                     string logFile = this.Context.Server.MapPath( logFileUrl );
                     this.SetBlockUserPreference( UserPreferenceKey.MigrateScheduledTransactionsResultFileURL, logFileUrl );
                     File.WriteAllText( logFile, scheduledTransactionMigrationResults.ToJson( indentOutput: true ) );
                 }
                 catch
                 {
                     //
                 }
             } );

            importTask.Start();

            btnMigrateScheduledTransactions.Style[HtmlTextWriterStyle.Display] = "none";
            btnDownloadScheduledTransactionsResultsJSON.Style[HtmlTextWriterStyle.Display] = "none";
            nbMigrateScheduledTransactionsResult.Text = "Migrating Scheduled Transactions...";
            nbMigrateScheduledTransactionsResult.Details = string.Empty;
            pnlMigrateScheduledTransactionsResults.Visible = true;
        }

        /// <summary>
        /// Updates the progress message.
        /// </summary>
        /// <param name="progressMessage">The progress message.</param>
        /// <param name="results">The results.</param>
        /// <param name="notificationboxJsHook">The js target.</param>
        public void UpdateProgressMessage( string progressMessage, string results, string notificationboxJsHook )
        {
            _hubContext.Clients.All.showProgress( this.SignalRNotificationKey, progressMessage, results, notificationboxJsHook );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlNMIGateway control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlNMIGateway_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnDownloadSavedAccountsResultsJSON control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected void btnDownloadSavedAccountsResultsJSON_Click( object sender, EventArgs e )
        {
            string logFile = this.Context.Server.MapPath( hfMigrateSavedAccountsResultFileURL.Value );
            Response.Clear();
            Response.ContentType = "text/json";
            Response.AppendHeader( "Content-Disposition", "attachment; filename=" + Path.GetFileName( logFile ) );

            Response.Charset = string.Empty;
            var jsonData = File.ReadAllText( logFile );
            Response.BinaryWrite( jsonData.ToMemoryStream().ToArray() );
            Response.Flush();
            Response.End();
        }

        /// <summary>
        /// Handles the Click event of the btnDownloadScheduledTransactionResultsJSON control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected void btnDownloadScheduledTransactionsResultsJSON_Click( object sender, EventArgs e )
        {
            string logFile = this.Context.Server.MapPath( hfMigrateScheduledTransactionsResultFileURL.Value );
            Response.Clear();
            Response.ContentType = "text/json";
            Response.AppendHeader( "Content-Disposition", "attachment; filename=" + Path.GetFileName( logFile ) );

            Response.Charset = string.Empty;
            var jsonData = File.ReadAllText( logFile );
            Response.BinaryWrite( jsonData.ToMemoryStream().ToArray() );
            Response.Flush();
            Response.End();
        }

        /// <summary>
        /// Handles the Click event of the btnRemoveEmailAddresses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRemoveEmailAddresses_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            var financialGatewayService = new FinancialGatewayService( rockContext );
            var myWellFinancialGatewayId = ddlMyWellGateway.SelectedValue.AsInteger();
            var myWellFinancialGateway = financialGatewayService.Get( myWellFinancialGatewayId );
            var myWellGatewayComponent = myWellFinancialGateway.GetGatewayComponent() as MyWellGateway;

            var notificationboxJsHook = ".js-remove-emails-notification";

            // Migrating Scheduled Transactions might take a while. Each migrated Scheduled Payment may take a half second or so to create on the MyWell Gateway.
            var updateEmailTask = new Task( () =>
            {
                // wait a little so the browser can render and start listening to events
                Task.Delay( 2000 ).Wait();

                UpdateProgressMessage( "Removing Emails...", "", notificationboxJsHook );

                var emailsRemoveCount = myWellGatewayComponent.RemoveEmails( myWellFinancialGateway, updateEmailOnProgress );
                if ( emailsRemoveCount == 0 )
                {
                    UpdateProgressMessage( string.Format( "Success: All emails were already removed.", emailsRemoveCount ), "", notificationboxJsHook );
                }
                else
                {
                    UpdateProgressMessage( string.Format( "Success: {0} emails were removed", emailsRemoveCount ), "", notificationboxJsHook );
                }
            } );

            nbRemoveEmailAddressesResult.Text = "Removing Emails...";
            nbRemoveEmailAddressesResult.Details = "";

            updateEmailTask.Start();
        }

        /// <summary>
        /// Updates the email on progress.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void updateEmailOnProgress( object sender, string e )
        {
            var notificationboxJsHook = ".js-remove-emails-notification";
            UpdateProgressMessage( e, string.Empty, notificationboxJsHook );
        }

        /// <summary>
        /// Handles the Click event of the btnExportVaultTransferRequestFiles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnExportVaultTransferRequestFiles_Click( object sender, EventArgs e )
        {
            var nmiFinancialGatewayID = ddlNMIGateway.SelectedValue.AsInteger();

            var rockContext = new RockContext();
            var financialPersonSavedAccountService = new FinancialPersonSavedAccountService( rockContext );
            var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
            var financialTransactionService = new FinancialTransactionService( rockContext );
            var financialPersonSavedAccountQry = financialPersonSavedAccountService.Queryable();

            var dateRange = sdrDateRange.SelectedDateRange;

            var financialTransactionsQuery = financialTransactionService.Queryable().Where( a => a.FinancialGatewayId == nmiFinancialGatewayID && a.TransactionDateTime >= dateRange.Start && a.TransactionDateTime < dateRange.End );

            var transactionFrequencyValueIdOneTime = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME.AsGuid() ).Value;

            // get any active scheduled transactions, except for OneTime transactions since those might have already been processed (we'll get ones that haven't been processed in the subscription_id export)
            var savedAccountsFinancialScheduledTransactionQuery = financialScheduledTransactionService.Queryable().Where( a => a.FinancialGatewayId == nmiFinancialGatewayID && a.IsActive && a.TransactionFrequencyValueId != transactionFrequencyValueIdOneTime );

            // Get a list NMI CustomerIds (Saved Accounts) that have
            // - Transactions that have occurred in date range
            // - or have an active ScheduledTransaction
            // - or were created in the date range
            List<string> nmiCustomerIds = financialPersonSavedAccountQry.Where( a => a.FinancialGatewayId == nmiFinancialGatewayID && !string.IsNullOrEmpty( a.ReferenceNumber ) ).Where( a =>
                    financialTransactionsQuery.Any( ft => ft.AuthorizedPersonAliasId == a.PersonAliasId )
                    || savedAccountsFinancialScheduledTransactionQuery.Any( fst => fst.AuthorizedPersonAliasId == a.PersonAliasId )
                    || a.CreatedDateTime > dateRange.Start )
                .Select( a => a.ReferenceNumber ).OrderBy( a => a ).ToList();

            /*SELECT fst.GatewayScheduleId [NMI SubscriptionId] from FinancialScheduledTransaction fst
where fst.IsActive = 1
and fst.NextPaymentDate >= GetDate()*/

            var currentDate = RockDateTime.Today;
            var scheduledTransactionList = financialScheduledTransactionService.Queryable().Where( a =>
                a.FinancialGatewayId == nmiFinancialGatewayID && a.IsActive
                && !string.IsNullOrEmpty( a.GatewayScheduleId ) ).ToList();

            // loop thru scheduledTransactions that might have a stale NextPaymentDate and make sure that they are updated 
            foreach ( var scheduledTransaction in scheduledTransactionList )
            {
                if ( scheduledTransaction.TransactionFrequencyValueId != transactionFrequencyValueIdOneTime )
                {
                    if ( scheduledTransaction.NextPaymentDate == null || scheduledTransaction.NextPaymentDate < currentDate )
                    {
                        string errorMessages;
                        financialScheduledTransactionService.GetStatus( scheduledTransaction, out errorMessages );
                    }
                }
            }

            var nmiSubscriptionIds = scheduledTransactionList.Where( a => a.NextPaymentDate.HasValue && a.NextPaymentDate >= currentDate ).Select( a => a.GatewayScheduleId ).OrderBy( a => a ).ToList();

            using ( var outputZip = new MemoryStream() )
            {
                using ( var archive = new ZipArchive( outputZip, ZipArchiveMode.Create, true ) )
                {
                    var nmiCustomerIdFile = archive.CreateEntry( "NMI_CustomerIds.csv" );

                    using ( var NMICustomerIdsEntryStream = nmiCustomerIdFile.Open() )
                    using ( var NMICustomerIdsStreamWriter = new StreamWriter( NMICustomerIdsEntryStream ) )
                    {
                        NMICustomerIdsStreamWriter.Write( nmiCustomerIds.AsDelimited( Environment.NewLine ) );
                    }

                    var nmiSubscriptionIdFile = archive.CreateEntry( "NMI_SubscriptionIds.csv" );
                    using ( var NMISubscriptionIdsEntryStream = nmiSubscriptionIdFile.Open() )
                    using ( var NMISubscriptionIdsStreamWriter = new StreamWriter( NMISubscriptionIdsEntryStream ) )
                    {
                        NMISubscriptionIdsStreamWriter.Write( nmiSubscriptionIds.AsDelimited( Environment.NewLine ) );
                    }
                }

                Response.Clear();
                Response.ContentType = "application/zip";
                Response.AppendHeader( "Content-Disposition", "attachment; filename=VaultRequestFiles.zip" );

                Response.Charset = string.Empty;
                outputZip.Seek( 0, SeekOrigin.Begin );
                Response.BinaryWrite( outputZip.ToArray() );
                Response.Flush();
                Response.End();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnUpdateOneTimeScheduleStatus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUpdateOneTimeScheduleStatus_Click( object sender, EventArgs e )
        {
            var oneTimeFrequencyId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME.AsGuid() );
            var scheduledTransactionQry = new FinancialScheduledTransactionService( new RockContext() ).Queryable()
                .Where( a => a.IsActive && a.TransactionFrequencyValueId == oneTimeFrequencyId && !string.IsNullOrEmpty( a.GatewayScheduleId ) )
                .OrderBy( a => a.Id );
            var scheduledTransactionIdList = scheduledTransactionQry.Select( a => a.Id ).ToList();

            var updateStatusTask = new Task( () =>
            {
                foreach ( var scheduledTransactionId in scheduledTransactionIdList )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
                        var scheduledTransaction = financialScheduledTransactionService.Get( scheduledTransactionId );
                        string errorMessage;
                        financialScheduledTransactionService.GetStatus( scheduledTransaction, out errorMessage );
                        rockContext.SaveChanges();
                    }
                }
            } );

            updateStatusTask.Start();
            nbUpdateOneTimeScheduleStatus.Text = "Started updating status of one-time scheduled transactions. Refresh this page to see the progress of how many are left.";
        }
    }

    #endregion Migrate Scheduled Transactions
}
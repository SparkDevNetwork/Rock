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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using CsvHelper;
using Microsoft.AspNet.SignalR;
using Rock;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.TransNational.Pi;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Financial Gateway Migration Utility
    /// </summary>
    [DisplayName( "Financial Gateway Migration Utility" )]
    [Category( "Finance" )]
    [Description( "Tool to assist in migrating records from NMI a Pi." )]

    #region Block Attributes
    #endregion Block Attributes
    public partial class GatewayMigrationUtility : RockBlock
    {

        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        protected static class AttributeKey
        {
        }

        #endregion Attribute Keys

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
            var migrateSavedAccountsResultSummary = this.GetBlockUserPreference( "MigrateSavedAccountsResultSummary" );
            var migrateSavedAccountsResultDetails = this.GetBlockUserPreference( "MigrateSavedAccountsResultDetails" );

            if ( migrateSavedAccountsResultSummary.IsNotNullOrWhiteSpace() )
            {
                nbMigrateSavedAccounts.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Info;
                nbMigrateSavedAccounts.Text = "Migrate Saved Accounts has already been run.";
                nbMigrateSavedAccounts.Details = migrateSavedAccountsResultDetails.ToString().ConvertCrLfToHtmlBr();
            }

            var migrateScheduledTransactionsResultSummary = this.GetBlockUserPreference( "MigrateScheduledTransactionsResultSummary" );
            var migrateScheduledTransactionsResultDetails = this.GetBlockUserPreference( "MigrateScheduledTransactionsResultDetails" );

            if ( migrateScheduledTransactionsResultSummary.IsNotNullOrWhiteSpace() )
            {
                nbMigrateScheduledTransactions.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Info;
                nbMigrateScheduledTransactions.Text = "Migrate Scheduled Transactions has already been run.";
                nbMigrateScheduledTransactions.Details = migrateScheduledTransactionsResultDetails.ToString().ConvertCrLfToHtmlBr();
            }

            var rockContext = new RockContext();
            var financialGatewayService = new FinancialGatewayService( rockContext );
            var activeGatewayList = financialGatewayService.Queryable().Where( a => a.IsActive == true ).AsNoTracking().ToList();
            var piGateways = activeGatewayList.Where( a => a.GetGatewayComponent() is Rock.TransNational.Pi.PiGateway ).ToList();
            ddlPiGateway.Items.Clear();
            foreach ( var piGateway in piGateways )
            {
                ddlPiGateway.Items.Add( new ListItem( piGateway.Name, piGateway.Id.ToString() ) );
            }

            var nmiGateways = activeGatewayList.Where( a => a.GetGatewayComponent() is Rock.NMI.Gateway ).ToList();
            ddlNMIGateway.Items.Clear();
            foreach ( var nmiGateway in nmiGateways )
            {
                ddlNMIGateway.Items.Add( new ListItem( nmiGateway.Name, nmiGateway.Id.ToString() ) );
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
            /// Gets or sets the Pi customer identifier.
            /// </summary>
            /// <value>
            /// The pi customer identifier.
            /// </value>
            public string PiCustomerId { get; set; }
        }

        /// <summary>
        /// Handles the Click event of the btnMigrateSavedAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMigrateSavedAccounts_Click( object sender, EventArgs e )
        {
            BinaryFile binaryFile = null;
            var rockContext = new RockContext();
            var binaryFileService = new BinaryFileService( rockContext );
            var binaryFileId = fuCustomerVaultImportFile.BinaryFileId;
            if ( binaryFileId.HasValue )
            {
                binaryFile = binaryFileService.Get( binaryFileId.Value );
            }

            Dictionary<string, string> nmiToPiCustomerIdLookup = null;

            var importData = binaryFile.ContentsToString();

            StringReader stringReader = new StringReader( importData );
            CsvReader csvReader = new CsvReader( stringReader );
            csvReader.Configuration.HasHeaderRecord = false;

            nmiToPiCustomerIdLookup = csvReader.GetRecords<CustomerVaultImportRecord>().ToDictionary( k => k.NMICustomerId, v => v.PiCustomerId );

            var financialGatewayService = new FinancialGatewayService( rockContext );
            var nmiFinancialGatewayID = ddlNMIGateway.SelectedValue.AsInteger();
            var nmiFinancialGateway = financialGatewayService.Get( nmiFinancialGatewayID );
            var nmiGatewayComponent = nmiFinancialGateway.GetGatewayComponent();
            var piFinancialGatewayId = ddlPiGateway.SelectedValue.AsInteger();
            var piFinancialGateway = financialGatewayService.Get( piFinancialGatewayId );
            var piGatewayComponent = piFinancialGateway.GetGatewayComponent() as IHostedGatewayComponent;

            var financialPersonSavedAccountService = new FinancialPersonSavedAccountService( rockContext );
            var nmiPersonSavedAccountList = financialPersonSavedAccountService.Queryable().Where( a => a.FinancialGatewayId == nmiFinancialGatewayID ).ToList();

            var personSavedAccountResultsBuilder = new StringBuilder();

            var nmiPersonSavedAccountListCount = nmiPersonSavedAccountList.Count();

            foreach ( var nmiPersonSavedAccount in nmiPersonSavedAccountList )
            {
                var nmiCustomerId = nmiPersonSavedAccount.GatewayPersonIdentifier ?? nmiPersonSavedAccount.ReferenceNumber;
                var piCustomerId = nmiToPiCustomerIdLookup.GetValueOrNull( nmiCustomerId );

                nmiPersonSavedAccount.GatewayPersonIdentifier = piCustomerId;
                nmiPersonSavedAccount.FinancialGatewayId = piFinancialGatewayId;

                // NOTE: NMI Customer IDs created after the Vault import file was created won't have a piCustomerId

                personSavedAccountResultsBuilder.AppendFormat(
                    "FinancialPersonSavedAccount.Id: {0} NMI CustomerId: '{1}', NMI GatewayPersonIdentifier: '{2}', NMI ReferenceNumber: '{3}', Pi CustomerId: '{4}'" + Environment.NewLine,
                    nmiPersonSavedAccount.Id,
                    nmiCustomerId,
                    nmiPersonSavedAccount.GatewayPersonIdentifier,
                    nmiPersonSavedAccount.ReferenceNumber,
                    piCustomerId
                    );
            }

            rockContext.SaveChanges();

            string resultSummary = string.Format( "Migrated {0} Saved Accounts", nmiPersonSavedAccountList.Count() );

            personSavedAccountResultsBuilder.AppendLine( resultSummary );

            if ( !nmiPersonSavedAccountList.Any() )
            {
                nbMigrateSavedAccounts.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Warning;
                nbMigrateSavedAccounts.Text = "No NMI Saved Accounts Found";
            }
            else
            {
                nbMigrateSavedAccounts.Title = "Success";
                nbMigrateSavedAccounts.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Success;
                nbMigrateSavedAccounts.Text = resultSummary;
            }

            nbMigrateSavedAccounts.Visible = true;
            nbMigrateSavedAccounts.Details = personSavedAccountResultsBuilder.ToString();
            this.SetBlockUserPreference( "MigrateSavedAccountsResultSummary", nbMigrateSavedAccounts.Text );
            this.SetBlockUserPreference( "MigrateSavedAccountsResultDetails", personSavedAccountResultsBuilder.ToString() );
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
            /// Gets or sets the Pi customer identifier.
            /// </summary>
            /// <value>
            /// The pi customer identifier.
            /// </value>
            public string PiCustomerId { get; set; }
        }

        /// <summary>
        /// Handles the FileUploaded event of the fuScheduleImportFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.FileUploaderEventArgs"/> instance containing the event data.</param>
        protected void fuScheduleImportFile_FileUploaded( object sender, Rock.Web.UI.Controls.FileUploaderEventArgs e )
        {
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

            var binaryFileService = new BinaryFileService( rockContext );
            var binaryFileId = fuScheduleImportFile.BinaryFileId;

            BinaryFile binaryFile = null;
            if ( binaryFileId.HasValue )
            {
                binaryFile = binaryFileService.Get( binaryFileId.Value );
            }

            Dictionary<string, string> subscriptionImportRecordLookup = null;

            var importData = binaryFile.ContentsToString();

            StringReader stringReader = new StringReader( importData );
            CsvReader csvReader = new CsvReader( stringReader );
            csvReader.Configuration.HasHeaderRecord = false;

            subscriptionImportRecordLookup = csvReader.GetRecords<SubscriptionCustomerImportRecord>().ToDictionary( k => k.NMISubscriptionId, v => v.PiCustomerId );

            var financialGatewayService = new FinancialGatewayService( rockContext );
            var nmiFinancialGatewayId = ddlNMIGateway.SelectedValue.AsInteger();
            var nmiFinancialGateway = financialGatewayService.Get( nmiFinancialGatewayId );
            var nmiGatewayComponent = nmiFinancialGateway.GetGatewayComponent();
            var piFinancialGatewayId = ddlPiGateway.SelectedValue.AsInteger();
            var piFinancialGateway = financialGatewayService.Get( piFinancialGatewayId );
            var piGatewayComponent = piFinancialGateway.GetGatewayComponent() as IHostedGatewayComponent;

            var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );

            // Get the ScheduledTransaction with NoTracking. If we need to update it, we'll track it with a different rockContext then save it.
            // Limit to active subscriptions that have a NextPaymentDate (onetime or canceled schedules might not have a NextPaymentDate)
            var scheduledTransactions = financialScheduledTransactionService.Queryable().Where( a => a.FinancialGatewayId == nmiFinancialGatewayId & a.IsActive && a.NextPaymentDate.HasValue ).AsNoTracking().ToList();

            var earliestPiStartDate = piGatewayComponent.GetEarliestScheduledStartDate( piFinancialGateway );
            var oneTimeFrequencyId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME.AsGuid() );

            string errorMessage;

            var scheduledTransactionResultsBuilder = new StringBuilder();

            var scheduledTransactionCount = scheduledTransactions.Count();
            var scheduledTransactionProgress = 0;

            // Migrating Scheduled Transactions might take a while. Each migrated Scheduled Payment may take a half second or so to create on the Pi Gateway.
            var importTask = new Task( () =>
            {
                // wait a little so the browser can render and start listening to events
                Task.Delay( 1000 ).Wait();
                _hubContext.Clients.All.setButtonVisibilty( this.SignalRNotificationKey, false );

                foreach ( var scheduledTransaction in scheduledTransactions )
                {
                    System.Threading.Thread.Sleep( 1000 );

                    UpdateProgressMessage( string.Format( "Migrating Scheduled Transactions: {0} of {1}", scheduledTransactionProgress, scheduledTransactionCount ), " " );

                    scheduledTransactionProgress++;
                    var nmiSubscriptionId = scheduledTransaction.GatewayScheduleId;
                    var nmiCustomerId = scheduledTransaction.ForeignKey;
                    var piCustomerId = subscriptionImportRecordLookup.GetValueOrNull( nmiSubscriptionId );
                    if ( piCustomerId == null )
                    {
                        scheduledTransactionResultsBuilder.AppendFormat(
        "WARNING: No Pi CustomerId found for Financial Scheduled Transaction with Id: {0} which is associated NMI SubscriptionId: '{1}'" + Environment.NewLine,
        scheduledTransaction.Id,
        nmiSubscriptionId
        );
                        continue;
                    }

                    // Pi requires that NextPaymentDate is in the Future (using UTC). That math is done in the gateway implementation...
                    // if the NextPayment null or earlier than whatever Pi considers the earliest start date, see if we can fix that up by calling GetStatus
                    if ( scheduledTransaction.NextPaymentDate == null || scheduledTransaction.NextPaymentDate < earliestPiStartDate )
                    {
                        financialScheduledTransactionService.GetStatus( scheduledTransaction, out errorMessage );
                    }

                    if ( scheduledTransaction.NextPaymentDate == null )
                    {
                        // Shouldn't happen, but just in case
                        scheduledTransactionResultsBuilder.AppendFormat(
        "WARNING: Unknown NextPaymentDate for FinancialScheduledTransaction.Id: {0} NMI SubscriptionId: '{1}'" + Environment.NewLine,
        scheduledTransaction.Id,
        nmiSubscriptionId
        );
                        continue;
                    }


                    if ( scheduledTransaction.NextPaymentDate < earliestPiStartDate )
                    {
                        if ( ( scheduledTransaction.NextPaymentDate > RockDateTime.Today ) && earliestPiStartDate.Subtract( scheduledTransaction.NextPaymentDate.Value ).TotalDays <= 2 )
                        {
                            // if the NextPaymentDate is after Today but before the Earliest Pi Start Date, it'll be off by less than 24 hrs, so just reschedule it for the Earliest Pi Start Date
                            scheduledTransaction.NextPaymentDate = earliestPiStartDate;
                        }
                        else
                        {
                            // if the NextPaymentDate is still too early AFTER getting the most recent status, then we can't safely figure it out, so report it
                            scheduledTransactionResultsBuilder.AppendFormat(
            "WARNING: NextPaymentDate of {0} for FinancialScheduledTransaction.Id: {1} and NMI SubscriptionId: '{2}' must have a NextPaymentDate of at least {3}." + Environment.NewLine,
            scheduledTransaction.NextPaymentDate,
            scheduledTransaction.Id,
            nmiSubscriptionId,
            earliestPiStartDate
            );
                        }
                    }

                    // create a subscription in the Pi System, then cancel the one on the NMI system
                    PaymentSchedule paymentSchedule = new PaymentSchedule
                    {
                        TransactionFrequencyValue = DefinedValueCache.Get( scheduledTransaction.TransactionFrequencyValueId ),
                        StartDate = scheduledTransaction.NextPaymentDate.Value,
                        PersonId = scheduledTransaction.AuthorizedPersonAlias.PersonId
                    };

                    ReferencePaymentInfo referencePaymentInfo = new ReferencePaymentInfo
                    {
                        GatewayPersonIdentifier = piCustomerId,
                        Description = string.Format( "Migrated from NMI SubscriptionID:{0}", nmiSubscriptionId )
                    };

                    var piGateway = ( piGatewayComponent as PiGateway );
                    string alreadyMigratedPiSubscriptionId = null;

                    if ( piGateway != null )
                    {
                        var customerPiSubscriptions = piGateway.SearchCustomerSubscriptions( piFinancialGateway, piCustomerId );
                        alreadyMigratedPiSubscriptionId = customerPiSubscriptions.Data.Where( a => a.Description.Contains( referencePaymentInfo.Description ) ).Select( a => a.Customer.Id ).FirstOrDefault();
                    }

                    if ( string.IsNullOrEmpty( alreadyMigratedPiSubscriptionId ) )
                    {
                        // hasn't already been migrated, so go ahead and migrate it
                        var tempFinancialScheduledTransaction = piGatewayComponent.AddScheduledPayment( piFinancialGateway, paymentSchedule, referencePaymentInfo, out errorMessage );
                        if ( tempFinancialScheduledTransaction != null )
                        {
                            ////////////#### DISABLE this when debugger #####
                            nmiGatewayComponent.CancelScheduledPayment( scheduledTransaction, out errorMessage );

                            // update the scheduled transaction to point to the Pi scheduled transaction
                            using ( var updateRockContext = new RockContext() )
                            {
                                // Attach the person to the updateRockContext so that it'll be tracked/saved using updateRockContext 
                                updateRockContext.FinancialScheduledTransactions.Attach( scheduledTransaction );
                                scheduledTransaction.TransactionCode = tempFinancialScheduledTransaction.TransactionCode;
                                scheduledTransaction.GatewayScheduleId = tempFinancialScheduledTransaction.GatewayScheduleId;
                                scheduledTransaction.FinancialGatewayId = tempFinancialScheduledTransaction.FinancialGatewayId;
                                updateRockContext.SaveChanges();
                            }

                            scheduledTransactionResultsBuilder.AppendFormat(
                                "SUCCESS: Scheduled Transaction migration succeeded. (FinancialScheduledTransaction.Id: {0}, NMI SubscriptionId: '{1}', Pi CustomerId: {2}, Pi SubscriptionId: {3})" + Environment.NewLine,
                                scheduledTransaction.Id,
                                nmiSubscriptionId,
                                piCustomerId,
                                scheduledTransaction.GatewayScheduleId
                                );
                        }
                        else
                        {
                            scheduledTransactionResultsBuilder.AppendFormat(
                                "ERROR: Scheduled Transaction migration failed. ErrorMessage: {0}, FinancialScheduledTransaction.Id: {1}, NMI SubscriptionId: '{2}', Pi CustomerId: {3}" + Environment.NewLine,
                                errorMessage,
                                scheduledTransaction.Id,
                                nmiSubscriptionId,
                                piCustomerId
                                );
                        }
                    }
                    else
                    {
                        scheduledTransactionResultsBuilder.AppendFormat(
                            "INFO: Scheduled Transaction already migrated to PI. FinancialScheduledTransaction.Id: {0}, NMI SubscriptionId: '{1}', Pi SubscriptionId: '{2}', Pi CustomerId: {3}" + Environment.NewLine,
                            scheduledTransaction.Id,
                            nmiSubscriptionId,
                            alreadyMigratedPiSubscriptionId,
                            piCustomerId
                            );
                    }
                }
            } );

            string importResult = string.Empty;

            importTask.ContinueWith( ( c ) =>
             {
                 if ( c.Exception != null )
                 {
                     ExceptionLogService.LogException( c.Exception );
                     scheduledTransactionResultsBuilder.AppendLine( string.Format( "EXCEPTION: {0}", c.Exception.Flatten().Message ) );
                     importResult = "EXCEPTION";
                     UpdateProgressMessage( importResult, scheduledTransactionResultsBuilder.ToString() );
                 }
                 else
                 {
                     importResult = "Migrate Scheduled Transactions Completed Successfully";
                     UpdateProgressMessage( importResult, scheduledTransactionResultsBuilder.ToString() );
                 }

                 this.SetBlockUserPreference( "MigrateScheduledTransactionsResultSummary", importResult );
                 this.SetBlockUserPreference( "MigrateScheduledTransactionsResultDetails", scheduledTransactionResultsBuilder.ToString() );
             } );

            importTask.Start();

            nbMigrateScheduledTransactions.Visible = false;

            // wait for 5 seconds to see if this happens fast enough to do without Signal R. Otherwise, let the importTask continue and send progress to Signal R. 
            var waitResult = importTask.Wait( 5000 );
            if ( waitResult )
            {
                // wait just a little bit to make sure the importResult gets set
                System.Threading.Thread.Sleep( 1000 );

                nbMigrateScheduledTransactions.Visible = true;
                nbMigrateScheduledTransactions.Title = "Success";
                nbMigrateScheduledTransactions.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Success;

                var resultDetails = scheduledTransactionResultsBuilder.ToString();
                if ( resultDetails.Contains( "ERROR" )  || resultDetails.Contains( "WARNING" ) )
                {
                    nbMigrateScheduledTransactions.Title = "Completed with Warnings";
                    nbMigrateScheduledTransactions.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Info;
                }
                
                nbMigrateScheduledTransactions.Text = importResult;
                nbMigrateScheduledTransactions.Details = resultDetails.ConvertCrLfToHtmlBr();
            }
        }

        /// <summary>
        /// Updates the progress message.
        /// </summary>
        /// <param name="progressMessage">The progress message.</param>
        public void UpdateProgressMessage( string progressMessage, string results )
        {
            _hubContext.Clients.All.showProgress( this.SignalRNotificationKey, progressMessage, results );
        }
    }

    #endregion Migrate Scheduled Transactions
}
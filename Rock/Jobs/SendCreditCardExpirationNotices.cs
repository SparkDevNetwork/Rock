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
using System.Linq;
using System.Text;
using System.Web;

using Quartz;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// </summary>
    [Description( "Sends an email and optional workflow if a credit card that is used for a scheduled transaction is going to expire next month. There is also an option to delete expired saved accounts." )]

    [SystemCommunicationField(
        "Expiring Credit Card Email",
        Key = AttributeKey.ExpiringCreditCardEmail,
        Description = "The system communication template to use for the credit card expiration notice. The merge fields 'Person', 'Card' (the last four digits of the credit card), and 'Expiring' (the MM/YYYY of expiration) will be available to the email template.",
        IsRequired = true,
        Order = 0 )]

    [WorkflowTypeField(
        "Workflow",
        Key = AttributeKey.Workflow,
        Description = "The Workflow to launch for person whose credit card is expiring. The attributes 'Person', 'Card' (the last four digits of the credit card), and 'Expiring' (the MM/YYYY of expiration) will be passed to the workflow as attributes.",
        AllowMultiple = false,
        IsRequired = false,
        Order = 1 )]

    [IntegerField(
        "Remove Expired Saved Accounts after days",
        Key = AttributeKey.RemovedExpiredSavedAccountDays,
        Description = "The number of days after a saved account expires to delete the saved account. For example, if a credit card expiration is January 2023, it'll expire on Feb 1st, 2023. Setting this to 0 will delete the saved account on Feb 1st. Leave this blank to not delete expired saved accounts.",
        DefaultValue = null,
        Order = 3
        )]

    [DisallowConcurrentExecution]
    public class SendCreditCardExpirationNotices : IJob
    {
        /// <summary>
        /// Keys to use for Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string ExpiringCreditCardEmail = "ExpiringCreditCardEmail";
            public const string Workflow = "Workflow";
            public const string RemovedExpiredSavedAccountDays = "RemovedExpiredSavedAccountDays";
        }

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public SendCreditCardExpirationNotices()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            List<string> expiredCreditCardEmailMessageSendErrors;
            string expiredCreditCardSummary;
            string removeSavedAccountSummary;
            context.Result = string.Empty;
            StringBuilder jobSummaryBuilder = new StringBuilder();

            SendExpiredCreditCardNotices( context, out expiredCreditCardEmailMessageSendErrors, out expiredCreditCardSummary );
            jobSummaryBuilder.AppendLine( expiredCreditCardSummary );

            RemoveExpiredSavedAccounts( context, out removeSavedAccountSummary );
            jobSummaryBuilder.AppendLine( removeSavedAccountSummary );

            if ( expiredCreditCardEmailMessageSendErrors.Any() )
            {
                StringBuilder sb = new StringBuilder();
                string expiredCreditCardEmailErrors = $"{expiredCreditCardEmailMessageSendErrors.Count()} errors occurred when sending expired credit card warning.";
                jobSummaryBuilder.AppendLine( expiredCreditCardEmailErrors );
                context.UpdateLastStatusMessage( jobSummaryBuilder.ToString() );

                sb.AppendLine( expiredCreditCardEmailErrors );
                foreach ( var expiredCreditCardEmailMessageSendError in expiredCreditCardEmailMessageSendErrors )
                {
                    sb.AppendLine( expiredCreditCardEmailMessageSendError );
                }

                string errorMessage = sb.ToString();
                context.Result += errorMessage;
                var exception = new Exception( errorMessage );
                HttpContext context2 = HttpContext.Current;
                ExceptionLogService.LogException( exception, context2 );

                throw exception;
            }
        }

        /// <summary>
        /// Removes any expired saved accounts (if <see cref="AttributeKey.RemovedExpiredSavedAccountDays"/> is set)
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="removeSavedAccountSummary">The remove saved account summary.</param>
        private void RemoveExpiredSavedAccounts( IJobExecutionContext context, out string removeSavedAccountSummary )
        {
            var dataMap = context.JobDetail.JobDataMap;
            int? removedExpiredSavedAccountDays = dataMap.GetString( AttributeKey.RemovedExpiredSavedAccountDays ).AsIntegerOrNull();
            if ( !removedExpiredSavedAccountDays.HasValue )
            {
                removeSavedAccountSummary = string.Empty;
                return;
            }

            var financialPersonSavedAccountQry = new FinancialPersonSavedAccountService( new RockContext() ).Queryable()
                .Where( a => !string.IsNullOrWhiteSpace( a.FinancialPaymentDetail.ExpirationMonthEncrypted ) )
                .Where( a => a.PersonAliasId.HasValue || a.GroupId.HasValue )
                .Where( a => a.FinancialPaymentDetailId.HasValue )
                .Where( a => a.IsSystem == false )
                .OrderBy( a => a.Id );

            var savedAccountInfoList = financialPersonSavedAccountQry.Select( a => new SavedAccountInfo
            {
                Id = a.Id,
                FinancialPaymentDetailExpirationMonthEncrypted = a.FinancialPaymentDetail.ExpirationMonthEncrypted,
                FinancialPaymentDetailExpirationYearEncrypted = a.FinancialPaymentDetail.ExpirationYearEncrypted,
                FinancialPaymentDetailAccountNumberMasked = a.FinancialPaymentDetail.AccountNumberMasked,
            } );

            DateTime now = DateTime.Now;
            int currentMonth = now.Month;
            int currentYear = now.Year;
            var expirationDateCutoff = RockDateTime.Today.AddDays( removedExpiredSavedAccountDays.Value );

            int financialPersonSavedAccountDeleteCount = 0;

            foreach ( var savedAccountInfo in savedAccountInfoList )
            {
                int? expirationMonth = Encryption.DecryptString( savedAccountInfo.FinancialPaymentDetailExpirationMonthEncrypted ).AsIntegerOrNull();
                int? expirationYear = Encryption.DecryptString( savedAccountInfo.FinancialPaymentDetailExpirationYearEncrypted ).AsIntegerOrNull();
                if ( !expirationMonth.HasValue || !expirationMonth.HasValue )
                {
                    continue;
                }

                if ( expirationMonth.Value < 1 || expirationMonth.Value > 12 || expirationYear <= DateTime.MinValue.Year || expirationYear >= DateTime.MaxValue.Year )
                {
                    // invalid month (or year)
                    continue;
                }

                // a credit card with an expiration of April 2020 would be expired on May 1st, 2020
                var expirationDate = new DateTime( expirationYear.Value, expirationMonth.Value, 1 ).AddMonths( 1 );
                if ( expirationDate < expirationDateCutoff )
                {
                    // not considered expired yet
                    continue;
                }

                using ( var savedAccountRockContext = new RockContext() )
                {
                    var financialPersonSavedAccountService = new FinancialPersonSavedAccountService( savedAccountRockContext );
                    var financialPersonSavedAccount = financialPersonSavedAccountService.Get( savedAccountInfo.Id );
                    if ( financialPersonSavedAccount != null )
                    {
                        if ( financialPersonSavedAccountService.CanDelete( financialPersonSavedAccount, out _ ) )
                        {
                            financialPersonSavedAccountService.Delete( financialPersonSavedAccount );
                            savedAccountRockContext.SaveChanges();
                            financialPersonSavedAccountDeleteCount++;
                        }
                    }
                }
            }

            removeSavedAccountSummary = $"Removed {financialPersonSavedAccountDeleteCount} expired saved accounts.";
        }

        /// <summary>
        /// Sends the expired credit card notices.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="expiredCreditCardEmailMessageSendErrors">The expired credit card email message send errors.</param>
        /// <param name="summaryMessage">The summary message.</param>
        /// <exception cref="Exception">Expiring credit card email is missing.</exception>
        private void SendExpiredCreditCardNotices( IJobExecutionContext context, out List<string> expiredCreditCardEmailMessageSendErrors, out string summaryMessage )
        {
            var dataMap = context.JobDetail.JobDataMap;
            var rockContext = new RockContext();

            // Get the details for the email that we'll be sending out.
            Guid? systemEmailGuid = dataMap.GetString( AttributeKey.ExpiringCreditCardEmail ).AsGuidOrNull();
            SystemCommunication systemCommunication = null;

            if ( systemEmailGuid.HasValue )
            {
                var systemCommunicationService = new SystemCommunicationService( rockContext );
                systemCommunication = systemCommunicationService.Get( systemEmailGuid.Value );
            }

            if ( systemCommunication == null )
            {
                throw new Exception( "Expiring credit card email is missing." );
            }

            // Fetch the configured Workflow once if one was set, we'll use it later.
            Guid? workflowGuid = dataMap.GetString( AttributeKey.Workflow ).AsGuidOrNull();
            WorkflowTypeCache workflowType = null;
            WorkflowService workflowService = new WorkflowService( rockContext );
            if ( workflowGuid != null )
            {
                workflowType = WorkflowTypeCache.Get( workflowGuid.Value );
            }

            var financialScheduledTransactionQuery = new FinancialScheduledTransactionService( rockContext ).Queryable()
                .Where( t => t.IsActive && t.FinancialPaymentDetail.ExpirationMonthEncrypted != null && ( t.EndDate == null || t.EndDate > DateTime.Now ) )
                .AsNoTracking();

            List<ScheduledTransactionInfo> scheduledTransactionInfoList = financialScheduledTransactionQuery.Select( a => new ScheduledTransactionInfo
            {
                Id = a.Id,
                FinancialPaymentDetailExpirationMonthEncrypted = a.FinancialPaymentDetail.ExpirationMonthEncrypted,
                FinancialPaymentDetailExpirationYearEncrypted = a.FinancialPaymentDetail.ExpirationYearEncrypted,
                FinancialPaymentDetailAccountNumberMasked = a.FinancialPaymentDetail.AccountNumberMasked,
                AuthorizedPersonAliasGuid = a.AuthorizedPersonAlias.Guid,
                Person = a.AuthorizedPersonAlias.Person
            } ).ToList();

            // Get the current month and year 
            DateTime now = DateTime.Now;
            int currentMonth = now.Month;
            int currentYear = now.Year;
            int expiredCreditCardCount = 0;
            expiredCreditCardEmailMessageSendErrors = new List<string>();
            foreach ( ScheduledTransactionInfo scheduledTransactionInfo in scheduledTransactionInfoList )
            {
                int? expirationMonthDecrypted = Encryption.DecryptString( scheduledTransactionInfo.FinancialPaymentDetailExpirationMonthEncrypted ).AsIntegerOrNull();
                int? expirationYearDecrypted = Encryption.DecryptString( scheduledTransactionInfo.FinancialPaymentDetailExpirationYearEncrypted ).AsIntegerOrNull();
                if ( !expirationMonthDecrypted.HasValue || !expirationMonthDecrypted.HasValue )
                {
                    continue;
                }

                string maskedCardNumber = string.Empty;

                if ( !string.IsNullOrEmpty( scheduledTransactionInfo.FinancialPaymentDetailAccountNumberMasked ) && scheduledTransactionInfo.FinancialPaymentDetailAccountNumberMasked.Length >= 4 )
                {
                    maskedCardNumber = scheduledTransactionInfo.FinancialPaymentDetailAccountNumberMasked.Substring( scheduledTransactionInfo.FinancialPaymentDetailAccountNumberMasked.Length - 4 );
                }

                int warningYear = expirationYearDecrypted.Value;
                int warningMonth = expirationMonthDecrypted.Value - 1;
                if ( warningMonth == 0 )
                {
                    warningYear -= 1;
                    warningMonth = 12;
                }

                string warningDate = warningMonth.ToString() + warningYear.ToString();
                string currentMonthString = currentMonth.ToString() + currentYear.ToString();

                if ( warningDate == currentMonthString )
                {
                    // as per ISO7813 https://en.wikipedia.org/wiki/ISO/IEC_7813
                    var expirationDate = string.Format( "{0:D2}/{1:D2}", expirationMonthDecrypted, expirationYearDecrypted );

                    var recipients = new List<RockEmailMessageRecipient>();
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                    var person = scheduledTransactionInfo.Person;

                    if ( !person.IsEmailActive || person.Email.IsNullOrWhiteSpace() || person.EmailPreference == EmailPreference.DoNotEmail )
                    {
                        continue;
                    }

                    mergeFields.Add( "Person", person );
                    mergeFields.Add( "Card", maskedCardNumber );
                    mergeFields.Add( "Expiring", expirationDate );
                    recipients.Add( new RockEmailMessageRecipient( person, mergeFields ) );

                    var emailMessage = new RockEmailMessage( systemCommunication );
                    emailMessage.SetRecipients( recipients );

                    var emailErrors = new List<string>();
                    emailMessage.Send( out emailErrors );
                    expiredCreditCardEmailMessageSendErrors.AddRange( emailErrors );

                    // Start workflow for this person
                    if ( workflowType != null )
                    {
                        Dictionary<string, string> attributes = new Dictionary<string, string>();
                        attributes.Add( "Person", scheduledTransactionInfo.AuthorizedPersonAliasGuid.ToString() );
                        attributes.Add( "Card", maskedCardNumber );
                        attributes.Add( "Expiring", expirationDate );
                        attributes.Add( "FinancialScheduledTransactionId", scheduledTransactionInfo.Id.ToString() );

                        StartWorkflow( workflowService, workflowType, attributes, $"{person.FullName} (scheduled transaction Id: {scheduledTransactionInfo.Id})" );
                    }

                    expiredCreditCardCount++;
                }

            }

            summaryMessage = $"{scheduledTransactionInfoList.Count()} scheduled credit card transactions were examined with {expiredCreditCardCount} notice(s) sent.";
        }

        /// <summary>
        /// Starts the workflow.
        /// </summary>
        /// <param name="workflowService">The workflow service.</param>
        /// <param name="workflowType">Type of the workflow.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="workflowNameSuffix">The workflow instance name suffix (the part that is tacked onto the end fo the name to distinguish one instance from another).</param>
        protected void StartWorkflow( WorkflowService workflowService, WorkflowTypeCache workflowType, Dictionary<string, string> attributes, string workflowNameSuffix )
        {
            // launch workflow if configured
            if ( workflowType != null && ( workflowType.IsActive ?? true ) )
            {
                var workflow = Rock.Model.Workflow.Activate( workflowType, "SendCreditCardExpiration " + workflowNameSuffix );

                // set attributes
                foreach ( KeyValuePair<string, string> attribute in attributes )
                {
                    workflow.SetAttributeValue( attribute.Key, attribute.Value );
                }

                // launch workflow
                List<string> workflowErrors;
                workflowService.Process( workflow, out workflowErrors );
            }
        }

        private class ScheduledTransactionInfo
        {
            public int Id { get; set; }
            public string FinancialPaymentDetailExpirationMonthEncrypted { get; set; }
            public string FinancialPaymentDetailExpirationYearEncrypted { get; set; }
            public string FinancialPaymentDetailAccountNumberMasked { get; set; }
            public Guid AuthorizedPersonAliasGuid { get; set; }
            public Person Person { get; set; }
        }

        private class SavedAccountInfo
        {
            public int Id { get; set; }
            public string FinancialPaymentDetailExpirationMonthEncrypted { get; set; }
            public string FinancialPaymentDetailExpirationYearEncrypted { get; set; }
            public string FinancialPaymentDetailAccountNumberMasked { get; set; }
        }
    }
}

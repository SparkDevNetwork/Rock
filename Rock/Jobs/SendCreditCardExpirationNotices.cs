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
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Checks scheduled transactions for credit cards that are expiring next month and sends an email notice to the person.
    /// </summary>
    [DisplayName( "Expiring Credit Card Notices" )]
    [Description( "Checks scheduled transactions for credit cards that are expiring next month and sends an email notice to the person." )]

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
        IsRequired = false,
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
            string removeSavedAccountSummary;
            context.Result = string.Empty;
            StringBuilder jobSummaryBuilder = new StringBuilder();

            SendExpiredCreditCardNoticesResult sendExpiredCreditCardNoticesResult = SendExpiredCreditCardNotices( context );
            string sendExpiredCreditCardNoticesSummary = $"{sendExpiredCreditCardNoticesResult.ExaminedCount} scheduled credit card transactions were examined with {sendExpiredCreditCardNoticesResult.NoticesSendCount } notice(s) sent.";
            jobSummaryBuilder.AppendLine( sendExpiredCreditCardNoticesSummary );

            RemoveExpiredSavedAccountsResult removeExpiredSavedAccountsResult = RemoveExpiredSavedAccounts( context );
            if ( removeExpiredSavedAccountsResult.FinancialPersonSavedAccountDeleteCount.HasValue )
            {
                removeSavedAccountSummary = $"Removed {removeExpiredSavedAccountsResult.FinancialPersonSavedAccountDeleteCount} saved accounts that expired before {removeExpiredSavedAccountsResult.DeleteIfExpiredBeforeDate.ToShortDateString()}";
                jobSummaryBuilder.AppendLine( removeSavedAccountSummary );
            }

            context.Result = jobSummaryBuilder.ToString().Trim();
            if ( sendExpiredCreditCardNoticesResult.EmailErrors.Any() )
            {
                StringBuilder sb = new StringBuilder();
                string expiredCreditCardEmailErrors = $"{sendExpiredCreditCardNoticesResult.EmailErrors.Count()} errors occurred when sending expired credit card warning.";
                jobSummaryBuilder.AppendLine( expiredCreditCardEmailErrors );
                context.UpdateLastStatusMessage( jobSummaryBuilder.ToString() );

                sb.AppendLine( expiredCreditCardEmailErrors );
                foreach ( var expiredCreditCardEmailMessageSendError in sendExpiredCreditCardNoticesResult.EmailErrors )
                {
                    sb.AppendLine( expiredCreditCardEmailMessageSendError );
                }

                string errorMessage = sb.ToString();
                context.Result += errorMessage;
                var exception = new Exception( errorMessage );
                HttpContext context2 = HttpContext.Current;
                ExceptionLogService.LogException( exception, context2 );

                context.Result = jobSummaryBuilder.ToString().Trim();

                throw exception;
            }
        }

        /// <summary>
        /// Removes any expired saved accounts (if <see cref="AttributeKey.RemovedExpiredSavedAccountDays"/> is set)
        /// </summary>
        /// <param name="context">The context.</param>
        private RemoveExpiredSavedAccountsResult RemoveExpiredSavedAccounts( IJobExecutionContext context )
        {
            var dataMap = context.JobDetail.JobDataMap;
            int? removedExpiredSavedAccountDays = dataMap.GetString( AttributeKey.RemovedExpiredSavedAccountDays ).AsIntegerOrNull();
            if ( !removedExpiredSavedAccountDays.HasValue )
            {
                return new RemoveExpiredSavedAccountsResult();
            }

            var financialPersonSavedAccountQry = new FinancialPersonSavedAccountService( new RockContext() ).Queryable()
                .Where( a => a.FinancialPaymentDetail.ExpirationMonthEncrypted != null )
                .Where( a => a.PersonAliasId.HasValue || a.GroupId.HasValue )
                .Where( a => a.FinancialPaymentDetailId.HasValue )
                .Where( a => a.IsSystem == false )
                .OrderBy( a => a.Id );

            var savedAccountInfoList = financialPersonSavedAccountQry.Select( a => new SavedAccountInfo
            {
                Id = a.Id,
                FinancialPaymentDetail = a.FinancialPaymentDetail
            } ).ToList();

            DateTime now = DateTime.Now;
            int currentMonth = now.Month;
            int currentYear = now.Year;

            int financialPersonSavedAccountDeleteCount = 0;

            // if today is 3/16/2020 and removedExpiredSavedAccountDays is 90, only delete card if it expired before 12/17/2019
            var deleteIfExpiredBeforeDate = RockDateTime.Today.AddDays( -removedExpiredSavedAccountDays.Value );

            foreach ( var savedAccountInfo in savedAccountInfoList )
            {
                int? expirationMonth = savedAccountInfo.FinancialPaymentDetail.ExpirationMonth;
                int? expirationYear = savedAccountInfo.FinancialPaymentDetail.ExpirationYear;
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
                var cardExpirationDate = new DateTime( expirationYear.Value, expirationMonth.Value, 1 ).AddMonths( 1 );

                /* Example:
                 Today's Date: 2020-3-16
                 removedExpiredSavedAccountDays: 90
                 Expired Before Date: 2019-12-17 (Today (2020-3-16) - removedExpiredSavedAccountDays)
                 Cards that expired before 2019-12-17 should be deleted
                 Delete 04/20 (Expires 05/01/2020) card? No
                 Delete 05/20 (Expires 06/01/2020) card? No
                 Delete 01/20 (Expires 03/01/2020) card? No
                 Delete 12/19 (Expires 01/01/2020) card? No
                 Delete 11/19 (Expires 12/01/2019) card? Yes
                 Delete 10/19 (Expires 11/01/2019) card? Yes

                 Today's Date: 2020-3-16
                 removedExpiredSavedAccountDays: 0
                 Expired Before Date: 2019-03-16 (Today (2020-3-16) - 0)
                 Cards that expired before 2019-03-16 should be deleted
                 Delete 04/20 (Expires 05/01/2020) card? No
                 Delete 05/20 (Expires 06/01/2020) card? No
                 Delete 01/20 (Expires 03/01/2020) card? Yes
                 Delete 12/19 (Expires 01/01/2020) card? Yes
                 Delete 11/19 (Expires 12/01/2019) card? Yes
                 Delete 10/19 (Expires 11/01/2019) card? Yes
                 */

                if ( cardExpirationDate >= deleteIfExpiredBeforeDate )
                {
                    // We want to only delete cards that expired more than X days ago, so if this card expiration day is after that, skip
                    continue;
                }

                // Card expiration date is older than X day ago, so delete it
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

            RemoveExpiredSavedAccountsResult removeExpiredSavedAccountsResult = new RemoveExpiredSavedAccountsResult()
            {
                FinancialPersonSavedAccountDeleteCount = financialPersonSavedAccountDeleteCount,
                DeleteIfExpiredBeforeDate = deleteIfExpiredBeforeDate,
            };

            return removeExpiredSavedAccountsResult;
        }

        /// <summary>
        /// Sends the expired credit card notices.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Expiring credit card email is missing.</exception>
        private SendExpiredCreditCardNoticesResult SendExpiredCreditCardNotices( IJobExecutionContext context )
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
                FinancialPaymentDetail = a.FinancialPaymentDetail,
                AuthorizedPersonAliasGuid = a.AuthorizedPersonAlias.Guid,
                Person = a.AuthorizedPersonAlias.Person
            } ).ToList();

            // Get the current month and year 
            DateTime now = DateTime.Now;
            int currentMonth = now.Month;
            int currentYYYY = now.Year;
            int expiredCreditCardCount = 0;
            var expiredCreditCardEmailMessageSendErrors = new List<string>();

            // get the common merge fields once, so we don't have to keep calling it for every person, then create a new mergeFields for each person, starting with a copy of the common merge fields
            var commonMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );

            foreach ( ScheduledTransactionInfo scheduledTransactionInfo in scheduledTransactionInfoList.OrderByDescending( a => a.Id ) )
            {
                int? expirationMonth = scheduledTransactionInfo.FinancialPaymentDetail.ExpirationMonth;
                int? expirationYYYY = scheduledTransactionInfo.FinancialPaymentDetail.ExpirationYear;
                if ( !expirationMonth.HasValue || !expirationMonth.HasValue )
                {
                    continue;
                }

                int warningYear = expirationYYYY.Value;
                int warningMonth = expirationMonth.Value - 1;
                if ( warningMonth == 0 )
                {
                    warningYear -= 1;
                    warningMonth = 12;
                }

                if ( ( warningYear == currentYYYY ) && ( warningMonth == currentMonth ) )
                {
                    string maskedCardNumber = string.Empty;

                    if ( !string.IsNullOrEmpty( scheduledTransactionInfo.FinancialPaymentDetail.AccountNumberMasked ) && scheduledTransactionInfo.FinancialPaymentDetail.AccountNumberMasked.Length >= 4 )
                    {
                        maskedCardNumber = scheduledTransactionInfo.FinancialPaymentDetail.AccountNumberMasked.Substring( scheduledTransactionInfo.FinancialPaymentDetail.AccountNumberMasked.Length - 4 );
                    }

                    string expirationDateMMYYFormatted = scheduledTransactionInfo.FinancialPaymentDetail?.ExpirationDate;

                    var recipients = new List<RockEmailMessageRecipient>();

                    var person = scheduledTransactionInfo.Person;

                    if ( !person.IsEmailActive || person.Email.IsNullOrWhiteSpace() || person.EmailPreference == EmailPreference.DoNotEmail )
                    {
                        continue;
                    }

                    // make a mergeFields for this person, starting with copy of the commonFieldFields 
                    var mergeFields = new Dictionary<string, object>( commonMergeFields );
                    mergeFields.Add( "Person", person );
                    mergeFields.Add( "Card", maskedCardNumber );
                    mergeFields.Add( "Expiring", expirationDateMMYYFormatted );

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
                        attributes.Add( "Expiring", expirationDateMMYYFormatted );
                        attributes.Add( "FinancialScheduledTransactionId", scheduledTransactionInfo.Id.ToString() );

                        StartWorkflow( workflowService, workflowType, attributes, $"{person.FullName} (scheduled transaction Id: {scheduledTransactionInfo.Id})" );
                    }

                    expiredCreditCardCount++;
                }
            }

            return new SendExpiredCreditCardNoticesResult
            {
                EmailErrors = expiredCreditCardEmailMessageSendErrors,
                ExaminedCount = scheduledTransactionInfoList.Count(),
                NoticesSendCount = expiredCreditCardCount
            };
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

            public Guid AuthorizedPersonAliasGuid { get; set; }

            public Person Person { get; set; }

            public FinancialPaymentDetail FinancialPaymentDetail { get; set; }
        }

        private class SavedAccountInfo
        {
            public int Id { get; set; }

            public FinancialPaymentDetail FinancialPaymentDetail { get; set; }
        }

        private class SendExpiredCreditCardNoticesResult
        {
            public List<string> EmailErrors { get; set; } = new List<string>();

            public int ExaminedCount { get; set; }

            public int NoticesSendCount { get; set; }
        }

        private class RemoveExpiredSavedAccountsResult
        {
            public int? FinancialPersonSavedAccountDeleteCount { get; internal set; }

            public DateTime? DeleteIfExpiredBeforeDate { get; internal set; }
        }
    }
}

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

using Quartz;

using Rock.Attribute;
using Rock.Bus.Message;
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
    [Description( "Checks scheduled transactions for credit cards that are expiring next month and sends an email notice to the person. Optionally removes expired, saved accounts after a specified number of days from the account's expiration date." )]

    [SystemCommunicationField(
        "Expiring Credit Card Email",
        Key = AttributeKey.ExpiringCreditCardEmail,
        Description = "The system communication template to use for the credit card expiration notice. The merge fields 'Person', 'Card' (the last four digits of the credit card), and 'Expiring' (the MM/YYYY of expiration) will be available to the email template.",
        IsRequired = false,
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

    [BooleanField(
        "Enable Sending Bus Event",
        Key = AttributeKey.EnableSendingBusEvent,
        Description = "When enabled, a 'Credit Card Expiring Soon Message' message will be sent to the Rock message bus.",
        DefaultValue = "False",
        IsRequired = false,
        Order = 4
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
            public const string EnableSendingBusEvent = "EnableSendingBusEvent";
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
            context.Result = string.Empty;
            StringBuilder jobSummaryBuilder = new StringBuilder();
            jobSummaryBuilder.AppendLine( "Summary:" );
            jobSummaryBuilder.AppendLine( string.Empty );

            // Send emails.
            SendExpiredCreditCardNoticesResult sendExpiredCreditCardNoticesResult = SendExpiredCreditCardNotices( context );
            jobSummaryBuilder.AppendLine( $"{sendExpiredCreditCardNoticesResult.ExaminedCount} scheduled credit card transaction(s) were examined." );

            // Report any email send successes.
            if ( sendExpiredCreditCardNoticesResult.NoticesSentCount > 0 || !sendExpiredCreditCardNoticesResult.EmailSendExceptions.Any() )
            {
                jobSummaryBuilder.AppendLine( $"<i class='fa fa-circle text-success'></i> {sendExpiredCreditCardNoticesResult.NoticesSentCount } notice(s) sent." );
            }

            context.UpdateLastStatusMessage( jobSummaryBuilder.ToString() );

            // Combine all Exceptions encoutered along the way; we'll roll them into an AggregateException below.
            var innerExceptions = new List<Exception>();

            // Report any email send failures.
            if ( sendExpiredCreditCardNoticesResult.EmailSendExceptions.Any() )
            {
                jobSummaryBuilder.AppendLine( $"<i class='fa fa-circle text-danger'></i> {sendExpiredCreditCardNoticesResult.EmailSendExceptions.Count()} error(s) occurred when sending expired credit card notice(s). See exception log for details." );
                context.UpdateLastStatusMessage( jobSummaryBuilder.ToString() );

                innerExceptions.AddRange( sendExpiredCreditCardNoticesResult.EmailSendExceptions );
            }

            // Remove expired, saved accounts.
            var removeExpiredSavedAccountsResult = RemoveExpiredSavedAccounts( context );

            // Report any account removal successes.
            if ( removeExpiredSavedAccountsResult.AccountsDeletedCount > 0 )
            {
                jobSummaryBuilder.AppendLine( $"<i class='fa fa-circle text-success'></i> {removeExpiredSavedAccountsResult.AccountsDeletedCount} saved account(s) that expired before {removeExpiredSavedAccountsResult.DeleteIfExpiredBeforeDate.ToShortDateString()} removed." );
                context.UpdateLastStatusMessage( jobSummaryBuilder.ToString() );
            }

            // Report any account removal failures.
            if ( removeExpiredSavedAccountsResult.AccountRemovalExceptions.Any() )
            {
                jobSummaryBuilder.AppendLine( $"<i class='fa fa-circle text-danger'></i> {removeExpiredSavedAccountsResult.AccountRemovalExceptions.Count()} error(s) occurred when removing saved, expired account(s). See exception log for details." );
                context.UpdateLastStatusMessage( jobSummaryBuilder.ToString() );

                innerExceptions.AddRange( removeExpiredSavedAccountsResult.AccountRemovalExceptions );
            }

            // If any errors encountered, throw an exception to mark the job as unsuccessful.
            if ( innerExceptions.Any() )
            {
                var aggregateException = new AggregateException( innerExceptions );

                throw new RockJobWarningException( "Send Credit Card Expiration Notices completed with warnings.", aggregateException );
            }
        }

        /// <summary>
        /// Removes any expired saved accounts (if <see cref="AttributeKey.RemovedExpiredSavedAccountDays"/> is set)
        /// </summary>
        /// <param name="context">The context.</param>
        private FinancialPersonSavedAccountService.RemoveExpiredSavedAccountsResult RemoveExpiredSavedAccounts( IJobExecutionContext context )
        {
            var dataMap = context.JobDetail.JobDataMap;
            int? removedExpiredSavedAccountDays = dataMap.GetString( AttributeKey.RemovedExpiredSavedAccountDays ).AsIntegerOrNull();

            if ( !removedExpiredSavedAccountDays.HasValue )
            {
                return new FinancialPersonSavedAccountService.RemoveExpiredSavedAccountsResult();
            }

            var service = new FinancialPersonSavedAccountService( new RockContext() );
            return service.RemoveExpiredSavedAccounts( removedExpiredSavedAccountDays.Value );
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

            // Fetch the configured Workflow once if one was set, we'll use it later.
            Guid? workflowGuid = dataMap.GetString( AttributeKey.Workflow ).AsGuidOrNull();
            WorkflowTypeCache workflowType = null;
            WorkflowService workflowService = new WorkflowService( rockContext );
            if ( workflowGuid != null )
            {
                workflowType = WorkflowTypeCache.Get( workflowGuid.Value );
            }

            var financialScheduledTransactionQuery = new FinancialScheduledTransactionService( rockContext ).Queryable()
                .Where( t => t.IsActive && t.FinancialPaymentDetail.CardExpirationDate != null && ( t.EndDate == null || t.EndDate > RockDateTime.Now ) )
                .AsNoTracking();

            List<ScheduledTransactionInfo> scheduledTransactionInfoList = financialScheduledTransactionQuery.Select( a => new ScheduledTransactionInfo
            {
                Id = a.Id,
                FinancialPaymentDetail = a.FinancialPaymentDetail,
                AuthorizedPersonAliasGuid = a.AuthorizedPersonAlias.Guid,
                Person = a.AuthorizedPersonAlias.Person
            } ).ToList();

            // Get the current month and year 
            var now = RockDateTime.Now;
            int currentMonth = now.Month;
            int currentYYYY = now.Year;

            // get the common merge fields once, so we don't have to keep calling it for every person, then create a new mergeFields for each person, starting with a copy of the common merge fields
            var commonMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );

            var result = new SendExpiredCreditCardNoticesResult
            {
                ExaminedCount = scheduledTransactionInfoList.Count()
            };

            // get attibute value, so we don't have to keep calling it for every person,
            bool enableSendingEvent = dataMap.GetString( AttributeKey.EnableSendingBusEvent ).AsBoolean();

            foreach ( ScheduledTransactionInfo scheduledTransactionInfo in scheduledTransactionInfoList.OrderByDescending( a => a.Id ) )
            {
                int? expirationMonth = scheduledTransactionInfo.FinancialPaymentDetail.ExpirationMonth;
                int? expirationYYYY = scheduledTransactionInfo.FinancialPaymentDetail.ExpirationYear;
                if ( !expirationMonth.HasValue || !expirationYYYY.HasValue )
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

                    if ( enableSendingEvent )
                    {
                        var financialScheduledTransactions = scheduledTransactionInfoList.Where( m => m.Person.Id == person.Id ).Select( m => m.Id );
                        CreditCardIsExpiringMessage.Publish( person, scheduledTransactionInfo.FinancialPaymentDetail, financialScheduledTransactions.ToList() );
                    }

                    bool isOpenToEmail = person.IsEmailActive && !person.Email.IsNullOrWhiteSpace() && person.EmailPreference != EmailPreference.DoNotEmail;
                    if ( systemCommunication != null && isOpenToEmail )
                    {
                        // make a mergeFields for this person, starting with copy of the commonFieldFields 
                        var mergeFields = new Dictionary<string, object>( commonMergeFields );
                        mergeFields.Add( "Person", person );
                        mergeFields.Add( "Card", maskedCardNumber );
                        mergeFields.Add( "Expiring", expirationDateMMYYFormatted );

                        recipients.Add( new RockEmailMessageRecipient( person, mergeFields ) );

                        var emailMessage = new RockEmailMessage( systemCommunication );
                        emailMessage.SetRecipients( recipients );
                        emailMessage.Send( out List<string> emailErrors );

                        if ( emailErrors.Any() )
                        {
                            var errorLines = new StringBuilder();
                            errorLines.AppendLine( string.Empty );
                            foreach ( string error in emailErrors )
                            {
                                errorLines.AppendLine( error );
                            }

                            // Provide better identifying context in case the errors are too vague.
                            var exception = new Exception( $"Unable to send email (Person ID = {person.Id}).{errorLines}" );

                            result.EmailSendExceptions.Add( exception );
                        }
                        else
                        {
                            result.NoticesSentCount++;
                        }
                    }

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
                }
            }

            return result;
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
            public int ExaminedCount { get; set; }

            public int NoticesSentCount { get; set; }

            public IList<Exception> EmailSendExceptions { get; set; } = new List<Exception>();
        }
    }
}

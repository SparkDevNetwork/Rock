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
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;

using Humanizer;

using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace com.bemaservices.MultiEventRegistration.Jobs
{
    /// <summary>
    /// Job to run quick SQL queries on a schedule
    /// </summary>
    [IntegerField( "Root Category Id", "The  Id of the root Daycation category", true, 30, key: "RootCategoryId" )]
    [DisallowConcurrentExecution]
    public class SendRegistrationScheduledPaymentReminders : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public SendRegistrationScheduledPaymentReminders()
        {
        }

        /// <summary>
        /// Job that will run quick SQL queries on a schedule.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            // get registrations where
            //    + template is active
            //    + instance is active
            //    + template has a number of days between reminders
            //    + template as fields needed to send a reminder email
            //    + the registration has a cost
            //    + the registration has been closed within the last xx days (to prevent eternal nagging)

            using ( RockContext rockContext = new RockContext() )
            {
                int sendCount = 0;
                int registrationInstanceCount = 0;

                var appRoot = GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" );

                RegistrationService registrationService = new RegistrationService( rockContext );
                RegistrationInstanceService registrationInstanceService = new RegistrationInstanceService( rockContext );
                FinancialScheduledTransactionService financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );

                var currentDate = RockDateTime.Today;
                var rootCategoryId = dataMap.GetString( "RootCategoryId" ).AsIntegerOrNull() ?? 0;

                var registrationInstances = registrationInstanceService.Queryable()
                                            .Where( r =>
                                                r.RegistrationTemplate.IsActive
                                                && r.IsActive == true
                                                && r.RegistrationTemplate.PaymentReminderEmailTemplate != null && r.RegistrationTemplate.PaymentReminderEmailTemplate.Length > 0
                                                && r.RegistrationTemplate.PaymentReminderFromEmail != null && r.RegistrationTemplate.PaymentReminderFromEmail.Length > 0
                                                && r.RegistrationTemplate.PaymentReminderSubject != null && r.RegistrationTemplate.PaymentReminderSubject.Length > 0
                                                && r.RegistrationTemplate.CategoryId == rootCategoryId )
                                            .ToList();

                var errors = new List<string>();
                var instanceIds = new List<int>();
                foreach ( var registrationInstance in registrationInstances )
                {
                    registrationInstance.LoadAttributes();
                    var paymentDate = registrationInstance.GetAttributeValue( "PaymentDate" ).AsDateTime();
                    if ( paymentDate != null )
                    {
                        var reminderDate = paymentDate.Value.AddDays( -7 );

                        if ( reminderDate.Date == RockDateTime.Today.Date )
                        {
                            foreach ( var registration in registrationInstance.Registrations )
                            {
                                instanceIds.Add( registrationInstance.Id );
                                if ( registration.DiscountedCost > registration.TotalPaid )
                                {
                                    var entityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Registration ) ).Id;

                                    var scheduledTransaction = financialScheduledTransactionService.Queryable().AsNoTracking().Where( fst => fst.IsActive == true && fst.ScheduledTransactionDetails.Any( std => std.EntityTypeId == entityTypeId && std.EntityId == registration.Id ) ).FirstOrDefault();
                                    if ( scheduledTransaction != null )
                                    {
                                        Dictionary<string, object> mergeObjects = new Dictionary<string, object>();
                                        mergeObjects.Add( "Registration", registration );
                                        mergeObjects.Add( "RegistrationInstance", registration.RegistrationInstance );
                                        mergeObjects.Add( "ScheduledTransaction", scheduledTransaction );

                                        List<Dictionary<string, object>> summaryDetails = new List<Dictionary<string, object>>();
                                        decimal totalAmount = 0;

                                        foreach ( FinancialScheduledTransactionDetail detail in scheduledTransaction.ScheduledTransactionDetails )
                                        {
                                            Dictionary<string, object> detailSummary = new Dictionary<string, object>();
                                            detailSummary.Add( "AccountId", detail.Id );
                                            detailSummary.Add( "AccountName", detail.Account.Name );
                                            detailSummary.Add( "Amount", detail.Amount );
                                            detailSummary.Add( "Summary", detail.Summary );

                                            summaryDetails.Add( detailSummary );

                                            totalAmount += detail.Amount;
                                        }

                                        mergeObjects.Add( "ScheduledAmount", totalAmount );
                                        mergeObjects.Add( "TransactionDetails", summaryDetails );

                                        var emailMessage = new RockEmailMessage();
                                        emailMessage.AdditionalMergeFields = mergeObjects;
                                        emailMessage.AddRecipient( registration.GetConfirmationRecipient( mergeObjects ) );
                                        emailMessage.FromEmail = registration.RegistrationInstance.RegistrationTemplate.PaymentReminderFromEmail;
                                        emailMessage.FromName = registration.RegistrationInstance.RegistrationTemplate.PaymentReminderSubject;
                                        emailMessage.Subject = registration.RegistrationInstance.RegistrationTemplate.PaymentReminderSubject;
                                        emailMessage.Message = registration.RegistrationInstance.RegistrationTemplate.PaymentReminderEmailTemplate;
                                        var emailErrors = new List<string>();
                                        emailMessage.Send( out errors );

                                        registration.LastPaymentReminderDateTime = RockDateTime.Now;
                                        rockContext.SaveChanges();

                                        if ( !emailErrors.Any() )
                                        {
                                            sendCount++;
                                        }
                                    }

                                }
                            }
                        }
                    }
                }

                registrationInstanceCount = instanceIds.Distinct().Count();

                context.Result = string.Format( "Sent {0} from {1}", "reminder".ToQuantity( sendCount ), "registration instances".ToQuantity( registrationInstanceCount ) );
                if ( errors.Any() )
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine();
                    sb.Append( string.Format( "{0} Errors: ", errors.Count() ) );
                    errors.ForEach( e => { sb.AppendLine(); sb.Append( e ); } );
                    string errorMessage = sb.ToString();
                    context.Result += errorMessage;
                    var exception = new Exception( errorMessage );
                    HttpContext context2 = HttpContext.Current;
                    ExceptionLogService.LogException( exception, context2 );
                    throw exception;
                }
            }
        }

    }
}

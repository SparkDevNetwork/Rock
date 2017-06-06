using System;
using System.Collections.Generic;
using System.Linq;
using church.ccv.Hr.Data;
using church.ccv.Hr.Model;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Jobs;

namespace church.ccv.Hr.Jobs
{
    /// <summary>
    /// Sends out reminders to TimeCard Approvers that they have pending approvals waiting
    /// </summary>
    [SystemEmailField( "Notification Email Template", required: true, order: 0 )]
    [DisallowConcurrentExecution]

    public class SendTimeCardPendingApprovalReminder : IJob
    {
        List<NotificationItem> _notificationList = new List<NotificationItem>();
        
        /// <summary>
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        ///  </summary>
        public SendTimeCardPendingApprovalReminder()
        {
        }

        /// <summary>
        /// Executes the specified context
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            HrContext hrContext = new HrContext();
            RockContext rockContext = new RockContext();

            JobDataMap dataMap = context.JobDetail.JobDataMap;
            Guid? systemEmailGuid = dataMap.GetString( "NotificationEmailTemplate" ).AsGuidOrNull();

            if ( systemEmailGuid.HasValue )
            {
                TimeCardPayPeriodService timeCardPayPeriodService = new TimeCardPayPeriodService( hrContext );
                TimeCardService timeCardService = new TimeCardService( hrContext );
                
                // Get Current Pay period
                var currentPayPeriodQry = timeCardPayPeriodService.Queryable().OrderByDescending( a => a.StartDate ).FirstOrDefault();

                // get time cards
                var timeCardsQry = timeCardService.Queryable().Where( a => a.TimeCardPayPeriodId == currentPayPeriodQry.Id );

                if ( timeCardsQry.Any() )
                {
                    foreach ( var timeCard in timeCardsQry )
                    {
                        // Check for time cards in "Submitted" status and if found add approver to _notificationList
                        if ( timeCard.TimeCardStatus == TimeCardStatus.Submitted )
                        {
                            NotificationItem notification = new NotificationItem();
                            notification.Person = timeCard.SubmittedToPersonAlias.Person;
                            _notificationList.Add( notification );
                        }
                    }

                    if ( _notificationList.Any() )
                    {
                        // Send email
                        var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "PublicApplicationRoot" );
                        var recipients = new List<RecipientData>();

                        var notificationRecipients = _notificationList.GroupBy( p => p.Person.Id ).ToList();
                        foreach ( var recipientId in notificationRecipients )
                        {
                            var recipient = _notificationList.Where( n => n.Person.Id == recipientId.Key ).Select( n => n.Person ).FirstOrDefault();
                            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );

                            mergeFields.Add( "Person", recipient );

                            recipients.Add( new RecipientData( recipient.Email, mergeFields ) );

                            Email.Send( systemEmailGuid.Value, recipients, appRoot );

                            recipients.Clear();
                        }
                        context.Result = string.Format( "{0} approvals pending notiication {1} sent", recipients.Count, "email".PluralizeIf( recipients.Count() != 1 ) );
                    }
                    else
                    {
                        context.Result = "No time cards waiting in Submitted status";
                    }
                }
                else
                {
                    context.Result = "Warning: No Timecards were processed";
                }
            }
            else
            {
                context.Result = "Warning: No NotificationEmailTemplate Found";
            }
        }
    }
}

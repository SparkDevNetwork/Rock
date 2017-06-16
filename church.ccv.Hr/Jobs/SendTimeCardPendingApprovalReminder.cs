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
        List<TimeCard> _submittedTimeCardList = new List<TimeCard>();
        
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
                
                // Get Current Pay period and Previous Pay Period
                var currentPayPeriodQry = timeCardPayPeriodService.Queryable().OrderByDescending( a => a.StartDate ).FirstOrDefault();
                var lastPayPeriodQry = timeCardPayPeriodService.Queryable().OrderByDescending( a => a.StartDate ).Skip( 1 ).FirstOrDefault();

                // get time cards for both pay periods
                var timeCardsQry = timeCardService.Queryable().Where( a => a.TimeCardPayPeriodId == currentPayPeriodQry.Id || a.TimeCardPayPeriodId == lastPayPeriodQry.Id );
                
                if ( timeCardsQry.Any() )
                {
                    foreach ( var timeCard in timeCardsQry )
                    {
                        // Check for time cards in "Submitted" status and if found add timecard and approver to List for notification
                        if ( timeCard.TimeCardStatus == TimeCardStatus.Submitted )
                        {
                            _submittedTimeCardList.Add( timeCard );

                            NotificationItem notification = new NotificationItem();
                            notification.Person = timeCard.SubmittedToPersonAlias.Person;
                            if( !_notificationList.Where( p => p.Person == notification.Person ).Any() )
                            {
                                _notificationList.Add( notification );
                            }
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
                            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );

                            var recipient = _notificationList.Where( n => n.Person.Id == recipientId.Key ).Select( n => n.Person ).FirstOrDefault();
                            mergeFields.Add( "Person", recipient );

                            List<TimeCard> submittedTimeCards = _submittedTimeCardList.Where( n => n.SubmittedToPersonAlias.Person == recipient ).ToList();
                            mergeFields.Add( "TimeCards", submittedTimeCards );                                                        
                            
                            recipients.Add( new RecipientData( recipient.Email, mergeFields ) );

                            Email.Send( systemEmailGuid.Value, recipients, appRoot );

                            recipients.Clear();
                        }
                        context.Result = string.Format( "{0} {1} pending notification {2} sent", notificationRecipients.Count, "approval".PluralizeIf( notificationRecipients.Count() != 1), "email".PluralizeIf( recipients.Count() != 1 ) );
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Rock.Attribute;
using Rock.Jobs;

namespace church.ccv.Hr.Jobs
{
    /// <summary>
    /// Sends out reminders to Hourly staff who have not started a timecard for the current pay period
    /// </summary>
    [SystemEmailField( "Notification Email Template", required: true, order:0 )]
    [DisallowConcurrentExecution]

    public class SendTimeCardReminder : IJob
    {
        List<NotificationItem> _notificationList = new List<NotificationItem>();

        /// <summary>
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor
        /// </para>
        /// </summary>

    }
}

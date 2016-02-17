// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Web;
using Quartz;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to run quick SQL queries on a schedule
    /// </summary>
    
    [DisallowConcurrentExecution]
    public class SendRegistrationPaymentReminders : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public SendRegistrationPaymentReminders()
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
            //    + the registration has been closed within the last 30 days (to prevent eternal nagging)

            using ( RockContext rockContext = new RockContext())
            {
                var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read().GetValue( "ExternalApplicationRoot" );

                RegistrationService registrationService = new RegistrationService( rockContext );

                var cutoffDate = RockDateTime.Now.AddDays( -30 );

                var registrations = registrationService.Queryable( "RegistrationInstance" )
                                                .Where( r =>
                                                         r.RegistrationInstance.RegistrationTemplate.IsActive
                                                         && r.RegistrationInstance.IsActive == true
                                                         && r.RegistrationInstance.RegistrationTemplate.PaymentReminderTimeSpan != null
                                                         && r.RegistrationInstance.RegistrationTemplate.PaymentReminderEmailTemplate != null && r.RegistrationInstance.RegistrationTemplate.PaymentReminderEmailTemplate.Length > 0
                                                         && r.RegistrationInstance.RegistrationTemplate.PaymentReminderFromEmail != null && r.RegistrationInstance.RegistrationTemplate.PaymentReminderFromEmail.Length > 0
                                                         && r.RegistrationInstance.RegistrationTemplate.PaymentReminderSubject != null && r.RegistrationInstance.RegistrationTemplate.PaymentReminderSubject.Length > 0
                                                         && (r.RegistrationInstance.RegistrationTemplate.Cost != 0 || (r.RegistrationInstance.Cost != null && r.RegistrationInstance.Cost != 0))
                                                         && (r.RegistrationInstance.EndDateTime == null || r.RegistrationInstance.EndDateTime <= cutoffDate) )
                                                 .ToList();

                foreach(var registration in registrations )
                {
                    if ( registration.TotalCost > registration.TotalPaid )
                    {
                        var reminderDate = RockDateTime.Now.AddDays( registration.RegistrationInstance.RegistrationTemplate.PaymentReminderTimeSpan.Value * -1 );

                        if ( registration.LastPaymentReminderDateTime < reminderDate )
                        {
                            var recipients = new List<string>();

                            Dictionary<string, object> mergeObjects = new Dictionary<string, object>();
                            mergeObjects.Add( "Registration", registration );
                            mergeObjects.Add( "RegistrationInstance", registration.RegistrationInstance );

                            recipients.Add( registration.ConfirmationEmail );

                            string message = registration.RegistrationInstance.RegistrationTemplate.PaymentReminderEmailTemplate.ResolveMergeFields( mergeObjects );
                            string fromEmail = registration.RegistrationInstance.RegistrationTemplate.PaymentReminderFromEmail.ResolveMergeFields( mergeObjects );
                            string fromName = registration.RegistrationInstance.RegistrationTemplate.PaymentReminderFromName.ResolveMergeFields( mergeObjects );
                            string subject = registration.RegistrationInstance.RegistrationTemplate.PaymentReminderSubject.ResolveMergeFields( mergeObjects );

                            Email.Send( fromEmail, fromName, subject, recipients, message, appRoot );

                            registration.LastPaymentReminderDateTime = RockDateTime.Now;
                            rockContext.SaveChanges();
                        }
                    }
                }

                                                    
            }
        }

    }
}

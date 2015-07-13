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
using System.Linq;

using Quartz;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{

    /// <summary>
    /// Job to process event registration reminders
    /// </summary>
    [DisallowConcurrentExecution]
    public class SendRegistrationReminders : IJob
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendCommunications"/> class.
        /// </summary>
        public SendRegistrationReminders()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            using ( var rockContext = new RockContext() )
            {
                DateTime now = RockDateTime.Now;

                foreach ( var instance in new RegistrationInstanceService( rockContext )
                    .Queryable( "RegistrationTemplate,Registrations" ).AsNoTracking()
                    .Where( i => 
                        i.IsActive &&
                        i.RegistrationTemplate.IsActive &&
                        i.RegistrationTemplate.ReminderEmailTemplate != "" &&
                        !i.ReminderSent &&
                        i.SendReminderDateTime.HasValue &&
                        i.SendReminderDateTime <= now ) )
                {
                    var template = instance.RegistrationTemplate;

                    foreach ( var registration in instance.Registrations
                        .Where( r => 
                            r.ConfirmationEmail != null &&
                            r.ConfirmationEmail != "") )
                    {
                        var mergeFields = new Dictionary<string, object>();
                        mergeFields.Add( "RegistrationInstance", registration.RegistrationInstance );
                        mergeFields.Add( "Registration", registration );

                        string from = template.ReminderFromName.ResolveMergeFields( mergeFields );
                        string fromName = template.ReminderFromEmail.ResolveMergeFields( mergeFields );
                        string subject = template.ReminderSubject.ResolveMergeFields( mergeFields );
                        string message = template.ReminderEmailTemplate.ResolveMergeFields( mergeFields );

                        var recipients = new List<string> { registration.ConfirmationEmail };
                        Email.Send( from, fromName, subject, recipients, message );
                    }

                    instance.SendReminderDateTime = now;
                    instance.ReminderSent = true;
                    rockContext.SaveChanges();
                }
            }
        }
    }
}
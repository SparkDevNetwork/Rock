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
using System.ComponentModel;
using System.Linq;

using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{

    /// <summary>
    /// This job fetches Rock notifications from the Spark Development Network.
    /// </summary>
    [DisplayName( "Spark Link" )]
    [Description( "This job fetches Rock notifications from the Spark Development Network." )]

    [DisallowConcurrentExecution]
    [GroupField( "Notification Group", "The group that should receive incoming notifications", true, Rock.SystemGuid.Group.GROUP_ADMINISTRATORS )]
    public class SparkLink : IJob
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SparkLink"/> class.
        /// </summary>
        public SparkLink()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var groupGuid = dataMap.Get( "NotificationGroup" ).ToString().AsGuid();

            var rockContext = new RockContext();
            var group = new GroupService( rockContext ).Get( groupGuid );

            if ( group != null )
            {
                var notifications = Utility.SparkLinkHelper.SendToSpark( rockContext );
                if ( notifications.Count == 0 )
                {
                    return;
                }

                var notificationService = new NotificationService( rockContext);
                foreach ( var notification in notifications.ToList() )
                {
                    if (notificationService.Get(notification.Guid) == null )
                    {
                        notificationService.Add( notification );
                    }
                    else
                    {
                        notifications.Remove( notification );
                    }
                }
                rockContext.SaveChanges();

                var notificationRecipientService = new NotificationRecipientService( rockContext );
                foreach (var notification in notifications )
                {
                    foreach ( var member in group.Members )
                    {
                        if ( member.Person.PrimaryAliasId.HasValue )
                        {
                            var recipientNotification = new NotificationRecipient();
                            recipientNotification.NotificationId = notification.Id;
                            recipientNotification.PersonAliasId = member.Person.PrimaryAliasId.Value;
                            notificationRecipientService.Add( recipientNotification );
                        }
                    }
                }
                rockContext.SaveChanges();                    
                
            }
        }

    }
}
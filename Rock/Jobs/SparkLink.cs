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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using Quartz;
using Rock.Data;
using Rock.Store;
using Rock.VersionInfo;
using Rock;
using System.Linq;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rock.Attribute;
using Rock.Model;

namespace Rock.Jobs
{

    /// <summary>
    /// Job to process event registration reminders
    /// </summary>
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

        protected class SparkLinkRequest
        {
            public string RockVersion { get; set; }
            public List<int> VersionIds { get; set; }

        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var groupGuid = dataMap.Get( "NotificationGroup" ).ToString().AsGuidOrNull();

            if ( groupGuid.HasValue )
            {
                var rockContext = new RockContext();
                var group = new GroupService( rockContext ).Get( groupGuid.Value );

                var installedPackages = InstalledPackageService.GetInstalledPackages();
                var sparLinkRequest = new SparkLinkRequest();
                sparLinkRequest.VersionIds = installedPackages.Select( i => i.VersionId ).ToList();
                sparLinkRequest.RockVersion = VersionInfo.VersionInfo.GetRockSemanticVersionNumber();
                var sparkLinkRequestJson = JsonConvert.SerializeObject( sparLinkRequest );

                var client = new RestClient( "http://rockrms.com/api/SparkLink/update" );
                var request = new RestRequest( "/", Method.POST );
                request.AddParameter( "application/json", sparkLinkRequestJson, ParameterType.RequestBody );
                IRestResponse response = client.Execute( request );

                if ( response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.Accepted )
                {
                    var notifications = JsonConvert.DeserializeObject<List<Notification>>(response.Content);
                    
                    if (notifications.Count == 0 )
                    {
                        return;
                    }
                    var notificationService = new NotificationService( rockContext);
                    foreach ( var notification in notifications )
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
                                var recipientNotification = new Rock.Model.NotificationRecipient();
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
}
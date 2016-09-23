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
using System.Net;

using Quartz;
using Rock.Data;
using Rock.Store;
using System.Linq;
using RestSharp;
using Newtonsoft.Json;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;

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
                var installedPackages = InstalledPackageService.GetInstalledPackages();

                var sparkLinkRequest = new SparkLinkRequest();
                sparkLinkRequest.RockInstanceId = Rock.Web.SystemSettings.GetRockInstanceId();
                sparkLinkRequest.OrganizationName = GlobalAttributesCache.Value( "OrganizationName" );
                sparkLinkRequest.VersionIds = installedPackages.Select( i => i.VersionId ).ToList();
                sparkLinkRequest.RockVersion = VersionInfo.VersionInfo.GetRockSemanticVersionNumber();

                var notifications = new List<Notification>();

                var sparkLinkRequestJson = JsonConvert.SerializeObject( sparkLinkRequest );

                var client = new RestClient( "https://www.rockrms.com/api/SparkLink/update" );
                //var client = new RestClient( "http://localhost:57822/api/SparkLink/update" );
                var request = new RestRequest( Method.POST );
                request.AddParameter( "application/json", sparkLinkRequestJson, ParameterType.RequestBody );
                IRestResponse response = client.Execute( request );
                if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
                {
                    foreach ( var notification in JsonConvert.DeserializeObject<List<Notification>>( response.Content ) )
                    {
                        notifications.Add( notification );
                    }
                }

                if ( sparkLinkRequest.VersionIds.Any() )
                {
                    client = new RestClient( "https://www.rockrms.com/api/Packages/VersionNotifications" );
                    //client = new RestClient( "http://localhost:57822/api/Packages/VersionNotifications" );
                    request = new RestRequest( Method.GET );
                    request.AddParameter( "VersionIds", sparkLinkRequest.VersionIds.AsDelimited( "," ) );
                    response = client.Execute( request );
                    if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
                    {
                        foreach ( var notification in JsonConvert.DeserializeObject<List<Notification>>( response.Content ) )
                        {
                            notifications.Add( notification );
                        }
                    }
                }

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

        #region Helper Class

        /// <summary>
        /// 
        /// </summary>
        public class SparkLinkRequest
        {
            /// <summary>
            /// Gets or sets the rock instance identifier.
            /// </summary>
            /// <value>
            /// The rock instance identifier.
            /// </value>
            public Guid RockInstanceId { get; set; }

            /// <summary>
            /// Gets or sets the name of the organization.
            /// </summary>
            /// <value>
            /// The name of the organization.
            /// </value>
            public string OrganizationName { get;  set;}

            /// <summary>
            /// Gets or sets the rock version.
            /// </summary>
            /// <value>
            /// The rock version.
            /// </value>
            public string RockVersion { get; set; }

            /// <summary>
            /// Gets or sets the version ids.
            /// </summary>
            /// <value>
            /// The version ids.
            /// </value>
            public List<int> VersionIds { get; set; }
        }

        #endregion

    }
}
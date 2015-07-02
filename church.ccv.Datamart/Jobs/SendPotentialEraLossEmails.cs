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
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

using church.ccv.Datamart.Model;

namespace church.ccv.Utility
{
    /// <summary>
    /// Sends eRA potential loss email
    /// </summary>
    [SystemEmailField( "Email Template", required: true )]
    [DisallowConcurrentExecution]
    public class SendPotentialEraLossEmails : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public SendPotentialEraLossEmails()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            
            var rockContext = new RockContext();

            // get system email
            Guid? systemEmailGuid = dataMap.GetString( "EmailTemplate" ).AsGuidOrNull();

            SystemEmailService emailService = new SystemEmailService( rockContext );

            SystemEmail systemEmail = null;
            if ( systemEmailGuid.HasValue )
            {
                systemEmail = emailService.Get( systemEmailGuid.Value );
            }

            if ( systemEmail == null )
            {
                // no email specified, so nothing to do
                return;
            }

            // lookup the role id for neighborhood pastors
            var neighborhoodPastorRoleId = new GroupTypeRoleService(rockContext).Get( SystemGuid.GroupRole.GROUPROLE_NEIGHBORHOOD_PASTOR.AsGuid() ).Id;

            DatamartEraLossService eraLossService = new DatamartEraLossService( rockContext );

            var lossRecipients = eraLossService.Queryable()
                                    .Where( e => e.Processed == true && e.SendEmail == true && e.Sent == false ).ToList();

            foreach (var lossRecipient in lossRecipients)
            {
                var family = new GroupService( rockContext ).Get( lossRecipient.FamilyId );
                var headOfHouse = family.Members.AsQueryable().HeadOfHousehold();

                if ( headOfHouse != null )
                {
                    var neighborhoodPastor = new GroupService( rockContext ).GetGeofencingGroups( headOfHouse.Id, SystemGuid.GroupType.GROUPTYPE_NEIGHBORHOOD_AREA.AsGuid() )
                                                .SelectMany( g => g.Members.Where( m => m.GroupRoleId == neighborhoodPastorRoleId ) )
                                                .Select( m => m.Person )
                                                .FirstOrDefault();

                    if ( neighborhoodPastor != null )
                    {
                        var recipients = new List<string>();

                        var mergeFields = new Dictionary<string, object>();
                        mergeFields.Add( "Person", headOfHouse );
                        mergeFields.Add( "Pastor", neighborhoodPastor );

                        var globalAttributeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( null );
                        globalAttributeFields.ToList().ForEach( d => mergeFields.Add( d.Key, d.Value ) );

                        recipients.Add( headOfHouse.Email );

                        var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "ExternalApplicationRoot" );
                        Email.Send( neighborhoodPastor.Email, neighborhoodPastor.FullName, systemEmail.Subject, recipients, systemEmail.Body.ResolveMergeFields( mergeFields ), appRoot );

                    }
                }

                // mark sent
                lossRecipient.Sent = true;
                rockContext.SaveChanges();
            }
        }
    }
}

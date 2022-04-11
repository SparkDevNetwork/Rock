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
using System.ComponentModel;
using System.Linq;
using System.Text;
using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// This job will create new InteractionSessionLocation records and/or link existing InteractionSession records to the corresponding InteractionSessionLocation record.
    /// </summary>
    [DisplayName( "Populate Interaction Session Data" )]
    [Description( "This job will create new InteractionSessionLocation records and / or link existing InteractionSession records to the corresponding InteractionSessionLocation record." )]

    [ComponentField(
        "Rock.IpAddress.IpAddressLookupContainer, Rock",
        Name = "IP Address GeoCoding Component",
        Description = "The service that will perform the IP GeoCoding lookup for any new IPs that have not been GeoCoded.Not required to be set here because the job will use the first active component if one is not configured here.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.IPAddressGeoCodingComponent )]
    [IntegerField(
        "Lookback Maximum( Days )",
        Description = "The number of days into the past the job should look for unmatched addresses in the InteractionSession table. ( default 30 days ).",
        IsRequired = false,
        Order = 1,
        DefaultIntegerValue = 30,
        Key = AttributeKey.LookbackMaximumInDays )]
    [DisallowConcurrentExecution]
    public class PopulateInteractionSessionData : IJob
    {
        #region Keys

        /// <summary>
        /// Keys to use for the attributes
        /// </summary>
        public static class AttributeKey
        {
            /// <summary>
            /// IP Address GeoCoding Component
            /// </summary>
            public const string IPAddressGeoCodingComponent = "IPAddressGeoCodingComponent";

            /// <summary>
            /// Lookback Maximum in Days
            /// </summary>
            public const string LookbackMaximumInDays = "LookbackMaximumInDays";
        }

        #endregion Keys

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public PopulateInteractionSessionData()
        {
        }

        /// <summary> 
        /// Job that updates the JobPulse setting with the current date/time.
        /// This will allow us to notify an admin if the jobs stop running.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            // get the job map
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            StringBuilder results = new StringBuilder();
            var errors = new List<string>();
            List<Exception> exceptions = new List<Exception>();

            var rockContext = new RockContext();
            var currentDateTime = RockDateTime.Now;

            var channelIdsWithGeoTracking = GetInteractionChannelsWithGeoTracking();
            var interactionComponentQry = new InteractionComponentService( rockContext ).Queryable().Where( a => channelIdsWithGeoTracking.Contains( a.InteractionChannelId ) ).Select( a => a.Id );
            var lookbackMaximumInDays = dataMap.GetString( AttributeKey.LookbackMaximumInDays ).AsInteger();
            var startDate = RockDateTime.Now.Date.AddDays( -lookbackMaximumInDays );
            var interationQry = new InteractionService( rockContext ).Queryable().Where( a => a.CreatedDateTime >= startDate && interactionComponentQry.Contains( a.InteractionComponentId ) ).Select( a => a.Id );
            var ipAddressesQry = new InteractionSessionLocationService( rockContext ).Queryable().Select( a => a.IpAddress );
            var interactionSessionQuery = new InteractionSessionService( rockContext )
                .Queryable()
                .Where( a =>
                    !a.InteractionSessionLocationId.HasValue &&
                    a.IpAddress != null &&
                    a.IpAddress != string.Empty &&
                    a.CreatedDateTime >= startDate &&
                    a.Interactions.Any( b => interationQry.Contains( b.Id ) )
                    ).Take(1000);

            var notFoundIpAddressList = interactionSessionQuery.ToList();

            context.Result = results.ToString();

            if ( exceptions.Any() )
            {
                var exceptionList = new AggregateException( "One or more exceptions occurred in UpdatePersistedDatasets.", exceptions );
                throw new RockJobWarningException( "UpdatePersistedDatasets completed with warnings", exceptionList );
            }

        }

        private List<int> GetInteractionChannelsWithGeoTracking()
        {
            var channelMediumTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE.AsGuid() ).Id;
            var rockContext = new RockContext();
            var siteWithEnableGeoTracking = new SiteService( rockContext ).Queryable().Where( a => a.EnablePageViewGeoTracking );
            return new InteractionChannelService( rockContext )
                .Queryable()
                .Where( ic =>
                    ic.ChannelTypeMediumValueId == channelMediumTypeValueId &&
                    siteWithEnableGeoTracking.Any( b => ic.ChannelEntityId == b.Id ) )
                .Select( a => a.Id )
                .ToList();
        }
    }
}

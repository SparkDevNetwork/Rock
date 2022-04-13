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
using Rock.IpAddress;
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
    [IntegerField(
        "How Many Records",
        Description = "The number of interaction session records to process on each run of this job.",
        IsRequired = false,
        DefaultIntegerValue = 50000,
        Order = 2,
        Key = AttributeKey.HowManyRecords )]
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

            /// <summary>
            /// How Many Records
            /// </summary>
            public const string HowManyRecords = "HowManyRecords";
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
            var howManyRecords = dataMap.GetString( AttributeKey.HowManyRecords ).AsIntegerOrNull() ?? 50000;
            var lookbackMaximumInDays = dataMap.GetString( AttributeKey.LookbackMaximumInDays ).AsInteger();
            var ipAddressComponentGuid = dataMap.GetString( AttributeKey.IPAddressGeoCodingComponent);
            var startDate = RockDateTime.Now.Date.AddDays( -lookbackMaximumInDays );
            StringBuilder results = new StringBuilder();
            var errors = new List<string>();
            List<Exception> exceptions = new List<Exception>();
            bool anyRemaining = true;
            var ipAddressSessionKeyValue = new Dictionary<string, List<int>>();
            var channelIdsWithGeoTracking = GetInteractionChannelsWithGeoTracking();
            var componentIds = InteractionComponentCache.All().Where( a => channelIdsWithGeoTracking.Contains( a.InteractionChannelId ) ).Select( a => a.Id ).ToList();
            var filterOnComponentQueryable = false;
            if ( componentIds.Count > 5000 )
            {
                filterOnComponentQueryable = true;
            }
            var recordsProcessed = 0;
            var maxSessionRecords = 1000;
            while ( howManyRecords > 0 )
            {
                using ( var rockContext = new RockContext() )
                {
                    var currentDateTime = RockDateTime.Now;
                    var interactionComponentQry = new InteractionComponentService( rockContext )
                        .Queryable()
                        .Where( a => channelIdsWithGeoTracking.Contains( a.InteractionChannelId ) )
                        .Select( a => a.Id );
                    var interactionQry = new InteractionService( rockContext )
                        .Queryable()
                        .Where( a => a.InteractionDateTime >= startDate );
                    if ( filterOnComponentQueryable )
                    {
                        interactionQry = interactionQry.Where( a => interactionComponentQry.Contains( a.InteractionComponentId ) );
                    }
                    else
                    {
                        interactionQry = interactionQry.Where( a => componentIds.Contains( a.InteractionComponentId ) );
                    }

                    var interactionIdsQry = interactionQry.Select( a => a.Id );
                    var interactionSessionLocationQry = new InteractionSessionLocationService( rockContext ).Queryable();
                    var take = howManyRecords < maxSessionRecords ? howManyRecords : maxSessionRecords;
                    var interactionSessionQuery = new InteractionSessionService( rockContext )
                        .Queryable()
                        .Where( a =>
                            !a.InteractionSessionLocationId.HasValue &&
                            a.IpAddress != null &&
                            a.IpAddress != string.Empty && a.IpAddress != "::1" && !a.IpAddress.Contains( "192.") && !a.IpAddress.Contains( "172." ) &&
                            a.Interactions.Any( b => interactionIdsQry.Contains( b.Id ) ) )
                        .Take( 1000 );

                    anyRemaining = interactionSessionQuery.Count() >= take;
                    howManyRecords = anyRemaining ? howManyRecords - take : 0;

                    foreach ( var interactionSession in interactionSessionQuery )
                    {
                        var interactionSessionLocationId = interactionSessionLocationQry
                            .Where( m => m.IpAddress == interactionSession.IpAddress )
                            .Select( a => a.Id )
                            .FirstOrDefault();
                        if ( interactionSessionLocationId != default( int ) )
                        {
                            interactionSession.InteractionSessionLocationId = interactionSessionLocationId;
                            recordsProcessed += 1;
                        }
                        else
                        {
                            if ( !ipAddressSessionKeyValue.ContainsKey( interactionSession.IpAddress ) )
                            {
                                ipAddressSessionKeyValue.Add( interactionSession.IpAddress, new List<int> { interactionSession.Id } );
                            }
                            else
                            {
                                ipAddressSessionKeyValue[interactionSession.IpAddress].Add( interactionSession.Id );
                            }
                        }
                    }

                    rockContext.SaveChanges(); 
                }
            }

            if ( ipAddressSessionKeyValue.Count > 0 )
            {
                var ipAddressSessionValueCount = ipAddressSessionKeyValue.Count;
                var requestCount = 0;
                var maxRecords = 50000;
                while ( ipAddressSessionValueCount > 0 )
                {
                    var take = ipAddressSessionValueCount > maxRecords ? maxRecords : ipAddressSessionValueCount;
                    var ipAddresses = ipAddressSessionKeyValue.Skip( requestCount * maxRecords ).Take( take ).ToDictionary( pair => pair.Key, pair => pair.Value );
                    var provider = IpAddressLookupContainer.GetComponent( ipAddressComponentGuid );
                    if ( provider != null )
                    {
                        var errorMessage = string.Empty;
                        var lookupResult = provider.Lookup( ipAddresses, out errorMessage );
                        recordsProcessed += lookupResult.SuccessCount;
                    }

                    ipAddressSessionValueCount = ipAddressSessionValueCount - take;
                    requestCount += 1;
                }
            }

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

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
using System.Diagnostics;
using System.Linq;
using System.Text;
using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.IpAddress;
using Rock.Logging;
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
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.IPAddressGeoCodingComponent )]
    [IntegerField(
        "Lookback Maximum (days)",
        Description = "The number of days into the past the job should look for unmatched addresses in the InteractionSession table. (default 30 days)",
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
    [IntegerField(
        "Command Timeout",
        AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of Interactions, this could take several hours or more.",
        IsRequired = false,
        DefaultIntegerValue = AttributeDefaultValue.CommandTimeout )]
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

            /// <summary>
            /// Command Timeout
            /// </summary>
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <summary>
        /// Default Values for Attributes
        /// </summary>
        private static class AttributeDefaultValue
        {
            public const int CommandTimeout = 60 * 60;
        }

        #endregion Keys

        #region Fields

        private List<string> _errors;

        private List<Exception> _exceptions;

        private int? _commandTimeout = null;

        #endregion

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
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            _errors = new List<string>();
            _exceptions = new List<Exception>();
            StringBuilder results = new StringBuilder();
            // get the configured timeout, or default to 20 minutes if it is blank
            _commandTimeout = dataMap.GetString( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 3600;

            var result = ProcessInteractionSessionForIP( context );
            results.AppendLine( result );

            result = ProcessInteractionCountAndDuration( context );
            results.AppendLine( result );

            foreach ( var error in _errors )
            {
                results.AppendLine( $"<i class='fa fa-circle text-danger'></i> {error}" );
            }
            context.Result = results.ToString();

            if ( _exceptions.Any() )
            {
                var exceptionList = new AggregateException( "One or more exceptions occurred in Process Interaction Session Data.", _exceptions );
                throw new RockJobWarningException( "Process Interaction Session Data completed with warnings", exceptionList );
            }
        }

        private string ProcessInteractionCountAndDuration( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var anyRemaining = true;
            var howManyRecords = dataMap.GetString( AttributeKey.HowManyRecords ).AsIntegerOrNull() ?? 50000;
            var maxSessionRecords = 2000;
            var totalRecordsProcessed = 0;
            Stopwatch stopwatch = Stopwatch.StartNew();
            while ( howManyRecords > 0 )
            {
                var recordsTobeUpdated = 0;
                var rockContext = new RockContext();
                rockContext.Database.CommandTimeout = _commandTimeout;
                var startDate = RockDateTime.Now.AddDays( -1 );
                var take = howManyRecords < maxSessionRecords ? howManyRecords : maxSessionRecords;
                var interactionSessionQuery = new InteractionSessionService( rockContext )
                    .Queryable( "Interactions" )
                    .Where( a => a.CreatedDateTime >= startDate &&
                                 !( a.DurationLastCalculatedDateTime.HasValue && !a.Interactions.Any( b => b.InteractionDateTime > a.DurationLastCalculatedDateTime ) ) )

                    .Take( take );

                context.UpdateLastStatusMessage( $"Processing Interaction Count And Session Duration : {take} sessions are being processed currently. Total {totalRecordsProcessed} Interaction Session{( totalRecordsProcessed < 2 ? "" : "s" )} are processed till now." );

                foreach ( var interactionSession in interactionSessionQuery )
                {
                    recordsTobeUpdated += 1;
                    interactionSession.InteractionCount = interactionSession.Interactions.Count();
                    interactionSession.DurationSeconds = ( int ) ( interactionSession.Interactions.Max( b => b.InteractionDateTime ) - interactionSession.Interactions.Min( b => b.InteractionDateTime ) ).TotalSeconds;
                    interactionSession.DurationLastCalculatedDateTime = RockDateTime.Now;
                }

                rockContext.SaveChanges();
                totalRecordsProcessed += recordsTobeUpdated;

                var totalRemainingCount = interactionSessionQuery.Count();
                anyRemaining = totalRemainingCount >= take;
                howManyRecords = anyRemaining ? howManyRecords - take : 0;
            }

            stopwatch.Stop();
            RockLogger.Log.Debug( RockLogDomains.Jobs, "{0} ({1}): Completed in {2} seconds.", nameof( PopulateInteractionSessionData ), "Process Interaction Count And Duration", stopwatch.Elapsed.TotalSeconds );
            return $"<i class='fa fa-circle text-success'></i> Updated Interaction Count And Session Duration for {totalRecordsProcessed} {"interaction session".PluralizeIf( totalRecordsProcessed != 1 )}";
        }

        private string ProcessInteractionSessionForIP( IJobExecutionContext jobContext )
        {
            JobDataMap dataMap = jobContext.JobDetail.JobDataMap;
            _commandTimeout = dataMap.GetString( "CommandTimeout" ).AsIntegerOrNull() ?? 3600;
            var lookbackMaximumInDays = dataMap.GetString( AttributeKey.LookbackMaximumInDays ).AsInteger();
            var startDate = RockDateTime.Now.Date.AddDays( -lookbackMaximumInDays );
            var anyRemaining = true;

            Stopwatch stopwatch = Stopwatch.StartNew();
            var howManyRecords = dataMap.GetString( AttributeKey.HowManyRecords ).AsIntegerOrNull() ?? 50000;
            var ipAddressComponentGuid = dataMap.GetString( AttributeKey.IPAddressGeoCodingComponent );
            var ipAddressSessionKeyValue = new Dictionary<string, List<int>>();
            var channelIdsWithGeoTracking = GetInteractionChannelsWithGeoTracking();
            var componentIds = InteractionComponentCache.All().Where( a => channelIdsWithGeoTracking.Contains( a.InteractionChannelId ) ).Select( a => a.Id ).ToList();
            if ( !componentIds.Any() )
            {
                return $"Processing Interaction Session : No Interaction Session found.";
            }

            var filterOnComponentQueryable = false;
            if ( componentIds.Count > 5000 )
            {
                filterOnComponentQueryable = true;
            }

            var recordsUpdated = 0;
            var totalRecordsProcessed = 0;
            var maxSessionRecords = 1000;
            while ( howManyRecords > 0 )
            {
                try
                {
                    var rockContext = new RockContext();
                    rockContext.Database.CommandTimeout = _commandTimeout;
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
                            a.IpAddress != string.Empty && a.IpAddress != "::1" && !a.IpAddress.Contains( "192." ) && !a.IpAddress.Contains( "172." ) &&
                            a.Interactions.Any( b => interactionIdsQry.Contains( b.Id ) ) )
                        .Take( take );

                    var totalRemainingCount = interactionSessionQuery.Count();
                    anyRemaining = totalRemainingCount >= take;
                    howManyRecords = anyRemaining ? howManyRecords - take : 0;

                    // Update the progress
                    jobContext.UpdateLastStatusMessage( $"Processing Interaction Session for IP : {take} IP's are being processed currently. Total {recordsUpdated} Interaction Session{( recordsUpdated < 2 ? "" : "s" )} are processed till now. " );

                    foreach ( var interactionSession in interactionSessionQuery )
                    {
                        totalRecordsProcessed += 1;
                        var interactionSessionLocationId = interactionSessionLocationQry
                            .Where( m => m.IpAddress == interactionSession.IpAddress )
                            .Select( a => a.Id )
                            .FirstOrDefault();
                        if ( interactionSessionLocationId != default( int ) )
                        {
                            interactionSession.InteractionSessionLocationId = interactionSessionLocationId;
                            recordsUpdated += 1;
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
                catch ( Exception ex )
                {
                    // Capture and log the exception because we're not going to fail this job
                    // unless all the data views fail.
                    var message = $"An error occurred while trying to process interaction sessions. Error: {ex.Message}";
                    _errors.Add( string.Format( @"ProcessInteractionSessionForIP method after {0} records.", totalRecordsProcessed ) );
                    var ex2 = new Exception( message, ex );
                    _exceptions.Add( ex2 );
                    ExceptionLogService.LogException( ex2, null );
                    continue;
                }
            }

            var warningMsg = string.Empty;
            if ( ipAddressSessionKeyValue.Count > 0 )
            {
                IpAddressLookupComponent provider = null;
                if ( ipAddressComponentGuid.AsGuidOrNull().HasValue )
                {
                    provider = IpAddressLookupContainer.GetComponent( ipAddressComponentGuid );
                }

                if ( provider == null )
                {
                    provider = IpAddressLookupContainer.Instance.Components.Select( a => a.Value.Value ).Where( x => x.IsActive ).FirstOrDefault();
                }

                if ( provider != null )
                {
                    jobContext.UpdateLastStatusMessage( $"Processing Interaction Session : Total {recordsUpdated} Interaction Session{( recordsUpdated < 2 ? "" : "s" )} are processed till now. {ipAddressSessionKeyValue.Count} sent to LookupComponent to process." );
                    recordsUpdated = ProcessIPOnLookupComponent( provider, ipAddressSessionKeyValue );
                }
                else
                {
                    jobContext.UpdateLastStatusMessage( $"Processing Interaction Session : Total {recordsUpdated} Interaction Session{( recordsUpdated < 2 ? "" : "s" )} are processed till now." );
                    warningMsg = $"There is no LookupComponent configured to process {ipAddressSessionKeyValue.Count} records.";
                }
            }
            stopwatch.Stop();
            RockLogger.Log.Debug( RockLogDomains.Jobs, "{0} ({1}): Completed in {2} seconds.", nameof( PopulateInteractionSessionData ), "Process Interaction Session", stopwatch.Elapsed.TotalSeconds );
            // Format the result message
            return $"<i class='fa fa-circle text-success'></i> Updated {recordsUpdated} out of {totalRecordsProcessed} {"interaction session".PluralizeIf( totalRecordsProcessed != 1 )}. {warningMsg}";
        }

        private int ProcessIPOnLookupComponent( IpAddressLookupComponent provider, Dictionary<string, List<int>> ipAddressSessionKeyValue )
        {
            var recordsProcessed = 0;
            if ( ipAddressSessionKeyValue.Count > 0 )
            {
                var ipAddressSessionValueCount = ipAddressSessionKeyValue.Count;
                var requestCount = 0;
                var maxRecords = 50000;
                while ( ipAddressSessionValueCount > 0 )
                {
                    var take = ipAddressSessionValueCount > maxRecords ? maxRecords : ipAddressSessionValueCount;
                    var ipAddresses = ipAddressSessionKeyValue.Skip( requestCount * maxRecords ).Take( take ).ToDictionary( pair => pair.Key, pair => pair.Value );
                    var errorMessage = string.Empty;
                    try
                    {
                        var lookupResult = provider.Lookup( ipAddresses, out errorMessage );
                        if ( errorMessage.IsNotNullOrWhiteSpace() )
                        {
                            _errors.Add( string.Format( @"Ip Lookup Component failed with batch of {0} IP with error message {1}.", ipAddresses.Count, errorMessage ) );
                        }

                        recordsProcessed += lookupResult.SuccessCount;
                    }
                    catch ( Exception ex )
                    {
                        // Capture and log the exception because we're not going to fail this job
                        // unless all the data views fail.
                        var message = $"An error occurred while trying to lookup IP Addresses from Lookup Component so it was skipped. Error: {ex.Message}";
                        _errors.Add( string.Format( @"Ip Lookup Component failed with batch of {0} IP.", ipAddresses.Count ) );
                        var ex2 = new Exception( message, ex );
                        _exceptions.Add( ex2 );
                        ExceptionLogService.LogException( ex2, null );
                        continue;
                    }

                    ipAddressSessionValueCount = ipAddressSessionValueCount - take;
                    requestCount += 1;
                }
            }

            return recordsProcessed;
        }

        private List<int> GetInteractionChannelsWithGeoTracking()
        {
            var channelMediumTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE.AsGuid() ).Id;
            var rockContext = new RockContext();
            rockContext.Database.CommandTimeout = _commandTimeout;
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
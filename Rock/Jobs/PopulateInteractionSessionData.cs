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
using EF6.TagWith;
using Rock.Attribute;
using Rock.Data;
using Rock.IpAddress;
using Rock.Logging;
using Rock.Model;
using Rock.SystemKey;

namespace Rock.Jobs
{
    /// <summary>
    /// This job will create new InteractionSessionLocation records and/or link existing InteractionSession records to the corresponding InteractionSessionLocation record.
    /// </summary>
    [DisplayName( "Populate Interaction Session Data" )]
    [Description( "This job will create new InteractionSessionLocation records and / or link existing InteractionSession records to the corresponding InteractionSessionLocation record." )]

    [ComponentField(
        "Rock.IpAddress.IpAddressLookupContainer, Rock",
        Name = "IP Address Geocoding Component",
        Description = "The service that will perform the IP GeoCoding lookup for any new IPs that have not been GeoCoded. Not required to be set here because the job will use the first active component if one is not configured here.",
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
        "Max Records To Process Per Run",
        Description = "The number of unique IP addresses to process on each run of this job.",
        IsRequired = false,
        DefaultIntegerValue = 50000,
        Order = 2,
        Key = AttributeKey.MaxRecordsToProcessPerRun )]
    [IntegerField(
        "Command Timeout",
        AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of Interactions, this could take several hours or more.",
        IsRequired = false,
        DefaultIntegerValue = AttributeDefaultValue.CommandTimeout )]
    public class PopulateInteractionSessionData : RockJob
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
            public const string MaxRecordsToProcessPerRun = "HowManyRecords";

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
            public const int CommandTimeout = 3600; // one hour in seconds
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

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            // Get the configured timeout, or default to 20 minutes if it is blank
            _commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 3600;

            var settings = Rock.Web.SystemSettings
                .GetValue( SystemSetting.POPULATE_INTERACTION_SESSION_DATA_JOB_SETTINGS )
                .FromJsonOrNull<PopulateInteractionSessionDataJobSettings>() ?? new PopulateInteractionSessionDataJobSettings();

            // Read settings from job
            settings.MaxRecordsToProcessPerRun = GetAttributeValue( AttributeKey.MaxRecordsToProcessPerRun ).AsIntegerOrNull() ?? 50000;

            var jobResult = Execute( settings );

            this.Result = jobResult.GetResultSummaryHtml();

            // Log caught exceptions
            if ( jobResult.Exception != null )
            {
                throw jobResult.Exception;
            }
        }

        internal RockJobResult Execute( PopulateInteractionSessionDataJobSettings settings )
        {
            _errors = new List<string>();
            _exceptions = new List<Exception>();

            var jobResult = new RockJobResult();

            // STEP 1: Process IP location lookups
            var result = ProcessInteractionSessionForIP( settings );
            if ( result.IsNotNullOrWhiteSpace() )
            {
                jobResult.OutputMessages.Add( result );
            }

            // STEP 2: Update Interaction Counts and Durations for Session
            result = ProcessInteractionCountAndDuration( settings );
            if ( result.IsNotNullOrWhiteSpace() )
            {
                jobResult.OutputMessages.Add( result );
            }

            // Print error messages
            foreach ( var error in _errors )
            {
                jobResult.OutputMessages.Add( $"<i class='fa fa-circle text-danger'></i> {error}" );
            }

            // Log caught exceptions
            if ( _exceptions.Any() )
            {
                var exceptionList = new AggregateException( "One or more exceptions occurred in Process Interaction Session Data.", _exceptions );
                jobResult.Exception = new RockJobWarningException( "Process Interaction Session Data completed with warnings", exceptionList );
            }

            return jobResult;
        }

        /// <summary>
        /// <para>Processes interaction sessions that are yet to have their interaction count and duration properties updated.</para>
        /// <para> If the job was run within the last 24 hours then all interaction sessions for interactions that happened within the last 24 hours are processed,</para>
        /// <para>if not then sessions that happened an hour before the last time the job was run are processed.</para>
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>System.String.</returns>
        private string ProcessInteractionCountAndDuration( PopulateInteractionSessionDataJobSettings settings )
        {
            // This portion of the job looks for interaction sessions that need to have their interaction count and
            // duration properties updated. This de-normalization occurs to increase performance of the analytics.
            // We'll be looking for sessions that have not been processed yet OR have had interactions written
            // since their last update.

            var stopwatch = Stopwatch.StartNew();

            var batchSize = 500;
            var totalRecordsProcessed = 0;
            var interactionCalculationDateTime = RockDateTime.Now;
            var oneDayAgo = RockDateTime.Now.AddDays( -1 );

            // We'll limit our process to sessions that started in the last 24 hours, or ones that haven't been calculated.
            var startDate = oneDayAgo;

            var lastSuccessfulJobRunDateTime = settings.LastSuccessfulJobRunDateTime;
            DateTime? cutoffStartDateTime;
            if ( lastSuccessfulJobRunDateTime.HasValue )
            {
                if ( lastSuccessfulJobRunDateTime.Value > oneDayAgo )
                {
                    // Have the cutoffStartDateTime be at least 1 day ago
                    cutoffStartDateTime = oneDayAgo;
                }
                else
                {
                    // If it has been more than a day since the job ran, have cutoffStartDateTime be an hour before that
                    cutoffStartDateTime = lastSuccessfulJobRunDateTime.Value.AddHours( -1 );
                }
            }
            else
            {
                cutoffStartDateTime = null;
            }

            while ( true )
            {
                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.CommandTimeout = _commandTimeout;

                    var interactionSessions = GetInteractionSessionsForActivityUpdate( rockContext, startDate, cutoffStartDateTime, batchSize );

                    this.UpdateLastStatusMessage( $"Processing Interaction Count And Session Duration : {batchSize} sessions are being processed currently. Total {totalRecordsProcessed} Interaction Session{( totalRecordsProcessed < 2 ? "" : "s" )} are processed till now." );

                    if ( interactionSessions.Any() )
                    {
                        var minSessionId = interactionSessions.Min( s => s.Id );
                        var maxSessionId = interactionSessions.Max( s => s.Id );

                        LogDebugInfo( "Update Interaction Metadata", $"BatchSize={ interactionSessions.Count }, SessionId={ minSessionId } --> { maxSessionId }." );

                        foreach ( var interactionSession in interactionSessions )
                        {
                            // If a Guid.Empty value exists in the database, subsequent attempts to update the record
                            // will cause a validation error. In this case, flag the invalid record so the source can be tracked and corrected.
                            if ( interactionSession.Guid == Guid.Empty )
                            {
                                _errors.Add( $"Interaction Session with invalid Guid ignored. [Id={interactionSession.Id}]" );
                                continue;
                            }

                            // Special Query to only get what we need to know.
                            // This could cause a lot of database calls, but it is consistently just a few milliseconds.
                            // This seems to increase overall performance vs Eager loading all the interactions of the session
                            var interactionStats = new InteractionSessionService( rockContext ).Queryable().Where( a => a.Id == interactionSession.Id ).Select( s => new
                            {
                                Count = s.Interactions.Count(),
                                MaxDateTime = s.Interactions.Max( i => ( DateTime? ) i.InteractionDateTime ),
                                MinDateTime = s.Interactions.Min( i => ( DateTime? ) i.InteractionDateTime ),
                                InteractionChannelId = s.Interactions.FirstOrDefault().InteractionComponent.InteractionChannelId
                            } ).FirstOrDefault();

                            interactionSession.InteractionCount = interactionStats?.Count ?? 0;
                            interactionSession.InteractionChannelId = interactionStats?.InteractionChannelId;

                            // Calculate the session duration depending on the number of interactions. Note that we won't know the
                            // duration of time spend on the last page so we'll assume 60 seconds as the average amount of time
                            // spent on a page is 52 seconds https://www.klipfolio.com/metrics/marketing/average-time-on-page
                            switch ( interactionSession.InteractionCount )
                            {
                                case int x when x > 1:
                                    {
                                        // When there is 2 or more interactions calculate the time between the interaction dates
                                        interactionSession.DurationSeconds = ( int ) ( ( interactionStats.MaxDateTime - interactionStats.MinDateTime )?.TotalSeconds ?? 0 ) + 60;
                                        break;
                                    }
                                case 1:
                                    {
                                        // Only one page view so we'll assume 60 seconds
                                        interactionSession.DurationSeconds = 60;
                                        break;
                                    }
                                default:
                                    {
                                        // Not sure how a session was created without an interaction but we give that a zero
                                        interactionSession.DurationSeconds = 0;
                                        break;
                                    }
                            }

                            interactionSession.DurationLastCalculatedDateTime = interactionCalculationDateTime;

                            totalRecordsProcessed += 1;
                        }

                        rockContext.SaveChanges( disablePrePostProcessing: true );
                    }

                    // Stop looping if we're out of sessions to process
                    if ( interactionSessions.Count() < batchSize )
                    {
                        break;
                    }
                }
            }

            settings.LastSuccessfulJobRunDateTime = interactionCalculationDateTime;
            settings.LastNullDurationLastCalculatedDateTimeUpdateDateTime = interactionCalculationDateTime;

            stopwatch.Stop();
            LogDebugInfo( "Process Interaction Count And Duration", $"Completed in {stopwatch.Elapsed.TotalSeconds:0.00}s." );

            Rock.Web.SystemSettings.SetValue( SystemSetting.POPULATE_INTERACTION_SESSION_DATA_JOB_SETTINGS, settings.ToJson() );

            return $"<i class='fa fa-circle text-success'></i> Updated Interaction Count And Session Duration for {totalRecordsProcessed} {"interaction session".PluralizeIf( totalRecordsProcessed != 1 )} in {Math.Round( stopwatch.Elapsed.TotalSeconds, 2 )} secs.";
        }

        /// <summary>
        /// Get recent sessions that have had new interactions since it was last processed and sessions that have not been processed yet.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="cutoffStartDateTime">The cutoff start date time.</param>
        /// <param name="batchSize">Size of the batch.</param>
        /// <returns></returns>
        private List<InteractionSession> GetInteractionSessionsForActivityUpdate( RockContext rockContext, DateTime startDate, DateTime? cutoffStartDateTime, int batchSize )
        {
            // Get recent sessions that have had new interactions since it was last processed
            // and then also look for sessions that have not been processed yet (on first run this could be a lot)

            /* 2022-08-30 MDP

              If the Job has successfully run before, only look for sessions that have had interactions since the job successfully ran (or at least one day ago if it was run more recently).
              This will help avoid an expensive full table scan on the InteractionSession table and/or Interactions Table.

            */

            var outOfDateInteractionSessionIdsQuery = new InteractionService( rockContext ).Queryable()
                .Where( a => a.InteractionSessionId != null
                    && a.InteractionSession.DurationLastCalculatedDateTime.HasValue
                    && a.InteractionDateTime > startDate
                    && a.InteractionDateTime > a.InteractionSession.DurationLastCalculatedDateTime ).Select( a => a.InteractionSessionId.Value ).Distinct();

            List<InteractionSession> interactionSessionsWithOutOfDate;

            /* 2022-08-31 MP

             Add an OPTION (RECOMPILE) to these queries to help keep consistent performance. We
             are thinking that since the Interaction Table is heavily modified, this gives
             SQL Server a chance to make sure the query plan is still optimal. This
             seems to fix situations where the query would sometimes take several minutes, instead of
             just few seconds or less.

             */
            using ( new QueryHintScope( rockContext, QueryHintType.RECOMPILE ) )
            {
                interactionSessionsWithOutOfDate = new InteractionSessionService( rockContext )
                    .Queryable()
                    .Where( s => outOfDateInteractionSessionIdsQuery.Contains( s.Id ) )
                    .Take( batchSize ).ToList();
            }

            var remainingBatchSize = batchSize - interactionSessionsWithOutOfDate.Count();

            IQueryable<InteractionSession> interactionSessionsWithNullDurationLastCalculatedDateTimeQuery;

            if ( cutoffStartDateTime.HasValue )
            {
                var recentInteractionSessionIdsQuery = new InteractionService( rockContext ).Queryable().Where( a => a.CreatedDateTime > cutoffStartDateTime && a.InteractionSessionId.HasValue ).Select( a => a.InteractionSessionId.Value ).Distinct();
                interactionSessionsWithNullDurationLastCalculatedDateTimeQuery = new InteractionSessionService( rockContext )
                    .Queryable().Where( s => s.DurationLastCalculatedDateTime == null && recentInteractionSessionIdsQuery.Contains( s.Id ) );
            }
            else
            {
                interactionSessionsWithNullDurationLastCalculatedDateTimeQuery = new InteractionSessionService( rockContext )
                    .Queryable().Where( s => s.DurationLastCalculatedDateTime == null && s.Interactions.Any() );
            }

            List<InteractionSession> interactionSessionsWithNullDurationLastCalculatedDateTime;

            using ( new QueryHintScope( rockContext, QueryHintType.RECOMPILE ) )
            {
                interactionSessionsWithNullDurationLastCalculatedDateTime = interactionSessionsWithNullDurationLastCalculatedDateTimeQuery
                .OrderByDescending( a => a.Id )
                .Take( remainingBatchSize ).ToList();
            }

            var interactionSessions = interactionSessionsWithNullDurationLastCalculatedDateTime.Union( interactionSessionsWithOutOfDate ).ToList();

            return interactionSessions;
        }

        /// <summary>
        /// Processes the looking up of Interaction Sessions that do not have geo information.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        internal string ProcessInteractionSessionForIP( PopulateInteractionSessionDataJobSettings settings )
        {
            // This portion of the job looks for interaction sessions tied to interaction channels whose websites
            // have geo tracking enabled. The logic is broken into two parts:
            //      1. First we look for sessions that need location information. This is stored in a collection of
            //         IP addresses with their matching session ids
            //      2. Next we send that collection to the IP lookup provider. This is done as a group to support
            //         bulk lookups.
            //
            // The job setting 'Number of Records to Process' tells us the max number of IP lookups to do at the provider
            // per job run. This will represent more than that number of sessions as many sessions have the
            // same IP address. In testing, after running a few times, processing 5,000 address can update over 100,000 sessions.

            if ( settings.IpAddressLookupIsDisabled )
            {
                return $"<i class='fa fa-circle text-warn'></i> IP Address lookup is disabled.";
            }

            var stopwatch = Stopwatch.StartNew();

            var warningMsg = string.Empty;

            // Read settings from job
            var numberOfRecordsToProcess = settings.MaxRecordsToProcessPerRun ?? 50000;

            var ipAddressComponentGuid = GetAttributeValue( AttributeKey.IPAddressGeoCodingComponent );
            var lookbackMaximumInDays = GetAttributeValue( AttributeKey.LookbackMaximumInDays ).ToStringOrDefault( "30" ).AsInteger();

            var lookBackStartDate = RockDateTime.Now.Date.AddDays( -lookbackMaximumInDays );

            // First ensure we have a lookup provider to use
            var provider = GetLookupComponent( ipAddressComponentGuid );

            if ( provider == null )
            {
                return $"<i class='fa fa-circle text-info'></i> No IP Address lookup service active.";
            }

            // If the lookup provider is not ready to process, exit early to avoid the relatively expensive operation of
            // querying the database for unresolved interaction sessions.
            var canProcess = provider.VerifyCanProcess( out var statusMessage );
            if ( !canProcess )
            {
                return $"<i class='fa fa-circle text-warn'></i> IP Address lookup service is unavailable."
                    + ( statusMessage.IsNullOrWhiteSpace() ? "" : $" ({statusMessage})" );
            }

            // This collection will be used to store IP address that need to be processed using the lookup provider
            var ipAddressSessionKeyValue = new Dictionary<string, List<int>>();

            // This collection will be used to store IP address that we have already looked up in the database to prevent unnessary future reads
            var previouslyFoundIpAddresses = new Dictionary<string, int>();

            // Counters for reporting back metrics
            var recordsUpdated = 0;
            var totalRecordsProcessed = 0;

            // Number of sessions to process at a time. Keep this small to stop the RockContext from gummed up
            var sessionBatchSize = 500;

            // Need to keep track of what sessions have been processed in the loop as we're not updating most sessions in the loop. That will be
            // done in the process step.
            var minSessionId = 0;

            while ( true )
            {
                try
                {
                    var rockContext = new RockContext();
                    rockContext.Database.CommandTimeout = _commandTimeout;
                    var currentDateTime = RockDateTime.Now;

                    // Determine how many sessions to take at a time with a max limit per batch. This helps us to not process more than we're allowed to in one run.
                    var remainingLookupsAllowed = numberOfRecordsToProcess - ipAddressSessionKeyValue.Count();
                    var maxRecordsToReturn = remainingLookupsAllowed < sessionBatchSize ? remainingLookupsAllowed : sessionBatchSize;

                    // Get next batch of Interaction Sessions
                    LogDebugInfo( "Retrieve Interaction Sessions", $"StartDate={lookBackStartDate}, MaxRecords={maxRecordsToReturn}" );

                    var interactionSessions = GetInteractionSessionsForIpResolution( rockContext, maxRecordsToReturn, lookBackStartDate, minSessionId );

                    // If there are no more sessions to process then exit the loop
                    if ( interactionSessions.Count() == 0 )
                    {
                        break;
                    }

                    // Update the min session id for the next batch run. This ensures we don't process the same batch of sessions over and over in the loop
                    minSessionId = interactionSessions.Max( s => s.Id );

                    // Update the job progress
                    this.UpdateLastStatusMessage( $"Processing Interaction Session for IP : {maxRecordsToReturn} IP's are being processed currently. Total {recordsUpdated} Interaction Session{( recordsUpdated < 2 ? "" : "s" )} are processed till now. " );

                    LogDebugInfo( "Process IP Address Lookups", $"BatchSize={ interactionSessions.Count }, SessionId={ interactionSessions.Min( s => s.Id ) } --> { interactionSessions.Max( s => s.Id ) }" );

                    foreach ( var interactionSession in interactionSessions )
                    {
                        totalRecordsProcessed += 1;

                        // In some rare cases the IP address of the session can have two address (comma separated). This can
                        // happen with CDNs and other web proxies. There is logic in Rock to handle this, but some older sessions
                        // might still have them. We'll clean this up here.
                        if ( interactionSession.IpAddress.Contains( "," ) )
                        {
                            interactionSession.IpAddress = interactionSession.IpAddress.Split( ',' ).FirstOrDefault().Trim();
                        }

                        // Check if we have already lookup this IP address up in the database
                        if ( previouslyFoundIpAddresses.ContainsKey( interactionSession.IpAddress ) )
                        {
                            interactionSession.InteractionSessionLocationId = previouslyFoundIpAddresses[interactionSession.IpAddress];
                            recordsUpdated += 1;
                            continue;
                        }

                        // Next check the list of IP addresses that we have already attempted to lookup and did not find
                        if ( ipAddressSessionKeyValue.ContainsKey( interactionSession.IpAddress ) )
                        {
                            ipAddressSessionKeyValue[interactionSession.IpAddress].Add( interactionSession.Id );
                            continue;
                        }

                        // Finally break down and look in the database for it. We'll only consider ones lookuped in the last
                        // 90 days in case the IP address information has been updated in the providers database.
                        var lookupExpireDate = RockDateTime.Now.AddDays( -90 );
                        var interactionSessionLocationId = new InteractionSessionLocationService( rockContext ).Queryable()
                            .Where( m => m.IpAddress == interactionSession.IpAddress )
                            .Where( m => m.LookupDateTime >= lookupExpireDate )
                            .Select( a => a.Id )
                            .FirstOrDefault();

                        // If we found a match link it up
                        if ( interactionSessionLocationId != default( int ) )
                        {
                            interactionSession.InteractionSessionLocationId = interactionSessionLocationId;
                            previouslyFoundIpAddresses.Add( interactionSession.IpAddress, interactionSessionLocationId );
                            recordsUpdated += 1;
                            continue;
                        }

                        // We didn't find it, add it to the list of addresses to lookup
                        ipAddressSessionKeyValue.Add( interactionSession.IpAddress, new List<int> { interactionSession.Id } );
                    }

                    rockContext.SaveChanges();

                    // If we processed the max number we're allowed per run exit
                    if ( ipAddressSessionKeyValue.Count() >= numberOfRecordsToProcess )
                    {
                        break;
                    }
                }
                catch ( Exception ex )
                {
                    // Capture and log the exception because we're not going to fail this job
                    var message = $"An error occurred while trying to process interaction sessions. Error: {ex.Message}";
                    _errors.Add( string.Format( @"ProcessInteractionSessionForIP method after {0} records.", totalRecordsProcessed ) );

                    var exceptionWrapper = new Exception( message, ex );
                    _exceptions.Add( exceptionWrapper );
                    ExceptionLogService.LogException( exceptionWrapper, null );

                    break;
                }
            }

            // We now have our list of IPs that need to be processed
            if ( ipAddressSessionKeyValue.Count > 0 )
            {
                this.UpdateLastStatusMessage( $"Processing Interaction Session : Total {recordsUpdated} Interaction Session{( recordsUpdated < 2 ? "" : "s" )} are processed till now. {ipAddressSessionKeyValue.Count} sent to LookupComponent to process." );
                recordsUpdated = ProcessIPOnLookupComponent( provider, ipAddressSessionKeyValue );
            }

            stopwatch.Stop();

            // Log our metrics
            LogDebugInfo( "Process Interaction Session", $"Completed in {stopwatch.Elapsed.TotalSeconds:0.00}s." );

            // Suppress sending an update if an error occurred. That will be show via the exception processing
            if ( recordsUpdated == -1 )
            {
                return string.Empty;
            }

            return $"<i class='fa fa-circle text-success'></i> Updated IP location on {recordsUpdated} {"interaction session".PluralizeIf( recordsUpdated != 1 )} with {totalRecordsProcessed} total interaction sessions using {ipAddressSessionKeyValue.Count} lookup credits (others were found in the database) in {stopwatch.Elapsed.TotalSeconds:0.00}s. {warningMsg}";
        }

        /// <summary>
        /// Gets the IP Lookup component.
        /// </summary>
        /// <param name="configuredProvider">The configured provider.</param>
        /// <returns></returns>
        internal virtual IpAddressLookupComponent GetLookupComponent( string configuredProvider )
        {
            // Get the configured component from the job settings
            if ( configuredProvider.AsGuidOrNull().HasValue )
            {
                return IpAddressLookupContainer.GetComponent( configuredProvider );
            }

            // Otherwise use an active provider
            return IpAddressLookupContainer.Instance.Components.Select( a => a.Value.Value ).Where( x => x.IsActive ).FirstOrDefault();
        }

        /// <summary>
        /// Gets the interaction sessions to process 
        /// </summary>
        private List<InteractionSession> GetInteractionSessionsForIpResolution( RockContext rockContext, int maxRecordsToReturn, DateTime lookBackStartDate, int minId )
        {
            // Create filter on the Interaction Components tied to a site with geo tracking enabled
            var interactionComponentQry = new InteractionComponentService( rockContext )
                .QueryByPagesOnSitesWithGeoTracking()
                .Select( a => a.Id );

            // Create interaction query so we can find sessions tied to sites with geo tracking enabled
            var interactionQry = new InteractionService( rockContext )
                .Queryable()
                .Where( i => i.InteractionDateTime >= lookBackStartDate )
                .Where( i => interactionComponentQry.Contains( i.InteractionComponentId ) )
                .Select( i => i.Id );

            // Create InteractionSession query
            using ( new QueryHintScope( rockContext, QueryHintType.RECOMPILE ) )
            {
                var sessions = new InteractionSessionService( rockContext )
                .Queryable()
                .Where( s =>
                    !s.InteractionSessionLocationId.HasValue
                    && s.IpAddress != null
                    && s.IpAddress != string.Empty && s.IpAddress != "::1" && !s.IpAddress.StartsWith( "192.168" )
                        && !s.IpAddress.StartsWith( "10." ) && !s.IpAddress.StartsWith( "169.254" ) && s.IpAddress != "127.0.0.1"
                    && s.Interactions.Any( i => interactionQry.Contains( i.Id ) )
                    && s.Id > minId )
                .OrderBy( s => s.Id )
                .Take( maxRecordsToReturn )
                .TagWith( this.GetType().FullName + $" ({this.GetJobId()})" )
                .ToList();

                return sessions;
            }
        }

        /// <summary>
        /// Processes the IP addresses that need to be looked up by the provider.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="ipAddressSessionKeyValue">The ip address session key value.</param>
        /// <returns>Count of the number of sessions updated</returns>
        private int ProcessIPOnLookupComponent( IpAddressLookupComponent provider, Dictionary<string, List<int>> ipAddressSessionKeyValue )
        {
            var errorMessage = string.Empty;
            try
            {
                // Get the IP locations for the selected IP addresses
                var ipAddressList = new List<string>( ipAddressSessionKeyValue.Keys );

                LogDebugInfo( "Process IP Lookups", $"BatchSize={ ipAddressList.Count }, Data={ ipAddressList.Take( 20 ).JoinStrings( "," ) }..." );
                var lookupResults = provider.BulkLookup( ipAddressList, out errorMessage );

                // Create Interaction Session Locations and update Sessions
                IpLocationUtilities.UpdateInteractionSessionLocations( lookupResults, ipAddressSessionKeyValue );

                if ( errorMessage.IsNotNullOrWhiteSpace() )
                {
                    _errors.Add( string.Format( @"IP Lookup Component failed with batch of {0} IP with error message {1}.", ipAddressSessionKeyValue.Count, errorMessage ) );
                }

                // Return the number sessions that we're updated
                return ipAddressSessionKeyValue.Values.Sum( v => v.Count() );
            }
            catch ( Exception ex )
            {
                // Capture and log the exception because we're not going to fail this job
                var message = $"An error occurred while trying to lookup IP Addresses from Lookup Component so it was skipped. Error: {ex.Message}";
                _errors.Add( string.Format( @"IP Lookup Component failed with batch of {0} IP.", ipAddressSessionKeyValue.Count ) );

                var exceptionWrapper = new Exception( message, ex );
                _exceptions.Add( exceptionWrapper );
                ExceptionLogService.LogException( exceptionWrapper, null );

                return -1;
            }
        }
        private void LogDebugInfo( string taskName, string message )
        {
            Log( RockLogLevel.Debug, $"({taskName}): {message}" );
        }

        internal class PopulateInteractionSessionDataJobSettings
        {
            public DateTime? LastSuccessfulJobRunDateTime { get; set; }
            public DateTime? LastNullDurationLastCalculatedDateTimeUpdateDateTime { get; set; }
            public int? MaxRecordsToProcessPerRun { get; set; } 

            /// <summary>
            /// A flag indicating if the IP Address Lookup process will attempt to resolve IP addresses
            /// in addition to recalculating interaction session data.
            /// </summary>
            public bool IpAddressLookupIsDisabled { get; set; }
        }

        /// <summary>
        /// The result data from a Rock Job.
        /// </summary>
        internal class RockJobResult
        {
            public List<string> OutputMessages { get; set; } = new List<string>();

            public string GetResultSummaryHtml()
            {
                var results = new StringBuilder();

                foreach ( var message in OutputMessages )
                {
                    if ( message.IsNotNullOrWhiteSpace() )
                    {
                        results.AppendLine( message );
                    }
                }

                return results.ToString();
            }

            /// <summary>
            /// Gets or sets the amount of time taken
            /// </summary>
            /// <value>
            /// The time.
            /// </value>
            public TimeSpan ExecutionTime { get; set; }

            public bool HasException => Exception != null;

            public Exception Exception { get; set; }
        }
    }
}
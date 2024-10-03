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
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;

using Rock.Bus.Message;
using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.SystemKey;
using Rock.Web;

namespace Rock.WebFarm
{
    /// <summary>
    /// Web Farm
    /// Internal Rock Code.  Will not be backwards compatible.
    /// </summary>
    public static class RockWebFarm
    {
        #region Helper Classes

        /// <summary>
        /// Default Values
        /// </summary>
        public static class DefaultValue
        {
            /// <summary>
            /// Is web farm enabled?
            /// </summary>
            public const bool IsWebFarmEnabled = false;

            /// <summary>
            /// The default leadership polling interval lower limit seconds
            /// </summary>
            public const int DefaultLeadershipPollingIntervalLowerLimitSeconds = 3 * 60;

            /// <summary>
            /// The default leadership polling interval upper limit seconds
            /// </summary>
            public const int DefaultLeadershipPollingIntervalUpperLimitSeconds = 5 * 60;

            /// <summary>
            /// The default minimum polling difference seconds
            /// </summary>
            public static int DefaultMinimumPollingDifferenceSeconds = 10;

            /// <summary>
            /// The default polling maximum wait seconds
            /// </summary>
            public static int DefaultPollingMaxWaitSeconds = 10;
        }

        /// <summary>
        /// Event Types
        /// </summary>
        public static class EventType
        {
            /// <summary>
            /// Startup
            /// </summary>
            public const string Startup = "Startup";

            /// <summary>
            /// Shutdown
            /// </summary>
            public const string Shutdown = "Shutdown";

            /// <summary>
            /// Availability change
            /// </summary>
            public const string Availability = "Availability";

            /// <summary>
            /// Error
            /// </summary>
            public const string Error = "Error";

            /// <summary>
            /// Ping
            /// </summary>
            public const string Ping = "Ping";

            /// <summary>
            /// Pong: the response to a ping
            /// </summary>
            public const string Pong = "Pong";
        }

        #endregion Helper Classes

        #region State

        /// <summary>
        /// Do debug logging
        /// </summary>
        private const bool DEBUG = false;

        /// <summary>
        /// The bytes per megabyte
        /// </summary>
        private const int BytesPerMegabyte = 1024 * 1024;

        /// <summary>
        /// The web farm enabled
        /// </summary>
        private static bool _isWebFarmEnabledAndUnlocked = false;

        /// <summary>
        /// The start stage
        /// </summary>
        private static int _startStage = 0;

        /// <summary>
        /// The node identifier
        /// </summary>
        private static int _nodeId = 0;

        /// <summary>
        /// Was this instance pinged?
        /// </summary>
        private static bool _wasPinged = false;

        /// <summary>
        /// The leadership ping key
        /// </summary>
        private static Guid? _leadershipPingKey = null;

        /// <summary>
        /// The polling interval seconds
        /// </summary>
        private static decimal _pollingIntervalSeconds = DefaultValue.DefaultLeadershipPollingIntervalUpperLimitSeconds;

        /// <summary>
        /// The polling interval
        /// </summary>
        private static IntervalAction _pollingInterval;

        /// <summary>
        /// The CPU counter
        /// </summary>
        private static PerformanceCounter _cpuCounter = null;

        /// <summary>
        /// The ram counter
        /// </summary>
        private static PerformanceCounter _ramCounter = null;

        /// <summary>
        /// The process start date time
        /// </summary>
        public static readonly DateTime ProcessStartDateTime = RockDateTime.Now;

        /// <summary>
        /// The total ram megabytes
        /// </summary>
        private static readonly int TotalRamMb = ( int ) ( new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / BytesPerMegabyte );

        /// <summary>
        /// Gets the name of the node.
        /// </summary>
        /// <value>
        /// The name of the node.
        /// </value>
        public static string NodeName
        {
            get
            {
                return RockApp.Current.HostingSettings.NodeName;
            }
        }

        /// <summary>
        /// Gets the process identifier.
        /// </summary>
        /// <value>
        /// The process identifier.
        /// </value>
        public static int ProcessId
        {
            get
            {
                if ( !_processId.HasValue )
                {
                    using ( var thisProcess = Process.GetCurrentProcess() )
                    {
                        _processId = thisProcess.Id;
                    }
                }

                return _processId.Value;
            }
        }

        /// <summary>
        /// The process identifier
        /// </summary>
        private static int? _processId;

        /// <summary>
        /// Gets a value indicating whether this instance is overlapped recycling.
        /// Overlapped recycling means a new process has just started for this node
        /// and this process will soon be shutdown.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is overlapped recycling; otherwise, <c>false</c>.
        /// </value>
        private static bool IsOverlappedRecycling()
        {
            if ( _hasDetectedOverlappedRecycling )
            {
                return true;
            }

            // If another process has started recently, then it will have set the RestartDateTime to a more
            // recent time than my start
            using ( var rockContext = new RockContext() )
            {
                var node = GetNode( rockContext, NodeName );

                if ( node.LastRestartDateTime > ProcessStartDateTime )
                {
                    // SQL server doesn't store the same precision of date as C# holds, so we need to check that
                    // the difference is actually substantial
                    var timespan = node.LastRestartDateTime - ProcessStartDateTime;
                    _hasDetectedOverlappedRecycling = timespan.TotalSeconds > 1;

                    if ( _hasDetectedOverlappedRecycling )
                    {
                        Debug( "Detected that this process may be overlapped for recycling" );

                        // JME 8/26/2021 remove writing this message to the cluster log as it is noise
                        // AddLog( rockContext, WebFarmNodeLog.SeverityLevel.Info, _nodeId, EventType.Availability, "Detected that this process may be overlapped for recycling" );
                    }

                    rockContext.SaveChanges();
                }
            }

            return _hasDetectedOverlappedRecycling;
        }

        /// <summary>
        /// Has detected overlapped recycling?
        /// </summary>
        private static bool _hasDetectedOverlappedRecycling = false;

        #endregion State

        #region Startup and Shutdown

        /// <summary>
        /// Stage 1 Start.
        /// Called before Hot load caches and start-up activities( EF migrations etc )
        /// </summary>
        public static void StartStage1()
        {
            if ( _startStage != 0 )
            {
                LogException( $"Web Farm cannot start stage 1 when at stage {_startStage}" );
                return;
            }

            Debug( "Start Stage 1" );

            using ( var rockContext = new RockContext() )
            {
                // Check that the WebFarmEnable = true.If yes, continue
                if ( !IsEnabled() )
                {
                    return;
                }

                // Check license key in the SystemSetting
                if ( !HasValidKey() )
                {
                    return;
                }

                _isWebFarmEnabledAndUnlocked = true;

                // Initialize the performance counters that will be used when logging metrics
                InitializePerformanceCounters();

                // Load upper and lower polling interval settings
                var lowerLimitSeconds = GetLowerPollingLimitSeconds();
                var upperLimitSeconds = GetUpperPollingLimitSeconds();

                // Find node record in DB using node name, if not found create a new record
                var webFarmNodeService = new WebFarmNodeService( rockContext );
                var webFarmNode = webFarmNodeService.Queryable().FirstOrDefault( wfn => wfn.NodeName == NodeName );
                var isNewNode = webFarmNode == null;

                if ( isNewNode )
                {
                    webFarmNode = new WebFarmNode
                    {
                        NodeName = NodeName
                    };

                    webFarmNodeService.Add( webFarmNode );
                    rockContext.SaveChanges();
                }

                _nodeId = webFarmNode.Id;

                // Determine leadership polling interval. If provided in database( ConfiguredLeadershipPollingIntervalSeconds ) use that
                // otherwise randomly select a number between upper and lower limits
                const int maxGenerationAttempts = 100;
                var generationAttempts = 1;

                _pollingIntervalSeconds =
                    webFarmNode.ConfiguredLeadershipPollingIntervalSeconds ??
                    GeneratePollingIntervalSeconds( lowerLimitSeconds, upperLimitSeconds );

                var isPollingIntervalInUse = IsPollingIntervalInUse( rockContext, NodeName, _pollingIntervalSeconds );

                while ( generationAttempts < maxGenerationAttempts && isPollingIntervalInUse )
                {
                    generationAttempts++;
                    _pollingIntervalSeconds = GeneratePollingIntervalSeconds( lowerLimitSeconds, upperLimitSeconds );
                    isPollingIntervalInUse = IsPollingIntervalInUse( rockContext, NodeName, _pollingIntervalSeconds );
                }

                if ( isPollingIntervalInUse )
                {
                    var errorMessage = $"Web farm node {NodeName} did not successfully pick a polling interval after {maxGenerationAttempts} attempts";
                    AddLog( rockContext, WebFarmNodeLog.SeverityLevel.Warning, webFarmNode.Id, EventType.Error, errorMessage );

                    // Try to use the maximum value
                    _pollingIntervalSeconds = upperLimitSeconds;
                    isPollingIntervalInUse = IsPollingIntervalInUse( rockContext, NodeName, _pollingIntervalSeconds );

                    if ( isPollingIntervalInUse )
                    {
                        LogException( $"{errorMessage} and could not use the maximum polling limit" );
                        return;
                    }
                }

                // Save the polling interval
                // If web.config set to run jobs make IsCurrentJobRunner = true
                // Set StoppedDateTime to null
                // Update LastRestartDateTime to now
                webFarmNode.CurrentLeadershipPollingIntervalSeconds = _pollingIntervalSeconds;
                webFarmNode.IsCurrentJobRunner = IsCurrentJobRunner();
                webFarmNode.StoppedDateTime = null;
                webFarmNode.LastRestartDateTime = ProcessStartDateTime;
                webFarmNode.LastSeenDateTime = ProcessStartDateTime;
                webFarmNode.IsActive = false;

                // Write to ClusterNodeLog -Startup Message
                AddLog( rockContext, WebFarmNodeLog.SeverityLevel.Info, webFarmNode.Id, EventType.Startup, $"Process ID: {ProcessId}" );

                rockContext.SaveChanges();
            }

            _startStage = 1;
            Debug( "Done with Stage 1" );
        }

        /// <summary>
        /// Start Stage 2.
        /// Called after Hot load caches and start-up activities( EF migrations etc )
        /// </summary>
        public static void StartStage2()
        {
            if ( !_isWebFarmEnabledAndUnlocked )
            {
                return;
            }

            if ( _startStage != 1 )
            {
                LogException( $"Web Farm cannot start stage 2 when at stage {_startStage}" );
                return;
            }

            Debug( "Start Stage 2" );

            using ( var rockContext = new RockContext() )
            {
                // Mark IsActive true
                // Update LastSeenDateTime = now
                var webFarmNode = GetNode( rockContext, NodeName );
                webFarmNode.IsActive = true;
                webFarmNode.LastSeenDateTime = RockDateTime.Now;
                rockContext.SaveChanges();
            }

            // Start the polling cycle
            _pollingInterval = IntervalAction.Start( DoIntervalProcessingAsync, TimeSpan.FromSeconds( decimal.ToDouble( _pollingIntervalSeconds ) ) );

            // Announce startup to EventBus
            PublishEvent( EventType.Startup );
            _startStage = 2;

            Debug( "Done with Stage 2" );
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public static void Shutdown( ApplicationShutdownReason shutdownReason )
        {
            var shutdownReasonText = shutdownReason.ConvertToString();

            if ( !_isWebFarmEnabledAndUnlocked )
            {
                return;
            }

            if ( _startStage != 2 )
            {
                LogException( $"Web Farm cannot shutdown properly when at stage {_startStage}" );
                return;
            }

            Debug( $"Shutdown ({shutdownReasonText})" );

            // Stop the polling interval
            _pollingInterval.Stop();

            // Announce to other nodes, unless I am being overlapped recycled.
            // If I am being overlapped recycled, then my twin is taking over
            // and I am not really shutting down.
            var isOverlappedRecycling = IsOverlappedRecycling();

            if ( !isOverlappedRecycling )
            {
                PublishEvent( EventType.Shutdown, payload: shutdownReasonText );
            }

            using ( var rockContext = new RockContext() )
            {
                if ( !isOverlappedRecycling )
                {
                    // Update IsActive = false, StoppedDateTime = now
                    // If IsCurrentJobRunning = true set this to false
                    var webFarmNode = GetNode( rockContext, NodeName );
                    webFarmNode.IsActive = false;
                    webFarmNode.StoppedDateTime = RockDateTime.Now;
                    webFarmNode.IsCurrentJobRunner = false;
                }

                // Add a note if recycling
                var recyclingText = isOverlappedRecycling ? "Recycling - " : string.Empty;

                // Write to ClusterNodeLog shutdown message
                AddLog( rockContext, WebFarmNodeLog.SeverityLevel.Info, _nodeId, EventType.Shutdown, $"{recyclingText}{shutdownReasonText}" );
                rockContext.SaveChanges();
            }

            _startStage = 0;
        }

        #endregion Startup and Shutdown

        #region Event Handlers

        /// <summary>
        /// Called when someone requests a Rock restart.
        /// </summary>
        /// <param name="currentPerson">The current person.</param>
        public static void OnRestartRequested( Person currentPerson )
        {
            if ( !_isWebFarmEnabledAndUnlocked )
            {
                return;
            }

            var personName = currentPerson == null ?
                "Unknown" :
                $"{currentPerson.FullName} (Person Id: {currentPerson.Id})";
            var payload = $"{personName} requested Rock restart";

            using ( var rockContext = new RockContext() )
            {
                AddLog( rockContext, WebFarmNodeLog.SeverityLevel.Info, _nodeId, EventType.Shutdown, payload );
                rockContext.SaveChanges();
            }

            PublishEvent( EventType.Shutdown, payload: payload );
        }

        /// <summary>
        /// Called when [ping].
        /// </summary>
        /// <param name="senderNodeName">Name of the node that pinged.</param>
        /// <param name="pingPongKey">The ping pong key.</param>
        internal static void OnReceivedPing( string senderNodeName, Guid? pingPongKey )
        {
            if ( !_isWebFarmEnabledAndUnlocked )
            {
                return;
            }

            if ( senderNodeName == NodeName )
            {
                // Don't talk to myself
                return;
            }

            if ( !pingPongKey.HasValue )
            {
                // There is no valid ping key
                return;
            }

            Debug( $"Got a Ping from {senderNodeName}" );
            _wasPinged = true;

            // Reply to the leader (sender of the ping)
            PublishEvent( EventType.Pong, senderNodeName, pingPongKey.Value.ToString() );
        }

        /// <summary>
        /// Called when [pong].
        /// </summary>
        /// <param name="senderNodeName">Name of the node that sent the pong.</param>
        /// <param name="recipientNodeName">Name of the recipient node.</param>
        /// <param name="pingPongKey">The ping pong key.</param>
        internal static void OnReceivedPong( string senderNodeName, string recipientNodeName, Guid? pingPongKey )
        {
            if ( !_isWebFarmEnabledAndUnlocked )
            {
                return;
            }

            if ( senderNodeName == NodeName )
            {
                // Don't talk to myself
                return;
            }

            if ( !recipientNodeName.IsNullOrWhiteSpace() && recipientNodeName != NodeName )
            {
                // This message is not for me
                return;
            }

            if ( !pingPongKey.HasValue || pingPongKey.Value != _leadershipPingKey )
            {
                // This pong key doesn't match the key that I sent out as a ping
                return;
            }

            Debug( $"Got a Pong from {senderNodeName}" );

            using ( var rockContext = new RockContext() )
            {
                var node = GetNode( rockContext, senderNodeName );

                // Write to log if the server was thought to be inactive but responded
                if ( !node.IsActive )
                {
                    AddLog( rockContext, WebFarmNodeLog.SeverityLevel.Info, node.Id, EventType.Availability, $"{node.NodeName} was marked inactive but responded to a ping" );
                }

                node.StoppedDateTime = null;
                node.LastSeenDateTime = RockDateTime.Now;
                node.IsActive = true;
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Does the interval processing asynchronous.
        /// </summary>
        internal static async Task DoIntervalProcessingAsync()
        {
            if ( !IsEnabled() )
            {
                return;
            }

            if ( IsOverlappedRecycling() )
            {
                // I am being recycled, so let the twin process handle this.
                return;
            }

            await DoLeadershipPollAsync();

            using ( var rockContext = new RockContext() )
            {
                AddMetrics( rockContext );
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Does the leadership poll.
        /// </summary>
        internal static async Task DoLeadershipPollAsync()
        {
            // If another node pinged this node, then that node is the leader, not this one
            if ( _wasPinged )
            {
                Debug( "My time to poll. I was pinged, so I'm not the leader" );
                _wasPinged = false;
                return;
            }

            Debug( "My time to poll. I was not pinged, so I'm starting leadership duties" );

            // Ping other nodes with a unique key for this ping-pong round
            var pollingTime = RockDateTime.Now;
            _leadershipPingKey = Guid.NewGuid();
            PublishEvent( EventType.Ping, payload: _leadershipPingKey.Value.ToString() );

            // Assert this node's leadership in the database
            using ( var rockContext = new RockContext() )
            {
                var webFarmNodeService = new WebFarmNodeService( rockContext );
                var nodes = webFarmNodeService.Queryable().ToList();
                var thisNode = nodes.FirstOrDefault( wfn => wfn.NodeName == NodeName );
                var otherNodes = nodes.Where( wfn => wfn.NodeName != NodeName );

                if ( !thisNode.IsLeader )
                {
                    AddLog( rockContext, WebFarmNodeLog.SeverityLevel.Info, _nodeId, EventType.Availability, $"{NodeName} assumed leadership" );
                }

                thisNode.IsLeader = true;
                thisNode.IsActive = true;
                thisNode.LastSeenDateTime = pollingTime;
                thisNode.StoppedDateTime = null;

                foreach ( var otherNode in otherNodes )
                {
                    otherNode.IsLeader = false;
                }

                rockContext.SaveChanges();
            }

            // Get polling wait time
            var pollingWaitTimeSeconds = GetMaxPollingWaitSeconds();

            // Wait a maximum of 10 seconds for responses
            await Task.Delay( TimeSpan.FromSeconds( pollingWaitTimeSeconds ) ).ContinueWith( t =>
            {
                // Clear the ping pong key because responses are now late and no longer accepted
                _leadershipPingKey = null;

                Debug( "Checking for unresponsive nodes" );

                using ( var rockContext = new RockContext() )
                {
                    var webFarmNodeService = new WebFarmNodeService( rockContext );
                    var unresponsiveNodes = webFarmNodeService.Queryable()
                        .Where( wfn =>
                            wfn.LastSeenDateTime < pollingTime &&
                            wfn.IsActive &&
                            wfn.NodeName != NodeName )
                        .ToList();

                    Debug( $"I found {unresponsiveNodes.Count} unresponsive nodes" );

                    foreach ( var node in unresponsiveNodes )
                    {
                        // Write to log if the server was thought to be active but did not respond
                        AddLog( rockContext, WebFarmNodeLog.SeverityLevel.Critical, node.Id, EventType.Availability, $"{node.NodeName} was marked active but did not respond to a ping" );
                        node.IsActive = false;
                    }

                    rockContext.SaveChanges();
                }
            } );
        }

        /// <summary>
        /// Called when [received startup].
        /// </summary>
        /// <param name="senderNodeName">Name of the sender node.</param>
        internal static void OnReceivedStartup( string senderNodeName )
        {
            Debug( $"I heard that {senderNodeName} started" );
        }

        /// <summary>
        /// Called when [received shutdown].
        /// </summary>
        /// <param name="senderNodeName">Name of the sender node.</param>
        /// <param name="payload">The payload.</param>
        internal static void OnReceivedShutdown( string senderNodeName, string payload )
        {
            Debug( $"I heard that {senderNodeName} shutdown ({payload})" );
        }

        /// <summary>
        /// Called when [received warning].
        /// </summary>
        /// <param name="senderNodeName">Name of the sender node.</param>
        /// <param name="payload">The payload.</param>
        internal static void OnReceivedAvailabilityChange( string senderNodeName, string payload )
        {
            Debug( $"I heard that {senderNodeName} availability changed: '{payload}'" );
        }

        /// <summary>
        /// Called when [received error].
        /// </summary>
        /// <param name="senderNodeName">Name of the sender node.</param>
        /// <param name="payload">The payload.</param>
        internal static void OnReceivedError( string senderNodeName, string payload )
        {
            Debug( $"I heard that {senderNodeName} had an error: '{payload}'" );
        }

        #endregion Event Handlers

        #region Helper Methods

        /// <summary>
        /// Initializes the performance counters.
        /// </summary>
        private static void InitializePerformanceCounters()
        {
            try
            {
                _cpuCounter = new PerformanceCounter( "Processor", "% Processor Time", "_Total" );
                _ramCounter = new PerformanceCounter( "Memory", "Available MBytes" );

                // Start the performance counters
                _cpuCounter.NextValue();
                _ramCounter.NextValue();
            }
            catch
            {
                // This may fail if the process doesn't have appropriate access to the stats needed
                _cpuCounter = null;
                _ramCounter = null;
            }
        }

        /// <summary>
        /// Adds the metrics.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private static void AddMetrics( RockContext rockContext )
        {
            var webFarmNodeMetricService = new WebFarmNodeMetricService( rockContext );

            if ( _cpuCounter != null )
            {
                var cpuPercent = Convert.ToDecimal( _cpuCounter.NextValue() );

                webFarmNodeMetricService.Add(
                    new WebFarmNodeMetric
                    {
                        WebFarmNodeId = _nodeId,
                        MetricType = WebFarmNodeMetric.TypeOfMetric.CpuUsagePercent,
                        MetricValue = cpuPercent
                    }
                );

                Debug( $"Added metric: CPU {cpuPercent:N0}%" );
            }

            if ( _ramCounter != null )
            {
                var ramAvailable = Convert.ToDecimal( _ramCounter.NextValue() );
                var ramUsage = TotalRamMb - ramAvailable;

                webFarmNodeMetricService.Add(
                    new WebFarmNodeMetric
                    {
                        WebFarmNodeId = _nodeId,
                        MetricType = WebFarmNodeMetric.TypeOfMetric.MemoryUsageMegabytes,
                        MetricValue = ramUsage
                    }
                );

                Debug( $"Added metric: RAM {ramUsage:N0}MB" );
            }
        }

        /// <summary>
        /// Adds the log.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="subjectNodeId">The subject node identifier.</param>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="text">The text.</param>
        private static void AddLog( RockContext rockContext, WebFarmNodeLog.SeverityLevel severity, int subjectNodeId, string eventType, string text = "" )
        {
            var webFarmNodeLogService = new WebFarmNodeLogService( rockContext );

            webFarmNodeLogService.Add( new WebFarmNodeLog
            {
                Severity = severity,
                WriterWebFarmNodeId = _nodeId,
                WebFarmNodeId = subjectNodeId,
                Message = $"(Process ID: {ProcessId}) {text}",
                EventType = eventType
            } );

            Debug( $"Logged {severity} {eventType} {text}" );
        }

        /// <summary>
        /// Determines whether this Rock instance has a valid web farm key.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [has valid key]; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasValidKey()
        {
            /*
             * Circumventing the code below is considered an integrity violation and harmful to the
             * Rock Community. It also breaks the Rock "License" (see top of this file). This Web Farm
             * feature is designed for the largest churches. Using this feature, as a very large
             * church without supporting the Rock Community, costs smaller churches with less
             * resources. Please contact Spark to get a key and support the vision of accessibility
             * for smaller churches.
             *
             * Core Team: See the Web Farm engineering document for more information on keys.
             */

            var webFarmKeyString = SystemSettings.GetValue( SystemSetting.WEBFARM_KEY );
            var webFarmKeyGuid = webFarmKeyString.AsGuidOrNull();

            if ( !webFarmKeyGuid.HasValue )
            {
                return false;
            }

            var keyBytes = webFarmKeyGuid.Value.ToByteArray();
            const int doorLock = 0b_0000_0110_1100_0010_0000_1010_1001_0000;
            var keyInt = BitConverter.ToInt32( keyBytes, 0 );

            var doesKeyFitLock = ( ( keyInt & doorLock ) == doorLock ) && ( ( keyInt | doorLock ) == keyInt );
            return doesKeyFitLock;
        }

        /// <summary>
        /// Determines whether this instance is running.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsRunning()
        {
            return _startStage == 2;
        }

        /// <summary>
        /// Determines whether this instance is enabled.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEnabled()
        {
            return
                SystemSettings.GetValue( SystemSetting.WEBFARM_IS_ENABLED ).AsBooleanOrNull() ??
                DefaultValue.IsWebFarmEnabled;
        }

        /// <summary>
        /// Sets the is enabled.
        /// </summary>
        /// <param name="isEnabled">if set to <c>true</c> [is enabled].</param>
        /// <returns></returns>
        public static void SetIsEnabled( bool isEnabled )
        {
            if ( isEnabled == IsEnabled() )
            {
                return;
            }

            SystemSettings.SetValue( SystemSetting.WEBFARM_IS_ENABLED, isEnabled.ToString() );

            if ( IsRunning() )
            {
                var text = isEnabled ? "enabled" : "disabled";

                using ( var rockContext = new RockContext() )
                {
                    AddLog( rockContext, WebFarmNodeLog.SeverityLevel.Info, _nodeId, EventType.Availability, $"The farm has been {text}" );
                    rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Gets the lower polling limit.
        /// </summary>
        /// <returns></returns>
        public static int GetLowerPollingLimitSeconds()
        {
            var lowerLimitSeconds =
                    SystemSettings.GetValue( SystemSetting.WEBFARM_LEADERSHIP_POLLING_INTERVAL_LOWER_LIMIT_SECONDS ).AsIntegerOrNull() ??
                    DefaultValue.DefaultLeadershipPollingIntervalLowerLimitSeconds;

            if ( lowerLimitSeconds < 1 )
            {
                lowerLimitSeconds = DefaultValue.DefaultLeadershipPollingIntervalLowerLimitSeconds;
            }

            return lowerLimitSeconds;
        }

        /// <summary>
        /// Gets the upper polling limit seconds.
        /// </summary>
        /// <returns></returns>
        public static int GetUpperPollingLimitSeconds()
        {
            var lowerLimitSeconds = GetLowerPollingLimitSeconds();
            var upperLimitSeconds =
                    SystemSettings.GetValue( SystemSetting.WEBFARM_LEADERSHIP_POLLING_INTERVAL_UPPER_LIMIT_SECONDS ).AsIntegerOrNull() ??
                    DefaultValue.DefaultLeadershipPollingIntervalUpperLimitSeconds;

            if ( upperLimitSeconds < lowerLimitSeconds )
            {
                upperLimitSeconds = lowerLimitSeconds + 1;
            }

            return upperLimitSeconds;
        }

        /// <summary>
        /// Gets the minimum difference in seconds between nodes' polling intervals.
        /// </summary>
        /// <returns></returns>
        public static int GetMinimumPollingDifferenceSeconds()
        {
            var minDifferenceSeconds =
                    SystemSettings.GetValue( SystemSetting.WEBFARM_LEADERSHIP_MIN_POLLING_DIFFERENCE_SECONDS ).AsIntegerOrNull() ??
                    DefaultValue.DefaultMinimumPollingDifferenceSeconds;
            return minDifferenceSeconds;
        }

        /// <summary>
        /// Gets the max time to wait after sending a ping before assuming non-responders are offline.
        /// </summary>
        /// <returns></returns>
        public static int GetMaxPollingWaitSeconds()
        {
            var maxWaitSeconds =
                    SystemSettings.GetValue( SystemSetting.WEBFARM_LEADERSHIP_MAX_WAIT_SECONDS ).AsIntegerOrNull() ??
                    DefaultValue.DefaultPollingMaxWaitSeconds;
            return maxWaitSeconds;
        }

        /// <summary>
        /// Determines whether [is current job runner].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is current job runner]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsCurrentJobRunner()
        {
            var webFarmJobRunnerString = ConfigurationManager.AppSettings["WebFarmJobRunner"];
            return webFarmJobRunnerString.AsBoolean();
        }

        /// <summary>
        /// Gets the node record, potentially creating it.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="nodeName">Name of the node.</param>
        /// <param name="pollingInterval">The polling interval.</param>
        /// <returns>
        ///   <c>true</c> if [is polling interval in use] [the specified node name]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsPollingIntervalInUse( RockContext rockContext, string nodeName, decimal pollingInterval )
        {
            Debug( $"Checking poll interval {pollingInterval}" );

            var webFarmNodeService = new WebFarmNodeService( rockContext );
            var anyMatches = webFarmNodeService.Queryable().Any( wfn =>
                wfn.NodeName != nodeName &&
                wfn.CurrentLeadershipPollingIntervalSeconds == pollingInterval );

            return anyMatches;
        }

        /// <summary>
        /// Generates a polling interval in seconds.
        /// </summary>
        /// <returns></returns>
        private static decimal GeneratePollingIntervalSeconds( int minSeconds, int maxSeconds )
        {
            var minPollingIntervalDifferenceSeconds = GetMinimumPollingDifferenceSeconds();

            var minSteps = minSeconds / minPollingIntervalDifferenceSeconds;
            var maxSteps = maxSeconds / minPollingIntervalDifferenceSeconds;

            var random = new Random();
            int randomSteps = random.Next( minSteps, maxSteps );

            var randomSeconds = randomSteps * minPollingIntervalDifferenceSeconds;
            return randomSeconds;
        }

        /// <summary>
        /// Gets the node.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="nodeName">Name of the node.</param>
        /// <returns></returns>
        private static WebFarmNode GetNode( RockContext rockContext, string nodeName )
        {
            var webFarmNodeService = new WebFarmNodeService( rockContext );
            var webFarmNode = webFarmNodeService.Queryable().Single( wfn => wfn.NodeName == nodeName );
            return webFarmNode;
        }

        /// <summary>
        /// Publishes the event.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="recipientNodeName">Name of the recipient node. Omit if meant for all nodes.</param>
        /// <param name="payload">The payload.</param>
        private static void PublishEvent( string eventType, string recipientNodeName = "", string payload = "" )
        {
            Debug( $"Sending {eventType} to {( recipientNodeName.IsNullOrWhiteSpace() ? "all" : recipientNodeName )}" );
            WebFarmWasUpdatedMessage.Publish( NodeName, eventType, recipientNodeName, payload );
        }

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="callerMethod">The caller method.</param>
        private static void LogException( string message, [CallerMemberName] string callerMethod = "" )
        {
            var severity = WebFarmNodeLog.SeverityLevel.Critical;
            var eventType = EventType.Error;
            var text = callerMethod.IsNullOrWhiteSpace() ? message : $"[{callerMethod}]: {message}";

            using ( var rockContext = new RockContext() )
            {
                AddLog( rockContext, severity, _nodeId, eventType, text );
            }
        }

        /// <summary>
        /// Debugs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        private static void Debug( string message )
        {
            if ( DEBUG && System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
            {
                System.Diagnostics.Debug.WriteLine( $"\tFARM {RockDateTime.Now:mm.ss.f} {message}" );
            }
        }

        #endregion Helper Methods
    }
}

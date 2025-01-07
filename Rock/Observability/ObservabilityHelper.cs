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
using System.Configuration;
using System.Diagnostics;

using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Rock.Configuration;
using Rock.SystemKey;
using Rock.ViewModels.Utility;

namespace Rock.Observability
{
    /// <summary>
    /// A helper class to assist in various observability tasks.
    /// </summary>
    public static class ObservabilityHelper
    {
        private static string DefaultTracesPath = "v1/traces";
        private static string DefaultMetricsPath = "v1/metrics";
        private static string DefaultLogsPath = "v1/logs";

        private static TracerProvider _currentTracerProvider;
        private static MeterProvider _currentMeterProvider;


        /// <summary>
        /// The global meter provider.
        /// </summary>
        public static MeterProvider MeterProvider {
            get {
                return _currentMeterProvider;
            }
        }

        /// <summary>
        /// The version number is used for every activity, which can be hundreds
        /// per request. And the value never changes so only grab it once.
        /// </summary>
        private static readonly Lazy<string> _rockVersion = new Lazy<string>( () => VersionInfo.VersionInfo.GetRockProductVersionFullName() );

        /// <summary>
        /// Cache the machine name to reduce the load from a Win32 native call.
        /// </summary>
        private static readonly Lazy<string> _machineName = new Lazy<string>( () => Environment.MachineName.ToLower() );

        /// <summary>
        /// Gets the maximum number of spans that will be allowed on a single trace.
        /// </summary>
        internal static int SpanCountLimit
        {
            get
            {
                if ( Rock.Web.SystemSettings.TryGetCachedValue( SystemSetting.OBSERVABILITY_SPAN_COUNT_LIMIT, out var value ) )
                {
                    return value.AsIntegerOrNull() ?? 9_900;
                }
                else
                {
                    return 9_900;
                }
            }
        }

        /// <summary>
        /// Gets the maximum length a single attribute value should be.
        /// </summary>
        internal static int MaximumAttributeLength
        {
            get
            {
                if ( Rock.Web.SystemSettings.TryGetCachedValue( SystemSetting.OBSERVABILITY_MAX_ATTRIBUTE_LENGTH, out var value ) )
                {
                    return value.AsIntegerOrNull() ?? 4_000;
                }
                else
                {
                    return 4_000;
                }
            }
        }

        static ObservabilityHelper()
        {
            _currentTracerProvider = null;
            _currentMeterProvider = null;
        }

        #region Properties
        /// <summary>
        /// Returns the service name defined in the web.config.
        /// </summary>
        public static string ServiceName
        {
            get => ConfigurationManager.AppSettings["ObservabilityServiceName"]?.Trim() ?? string.Empty;
        }
        #endregion

        /// <summary>
        /// Configures the observability TraceProvider and passes back a reference to it.
        /// </summary>
        /// <returns></returns>
        public static TracerProvider ConfigureObservability( bool isRockStartup = false )
        {
            // Configure the trace provider
            ConfigureTraceProvider();

            // Configure the metric provider
            ConfigureMeterProvider();

            // Wire-up the system metrics
            if ( isRockStartup )
            {
                RockMetricSource.StartCoreMetrics();
            }
            
            return _currentTracerProvider;
        }

        /// <summary>
        /// Configures the observability TraceProvider.
        /// </summary>
        internal static void ReconfigureObservability()
        {
            ConfigureObservability();
        }

        /// <summary>
        /// Configures the trace provider.
        /// </summary>
        /// <returns></returns>
        public static TracerProvider ConfigureTraceProvider()
        {
            // Determine if a trace provider is already configured.
            var traceProviderPreviouslyConfigured = _currentTracerProvider != null;

            // Clear out the current trace provider
            _currentTracerProvider?.Dispose();

            Uri.TryCreate( Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_ENDPOINT ), UriKind.Absolute, out var endpointUri );
            var observabilityEnabled = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_ENABLED ).AsBoolean();
            var endpointHeaders = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_ENDPOINT_HEADERS )?.Replace( "^", "=" ).Replace( "|", "," );
            var endpointProtocol = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_ENDPOINT_PROTOCOL ).ToString().ConvertToEnumOrNull<OpenTelemetry.Exporter.OtlpExportProtocol>() ?? OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            var serviceName = ObservabilityHelper.ServiceName;

            if ( endpointUri != null && endpointProtocol == OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf )
            {
                endpointUri = AppendPathIfNotEndsWith( endpointUri, DefaultTracesPath );
            }

            if ( observabilityEnabled && endpointUri != null )
            {
                _currentTracerProvider = Sdk.CreateTracerProviderBuilder()
                    .AddOtlpExporter( o =>
                    {
                        o.Endpoint = endpointUri;
                        o.Protocol = endpointProtocol;
                        o.Headers = endpointHeaders;
                    } )

                   // Other configuration, like adding an exporter and setting resources
                   .AddSource( serviceName )  // Be sure to update this in RockActivitySource.cs also!!!    

                   .SetResourceBuilder(
                       ResourceBuilder.CreateDefault()
                           .AddService( serviceName: serviceName, serviceVersion: "1.0.0", serviceInstanceId: GetServiceInstanceId() ) )
                   .Build();

                // If there was already a trace provider running call the ActivitySource refresh to ensure it knows to update it's service name
                if ( traceProviderPreviouslyConfigured )
                {
                    RockActivitySource.RefreshActivitySource();
                }
            }

            return _currentTracerProvider;
        }

        /// <summary>
        /// Configures and returns a meter provider
        /// </summary>
        /// <returns></returns>
        public static MeterProvider ConfigureMeterProvider()
        {
            // Clear out the current trace provider
            _currentMeterProvider?.Dispose();

            Uri.TryCreate( Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_ENDPOINT ), UriKind.Absolute, out var endpointUri );
            var observabilityEnabled = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_ENABLED ).AsBoolean();
            var endpointHeaders = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_ENDPOINT_HEADERS )?.Replace( "^", "=" ).Replace( "|", "," );
            var endpointProtocol = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_ENDPOINT_PROTOCOL ).ToString().ConvertToEnumOrNull<OpenTelemetry.Exporter.OtlpExportProtocol>() ?? OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            var serviceName = ObservabilityHelper.ServiceName;

            if ( endpointUri != null && endpointProtocol == OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf )
            {
                endpointUri = AppendPathIfNotEndsWith( endpointUri, DefaultMetricsPath );
            }

            if ( observabilityEnabled && endpointUri != null )
            {
                _currentMeterProvider = Sdk.CreateMeterProviderBuilder()
                    .SetResourceBuilder( ResourceBuilder.CreateDefault().AddService( serviceName: serviceName, serviceVersion: "1.0.0", serviceInstanceId: GetServiceInstanceId() ) )
                    .AddMeter( serviceName )
                    .AddOtlpExporter( o =>
                    {
                        o.Endpoint = endpointUri;
                        o.Protocol = endpointProtocol;
                        o.Headers = endpointHeaders;
                    }
                    )
                    .Build();
            }

            return _currentMeterProvider;
        }

        /// <summary>
        /// Helper method to create a new observability activity that has a common set of attributes applied to it.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="kind"></param>
        public static Activity StartActivity( string name, ActivityKind kind = ActivityKind.Internal )
        {
            // Some systems only support a specific number of spans for a single
            // trace. This checks to see if the root activity (trace) has more
            // activities than the limit and if so does not create a new activity.
            if ( Activity.Current != null )
            {
                var rootActivity = GetRootActivity( Activity.Current );
                var childCount = rootActivity.GetTagItem( "rock.descendant_count" ) as int? ?? 0;

                rootActivity.SetTag( "rock.descendant_count", childCount + 1 );

                // Subtract one since the root activity is not counted in childCount.
                if ( childCount >= SpanCountLimit - 1 )
                {
                    return null;
                }
            }

            var activity = RockActivitySource.ActivitySource.StartActivity( name, kind );

            if ( activity == null )
            {
                return null;
            }

            var nodeName = RockApp.Current.HostingSettings.NodeName.ToLower();
            var machineName = _machineName.Value;

            // Add on default attributes
            activity.AddTag( "rock.node", nodeName );

            if (nodeName != machineName )
            {
                activity.AddTag( "service.instance.id", $"{machineName} ({nodeName})" );
            }
            else
            {
                activity.AddTag( "service.instance.id", machineName );
            }

            activity.AddTag( "service.version", _rockVersion.Value );

            return activity;
        }

        /// <summary>
        /// Converts the open telemetry exporter protocols to a <see cref="ListItemBag"/> list.
        /// </summary>
        /// <returns></returns>
        public static List<ListItemBag> GetOpenTelemetryExporterProtocolsAsListItemBag()
        {
            return typeof( OpenTelemetry.Exporter.OtlpExportProtocol ).ToEnumListItemBag();
        }

        /// <summary>
        /// Finds the root activity of the given activity. This walks up the
        /// Parent chain until no more parents are found.
        /// </summary>
        /// <param name="activity">The activity to start with when walking up the ancestor tree.</param>
        /// <returns>The ancestor Activity that has no parent or <c>null</c> if <paramref name="activity"/> was also null.</returns>
        internal static Activity GetRootActivity( Activity activity )
        {
            if ( activity == null )
            {
                return null;
            }

            while ( activity.Parent != null )
            {
                activity = activity.Parent;
            }

            return activity;
        }

        /// <summary>
        /// Increments the database query count tag on the root activity and any
        /// intermediate activities with an existing "rock.db.query_count" tag.
        /// </summary>
        /// <param name="activity">The activity to start with when walking up the ancestor tree.</param>
        internal static void IncrementDbQueryCount( Activity activity )
        {
            while ( activity != null )
            {
                var queryCount = activity.GetTagItem( "rock.db.query_count" ) as int?;

                // If the activity already has a query count or its the root
                // activity then increment the value. This allows activities
                // to request that they also get the query count recorded
                // on them by setting the initial value to zero.
                if ( queryCount.HasValue || activity.Parent == null )
                {
                    activity.SetTag( "rock.db.query_count", ( queryCount ?? 0 ) + 1 );
                }

                activity = activity.Parent;
            }
        }

        /// <summary>
        /// Enables tracking of database query counts for the specified activity.
        /// The root activity will always track query counts.
        /// </summary>
        /// <param name="activity">The activity for which to enable database query count tracking.</param>
        internal static void EnableDbQueryCountTracking( Activity activity )
        {
            if ( activity != null && activity.GetTagItem( "rock.db.query_count" ) == null )
            {
                activity.SetTag( "rock.db.query_count", 0 );
            }
        }

        /// <summary>
        /// Appends the path to the URI if it doesn't already end with the path.
        /// </summary>
        /// <param name="uri">The URI to be modified.</param>
        /// <param name="path">The path to be appended.</param>
        /// <returns>A new <see cref="Uri"/> if the path is modified, otherwise the original <paramref name="uri"/> is returned.</returns>
        private static Uri AppendPathIfNotEndsWith( Uri uri, string path )
        {
            var absoluteUri = uri.AbsoluteUri;
            var separator = string.Empty;

            if ( absoluteUri.EndsWith( "/" ) )
            {
                // Endpoint already ends with 'path/'
                if ( absoluteUri.EndsWith( string.Concat( path, "/" ), StringComparison.OrdinalIgnoreCase ) )
                {
                    return uri;
                }
            }
            else
            {
                // Endpoint already ends with 'path'
                if ( absoluteUri.EndsWith( path, StringComparison.OrdinalIgnoreCase ) )
                {
                    return uri;
                }

                separator = "/";
            }

            return new Uri( string.Concat( uri.AbsoluteUri, separator, path ) );
        }

        /// <summary>
        /// Gets the service instance identifier.
        /// </summary>
        /// <returns>A string containing the instance identifier.</returns>
        private static string GetServiceInstanceId()
        {
            var nodeName = RockApp.Current.HostingSettings.NodeName.ToLower();
            var machineName = _machineName.Value;

            if ( nodeName != machineName )
            {
                return $"{machineName} ({nodeName})";
            }
            else
            {
                return machineName;
            }
        }
    }
}

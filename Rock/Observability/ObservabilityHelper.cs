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
using OpenTelemetry.Resources;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Rock.SystemKey;
using System;
using System.Configuration;
using System.Diagnostics;
using Rock.Bus;

namespace Rock.Observability
{
    /// <summary>
    /// A helper class to assist in various observability tasks.
    /// </summary>
    public static class ObservabilityHelper
    {
        private static TracerProvider _currentTracerProvider;

        /// <summary>
        /// The version number is used for every activity, which can be hundreds
        /// per request. And the value never changes so only grab it once.
        /// </summary>
        private static readonly Lazy<string> _rockVersion = new Lazy<string>( () => VersionInfo.VersionInfo.GetRockProductVersionFullName() );

        /// <summary>
        /// Cache the machine name to reduce the load from a Win32 native call.
        /// </summary>
        private static readonly Lazy<string> _machineName = new Lazy<string>( () => Environment.MachineName.ToLower() );

        static ObservabilityHelper()
        {
            _currentTracerProvider = null;
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
        public static TracerProvider ConfigureTraceProvider()
        {
            // Determine if a trace provider is already configured.
            var traceProviderPreviouslyConfigured = _currentTracerProvider != null;

            // Clear out the current trace provider
            _currentTracerProvider?.Dispose();

            Uri endpointUri = null;
            Uri.TryCreate( Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_ENDPOINT ), UriKind.Absolute, out endpointUri );
            var observabilityEnabled = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_ENABLED ).AsBoolean();
            var endpointHeaders = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_ENDPOINT_HEADERS )?.Replace("^", "=").Replace("|", ",");
            var endpointProtocol = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_ENDPOINT_PROTOCOL ).ToString().ConvertToEnumOrNull<OpenTelemetry.Exporter.OtlpExportProtocol>() ?? OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            var serviceName = ObservabilityHelper.ServiceName;

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
                       .AddService( serviceName: serviceName, serviceVersion: "1.0.0" ) )
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
        /// Helper method to create a new observability activity that has a common set of attributes applied to it.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="kind"></param>
        public static Activity StartActivity( string name, ActivityKind kind = ActivityKind.Internal )
        {
            // Some systems only support an activity chain with up to 10,000
            // total related activities. We store the number on the root
            // activity and if it exceeds 9,999 then we don't start an activity.
            if ( Activity.Current != null )
            {
                var rootActivity = GetRootActivity( Activity.Current );
                var childCount = rootActivity.GetTagItem( "rock-descendant-count" ) as int? ?? 0;

                rootActivity.SetTag( "rock-descendant-count", childCount + 1 );

                if ( childCount >= 9_999 )
                {
                    return null;
                }
            }

            var activity = RockActivitySource.ActivitySource.StartActivity( name, kind );

            if ( activity == null )
            {
                return null;
            }

            var nodeName = RockMessageBus.NodeName.ToLower();
            var machineName = _machineName.Value;

            // Add on default attributes
            activity.AddTag( "rock-node", nodeName );

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
        /// Finds the root activity of the given activity. This walks up the
        /// Parent chain until no more parents are found.
        /// </summary>
        /// <param name="activity">The activity to start with when walking up the ancestor tree.</param>
        /// <returns>The ancestor Activity that has no parent or <c>null</c> if <paramref name="activity"/> was also null.</returns>
        private static Activity GetRootActivity( Activity activity )
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
    }
}

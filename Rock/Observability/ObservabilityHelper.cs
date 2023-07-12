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
using Rock.Field.Types;
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
        private static TracerProvider _curretTracerProvider;

        static ObservabilityHelper()
        {
            _curretTracerProvider = null;
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
            var traceProviderPreviouslyConfigured = _curretTracerProvider != null;

            // Clear out the current trace provider
            _curretTracerProvider?.Dispose();

            Uri endpointUri = null;
            Uri.TryCreate( Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_ENDPOINT ), UriKind.Absolute, out endpointUri );
            var observabilityEnabled = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_ENABLED ).AsBoolean();
            var endpointHeaders = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_ENDPOINT_HEADERS )?.Replace("^", "=").Replace("|", ",");
            var endpointProtocol = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_ENDPOINT_PROTOCOL ).ToString().ConvertToEnumOrNull<OpenTelemetry.Exporter.OtlpExportProtocol>() ?? OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            var serviceName = ObservabilityHelper.ServiceName;

            if ( observabilityEnabled && endpointUri != null )
            {

                _curretTracerProvider = Sdk.CreateTracerProviderBuilder()
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

            return _curretTracerProvider;
        }

        /// <summary>
        /// Helper method to create a new observability activity that has a common set of attributes applied to it.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="kind"></param>
        public static Activity StartActivity( string name, ActivityKind kind = ActivityKind.Internal )
        {
            RockActivitySource.ActivitySource.StartActivity( name, kind );

            var nodeName = RockMessageBus.NodeName.ToLower();
            var machineName = Environment.MachineName.ToLower();

            // Add on default attributes
            Activity.Current?.AddTag( "rock-node", nodeName );

            if (nodeName != machineName )
            {
                Activity.Current?.AddTag( "service.instance.id", $"{machineName} ({nodeName})" );
            }
            else
            {
                Activity.Current?.AddTag( "service.instance.id", machineName );
            }
            
            Activity.Current?.AddTag( "service.version", VersionInfo.VersionInfo.GetRockProductVersionFullName() );

            return Activity.Current;
        }
    }
}

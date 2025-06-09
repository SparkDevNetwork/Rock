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

using System.Collections.Generic;

using Rock.Enums.Observability;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Administration.SystemConfiguration
{
    /// <summary>
    /// Contains the observability configuration details.
    /// </summary>
    public class ObservabilityConfigurationBag
    {
        /// <summary>
        /// The individual features that are enabled for observability.
        /// </summary>
        public FeatureFlags EnabledFeatures { get; set; }

        /// <summary>
        /// Gets or sets the endpoint.
        /// </summary>
        /// <value>
        /// The endpoint.
        /// </value>
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the span count limit.
        /// </summary>
        /// <value>
        /// The span count limit.
        /// </value>
        public int? SpanCountLimit { get; set; }

        /// <summary>
        /// Gets or sets the endpoint protocol.
        /// </summary>
        /// <value>
        /// The endpoint protocol.
        /// </value>
        public string EndpointProtocol { get; set; }

        /// <summary>
        /// The detail level of the trace information that will be generated.
        /// </summary>
        public TraceLevel TraceLevel { get; set; }

        /// <summary>
        /// Gets or sets the maximum length of the attribute.
        /// </summary>
        /// <value>
        /// The maximum length of the attribute.
        /// </value>
        public int? MaximumAttributeLength { get; set; }

        /// <summary>
        /// Gets or sets the endpoint headers.
        /// </summary>
        /// <value>
        /// The endpoint headers.
        /// </value>
        public List<ListItemBag> EndpointHeaders { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include query statements].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include query statements]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeQueryStatements { get; set; }

        /// <summary>
        /// Gets or sets the targeted queries.
        /// </summary>
        /// <value>
        /// The targeted queries.
        /// </value>
        public List<string> TargetedQueries { get; set; }
    }
}

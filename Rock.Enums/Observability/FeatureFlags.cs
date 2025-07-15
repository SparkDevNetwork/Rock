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

namespace Rock.Enums.Observability
{
    /// <summary>
    /// The features that can be enabled for observability.
    /// </summary>
    [Flags]
    public enum FeatureFlags
    {
        /// <summary>
        /// Traces are recorded and sent to the observability endpoint.
        /// </summary>
        Traces = 0x0001,

        /// <summary>
        /// Metrics are recorded and sent to the observability endpoint.
        /// </summary>
        Metrics = 0x0002,

        /// <summary>
        /// Logs are recorded and sent to the observability endpoint.
        /// </summary>
        Logs = 0x0004,
    }
}

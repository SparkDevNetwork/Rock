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

namespace Rock.ViewModels.Blocks.Administration.SparkConnectedServices
{
    /// <summary>
    /// Represents the result of a save operation for Rock Intelligence
    /// configuration.
    /// </summary>
    public class SaveRockIntelligenceResponseBag
    {
        /// <summary>
        /// The refreshed Rock Intelligence configuration reflecting the
        /// state after the save operation. This is populated whether or
        /// not a one-time boost was attempted, so the client can bind
        /// its view to the latest configuration.
        /// </summary>
        public RockIntelligenceConfigurationBag Configuration { get; set; }

        /// <summary>
        /// The status of the one-time boost attempt, or <c>null</c> when
        /// no boost was included in the save request. When present, the
        /// client should surface pending or declined outcomes with an
        /// appropriate message.
        /// </summary>
        public int BoostStatus { get; set; }

        /// <summary>
        /// A human-readable message describing the outcome of the
        /// one-time boost attempt when <see cref="BoostStatus"/> warrants
        /// one. May be <c>null</c>.
        /// </summary>
        public string BoostMessage { get; set; }
    }
}

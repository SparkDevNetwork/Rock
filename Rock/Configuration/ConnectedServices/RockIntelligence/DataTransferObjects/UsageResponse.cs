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

namespace Rock.Configuration.ConnectedServices.RockIntelligence.DataTransferObjects
{
    /// <summary>
    /// Represents the response from the Spark connected services API regarding
    /// the usage of the service.
    /// </summary>
    internal class UsageResponse
    {
        /// <summary>
        /// The balance remaining and available on the service. This will be
        /// <c>null</c> if not available.
        /// </summary>
        public decimal? Balance { get; set; }

        /// <summary>
        /// The amount spent this month on the service. This will be <c>null</c>
        /// if not available.
        /// </summary>
        public decimal? MonthlyUsage { get; set; }
    }
}

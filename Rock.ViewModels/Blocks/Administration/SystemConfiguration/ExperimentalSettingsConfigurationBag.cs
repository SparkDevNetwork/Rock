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

namespace Rock.ViewModels.Blocks.Administration.SystemConfiguration
{
    /// <summary>
    /// Contains the experimental settings configuration details.
    /// </summary>
    public class ExperimentalSettingsConfigurationBag
    {
        /// <summary>
        /// Gets or sets the starting day of week.
        /// </summary>
        /// <value>
        /// The starting day of week.
        /// </value>
        public string StartingDayOfWeek { get; set; }

        /// <summary>
        /// Gets or sets the duration of the security grant token.
        /// </summary>
        /// <value>
        /// The duration of the security grant token.
        /// </value>
        public int? SecurityGrantTokenDuration { get; set; }
    }
}

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

using Rock.Update.Models;

namespace Rock.Update.Interfaces
{
    /// <summary>
    /// This interface is implemented by RockImpactService and is used so we can mock the service for testing.
    /// </summary>
    public interface IRockImpactService
    {
        /// <summary>
        /// Sends the impact statistics to spark.
        /// </summary>
        /// <param name="impactStatistic">The impact statistic.</param>
        void SendImpactStatisticsToSpark( ImpactStatistic impactStatistic );
    }
}
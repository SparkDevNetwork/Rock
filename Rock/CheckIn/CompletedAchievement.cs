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

using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.CheckIn
{
    /// <summary>
    /// Checkin object for a completed <see cref="Rock.Model.AchievementAttempt"/>
    /// </summary>
    /// <seealso cref="Rock.Utility.RockDynamic" />
    public class CompletedAchievement : RockDynamic
    {
        /// <inheritdoc cref="AchievementTypeCache"/>
        public AchievementTypeCache AchievementType => AchievementTypeCache.Get( this.AchievementTypeId );

        /// <inheritdoc cref="AchievementAttempt.AchievementTypeId"/>
        public int AchievementTypeId { get; set; }

        /// <inheritdoc cref="AchievementAttempt.AchievementAttemptStartDateTime"/>
        public DateTime? AttemptStartDateTime { get; set; }

        /// <inheritdoc cref="AchievementAttempt.AchievementAttemptEndDateTime"/>
        public DateTime? AttemptEndDateTime { get; set; }

        /// <summary>
        /// The Id of the Person that completed the achievement
        /// </summary>
        /// <value>The person identifier.</value>
        public int PersonId { get; set; }
    }
}

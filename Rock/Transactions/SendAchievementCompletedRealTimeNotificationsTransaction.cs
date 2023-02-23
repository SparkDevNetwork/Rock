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
using System.Linq;
using System.Threading.Tasks;

using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Sends any real-time notifications for achievement attempt records that
    /// have been completed.
    /// </summary>
    internal class SendAchievementCompletedRealTimeNotificationsTransaction : AggregateAsyncTransaction<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendAchievementCompletedRealTimeNotificationsTransaction"/> class.
        /// </summary>
        /// <param name="achievementAttemptGuid">The achievement attempt unique identifier.</param>
        public SendAchievementCompletedRealTimeNotificationsTransaction( Guid achievementAttemptGuid )
            : base( achievementAttemptGuid )
        {
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync( IList<Guid> items )
        {
            // Get the distinct set of items that were added/modified.
            var itemGuids = items
                .Distinct()
                .ToList();

            await AchievementAttemptService.SendAchievementCompletedRealTimeNotificationsAsync( itemGuids );
        }
    }
}

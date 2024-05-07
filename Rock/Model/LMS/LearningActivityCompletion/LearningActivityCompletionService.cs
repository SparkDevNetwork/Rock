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
using System.Linq;

namespace Rock.Model
{
    public partial class LearningActivityCompletionService
    {
        /// <summary>
        /// Updates the NotificationCommunicationId property of <see cref="LearningActivityCompletion"/>
        /// for the provided <paramref name="activityCompletionIds"/>.
        /// </summary>
        /// <param name="activityCompletionIds">List of <see cref="LearningActivityCompletion"/> identifiers to update.</param>
        /// <param name="systemCommunicationId">The system communication id to set for the given identifiers.</param>
        public void UpdateNotificationCommunicationProperties( List<int> activityCompletionIds, int systemCommunicationId )
        {
            var activityCompletions = Queryable().Where( c => activityCompletionIds.Contains( c.Id ) );

            this.Context.BulkUpdate( activityCompletions, a => new LearningActivityCompletion { NotificationCommunicationId = systemCommunicationId } );
        }
    }
}
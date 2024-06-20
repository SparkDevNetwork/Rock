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
using System.Data.Entity;
using System.Linq;

using Rock.Utility;

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

            Context.BulkUpdate( activityCompletions, a => new LearningActivityCompletion { NotificationCommunicationId = systemCommunicationId } );
        }

        /// <summary>
        /// Gets a list of class activies for the specified person and class.
        /// </summary>
        /// <param name="personId">The identifier of the person for whom to get the activities.</param>
        /// <param name="classIdKey">The hashed identifier of the class for which to get the activities.</param>
        /// <returns>An IQueryable of <see cref="LearningActivityCompletion"/> records for the specified person and class.</returns>
        public IQueryable<LearningActivityCompletion> GetClassActivities( int personId, string classIdKey )
        {
            var classId = IdHasher.Instance.GetId( classIdKey );
            return classId.HasValue ?
                GetClassActivities( personId, classId.Value ) :
                new List<LearningActivityCompletion>().AsQueryable();
        }

        /// <summary>
        /// Gets a list of class activies for the specified person and class.
        /// </summary>
        /// <param name="personId">The identifier of the person for whom to get the activities.</param>
        /// <param name="classId">The identifier of the class for which to get the activities.</param>
        /// <returns>An IQueryable of <see cref="LearningActivityCompletion"/> records for the specified person and class.</returns>
        public IQueryable<LearningActivityCompletion> GetClassActivities( int personId, int classId )
        {
            return Queryable()
                .Include( c => c.LearningActivity )
                .Include( c => c.Student )
                .Where( c => c.Student.LearningClassId == classId )
                .Where( c => c.Student.PersonId == personId );
        }
    }
}
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
using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// Note Extension methods.
    /// </summary>
    public static partial class LearningParticipantExtensionMethods
    {
        /// <summary>
        /// Filters the Queryable of participants to students only.
        /// </summary>
        /// <param name="participants">The participant queryable to filter</param>
        /// <returns>An IQueryable of participants filtered to students only.</returns>
        public static IQueryable<LearningParticipant> AreStudents( this IQueryable<LearningParticipant> participants )
        {
            return participants.Where( p => !p.GroupRole.IsLeader );
        }

        /// <summary>
        /// Filters the Queryable of participants to facilitators only.
        /// </summary>
        /// <param name="participants">The participant queryable to filter</param>
        /// <returns>An IQueryable of participants filtered to facilitators only.</returns>
        public static IQueryable<LearningParticipant> AreFacilitators( this IQueryable<LearningParticipant> participants )
        {
            return participants.Where( p => p.GroupRole.IsLeader );
        }
    }
}

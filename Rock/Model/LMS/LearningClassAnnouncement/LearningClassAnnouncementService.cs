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
    public partial class LearningClassAnnouncementService
    {
        /// <summary>
        /// Gets the announcements for the specified learning class.
        /// </summary>
        /// <param name="classId">The identifier of the class to get announcements for.</param>
        /// <returns>An IQueryable of learning class announcements for the specified class. </returns>
        public IQueryable<LearningClassAnnouncement> GetForClass( int classId )
        {
            var now = RockDateTime.Now;
            return Queryable()
                .Where( a => a.LearningClassId == classId && a.PublishDateTime <= now );

        }
    }
}
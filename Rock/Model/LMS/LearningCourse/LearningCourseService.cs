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
using Rock.Data;
using Rock.Utility;

namespace Rock.Model
{
    public partial class LearningCourseService
    {
        /// <summary>
        /// Gets the default active <see cref="Rock.Model.LearningClass"/> for the specified Course IdKey.
        /// </summary>
        /// <param name="idKey">The idKey of the <see cref="LearningCourse"/> for which to retrieve the default class.</param>
        /// <returns>The first active learning class.</returns>
        public LearningClass GetDefaultClass( string idKey )
        {
            var id = IdHasher.Instance.GetId( idKey ).ToIntSafe();
            return id > 0 ? new LearningClass() : GetDefaultClass( id );
        }

        /// <summary>
        /// Gets the default active <see cref="Rock.Model.LearningClass"/> for the specified Course Id.
        /// </summary>
        /// <param name="id">The identifier of the <see cref="LearningCourse"/> for which to retrieve the default class.</param>
        /// <returns>The first active learning class.</returns>
        public LearningClass GetDefaultClass( int id )
        {
            var classService = new LearningClassService( ( RockContext ) Context );
            return classService
                .Queryable()
                .OrderBy( c => c.Order )
                .FirstOrDefault( c => c.IsActive && c.LearningCourseId == id );
        }
    }
}
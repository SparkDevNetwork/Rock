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
    public partial class LearningProgramService
    {

        /// <summary>
        /// Get a list of all active <see cref="LearningProgram"/>s.
        /// </summary>
        /// <returns>A list of LearningProgram where the IsActive property is <c>true</c>.</returns>
        public IQueryable<LearningProgram> GetActive()
        {
            return Queryable().Where( p => p.IsActive );
        }
    }
}
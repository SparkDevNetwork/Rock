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
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// CampusTopicService Service class
    /// </summary>
    public class CampusTopicService : Service<CampusTopic>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CampusScheduleService"/> class
        /// </summary>
        /// <param name="context">The context.</param>
        public CampusTopicService( DbContext context ) : base( context )
        {
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( CampusTopic item, out string errorMessage )
        {
            errorMessage = string.Empty;
            return true;
        }
    }
}

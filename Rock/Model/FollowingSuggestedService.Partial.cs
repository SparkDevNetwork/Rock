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
using System.Linq;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.FollowingSuggested"/> entity objects
    /// </summary>
    /// <seealso cref="Rock.Model.FollowingSuggested" />
    public partial class FollowingSuggestedService
    {
        /// <summary>
        /// Gets the by entity and person.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public IQueryable<FollowingSuggested> GetByEntityAndPerson( int entityTypeId, int entityId, int personId )
        {
            return this.Queryable()
                .Where( f => f.EntityTypeId == entityTypeId
                    && f.PersonAlias.PersonId == personId
                    && f.EntityId == entityId );
        }
    }
}

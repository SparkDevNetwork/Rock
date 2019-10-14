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

using Rock;

namespace Rock.Model
{
    /// <summary>
    /// EntitySetItem POCO Service class
    /// </summary>
    public partial class EntitySetItemService
    {
        /// <summary>
        /// Gets the by entity set identifier.
        /// </summary>
        /// <param name="entitySetId">The entity set identifier.</param>
        /// <param name="overrideExpiration">if set to <c>true</c> [override expiration].</param>
        /// <returns></returns>
        public IQueryable<EntitySetItem> GetByEntitySetId( int entitySetId, bool overrideExpiration = false )
        {
            var qry = Queryable().Where( s => s.EntitySetId == entitySetId);
            if (!overrideExpiration)
            {
                var currentDateTime = RockDateTime.Now;
                qry = qry.Where( s => !s.EntitySet.ExpireDateTime.HasValue || s.EntitySet.ExpireDateTime.Value > currentDateTime);
            }

            qry = qry.OrderBy( s => s.Order ).ThenBy( s => s.Id );

            return qry;
        }
    }
}

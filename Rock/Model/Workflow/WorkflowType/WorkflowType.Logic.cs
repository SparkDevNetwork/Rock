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

using Rock.Web.Cache;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// WorkflowType Logic
    /// </summary>
    public partial class WorkflowType
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance has active forms.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has active forms; otherwise, <c>false</c>.
        /// </value>
        public bool HasActiveForms
        {
            get
            {
                return ActivityTypes
                    .Where( t => t.IsActive.HasValue && t.IsActive.Value )
                    .SelectMany( t => t.ActionTypes )
                    .Where( a => a.WorkflowFormId.HasValue )
                    .Any();
            }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this WorkflowType.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this WorkflowType.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion Public Methods

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return WorkflowTypeCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            WorkflowTypeCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion ICacheable
    }
}

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
using System.Data.Entity;

namespace Rock.Model
{
    /// <summary>
    /// WorkflowActivityType Logic
    /// </summary>
    public partial class WorkflowActivityType
    {
        #region Virtual Properties

        /// <summary>
        /// Gets the parent security authority for this WorkflowActivityType. 
        /// </summary>
        /// <value>
        /// An entity object implementing the  <see cref="Security.ISecured"/> interface, representing the parent security authority.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.WorkflowType != null ? this.WorkflowType : base.ParentAuthority;
            }
        }

        #endregion Virtual Properties

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this WorkflowActivityType.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this WorkflowActivityType.
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
            return WorkflowActivityTypeCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Data.DbContext dbContext )
        {
            WorkflowTypeCache.UpdateCachedEntity( this.WorkflowTypeId, EntityState.Modified );
            WorkflowActivityTypeCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion ICacheable
    }
}


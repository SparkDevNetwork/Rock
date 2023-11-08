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

using System.Data.Entity;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// DefinedValue Logic
    /// </summary>
    public partial class DefinedValue
    {
        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return DefinedValueCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            DefinedValueCache.UpdateCachedEntity( this.Id, entityState );
            DefinedTypeCache.FlushItem( this.DefinedTypeId );
        }

        #endregion

        /// <summary>
        /// Gets the parent security authority for this DefinedValue.
        /// </summary>
        /// <value>
        /// An entity that implements the <see cref="Rock.Security.ISecured"/> interface that this DefinedValue inherits security authority from.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.DefinedType != null ? this.DefinedType : base.ParentAuthority;
            }
        }
    }
}

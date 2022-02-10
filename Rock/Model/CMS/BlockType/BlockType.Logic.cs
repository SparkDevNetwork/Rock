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

using System.Data.Entity;

using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class BlockType
    {
        #region Properties

        /// <summary>
        /// Returns true if this block type is valid.
        /// </summary>
        /// <value>
        /// A <see cref="T:System.Boolean" /> that is <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public override bool IsValid
        {
            get
            {
                if ( !base.IsValid )
                {
                    return false;
                }

                //
                // If we have both, not valid.
                //
                if ( !string.IsNullOrWhiteSpace( Path ) && EntityTypeId.HasValue )
                {
                    return false;
                }

                //
                // If we have neither, not valid.
                //
                if ( string.IsNullOrWhiteSpace( Path ) && !EntityTypeId.HasValue )
                {
                    return false;
                }

                return true;
            }
        }

        #endregion Properties

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return BlockTypeCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            BlockTypeCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion
    }
}

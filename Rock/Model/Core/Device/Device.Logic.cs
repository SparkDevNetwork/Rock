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
    /// Device Logic
    /// </summary>
    public partial class Device
    {
        #region ICacheable

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Data.DbContext dbContext )
        {
            if ( entityState == EntityState.Added )
            {
                // Most other cache classes just always call this, but since we're specifically
                // doing other things for updated devices, we'll just call this
                // when EntityState.Added
                DeviceCache.UpdateCachedEntity( this.Id, entityState );
            }
            else
            {
                // There was a need in v1 Check-in to flush this item from the *special* KioskDevice cache too,
                // but this can likely be removed once Check-in v1 is no longer needed.
                // ( See b6c03d573eea6c3fbf14d23c40d08e8ae7d42a5b )
                Rock.CheckIn.KioskDevice.FlushItem( this.Id );
                DeviceCache.FlushItem( Id );
            }
        }

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return DeviceCache.Get( Id );
        }

        #endregion ICacheable
    }
}

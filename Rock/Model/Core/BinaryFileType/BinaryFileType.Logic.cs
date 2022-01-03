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
using System.Linq;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a type or category of binary files in Rock, and configures how binary files of this type are stored and accessed.
    /// </summary>
    public partial class BinaryFileType : Model<BinaryFileType>, ICacheable
    {
        /// <summary>
        /// Gets the count of <see cref="Rock.Model.BinaryFile" /> entities that are children of this <see cref="Rock.Model.BinaryFileType"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the count of <see cref="Rock.Model.BinaryFile"/> entities that are children of this <see cref="Rock.Model.BinaryFileType"/>.
        /// </value>
        public virtual int FileCount
        {
            get
            {
                return FileQuery.Count();
            }
        }

        /// <summary>
        /// Gets a queryable collection of <see cref="Rock.Model.BinaryFile" /> entities that are children of this <see cref="Rock.Model.BinaryFileType"/>.
        /// </summary>
        /// <value>
        /// A queryable collection of <see cref="Rock.Model.BinaryFile"/> entities that are children of this<see cref="Rock.Model.BinaryFileType"/>.
        /// </value>
        public virtual IQueryable<BinaryFile> FileQuery
        {
            get
            {
                var fileService = new BinaryFileService( new RockContext() );
                var qry = fileService.Queryable()
                    .Where( f => f.BinaryFileTypeId.HasValue && f.BinaryFileTypeId == this.Id );
                return qry;
            }
        }

        #region ICacheable

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Data.DbContext dbContext )
        {
            BinaryFileTypeCache.UpdateCachedEntity( this.Id, entityState );
        }

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return BinaryFileTypeCache.Get( this.Id );
        }

        #endregion
    }
}

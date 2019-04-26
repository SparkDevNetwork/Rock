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

using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.Block"/> objects.
    /// </summary>
    public partial class BlockService 
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Block">Blocks</see> that implement a specific <see cref="Rock.Model.BlockType"/>.
        /// </summary>
        /// <param name="blockTypeId">The Id of the <see cref="Rock.Model.BlockType"/> to search for.</param>
        /// <returns>An enumerable collection of <see cref="Block"/> entity objects that implemented the referenced <see cref="Rock.Model.BlockType"/>.</returns>
        public IQueryable<Block> GetByBlockTypeId( int blockTypeId )
        {
            return Queryable()
                .Where( t => t.BlockTypeId == blockTypeId )
                .OrderBy( t => t.Order );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Block">Blocks</see> that are implemented as part of a <see cref="Rock.Model.Site" /> layout.
        /// </summary>
        /// <param name="layoutId">An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Layout"/> that the block belongs to.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Block">Blocks</see> that are implemented as part of the provided site layout.
        /// </returns>
        public IQueryable<Block> GetByLayout( int layoutId )
        {
            return Queryable()
                .Where( t => t.LayoutId == layoutId )
                .OrderBy( t => t.Zone ).ThenBy( t => t.Order );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Block">Blocks</see> that are implemented in a specific zone on a Site Layout template.
        /// </summary>
        /// <param name="layoutId">An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Layout"/> that the block belongs to.</param>
        /// <param name="zone">A <see cref="System.String"/> representing the name of the Zone to search by.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Block">Blocks</see> that are implemented in a Zone of a <see cref="Rock.Model.Site"/> Layout.
        /// </returns>
        public IQueryable<Block> GetByLayoutAndZone( int layoutId, string zone )
        {
            return Queryable()
                .Where( t => 
                    t.LayoutId == layoutId && 
                    string.Compare(t.Zone, zone ) == 0)
                .OrderBy( t => t.Order );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Block">Blocks</see> that included on all pages of the Site
        /// </summary>
        /// <param name="siteId">An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Site"/> that the block belongs to.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Block">Blocks</see> that are implemented as part of the provided site.
        /// </returns>
        public IQueryable<Block> GetBySite( int siteId )
        {
            return Queryable()
                .Where( t => t.SiteId == siteId )
                .OrderBy( t => t.Zone ).ThenBy( t => t.Order );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Block">Blocks</see> that are implemented all pages of a site for the specific zone. 
        /// </summary>
        /// <param name="siteId">An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Site"/> that the block belongs to.</param>
        /// <param name="zone">A <see cref="System.String"/> representing the name of the Zone to search by.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Block">Blocks</see> that are implemented in a Zone on all pages of a <see cref="Rock.Model.Site"/> .
        /// </returns>
        public IQueryable<Block> GetBySiteAndZone( int siteId, string zone )
        {
            return Queryable()
                .Where( t =>
                    t.SiteId == siteId &&
                    string.Compare( t.Zone, zone ) == 0 )
                .OrderBy( t => t.Order );
        }

        /// <summary>
        /// Returns a collection of <see cref="Rock.Model.Block">Blocks</see> that are implemented on a specific page.
        /// </summary>
        /// <param name="pageId">An <see cref="System.Int32"/> representing the Id of a <see cref="Page"/> that a <see cref="Block"/> may be implemented on.</param>
        /// <returns>An enumerable collection of <see cref="Block">Blocks</see> that are implemented on the <see cref="Rock.Model.Page"/>.</returns>
        public IQueryable<Block> GetByPage( int pageId )
        {
            return Queryable()
                .Where( t => t.PageId == pageId )
                .OrderBy( t => t.Order );
        }

        /// <summary>
        /// Returns a collection of <see cref="Rock.Model.Block">Blocks</see> that are implemented in a Zone on a specific page.
        /// </summary>
        /// <param name="pageId">An <see cref="System.Int32"/> that represents the Id of <see cref="Rock.Model.Page"/> that a <see cref="Rock.Model.Block"/> may be implemented on.</param>
        /// <param name="zone">A <see cref="System.String"/> that represents the name of a page/layout zone that a <see cref="Rock.Model.Block"/> may be implemented on.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.Block">Blocks</see> that are implemented in a specific Zone of a <see cref="Rock.Model.Page"/>.</returns>
        public IQueryable<Block> GetByPageAndZone( int pageId, string zone )
        {
            return Queryable()
                .Where( t =>
                    t.PageId == pageId &&
                    string.Compare( t.Zone, zone ) == 0 )
                .OrderBy( t => t.Order );
        }

        /// <summary>
        /// Returns the next available position for a <see cref="Rock.Model.Block"/> in a given Zone.
        /// </summary>
        /// <param name="block">A <see cref="Rock.Model.Block"/> entity object.</param>
        /// <returns>An <see cref="System.Int32"/> that contains the next available position for a <see cref="Rock.Model.Block"/> in a Zone</returns>
        public int GetMaxOrder( Block block )
        {
            Block existingBlock = Get( block.Id );

            int? order = Queryable()
                .Where( b =>
                    b.SiteId == block.SiteId &&
                    b.LayoutId == block.LayoutId &&
                    b.PageId == block.PageId &&
                    b.Zone == block.Zone )
                .Select( b => ( int? )b.Order ).Max();
                
            return order.HasValue ? order.Value + 1 : 0;
        }

        /// <summary>
        /// Gets the Guid for the Block that has the specified Id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Guid? GetGuid( int id )
        {
            var cacheItem = BlockCache.Get( id );
            if ( cacheItem != null )
            {
                return cacheItem.Guid;
            }

            return null;
        }
    }
}

//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// The data access/service class for <see cref="Block"/> entity type objects that extends the functionality of <see cref="Rock.Model.Service"/>
    /// </summary>
    public partial class BlockService 
    {
        /// <summary>
        /// Returns an enmerable collection of <see cref="Block">Blocks</see> that implement a specific <see cref="BlockType"/>.
        /// </summary>
        /// <param name="blockTypeId">The Id of a <see cref="BlockType"/>.</param>
        /// <returns>An enumerable collection of <see cref="Block"/> entity objects that implmeneted the referenced <see cref="BlockType"/>.</returns>
        public IEnumerable<Block> GetByBlockTypeId( int blockTypeId )
        {
            return Repository
                .Find( t => 
                    t.BlockTypeId == blockTypeId )
                .OrderBy( t => t.Order );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Block">Blocks</see> that are implemented as part of a <see cref="Site" /> layout.
        /// </summary>
        /// <param name="siteId">An <see cref="System.Int"/> representing the Id of the <see cref="Site"/> that the Layout belongs to.</param>
        /// <param name="layout">A <see cref="System.String"/> representing the name of a layout.</param>
        /// <returns>
        /// An enumerable collection of Blocks that are implmented as part of the provided site layout.
        /// </returns>
        public IEnumerable<Block> GetByLayout( int siteId, string layout )
        {
            return Repository
                .Find( t => 
                    t.SiteId == siteId && 
                    string.Compare(t.Layout, layout, true) == 0 )
                .OrderBy( t => t.Order );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Block">Blocks</see> that are implmented in a speicific zone on a site layout.
        /// </summary>
        /// <param name="siteId">A <see cref="System.Int"/> representing the Id of the <see cref="Site"/> that the Layout belongs to.</param>
        /// <param name="layout">A <see cref="System.String"/> representing the name of the Layout.</param>
        /// <param name="zone">A <see cref="System.String"/> representing the name of the Zone to search by.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Block">Blocks</see> that are implmented in a Zone of a site layout.
        /// </returns>
        public IEnumerable<Block> GetByLayoutAndZone( int siteId, string layout, string zone )
        {
            return Repository
                .Find( t => 
                    t.SiteId == siteId && 
                    string.Compare(t.Layout, layout, true) == 0 && 
                    string.Compare(t.Zone, zone ) == 0)
                .OrderBy( t => t.Order );
        }

        /// <summary>
        /// Returns a collection of <see cref="Block">Blocks</see> that are implemented on a specific page.
        /// </summary>
        /// <param name="pageId">An <see cref="System.Int"/> representing the Id of a <see cref="Page"/> that a <see cref="Block"/> may be implmeneted on.</param>
        /// <returns>An enumerable collection of <see cref="Block">Blocks</see> that are implemented on the page.</returns>
        public IEnumerable<Block> GetByPage( int pageId )
        {
            return Repository
                .Find( t =>
                    t.PageId == pageId )
                .OrderBy( t => t.Order );
        }

        /// <summary>
        /// Returns a collection of <see cref="Block">Blocks</see> that are implmented in a Zone on a specific page.
        /// </summary>
        /// <param name="pageId">An <see cref="System.Int"/> that represents the Id of <see cref="Page"/> that a <see cref="Block"/> may be implemented on.</param>
        /// <param name="zone">A <see cref="System.String"/> that represents the name of a page/layout zone that a <see cref="Block"/> may be implemented on.</param>
        /// <returns>An enumerable collection of <see cref="Block">Blocks</see> that are implemented in a specific Zone of a <see cref="Page"/>.</returns>
        public IEnumerable<Block> GetByPageAndZone( int pageId, string zone )
        {
            return Repository
                .Find( t =>
                    t.PageId == pageId &&
                    string.Compare( t.Zone, zone ) == 0 )
                .OrderBy( t => t.Order );
        }

        /// <summary>
        /// Returns the next avaialble position for a <see cref="Block"/> in a given Zone.
        /// </summary>
        /// <param name="block">A <see cref="Block"/> entity object.</param>
        /// <returns>An <see cref="System.Int"/> that contains the next available position for a <see cref="Block"/> in a Zone</returns>
        public int GetMaxOrder( Block block )
        {
            Block existingBlock = Get( block.Id );

            int? order = Queryable()
                .Where( b => 
                    b.SiteId == block.SiteId &&
                    b.Layout == block.Layout &&
                    b.PageId == block.PageId &&
                    b.Zone == block.Zone )
                .Select( b => ( int? )b.Order ).Max();
                
            return order.HasValue ? order.Value + 1 : 0;
        }
    }
}

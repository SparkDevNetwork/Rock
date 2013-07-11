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
    /// Block POCO Service class
    /// </summary>
    public partial class BlockService 
    {
        /// <summary>
        /// Gets Blocks by Block Type Id
        /// </summary>
        /// <param name="blockTypeId">Block Type Id.</param>
        /// <returns>An enumerable list of Block objects.</returns>
        public IEnumerable<Block> GetByBlockTypeId( int blockTypeId )
        {
            return Repository
                .Find( t => 
                    t.BlockTypeId == blockTypeId )
                .OrderBy( t => t.Order );
        }
        
        /// <summary>
        /// Gets Block Instances by Layout
        /// </summary>
        /// <param name="layout">Layout.</param>
        /// <returns>An enumerable list of Block objects.</returns>
        public IEnumerable<Block> GetByLayout( int siteId, string layout )
        {
            return Repository
                .Find( t => 
                    t.SiteId == siteId && 
                    string.Compare(t.Layout, layout, true) == 0 )
                .OrderBy( t => t.Order );
        }

        public IEnumerable<Block> GetByLayoutAndZone( int siteId, string layout, string zone )
        {
            return Repository
                .Find( t => 
                    t.SiteId == siteId && 
                    string.Compare(t.Layout, layout, true) == 0 && 
                    string.Compare(t.Zone, zone ) == 0)
                .OrderBy( t => t.Order );
        }

        public IEnumerable<Block> GetByPage( int pageId )
        {
            return Repository
                .Find( t =>
                    t.PageId == pageId )
                .OrderBy( t => t.Order );
        }

        public IEnumerable<Block> GetByPageAndZone( int pageId, string zone )
        {
            return Repository
                .Find( t =>
                    t.PageId == pageId &&
                    string.Compare( t.Zone, zone ) == 0 )
                .OrderBy( t => t.Order );
        }

        /// <summary>
        /// Moves the specified block to another zone.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <returns></returns>
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

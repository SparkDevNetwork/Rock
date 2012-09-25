//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Cms
{
	/// <summary>
	/// Block POCO Service class
	/// </summary>
    public partial class BlockService : Service<Block, BlockDto>
    {
		/// <summary>
		/// Gets Blocks by Block Type Id
		/// </summary>
		/// <param name="blockTypeId">Block Type Id.</param>
		/// <returns>An enumerable list of Block objects.</returns>
	    public IEnumerable<Block> GetByBlockTypeId( int blockTypeId )
        {
            return Repository.Find( t => t.BlockTypeId == blockTypeId ).OrderBy( t => t.Order );
        }
		
		/// <summary>
		/// Gets Block Instances by Layout
		/// </summary>
		/// <param name="layout">Layout.</param>
		/// <returns>An enumerable list of Block objects.</returns>
	    public IEnumerable<Block> GetByLayout( string layout )
        {
            return Repository.Find( t => ( t.Layout == layout || ( layout == null && t.Layout == null ) ) ).OrderBy( t => t.Order );
        }
		
		/// <summary>
		/// Gets Block Instances by Layout And Page Id And Zone
		/// </summary>
		/// <param name="layout">Layout.</param>
		/// <param name="pageId">Page Id.</param>
		/// <param name="zone">Zone.</param>
		/// <returns>An enumerable list of Block objects.</returns>
	    public IEnumerable<Block> GetByLayoutAndPageIdAndZone( string layout, int? pageId, string zone )
        {
            return Repository.Find( t => ( t.Layout == layout || ( layout == null && t.Layout == null ) ) && ( t.PageId == pageId || ( pageId == null && t.PageId == null ) ) && t.Zone == zone ).OrderBy( t => t.Order );
        }
		
		/// <summary>
		/// Gets Block Instances by Page Id
		/// </summary>
		/// <param name="pageId">Page Id.</param>
		/// <returns>An enumerable list of Block objects.</returns>
	    public IEnumerable<Block> GetByPageId( int? pageId )
        {
            return Repository.Find( t => ( t.PageId == pageId || ( pageId == null && t.PageId == null ) ) ).OrderBy( t => t.Order );
        }

        /// <summary>
        /// Moves the specified block to another zone.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <returns></returns>
        public int Move( Block block )
        {
            Block existingBlock = Get( block.Id );

            int? order = Queryable().
                            Where( b => b.Layout == block.Layout &&
                                b.PageId == block.PageId &&
                                b.Zone == block.Zone ).
                            Select( b => ( int? )b.Order ).Max();

            block.Order = order.HasValue ? order.Value + 1 : 0;

            return block.Order;
        }
    }
}

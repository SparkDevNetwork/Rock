//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Rock.Models.Cms;

namespace Rock.Services.Cms
{
	public partial class BlockInstanceService
	{
        /// <summary>
        /// Moves the specified block instance to another zone.
        /// </summary>
        /// <param name="blockInstance">The block instance.</param>
        /// <returns></returns>
        public int Move( Rock.Models.Cms.BlockInstance blockInstance )
        {
            Rock.Models.Cms.BlockInstance existingBlockInstance = Get( blockInstance.Id );

            int? order = Queryable().
                            Where( b => b.Layout == blockInstance.Layout &&
                                b.PageId == blockInstance.PageId &&
                                b.Zone == blockInstance.Zone ).
                            Select( b => ( int? )b.Order ).Max();

            blockInstance.Order = order.HasValue ? order.Value + 1 : 0;

            return blockInstance.Order;
        }
	}
}
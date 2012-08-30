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
	/// Page Route POCO Service class
	/// </summary>
    public partial class PageRouteService : Service<PageRoute, PageRouteDto>
    {
		/// <summary>
		/// Gets Page Routes by Page Id
		/// </summary>
		/// <param name="pageId">Page Id.</param>
		/// <returns>An enumerable list of PageRoute objects.</returns>
	    public IEnumerable<PageRoute> GetByPageId( int pageId )
        {
            return Repository.Find( t => t.PageId == pageId );
        }
    }
}

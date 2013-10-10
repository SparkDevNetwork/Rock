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
    /// The data access/service class for the <see cref="Rock.Model.PageRoute"/> class.
    /// </summary>
    public partial class PageRouteService
    {
        /// <summary>
        /// Gets an enumerable list of <see cref="Rock.Model.PageRoute"/> entities that are linked to a <see cref="Rock.Model.Page"/> by the 
        /// by the <see cref="Rock.Model.Page">Page's</see> Id.
        /// </summary>
        /// <param name="pageId">An <see cref="System.Int32"/> value containing the Id of the <see cref="Rock.Model.Page"/> .</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.PageRoute"/> entities that reference the supplied PageId.</returns>
        public IEnumerable<PageRoute> GetByPageId( int pageId )
        {
            return Repository.Find( t => t.PageId == pageId );
        }
    }
}

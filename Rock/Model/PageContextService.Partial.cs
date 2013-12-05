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
    /// Data access and service class for the <see cref="Rock.Model.PageContext"/> model object. This class inherits from the Service class.
    /// </summary>
    public partial class PageContextService 
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.PageContext"/> entities that are used on a page.
        /// </summary>
        /// <param name="pageId">An <see cref="System.Int32"/> that contains the Id of the <see cref="Rock.Model.Page"/> to search by.</param>
        /// <returns>An enumerable list of <see cref="Rock.Model.PageContext">PageContexts</see> that are referenced on the page.</returns>
        public IEnumerable<PageContext> GetByPageId( int pageId )
        {
            return Repository.Find( t => t.PageId == pageId );
        }
    }
}

//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Page POCO Service class
    /// </summary>
    public partial class PageService 
    {
        /// <summary>
        /// Gets Pages by Parent Page Id
        /// </summary>
        /// <param name="parentPageId">Parent Page Id.</param>
        /// <returns>An enumerable list of Page objects.</returns>
        public IEnumerable<Page> GetByParentPageId( int? parentPageId )
        {
            return Repository.Find( t => ( t.ParentPageId == parentPageId || ( parentPageId == null && t.ParentPageId == null ) ) ).OrderBy( t => t.Order );
        }
        
        /// <summary>
        /// Gets Pages by Site Id
        /// </summary>
        /// <param name="siteId">Site Id.</param>
        /// <returns>An enumerable list of Page objects.</returns>
        public IEnumerable<Page> GetBySiteId( int? siteId )
        {
            return Repository.Find( t => ( t.SiteId == siteId || ( siteId == null && t.SiteId == null ) ) ).OrderBy( t => t.Order );
        }
    }
}

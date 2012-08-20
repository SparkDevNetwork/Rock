//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.CMS
{
	/// <summary>
	/// Site POCO Service class
	/// </summary>
    public partial class SiteService : Service<Site, DTO.Site>
    {
		/// <summary>
		/// Gets Sites by Default Page Id
		/// </summary>
		/// <param name="defaultPageId">Default Page Id.</param>
		/// <returns>An enumerable list of Site objects.</returns>
	    public IEnumerable<Site> GetByDefaultPageId( int? defaultPageId )
        {
            return Repository.Find( t => ( t.DefaultPageId == defaultPageId || ( defaultPageId == null && t.DefaultPageId == null ) ) );
        }

        /// <summary>
        /// Query DTO objects
        /// </summary>
        /// <returns>A queryable list of related DTO objects.</returns>
        public override IQueryable<DTO.Site> QueryableDTO()
        {
            throw new System.NotImplementedException();
        }
    }
}

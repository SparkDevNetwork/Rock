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
    /// Category Service class
    /// </summary>
    public partial class CategoryService
    {
        /// <summary>
        /// Gets Categories for the given Entity Type
        /// </summary>
        /// <param name="categoryId">The category id.</param>
        /// <returns></returns>
        public IEnumerable<Category> GetByEntityTypeId( int? entityTypeId )
        {
            return Repository.Find( t => ( t.EntityTypeId == entityTypeId || ( !entityTypeId.HasValue ) ) );// TODO - do categories need an order? as in: .OrderBy( t => t.Order );
        }
    }
}

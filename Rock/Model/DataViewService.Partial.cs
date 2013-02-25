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
    /// DataView POCO Service class
    /// </summary>
    public partial class DataViewService 
    {
        /// <summary>
        /// Gets the entity types that have existing dataviews
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Rock.Model.EntityType> GetAvailableEntityTypes()
        {
            return Repository.AsQueryable()
                .Select( d => d.EntityType )
                .Distinct();
        }

        /// <summary>
        /// Gets the by entity type id.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <returns></returns>
        public IEnumerable<Rock.Model.DataView> GetByEntityTypeId( int entityTypeId )
        {
            return Repository.AsQueryable()
                .Where( d => d.EntityTypeId == entityTypeId )
                .OrderBy( d => d.Name );
        }
       
    }
}

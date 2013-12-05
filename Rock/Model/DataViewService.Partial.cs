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
    /// DataView Service and Data access class
    /// </summary>
    public partial class DataViewService 
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.EntityType">EntityTypes</see> that have a DataView associated with them.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="Rock.Model.EntityType">EntityTypes</see> that have a <see cref="Rock.Model.DataView" /> associated with them.</returns>
        public IEnumerable<Rock.Model.EntityType> GetAvailableEntityTypes()
        {
            return Repository.AsQueryable()
                .Select( d => d.EntityType )
                .Distinct();
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.DataView">DataViews</see> that are associated with a specified <see cref="Rock.Model.EntityType"/>.
        /// </summary>
        /// <param name="entityTypeId">A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.DataView">DataViews</see> that are associated with the specified <see cref="Rock.Model.EntityType"/>.</returns>
        public IEnumerable<Rock.Model.DataView> GetByEntityTypeId( int entityTypeId )
        {
            return Repository.AsQueryable()
                .Where( d => d.EntityTypeId == entityTypeId )
                .OrderBy( d => d.Name );
        }
       
    }
}

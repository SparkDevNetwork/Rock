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
    /// Data access/service class for <see cref="Rock.Model.PrayerRequest"/> entity objects.
    /// </summary>
    public partial class PrayerRequestService
    {
        /// <summary>
        /// Returns an enumerable collection of the <see cref="Rock.Model.PrayerRequest">PrayerRequests</see> that are in a specified <see cref="Rock.Model.Category"/> or any of it's subcategories.
        /// </summary>
        /// <param name="categoryId">A <see cref="System.Int32"/> representing the CategoryId of the <see cref="Rock.Model.Category"/> to retrieve PrayerRequests for.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.PrayerRequests"/> that are in the specified <see cref="Rock.Model.Category"/> or any of it's subcategories.</returns>
        public IEnumerable<PrayerRequest> GetByCategoryId( int categoryId )
        {
            PrayerRequest prayerRequest = new PrayerRequest();
            Type type = prayerRequest.GetType();

            // Not sure if I can/should rely on the Rock.Web.* here.  Is there a more correct way?
            var prayerRequestEntityTypeId = Rock.Web.Cache.EntityTypeCache.GetId( type );

            return GetByCategoryId( prayerRequestEntityTypeId, categoryId );
         }

        /// <summary>
        /// Gets an enumerable collection of  PrayerRequests by category id (for the prayer request entity type id
        /// </summary>
        /// <param name="prayerRequestEntityTypeId">A <see cref="System.Int32" /> representing the The prayer request entity type id.</param>
        /// <param name="categoryId">A <see cref="System.Int32"/> representing the CategoryId </param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.PrayerRequest">PrayerRequests</see> that match the search criteria.</returns>
        public IEnumerable<PrayerRequest> GetByCategoryId( int? prayerRequestEntityTypeId, int categoryId )
        {
            // Get all the category Ids for the PrayerRequest entity and that are the parent or child of the given categoryId.
            CategoryService categoryService = new CategoryService();
            IEnumerable<int> categoryIds = categoryService.GetByEntityTypeId( prayerRequestEntityTypeId ).Where( c => c.Id == categoryId || c.ParentCategoryId == categoryId ).Select( a => a.Id );

            // Now find the PrayerRequests that have any of those category Ids.
            return Repository.Find( t => categoryIds.Contains( t.CategoryId ?? -1 ) );
        }
    }
}

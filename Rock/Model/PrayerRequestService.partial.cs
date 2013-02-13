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
    /// PrayerRequest Service class
    /// </summary>
    public partial class PrayerRequestService
    {
        /// <summary>
        /// Gets Prayer Requests which either have the given categoryId or are under (subcategory) that categoryId.
        /// </summary>
        /// <param name="categoryId">The category id.</param>
        /// <returns></returns>
        public IEnumerable<PrayerRequest> GetByCategoryId( int categoryId )
        {
            PrayerRequest prayerRequest = new PrayerRequest();
            Type type = prayerRequest.GetType();

            // Not sure if I can/should rely on the Rock.Web.* here.  Is there a more correct way?
            var prayerRequestEntityTypeId = Rock.Web.Cache.EntityTypeCache.GetId( type.FullName );

            return GetByCategoryId( prayerRequestEntityTypeId, categoryId );
         }

        /// <summary>
        /// Gets the PrayerRequests by category id (for the prayer request entity type id).
        /// </summary>
        /// <param name="prayerRequestEntityTypeId">The prayer request entity type id.</param>
        /// <param name="categoryId">The category id.</param>
        /// <returns></returns>
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

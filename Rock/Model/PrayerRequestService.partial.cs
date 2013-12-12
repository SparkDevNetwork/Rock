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
        /// Returns a collection of active <see cref="Rock.Model.PrayerRequest">PrayerRequests</see> that
        /// are in a specified <see cref="Rock.Model.Category"/> or any of it's subcategories.
        /// </summary>
        /// <param name="categoryIds">A <see cref="System.Collections.Generic.List"/> of
        /// the <see cref="Rock.Model.Category"/> IDs to retrieve PrayerRequests for.</param>
        /// <param name="onlyApproved">set false to include un-approved requests.</param>
        /// <param name="onlyUnexpired">set false to include expired requests.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.PrayerRequest"/> that
        /// are in the specified <see cref="Rock.Model.Category"/> or any of it's subcategories.</returns>
        public IEnumerable<PrayerRequest> GetByCategoryIds( List<int> categoryIds, bool onlyApproved = true, bool onlyUnexpired = true )
        {
            PrayerRequest prayerRequest = new PrayerRequest();
            Type type = prayerRequest.GetType();

            var prayerRequestEntityTypeId = Rock.Web.Cache.EntityTypeCache.GetId( type );

            // Get all PrayerRequest category Ids that are the **parent or child** of the given categoryIds.
            CategoryService categoryService = new CategoryService();
            IEnumerable<int> expandedCategoryIds = categoryService.GetByEntityTypeId( prayerRequestEntityTypeId )
                .Where( c => categoryIds.Contains( c.Id ) || categoryIds.Contains( c.ParentCategoryId ?? -1 ) )
                .Select( a => a.Id );

            // Now find the active PrayerRequests that have any of those category Ids.
            var list = Repository.Find( p => p.IsActive == true && categoryIds.Contains( p.CategoryId ?? -1 ) );

            if ( onlyApproved )
            {
                list = list.Where( p => p.IsApproved == true );
            }

            if ( onlyUnexpired )
            {
                list = list.Where( p => DateTime.Today <= p.ExpirationDate );
            }

            return list;
        }

        /// <summary>
        /// Returns a active, approved, unexpired <see cref="Rock.Model.PrayerRequest">PrayerRequests</see>
        /// order by urgency and then by total prayer count.
        /// </summary>
        /// <returns>A queryable collection of <see cref="Rock.Model.PrayerRequest">PrayerRequest</see>.</returns>
        public IQueryable<PrayerRequest> GetActiveApprovedUnexpired()
        {
            return Repository.AsQueryable()
                .Where( p => p.IsActive == true && p.IsApproved == true && DateTime.Today <= p.ExpirationDate )
                .OrderByDescending( p => p.IsUrgent ).ThenBy( p => p.PrayerCount );
        }
    }
}

// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
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
        /// are in a specified <see cref="Rock.Model.Category"/> or any of its subcategories.
        /// </summary>
        /// <param name="categoryIds">A <see cref="System.Collections.Generic.List{Int32}"/> of
        /// the <see cref="Rock.Model.Category"/> IDs to retrieve PrayerRequests for.</param>
        /// <param name="onlyApproved">set false to include un-approved requests.</param>
        /// <param name="onlyUnexpired">set false to include expired requests.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.PrayerRequest"/> that
        /// are in the specified <see cref="Rock.Model.Category"/> or any of its subcategories.</returns>
        public IEnumerable<PrayerRequest> GetByCategoryIds( List<int> categoryIds, bool onlyApproved = true, bool onlyUnexpired = true )
        {
            PrayerRequest prayerRequest = new PrayerRequest();
            Type type = prayerRequest.GetType();

            var prayerRequestEntityTypeId = Rock.Web.Cache.EntityTypeCache.GetId( type );

            // Get all PrayerRequest category Ids that are the **parent or child** of the given categoryIds.
            CategoryService categoryService = new CategoryService( (RockContext)Context );
            IEnumerable<int> expandedCategoryIds = categoryService.GetByEntityTypeId( prayerRequestEntityTypeId )
                .Where( c => categoryIds.Contains( c.Id ) || categoryIds.Contains( c.ParentCategoryId ?? -1 ) )
                .Select( a => a.Id );

            // Now find the active PrayerRequests that have any of those category Ids.
            var list = Queryable( "RequestedByPersonAlias.Person" ).Where( p => p.IsActive == true && categoryIds.Contains( p.CategoryId ?? -1 ) );

            if ( onlyApproved )
            {
                list = list.Where( p => p.IsApproved == true );
            }

            if ( onlyUnexpired )
            {
                list = list.Where( p => RockDateTime.Today <= p.ExpirationDate );
            }

            return list;
        }

        /// <summary>
        /// Returns a active, approved, unexpired <see cref="Rock.Model.PrayerRequest">PrayerRequests</see>
        /// order by urgency and then by total prayer count.
        /// </summary>
        /// <returns>A queryable collection of <see cref="Rock.Model.PrayerRequest">PrayerRequest</see>.</returns>
        public IOrderedQueryable<PrayerRequest> GetActiveApprovedUnexpired()
        {
            return Queryable()
                .Where( p => p.IsActive == true && p.IsApproved == true && RockDateTime.Today <= p.ExpirationDate )
                .OrderByDescending( p => p.IsUrgent ).ThenBy( p => p.PrayerCount );
        }
    }
}

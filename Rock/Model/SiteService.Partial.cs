// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Linq;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// The data access/service class for the <see cref="Rock.Model.Site"/> entity. This inherits from the Service class
    /// </summary>
    public partial class SiteService 
    {
        /// <summary>
        /// Returns a collection of <see cref="Rock.Model.Site"/> entities that by their Default <see cref="Rock.Model.Page">Page's</see> PageId.
        /// </summary>
        /// <param name="defaultPageId">An <see cref="System.Int32"/> containing the Id of the default <see cref="Rock.Model.Page"/> to search by. This
        /// value is nullable.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.Site"/> entities that use reference the provided PageId.</returns>
        public IQueryable<Site> GetByDefaultPageId( int? defaultPageId )
        {
            return Queryable().Where( t => ( t.DefaultPageId == defaultPageId || ( defaultPageId == null && t.DefaultPageId == null ) ) );
        }

        /// <summary>
        /// Determines whether the specified site can be deleted.
        /// Performs some additional checks that are missing from the
        /// auto-generated SiteService.CanDelete().
        /// TODO This should move into the SiteService CanDelete at some point
        /// once the generator tool is adjusted.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="includeSecondLvl">If set to true, verifies that there are no site layouts with any existing pages.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( Site item, out string errorMessage, bool includeSecondLvl )
        {
            errorMessage = string.Empty;

            bool canDelete = CanDelete( item, out errorMessage );

            if ( canDelete && includeSecondLvl && new Service<Layout>( (RockContext)Context ).Queryable().Where( l => l.SiteId == item.Id ).Any( a => a.Pages.Count() > 0 ) )
            {
                errorMessage = string.Format( "This {0} has a {1} which is used by a {2}.", Site.FriendlyTypeName, Layout.FriendlyTypeName, Page.FriendlyTypeName );
                canDelete = false;
            }

            return canDelete;
        }

        /// <summary>
        /// Gets the Guid for the Site that has the specified Id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Guid? GetGuid( int id )
        {
            var cacheItem = SiteCache.Get( id );
            if ( cacheItem != null )
            {
                return cacheItem.Guid;
            }

            return null;
        }


        /// <summary>
        /// Gets the domain URI.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <returns></returns>
        public Uri GetDefaultDomainUri( int siteId )
        {
            var site = this.Get( siteId );
            if ( site != null )
            {
                return site.DefaultDomainUri;
            }

            return new Uri( GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ) );
        }
    }
}

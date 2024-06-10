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
using System.Collections.Generic;
using System.Linq;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.TestData;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Cms
{
    /// <summary>
    /// Provides actions to manage Website data.
    /// </summary>
    public class WebsiteDataManager
    {
        private static Lazy<WebsiteDataManager> _dataManager = new Lazy<WebsiteDataManager>();
        public static WebsiteDataManager Current => _dataManager.Value;

        #region PageShortLink

        /// <summary>
        /// Create a new instance with the minimum required fields.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="token"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public PageShortLink CreatePageShortLink( int siteId, string token, string url )
        {
            var newEntity = new PageShortLink()
            {
                SiteId = siteId,
                Token = token,
                Url = url
            };
            return newEntity;
        }

        /// <summary>
        /// Remove an instance.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public bool DeletePageShortLink( string identifier, RockContext rockContext )
        {
            var dataContext = rockContext ?? new RockContext();

            var service = new PageShortLinkService( dataContext );

            var entity = service.Get( identifier );
            if ( entity == null )
            {
                return false;
            }

            var result = service.Delete( entity );

            if ( rockContext == null )
            {
                dataContext.SaveChanges();
            }

            return result;
        }

        /// <summary>
        /// Save a new instance to the data store.
        /// </summary>
        /// <param name="shortlink"></param>
        /// <returns></returns>
        public void SavePageShortLink( PageShortLink shortlink, CreateExistingItemStrategySpecifier existingItemStrategy = CreateExistingItemStrategySpecifier.Replace )
        {
            var rockContext = new RockContext();

            rockContext.WrapTransaction( () =>
            {
                var service = new PageShortLinkService( rockContext );
                if ( shortlink.Guid != Guid.Empty )
                {
                    var existingEntity = service.Get( shortlink.Guid );
                    if ( existingEntity != null )
                    {
                        if ( existingItemStrategy == CreateExistingItemStrategySpecifier.Fail )
                        {
                            throw new Exception( "Item exists." );
                        }
                        else if ( existingItemStrategy == CreateExistingItemStrategySpecifier.Replace )
                        {
                            var isDeleted = DeletePageShortLink( existingEntity.Guid.ToString(), rockContext );

                            if ( !isDeleted )
                            {
                                throw new Exception( "Could not replace existing item." );
                            }
                        }
                    }
                }

                service.Add( shortlink );

                rockContext.SaveChanges();
            } );
        }

        #endregion

        #region Site

        /// <summary>
        /// Get the internal site.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        public static Site GetInternalSite( RockContext rockContext = null )
        {
            rockContext = TestDataHelper.GetActiveRockContext( rockContext );
            var siteService = new SiteService( rockContext );

            var internalSite = siteService.Get( SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid() );
            return internalSite;
        }

        /// <summary>
        /// Get the list of pages associated with the internal site.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        public static List<Page> GetInternalSitePages( RockContext rockContext = null )
        {
            rockContext = TestDataHelper.GetActiveRockContext( rockContext );
            var pageService = new PageService( rockContext );

            var internalSite = GetInternalSite( rockContext );
            var pages = pageService.Queryable()
                .Where( p => p.Layout != null && p.Layout.SiteId == internalSite.Id )
                .ToList();

            return pages;
        }

        #endregion
    }
}

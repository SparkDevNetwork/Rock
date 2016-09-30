// <copyright>
// Copyright by the Spark Development Network
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
using System.Text;
using System.Web;
using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.UniversalSearch.Crawler;
using Rock.Web.Cache;
using Rock.UniversalSearch.IndexModels;
using Rock.UniversalSearch;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to run quick SQL queries on a schedule
    /// </summary>
    [DisallowConcurrentExecution]

    [SiteField("Site", "The site that will be indexed", true, order: 0)]
    [TextField("Starting URL", "The URL to start the index from.", true, key: "StartingUrl", order: 1)]
    public class IndexRockSite : IJob
    {
        private int _indexedPageCount = 0;
        private SiteCache _site;
        
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public IndexRockSite()
        {
        }

        /// <summary>
        /// Job that will run quick SQL queries on a schedule.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var siteGuid = dataMap.GetString( "Site" ).AsIntegerOrNull();
            var startingUrlAttrib = dataMap.GetString( "StartingUrl" );

            Uri startingUri;

            if ( siteGuid.HasValue )
            {
                _site = SiteCache.Read( siteGuid.Value );

                if ( _site != null )
                {

                    if ( Uri.TryCreate( startingUrlAttrib, UriKind.Absolute, out startingUri ) && (startingUri.Scheme == Uri.UriSchemeHttp || startingUri.Scheme == Uri.UriSchemeHttps) )
                    {
                        // ensure that an index is configured for site pages, if not create it
                        IndexContainer.CreateIndex( typeof( SitePageIndex ), false );

                        // release the crawler, like the kraken... but not...
                        //var pages = new Crawler().CrawlSite( startingUri.ToString(), PageCallback );

                        //context.Result = string.Format("Crawler found {0} pages, {1} pages sent to be indexed.", pages, _indexedPageCount);
                    }
                    else
                    {
                        context.Result = "An invalid starting URL was provided.";
                    }
                }
                else
                {
                    context.Result = "Could not locate the site provided.";
                }
            }
            else
            {
                context.Result = "An invalid site was provided.";
            }

            
        }

        /// <summary>
        /// This method will be called each time a page is found by the crawler.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public bool PageCallback( CrawledPage page)
        {
            if ( page.AllowsIndex )
            {
                // clean up the page title a bit by removing  the site name off it
                var pageTitle = page.Title.Substring( 0, (page.Title.IndexOf( '|' ) - 1) ).Trim();

                SitePageIndex sitePage = new SitePageIndex();
                sitePage.Id = page.Url.MakeInt64HashCode();
                sitePage.Content = page.Text;
                sitePage.PageTitle = pageTitle;
                sitePage.SiteName = _site.Name;
                sitePage.SiteId = _site.Id;
                sitePage.Url = page.Url;
                sitePage.LastIndexedDateTime = RockDateTime.Now;

                IndexContainer.IndexDocument( sitePage );

                _indexedPageCount++;
                return true;
            }

            return false;
        }
    }
}

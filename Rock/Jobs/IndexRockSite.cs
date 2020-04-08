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

using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.UniversalSearch;
using Rock.UniversalSearch.Crawler;
using Rock.UniversalSearch.IndexModels;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to run quick SQL queries on a schedule
    /// </summary>
    [DisallowConcurrentExecution]

    [SiteField( "Site", "The site that will be indexed", true, order: 0 )]
    [TextField( "Login Id", "The login to impersonate when navigating to secured pages. Leave blank if secured pages should not be indexed.", false, "", "", 1, "LoginId" )]
    [TextField( "Password", "The password associated with the Login Id.", false, "", "", 2, "Password", true )]

    public class IndexRockSite : IJob
    {
        private int _indexedPageCount = 0;
        private Site _site;

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
            var siteId = dataMap.GetString( "Site" ).AsIntegerOrNull();
            string loginId = dataMap.GetString( "LoginId" );
            string password = dataMap.GetString( "Password" );

            Uri startingUri;

            if ( siteId.HasValue )
            {
                _site = new SiteService( new RockContext() ).Get( siteId.Value );

                if ( _site != null )
                {
                    var startingUrl = _site.IndexStartingLocation;

                    if ( Uri.TryCreate( startingUrl, UriKind.Absolute, out startingUri ) && ( startingUri.Scheme == Uri.UriSchemeHttp || startingUri.Scheme == Uri.UriSchemeHttps ) )
                    {
                        // ensure that an index is configured for site pages, if not create it
                        IndexContainer.CreateIndex( typeof( SitePageIndex ), false );

                        // release the crawler, like the kraken... but not...
                        var pages = new Crawler().CrawlSite( _site, loginId, password );

                        context.Result = string.Format( "Crawler indexed {0} pages.", pages, _indexedPageCount );
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
    }
}

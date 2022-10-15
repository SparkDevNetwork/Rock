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
using System.ComponentModel;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.UniversalSearch;
using Rock.UniversalSearch.Crawler;
using Rock.UniversalSearch.IndexModels;

namespace Rock.Jobs
{
    /// <summary>
    /// This job indexes the specified site.
    /// </summary>
    [DisplayName( "Index Rock Site" )]
    [Description( "This job indexes the specified site." )]


    [SiteField( "Site", "The site that will be indexed", true, order: 0 )]
    [TextField( "Login Id", "The login to impersonate when navigating to secured pages. Leave blank if secured pages should not be indexed.", false, "", "", 1, "LoginId" )]
    [TextField( "Password", "The password associated with the Login Id.", false, "", "", 2, "Password", true )]

    public class IndexRockSite : RockJob
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

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            var siteId = GetAttributeValue( "Site" ).AsIntegerOrNull();
            string loginId = GetAttributeValue( "LoginId" );
            string password = GetAttributeValue( "Password" );

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

                        this.Result = string.Format( "Crawler indexed {0} pages.", pages, _indexedPageCount );
                    }
                    else
                    {
                        this.Result = "An invalid starting URL was provided.";
                    }
                }
                else
                {
                    this.Result = "Could not locate the site provided.";
                }
            }
            else
            {
                this.Result = "An invalid site was provided.";
            }


        }
    }
}

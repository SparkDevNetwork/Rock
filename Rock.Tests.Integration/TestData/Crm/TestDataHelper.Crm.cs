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
using System.Collections.Generic;
using Rock.Data;
using Rock.Model;
using System.Linq;

namespace Rock.Tests.Integration
{
    public static partial class TestDataHelper
    {
        public static class Crm
        {
            public static Site GetInternalSite( RockContext rockContext = null )
            {
                rockContext = GetActiveRockContext( rockContext );
                var siteService = new SiteService( rockContext );

                var internalSite = siteService.Get( SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid() );
                return internalSite;
            }

            public static List<Page> GetInternalSitePages( RockContext rockContext = null )
            {
                rockContext = GetActiveRockContext( rockContext );
                var pageService = new PageService( rockContext );

                var internalSite = GetInternalSite( rockContext );
                var pages = pageService.Queryable()
                    .Where( p => p.Layout != null && p.Layout.SiteId == internalSite.Id )
                    .ToList();

                return pages;
            }
        }
    }
}

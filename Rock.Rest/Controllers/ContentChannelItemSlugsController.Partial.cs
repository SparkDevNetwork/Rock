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
using System.Web.Http;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    ///
    /// </summary>
    public partial class ContentChannelItemSlugsController
    {
        /// <summary>
        /// Posts the content slug.
        /// </summary>
        /// <param name="contentChannelItemId">The content channel item identifier.</param>
        /// <param name="slug">The slug.</param>
        /// <param name="contentChannelItemSlugId">The content channel item slug identifier.</param>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/ContentChannelItemSlugs/SaveContentSlug/{contentChannelItemId}/{slug}/{contentChannelItemSlugId?}" )]
        [Rock.SystemGuid.RestActionGuid( "378CB619-2BF6-41CB-8951-FB75E756246F" )]
        public SaveSlugResponse SaveContentSlug( int contentChannelItemId, string slug, int? contentChannelItemSlugId = null )
        {
            SaveSlugResponse response = new SaveSlugResponse();
            using ( var rockContext = new RockContext() )
            {
                var contentChannelItemSlugService = new ContentChannelItemSlugService( rockContext );

                var contentChannelItemSlug = contentChannelItemSlugService.SaveSlug( contentChannelItemId, slug, contentChannelItemSlugId );
                if ( contentChannelItemSlug != null )
                {
                    response.Slug = contentChannelItemSlug.Slug;
                    response.Id = contentChannelItemSlug.Id;
                }
            }
            return response;
        }

        /// <summary>
        /// Gets the unique slug.
        /// </summary>
        /// <param name="contentChannelItemId">The content channel item identifier.</param>
        /// <param name="slug">The slug.</param>
        /// <param name="contentChannelItemSlugId">The content channel item slug identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/ContentChannelItemSlugs/GetUniqueContentSlug/{contentChannelItemId}/{slug}/{contentChannelItemSlugId?}" )]
        [Rock.SystemGuid.RestActionGuid( "98C1DB14-6693-4AE5-91BF-E2580BA44451" )]
        public string GetUniqueContentSlug( int contentChannelItemId, string slug, int? contentChannelItemSlugId = null )
        {
            string uniqueSlug = string.Empty;

            using ( var rockContext = new RockContext() )
            {
                var contentChannelItemSlugService = new ContentChannelItemSlugService( rockContext );

                uniqueSlug = contentChannelItemSlugService.GetUniqueContentSlug( slug, contentChannelItemSlugId, contentChannelItemId );
            }

            return uniqueSlug ?? string.Empty;
        }

        /// <summary>
        /// Gets the unique slug for the content channel.
        /// </summary>
        /// <param name="contentChannelId">The content channel item identifier.</param>
        /// <param name="slug">The slug.</param>
        /// <param name="contentChannelItemSlugId">The content channel item slug identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/ContentChannelItemSlugs/GetUniqueContentChannelSlug/{contentChannelId}/{slug}/{contentChannelItemSlugId?}" )]
        [Rock.SystemGuid.RestActionGuid( "BD61FABC-679A-490D-B4DC-A81F9C876CCF" )]
        public string GetUniqueContentSlugForContentChannel( int contentChannelId, string slug, int? contentChannelItemSlugId = null )
        {
            string uniqueSlug = string.Empty;

            using ( var rockContext = new RockContext() )
            {
                var contentChannelItemSlugService = new ContentChannelItemSlugService( rockContext );

                uniqueSlug = contentChannelItemSlugService.GetUniqueSlugForContentChannel( slug, contentChannelId, contentChannelItemSlugId );
            }

            return uniqueSlug ?? string.Empty;
        }

        /// <summary>
        /// Return object of the SaveContentSlug action
        /// </summary>
        public class SaveSlugResponse
        {
            /// <summary>
            /// Gets or sets the slug.
            /// </summary>
            /// <value>
            /// The slug.
            /// </value>
            public string Slug { get; set; }

            /// <summary>
            /// Gets or sets the Id.
            /// </summary>
            /// <value>
            /// The Id.
            /// </value>
            public int Id { get; set; }
        }

    }
}
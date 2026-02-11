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
using System.Web;

using Rock.Blocks;
using Rock.Web.Cache;

namespace Rock
{
    /// <summary>
    /// Extension methods for <see cref="RockBlockType"/>
    /// </summary>
    public static class RockBlockTypeExtensions
    {
        /// <summary>
        /// Builds and returns the URL for a linked <see cref="Rock.Model.Page"/>
        /// from a <see cref="Rock.Attribute.LinkedPageAttribute"/> and any necessary
        /// query parameters.
        /// </summary>
        /// <param name="block">The block to get instance data from.</param>
        /// <param name="attributeKey">The attribute key that contains the linked page value.</param>
        /// <param name="queryParams">Any query string parameters that should be included in the built URL.</param>
        /// <returns>A string representing the URL to the linked <see cref="Rock.Model.Page"/>.</returns>
        public static string GetLinkedPageUrl( this RockBlockType block, string attributeKey, IDictionary<string, string> queryParams = null )
        {
            var pageReference = new Rock.Web.PageReference( block.GetAttributeValue( attributeKey ), queryParams != null ? new Dictionary<string, string>( queryParams ) : null );

            if ( pageReference.PageId > 0 )
            {
                return pageReference.BuildUrl();
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Builds and returns the URL for a linked <see cref="Rock.Model.Page"/>
        /// from a <see cref="Rock.Attribute.LinkedPageAttribute"/> and any necessary
        /// query parameters.
        /// </summary>
        /// <param name="block">The block to get instance data from.</param>
        /// <param name="attributeKey">The attribute key that contains the linked page value.</param>
        /// <param name="queryKey">The name key for the single query string parameter.</param>
        /// <param name="queryValue">The value for the query key.</param>
        /// <returns>A string representing the URL to the linked <see cref="Rock.Model.Page"/>.</returns>
        public static string GetLinkedPageUrl( this RockBlockType block, string attributeKey, string queryKey, string queryValue )
        {
            var pageReference = new Rock.Web.PageReference( block.GetAttributeValue( attributeKey ), new Dictionary<string, string>
            {
                [queryKey] = queryValue
            } );

            if ( pageReference.PageId > 0 )
            {
                return pageReference.BuildUrl();
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Builds and returns the URL for the parent <see cref="Rock.Model.Page"/>
        /// from the current page and any necessary query parameters.
        /// </summary>
        /// <param name="block">The block to get instance data from.</param>
        /// <param name="queryParams">Any query string parameters that should be included in the built URL.</param>
        /// <returns>A string representing the URL to the parent <see cref="Rock.Model.Page"/>.</returns>
        public static string GetParentPageUrl( this RockBlockType block, IDictionary<string, string> queryParams = null )
        {
            if ( block.PageCache.ParentPage == null )
            {
                return string.Empty;
            }

            var pageReference = new Rock.Web.PageReference( block.PageCache.ParentPage.Guid.ToString(), queryParams != null ? new Dictionary<string, string>( queryParams ) : null );

            if ( pageReference.PageId > 0 )
            {
                return pageReference.BuildUrl();
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Builds and returns the URL for the current <see cref="Rock.Model.Page"/>
        /// and any necessary query parameters.
        /// </summary>
        /// <param name="block">The block to get instance data from.</param>
        /// <param name="queryParams">
        ///   Any query string parameters that should be included in the built URL.
        ///   If any of these parameters is also supplied by the current page's URL,
        ///   the value from <paramref name="queryParams"/> will be used instead of
        ///   the existing value in the current page's URL.
        /// </param>
        /// <returns>A string representing the URL to the current <see cref="Rock.Model.Page"/>.</returns>
        public static string GetCurrentPageUrl( this RockBlockType block, IDictionary<string, string> queryParams = null )
        {
            return GetCurrentPageUrl( block, queryParams, skipExistingParameters: false );
        }

        /// <summary>
        /// Builds and returns the URL for the current <see cref="Rock.Model.Page"/>
        /// and any necessary query parameters.
        /// </summary>
        /// <param name="block">The block to get instance data from.</param>
        /// <param name="queryParams">
        ///   Any query string parameters that should be included in the built URL.
        ///   If any of these parameters is also supplied by the current page's URL,
        ///   when <paramref name="skipExistingParameters"/> is <see langword="true"/>,
        ///   the value from <paramref name="queryParams"/> will be used instead of
        ///   the existing value in the current page's URL.
        /// </param>
        /// <param name="skipExistingParameters">
        ///  <para>
        ///    If <see langword="true"/>, parameters already included in the current page's URL will be dropped.
        ///    Only parameters supplied by <paramref name="queryParams"/> will be included, if any.
        ///  </para>
        ///  <para>
        ///    If <see langword="false"/>, parameters already included in current page's URL will be included.
        ///    If any of these parameters is also supplied by <paramref name="queryParams"/>, the value from
        ///   <paramref name="queryParams"/> will be used instead of the existing value in the current page's URL.
        ///  </para>
        /// </param>
        /// <returns>A string representing the URL to the current <see cref="Rock.Model.Page"/>.</returns>
        public static string GetCurrentPageUrl( this RockBlockType block, IDictionary<string, string> queryParams, bool skipExistingParameters )
        {
            var parameters = queryParams != null ? new Dictionary<string, string>( queryParams ) : new Dictionary<string, string>();

            if ( !skipExistingParameters )
            {
                foreach ( var qp in block.RequestContext.GetPageParameters() )
                {
                    if ( qp.Key == "PageId" )
                    {
                        continue;
                    }

                    parameters.TryAdd( qp.Key, qp.Value );
                }
            }

            var pageReference = new Rock.Web.PageReference( block.PageCache.Guid.ToString(), parameters );

            if ( pageReference.PageId > 0 )
            {
                return pageReference.BuildUrl();
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Builds and returns the URL for the standard login <see cref="Rock.Model.Page"/>
        /// for a block along with the return URL parameter.
        /// </summary>
        /// <param name="block">The block to get instance data from.</param>
        /// <param name="returnUrl">The URL to be redirected to after login.</param>
        /// <returns>A string representing the URL to the login <see cref="Rock.Model.Page"/>.</returns>
        public static string GetLoginPageUrl( this RockBlockType block, string returnUrl )
        {
            var site = SiteCache.Get( block.PageCache.SiteId );
            var pageReference = new Rock.Web.PageReference( site.LoginPageReference );

            if ( returnUrl.IsNotNullOrWhiteSpace() )
            {
                pageReference.QueryString["returnurl"] = returnUrl;
            }

            if ( pageReference.PageId > 0 )
            {
                return pageReference.BuildUrl();
            }
            else if ( block.RequestContext != null )
            {
                var url = block.RequestContext.ResolveRockUrl( "~/Login" );

                if ( returnUrl.IsNotNullOrWhiteSpace() )
                {
                    url = $"{url}?returnurl={HttpUtility.UrlEncode( returnUrl )}";
                }

                return url;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}

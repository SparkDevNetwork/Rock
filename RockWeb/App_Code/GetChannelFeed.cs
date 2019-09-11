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
using System.Web;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace RockWeb
{
    /// <summary>
    /// Handles retrieving file data from storage
    /// </summary>
    public class GetChannelFeed : IHttpHandler
    {

        private HttpRequest request;
        private HttpResponse response;

        private int rssItemLimit = 10;

        public void ProcessRequest( HttpContext context )
        {
            request = context.Request;
            response = context.Response;

            RockContext rockContext = new RockContext();

            if ( request.HttpMethod != "GET" && request.HttpMethod != "HEAD" )
            {
                response.TrySkipIisCustomErrors = true;
                response.StatusCode = 405;
                response.Headers.Add( "Allow", "GET" );
                response.Write( "Invalid request method." );
                return;
            }

            if ( request.QueryString["ChannelId"] == null )
            {
                response.TrySkipIisCustomErrors = true;
                response.StatusCode = 400;
                response.Write( "A ChannelId is required." );
                return;
            }

            int? channelId = request.QueryString["ChannelId"].AsIntegerOrNull();

            if ( channelId == null )
            {
                response.TrySkipIisCustomErrors = true;
                response.StatusCode = 400;
                response.Write( "Invalid channel id." );
                return;
            }

            ContentChannel channel = new ContentChannelService( rockContext ).Queryable( "ContentChannelType" ).FirstOrDefault( c => c.Id == channelId.Value );

            if ( channel == null )
            {
                response.TrySkipIisCustomErrors = true;
                response.StatusCode = 404;
                response.Write( "Channel does not exist." );
                return;
            }

            if ( !channel.EnableRss )
            {
                response.TrySkipIisCustomErrors = true;
                response.StatusCode = 403;
                response.Write( "RSS is not enabled for this channel." );
                return;
            }

            DefinedValueCache dvRssTemplate = null;

            if ( request.QueryString["TemplateId"] != null )
            {
                int? templateDefinedValueId = request.QueryString["TemplateId"].AsIntegerOrNull();

                if ( templateDefinedValueId == null )
                {
                    response.TrySkipIisCustomErrors = true;
                    response.StatusCode = 400;
                    response.Write( "Invalid template id." );
                    return;
                }

                dvRssTemplate = DefinedValueCache.Get( templateDefinedValueId.Value );
            }

            if ( dvRssTemplate == null )
            {
                dvRssTemplate = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.DEFAULT_RSS_CHANNEL );
            }

            if ( dvRssTemplate.DefinedType.Guid != new Guid( Rock.SystemGuid.DefinedType.LAVA_TEMPLATES ) )
            {
                response.TrySkipIisCustomErrors = true;
                response.StatusCode = 400;
                response.Write( "Invalid template id." );
                return;
            }

            string rssTemplate = dvRssTemplate.GetAttributeValue( "Template" );

            if ( string.IsNullOrWhiteSpace( dvRssTemplate.GetAttributeValue( "MimeType" ) ) )
            {
                response.ContentType = "application/rss+xml";
            }
            else
            {
                response.ContentType = dvRssTemplate.GetAttributeValue( "MimeType" );
            }

            if ( request.HttpMethod == "HEAD" )
            {
                response.StatusCode = 200;
                return;
            }

            // load merge fields
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
            mergeFields.Add( "Channel", channel );

            Dictionary<string, object> requestObjects = new Dictionary<string, object>();
            requestObjects.Add( "Scheme", request.Url.Scheme );
            requestObjects.Add( "Host", WebRequestHelper.GetHostNameFromRequest( context ) );
            requestObjects.Add( "Authority", request.Url.Authority );
            requestObjects.Add( "LocalPath", request.Url.LocalPath );
            requestObjects.Add( "AbsoluteUri", request.Url.AbsoluteUri );
            requestObjects.Add( "AbsolutePath", request.Url.AbsolutePath );
            requestObjects.Add( "Port", request.Url.Port );
            requestObjects.Add( "Query", request.Url.Query );
            requestObjects.Add( "OriginalString", request.Url.OriginalString );

            mergeFields.Add( "Request", requestObjects );

            // check for new rss item limit
            if ( request.QueryString["Count"] != null )
            {
                int.TryParse( request.QueryString["Count"], out rssItemLimit );
            }

            // get channel items
            ContentChannelItemService contentService = new ContentChannelItemService( rockContext );

            var content = contentService.Queryable( "ContentChannelType" )
                            .Where( c =>
                                c.ContentChannelId == channel.Id &&
                                ( c.Status == ContentChannelItemStatus.Approved || c.ContentChannel.ContentChannelType.DisableStatus || c.ContentChannel.RequiresApproval == false ) &&
                                c.StartDateTime <= RockDateTime.Now );

            if ( channel.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange )
            {
                if ( channel.ContentChannelType.IncludeTime )
                {
                    content = content.Where( c => !c.ExpireDateTime.HasValue || c.ExpireDateTime >= RockDateTime.Now );
                }
                else
                {
                    content = content.Where( c => !c.ExpireDateTime.HasValue || c.ExpireDateTime > RockDateTime.Today );
                }
            }

            if ( channel.ItemsManuallyOrdered )
            {
                content = content.OrderBy( c => c.Order );
            }
            else
            {
                content = content.OrderByDescending( c => c.StartDateTime );
            }

            content = content.Take( rssItemLimit );

            foreach ( var item in content )
            {
                item.Content = item.Content.ResolveMergeFields( mergeFields );

                // resolve any relative links
                var globalAttributes = GlobalAttributesCache.Get();
                string publicAppRoot = globalAttributes.GetValue( "PublicApplicationRoot" ).EnsureTrailingForwardslash();
                item.Content = item.Content.Replace( @" src=""/", @" src=""" + publicAppRoot );
                item.Content = item.Content.Replace( @" href=""/", @" href=""" + publicAppRoot );

                // get item attributes and add them as elements to the feed
                item.LoadAttributes( rockContext );
                foreach ( var attributeValue in item.AttributeValues )
                {
                    attributeValue.Value.Value = attributeValue.Value.Value.ResolveMergeFields( mergeFields );
                }
            }

            mergeFields.Add( "Items", content );

            mergeFields.Add( "RockVersion", Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );

            response.Write( rssTemplate.ResolveMergeFields( mergeFields ) );

        }

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler" /> instance.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler" /> instance is reusable; otherwise, false.</returns>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
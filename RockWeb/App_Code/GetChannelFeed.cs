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
using System.Data.Entity;
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
    public class GetChannelFeed : IHttpAsyncHandler
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Attributes
        /// </summary>

        private static class AttributeKey
        {
            public const string CacheDuration = "CacheDuration";
            public const string MimeType = "MimeType";
            public const string Template = "Template";
            public const string CacheKeyPrefix = "Rock:GetChannelFeed:";
            public const string LastModified = "Last-Modified";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string ChannelId = "ChannelId";
            public const string TemplateId = "TemplateId";
            public const string Count = "Count";
        }

        #endregion Page Parameter Keys
        private HttpRequest request;
        private HttpResponse response;

        private AsyncProcessorDelegate _delegate;
        protected delegate void AsyncProcessorDelegate( HttpContext context );

        private int rssItemLimit = 10;

        private const string HEAD = "HEAD";
        private const string GET = "GET";

        public IAsyncResult BeginProcessRequest( HttpContext context, AsyncCallback cb, object extraData )
        {
            _delegate = new AsyncProcessorDelegate( ProcessRequest );

            return _delegate.BeginInvoke( context, cb, extraData );
        }

        public void EndProcessRequest( IAsyncResult result )
        {
            _delegate.EndInvoke( result );
        }

        public void ProcessRequest( HttpContext context )
        {
            request = context.Request;
            response = context.Response;

            string cacheKey = AttributeKey.CacheKeyPrefix + request.RawUrl;

            DefinedValueCache dvRssTemplate = null;

            if ( request.QueryString[PageParameterKey.TemplateId] != null )
            {
                int? templateDefinedValueId = request.QueryString[PageParameterKey.TemplateId].AsIntegerOrNull();

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

            var cacheDuration = dvRssTemplate.GetAttributeValue( AttributeKey.CacheDuration ).AsInteger();
            var contentCache = RockCache.Get( cacheKey );

            if ( cacheDuration == 0 && contentCache != null )
            {
                RockCache.Remove( cacheKey );
            }

            var mimeTypeCache = RockCache.Get( $"{cacheKey}:{AttributeKey.MimeType}" );
            if (  mimeTypeCache != null && contentCache != null  && cacheDuration > 0 )
            {
                response.ContentType = ( string ) mimeTypeCache;
                response.Write( ( string ) contentCache );
                response.StatusCode = 200;
                return;
            }

            if ( request.HttpMethod != GET && request.HttpMethod != HEAD )
            {
                response.TrySkipIisCustomErrors = true;
                response.StatusCode = 405;
                response.Headers.Add( "Allow", GET );
                response.Write( "Invalid request method." );
                return;
            }

            if ( request.QueryString[PageParameterKey.ChannelId] == null )
            {
                response.TrySkipIisCustomErrors = true;
                response.StatusCode = 400;
                response.Write( "A ChannelId is required." );
                return;
            }

            int? channelId = request.QueryString[PageParameterKey.ChannelId].AsIntegerOrNull();

            if ( channelId == null )
            {
                response.TrySkipIisCustomErrors = true;
                response.StatusCode = 400;
                response.Write( "Invalid channel id." );
                return;
            }

            var channel = ContentChannelCache.Get( channelId.Value );

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

            if ( dvRssTemplate.DefinedType.Guid != new Guid( Rock.SystemGuid.DefinedType.LAVA_TEMPLATES ) )
            {
                response.TrySkipIisCustomErrors = true;
                response.StatusCode = 400;
                response.Write( "Invalid template id." );
                return;
            }

            string rssTemplate = dvRssTemplate.GetAttributeValue( AttributeKey.Template );

            if ( string.IsNullOrWhiteSpace( dvRssTemplate.GetAttributeValue( AttributeKey.MimeType ) ) )
            {
                response.ContentType = "application/rss+xml";
            }
            else
            {
                response.ContentType = dvRssTemplate.GetAttributeValue( AttributeKey.MimeType );
            }

            if ( request.HttpMethod == HEAD )
            {
                response.StatusCode = 200;
                var lastModifiedDateTime = RockCache.Get( $"{cacheKey}:{AttributeKey.LastModified}" );
                if ( lastModifiedDateTime != null )
                {
                    response.Headers.Add( AttributeKey.LastModified, ( string ) lastModifiedDateTime );
                }
                return;
            }

            // load merge fields
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
            mergeFields.Add( "Channel", channel );
            var safeProxyUrl = request.UrlProxySafe();

            Dictionary<string, object> requestObjects = new Dictionary<string, object>();
            requestObjects.Add( "Scheme", safeProxyUrl.Scheme );
            requestObjects.Add( "Host", WebRequestHelper.GetHostNameFromRequest( context ) );
            requestObjects.Add( "Authority", safeProxyUrl.Authority );
            requestObjects.Add( "LocalPath", safeProxyUrl.LocalPath );
            requestObjects.Add( "AbsoluteUri", safeProxyUrl.AbsoluteUri );
            requestObjects.Add( "AbsolutePath", safeProxyUrl.AbsolutePath );
            requestObjects.Add( "Port", safeProxyUrl.Port );
            requestObjects.Add( "Query", safeProxyUrl.Query );
            requestObjects.Add( "OriginalString", safeProxyUrl.OriginalString );

            mergeFields.Add( "Request", requestObjects );

            // check for new rss item limit
            if ( request.QueryString[PageParameterKey.Count] != null )
            {
                int.TryParse( request.QueryString[PageParameterKey.Count], out rssItemLimit );
            }

            // get channel items
            var rockContext = new RockContext();
            ContentChannelItemService contentService = new ContentChannelItemService( rockContext );

            var content = contentService.Queryable().AsNoTracking().Where( c =>
                c.ContentChannelId == channel.Id &&
                c.StartDateTime <= RockDateTime.Now );

            if ( !channel.ContentChannelType.DisableStatus && channel.RequiresApproval )
            {
                content = content.Where( cci => cci.Status == ContentChannelItemStatus.Approved );
            }

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

            var contentItems = content.Take( rssItemLimit ).ToList();

            foreach ( var item in contentItems )
            {
                item.Content = item.Content.ResolveMergeFields( mergeFields );

                // resolve any relative links
                var globalAttributes = GlobalAttributesCache.Get();
                string publicAppRoot = globalAttributes.GetValue( "PublicApplicationRoot" );
                item.Content = item.Content.Replace( @" src=""/", @" src=""" + publicAppRoot );
                item.Content = item.Content.Replace( @" href=""/", @" href=""" + publicAppRoot );

                // get item attributes and add them as elements to the feed
                item.LoadAttributes( rockContext );
                foreach ( var attributeValue in item.AttributeValues )
                {
                    attributeValue.Value.Value = attributeValue.Value.Value.ResolveMergeFields( mergeFields );
                }
            }

            mergeFields.Add( "Items", contentItems );

            mergeFields.Add( "RockVersion", Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );

            var outputContent = rssTemplate.ResolveMergeFields( mergeFields );
            response.Write( outputContent );

            var lastModifiedItem = contentItems.Where( c => c.ModifiedDateTime.HasValue )
                .OrderByDescending( c => c.ModifiedDateTime )
                .Select( c => c.ModifiedDateTime )
                .FirstOrDefault();

            if ( lastModifiedItem.HasValue )
            {
                RockCache.AddOrUpdate( $"{cacheKey}:{AttributeKey.LastModified}", lastModifiedItem.ToString() );
            }

            if ( cacheDuration > 0 )
            {
                var expiration = RockDateTime.Now.AddMinutes( cacheDuration );
                if ( expiration > RockDateTime.Now )
                {
                    RockCache.AddOrUpdate( $"{cacheKey}:{AttributeKey.MimeType}", null, response.ContentType, expiration );
                    RockCache.AddOrUpdate( cacheKey, null, outputContent, expiration );
                }
            }
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
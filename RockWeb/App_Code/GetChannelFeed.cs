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
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Text;
using System.Net;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using System.Collections.Generic;

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

            if ( request.HttpMethod != "GET" )
            {
                response.Write( "Invalid request type." );
                response.StatusCode = 200;
                return;
            }

            if ( request.QueryString["ChannelId"] != null )
            {
                int channelId;
                int templateDefinedValueId;
                DefinedValueCache dvRssTemplate;
                string rssTemplate;

                if ( !int.TryParse( request.QueryString["ChannelId"] , out channelId ))
                {
                    response.Write( "Invalid channel id." );
                    response.StatusCode = 200;
                    return;
                }

                if ( request.QueryString["TemplateId"] == null || !int.TryParse( request.QueryString["TemplateId"], out templateDefinedValueId ) )
                {
                    dvRssTemplate = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.DEFAULT_RSS_CHANNEL );
                }
                else
                {
                    dvRssTemplate = DefinedValueCache.Read( templateDefinedValueId );
                }

                rssTemplate = dvRssTemplate.GetAttributeValue( "Template" );

                if ( string.IsNullOrWhiteSpace( dvRssTemplate.GetAttributeValue( "MimeType" ) ) )
                {
                    response.ContentType = "application/rss+xml";
                }
                else
                {
                    response.ContentType = dvRssTemplate.GetAttributeValue( "MimeType" );
                }
                 
                ContentChannelService channelService = new ContentChannelService( rockContext );

                var channel = channelService.Queryable( "ContentChannelType" ).Where( c => c.Id == channelId ).FirstOrDefault();

                if ( channel != null )
                {
                    if ( channel.EnableRss )
                    {
                        // load merge fields
                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                        mergeFields.Add( "Channel", channel );

                        Dictionary<string, object> requestObjects = new Dictionary<string, object>();
                        requestObjects.Add( "Scheme", request.Url.Scheme );
                        requestObjects.Add( "Host", request.Url.Host );
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
                                content = content.Where( c => c.ExpireDateTime >= RockDateTime.Now );
                            }
                            else
                            {
                                content = content.Where( c => c.ExpireDateTime > RockDateTime.Today );
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
                            var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();
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

                        // show debug info
                        response.Write( rssTemplate.ResolveMergeFields( mergeFields ) );
                    }
                    else
                    {
                        response.Write( "RSS is not enabled for this channel." );
                        response.StatusCode = 200;
                        return;
                    }
                }
                else
                {
                    response.StatusCode = 200;
                    response.Write( "Invalid channel id." );
                    response.StatusCode = 200;
                    return;
                }

                
            }
            else
            {
                response.Write( "A ChannelId is required." );
                response.StatusCode = 200;
                return;
            }

        }

        /// <summary>
        /// Sends a 403 (forbidden)
        /// </summary>
        /// <param name="context">The context.</param>
        private void SendNotAuthorized( HttpContext context )
        {
            context.Response.StatusCode = System.Net.HttpStatusCode.Forbidden.ConvertToInt();
            context.Response.StatusDescription = "Not authorized to view file";
            context.ApplicationInstance.CompleteRequest();
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
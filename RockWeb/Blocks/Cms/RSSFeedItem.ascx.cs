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
using System.ComponentModel;
using System.Net;
using System.ServiceModel.Syndication;
using System.Runtime.Caching;
using System.Xml;
using System.Xml.Linq;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web.Cache;
using Rock.Lava;
using DotLiquid;

namespace RockWeb.Blocks.Cms
{
    [DisplayName( "RSS Feed Item" )]
    [Category( "CMS" )]
    [Description( "Gets an item from a RSS feed and displays the content of that item based on a provided Lava template." )]

    #region Block Attributes

    [TextField(
        "RSS Feed URL",
        Description = "The URL of the RSS Feed to retrieve and consume",
        IsRequired = true,
        Category = "Feed",
        Key = AttributeKey.RSSFeedUrl )]

    [IntegerField(
        "Cache Duration",
        Description = "The length of time (in minutes) that the RSS feed data is stored in cache. If this value is 0, the feed will not be cached. Default is 20 minutes.",
        IsRequired = false,
        DefaultIntegerValue = 20,
        Category = "Feed",
        Key = AttributeKey.CacheDuration )]

    [TextField(
        "CSS File",
        Description = "An optional CSS File to add to the page for styling. Example \"Styles/rss.css\" would point to a stylesheet in the current theme's style folder.",
        IsRequired = false,
        Category = "Layout",
        Key = AttributeKey.CSSFile )]
    [CodeEditorField(
        "Template",
        Description = "The Lava template to use for rendering. This template would typically be in the theme's \"Assets/Lava\" folder.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = true,
        DefaultValue = @"{% include '~~/Assets/Lava/RSSFeedItem.lava' %}",
        Category = "Layout",
        Key = AttributeKey.Template )]
    [BooleanField(
        "Include RSS Link",
        Description = "Flag indicating that an RSS link should be included in the page header.",
        DefaultBooleanValue = true,
        Category = "Feed",
        Key = AttributeKey.IncludeRSSLink )]

    #endregion
    [Rock.SystemGuid.BlockTypeGuid( "F7898E47-8496-4D70-9594-4D1F616928F5" )]
    public partial class RSSFeedItem : RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string RSSFeedUrl = "RSSFeedUrl";
            public const string CacheDuration = "CacheDuration";
            public const string CSSFile = "CSSFile";
            public const string Template = "Template";
            public const string IncludeRSSLink = "IncludeRSSLink";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string FeedItemId = "FeedItemId";
        }

        #endregion Page Parameter Keys

        #region Private Properties

        private string TemplateCacheKey
        {
            get
            {
                return string.Format( "Rock:Template:{0}", BlockId );
            }
        }
        #endregion

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            BlockUpdated += RSSFeedItem_BlockUpdated;
            AddConfigurationUpdateTrigger( upContent );

            string cssFile = GetAttributeValue( AttributeKey.CSSFile );
            if ( !String.IsNullOrWhiteSpace( cssFile ) )
            {
                RockPage.AddCSSLink( ResolveRockUrl( cssFile ), false );
            }

        }

        protected override void OnLoad( EventArgs e )
        {
            string feedItemId = System.Web.HttpUtility.UrlDecode( PageParameter( PageParameterKey.FeedItemId ) );
            SetNotificationBox( String.Empty, String.Empty );

            LoadFeedItem( feedItemId );

            base.OnLoad( e );
        }
        #endregion

        #region Block Events
        protected void RSSFeedItem_BlockUpdated( object sender, EventArgs e )
        {
            ClearCache();
            pnlContent.Visible = false;
            LoadFeedItem( System.Web.HttpUtility.UrlDecode( PageParameter( PageParameterKey.FeedItemId ) ) );
        }
        #endregion

        #region internal methods

        private void ClearCache()
        {
            SyndicationFeedHelper.ClearCachedFeed( GetAttributeValue( AttributeKey.RSSFeedUrl ) );

            if ( LavaService.RockLiquidIsEnabled )
            {
#pragma warning disable CS0618 // Type or member is obsolete
                LavaTemplateCache.Remove( this.TemplateCacheKey );
#pragma warning restore CS0618 // Type or member is obsolete
            }

            LavaService.RemoveTemplateCacheEntry( this.TemplateCacheKey );
        }

        [RockObsolete( "1.13" )]
        [Obsolete( "This method is only required for the DotLiquid Lava implementation." )]
        private Template GetTemplate()
        {
            var cacheTemplate = LavaTemplateCache.Get( TemplateCacheKey, GetAttributeValue( AttributeKey.Template ) );

            LavaHelper.VerifyParseTemplateForCurrentEngine( GetAttributeValue( AttributeKey.Template ) );

            return cacheTemplate != null ? cacheTemplate.Template as DotLiquid.Template : null;
        }

        private string LoadDebugData( Dictionary<string, object> feedDictionary )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if ( feedDictionary != null && feedDictionary.Count > 0 )
            {
                sb.AppendLine( "<ul id=\"debugFeed\">" );
                foreach ( var kvp in feedDictionary )
                {
                    sb.Append( FeedDebugNode( kvp ) );
                }
                sb.AppendLine( "</ul>" );
            }
            return sb.ToString();
        }

        private string FeedDebugNode( KeyValuePair<string, object> node )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();


            if ( node.Value == null )
            {
                sb.AppendFormat( "<li><span>{0}: {1}</span>", node.Key, "{null}" );
            }
            else if ( node.Value.GetType() == typeof( Dictionary<string, object> ) )
            {
                sb.AppendFormat( "<li><span>{0}</span>", node.Key );

                foreach ( var child in (Dictionary<string, object>)node.Value )
                {
                    sb.AppendLine( "<ul>" );
                    sb.Append( FeedDebugNode( child ) );
                    sb.AppendLine( "</ul>" );
                }
            }
            else if ( node.Value.GetType() == typeof( List<Dictionary<string, object>> ) )
            {
                List<Dictionary<string, object>> nodeList = (List<Dictionary<string, object>>)node.Value;

                foreach ( var listItem in nodeList )
                {
                    sb.AppendFormat( "<li><span>{0}</span>", node.Key );
                    sb.AppendLine( "<ul>" );
                    foreach ( var child in listItem )
                    {
                        sb.Append( FeedDebugNode( child ) );
                    }
                    sb.AppendLine( "</ul>" );
                }
            }
            else
            {
                sb.AppendFormat( "<li><span>{0}: {1}</span>", node.Key, node.Value == null ? "{null}" : System.Web.HttpUtility.HtmlEncode( node.Value.ToString() ) );
            }

            sb.Append( "</li>" );

            return sb.ToString();
        }

        private void LoadFeedItem( string feedItemId )
        {
            string feedUrl = GetAttributeValue( AttributeKey.RSSFeedUrl );
            Dictionary<string, string> messages = new Dictionary<string, string>();
            bool isError = false;

            try
            {
                Dictionary<string, object> feedDictionary = SyndicationFeedHelper.GetFeed( feedUrl, RockPage.Guid.ToString(), GetAttributeValue( AttributeKey.CacheDuration ).AsInteger(), ref messages, ref isError );

                if ( feedDictionary != null && feedDictionary.Count > 0 )
                {


                    if ( !String.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.RSSFeedUrl ) ) && GetAttributeValue( AttributeKey.IncludeRSSLink ).AsBoolean() )
                    {
                        string rssLink = string.Format( "<link rel=\"alternate\" type=\"application/rss+xml\" title=\"{0}\" href=\"{1}\" />",
                            feedDictionary.ContainsKey( "title" ) ? feedDictionary["title"].ToString() : "RSS",
                            GetAttributeValue( AttributeKey.RSSFeedUrl ) );

                        Page.Header.Controls.Add( new LiteralControl( rssLink ) );
                    }

                    Dictionary<string, object> previousItem = null;
                    Dictionary<string, object> selectedItem = null;
                    Dictionary<string, object> nextItem = null;
                    if ( feedDictionary.ContainsKey( "item" ) || feedDictionary.ContainsKey( "entry" ) )
                    {
                        List<Dictionary<string, object>> items = ( (List<Dictionary<string, object>>)feedDictionary.Where( i => i.Key == "item" || i.Key == "entry" ).FirstOrDefault().Value );
                        for ( int i = 0; i < items.Count; i++ )
                        {
                            if ( items[i]["articleHash"].ToString() == feedItemId )
                            {
                                selectedItem = items[i];


                                if ( i > 0 )
                                {
                                    nextItem = items[i - 1];
                                }

                                if ( i < ( items.Count - 1 ) )
                                {
                                    previousItem = items[i + 1];
                                }
                                break;
                            }
                        }

                    }

                    Dictionary<string, object> feedFinal = new Dictionary<string, object>();
                    feedFinal.Add( "Feed", feedDictionary );
                    feedFinal.Add( "SelectedItem", selectedItem );
                    feedFinal.Add( "PreviousItem", previousItem );
                    feedFinal.Add( "NextItem", nextItem );

                    if ( selectedItem == null )
                    {
                        messages.Add( "Requested item not available", "The item that you requested is currently not available." );
                    }
                    else
                    {
                        string content;

                        if ( LavaService.RockLiquidIsEnabled )
                        {
#pragma warning disable CS0618 // Type or member is obsolete
                            content = GetTemplate().Render( Hash.FromDictionary( feedFinal ) );
#pragma warning restore CS0618 // Type or member is obsolete
                        }
                        else
                        {
                            var renderParameters = new LavaRenderParameters
                            {
                                Context = LavaService.NewRenderContext( feedFinal ),
                                CacheKey = this.TemplateCacheKey
                            };

                            var result = LavaService.RenderTemplate( GetAttributeValue( AttributeKey.Template ), renderParameters );

                            content = result.Text;
                        }

                        if ( content.Contains( "No such template" ) )
                        {
                            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match( GetAttributeValue( AttributeKey.Template ), @"'([^']*)" );
                            if ( match.Success )
                            {
                                messages.Add( "Warning", string.Format( "Could not find the template _{0}.liquid in {1}.", match.Groups[1].Value, ResolveRockUrl( "~~/Assets/Liquid" ) ) );
                                isError = true;
                            }
                            else
                            {
                                messages.Add( "Warning", "Unable to parse the template name from settings." );
                                isError = true;

                            }
                        }
                        else
                        {
                            phFeedItem.Controls.Clear();
                            phFeedItem.Controls.Add( new LiteralControl( content ) );
                            pnlContent.Visible = true;
                        }

                    }
                }
            }
            catch ( Exception ex )
            {
                if ( IsUserAuthorized( Rock.Security.Authorization.ADMINISTRATE ) )
                {
                    throw ex;
                }
                else
                {
                    messages.Add( "Content not available.", "Oops. The requested content is not currently available. Please try again later." );
                    isError = true;
                }

            }


            if ( messages.Count > 0 )
            {
                if ( IsUserAuthorized( Rock.Security.Authorization.ADMINISTRATE ) )
                {
                    SetNotificationBox( messages.FirstOrDefault().Key, messages.FirstOrDefault().Value, isError ? NotificationBoxType.Warning : NotificationBoxType.Info );
                }
                else
                {
                    SetNotificationBox( "Content not available", "Oops. The requested content is not currently available. Please try again later." );
                }
            }

        }

        private void SetNotificationBox( string heading, string bodyText )
        {
            SetNotificationBox( heading, bodyText, NotificationBoxType.Info );
        }

        private void SetNotificationBox( string heading, string bodyText, NotificationBoxType boxType )
        {
            nbRssItem.Heading = heading;
            nbRssItem.Text = bodyText;
            nbRssItem.NotificationBoxType = boxType;

            nbRssItem.Visible = !( String.IsNullOrWhiteSpace( heading ) || String.IsNullOrWhiteSpace( bodyText ) );
        }


        #endregion
    }
}
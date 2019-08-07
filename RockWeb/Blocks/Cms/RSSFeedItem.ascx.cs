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

using DotLiquid;

using Rock;
using Rock.Attribute;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Cms
{
    [DisplayName( "RSS Feed Item" )]
    [Category( "CMS" )]
    [Description( "Gets an item from a RSS feed and displays the content of that item based on a provided Lava template." )]
    [TextField( "RSS Feed Url", "The Url to the RSS feed that the item belongs to.", true, "", "Feed" )]
    [IntegerField( "Cache Duration", "The length of time (in minutes) that the RSS feed data is stored in cache. If this value is 0, the feed will not be cached. Default is 20 minutes.", false, 20, "Feed" )]
    [TextField( "CSS File", "An optional CSS File to add to the page for styling. Example \"Styles/rss.css\" would point to a stylesheet in the current theme's style folder.", false, "", "Layout" )]
    [CodeEditorField( "Template", "The Lava template to use for rendering. This template would typically be in the theme's \"Assets/Lava\" folder.",
        CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, true, @"{% include '~~/Assets/Lava/RSSFeedItem.lava' %}", "Layout" )]
    [BooleanField( "Include RSS Link", "Flag indicating that an RSS link should be included in the page header.", true, "Feed" )]
    public partial class RSSFeedItem : RockBlock
    {
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

            string cssFile = GetAttributeValue( "CSSFile" );
            if ( !String.IsNullOrWhiteSpace( cssFile ) )
            {
                RockPage.AddCSSLink( ResolveRockUrl( cssFile ), false );
            }

        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            string feedItemId = System.Web.HttpUtility.UrlDecode( PageParameter( "feedItemId" ) );
            SetNotificationBox( String.Empty, String.Empty );

            LoadFeedItem( feedItemId );
        }
        #endregion

        #region Block Events
        protected void RSSFeedItem_BlockUpdated( object sender, EventArgs e )
        {
            ClearCache();
            pnlContent.Visible = false;
            LoadFeedItem( System.Web.HttpUtility.UrlDecode( PageParameter( "feedItemId" ) ) );
        }
        #endregion

        #region internal methods

        private void ClearCache()
        {
            SyndicationFeedHelper.ClearCachedFeed( GetAttributeValue( "RSSFeedUrl" ) );
            RockCache.Remove( TemplateCacheKey );
        }

        private Template GetTemplate()
        {
            var cacheTemplate = LavaTemplateCache.Get( TemplateCacheKey, GetAttributeValue( "Template" ) );
            return cacheTemplate != null ? cacheTemplate.Template : null;
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
            string feedUrl = GetAttributeValue( "RSSFeedUrl" );
            Dictionary<string, string> messages = new Dictionary<string, string>();
            bool isError = false;

            try
            {
                Dictionary<string, object> feedDictionary = SyndicationFeedHelper.GetFeed( feedUrl, RockPage.Guid.ToString(), GetAttributeValue( "CacheDuration" ).AsInteger(), ref messages, ref isError );

                if ( feedDictionary != null && feedDictionary.Count > 0 )
                {


                    if ( !String.IsNullOrWhiteSpace( GetAttributeValue( "RSSFeedUrl" ) ) && GetAttributeValue( "IncludeRSSLink" ).AsBoolean() )
                    {
                        string rssLink = string.Format( "<link rel=\"alternate\" type=\"application/rss+xml\" title=\"{0}\" href=\"{1}\" />",
                            feedDictionary.ContainsKey( "title" ) ? feedDictionary["title"].ToString() : "RSS",
                            GetAttributeValue( "RSSFeedUrl" ) );

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
                        string content = GetTemplate().Render( Hash.FromDictionary( feedFinal ) );

                        if ( content.Contains( "No such template" ) )
                        {
                            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match( GetAttributeValue( "Template" ), @"'([^']*)" );
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
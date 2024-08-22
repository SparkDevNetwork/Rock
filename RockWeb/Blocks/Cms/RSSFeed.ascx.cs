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
using System.ServiceModel.Syndication;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Lava;
using DotLiquid;

namespace RockWeb.Blocks.Cms
{

    [DisplayName( "RSS Feed" )]
    [Category( "CMS" )]
    [Description( "Gets and consumes and RSS Feed. The feed is rendered based on a provided lava template. " )]

    #region Block Attributes

    [TextField(
        "RSS Feed URL",
        Description = "The URL of the RSS Feed to retrieve and consume",
        IsRequired = true,
        Category = "Feed",
        Key = AttributeKey.RSSFeedUrl )]

    [IntegerField(
        "Results per page",
        Description = "How many results/articles to display on the page at a time. Default is 10.",
        IsRequired = false,
        DefaultIntegerValue =  10,
        Category = "Feed",
        Key = AttributeKey.Resultsperpage )]

    [IntegerField(
        "Cache Duration",
        Description = "The length of time (in minutes) that the RSS Feed data is stored in cache. If this value is 0, the feed will not be cached. Default is 20 minutes",
        IsRequired = false,
        DefaultIntegerValue = 20,
        Category = "Feed",
        Key = AttributeKey.CacheDuration )]

    [TextField(
        "CSS File",
        Description = "An optional CSS file to add to the page for styling. Example \"Styles/rss.css\" would point to the stylesheet in the current theme's styles folder.",
        IsRequired =false,
        Category = "Layout",
        Key = AttributeKey.CSSFile )]

    [CodeEditorField(
        "Template",
        Description = "The lava template to use for rendering. This template would typically be in the theme's \"Assets/Lava\" folder.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = true,
        DefaultValue = @"{% include '~~/Assets/Lava/RSSFeed.lava' %}",
        Category = "Layout",
        Key = AttributeKey.Template )]

    [BooleanField(
        "Include RSS Link",
        Description = "Flag indicating that an RSS link should be included in the page header.",
        DefaultBooleanValue = true,
        Category = "Feed",
        Key = AttributeKey.IncludeRSSLink )]

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage )]

    #endregion
    [Rock.SystemGuid.BlockTypeGuid( "2760F435-3E89-4016-85D9-13C019D0C58F" )]
    public partial class RSSFeed : RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string RSSFeedUrl = "RSSFeedUrl";
            public const string Resultsperpage = "Resultsperpage";
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
            public const string ArticlePage = "ArticlePage";
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

        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            BlockUpdated += RSSFeed_BlockUpdated;
            AddConfigurationUpdateTrigger( upContent );

            if ( !String.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.CSSFile ) ) )
            {
                RockPage.AddCSSLink( ResolveRockUrl( GetAttributeValue( AttributeKey.CSSFile ) ), false );
            }

        }

        protected override void OnLoad( EventArgs e )
        {
            SetNotificationBox( String.Empty, String.Empty );

            LoadFeed();

            base.OnLoad( e );
        }

        #endregion Base Control Methods

        #region Page Events

        protected void RSSFeed_BlockUpdated( object sender, EventArgs e )
        {
            ClearCache();
            pnlContent.Visible = false;
            LoadFeed();
        }

        #endregion

        #region Internal Methods

        private void ClearCache()
        {
            SyndicationFeedHelper.ClearCachedFeed( GetAttributeValue( AttributeKey.RSSFeedUrl ) );

            if ( LavaService.RockLiquidIsEnabled )
            {
#pragma warning disable CS0618 // Type or member is obsolete
                LavaTemplateCache.Remove( TemplateCacheKey );
#pragma warning restore CS0618 // Type or member is obsolete
            }

            LavaService.RemoveTemplateCacheEntry( TemplateCacheKey );
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

            if ( node.Value.GetType() == typeof( Dictionary<string, object> ) )
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
                sb.AppendFormat( "<li><span>{0}: {1}</span>", node.Key, node.Value == null ? String.Empty : System.Web.HttpUtility.HtmlEncode( node.Value.ToString() ) );
            }

            sb.Append( "</li>" );

            return sb.ToString();
        }

        private void LoadFeed()
        {

            string feedUrl = GetAttributeValue( AttributeKey.RSSFeedUrl );

            Dictionary<string, string> messages = new Dictionary<string, string>();
            bool isError = false;

            try
            {

                Dictionary<string, object> feedDictionary = SyndicationFeedHelper.GetFeed( feedUrl, GetAttributeValue( AttributeKey.DetailPage ), GetAttributeValue( AttributeKey.CacheDuration ).AsInteger(), ref messages, ref isError );

                if ( feedDictionary != null )
                {

                    int articlesPerPage = GetAttributeValue( AttributeKey.Resultsperpage ).AsInteger();
                    int currentPage = 0;
                    string baseUrl = new PageReference( RockPage.PageId ).BuildUrl();

                    int.TryParse( PageParameter( PageParameterKey.ArticlePage ), out currentPage );

                    if ( feedDictionary.ContainsKey( "ResultsPerPage" ) )
                    {
                        feedDictionary["ResultsPerPage"] = articlesPerPage;
                    }
                    else
                    {
                        feedDictionary.Add( "ResultsPerPage", articlesPerPage );
                    }


                    if ( feedDictionary.ContainsKey( "CurrentPage" ) )
                    {
                        feedDictionary["CurrentPage"] = currentPage;
                    }
                    else
                    {
                        feedDictionary.Add( "CurrentPage", currentPage );
                    }

                    if ( feedDictionary.ContainsKey( "BaseUrl" ) )
                    {
                        feedDictionary["BaseUrl"] = baseUrl;
                    }
                    else
                    {
                        feedDictionary.Add( "BaseUrl", baseUrl );
                    }


                    if ( !String.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.RSSFeedUrl ) ) && GetAttributeValue( AttributeKey.IncludeRSSLink ).AsBoolean() )
                    {
                        string rssLink = string.Format( "<link rel=\"alternate\" type=\"application/rss+xml\" title=\"{0}\" href=\"{1}\" />",
                            feedDictionary.ContainsKey( "title" ) ? feedDictionary["title"].ToString() : "RSS",
                            GetAttributeValue( AttributeKey.RSSFeedUrl ) );

                        Page.Header.Controls.Add( new LiteralControl( rssLink ) );
                    }


                    // rearrange the dictionary for cleaning purposes
                    if ( feedDictionary.ContainsKey( "entry" ) )
                    {
                        var item = feedDictionary["entry"];

                        if ( item != null )
                        {
                            feedDictionary.Remove( "entry" );
                            feedDictionary["Entries"] = item;
                        }
                    }


                    // remove the link item
                    if ( feedDictionary.ContainsKey( "link" ) )
                    {
                        var item = feedDictionary["link"];

                        if ( item != null )
                        {
                            feedDictionary.Remove( "link" );
                        }
                    }

                    string content = String.Empty;

                    if ( LavaService.RockLiquidIsEnabled )
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        content = GetTemplate().Render( Hash.FromDictionary( feedDictionary ) );
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                    else
                    {
                        var renderParameters = new LavaRenderParameters
                        {
                            Context = LavaService.NewRenderContext( feedDictionary ),
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
                        phRSSFeed.Controls.Clear();
                        phRSSFeed.Controls.Add( new LiteralControl( content ) );

                    }

                    pnlContent.Visible = true;
                }


            }
            catch ( Exception ex )
            {
                if ( IsUserAuthorized( Authorization.ADMINISTRATE ) )
                {
                    throw ex;
                }
                else
                {
                    messages.Add( "exception", "An exception has occurred." );
                }
            }

            if ( messages.Count > 0 )
            {
                if ( IsUserAuthorized( Authorization.ADMINISTRATE ) )
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
            nbRSSFeed.Heading = heading;
            nbRSSFeed.Text = bodyText;
            nbRSSFeed.NotificationBoxType = boxType;

            nbRSSFeed.Visible = !( String.IsNullOrWhiteSpace( heading ) || String.IsNullOrWhiteSpace( bodyText ) );
        }

        #endregion
    }


}
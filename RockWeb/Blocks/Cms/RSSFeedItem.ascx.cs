// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

namespace RockWeb.Blocks.Cms
{
    [DisplayName("RSS Feed Item")]
    [Category("CMS")]
    [Description("Gets an item from a RSS feed and displays the content of that item based on a provided liquid template.")]
    [TextField("RSS Feed Url", "The Url to the RSS feed that the item belongs to.", true, "", "Feed")]
    [IntegerField("Cache Duration", "The length of time (in minutes) that the RSS feed data is stored in cache. If this value is 0, the feed will not be cached. Default is 20 minutes.", false, 20, "Feed")]
    [TextField("CSS File", "An optional CSS File to add to the page for styling. Example \"Styles/rss.css\" would point to a stylesheet in the current theme's style folder.", false, "", "Layout")]
    [CodeEditorField("Template", "The liquid template to use for rendering. This template should be in the theme's \"Assets/Liquid\" folder and should have a underscore prepended to the filename.", 
        CodeEditorMode.Liquid, CodeEditorTheme.Rock, 200, true, @"{% include 'RSSFeedItem' %}", "Layout")]
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
                RockPage.AddCSSLink( ResolveRockUrl( cssFile ) );
            }
        }



        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            string feedItemId = System.Web.HttpUtility.UrlDecode( PageParameter( "feedItemId" ) );
            if ( !Page.IsPostBack )
            {
                LoadFeedItem( feedItemId );
            }
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
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( TemplateCacheKey );
        }

        private Template GetTemplate()
        {
            string liquidFolder = System.Web.HttpContext.Current.Server.MapPath( ResolveRockUrl( "~~/Assets/Liquid" ) );
            Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();
            Template.FileSystem = new DotLiquid.FileSystems.LocalFileSystem( liquidFolder );

            ObjectCache cache = MemoryCache.Default;
            Template template = null;

            if ( cache[TemplateCacheKey] != null )
            {
                template = (Template)cache[TemplateCacheKey];
            }
            else
            {
                template = Template.Parse( GetAttributeValue( "Template" ) );
                cache.Set( TemplateCacheKey, template, new CacheItemPolicy() );
            }

            return template;
        }

        private void LoadFeedItem(string feedItemId)
        {
            string feedUrl = GetAttributeValue( "RSSFeedUrl" );
            Dictionary<string, string> messages = new Dictionary<string, string>();
            bool isError = false;

            try
            {
                Dictionary<string, object> feedDictionary = SyndicationFeedHelper.GetFeed( feedUrl, RockPage.Guid.ToString(), (int)GetAttributeValue( "CacheDuration" ).AsInteger( true ), ref messages, ref isError );

                if ( feedDictionary != null && feedDictionary.Count > 0 )
                {
                    Dictionary<string, object> previousItem = null;
                    Dictionary<string, object> selectedItem = null;
                    Dictionary<string, object> nextItem = null;
                    List<Dictionary<string, object>> items = ( (List<Dictionary<string, object>>)feedDictionary.Where(i => i.Key == "item" || i.Key == "entry").FirstOrDefault().Value );
                    for ( int i = 0; i < items.Count; i++ )
                    {
                        if ( items[i]["articleHashCode"].ToString() == feedItemId )
                        {
                            selectedItem = items[i];


                            if ( i > 0 )
                            {
                                previousItem = items[i - 1];
                            }

                            if ( i < ( items.Count - 1 ) )
                            {
                                nextItem = items[i + 1];
                            }
                            break;
                        }
                    }

                    if ( selectedItem == null )
                    {
                        messages.Add( "Requested item not available", "The item that you requested is currently not available." );
                    }
                    else
                    {
                        Dictionary<string, object> feedFinal = new Dictionary<string, object>();
                        feedFinal.Add( "Feed", feedDictionary );
                        feedFinal.Add( "SelectedItem", selectedItem );
                        feedFinal.Add( "PreviousItem", previousItem );
                        feedFinal.Add( "NextItem", nextItem );

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
                if ( IsUserAuthorized( "Administrate" ) )
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
                if ( IsUserAuthorized( "Administrate" ) )
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
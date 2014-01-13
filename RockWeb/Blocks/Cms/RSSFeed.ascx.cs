using System;
using System.Collections.Generic;

using System.Linq;
using System.ComponentModel;
using System.Net;
using System.ServiceModel.Syndication;
using System.Runtime.Caching;
using System.Xml;
using System.Xml.Linq;

using DotLiquid;

using Rock;
using Rock.Attribute;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{

    [DisplayName("RSS Feed")]
    [Category("CMS")]
    [Description("Gets and consumes and RSS Feed. The feed is rendered based on a provided liquid template. ")]
    [TextField("RSS Feed Url", "The Url of the RSS Feed to retrieve and consume", true, "", "Feed")]
    [IntegerField("Results per page", "How many results/articles to display on the page at a time. Default is 10.", false, 10)]
    [IntegerField("Cache Duration", "The length of time (in minutes) that the RSS Feed data is stored in cache. If this value is 0, the feed will not be cached. Default is 20 minutes", false, 20)]
    [TextField("CSS File", "An optional CSS file to add to the page for styling. Example \"Styles/rss.css\" would point to the stylesheet in the current theme's styles folder.", false, "")]
    [CodeEditorField("Template", "The liquid template to use for rendering. This template should be in the theme's \"Assets/Liquid\" folder and should have an underscore prepended to the filename.", 
        CodeEditorMode.Liquid, CodeEditorTheme.Rock, 200, true, @"{% include 'RSSFeed' %}")]
    public partial class RSSFeed : RockBlock
    {
        #region Private Properties
        private string RSSCacheKey
        {
            get
            {
                return string.Format( "Rock:RSSFeed:{0}", BlockId );
                
            }
        }

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
            BlockUpdated += RSSFeed_BlockUpdated;
            AddConfigurationUpdateTrigger( upContent );    
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            SetNotificationBox( String.Empty, String.Empty );

            if(!Page.IsPostBack)
            {
                LoadFeed();
            }
        }
        #endregion

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
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( RSSCacheKey );
            cache.Remove( TemplateCacheKey );
        }

        /// <summary>
        /// Gets the feed.
        /// </summary>
        /// <param name="feedUrl">The feed URL.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        private SyndicationFeed GetFeed( string feedUrl, ref Dictionary<string, string> message, out bool isError )
        {
            ObjectCache cache = MemoryCache.Default;
            SyndicationFeed feed = null;
            isError = false;

            if ( cache[RSSCacheKey] != null )
            {
                feed = (SyndicationFeed)cache[RSSCacheKey];
            }
            else
            {
                if ( String.IsNullOrEmpty( feedUrl ) )
                {
                    message.Add( "Feed URL not provided.", "The RSS Feed URL has not been provided. Please update the \"RSS Feed URL\" attribute in the block settings." );
                    return feed;
                }
                if ( !System.Text.RegularExpressions.Regex.IsMatch( feedUrl, @"^(http://|https://|)([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?" ) )
                {
                    message.Add( "Feed URL not valid.", "The RSS Feed URL is not formatted properly. Please verify the \"RSS Feed URL\" attribute in block settings." );
                    isError = false;
                    return feed;
                }

                if ( !feedUrl.StartsWith( "http://", StringComparison.InvariantCultureIgnoreCase ) && !feedUrl.StartsWith( "https://", StringComparison.InvariantCultureIgnoreCase ) )
                {
                    feedUrl = "http://" + feedUrl;
                }

                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(feedUrl);

                using ( HttpWebResponse resp = (HttpWebResponse)req.GetResponse() )
                {
                    if ( resp.StatusCode == HttpStatusCode.OK )
                    {
                        XmlReader feedReader = XmlReader.Create( resp.GetResponseStream());

                        if ( new Rss20FeedFormatter().CanRead( feedReader ) || new Atom10FeedFormatter().CanRead( feedReader ) )
                        {
                            feed = SyndicationFeed.Load( feedReader );
                        }
                        else
                        {
                            message.Add( "Unable to read feed.", "Returned feed was not in ATOM or RSS format and could not be read." );
                        }
                        feedReader.Close();

                    }
                    else
                    {
                        message.Add("Error loading feed.", string.Format( "An error has occurred while loading the feed.  Status Code: {0} - {1}", (int)resp.StatusCode, resp.StatusDescription ) );
                        isError = true;
                    }
                }

                if(feed != null)
                { 
                    int cacheDuration = 0;

                    if ( int.TryParse( GetAttributeValue( "CacheDuration" ), out cacheDuration ) && cacheDuration > 0 )
                    {
                        cache.Add( RSSCacheKey, feed, DateTimeOffset.Now.AddMinutes( cacheDuration ) );
                    }
                }

            }

            return feed;
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

        private void LoadFeed()
        {

            string feedUrl = GetAttributeValue( "RSSFeedUrl" );

            Dictionary<string, string> messages = new Dictionary<string, string>();
            bool isError = false;

            try
            {
                SyndicationFeed feed = GetFeed( feedUrl, ref messages, out isError );

                if ( feed != null )
                {
                    Dictionary<string, object> feedDictionary = BuildFeedDictionary( feed );

                    pnlContent.Visible = true;
                }


            }
            catch ( Exception ex )
            {
                if ( IsUserAuthorized( "Administrate" ) )
                {
                    throw ex;
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

        private Dictionary<string, object> BuildFeedDictionary( SyndicationFeed feed )
        {
            Dictionary<string, object> feedDictionary = new Dictionary<string, object>();

            if(feed != null)
            {
                feedDictionary.Add( "AttributeExtensions", feed.AttributeExtensions.ToDictionary( a => a.Key.ToStringSafe(), a => a.Value ) );
                
                List<Dictionary<string, object>> authors = new List<Dictionary<string, object>>();
                foreach ( var author in feed.Authors )
                {
                    Dictionary<string, object> authorDictionary = new Dictionary<string, object>();
                    authorDictionary.Add( "AttributeExtensions", author.AttributeExtensions.ToDictionary( a => a.Key.ToStringSafe(), a => a.Value ) );
                    authorDictionary.Add( "ElementExtensions", author.ElementExtensions );
                    authorDictionary.Add( "Email", author.Email );
                    authorDictionary.Add( "Name", author.Name );
                    authorDictionary.Add( "Uri", author.Uri );

                    authors.Add( authorDictionary );
                }
                feedDictionary.Add("Authors", authors);
                feedDictionary.Add( "BaseUri", feed.BaseUri );

                List<Dictionary<string, object>> categories = new List<Dictionary<string, object>>();
                foreach ( var cat in feed.Categories )
                {
                    Dictionary<string, object> categoryDictionary = new Dictionary<string, object>();
                    categoryDictionary.Add( "AttributeExtensions", cat.AttributeExtensions.ToDictionary( a => a.Key.ToStringSafe(), a => a.Value ) );
                    categoryDictionary.Add( "ElementExtensions", cat.ElementExtensions );
                    categoryDictionary.Add( "Label", cat.Label );
                    categoryDictionary.Add( "Name", cat.Name );
                    categoryDictionary.Add( "Scheme", cat.Scheme );

                    categories.Add( categoryDictionary );
                }
                feedDictionary.Add( "Categories", categories );

                List<Dictionary<string, object>> contributors = new List<Dictionary<string, object>>();
                foreach ( var contrib in feed.Contributors )
                {
                    Dictionary<string, object> contributorDictionary = new Dictionary<string, object>();
                    contributorDictionary.Add( "AttributeExtensions", contrib.AttributeExtensions.ToDictionary( a => a.Key.ToStringSafe(), a => a.Value ) );
                    contributorDictionary.Add( "ElementExtensions", contrib.ElementExtensions );
                    contributorDictionary.Add( "Email", contrib.Email );
                    contributorDictionary.Add( "Name", contrib.Name );
                    contributorDictionary.Add( "Uri", contrib.Uri );
                    contributors.Add( contributorDictionary );
                }
                feedDictionary.Add( "Contributors", contributors );
                feedDictionary.Add( "Copyright", feed.Copyright == null ? null : feed.Copyright.Text );
                feedDictionary.Add( "Description", feed.Description == null ? null : feed.Description.Text );
                feedDictionary.Add( "ElemenentExtensions", feed.ElementExtensions );
                feedDictionary.Add( "Generator", feed.Generator );
                feedDictionary.Add( "Id", feed.Id );
                feedDictionary.Add( "ImageUrl", feed.ImageUrl );

                List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();
                foreach ( var feedItem in feed.Items.OrderByDescending( i => i.PublishDate ) )
                {
                    Dictionary<string, object> itemDictionary = new Dictionary<string, object>();
                    itemDictionary.Add( "AttributeExtensions", feedItem.AttributeExtensions.ToDictionary( a => a.Key.ToStringSafe(), a => a.Value ) );
                    List<Dictionary<string, object>> itemAuthors = new List<Dictionary<string, object>>();

                    foreach ( var author in feedItem.Authors )
                    {
                        Dictionary<string, object> authorDictionary = new Dictionary<string, object>();
                        authorDictionary.Add( "AttributeExtensions", author.AttributeExtensions.ToDictionary( a => a.Key.ToStringSafe(), a => a.Value ) );
                        authorDictionary.Add( "ElementExtensions", author.ElementExtensions );
                        authorDictionary.Add( "Email", author.Email );
                        authorDictionary.Add( "Name", author.Name );
                        authorDictionary.Add( "Uri", author.Uri );

                        itemAuthors.Add( authorDictionary );
                    }
                    itemDictionary.Add( "Authors", itemAuthors );
                    itemDictionary.Add( "BaseUri", feedItem.BaseUri );

                    List<Dictionary<string, object>> itemCategories = new List<Dictionary<string, object>>();
                    foreach ( var itemCat in feedItem.Categories )
                    {
                        Dictionary<string, object> itemCatDictionary = new Dictionary<string, object>();
                        itemCatDictionary.Add( "AttributeExtensions", itemCat.AttributeExtensions.ToDictionary( a => a.Key.ToStringSafe(), a => a.Value ) );
                        itemCatDictionary.Add( "ElementExtensions", itemCat.ElementExtensions );
                        itemCatDictionary.Add( "Label", itemCat.Label );
                        itemCatDictionary.Add( "Name", itemCat.Name );
                        itemCatDictionary.Add( "Scheme", itemCat.Scheme );

                        itemCategories.Add( itemCatDictionary );
                    }
                    itemDictionary.Add( "Categories", itemCategories );
                    itemDictionary.Add( "Content", feedItem.Content == null ? null : System.Web.HttpUtility.HtmlDecode( feedItem.Content.ToString() ) );

                    List<Dictionary<string, object>> itemContributors = new List<Dictionary<string, object>>();
                    foreach ( var itemContrib in feedItem.Contributors )
                    {
                        Dictionary<string, object> contribDictionary = new Dictionary<string, object>();
                        contribDictionary.Add( "AttributeExtensions", itemContrib.AttributeExtensions.ToDictionary( a => a.Key.ToStringSafe(), a => a.Value ) );
                        contribDictionary.Add( "ElementExtensions", itemContrib.ElementExtensions );
                        contribDictionary.Add( "Email", itemContrib.Email );
                        contribDictionary.Add( "Name", itemContrib.Name );
                        contribDictionary.Add( "Uri", itemContrib.Uri );

                        itemContributors.Add( contribDictionary );
                    }
                    itemDictionary.Add( "Contributors", itemContributors );
                    itemDictionary.Add( "Copyright", feedItem.Copyright == null ? null : feed.Copyright.Text );
                    itemDictionary.Add( "ElementExtensions", feedItem.ElementExtensions );
                    itemDictionary.Add( "Id", feedItem.Id );
                    itemDictionary.Add( "LastUpdatedTime",  feedItem.LastUpdatedTime.ToLocalTime() );

                    List<Dictionary<string, object>> itemLinks = new List<Dictionary<string, object>>();
                    foreach ( var link in feedItem.Links )
                    {
                        Dictionary<string, object> linkDictionary = new Dictionary<string, object>();
                        linkDictionary.Add( "AttributeExtensions", link.AttributeExtensions.ToDictionary( a => a.Key.ToStringSafe(), a => a.Value ) );
                        linkDictionary.Add( "BaseUri", link.BaseUri );
                        linkDictionary.Add( "ElementExtensions", link.ElementExtensions );
                        linkDictionary.Add( "Length", link.Length );
                        linkDictionary.Add( "MediaType", link.MediaType );
                        linkDictionary.Add( "RelationshipType", link.RelationshipType );
                        linkDictionary.Add( "Title", link.Title );
                        linkDictionary.Add( "Uri", link.Uri );

                        itemLinks.Add( linkDictionary );
                    }
                    itemDictionary.Add( "Links", itemLinks );
                    itemDictionary.Add( "PublishDate", feedItem.PublishDate.ToLocalTime() );
                    itemDictionary.Add( "Summary", feedItem.Summary == null ? null :  System.Web.HttpUtility.HtmlDecode( feedItem.Summary.Text ) );
                    itemDictionary.Add( "Title", feedItem.Title == null ? null : feedItem.Title.Text );

                    items.Add( itemDictionary );                   

                }
                feedDictionary.Add( "Items", items );
                feedDictionary.Add( "Language", feed.Language );
                feedDictionary.Add( "LastUpdatedTime", feed.LastUpdatedTime.ToLocalTime() );

                List<Dictionary<string, object>> links = new List<Dictionary<string, object>>();
                foreach ( var feedLink in feed.Links )
                {
                    Dictionary<string, object> feedLinkDictionary = new Dictionary<string, object>();
                    feedLinkDictionary.Add( "AttributeExtensions", feedLink.AttributeExtensions.ToDictionary( a => a.Key.ToStringSafe(), a => a.Value ) );
                    feedLinkDictionary.Add( "BaseUri", feedLink.BaseUri );
                    feedLinkDictionary.Add( "ElementExtensions", feedLink.ElementExtensions );
                    feedLinkDictionary.Add( "Length", feedLink.Length );
                    feedLinkDictionary.Add( "MediaType", feedLink.MediaType );
                    feedLinkDictionary.Add( "RelationshipType", feedLink.RelationshipType );
                    feedLinkDictionary.Add( "Title", feedLink.Title );
                    feedLinkDictionary.Add( "Uri", feedLink.Uri );

                    links.Add( feedLinkDictionary );
                }
                feedDictionary.Add( "Links", links );
                feedDictionary.Add( "Title", feed.Title == null ? null : feed.Title.Text );

            }

            return feedDictionary;
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
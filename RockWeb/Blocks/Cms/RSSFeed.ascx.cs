using System;
using System.Collections.Generic;

using System.Linq;
using System.ComponentModel;
using System.Net;
using System.ServiceModel.Syndication;
using System.Runtime.Caching;
using System.Xml;
using System.Xml.Linq;

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
    public partial class RSSFeed : RockBlock
    {
        private string CacheKey
        {
            get
            {
                return string.Format( "Rock:RSSFeed:{0}", BlockId );
                
            }
        }
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
            cache.Remove( CacheKey );
        }

        /// <summary>
        /// Gets the feed.
        /// </summary>
        /// <param name="feedUrl">The feed URL.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        private SyndicationFeed GetFeed( string feedUrl, out Dictionary<string, string> message, out bool isError )
        {
            ObjectCache cache = MemoryCache.Default;
            message = new Dictionary<string, string>();
            SyndicationFeed feed = null;
            isError = false;

            if ( cache[CacheKey] != null )
            {
                feed = (SyndicationFeed)cache[CacheKey];
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

                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(feedUrl);

                using ( HttpWebResponse resp = (HttpWebResponse)req.GetResponse() )
                {
                    if ( resp.StatusCode == HttpStatusCode.OK )
                    {

                        if ( new List<string>() { "text/xml", "text/plain", "application/rss", "application/rss+xml" }.Where( rt => resp.ContentType.Contains( rt ) ).Count() > 0 )
                        {
                            XmlReader feedReader = XmlReader.Create( resp.GetResponseStream() );

                            if ( new Rss20FeedFormatter().CanRead( feedReader ) )
                            {
                                feed = SyndicationFeed.Load( feedReader );
                            }
                            feedReader.Close();
                        }         
                        else
                        {
                            message.Add( "Unexpected content type returned.",
                                string.Format( "An unexpected content type was returned - {0}", resp.ContentType ) );
                            isError = true;
                        }
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
                        cache.Add( CacheKey, feed, DateTimeOffset.Now.AddMinutes( cacheDuration ) );
                    }
                }

            }

            return feed;
        }

        private void LoadFeed()
        {
            
            string feedUrl =  GetAttributeValue( "RSSFeedUrl" );

            Dictionary<string, string> message;
            bool isError = false;

            try
            {
                SyndicationFeed feed = GetFeed( feedUrl, out message, out isError );

                if ( message.Count > 0 )
                {
                    if ( IsUserAuthorized( "Administrate" ) )
                    {
                        SetNotificationBox( message.FirstOrDefault().Key, message.FirstOrDefault().Value, isError ? NotificationBoxType.Warning : NotificationBoxType.Info );

                    }
                    else
                    {
                        SetNotificationBox( "Content not available",
                            "The requested content is currently not available. Please try again later.",
                            NotificationBoxType.Info );
                    }
                    return;
                }


                litTitle.Text = feed.Title.Text;
                gRSSItems.DataSource = feed.Items.OrderByDescending( f => f.PublishDate )
                                        .Select( f => new
                                        {
                                            guid = f.Id,
                                            title = f.Title.Text,
                                            description = System.Web.HttpUtility.HtmlDecode( f.Summary.Text ),
                                            link = f.Links.FirstOrDefault().Uri,
                                            pubDate = f.PublishDate

                                        } );
                gRSSItems.DataBind();
                pnlContent.Visible = true;
            }
            catch ( Exception ex )
            {
                if ( IsUserAuthorized( "Administrate" ) )
                {
                    throw ex;
                }
                else
                {
                    SetNotificationBox( "Content not available.", "The requested content is currently not available. Please try again later.", NotificationBoxType.Info );
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
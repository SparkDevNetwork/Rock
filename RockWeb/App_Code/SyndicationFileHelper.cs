//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.ServiceModel.Syndication;
using System.Web;
using System.Xml;
using System.Xml.Linq;

using Rock.Web;

public class SyndicationFeedHelper
{

    #region Public Methods

    /// <summary>
    /// Clears the specified feed from object cache.
    /// </summary>
    /// <param name="feedUrl">A <see cref="System.String"/> representing the URL of the feed.</param>
    public static void ClearCachedFeed( string feedUrl )
    {
        ObjectCache cache = MemoryCache.Default;
        string cacheKey = GetFeedCacheKey( feedUrl );

        cache.Remove( cacheKey );
    }

    /// <summary>
    /// Gets a <see cref="System.Collections.Generic.Dictionary{string,object}"/> representing the contents of the syndicated feed.
    /// </summary>
    /// <param name="feedUrl">A <see cref="System.String"/> representing the URL of the feed.</param>
    /// <param name="detailPage">A <see cref="System.String"/> representing the Guid of the detail page. </param>
    /// <param name="cacheDuration">A <see cref="System.Int32"/> representing the length of time that the content of the RSS feed will be saved to cache.</param>
    /// <param name="message">A <see cref="System.Collections.Generic.Dictionary{string,object}"/> that will contain any error or alert messages that are returned.</param>
    /// <param name="isError">A <see cref="System.Boolean"/> that is <c>true</c> if an error has occurred, otherwise <c>false</c>.</param>
    /// <returns></returns>
    public static Dictionary<string, object> GetFeed( string feedUrl, string detailPage, int cacheDuration, ref Dictionary<string, string> message, ref bool isError )
    {
        Dictionary<string, object> feedDictionary = new Dictionary<string, object>();

        if ( message == null )
        {
            message = new Dictionary<string, string>();
        }

        if ( String.IsNullOrEmpty( feedUrl ) )
        {
            message.Add( "Feed URL not provided.", "The RSS Feed URL has not been provided. Please update the \"RSS Feed URL\" attribute in the block settings." );
            return feedDictionary;
        }
        if ( !System.Text.RegularExpressions.Regex.IsMatch( feedUrl, @"^(http://|https://|)([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?" ) )
        {
            message.Add( "Feed URL not valid.", "The Feed URL is not formatted properly. Please verify the \"RSS Feed URL\" attribute in block settings." );
            isError = false;
            return feedDictionary;
        }

        ObjectCache feedCache = MemoryCache.Default;

        if ( feedCache[GetFeedCacheKey( feedUrl )] != null )
        {
            feedDictionary = (Dictionary<string, object>)feedCache[GetFeedCacheKey( feedUrl )];
        }
        else
        {
            SyndicationFeed feed = null;

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create( feedUrl );

            using ( HttpWebResponse resp = (HttpWebResponse)req.GetResponse() )
            {
                if ( resp.StatusCode == HttpStatusCode.OK )
                {
                    XmlReader feedReader = XmlReader.Create( resp.GetResponseStream() );

                    if ( new Rss20FeedFormatter().CanRead( feedReader ) || new Atom10FeedFormatter().CanRead( feedReader ) )
                    {
                        feed = SyndicationFeed.Load( feedReader );
                    }
                    else
                    {
                        message.Add( "Unable to read feed.", "Returned feed was not in ATOM or RSS format and could not be read." );
                        isError = true;
                    }
                    feedReader.Close();

                }
                else
                {
                    message.Add( "Error loading feed.", string.Format( "An error has occurred while loading the feed.  Status Code: {0} - {1}", (int)resp.StatusCode, resp.StatusDescription ) );
                    isError = true;
                }
            }

            if ( feed != null )
            {
                feedDictionary = BuildFeedDictionary( feed, detailPage );

                feedCache.Set( GetFeedCacheKey( feedUrl ), feedDictionary, DateTimeOffset.Now.AddMinutes( cacheDuration ) );
            }

        }

        return feedDictionary;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Builds the feed dictionary object
    /// </summary>
    /// <param name="feed">A <see cref="System.ServiceModel.Syndication.SyndicationFeed"/> containing the content of the feed.</param>
    /// <param name="detailPage">A <see cref="System.String"/> containing the Guid of the detail page.</param>
    /// <returns>A <see cref="System.Collections.Generic.Dictionary{string,object}"/> representing the feed.</returns>
    private static Dictionary<string, object> BuildFeedDictionary( SyndicationFeed feed, string detailPage )
    {
        Dictionary<string, object> feedDictionary = new Dictionary<string, object>();

        if ( feed == null )
        {
            return feedDictionary;
        }

        feedDictionary.Add( "AttributeExtensions", feed.AttributeExtensions.ToDictionary( a => a.Key.ToString(), a => a.Value ) );

        List<Dictionary<string, object>> authors = new List<Dictionary<string, object>>();
        foreach ( var a in feed.Authors )
        {
            authors.Add( BuildSyndicationPerson( a ) );
        }
        feedDictionary.Add( "Authors", authors );

        feedDictionary.Add( "BaseUri", feed.BaseUri == null ? null : feed.BaseUri.ToString() );

        List<Dictionary<string, object>> categories = new List<Dictionary<string, object>>();
        foreach ( var c in feed.Categories )
        {
            categories.Add( BuildSyndicationCategory( c ) );
        }
        feedDictionary.Add( "Categories", categories );

        List<Dictionary<string, object>> contributors = new List<Dictionary<string, object>>();
        foreach ( var c in feed.Contributors )
        {
            contributors.Add( BuildSyndicationPerson( c ) );
        }
        feedDictionary.Add( "Contributors", contributors );

        feedDictionary.Add( "Copyright", feed.Copyright == null ? null : feed.Copyright.Text );
        feedDictionary.Add( "Description", feed.Description == null ? null : feed.Description.Text );
        feedDictionary.Add( "ElemenentExtensions", feed.ElementExtensions );
        feedDictionary.Add( "Generator", feed.Generator );
        feedDictionary.Add( "Id", feed.Id );
        feedDictionary.Add( "ImageUrl", feed.ImageUrl == null ? null : feed.ImageUrl.ToString() );

        List<Dictionary<string, object>> feedItems = new List<Dictionary<string, object>>();
        foreach ( var i in feed.Items.OrderByDescending( i => i.PublishDate ) )
        {
            feedItems.Add( BuildSyndicationItem( i, detailPage ) );
        }
        feedDictionary.Add( "Items", feedItems );

        feedDictionary.Add( "Language", feed.Language );
        feedDictionary.Add( "LastUpdatedTime", feed.LastUpdatedTime.ToLocalTime().DateTime );

        List<Dictionary<string, object>> links = new List<Dictionary<string, object>>();
        foreach ( var l in feed.Links )
        {
            links.Add( BuildSyndicationLink( l ) );
        }
        feedDictionary.Add( "Links", links );
        feedDictionary.Add( "Title", feed.Title == null ? null : feed.Title.Text );

        return feedDictionary;
    }

    /// <summary>
    /// Builds a dictionary that contains Syndication Category.
    /// </summary>
    /// <param name="c">A <see cref="System.ServiceModel.Syndication.SyndicationCategory"/> representing a category from a syndicated feed.</param>
    /// <returns>A <see cref="System.Collections.Generic.Dictionary{string,object}"/> containing the content of the category.</returns>
    private static Dictionary<string, object> BuildSyndicationCategory( SyndicationCategory c )
    {
        Dictionary<string, object> category = new Dictionary<string, object>();

        if ( c != null )
        {
            category.Add( "AttributeExtensions", c.AttributeExtensions.ToDictionary( a => a.Key.ToString(), a => a.Value ) );
            category.Add( "ElementExtensions", c.ElementExtensions );
            category.Add( "Label", c.Label );
            category.Add( "Name", c.Name );
            category.Add( "Scheme", c.Scheme );
        }

        return category;
    }

    /// <summary>
    /// Builds a dictionary containing the content of the provided <see cref="System.ServiceModel.Syndication.SyndicationItem"/>
    /// </summary>
    /// <param name="i">The <see cref="System.ServiceModel.Syndicawtion.SyndicationItem"/> to be converted to a dictionary.</param>
    /// <param name="detailPage">A <see cref="System.String"/> representing the Guid of the detail page.</param>
    /// <returns>A <see cref="System.Collections.Generic.Dictionary{string,object}"/> representing the provided syndication item.</returns>
    private static Dictionary<string, object> BuildSyndicationItem( SyndicationItem i, string detailPage )
    {
        Dictionary<string, object> itemDictionary = new Dictionary<string, object>();

        if ( i != null )
        {
            itemDictionary.Add( "AttributeExtensions", i.AttributeExtensions.ToDictionary( a => a.Key.ToString(), a => a.Value ) );

            string detailPageUrl = string.Empty;

            if ( !String.IsNullOrWhiteSpace( detailPage ) )
            {
                Rock.Model.Page page = new Rock.Model.PageService().Get( new Guid( detailPage ) );
                Dictionary<string, string> queryString = new Dictionary<string, string>();
                queryString.Add( "feedItemId", System.Web.HttpUtility.UrlEncode( i.Id ) );
                detailPageUrl = new PageReference( page.Id, 0, queryString ).BuildUrl();
            }

            List<Dictionary<string, object>> authors = new List<Dictionary<string, object>>();
            foreach ( var a in i.Authors )
            {
                authors.Add( BuildSyndicationPerson( a ) );
            }
            itemDictionary.Add( "Authors", authors );

            itemDictionary.Add( "BaseUri", i.BaseUri == null ? null : i.BaseUri.ToString() );

            List<Dictionary<string, object>> categories = new List<Dictionary<string, object>>();
            foreach ( var c in i.Categories )
            {
                categories.Add( BuildSyndicationCategory( c ) );
            }
            itemDictionary.Add( "Categories", categories );
            itemDictionary.Add( "Content", i.Content == null ? null : System.Web.HttpUtility.HtmlDecode( ( (TextSyndicationContent)i.Content ).Text ) );

            List<Dictionary<string, object>> contributors = new List<Dictionary<string, object>>();
            foreach ( var c in i.Contributors )
            {
                contributors.Add( BuildSyndicationPerson( c ) );
            }
            itemDictionary.Add( "Contributors", contributors );

            itemDictionary.Add( "Copyright", i.Copyright == null ? null : i.Copyright.Text );
            itemDictionary.Add( "DetailPageUrl", detailPageUrl );

            itemDictionary.Add( "ElementExtensions", i.ElementExtensions );
            itemDictionary.Add( "Id", i.Id );
            itemDictionary.Add( "LastUpdatedTime", i.LastUpdatedTime.ToLocalTime().DateTime );

            List<Dictionary<string, object>> link = new List<Dictionary<string, object>>();
            foreach ( var l in i.Links )
            {
                link.Add( BuildSyndicationLink( l ) );
            }
            itemDictionary.Add( "Links", link );

            itemDictionary.Add( "PublishDate", i.PublishDate.ToLocalTime().DateTime );
            itemDictionary.Add( "Summary", i.Summary == null ? null : i.Summary.Text );
            itemDictionary.Add( "Title", i.Title == null ? null : i.Title.Text );
        }


        return itemDictionary;

    }


    /// <summary>
    /// Builds a dictionary based on a provided <see cref="System.ServiceModel.Syndication.SyndicationLink"/>
    /// </summary>
    /// <param name="l">The <see cref="System.ServiceModel.Syndication.SyndicationLink"/> to be converted to a dictionary.</param>
    /// <returns>A <see cref="System.Collections.Generic.Dictionary{string,object}"/> representation of the provided syndication link. </returns>
    private static Dictionary<string, object> BuildSyndicationLink( SyndicationLink l )
    {
        Dictionary<string, object> linkDictionary = new Dictionary<string, object>();

        if ( l != null )
        {
            linkDictionary.Add( "AttributeExtensions", l.AttributeExtensions.ToDictionary( a => a.Key.ToString(), a => a.Value ) );
            linkDictionary.Add( "BaseUri", l.BaseUri == null ? null : l.BaseUri.ToString() );
            linkDictionary.Add( "ElementExtensions", l.ElementExtensions );
            linkDictionary.Add( "Length", l.Length );
            linkDictionary.Add( "MediaType", l.MediaType );
            linkDictionary.Add( "RelationshipType", l.RelationshipType );
            linkDictionary.Add( "Title", l.Title );
            linkDictionary.Add( "Uri", l.Uri == null ? null : l.Uri.ToString() );
        }

        return linkDictionary;
    }

    /// <summary>
    /// Builds a dictionary based on a provided <see cref="System.ServiceModel.Syndication.SyndicationPerson"/>
    /// </summary>
    /// <param name="a">The <see cref="System.ServiceModel.Syndication.SyndicationPerson"/> to be converted to a dictionary.</param>
    /// <returns>A <see cref="System.Collections.Generic.Dictionary{string,object}"/> representation of the provided syndication person.</returns>
    private static Dictionary<string, object> BuildSyndicationPerson( SyndicationPerson a )
    {
        Dictionary<string, object> personDictionary = new Dictionary<string, object>();

        if ( a != null )
        {
            personDictionary.Add( "AttributeExtensions", a.AttributeExtensions.ToDictionary( ae => ae.Key.ToString(), ae => ae.Value ) );
            personDictionary.Add( "ElementExtensions", a.ElementExtensions );
            personDictionary.Add( "Email", a.Email );
            personDictionary.Add( "Name", a.Name );
            personDictionary.Add( "Uri", a.Uri == null ? null : a.Uri.ToString() );

        }

        return personDictionary;
    }

    private static string GetFeedCacheKey( string feedUrl )
    {
        return string.Format( "Rock:SyndicationFeed:{0}", feedUrl );
    }

    #endregion

}
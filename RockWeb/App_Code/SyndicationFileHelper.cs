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
            XDocument feed = null;

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create( feedUrl );

            using ( HttpWebResponse resp = (HttpWebResponse)req.GetResponse() )
            {
                if ( resp.StatusCode == HttpStatusCode.OK )
                {
                    XmlReader feedReader = XmlReader.Create( resp.GetResponseStream() );
                    feed = XDocument.Load( feedReader );

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

                string detailPageBaseUrl = string.Empty;
                int detailPageID = 0;

                if(!String.IsNullOrEmpty(detailPage))
                {
                    detailPageID = new Rock.Model.PageService().Get(new Guid(detailPage)).Id;

                    detailPageBaseUrl = new PageReference( detailPageID ).BuildUrl();
                }

  
                Dictionary<string, XNamespace> namespaces = feed.Root.Attributes()
                    .Where( a => a.IsNamespaceDeclaration )
                    .GroupBy( a => a.Name.Namespace == XNamespace.None ? String.Empty : a.Name.LocalName,
                                a => XNamespace.Get( a.Value ) )
                    .ToDictionary( g => g.Key, g => g.First() );


                feedDictionary = BuildElementDictionary( feed.Elements().First(), namespaces );

                if ( feedDictionary.Count == 1  &&  feedDictionary.First().Value.GetType() == typeof(Dictionary<string,object>) )
                {
                    feedDictionary = (Dictionary<string, object>)feedDictionary.First().Value;
                }

                if ( feedDictionary.ContainsKey("lastBuildDate") )
                {
                    feedDictionary["lastBuildDate"] = DateTimeOffset.Parse( feedDictionary["lastBuildDate"].ToString() ).LocalDateTime;
                }

                if ( feedDictionary.ContainsKey("updated") )
                {
                    feedDictionary["updated"] = DateTimeOffset.Parse( feedDictionary["updated"].ToString() ).LocalDateTime;
                }

                List<Dictionary<string, object>> articles = (List<Dictionary<string, object>>)feedDictionary.Where( x => x.Key == "item" || x.Key == "entry" ).FirstOrDefault().Value;

                foreach ( var article in articles )
                {
                    if(article.ContainsKey("id") || article.ContainsKey("guid"))
                    {
                        var idEntry = article.Where( a => a.Key == "id" || a.Key == "guid" ).FirstOrDefault();

                        string idValue = string.Empty;
                        if ( idEntry.Value.GetType() == typeof( Dictionary<string, object> ) )
                        {
                            idValue = ( (Dictionary<string, object>)idEntry.Value )["value"].ToString();
                        }
                        else
                        {
                            idValue = idEntry.Value.GetHashCode().ToString();
                        }

                        Dictionary<string, string> queryString = new Dictionary<string, string>();
                        queryString.Add( "feedItemId", idValue.GetHashCode().ToString() );

                        article.Add( "detailPageUrl", new PageReference( detailPageID, 0, queryString ).BuildUrl() );
                        article.Add( "articleHashCode", idValue.GetHashCode().ToString() );
                    }


                    if ( article.ContainsKey("pubDate") )
                    {
                        article["pubDate"] = DateTimeOffset.Parse( article["pubDate"].ToString() ).LocalDateTime;
                    }

                    if ( article.ContainsKey("updated") )
                    {
                        article["updated"] = DateTimeOffset.Parse( article["updated"].ToString() ).LocalDateTime;
                    }
                
                }

                if(!String.IsNullOrEmpty(detailPageBaseUrl))
                {
                    feedDictionary.Add( "DetailPageBaseUrl", detailPageBaseUrl );
                }
            }



            if ( feedDictionary != null )
            {
                feedCache.Set( GetFeedCacheKey( feedUrl ), feedDictionary, DateTimeOffset.Now.AddMinutes( cacheDuration ) );
            }

        }

        return feedDictionary;
    }


    #endregion

    #region Private Methods

    //private static Dictionary<string, object> BuildRSSDictionary( XDocument feed, string detailPageGuid )
    //{
    //    int detailPageId = 0;
    //    if ( !String.IsNullOrWhiteSpace( detailPageGuid ) )
    //    {
    //        Rock.Model.Page page = new Rock.Model.PageService().Get( new Guid( detailPageGuid ) );
    //        //Dictionary<string, string> queryString = new Dictionary<string, string>();
    //        //queryString.Add( "feedItemId", System.Web.HttpUtility.UrlEncode( i.Id ) );
    //        //detailPageUrl = new PageReference( page.Id, 0, queryString ).BuildUrl();
    //        detailPageId = page.Id;
    //    }

    //    var Namespaces = feed.Root.Attributes()
    //                .Where( a => a.IsNamespaceDeclaration )
    //                .GroupBy( a => a.Name.Namespace == XNamespace.None ? String.Empty : a.Name.LocalName,
    //                            a => XNamespace.Get( a.Value ) )
    //                .ToDictionary( g => g.Key, g => g.First() );

    //    XNamespace  mediaNS = "http://search.yahoo.com/mrss/";
    //    var rss = feed.Descendants( "channel" )
    //        .Select( r => new
    //            {
    //                Title = r.Element( "title" ) == null ? null : r.Element( "title" ).Value,
    //                Link = r.Element( "link" ) == null ? null : r.Element( "link" ).Value,
    //                Description = r.Element( "description" ) == null ? null : r.Element( "description" ).Value,
    //                Image = r.Element( "image" ) == null ? null : r.Descendants( "image" )
    //                        .Select( i => new
    //                        {
    //                            Url = i.Element( "url" ) == null ? null : i.Element( "url" ).Value,
    //                            Title = i.Element( "title" ) == null ? null : i.Element( "title" ).Value,
    //                            Link = i.Element( "link" ) == null ? null : i.Element( "link" ).Value
    //                        } ),
    //                Copyright = r.Element( "copyright" ) == null ? null : r.Element( "copyright" ).Value,
    //                LastBuildDate = r.Element( "lastBuildDate" ) == null ? new DateTimeOffset().LocalDateTime : String.IsNullOrWhiteSpace( r.Element( "lastBuildDate" ).Value ) ? new DateTimeOffset().LocalDateTime : DateTimeOffset.Parse( r.Element( "lastBuildDate" ).Value ).LocalDateTime,
    //                Generator = r.Element( "generator" ) == null ? null : r.Element( "generator" ).Value,
    //                ManagingEditor = r.Element( "managingEditor" ) == null ? null : r.Element( "managingEditor" ).Value,
    //                webMaster = r.Element( "webMaster" ) == null ? null : r.Element( "webMaster" ).Value,
    //                Item = r.Element( "item" ) == null ? null : r.Descendants( "item" )
    //                    .Select( i => new
    //                        {
    //                            Title = i.Element( "title" ) == null ? null : i.Element( "title" ).Value,
    //                            Link = i.Element( "link" ) == null ? null : i.Element( "link" ).Value,
    //                            Comments = i.Element( "comments" ) == null ? null : i.Element( "comments" ).Value,
    //                            PubDate = i.Element( "pubDate" ) == null ? new DateTimeOffset().LocalDateTime : DateTimeOffset.Parse( i.Element( "pubDate" ).Value ).LocalDateTime,
    //                            Creator = i.Elements().Where( c => c.Name.ToString().Contains( "creator" ) ).FirstOrDefault() == null ? null : i.Elements().Where( c => c.Name.ToString().Contains( "creator" ) ).FirstOrDefault().Value,
    //                            Category = i.Element( "category" ) == null ? null : i.Elements( "category" ).Select( c => c.Value ).ToList(),
    //                            Guid = i.Element( "guid" ) == null ? null : i.Element( "guid" ).Value,
    //                            Description = i.Element( "description" ) == null ? null : i.Element( "description" ).Value,
    //                            //Content = i.Elements().Where( c => c.Name.ToString().Contains( "content" ) ).FirstOrDefault() == null ? null : i.Elements().Where( c => c.Name.ToString().Contains( "content" ) ).FirstOrDefault().Value,
    //                            Content = !Namespaces.ContainsKey("content") ||  i.Element(Namespaces["content"] + "encoded") == null ? null : i.Element(Namespaces["content"] + "encoded").Value,
    //                            CommentRSS = i.Elements().Where( c => c.Name.ToString().Contains( "commentRSS" ) ).FirstOrDefault() == null ? null : i.Elements().Where( c => c.Name.ToString().Contains( "commentRSS" ) ).FirstOrDefault().Value,
    //                            OrigLink = i.Elements().Where( l => l.Name.ToString().Contains( "origLink" ) ).FirstOrDefault() == null ? null : i.Elements().Where( l => l.Name.ToString().Contains( "origLink" ) ).FirstOrDefault().Value,
    //                            MediaThumbnail = !Namespaces.ContainsKey( "media" ) || i.Element( Namespaces["media"] + "thumbnail" ) == null && i.Element( Namespaces["media"] + "thumbnail" ).Attribute( "url" ) == null ? null : i.Element( Namespaces["media"] + "thumbnail" ).Attribute( "url" ).Value,
    //                            Media = !Namespaces.ContainsKey( "media" ) || i.Element( Namespaces["media"] + "content" ) == null ? null : i.Elements( Namespaces["media"] + "content" )
    //                                .Select( m => new
    //                                    {
    //                                        Url = m.Attribute( "url" ) == null ? null : m.Attribute( "url" ).Value,
    //                                        Medium = m.Attribute( "medium" ) == null ? null : m.Attribute( "medium" ).Value,
    //                                        Title = !Namespaces.ContainsKey( "media" ) || m.Element( Namespaces["media"] + "title" ) == null ? null : m.Element( Namespaces["media"] + "title" ).Value,
    //                                        Thumbnail = !Namespaces.ContainsKey( "media" ) || ( m.Element( Namespaces["media"] + "thumbnail" ) == null || m.Element( Namespaces["media"] + "thumbnail" ).Attribute( "url" ) == null ) ? null : m.Element( Namespaces["media"] + "thumbnail" ).Attribute( "url" ).Value
    //                                    }

    //                                ),
    //                            DetailPageUrl = BuildDetailPageLink( detailPageId, i.Element( "guid" ).Value.GetHashCode().ToString() )
    //                        } ).OrderByDescending( i => i.PubDate )

    //            }
    //        ).FirstOrDefault();

    //    return rss.GetType().GetProperties().ToDictionary( r => r.Name, r => r.GetValue( rss ) );
    //}

    private static Dictionary<string,object> BuildElementDictionary( XElement feedElement, Dictionary<string, XNamespace> namespaces )
    {
        Dictionary<string, object> elementDictionary = new Dictionary<string, object>();

        if ( feedElement != null)
        {
            if ( feedElement.HasElements )
            {
                var elementTypes = feedElement.Elements()
                    .GroupBy( g => g.Name )
                    .Select( eg => new
                        {
                            elementName = eg.Key,
                            count = eg.Count()
                        }
                    );

                foreach ( var elementType in elementTypes )
                {
                    string updatedName = elementType.elementName.ToString();
                    var elementNamespace = namespaces.Where( n => elementType.elementName.ToString().Contains( n.Value.ToString() ) );

                    if(elementNamespace.Count() > 0)
                    {
                        updatedName = updatedName.Replace( "{" + elementNamespace.First().Value.ToString() + "}", elementNamespace.First().Key + "_" );
                    }

                    if ( elementType.count == 1 )
                    {
                        XElement element = feedElement.Elements( elementType.elementName ).FirstOrDefault();
                        if ( element.HasElements )
                        {
                            elementDictionary.Add( updatedName, BuildElementDictionary( element, namespaces ) );
                        }
                        else if ( element.HasAttributes )
                        {
                            var itemDictionary = element.Attributes().ToDictionary( a => a.Name, a => a.Value );
                            itemDictionary.Add( "value", element.Value );

                            elementDictionary.Add( updatedName, itemDictionary );
                        }

                        else
                        {
                            elementDictionary.Add( updatedName, element.Value );
                        }
                    }
                    else
                    {
                        List<Dictionary<string, object>> elementList = new List<Dictionary<string, object>>();
                        foreach ( var element in feedElement.Elements( elementType.elementName ) )
                        {
                            elementList.Add( BuildElementDictionary( element, namespaces ) );
                        }
                        elementDictionary.Add( updatedName, elementList );
                    }
                }
            }
            else
            {
                string updatedName = feedElement.Name.ToString();
                var elementNamespace = namespaces.Where( n => updatedName.Contains( n.Value.ToString() ) );

                if ( elementNamespace.Count() > 0 )
                {
                    updatedName = updatedName.ToString().Replace( "{" + elementNamespace.First().Value.ToString() + "}", elementNamespace.First().Key + "_" );
                }
                elementDictionary.Add( updatedName.ToString(), feedElement.Value );
            }
        }

        return elementDictionary;
    }

    private static string BuildDetailPageLink( int detailPageId, string hashedId )
    {
        Dictionary<string, string> queryString = new Dictionary<string, string>();
        queryString.Add( "feedItemId", hashedId );

        return new PageReference( detailPageId, 0, queryString ).BuildUrl();
    }


    private static string GetFeedCacheKey( string feedUrl )
    {
        return string.Format( "Rock:SyndicationFeed:{0}", feedUrl );
    }

    #endregion

}
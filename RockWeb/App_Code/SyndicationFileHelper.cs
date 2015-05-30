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
using Rock.Web.Cache;

public class SyndicationFeedHelper
{

    #region Public Methods

    /// <summary>
    /// Clears the specified feed from object cache.
    /// </summary>
    /// <param name="feedUrl">A <see cref="System.String"/> representing the URL of the feed.</param>
    public static void ClearCachedFeed( string feedUrl )
    {
        RockMemoryCache cache = RockMemoryCache.Default;
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

        ObjectCache feedCache = RockMemoryCache.Default;

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
                    detailPageID = new Rock.Model.PageService( new Rock.Data.RockContext() ).Get( new Guid( detailPage ) ).Id;

                    detailPageBaseUrl = new PageReference( detailPageID ).BuildUrl();
                }

                if ( detailPageID > 0 )
                {
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

                if ( feedDictionary.ContainsKey( "item" ) || feedDictionary.ContainsKey( "entry" ) )
                {
                    List<Dictionary<string, object>> articles = (List<Dictionary<string, object>>)feedDictionary.Where( x => x.Key == "item" || x.Key == "entry" ).FirstOrDefault().Value;

                    foreach ( var article in articles )
                    {

                        string idEntry = String.Empty;
                        string idEntryHashed = string.Empty;
                        if ( article.ContainsKey( "id" ) )
                        {
                            idEntry = article["id"].ToString();
                        }

                        if ( article.ContainsKey( "guid" ) )
                        {
                            if ( article["guid"].GetType() == typeof( Dictionary<string, object> ) )
                            {
                                idEntry = ( (Dictionary<string, object>)article["guid"] )["value"].ToString();
                            }
                            else
                            {
                                idEntry = article["guid"].ToString();
                            }
                        }

                        if ( !String.IsNullOrWhiteSpace( idEntry ) )
                        {
                            System.Security.Cryptography.HashAlgorithm hashAlgorithm = System.Security.Cryptography.SHA1.Create();
                            System.Text.StringBuilder sb = new System.Text.StringBuilder();
                            foreach ( byte b in hashAlgorithm.ComputeHash( System.Text.Encoding.UTF8.GetBytes( idEntry ) ) )
                            {
                                sb.Append( b.ToString( "X2" ) );
                            }

                            idEntryHashed = sb.ToString();

                            Dictionary<string, string> queryString = new Dictionary<string, string>();
                            queryString.Add( "feedItemId", idEntryHashed );

                            if ( detailPageID > 0 )
                            {
                                article.Add( "detailPageUrl", new PageReference( detailPageID, 0, queryString ).BuildUrl() );
                            }

                            article.Add( "articleHash", idEntryHashed );
                        }

                        if ( article.ContainsKey( "pubDate" ) )
                        {
                            article["pubDate"] = DateTimeOffset.Parse( article["pubDate"].ToString() ).LocalDateTime;
                        }

                        if ( article.ContainsKey( "updated" ) )
                        {
                            article["updated"] = DateTimeOffset.Parse( article["updated"].ToString() ).LocalDateTime;
                        }

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
                        if ( !String.IsNullOrWhiteSpace( elementNamespace.First().Key ) )
                        {
                            updatedName = updatedName.Replace( "{" + elementNamespace.First().Value.ToString() + "}", elementNamespace.First().Key + "_" );
                        }
                        else
                        {
                            updatedName = updatedName.Replace( "{" + elementNamespace.First().Value.ToString() + "}", String.Empty );
                        }
                        
                    }

                    if ( elementType.count == 1  && elementType.elementName != "item" && elementType.elementName != "entry" )
                    {
                        XElement element = feedElement.Elements( elementType.elementName ).FirstOrDefault();
                        if ( element.HasElements )
                        {
                            elementDictionary.Add( updatedName, BuildElementDictionary( element, namespaces ) );
                        }
                        else if ( element.HasAttributes )
                        {

                            Dictionary<string, object> itemDictionary = new Dictionary<string, object>();
                            foreach ( var item in element.Attributes() )
                            {
                                itemDictionary.Add( item.Name.ToString(), item.Value );
                            }
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
                    if ( !String.IsNullOrWhiteSpace( elementNamespace.First().Key ) )
                    {
                        updatedName = updatedName.Replace( "{" + elementNamespace.First().Value.ToString() + "}", elementNamespace.First().Key + "_" );
                    }
                    else
                    {
                        updatedName = updatedName.Replace( "{" + elementNamespace.First().Value.ToString() + "}", String.Empty );
                    }
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
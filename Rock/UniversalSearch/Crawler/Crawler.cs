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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using HtmlAgilityPack;

using RestSharp;

using Rock.Model;
using Rock.Security;
using Rock.UniversalSearch.Crawler.RobotsTxt;
using Rock.UniversalSearch.IndexModels;

namespace Rock.UniversalSearch.Crawler
{
    /// <summary>
    /// Crawler
    /// </summary>
    public class Crawler
    {
        #region Private Fields
        private string _userAgent = "Rock Web Indexer";
        private List<string> _previouslyCrawledPages = new List<string>();
        private Site _site = null;
        private string _baseUrl = string.Empty;
        private Robots _robotHelper = null;
        private string _startUrl = string.Empty;
        private CookieContainer _cookieContainer = null;
        private Queue<string> _urlQueue = new Queue<string>();

        string[] nonLinkStartsWith = new string[] { "#", "javascript:", "mailto:" };
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Crawler"/> class.
        /// </summary>
        public Crawler() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Crawler"/> class.
        /// </summary>
        /// <param name="site">The site.</param>
        public Crawler( Site site )
        {
            this.CrawlSite( site );
        }

        /// <summary>
        /// Crawls a site.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns></returns>
        public int CrawlSite(Site site)
        {
            return CrawlSite( site, null, null );
        }

        /// <summary>
        /// Crawls a site.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <param name="loginId">The login identifier.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public int CrawlSite( Site site, string loginId, string password )
        {
            _site = site;

            _startUrl = _site.IndexStartingLocation;
            var startingUri = new Uri( _startUrl );

            _baseUrl = startingUri.Scheme + "://" + startingUri.Authority;
            var baseUri = new Uri( _baseUrl );

            // get the robot helper class up and running
            var robotsUri = new Uri( baseUri, "robots.txt" );
            var robotsTxt = GetWebText( robotsUri );
            _robotHelper = Robots.Load( robotsTxt );

            _cookieContainer = new CookieContainer();

            // If a loginId and password were included, get an authentication cookie
            if ( loginId.IsNotNullOrWhiteSpace() && password.IsNotNullOrWhiteSpace() )
            {
                var loginParam = new LoginParameters();
                loginParam.Username = loginId;
                loginParam.Password = password;
                loginParam.Persisted = false;

                var authUri = new Uri( baseUri, "api/Auth/Login" );
                var restClient = new RestClient( authUri );
                restClient.CookieContainer = _cookieContainer;

                var request = new RestRequest( Method.POST );
                request.RequestFormat = DataFormat.Json;
                request.AddBody( loginParam );

                var response = restClient.Execute( request );
            }

            _urlQueue.Enqueue( _site.IndexStartingLocation );
            while ( _urlQueue.Any() )
            {
                string url = _urlQueue.Dequeue().Replace( "?", "\\?" );
                CrawlPage( url );
            }

            return _previouslyCrawledPages.Count;
        }

        /// <summary>
        /// Crawls a page.
        /// </summary>
        /// <param name="url">The url to crawl.</param>
        private void CrawlPage( string url )
        {
            try
            {
                // clean up the url a bit
                url = StandardizeUrl( url );

                if ( !PageHasBeenCrawled( url ) )
                {
                    _previouslyCrawledPages.Add( url );

                    if ( url.StartsWith( _baseUrl ) && _robotHelper.IsPathAllowed( _userAgent, url.Replace( _baseUrl, "" ) ) )
                    {
                        string rawPage = GetWebText( url );

                        if ( !string.IsNullOrWhiteSpace( rawPage ) )
                        {
                            var htmlDoc = new HtmlDocument();
                            htmlDoc.LoadHtml( rawPage );

                            // ensure the page should be indexed by looking at the robot and rock conventions
                            HtmlNode metaRobot = htmlDoc.DocumentNode.SelectSingleNode( "//meta[@name='robot']" );
                            if ( metaRobot == null || metaRobot.Attributes["content"] == null || !metaRobot.Attributes["content"].Value.Contains( "noindex" ) )
                            {
                                // index the page
                                SitePageIndex sitePage = new SitePageIndex();

                                sitePage.Content = GetPageText( htmlDoc );
                                sitePage.Url = url;
                                sitePage.Id = url.MakeInt64HashCode();
                                sitePage.SourceIndexModel = "Rock.Model.Site";
                                sitePage.PageTitle = GetPageTitle( htmlDoc, url );
                                sitePage.DocumentName = sitePage.PageTitle;
                                sitePage.SiteName = _site.Name;
                                sitePage.SiteId = _site.Id;
                                sitePage.LastIndexedDateTime = RockDateTime.Now;

                                HtmlNode metaDescription = htmlDoc.DocumentNode.SelectSingleNode( "//meta[@name='description']" );
                                if ( metaDescription != null && metaDescription.Attributes["content"] != null )
                                {
                                    sitePage.PageSummary = metaDescription.Attributes["content"].Value;
                                }

                                HtmlNode metaKeynotes = htmlDoc.DocumentNode.SelectSingleNode( "//meta[@name='keywords']" );
                                if ( metaKeynotes != null && metaKeynotes.Attributes["content"] != null )
                                {
                                    sitePage.PageKeywords = metaKeynotes.Attributes["content"].Value;
                                }

                                IndexContainer.IndexDocument( sitePage );
                            }

                            if ( metaRobot == null || metaRobot.Attributes["content"] == null || !metaRobot.Attributes["content"].Value.Contains( "nofollow" ) )
                            {
                                // crawl all the links found on the page.
                                var links = ParseLinks( htmlDoc );

                                foreach ( string link in links )
                                {
                                    _urlQueue.Enqueue( link );
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Parses the links in the HTML document.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public List<string> ParseLinks( HtmlDocument page )
        {
            var links = new List<string>();

            if ( page.DocumentNode.SelectNodes( "//a[@href]" ) != null )
            {
                foreach ( HtmlNode link in page.DocumentNode.SelectNodes( "//a[@href]" ) )
                {
                    HtmlAttribute hrefAttribute = link.Attributes["href"];
                    string anchorLink = hrefAttribute.Value;

                    // Skip links that have been flagged to not be followed.
                    if ( link.Attributes.Contains( "rel" ) && link.Attributes["rel"].Value.Contains( "nofollow" ) )
                    {
                        continue;
                    }

                    // check for links that aren't pages (javascript:, mailto:)
                    if ( IsValidLink( anchorLink ) )
                    {

                        // make link absolute
                        var uri = new Uri( anchorLink, UriKind.RelativeOrAbsolute );

                        if ( !uri.IsAbsoluteUri )
                        {
                            uri = new Uri( new Uri( _baseUrl ), uri );
                        }

                        links.Add( uri.ToString() );
                    }
                }
            }

            return links;
        }

        /// <summary>
        /// Determines whether [is valid link] [the specified link].
        /// </summary>
        /// <param name="link">The link.</param>
        /// <returns></returns>
        private bool IsValidLink(string link )
        {
            foreach ( string test in nonLinkStartsWith )
            {
                if ( link.StartsWith( test, StringComparison.OrdinalIgnoreCase ) )
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Standardizes the URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        private string StandardizeUrl(string url )
        {
            // lower case string for comparisons
            url = url.ToLower();

            // remove any named anchors (otherwise the look like unique matches)
            var poundIndex = url.IndexOf( '#' );
            if ( poundIndex > 0 )
            {
                url = url.Substring( 0, url.IndexOf( '#' ) );
            }

            // strip trailing /
            url = url.TrimEnd( '/' );

            return url;
        }

        /// <summary>
        /// Gets the page title.
        /// </summary>
        /// <param name="htmlDoc">The HTML document.</param>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        private string GetPageTitle( HtmlDocument htmlDoc, string url )
        {
            string title;

            if ( htmlDoc.DocumentNode.SelectSingleNode( "//head/title" ) != null )
            {
                title = htmlDoc.DocumentNode.SelectSingleNode( "//head/title" ).InnerText.Trim();

                if ( title.Contains( "|" ) )
                {
                    title = title.Substring( 0, ( title.IndexOf( '|' ) - 1) ).Trim();

                }
                else
                {
                    title = title.Trim();
                }
            }
            else
            {
                title = url;
            }

            return title;
        }

        /// <summary>
        /// Gets the page text.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        private string GetPageText(HtmlDocument page )
        {
            if ( page.DocumentNode.SelectSingleNode( "//body" ) != null )
            {
                StringBuilder cleanText = new StringBuilder();

                foreach ( var childNode in page.DocumentNode.SelectSingleNode( "//body" ).ChildNodes )
                {
                    GetNodeText( childNode, cleanText );
                }

                return cleanText.ToString().SanitizeHtml();
            }
            else
            {
                return page.ToString(); // must be a text file
            }
        }

        /// <summary>
        /// Gets the node text.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="cleanText">The clean text.</param>
        private void GetNodeText(HtmlNode node, StringBuilder cleanText )
        {
            if ( node.Name == "script" || node.Name == "style" )
            {
                return;
            }
                
            // make sure it's not an element we don't want text from 
            var classValue = string.Empty;

            if ( node.Attributes["class"] != null )
            {
                classValue = node.Attributes["class"].Value;

                if ( classValue.Contains( "no-index" ) || classValue.Contains( "nav" ) || classValue.Contains( "navbar" ) )
                {
                    return;
                }
            }

            // add node's text
            if ( node.NodeType == HtmlNodeType.Text )
            {
                if ( node.InnerText.Trim() != "" )
                {
                    cleanText.Append( node.InnerText.Trim() + " " );
                }
            }

            // get any child node text
            foreach( var childNode in node.ChildNodes )
            {
                GetNodeText( childNode, cleanText );
            }
        }

        /// <summary>
        /// Checks to see if the page has been crawled.
        /// </summary>
        /// <param name="url">The url that has potentially been crawled.</param>
        /// <returns>Boolean indicating whether or not the page has been crawled.</returns>
        private bool PageHasBeenCrawled( string url )
        {
            string matchUrl = url.ToLower();

            return _previouslyCrawledPages.Contains( matchUrl );
        }

        /// <summary>
        /// Merges a two lists of strings.
        /// </summary>
        /// <param name="targetList">The list into which to merge.</param>
        /// <param name="sourceList">The list whose values need to be merged.</param>
        private void AddRangeButNoDuplicates( List<string> targetList, List<string> sourceList )
        {
            foreach ( string str in sourceList )
            {
                if ( !targetList.Contains( str ) )
                    targetList.Add( str );
            }
        }

        /// <summary>
        /// Gets the response text for a given url.
        /// </summary>
        /// <param name="url">The url whose text needs to be fetched.</param>
        /// <returns>The text of the response.</returns>
        private string GetWebText( string url )
        {
            try
            {
                Uri requestURL;

                if ( Uri.TryCreate( url, UriKind.RelativeOrAbsolute, out requestURL ) && IsValidUrl( url ) )
                {
                    return GetWebText( requestURL );
                }
            }
            catch { }

            return string.Empty;
        }

        /// <summary>
        /// Gets the response text for a given uri.
        /// </summary>
        /// <param name="requestURL">The request URL.</param>
        /// <returns>
        /// The text of the response.
        /// </returns>
        private string GetWebText( Uri requestURL )
        {
            try
            {
                if ( requestURL.Scheme == Uri.UriSchemeHttp || requestURL.Scheme == Uri.UriSchemeHttps )
                {
                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create( requestURL );
                    request.CookieContainer = _cookieContainer;
                    request.AllowAutoRedirect = false;
                    request.UserAgent = _userAgent;

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    // make sure the response was text
                    if ( response.ContentType.StartsWith( "text" ) )
                    {
                        // handle redirects by indexing the redirect only if it shares the same hostname
                        if ( (int)response.StatusCode >= 300 && (int)response.StatusCode <= 399 )
                        {
                            if ( response.Headers["Location"] != null )
                            {
                                string redirectUrl = response.Headers["Location"];

                                var redirectUri = new Uri( redirectUrl, UriKind.RelativeOrAbsolute );
                                if ( !redirectUri.IsAbsoluteUri )
                                {
                                    redirectUri = new Uri( new Uri( _baseUrl ), redirectUri );
                                }

                                if ( requestURL.Host == redirectUri.Host )
                                {
                                    return GetWebText( redirectUri );
                                }
                                else
                                {
                                    return string.Empty;
                                }
                            }
                            else
                            {
                                return string.Empty;
                            }
                        }

                        string htmlText;

                        using ( Stream stream = response.GetResponseStream() )
                        {
                            using ( StreamReader reader = new StreamReader( stream ) )
                            {
                                htmlText = reader.ReadToEnd();
                            }
                        }

                        return htmlText;
                    }
                }
            }
            catch { }

            return string.Empty;
        }

        /// <summary>
        /// Determines whether the specified URL is valid.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        private bool IsValidUrl(string url )
        {
            if( url.Length > 2000 )
            {
                return false;
            }

            if (url.Contains( " " ) )
            {
                return false;
            }

            if( url.Contains( "../" ) )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public class CrawlUrl
        {
            /// <summary>
            /// Gets or sets the level.
            /// </summary>
            /// <value>
            /// The level.
            /// </value>
            public int Level { get; set; }

            /// <summary>
            /// Gets or sets the URL.
            /// </summary>
            /// <value>
            /// The URL.
            /// </value>
            public string Url { get; set; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Rock.Model;
using Rock.UniversalSearch.Crawler.RobotsTxt;
using Rock.UniversalSearch.IndexModels;
using Rock.Web.Cache;

namespace Rock.UniversalSearch.Crawler
{
    public class Crawler
    {
        #region Private Fields
        private string _userAgent = "Rock Web Indexer";
        private List<CrawledPage> _pages = new List<CrawledPage>();
        private List<string> _externalUrls = new List<string>();
        private List<string> _otherUrls = new List<string>();
        private List<string> _failedUrls = new List<string>();
        private List<string> _exceptions = new List<string>();
        private Site _site = null;
        private Robots _robot = null;
        private string _startUrl = string.Empty;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Crawler"/> class.
        /// </summary>
        public Crawler() { }

        public Crawler( Site site )
        {
            this.CrawlSite( site );
        }

        /// <summary>
        /// Crawls a site.
        /// </summary>
        public int CrawlSite(Site site)
        {
            _site = site;
            
            // get the robot helper class up and running
            _robot = Robots.Load( _site.IndexStartingLocation );

            _startUrl = _site.IndexStartingLocation;
            
            CrawlPage( _site.IndexStartingLocation );

            return _pages.Count;
        }      

        /// <summary>
        /// Crawls a page.
        /// </summary>
        /// <param name="url">The url to crawl.</param>
        private void CrawlPage( string url )
        {
            if ( !PageHasBeenCrawled( url ) && _robot.IsPathAllowed( _userAgent, url ) )
            {
                string rawPage = GetWebText( url );

                if ( !string.IsNullOrWhiteSpace( rawPage ) )
                {
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml( rawPage );
                    
                    // get page title
                    CrawledPage page = new CrawledPage();

                    if ( htmlDoc.DocumentNode.SelectSingleNode( "//body" ) != null )
                    {
                        page.Text = GetPageText( htmlDoc );// htmlDoc.DocumentNode.SelectSingleNode( "//body" ).InnerHtml;
                    }
                    else
                    {
                        page.Text = rawPage;
                    }

                    if ( htmlDoc.DocumentNode.SelectSingleNode( "//head/title" ) != null)
                    {
                        page.Title = htmlDoc.DocumentNode.SelectSingleNode( "//head/title" ).InnerText.Trim();
                    }
                    else
                    {
                        page.Title = url;
                    }
                    
                    page.Url = url;

                    // set whether that page should in indexed
                    HtmlNode metaRobot = htmlDoc.DocumentNode.SelectSingleNode( "//meta[@name='robot']" );
                    if ( metaRobot != null && metaRobot.Attributes["content"] != null && metaRobot.Attributes["content"].Value.Contains( "noindex" ) )
                    {
                        page.AllowsIndex = false;
                    }

                    _pages.Add( page );

                    // index the page
                    // clean up the page title a bit by removing  the site name off it
                    if ( page.AllowsIndex )
                    {
                        SitePageIndex sitePage = new SitePageIndex();
                        sitePage.Id = page.Url.MakeInt64HashCode();
                        sitePage.Content = page.Text.SanitizeHtml();

                        // store only the page title (strip the site name off per Rock convention)
                        if ( page.Title.Contains( "|" ) )
                        {
                            sitePage.PageTitle = page.Title.Substring( 0, (page.Title.IndexOf( '|' ) - 1) ).Trim();

                        }
                        else
                        {
                            sitePage.PageTitle = page.Title.Trim();
                        }
                        
                        sitePage.SiteName = _site.Name;
                        sitePage.SiteId = _site.Id;
                        sitePage.Url = page.Url;
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

                    LinkParser linkParser = new LinkParser();
                    linkParser.ParseLinks( htmlDoc, url, _startUrl );


                    //Add data to main data lists
                    AddRangeButNoDuplicates( _externalUrls, linkParser.ExternalUrls );
                    AddRangeButNoDuplicates( _otherUrls, linkParser.OtherUrls );
                    AddRangeButNoDuplicates( _failedUrls, linkParser.BadUrls );

                    foreach ( string exception in linkParser.Exceptions )
                        _exceptions.Add( exception );


                    //Crawl all the links found on the page.
                    foreach ( string link in linkParser.GoodUrls )
                    {
                        CrawlPage( link );
                    }
                }
            }
        }

        /// <summary>
        /// Gets the page text.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        private string GetPageText(HtmlDocument page )
        {
            StringBuilder cleanText = new StringBuilder();

            foreach(var childNode in page.DocumentNode.SelectSingleNode( "//body" ).ChildNodes )
            {
                GetNodeText( childNode, cleanText );
            }

            return cleanText.ToString();
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
            foreach ( CrawledPage page in _pages )
            {
                if ( page.Url == url )
                    return true;
            }

            return false;
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
            /* for future to impersonate a rock person while indexing
            var ticket = new System.Web.Security.FormsAuthenticationTicket( 1, userName, RockDateTime.Now,
                RockDateTime.Now.Add( System.Web.Security.FormsAuthentication.Timeout ), isPersisted,
                IsImpersonated.ToString(), System.Web.Security.FormsAuthentication.FormsCookiePath );

            var encryptedTicket = System.Web.Security.FormsAuthentication.Encrypt( ticket );

            var httpCookie = new System.Web.HttpCookie( System.Web.Security.FormsAuthentication.FormsCookieName, encryptedTicket );
            httpCookie.HttpOnly = true;
            httpCookie.Path = System.Web.Security.FormsAuthentication.FormsCookiePath;
            httpCookie.Secure = System.Web.Security.FormsAuthentication.RequireSSL;
            if ( System.Web.Security.FormsAuthentication.CookieDomain != null )
                httpCookie.Domain = System.Web.Security.FormsAuthentication.CookieDomain;
            if ( ticket.IsPersistent )
                httpCookie.Expires = ticket.Expiration;*/

            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create( url );
                request.AllowAutoRedirect = false;

                request.UserAgent = _userAgent;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // handle redirects by indexing the redirect only if it shares the same hostname
                if ((int)response.StatusCode >= 300 && (int)response.StatusCode <= 399 )
                {
                    if ( response.Headers["Location"] != null )
                    {
                        string redirectUrl = response.Headers["Location"];

                        var originalUri = new Uri( url );
                        var redirectUri = new Uri( redirectUrl );

                        if (originalUri.Host == redirectUri.Host )
                        {
                            return GetWebText( redirectUrl );
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

                Stream stream = response.GetResponseStream();

                StreamReader reader = new StreamReader( stream );
                string htmlText = reader.ReadToEnd();

                return htmlText;
            }
            catch { }

            return string.Empty;
        }
    }
}

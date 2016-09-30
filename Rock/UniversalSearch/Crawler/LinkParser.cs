using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Rock.UniversalSearch.Crawler
{
    public class LinkParser
    {
        string[] nonLinkStartsWith = new string[] { "#", "javascript:", "mailto:" };
        string[] interalLinkStartsWith = new string[] { @"./", @"/", @"../" };
        
        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public LinkParser() { }

        #endregion

        #region Constants

        private const string _LINK_REGEX = "href=\"[?a-zA-Z./:&\\d_-]+\"";

        #endregion

        #region Private Instance Fields

        private List<string> _goodUrls = new List<string>();
        private List<string> _badUrls = new List<string>();
        private List<string> _otherUrls = new List<string>();
        private List<string> _externalUrls = new List<string>();
        private List<string> _exceptions = new List<string>();

        #endregion

        #region Public Properties

        public List<string> GoodUrls
        {
            get { return _goodUrls; }
            set { _goodUrls = value; }
        }

        public List<string> BadUrls
        {
            get { return _badUrls; }
            set { _badUrls = value; }
        }

        public List<string> OtherUrls
        {
            get { return _otherUrls; }
            set { _otherUrls = value; }
        }

        public List<string> ExternalUrls
        {
            get { return _externalUrls; }
            set { _externalUrls = value; }
        }

        public List<string> Exceptions
        {
            get { return _exceptions; }
            set { _exceptions = value; }
        }

        #endregion

        /// <summary>
        /// Parses a page looking for links.
        /// </summary>
        /// <param name="page">The page whose text is to be parsed.</param>
        /// <param name="sourceUrl">The source url of the page.</param>
        public void ParseLinks( HtmlDocument page, string sourceUrl, string startUrl )
        {
            if ( page.DocumentNode.SelectNodes( "//a[@href]" ) != null )
            {
                foreach ( HtmlNode link in page.DocumentNode.SelectNodes( "//a[@href]" ) )
                {
                    HtmlAttribute hrefAttribute = link.Attributes["href"];
                    string anchorLink = hrefAttribute.Value;

                    var linkType = DetermineLinkType( anchorLink, startUrl );

                    switch ( linkType )
                    {
                        case LinkType.Internal:
                            {
                                GoodUrls.Add( MakeAbsoluteLink( anchorLink, sourceUrl ) );
                                break;
                            }
                        case LinkType.External:
                            {
                                _externalUrls.Add( anchorLink );
                                break;
                            }
                        default:
                            {
                                _badUrls.Add( anchorLink );
                                break;
                            }
                    }
                }
            }
        }

        /// <summary>
        /// Determines the type of the link.
        /// </summary>
        /// <param name="link">The link.</param>
        /// <param name="startingUrl">The starting URL.</param>
        /// <returns></returns>
        private LinkType DetermineLinkType(string link, string startingUrl )
        {
            // check for nonlinks
            foreach(string test in nonLinkStartsWith )
            {
                if (link.StartsWith(test, StringComparison.OrdinalIgnoreCase ) )
                {
                    return LinkType.NonLink;
                }
            }

            // check for links to GetFile
            if (link.Contains( "GetFile.ashx" ) )
            {
                return LinkType.File;
            }

            foreach(string test in interalLinkStartsWith )
            {
                if ( link.StartsWith( test, StringComparison.OrdinalIgnoreCase ) )
                {
                    return LinkType.Internal;
                }
            }

            if ( link.StartsWith( startingUrl ) )
            {
                return LinkType.Internal;
            }

            if ( link.StartsWith( "http" ) )
            {
                return LinkType.External;
            }

            return LinkType.Internal;
        }

        /// <summary>
        /// Makes the absolute link.
        /// </summary>
        /// <param name="link">The link.</param>
        /// <param name="originatingLink">The originating link.</param>
        /// <returns></returns>
        private string MakeAbsoluteLink(string link, string originatingLink )
        {
            if ( link.StartsWith( "http" ) )
            {
                return link;
            }

            if ( link.StartsWith( "/" ) )
            {
                Uri originatingUri = new Uri( originatingLink );
                return string.Format( "{0}://{1}{2}", originatingUri.Scheme, originatingUri.Host, link );
            }

            if ( link.StartsWith( "./" ) )
            {
                return originatingLink.TrimEnd( '/' ) + link.TrimStart( '.' );
            }

            if ( link.StartsWith( "../" ) )
            {
                ResolveRelativePaths( link, originatingLink );
            }

            return originatingLink + link;
        }

        /// <summary>
        /// Needed a method to turn a relative path into an absolute path. And this seems to work.
        /// </summary>
        /// <param name="relativeUrl">The relative url.</param>
        /// <param name="originatingUrl">The url that contained the relative url.</param>
        /// <returns>A url that was relative but is now absolute.</returns>
        private static string ResolveRelativePaths( string relativeUrl, string originatingUrl )
        {
            string resolvedUrl = String.Empty;

            string[] relativeUrlArray = relativeUrl.Split( new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries );
            string[] originatingUrlElements = originatingUrl.Split( new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries );
            int indexOfFirstNonRelativePathElement = 0;
            for ( int i = 0; i <= relativeUrlArray.Length - 1; i++ )
            {
                if ( relativeUrlArray[i] != ".." )
                {
                    indexOfFirstNonRelativePathElement = i;
                    break;
                }
            }

            int countOfOriginatingUrlElementsToUse = originatingUrlElements.Length - indexOfFirstNonRelativePathElement - 1;
            for ( int i = 0; i <= countOfOriginatingUrlElementsToUse - 1; i++ )
            {
                if ( originatingUrlElements[i] == "http:" || originatingUrlElements[i] == "https:" )
                    resolvedUrl += originatingUrlElements[i] + "//";
                else
                    resolvedUrl += originatingUrlElements[i] + "/";
            }

            for ( int i = 0; i <= relativeUrlArray.Length - 1; i++ )
            {
                if ( i >= indexOfFirstNonRelativePathElement )
                {
                    resolvedUrl += relativeUrlArray[i];

                    if ( i < relativeUrlArray.Length - 1 )
                        resolvedUrl += "/";
                }
            }

            return resolvedUrl;
        }

        /// <summary>
        /// Is the url to an external site?
        /// </summary>
        /// <param name="url">The url whose externality of destination is in question.</param>
        /// <returns>Boolean indicating whether or not the url is to an external destination.</returns>
        /*private static bool IsExternalUrl( string url, string startUrl )
        {
            if ( url.IndexOf( startUrl ) > -1 )
            {
                return false;
            }
            else if ( url.Length > 7 && ( url.Substring( 0, 7 ) == "http://" || url.Substring( 0, 3 ) == "www" || url.Substring( 0, 7 ) == "https://" ))
            {
                return true;
            }

            return false;
        }*/

        /// <summary>
        /// Is the value of the href pointing to a web page?
        /// </summary>
        /// <param name="foundHref">The value of the href that needs to be interogated.</param>
        /// <returns>Boolen </returns>
        /*private static bool IsAWebPage( string foundHref )
        {
            if ( foundHref.IndexOf( "javascript:" ) == 0 )
            {
                return false;
            }

            if ( foundHref.IndexOf( "mailto:" ) == 0 )
            {
                return false;
            }

            if ( foundHref.StartsWith( "#" ) )
            {
                return false;
            }

            string extension = foundHref.Substring( foundHref.LastIndexOf( "." ) + 1, foundHref.Length - foundHref.LastIndexOf( "." ) - 1 );
            switch ( extension )
            {
                case "jpg":
                case "css":
                    return false;
                default:
                    return true;
            }

        }*/

        private enum LinkType { Internal, External, NonLink, File }
    }
}

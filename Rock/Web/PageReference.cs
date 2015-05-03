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
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Routing;

using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Model;

namespace Rock.Web
{
    /// <summary>
    /// Helper class to work with the PageReference field type
    /// </summary>
    public class PageReference
    {

        #region Properties

        /// <summary>
        /// Gets or sets the page id.
        /// </summary>
        public int PageId { get; set; }

        /// <summary>
        /// Gets the route id.
        /// </summary>
        public int RouteId { get; set; }

        /// <summary>
        /// Gets the route parameters.
        /// </summary>
        /// <value>
        /// The route parameters.
        /// </value>
        public Dictionary<string, string> Parameters { get; set; }

        /// <summary>
        /// Gets the query string.
        /// </summary>
        /// <value>
        /// The query string.
        /// </value>
        public NameValueCollection QueryString { get; set; }

        /// <summary>
        /// Gets or sets the bread crumbs.
        /// </summary>
        /// <value>
        /// The bread crumbs.
        /// </value>
        public List<BreadCrumb> BreadCrumbs { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid
        {
            get
            {
                if ( PageId != 0 )
                    return true;
                else
                    return false;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PageReference"/> class.
        /// </summary>
        public PageReference()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageReference" /> class.
        /// </summary>
        /// <param name="linkedPageValue">The linked page value.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="queryString">The query string.</param>
        public PageReference( string linkedPageValue, Dictionary<string, string> parameters = null, NameValueCollection queryString = null  )
        {
            if ( !string.IsNullOrWhiteSpace( linkedPageValue ) )
            {
                string[] items = linkedPageValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

                //// linkedPageValue is in format "Page.Guid,PageRoute.Guid"
                //// If only the Page.Guid is specified this is just a reference to a page without a special route
                //// In case the PageRoute record can't be found from PageRoute.Guid (maybe the pageroute was deleted), fall back to the Page without a PageRoute

                Guid pageGuid = Guid.Empty;
                if ( items.Length > 0 )
                {
                    if ( Guid.TryParse( items[0], out pageGuid ) )
                    {
                        var pageCache = PageCache.Read( pageGuid );
                        if ( pageCache != null )
                        {
                            // Set the page
                            PageId = pageCache.Id;

                            Guid pageRouteGuid = Guid.Empty;
                            if ( items.Length == 2 )
                            {
                                if ( Guid.TryParse( items[1], out pageRouteGuid ) )
                                {
                                    var pageRouteInfo = pageCache.PageRoutes.FirstOrDefault( a => a.Guid == pageRouteGuid );
                                    if ( pageRouteInfo != null )
                                    {
                                        // Set the route
                                        RouteId = pageRouteInfo.Id;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Parameters = parameters;
            QueryString = queryString;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageReference" /> class.
        /// </summary>
        /// <param name="pageId">The page id.</param>
        public PageReference( int pageId )
        {
            Parameters = new Dictionary<string, string>();
            PageId = pageId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageReference"/> class.
        /// </summary>
        /// <param name="pageId">The page id.</param>
        /// <param name="routeId">The route id.</param>
        public PageReference( int pageId, int routeId )
            : this( pageId )
        {
            RouteId = routeId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageReference" /> class.
        /// </summary>
        /// <param name="pageId">The page id.</param>
        /// <param name="routeId">The route id.</param>
        /// <param name="parameters">The route parameters.</param>
        public PageReference( int pageId, int routeId, Dictionary<string, string> parameters )
            : this( pageId, routeId )
        {
            Parameters = parameters;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageReference" /> class.
        /// </summary>
        /// <param name="pageId">The page id.</param>
        /// <param name="routeId">The route id.</param>
        /// <param name="parameters">The route parameters.</param>
        /// <param name="queryString">The query string.</param>
        public PageReference( int pageId, int routeId, Dictionary<string, string> parameters, NameValueCollection queryString )
            : this( pageId, routeId, parameters )
        {
            QueryString = queryString;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageReference" /> class.
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        public PageReference( PageReference pageReference )
            : this( pageReference.PageId, pageReference.RouteId, pageReference.Parameters, pageReference.QueryString )
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Builds the URL.
        /// </summary>
        /// <returns></returns>
        public string BuildUrl()
        {
            string url = string.Empty;

            var parms = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

            // Add any route parameters
            if (Parameters != null)
            {
                foreach(var route in Parameters)
                {
                    if ( !parms.ContainsKey( route.Key ) )
                    {
                        parms.Add( route.Key, route.Value );
                    }
                }
            }

            // merge parms from query string to the parms dictionary to get a single list of parms
            // skipping those parms that are already in the dictionary
            if ( QueryString != null )
            {
                foreach ( string key in QueryString.AllKeys )
                {
                    // check that the dictionary doesn't already have this key
                    if ( key != null && !parms.ContainsKey( key ) && QueryString[key] != null )
                        parms.Add( key, QueryString[key].ToString() );
                }
            }

            // See if there's a route that matches all parms
            if ( RouteId == 0 )
            {
                RouteId = GetRouteIdFromPageAndParms() ?? 0;
            }

            // load route URL 
            if ( RouteId != 0 )
            {
                url = BuildRouteURL( parms );
            }

            // build normal url if route url didn't process
            if ( url == string.Empty )
            {
                url = "page/" + PageId;

                // add parms to the url
                if ( parms != null )
                {
                    string delimitor = "?";
                    foreach ( KeyValuePair<string, string> parm in parms )
                    {
                        url += delimitor + HttpUtility.UrlEncode(parm.Key) + "=" + HttpUtility.UrlEncode( parm.Value );
                        delimitor = "&";
                    }
                }
            }

            // add base path to url -- Fixed bug #84
            url = ( HttpContext.Current.Request.ApplicationPath == "/" ) ? "/" + url : HttpContext.Current.Request.ApplicationPath + "/" + url;

            return url;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the route id from page and parms.
        /// </summary>
        /// <returns></returns>
        private int? GetRouteIdFromPageAndParms()
        {
            var pageCache = PageCache.Read( PageId );
            if ( pageCache != null && pageCache.PageRoutes.Any() )
            {
                var r = new Regex( @"(?<={)[A-Za-z0-9\-]+(?=})" );

                foreach ( var item in pageCache.PageRoutes )
                {
                    // If route contains no parameters, and no parameters were provided, return this route
                    var matches = r.Matches( item.Route);
                    if ( matches.Count == 0 && ( Parameters == null || Parameters.Count == 0 ) )
                    {
                        return item.Id;
                    }

                    // If route contains the same number of parameters as provided, check to see if they all match names
                    if ( matches.Count > 0 && Parameters != null && Parameters.Count == matches.Count )
                    {
                        bool matchesAllParms = true;

                        foreach ( Match match in matches )
                        {
                            if ( !Parameters.ContainsKey( match.Value ) )
                            {
                                matchesAllParms = false;
                                break;
                            }
                        }

                        if ( matchesAllParms )
                        {
                            return item.Id;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Builds the route URL.
        /// </summary>
        /// <param name="parms">The parms.</param>
        /// <returns></returns>
        private string BuildRouteURL( Dictionary<string, string> parms )
        {
            string routeUrl = string.Empty;

            foreach ( Route route in RouteTable.Routes )
            {
                if ( route.DataTokens != null && route.DataTokens.ContainsKey( "RouteId" ) && route.DataTokens["RouteId"].ToString() == RouteId.ToString() )
                {
                    routeUrl = route.Url;
                    break;
                }
            }

            // get dictionary of parms in the route
            Dictionary<string, string> routeParms = new Dictionary<string, string>();
            bool allRouteParmsProvided = true;

            var r = new Regex( @"{([A-Za-z0-9\-]+)}" );
            foreach ( Match match in r.Matches( routeUrl ) )
            {
                // add parm to dictionary
                routeParms.Add( match.Groups[1].Value, match.Value );

                // check that a value for that parm is available
                if ( parms == null || !parms.ContainsKey( match.Groups[1].Value ) )
                    allRouteParmsProvided = false;
            }

            // if we have a value for all route parms build route url
            if ( allRouteParmsProvided )
            {
                // merge route parm values
                foreach ( KeyValuePair<string, string> parm in routeParms )
                {
                    // merge field
                    routeUrl = routeUrl.Replace( parm.Value, parms[parm.Key] );

                    // remove parm from dictionary
                    parms.Remove( parm.Key );
                }

                // add remaining parms to the query string
                if ( parms != null )
                {
                    string delimitor = "?";
                    foreach ( KeyValuePair<string, string> parm in parms )
                    {
                        routeUrl += delimitor + parm.Key + "=" + HttpUtility.UrlEncode( parm.Value );
                        delimitor = "&";
                    }
                }

                return routeUrl;
            }
            else
                return string.Empty;
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Gets the parent page references.
        /// </summary>
        /// <returns></returns>
        public static List<PageReference> GetParentPageReferences(RockPage rockPage, PageCache currentPage, PageReference currentPageReference)
        {
            // Get previous page references in nav history
            var pageReferenceHistory = HttpContext.Current.Session["RockPageReferenceHistory"] as List<PageReference>;
                        
            // Current page heirarchy references
            var pageReferences = new List<PageReference>();

            if (currentPage != null)
            {
                var parentPage = currentPage.ParentPage;
                if ( parentPage != null )
                {
                    var currentParentPages = parentPage.GetPageHierarchy();
                    if ( currentParentPages != null && currentParentPages.Count > 0 )
                    {
                        currentParentPages.Reverse();
                        foreach ( PageCache page in currentParentPages )
                        {
                            PageReference parentPageReference = null;
                            if ( pageReferenceHistory != null )
                            {
                                parentPageReference = pageReferenceHistory.Where( p => p.PageId == page.Id ).FirstOrDefault();
                            }

                            if ( parentPageReference == null )
                            {
                                parentPageReference = new PageReference( );
                                parentPageReference.PageId = page.Id;

                                parentPageReference.BreadCrumbs = new List<BreadCrumb>();
                                parentPageReference.QueryString = new NameValueCollection();
                                parentPageReference.Parameters = new Dictionary<string, string>();

                                string bcName = page.BreadCrumbText;
                                if ( bcName != string.Empty )
                                {
                                    parentPageReference.BreadCrumbs.Add( new BreadCrumb( bcName, parentPageReference.BuildUrl() ) );
                                }

                                foreach ( var block in page.Blocks.Where( b=> b.BlockLocation == Model.BlockLocation.Page) )
                                {
                                    try
                                    {
                                        System.Web.UI.Control control = rockPage.TemplateControl.LoadControl(block.BlockType.Path);
                                        if (control is RockBlock)
                                        {
                                            RockBlock rockBlock = control as RockBlock;
                                            rockBlock.SetBlock(page, block);
                                            rockBlock.GetBreadCrumbs(parentPageReference).ForEach(c => parentPageReference.BreadCrumbs.Add(c));
                                        }
                                        control = null;
                                    }
                                    catch (Exception ex)
                                    {
                                        ExceptionLogService.LogException(ex, HttpContext.Current, currentPage.Id, currentPage.Layout.SiteId);
                                    }
                                }

                            }

                            parentPageReference.BreadCrumbs.ForEach( c => c.Active = false );
                            pageReferences.Add( parentPageReference );
                        }
                    }
                }
            }

            return pageReferences;
        }

        /// <summary>
        /// Saves the history.
        /// </summary>
        /// <param name="pageReferences">The page references.</param>
        public static void SavePageReferences( List<PageReference> pageReferences)
        {
            HttpContext.Current.Session["RockPageReferenceHistory"] = pageReferences;
        }

        #endregion
    }
}
//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Caching;
using System.Web.Compilation;
using System.Web.Routing;

namespace Rock.Web
    
    /// <summary>
    /// Rock custom route handler
    /// </summary>
    public sealed class RockRouteHandler : IRouteHandler
        
        /// <summary>
        /// Determine the logical page being requested by evaluating the routedata, or querystring and
        /// then loading the appropriate layout (ASPX) page
        /// </summary>
        /// <param name="requestContext"></param>
        /// <returns></returns>
        System.Web.IHttpHandler IRouteHandler.GetHttpHandler( RequestContext requestContext )
            
            if ( requestContext == null )
                throw new ArgumentNullException( "requestContext" );

            string pageId = "";
            int routeId = -1;

            // Pages using the default routing URL will have the page id in the RouteData.Values collection
            if ( requestContext.RouteData.Values["PageId"] != null )
                
                pageId = (string)requestContext.RouteData.Values["PageId"];
            }
            // Pages that use a custom URL route will have the page id in the RouteDate.DataTokens collection
            else if ( requestContext.RouteData.DataTokens["PageId"] != null )
                
                pageId = (string)requestContext.RouteData.DataTokens["PageId"];
                routeId = Int32.Parse( (string)requestContext.RouteData.DataTokens["RouteId"] );
            }
            // If page has not been specified get the site by the domain and use the site's default page
            else
                
                string host = requestContext.HttpContext.Request.Url.Host;
                string cacheKey = "Rock:DomainSites";

                ObjectCache cache = MemoryCache.Default;
                Dictionary<string, int> sites = cache[cacheKey] as Dictionary<string, int>;
                if ( sites == null )
                    sites = new Dictionary<string, int>();

                Rock.Web.Cache.SiteCache site = null;
                if ( sites.ContainsKey( host ) )
                    site = Rock.Web.Cache.SiteCache.Read( sites[host] );
                else
                    
                    Rock.Cms.SiteDomainService siteDomainService = new Rock.Cms.SiteDomainService();
                    Rock.Cms.SiteDomain siteDomain = siteDomainService.GetByDomainContained( requestContext.HttpContext.Request.Url.Host );
                    if ( siteDomain != null )
                        
                        sites.Add( host, siteDomain.SiteId );
                        site = Rock.Web.Cache.SiteCache.Read( siteDomain.SiteId );
                    }
                }

                cache[cacheKey] = sites;

                if ( site != null && site.DefaultPageId.HasValue )
                    pageId = site.DefaultPageId.Value.ToString();

                if ( string.IsNullOrEmpty( pageId ) )
                    throw new SystemException( "Invalid Site Configuration" );
            }

            Rock.Web.Cache.PageCache page = null;

            if ( !string.IsNullOrEmpty( pageId ) )
                
                page = Rock.Web.Cache.PageCache.Read( Convert.ToInt32( pageId ) );
                if ( page == null )
                    
                    return new HttpHandlerError( 404 );
                }
            }

            if ( page != null && !String.IsNullOrEmpty( page.LayoutPath ) )
                
                // load the route id
                page.RouteId = routeId;

                // Return the page using the cached route
                Rock.Web.UI.RockPage cmsPage = ( Rock.Web.UI.RockPage )BuildManager.CreateInstanceFromVirtualPath( page.LayoutPath, typeof( Rock.Web.UI.RockPage ) );
                cmsPage.CurrentPage = page;
                return cmsPage;
            }
            else
                
                string theme = "RockCms";
                string layout = "Default";
                string layoutPath = Rock.Web.Cache.PageCache.FormatPath( theme, layout );

                if ( page != null )
                    
                    // load the route id
                    page.RouteId = routeId;

                    theme = page.Site.Theme;
                    layout = page.Layout;
                    layoutPath = Rock.Web.Cache.PageCache.FormatPath( theme, layout );

                    page.LayoutPath = layoutPath;
                }
                else
                    page = Cache.PageCache.Read( new Cms.Page() );

                try
                    
                    // Return the page for the selected theme and layout
                    Rock.Web.UI.RockPage cmsPage = ( Rock.Web.UI.RockPage )BuildManager.CreateInstanceFromVirtualPath( layoutPath, typeof( Rock.Web.UI.RockPage ) );
                    cmsPage.CurrentPage = page;
                    return cmsPage;
                }
                catch ( System.Web.HttpException )
                    
                    // The Selected theme and/or layout didn't exist, attempt first to use the default layout in the selected theme
                    layout = "Default";

                    // If not using the Rock theme, verify that default Layout exists in the selected theme directory
                    if ( theme != "RockCms" &&
                        !File.Exists( requestContext.HttpContext.Server.MapPath( string.Format( "~/Themes/    0}/Layouts/Default.aspx", theme ) ) ) )
                        
                        // If default layout doesn't exist in the selected theme, switch to the Default layout
                        theme = "RockCms";
                        layout = "Default";
                    }

                    // Build the path to the aspx file to
                    layoutPath = Rock.Web.Cache.PageCache.FormatPath( theme, layout );

                    if ( page != null )
                        page.LayoutPath = layoutPath;

                    // Return the default layout and/or theme
                    Rock.Web.UI.RockPage cmsPage = ( Rock.Web.UI.RockPage )BuildManager.CreateInstanceFromVirtualPath( layoutPath, typeof( Rock.Web.UI.RockPage ) );
                    cmsPage.CurrentPage = page;
                    return cmsPage;
                }
            }
        }
    }

    /// <summary>
    /// Handler used when an error occurrs
    /// </summary>
    public class HttpHandlerError : System.Web.IHttpHandler
        
        /// <summary>
        /// Gets the status code.
        /// </summary>
        public int StatusCode      get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHandlerError"/> class.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        public HttpHandlerError( int statusCode )
            
            StatusCode = statusCode;
        }

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"/> instance.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler"/> instance is reusable; otherwise, false.</returns>
        public bool IsReusable
            
            get      return true; }
        }

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"/> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext"/> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public void ProcessRequest( System.Web.HttpContext context )
            
            context.Response.StatusCode = StatusCode;
            context.Response.End();
            return;
        }
    }

}

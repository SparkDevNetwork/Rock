//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;
using Rock.Model;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class PagesController : IHasCustomRoutes
    {
        /// <summary>
        /// Adds the routes.
        /// </summary>
        /// <param name="routes">The routes.</param>
        public void AddRoutes( System.Web.Routing.RouteCollection routes )
        {
            routes.MapHttpRoute(
                name: "PagesGetChildren",
                routeTemplate: "api/Pages/GetChildren/{id}",
                defaults: new
                {
                    controller = "Pages",
                    action = "GetChildren"
                } );
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public IQueryable<PageItem> GetChildren( int id)
        {
            IQueryable<Page> qry;
            if ( id == 0 )
            {
                qry = Get().Where( a => a.ParentPageId == null );
            }
            else
            {
                qry = Get().Where( a => a.ParentPageId == id );
            }

            List<Page> pageList = qry.OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            List<PageItem> pageItemList = new List<PageItem>();
            foreach ( var page in pageList )
            {
                var pageItem = new PageItem();
                pageItem.Id = page.Id;
                pageItem.Name = page.Name;

                pageItemList.Add( pageItem );
            }

            // try to quickly figure out which items have Children
            List<int> resultIds = pageItemList.Select( a => a.Id ).ToList();

            var qryHasChildren = from x in Get().Select( a => a.ParentPageId )
                                 where resultIds.Contains( x.Value )
                                 select x.Value;

            var qryHasChildrenList = qryHasChildren.ToList();

            foreach ( var g in pageItemList )
            {
                g.HasChildren = qryHasChildrenList.Any( a => a == g.Id );
            }

            return pageItemList.AsQueryable();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PageItem
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has children.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has children; otherwise, <c>false</c>.
        /// </value>
        public bool HasChildren { get; set; }
    }
}

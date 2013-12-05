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
using Rock.Web.UI.Controls;

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
        public IQueryable<TreeViewItem> GetChildren( int id)
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
            List<TreeViewItem> pageItemList = new List<TreeViewItem>();
            foreach ( var page in pageList )
            {
                var pageItem = new TreeViewItem();
                pageItem.Id = page.Id.ToString();
                pageItem.Name = page.Name;

                pageItemList.Add( pageItem );
            }

            // try to quickly figure out which items have Children
            List<int> resultIds = pageList.Select( a => a.Id ).ToList();

            var qryHasChildren = from x in Get().Select( a => a.ParentPageId )
                                 where resultIds.Contains( x.Value )
                                 select x.Value;

            var qryHasChildrenList = qryHasChildren.ToList();

            foreach ( var g in pageItemList )
            {
                int pageId = int.Parse( g.Id );
                g.HasChildren = qryHasChildrenList.Any( a => a == pageId );
                g.IconCssClass = "fa fa-file-o";
            }

            return pageItemList.AsQueryable();
        }
    }
}

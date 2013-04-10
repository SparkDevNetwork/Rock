//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class FundsController : IHasCustomRoutes
    {
        /// <summary>
        /// Adds the routes.
        /// </summary>
        /// <param name="routes">The routes.</param>
        public void AddRoutes( RouteCollection routes )
        {
            routes.MapHttpRoute(
                name: "FundsGetChildren",
                routeTemplate: "api/Funds/GetChildren/{id}",
                defaults: new
                {
                    controller = "Funds",
                    action = "GetChildren"
                } );
        }

        [Authenticate]
        public IQueryable<FundItem> GetChildren( int id )
        {
            IQueryable<Fund> qry;

            if ( id == 0 )
            {
                qry = Get().Where( f => f.ParentFund == null );
            }
            else
            {
                qry = Get().Where( f => f.ParentFundId == id );
            }

            var fundList = qry.OrderBy( f => f.Order ).ThenBy( f => f.Name ).ToList();
            var fundItemList = fundList.Select( fund => new FundItem
                {
                    Id = fund.Id,
                    Name = HttpUtility.HtmlEncode( fund.PublicName )
                } ).ToList();

            var resultIds = fundItemList.Select( f => f.Id );
            var qryHasChildren = from f in Get().Select( f => f.ParentFundId )
                                 where resultIds.Contains( f.Value )
                                 select f.Value;

            foreach ( var fundItem in fundItemList )
            {
                fundItem.HasChildren = qryHasChildren.Any( f => f == fundItem.Id );
                fundItem.IconCssClass = "icon-file-alt";
            }

            return fundItemList.AsQueryable();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FundItem
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

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        public string IconCssClass { get; set; }
    }
}

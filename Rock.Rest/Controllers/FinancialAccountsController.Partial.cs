//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

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
    public partial class FinancialAccountsController : IHasCustomRoutes
    {
        /// <summary>
        /// Adds the routes.
        /// </summary>
        /// <param name="routes">The routes.</param>
        public void AddRoutes( RouteCollection routes )
        {
            routes.MapHttpRoute(
                name: "FinancialAccountsGetChildren",
                routeTemplate: "api/FinancialAccounts/GetChildren/{id}",
                defaults: new
                {
                    controller = "FinancialAccounts",
                    action = "GetChildren"
                } );
        }

        [Authenticate]
        public IQueryable<AccountItem> GetChildren( int id )
        {
            IQueryable<FinancialAccount> qry;

            if ( id == 0 )
            {
                qry = Get().Where( f => f.ParentAccount == null );
            }
            else
            {
                qry = Get().Where( f => f.ParentAccountId == id );
            }

            var accountList = qry.OrderBy( f => f.Order ).ThenBy( f => f.Name ).ToList();
            var accountItemList = accountList.Select( fund => new AccountItem
                {
                    Id = fund.Id,
                    Name = HttpUtility.HtmlEncode( fund.PublicName )
                } ).ToList();

            var resultIds = accountItemList.Select( f => f.Id );
            var qryHasChildren = from f in Get().Select( f => f.ParentAccountId )
                                 where resultIds.Contains( f.Value )
                                 select f.Value;

            foreach ( var accountItem in accountItemList )
            {
                accountItem.HasChildren = qryHasChildren.Any( f => f == accountItem.Id );
                accountItem.IconCssClass = "icon-file-alt";
            }

            return accountItemList.AsQueryable();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AccountItem
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

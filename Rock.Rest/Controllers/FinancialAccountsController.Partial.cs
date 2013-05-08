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
using Rock.Web.UI.Controls;

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
        public IQueryable<TreeViewItem> GetChildren( int id )
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
            var accountItemList = accountList.Select( fund => new TreeViewItem
                {
                    Id = fund.Id.ToString(),
                    Name = HttpUtility.HtmlEncode( fund.PublicName )
                } ).ToList();

            var resultIds = accountList.Select( f => f.Id );
            var qryHasChildren = from f in Get().Select( f => f.ParentAccountId )
                                 where  resultIds.Contains( f.Value )
                                 select f.Value;

            foreach ( var accountItem in accountItemList )
            {
                int accountId = int.Parse( accountItem.Id );

                accountItem.HasChildren = qryHasChildren.Any( f => f == accountId );
                accountItem.IconCssClass = "icon-file-alt";
            }

            return accountItemList.AsQueryable();
        }
    }
}

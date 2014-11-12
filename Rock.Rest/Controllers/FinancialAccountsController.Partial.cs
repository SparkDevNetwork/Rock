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

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
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
            var accountItemList = accountList.Select( a => new TreeViewItem
                {
                    Id = a.Id.ToString(),
                    Name = HttpUtility.HtmlEncode( a.PublicName )
                } ).ToList();

            var resultIds = accountList.Select( f => f.Id );
            var qryHasChildren = from f in Get().Select( f => f.ParentAccountId )
                                 where  resultIds.Contains( f.Value )
                                 select f.Value;

            foreach ( var accountItem in accountItemList )
            {
                int accountId = int.Parse( accountItem.Id );

                accountItem.HasChildren = qryHasChildren.Any( f => f == accountId );
                accountItem.IconCssClass = "fa fa-file-o";
            }

            return accountItemList.AsQueryable();
        }
    }
}

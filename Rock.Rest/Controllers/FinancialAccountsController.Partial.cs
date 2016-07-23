﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
    public partial class FinancialAccountsController
    {
        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="activeOnly">if set to <c>true</c> [active only].</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/FinancialAccounts/GetChildren/{id}/{activeOnly}" )]
        public IQueryable<TreeViewItem> GetChildren( int id, bool activeOnly )
        {
            IQueryable<FinancialAccount> qry;

            if ( id == 0 )
            {
                qry = Get().Where( f => 
                    f.ParentAccountId.HasValue == false );
            }
            else
            {
                qry = Get().Where( f => 
                    f.ParentAccountId.HasValue && 
                    f.ParentAccountId.Value == id );
            }

            if ( activeOnly )
            {
                qry = qry
                    .Where( f => f.IsActive == true );
            }

            var accountList = qry
                .OrderBy( f => f.Order )
                .ThenBy( f => f.Name )
                .ToList();

            var accountItemList = accountList.Select( a => new TreeViewItem
                {
                    Id = a.Id.ToString(),
                    Name = HttpUtility.HtmlEncode( a.PublicName )
                } ).ToList();

            var resultIds = accountList.Select( f => f.Id ).ToList();

            var qryHasChildren = Get()
                .Where( f => 
                    f.ParentAccountId.HasValue &&
                    resultIds.Contains( f.ParentAccountId.Value ) )
                .Select( f => f.ParentAccountId.Value )
                .Distinct()
                .ToList();

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

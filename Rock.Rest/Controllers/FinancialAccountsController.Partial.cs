// <copyright>
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
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Rest.Controllers
{
    /// <summary>
    ///
    /// </summary>
    [RockGuid( "8a8ff948-147e-4783-8c08-202f6e402caa" )]
    public partial class FinancialAccountsController
    {
        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="activeOnly">if set to <c>true</c> [active only].</param>
        /// <param name="lazyLoad">if set to <c>true</c> [lazy load]</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/FinancialAccounts/GetChildren/{id}/{activeOnly}" )]
        [RockGuid( "5c21d8b8-5c68-42ca-bf19-80050c8ff2a4" )]
        public IQueryable<TreeViewItem> GetChildren( int id, bool activeOnly, bool lazyLoad = false )
        {
            return GetChildren( id, activeOnly, true );
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="activeOnly">if set to <c>true</c> [active only].</param>
        /// <param name="displayPublicName">if set to <c>true</c> [public name].</param>
        /// <param name="lazyLoad">if set to <c>true</c> [lazy load].</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/FinancialAccounts/GetChildren/{id}/{activeOnly}/{displayPublicName}" )]
        [RockGuid( "976bdf2a-92e6-4902-a84d-be7cb25a3824" )]
        public IQueryable<TreeViewItem> GetChildren( int id, bool activeOnly, bool displayPublicName, bool lazyLoad = false )
        {
            List<TreeViewItem> accountItemList = GetChildrenRecursive( this.Service.Context as RockContext, id, activeOnly, displayPublicName, lazyLoad );
            return accountItemList.AsQueryable();
        }

        /// <summary>
        /// Gets the children recursive.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="activeOnly">if set to <c>true</c> [active only].</param>
        /// <param name="displayPublicName">if set to <c>true</c> [display public name].</param>
        /// <param name="lazyLoad">if set to <c>true</c> [lazy load].</param>
        /// <returns></returns>
        private static List<TreeViewItem> GetChildrenRecursive( RockContext rockContext, int id, bool activeOnly, bool displayPublicName, bool lazyLoad )
        {
            IEnumerable<FinancialAccountCache> childAccounts;

            if ( id == 0 )
            {
                childAccounts = new FinancialAccountService( rockContext as RockContext ).Queryable().Where( a => a.ParentAccountId == null )
                    .Select( a => a.Id )
                    .ToList()
                    .Select( a => FinancialAccountCache.Get( a ) )
                    .ToList();
            }
            else
            {
                childAccounts = FinancialAccountCache.Get( id )?.ChildAccounts ?? new FinancialAccountCache[0];
            }

            if ( activeOnly )
            {
                childAccounts = childAccounts
                    .Where( f => f.IsActive == true );
            }

            var accountList = childAccounts
                .OrderBy( f => f.Order )
                .ThenBy( f => f.Name )
                .ToList();

            var accountItemList = accountList.Select( a => new TreeViewItem
            {
                Id = a.Id.ToString(),
                Name = HttpUtility.HtmlEncode( displayPublicName ? a.PublicName : a.Name ),
                HasChildren = a.ChildAccounts.Any()
            } ).ToList();

            var resultIds = accountList.Select( f => f.Id ).ToList();

            foreach ( var accountItem in accountItemList )
            {
                int accountId = int.Parse( accountItem.Id );

                
                if ( lazyLoad == false )
                {
                    
                    accountItem.Children = GetChildrenRecursive( rockContext, accountId, activeOnly, displayPublicName, lazyLoad ).ToList();
                }

                accountItem.IconCssClass = "fa fa-file-o";
            }

            return accountItemList;
        }

    }
}

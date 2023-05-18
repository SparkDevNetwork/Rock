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
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.UI.Controls;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Rock.Rest.Controllers
{
    /// <summary>
    ///
    /// </summary>
    public partial class FinancialAccountsController
    {
        /// <summary>
        /// Gets the children. Please consider this endpoint obsolete as of v 1.14.1, use ~api/FinancialAccounts/GetChildrenBySearchTerm instead
        /// </summary>
        /// <param name="activeOnly">if set to <c>true</c> [active only].</param>
        /// <param name="displayPublicName">if set to <c>true</c> [display public name].</param>
        /// <param name="searchTerm">The searchTerm.</param>
        /// <returns>IQueryable&lt;AccountTreeViewItem&gt;.</returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/FinancialAccounts/GetChildrenBySearchTerm/{activeOnly}/{displayPublicName}/{searchTerm}" )]
        [Rock.SystemGuid.RestActionGuid( "C8715F2B-BAE6-4F92-A651-AD8CC7B104F1" )]
        [RockObsolete( "1.14.1" )]
        [Obsolete( "Use api/FinancialAccounts/GetChildrenBySearchTerm instead" )]
        public IQueryable<AccountTreeViewItem> GetChildrenBySearchTermObsolete( bool activeOnly, bool displayPublicName, string searchTerm )
        {
            return GetSearchTermData( activeOnly, displayPublicName, searchTerm );
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="activeOnly">if set to <c>true</c> [active only].</param>
        /// <param name="displayPublicName">if set to <c>true</c> [display public name].</param>
        /// <param name="searchTerm">The searchTerm.</param>
        /// <returns>IQueryable&lt;AccountTreeViewItem&gt;.</returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/FinancialAccounts/GetChildrenBySearchTerm" )]
        [Rock.SystemGuid.RestActionGuid( "21BF6409-CC65-4562-BD1A-F9FEEC1634F3" )]
        public IQueryable<AccountTreeViewItem> GetChildrenBySearchTerm( bool activeOnly, bool displayPublicName, string searchTerm )
        {
            return GetSearchTermData( activeOnly, displayPublicName, searchTerm );
        }

        /// <summary>
        /// Gets the parent ids.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>IOrderedEnumerable&lt;System.Int32&gt;.</returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/FinancialAccounts/GetParentIds/{id}" )]
        [Rock.SystemGuid.RestActionGuid( "1E007DCA-3785-4EFF-A09D-8F9A034450AA" )]
        public IEnumerable<int> GetParentIds( int id )
        {
            var accountService = new FinancialAccountService( new Data.RockContext() );
            return accountService.GetAllAncestorIds( id )?.Reverse();
        }

        /// <summary>
        /// Gets the parent ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <returns>Dictionary&lt;System.String, System.String&gt;.</returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/FinancialAccounts/GetParentIdsCollection" )]
        [Rock.SystemGuid.RestActionGuid( "1858BC66-D4BA-48CE-9866-154B902AE7A4" )]
        public Dictionary<string, List<string>> GetParentIds( [FromUri] IEnumerable<string> ids )
        {
            var accountService = new FinancialAccountService( new Data.RockContext() );
            var retVal = new Dictionary<string, List<string>>();

            foreach ( var id in ids )
            {
                var idString = id.ToString();
                var ancestors = accountService.GetAllAncestorIds( id.AsInteger() )?
                    .Reverse()?
                    .Select( v => v.ToString() );

                if ( !retVal.ContainsKey( idString ) )
                {
                    retVal.Add( idString, new List<string>( ancestors ) );
                }
            }

            return retVal;
        }

        /// <summary>
        /// Gets the children. Please consider this endpoint obsolete as of v 1.14.1, use ~api/FinancialAccounts/GetChildren/{id} instead.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="activeOnly">if set to <c>true</c> [active only].</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/FinancialAccounts/GetChildren/{id}/{activeOnly}" )]
        [Rock.SystemGuid.RestActionGuid( "B1C3FF53-CE30-4006-8ABC-5A513933CAF0" )]
        [RockObsolete( "1.14.1" )]
        [Obsolete( "Use api/FinancialAccounts/GetChildren/{id} instead" )]
        public IQueryable<AccountTreeViewItem> GetChildrenObsolete( int id, bool activeOnly )
        {
            return GetChildrenData( id, activeOnly, true );
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="activeOnly">if set to <c>true</c> [active only].</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/FinancialAccounts/GetChildren/{id}" )]
        [Rock.SystemGuid.RestActionGuid( "5C21D8B8-5C68-42CA-BF19-80050C8FF2A4" )]
        public IQueryable<AccountTreeViewItem> GetChildren( int id, bool activeOnly )
        {
            return GetChildrenData( id, activeOnly, true );
        }

        /// <summary>
        /// Gets the inactive. Please consider this endpoint obsolete as of v 1.14.1, use ~api/FinancialAccounts/GetInactive instead.
        /// </summary>
        /// <param name="displayPublicName">if set to <c>true</c> [display public name].</param>
        /// <returns>IQueryable&lt;TreeViewItem&gt;.</returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/FinancialAccounts/GetInactive/{displayPublicName}" )]
        [Rock.SystemGuid.RestActionGuid( "06082C90-48C4-4D29-9E31-1A9BED21859E" )]
        [RockObsolete( "1.14.1" )]
        [Obsolete( "Use api/FinancialAccounts/GetInactive instead" )]
        public IQueryable<AccountTreeViewItem> GetInactiveObsolete( bool displayPublicName )
        {
            return GetInactiveData( displayPublicName );
        }

        /// <summary>
        /// Gets the inactive.
        /// </summary>
        /// <param name="displayPublicName">if set to <c>true</c> [display public name].</param>
        /// <returns>IQueryable&lt;TreeViewItem&gt;.</returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/FinancialAccounts/GetInactive" )]
        [Rock.SystemGuid.RestActionGuid( "4B08E38F-0C6A-41B1-9C52-DEB40028927F" )]
        public IQueryable<AccountTreeViewItem> GetInactive( bool displayPublicName )
        {
            return GetInactiveData( displayPublicName );
        }

        /// <summary>
        /// Gets the children.  Please consider this endpoint obsolete as of v 1.14.1, use ~api/FinancialAccounts/GetChildren/{id} instead.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="activeOnly">if set to <c>true</c> [active only].</param>
        /// <param name="displayPublicName">if set to <c>true</c> [public name].</param>
        /// <param name="countsType"></param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/FinancialAccounts/GetChildren/{id}/{activeOnly}/{displayPublicName}" )]
        [Rock.SystemGuid.RestActionGuid( "5289EB7D-5A89-4E99-BEF6-44C81EBB2BCB" )]
        [RockObsolete( "1.14.1" )]
        [Obsolete( "Use api/FinancialAccounts/GetChildren/{id} instead" )]
        public IQueryable<AccountTreeViewItem> GetChildrenObsolete( int id, bool activeOnly, bool displayPublicName, AccountTreeViewItem.GetCountsType countsType = AccountTreeViewItem.GetCountsType.None )
        {
            return GetChildrenData( id, activeOnly, displayPublicName, countsType );
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="activeOnly">if set to <c>true</c> [active only].</param>
        /// <param name="displayPublicName">if set to <c>true</c> [public name].</param>
        /// <param name="countsType">The counts type, if set to <see cref="AccountTreeViewItem.GetCountsType.ChildGroups"/>the count of all child accounts of a parent account is added.</param>
        /// <param name="loadChildren">if set to true all the child accounts of the financial accounts are loaded.
        /// It is not advisable to set this to true when there more than several hunderd accounts as it can result in a performance hit.
        /// </param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/FinancialAccounts/GetChildren/{id}" )]
        [Rock.SystemGuid.RestActionGuid( "976BDF2A-92E6-4902-A84D-BE7CB25A3824" )]
        public IQueryable<AccountTreeViewItem> GetChildren( int id, bool activeOnly, bool displayPublicName, AccountTreeViewItem.GetCountsType countsType = AccountTreeViewItem.GetCountsType.None, bool loadChildren = false )
        {
            return GetChildrenData( id, activeOnly, displayPublicName, countsType, loadChildren );
        }

        #region Methods
        private IQueryable<AccountTreeViewItem> GetChildrenData( int id, bool activeOnly, bool displayPublicName, AccountTreeViewItem.GetCountsType countsType = AccountTreeViewItem.GetCountsType.None, bool loadChildren = false )
        {
            var financialAccountService = new FinancialAccountService( new Data.RockContext() );

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
                    .Where( f => f.IsActive == activeOnly );
            }

            var accountList = qry
                .OrderBy( f => f.Order )
                .ThenBy( f => f.Name )
                .ToList();

            var accountTreeViewItems = accountList
                .Select( a => new AccountTreeViewItem
                {
                    Id = a.Id.ToString(),
                    Name = HttpUtility.HtmlEncode( displayPublicName ? a.PublicName : a.Name ),
                    GlCode = a.GlCode,
                    IsActive = a.IsActive,
                    ParentId = a.ParentAccountId.GetValueOrDefault( 0 ).ToString(),
                } ).ToList();

            accountTreeViewItems = financialAccountService.GetTreeviewPaths( accountTreeViewItems, accountList );

            var resultIds = accountList.Select( f => f.Id ).ToList();

            var childQry = Get()
            .Where( f =>
                f.ParentAccountId.HasValue &&
                resultIds.Contains( f.ParentAccountId.Value ) );

            if ( activeOnly )
            {
                childQry = childQry.Where( f => f.IsActive == activeOnly );
            }

            var childrenList = childQry.Select( f => f.ParentAccountId.Value )
                .ToList();

            var totalCount = financialAccountService.Queryable().Count();

            foreach ( var accountTreeViewItem in accountTreeViewItems )
            {
                int accountId = int.Parse( accountTreeViewItem.Id );
                int childrenCount = ( childrenList?.Count( v => v == accountId ) ).GetValueOrDefault( 0 );

                accountTreeViewItem.HasChildren = childrenCount > 0;

                if ( accountTreeViewItem.HasChildren )
                {
                    if ( countsType == AccountTreeViewItem.GetCountsType.ChildGroups )
                    {
                        accountTreeViewItem.CountInfo = childrenCount;
                    }

                    if ( loadChildren )
                    {
                        accountTreeViewItem.Children = GetChildrenData( accountId, activeOnly, displayPublicName, countsType, loadChildren ).ToList();
                    }

                    accountTreeViewItem.ParentId = id.ToString();
                }

                accountTreeViewItem.IconCssClass = "fa fa-file-o";
                accountTreeViewItem.TotalCount = totalCount;
            }

            return accountTreeViewItems.AsQueryable();
        }

        /// <summary>
        /// Gets the search term data.
        /// </summary>
        /// <param name="activeOnly">if set to <c>true</c> [active only].</param>
        /// <param name="displayPublicName">if set to <c>true</c> [display public name].</param>
        /// <param name="searchTerm">The search term.</param>
        /// <returns>IQueryable&lt;AccountTreeViewItem&gt;.</returns>
        private IQueryable<AccountTreeViewItem> GetSearchTermData( bool activeOnly, bool displayPublicName, string searchTerm )
        {
            IQueryable<FinancialAccount> qry;

            if ( searchTerm.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var financialAccountService = new FinancialAccountService( new Data.RockContext() );
            qry = financialAccountService.GetAccountsBySearchTerm( searchTerm );

            if ( activeOnly )
            {
                qry = qry
                    .Where( f => f.IsActive == activeOnly );
            }

            var accountList = qry
                .OrderBy( f => f.Order )
                .ThenBy( f => f.Name )
                .ToList();

            var accountTreeViewItems = accountList
                .Select( a => new AccountTreeViewItem
                {
                    Id = a.Id.ToString(),
                    Name = HttpUtility.HtmlEncode( displayPublicName ? a.PublicName : a.Name ),
                    GlCode = a.GlCode,
                    IsActive = a.IsActive,
                    ParentId = a.ParentAccountId.GetValueOrDefault( 0 ).ToString(),
                } ).ToList();

            accountTreeViewItems = financialAccountService.GetTreeviewPaths( accountTreeViewItems, accountList );

            var resultIds = accountList.Select( f => f.Id ).ToList();

            var childrenList = Get()
                .Where( f =>
                    f.ParentAccountId.HasValue &&
                    resultIds.Contains( f.ParentAccountId.Value ) )
                .Select( f => f.ParentAccountId.Value )
                .ToList();

            foreach ( var accountTreeViewItem in accountTreeViewItems )
            {
                int accountId = int.Parse( accountTreeViewItem.Id );
                int childrenCount = ( childrenList?.Count( v => v == accountId ) ).GetValueOrDefault( 0 );

                accountTreeViewItem.HasChildren = childrenCount > 0;
                var lastChildId = ( childrenList?.LastOrDefault() ).GetValueOrDefault( 0 );

                if ( accountTreeViewItem.HasChildren )
                {
                    accountTreeViewItem.CountInfo = childrenCount;
                }

                accountTreeViewItem.IconCssClass = "fa fa-file-o";
            }

            return accountTreeViewItems.AsQueryable();
        }

        private IQueryable<AccountTreeViewItem> GetInactiveData( bool displayPublicName )
        {
            var financialAccountService = new FinancialAccountService( new Data.RockContext() );

            IQueryable<FinancialAccount> qry;

            qry = Get().Where( f =>
                f.ParentAccountId.HasValue == false );

            qry = qry
                .Where( f => f.IsActive == false );

            var accountList = qry
                .OrderBy( f => f.Order )
                .ThenBy( f => f.Name )
                .ToList();

            var accountTreeViewItems = accountList.Select( a => new AccountTreeViewItem
            {
                Id = a.Id.ToString(),
                Name = HttpUtility.HtmlEncode( displayPublicName ? a.PublicName : a.Name ),
                GlCode = a.GlCode,
                IsActive = a.IsActive,
                ParentId = a.ParentAccountId.GetValueOrDefault( 0 ).ToString(),
            } ).ToList();

            accountTreeViewItems = financialAccountService.GetTreeviewPaths( accountTreeViewItems, accountList );

            var resultIds = accountList.Select( f => f.Id ).ToList();

            var childrenList = Get()
                .Where( f =>
                    f.ParentAccountId.HasValue &&
                    resultIds.Contains( f.ParentAccountId.Value ) )
                .Select( f => f.ParentAccountId.Value )
                .ToList();

            foreach ( var accountTreeViewItem in accountTreeViewItems )
            {
                int accountId = int.Parse( accountTreeViewItem.Id );
                int childrenCount = ( childrenList?.Count( v => v == accountId ) ).GetValueOrDefault( 0 );

                accountTreeViewItem.HasChildren = childrenCount > 0;
                var lastChildId = ( childrenList?.LastOrDefault() ).GetValueOrDefault( 0 );

                if ( accountTreeViewItem.HasChildren )
                {
                    accountTreeViewItem.CountInfo = childrenCount;
                    accountTreeViewItem.ParentId = accountId.ToString();

                }

                accountTreeViewItem.IconCssClass = "fa fa-file-o";
            }

            return accountTreeViewItems.AsQueryable();
        }
        #endregion Methods
    }
}
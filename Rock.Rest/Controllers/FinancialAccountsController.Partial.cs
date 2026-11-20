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
        public IEnumerable<int> GetParentIds( int id ) => GetVisibleParentIds( id, GetAllParentIds( id ) );

        /// <summary>
        /// Gets the identifiers of all the ancestors of the account, root first.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The ancestor identifiers, root first.</returns>
        private static IEnumerable<int> GetAllParentIds( int id )
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
            var visibleAccounts = GetVisibleAccounts( GetPerson() );

            foreach ( var id in ids )
            {
                var idString = id.ToString();
                var accountId = id.AsInteger();

                // Every requested id stays a key, because the account picker rebuilds
                // the selected ids from the keys. A hidden account has no ancestors.
                var ancestors = visibleAccounts.ContainsKey( accountId )
                    ? GetVisibleAncestorIds( accountService, accountId, visibleAccounts )
                        .Reverse()
                        .Select( v => v.ToString() )
                    : Enumerable.Empty<string>();

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
        private IQueryable<AccountTreeViewItem> GetChildrenData( int id, bool activeOnly, bool displayPublicName, AccountTreeViewItem.GetCountsType countsType = AccountTreeViewItem.GetCountsType.None, bool loadChildren = false, Dictionary<int, bool> visibleAccounts = null )
        {
            var financialAccountService = new FinancialAccountService( new Data.RockContext() );

            // Built once per request and passed down when loading children, see GetVisibleAccounts.
            visibleAccounts = visibleAccounts ?? GetVisibleAccounts( GetPerson() );

            // The children of an account the person cannot see are not listed under it.
            if ( id != 0 && !visibleAccounts.ContainsKey( id ) )
            {
                return new List<AccountTreeViewItem>().AsQueryable();
            }

            var accountList = OrderAccounts( GetAccounts( visibleAccounts.Keys, visibleAccounts )
                .Where( a => GetVisibleParentId( a, visibleAccounts ) == id && ( !activeOnly || a.IsActive ) ), visibleAccounts );

            var accountTreeViewItems = accountList
                .Select( a => CreateAccountTreeViewItem( a, displayPublicName, financialAccountService, visibleAccounts ) )
                .ToList();

            var resultIds = accountList.Select( f => f.Id ).ToList();

            var childrenList = GetVisibleChildParentIds( resultIds, activeOnly, visibleAccounts );

            var totalCount = visibleAccounts.Count;

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
                        accountTreeViewItem.Children = GetChildrenData( accountId, activeOnly, displayPublicName, countsType, loadChildren, visibleAccounts ).ToList();
                    }

                    accountTreeViewItem.ParentId = id.ToString();
                }

                accountTreeViewItem.IconCssClass = "ti ti-file";
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

            var visibleAccounts = GetVisibleAccounts( GetPerson() );

            // An account in its public form only matches on the text that is shown,
            // so the search cannot be used to find its internal name or GL code.
            var accountList = OrderAccounts( GetAccounts( qry.Select( f => f.Id ).ToList(), visibleAccounts )
                .Where( a => visibleAccounts[a.Id] || GetPublicText( a ).IndexOf( searchTerm, StringComparison.OrdinalIgnoreCase ) >= 0 ), visibleAccounts );

            var accountTreeViewItems = accountList
                .Select( a => CreateAccountTreeViewItem( a, displayPublicName, financialAccountService, visibleAccounts ) )
                .ToList();

            var resultIds = accountList.Select( f => f.Id ).ToList();

            var childrenList = GetVisibleChildParentIds( resultIds, false, visibleAccounts );

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

                accountTreeViewItem.IconCssClass = "ti ti-file";
            }

            return accountTreeViewItems.AsQueryable();
        }

        private IQueryable<AccountTreeViewItem> GetInactiveData( bool displayPublicName )
        {
            var financialAccountService = new FinancialAccountService( new Data.RockContext() );

            var visibleAccounts = GetVisibleAccounts( GetPerson() );

            // The public form only covers active accounts, so only accounts the person
            // can view are listed here.
            var accountList = OrderAccounts( GetAccounts( visibleAccounts.Keys, visibleAccounts )
                .Where( a => visibleAccounts[a.Id] && !a.IsActive && GetVisibleParentId( a, visibleAccounts ) == 0 ), visibleAccounts );

            var accountTreeViewItems = accountList
                .Select( a => CreateAccountTreeViewItem( a, displayPublicName, financialAccountService, visibleAccounts ) )
                .ToList();

            var resultIds = accountList.Select( f => f.Id ).ToList();

            var childrenList = GetVisibleChildParentIds( resultIds, false, visibleAccounts );

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

                accountTreeViewItem.IconCssClass = "ti ti-file";
            }

            return accountTreeViewItems.AsQueryable();
        }

        /// <summary>
        /// Gets the accounts the person may see in the account picker. The key is the
        /// account identifier and the value is <c>true</c> when the person can view
        /// the account, or <c>false</c> when only its public form may be shown.
        /// </summary>
        /// <param name="person">The person making the request.</param>
        /// <returns>The accounts the person may see.</returns>
        private static Dictionary<int, bool> GetVisibleAccounts( Person person )
        {
            /*
                11/20/2026 - MSE

                The REST action grant only says the person may use these picker endpoints. Each
                financial account has its own VIEW rule, which stock security only grants to the
                finance roles. A person who can VIEW an account sees it in full, as before.

                Staff outside the finance roles still pick accounts on forms such as registration
                fees, so an active public account (one a giver can already see on a giving page)
                is also shown to them in its public form: the public name (or the name when it
                has none), no GL code, and no name or position of an ancestor they cannot see.
                An account whose parent is hidden is shown at the top level so it can still be
                picked. Accounts in their public form are ordered and searched by the text shown,
                so neither one can be used to discover internal names or GL codes. This matches
                the public view of the v2 account picker (ControlsController).

                Reason: Anyone with the REST action could read every account name, GL code and hierarchy.
            */
            var visibleAccounts = new Dictionary<int, bool>();

            foreach ( var account in Rock.Web.Cache.FinancialAccountCache.All() )
            {
                var access = Rock.Web.UI.Controls.AccountPicker.GetPickerAccess( account, person );
                if ( access.HasValue )
                {
                    visibleAccounts.Add( account.Id, access.Value );
                }
            }

            return visibleAccounts;
        }

        /// <summary>
        /// Gets the cached accounts for the identifiers that the person may see.
        /// </summary>
        /// <param name="accountIds">The account identifiers.</param>
        /// <param name="visibleAccounts">The accounts the person may see, from <see cref="GetVisibleAccounts(Person)"/>.</param>
        /// <returns>The accounts.</returns>
        private static IEnumerable<Rock.Web.Cache.FinancialAccountCache> GetAccounts( IEnumerable<int> accountIds, Dictionary<int, bool> visibleAccounts )
        {
            return accountIds
                .Where( a => visibleAccounts.ContainsKey( a ) )
                .Select( a => Rock.Web.Cache.FinancialAccountCache.Get( a ) )
                .Where( a => a != null );
        }

        /// <summary>
        /// Orders the accounts by their order, then by the name for accounts the person can
        /// view and by the text shown for accounts in their public form.
        /// </summary>
        /// <param name="accounts">The accounts.</param>
        /// <param name="visibleAccounts">The accounts the person may see, from <see cref="GetVisibleAccounts(Person)"/>.</param>
        /// <returns>The ordered accounts.</returns>
        private static List<Rock.Web.Cache.FinancialAccountCache> OrderAccounts( IEnumerable<Rock.Web.Cache.FinancialAccountCache> accounts, Dictionary<int, bool> visibleAccounts )
        {
            return accounts
                .OrderBy( a => a.Order )
                .ThenBy( a => visibleAccounts[a.Id] ? a.Name : GetPublicText( a ) )
                .ToList();
        }

        /// <summary>
        /// Gets the text shown for an account in its public form: the public name, or the
        /// name when it has no public name.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <returns>The text to show.</returns>
        private static string GetPublicText( Rock.Web.Cache.FinancialAccountCache account )
        {
            return account.PublicName.IsNotNullOrWhiteSpace() ? account.PublicName : account.Name;
        }

        /// <summary>
        /// Gets the parent account identifier to show for an account, which is 0 (the top
        /// level) when the person may not see the parent account.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="visibleAccounts">The accounts the person may see, from <see cref="GetVisibleAccounts(Person)"/>.</param>
        /// <returns>The parent account identifier, or 0.</returns>
        private static int GetVisibleParentId( Rock.Web.Cache.FinancialAccountCache account, Dictionary<int, bool> visibleAccounts )
        {
            return account.ParentAccountId.HasValue && visibleAccounts.ContainsKey( account.ParentAccountId.Value )
                ? account.ParentAccountId.Value
                : 0;
        }

        /// <summary>
        /// Gets the ancestor account identifiers the person may see, root first. A hidden
        /// account has none, and the list stops at the first ancestor the person may not see.
        /// </summary>
        /// <param name="id">The account identifier.</param>
        /// <param name="allParentIds">The identifiers of all the ancestors of the account, root first.</param>
        /// <returns>The ancestor account identifiers, root first.</returns>
        private IEnumerable<int> GetVisibleParentIds( int id, IEnumerable<int> allParentIds )
        {
            var visibleAccounts = GetVisibleAccounts( GetPerson() );

            if ( !visibleAccounts.ContainsKey( id ) )
            {
                return new List<int>();
            }

            return allParentIds
                .Reverse()
                .TakeWhile( a => visibleAccounts.ContainsKey( a ) )
                .Reverse()
                .ToList();
        }

        /// <summary>
        /// Gets the ancestor account identifiers the person may see, nearest first. The list
        /// stops at the first ancestor the person may not see.
        /// </summary>
        /// <param name="financialAccountService">The financial account service.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="visibleAccounts">The accounts the person may see, from <see cref="GetVisibleAccounts(Person)"/>.</param>
        /// <returns>The ancestor account identifiers, nearest first.</returns>
        private static IEnumerable<int> GetVisibleAncestorIds( FinancialAccountService financialAccountService, int accountId, Dictionary<int, bool> visibleAccounts )
        {
            return financialAccountService.GetAllAncestorIds( accountId )
                .TakeWhile( a => visibleAccounts.ContainsKey( a ) )
                .ToList();
        }

        /// <summary>
        /// Gets the visible parent account identifier of each visible child of the given
        /// accounts, once per child, so the children of each account can be counted.
        /// </summary>
        /// <param name="parentAccountIds">The parent account identifiers.</param>
        /// <param name="activeOnly">if set to <c>true</c> only active children are counted.</param>
        /// <param name="visibleAccounts">The accounts the person may see, from <see cref="GetVisibleAccounts(Person)"/>.</param>
        /// <returns>The parent account identifier of each child.</returns>
        private static List<int> GetVisibleChildParentIds( List<int> parentAccountIds, bool activeOnly, Dictionary<int, bool> visibleAccounts )
        {
            var parentAccountIdSet = new HashSet<int>( parentAccountIds );

            return GetAccounts( visibleAccounts.Keys, visibleAccounts )
                .Where( a => !activeOnly || a.IsActive )
                .Select( a => GetVisibleParentId( a, visibleAccounts ) )
                .Where( a => parentAccountIdSet.Contains( a ) )
                .ToList();
        }

        /// <summary>
        /// Creates the tree view item for an account, in full when the person can view the
        /// account, otherwise in its public form.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="displayPublicName">if set to <c>true</c> the public name is shown for accounts the person can view.</param>
        /// <param name="financialAccountService">The financial account service.</param>
        /// <param name="visibleAccounts">The accounts the person may see, from <see cref="GetVisibleAccounts(Person)"/>.</param>
        /// <returns>The tree view item.</returns>
        private static AccountTreeViewItem CreateAccountTreeViewItem( Rock.Web.Cache.FinancialAccountCache account, bool displayPublicName, FinancialAccountService financialAccountService, Dictionary<int, bool> visibleAccounts )
        {
            var canView = visibleAccounts[account.Id];

            // The path lists the ancestor names the same way FinancialAccountService.GetTreeviewPaths
            // does, leaving out the ancestors the person may not see.
            var path = GetVisibleAncestorIds( financialAccountService, account.Id, visibleAccounts )
                .OrderBy( a => a )
                .Select( a => System.Net.WebUtility.HtmlEncode( GetPublicText( Rock.Web.Cache.FinancialAccountCache.Get( a ) ) ) )
                .JoinStrings( "^" );

            return new AccountTreeViewItem
            {
                Id = account.Id.ToString(),
                Name = HttpUtility.HtmlEncode( canView ? ( displayPublicName ? account.PublicName : account.Name ) : GetPublicText( account ) ),
                GlCode = canView ? account.GlCode : null,
                IsActive = account.IsActive,
                ParentId = GetVisibleParentId( account, visibleAccounts ).ToString(),
                Path = path
            };
        }
        #endregion Methods
    }
}
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.AccountTreeView;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Displays a navigation tree of financial accounts. Selecting a node navigates to the configured
    /// Detail Page (or reloads the current page) with the selection and expanded nodes on the query
    /// string so sibling blocks read them as page parameters.
    /// </summary>
    [DisplayName( "Account Tree View" )]
    [Category( "Finance" )]
    [Description( "Creates a navigation tree for accounts." )]
    [IconCssClass( "ti ti-list-tree" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [TextField(
        "Treeview Title",
        Key = AttributeKey.TreeviewTitle,
        Description = "Account Tree View",
        IsRequired = false,
        Order = 1 )]

    [BooleanField(
        "Show Settings Panel",
        Key = AttributeKey.ShowSettingsPanel,
        DefaultBooleanValue = true,
        Order = 2 )]

    [CustomDropdownListField(
        "Initial Active Setting",
        Key = AttributeKey.InitialActiveSetting,
        Description = "Select whether to initially show all or just active accounts in the treeview.",
        ListSource = "0^All,1^Active",
        IsRequired = false,
        DefaultValue = AttributeDefault.InitialActiveSettingActive,
        Order = 3 )]

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Order = 4 )]

    [LinkedPage(
        "Order Top-Level Page",
        Key = AttributeKey.OrderTopLevelPage,
        Order = 5 )]

    [BooleanField(
        "Use Public Name",
        Key = AttributeKey.UsePublicName,
        Description = "Determines if the public name should be displayed for accounts.",
        DefaultBooleanValue = false,
        Order = 6 )]

    #endregion Block Attributes

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Navigation )]
    [Rock.SystemGuid.EntityTypeGuid( "4348EA2F-E054-45FA-B88B-89F3B6D4F27A" )]
    // WAS [Rock.SystemGuid.BlockTypeGuid( "04875612-354D-4D9E-89CD-6C72B9884E09" )]
    [Rock.SystemGuid.BlockTypeGuid( "EC6ECB2B-665F-43FC-9FF4-A6B1CD5F2AE6" )]
    public class AccountTreeView : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string TreeviewTitle = "TreeviewTitle";
            public const string ShowSettingsPanel = "ShowFilterOption";
            public const string InitialActiveSetting = "InitialActiveSetting";
            public const string DetailPage = "DetailPage";
            public const string OrderTopLevelPage = "OrderTopLevelPage";
            public const string UsePublicName = "UsePublicName";
        }

        /// <summary>
        /// Default and list values for block attributes stored as opaque strings (custom dropdowns).
        /// </summary>
        private static class AttributeDefault
        {
            /// <summary>
            /// <see cref="AttributeKey.InitialActiveSetting"/>: show active accounts only (ListSource 1^Active).
            /// </summary>
            public const string InitialActiveSettingActive = "1";
        }

        private static class PageParameterKey
        {
            public const string AccountId = "AccountId";
            public const string ParentAccountId = "ParentAccountId";
            public const string ExpandedIds = "ExpandedIds";
            public const string TopLevel = "TopLevel";
        }

        private static class NavigationUrlKey
        {
            public const string OrderTopLevelPage = "OrderTopLevelPage";
        }

        private static class PersonPreferenceKey
        {
            public const string HideInactiveAccounts = "hide-inactive-accounts";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<AccountTreeViewBag, AccountTreeViewOptionsBag>
            {
                Options = GetBoxOptions(),
                NavigationUrls = GetBoxNavigationUrls()
            };

            box.Bag = GetBag();

            return box;
        }

        /// <summary>
        /// Builds the block's configured settings for the client.
        /// </summary>
        /// <returns>The options bag describing how the tree should be displayed and scoped.</returns>
        private AccountTreeViewOptionsBag GetBoxOptions()
        {
            return new AccountTreeViewOptionsBag
            {
                BlockProperties = new AccountTreeViewBlockAttributesBag
                {
                    PanelTitle = GetAttributeValue( AttributeKey.TreeviewTitle ),
                    ShowSettingsPanel = GetAttributeValue( AttributeKey.ShowSettingsPanel ).AsBooleanOrNull() ?? false,
                    UsePublicName = GetAttributeValue( AttributeKey.UsePublicName ).AsBoolean()
                }
            };
        }

        /// <summary>
        /// Builds the navigation URLs for the configured linked pages.
        /// </summary>
        /// <returns>A map of navigation key to URL; an entry is blank when its page is not configured.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.OrderTopLevelPage] = this.GetLinkedPageUrl( AttributeKey.OrderTopLevelPage, PageParameterKey.TopLevel, "True" )
            };
        }

        /// <summary>
        /// Builds the runtime data for the client: selection, expansion, auth, and active-filter state.
        /// </summary>
        /// <returns>The populated runtime bag.</returns>
        private AccountTreeViewBag GetBag()
        {
            var showSettingsPanel = GetAttributeValue( AttributeKey.ShowSettingsPanel ).AsBooleanOrNull() ?? false;

            var preferences = GetBlockTypePersonPreferences();

            // Go with person preference for showing inactive, if no preference, go with block attribute
            var hideInactiveAccounts = preferences.GetValue( PersonPreferenceKey.HideInactiveAccounts ).AsBooleanOrNull();
            if ( !hideInactiveAccounts.HasValue || !showSettingsPanel )
            {
                hideInactiveAccounts = GetAttributeValue( AttributeKey.InitialActiveSetting ) == AttributeDefault.InitialActiveSettingActive;
            }

            var canEditBlock = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            var bag = new AccountTreeViewBag
            {
                SelectedAccountGuids = new List<Guid>(),
                ExpandedAccountGuids = new List<Guid>(),
                HideInactiveAccounts = hideInactiveAccounts ?? true,
                IsAddRootEnabled = canEditBlock,
                IsAddAccountVisible = canEditBlock,
                IsAddChildEnabled = false
            };

            var allowIntegerId = !PageCache.Layout.Site.DisablePredictableIds;
            var accountKey = RequestContext.GetPageParameter( PageParameterKey.AccountId );

            if ( accountKey.IsNullOrWhiteSpace() )
            {
                // No account was requested, so auto-select and redirect to the first authorized account.
                var firstAccount = FindFirstAccount( bag.HideInactiveAccounts );
                if ( firstAccount != null )
                {
                    // Auto-select stays on the current page; only a deliberate selection navigates to a Detail Page.
                    var autoSelectUrl = GetNavigationUrl( firstAccount.Guid, Guid.Empty, bag.ExpandedAccountGuids, out _, forceCurrentPage: true );
                    if ( autoSelectUrl.IsNotNullOrWhiteSpace() )
                    {
                        RequestContext.Response.RedirectToUrl( autoSelectUrl );
                    }
                }

                return bag;
            }

            // Resolve the selected account. AccountId=0 is add mode and intentionally resolves to no selection.
            if ( accountKey != "0" )
            {
                var selectedAccount = FinancialAccountCache.Get( accountKey, allowIntegerId );

                if ( selectedAccount != null )
                {
                    bag.SelectedAccountGuids.Add( selectedAccount.Guid );

                    // Expand the selection's ancestors so a deep-link opens the tree far enough to reveal it.
                    foreach ( var ancestor in selectedAccount.GetAncestorFinancialAccounts() )
                    {
                        if ( !bag.ExpandedAccountGuids.Contains( ancestor.Guid ) )
                        {
                            bag.ExpandedAccountGuids.Add( ancestor.Guid );
                        }
                    }

                    ApplyAddChildAuthorization( bag, selectedAccount, canEditBlock );
                }
            }

            // Honor any explicitly expanded nodes (from a selection or the Add actions) so the tree keeps its open state.
            var expandedValue = RequestContext.GetPageParameter( PageParameterKey.ExpandedIds );
            if ( expandedValue.IsNotNullOrWhiteSpace() )
            {
                foreach ( var key in expandedValue.SplitDelimitedValues() )
                {
                    var expandedAccount = FinancialAccountCache.Get( key, allowIntegerId );
                    if ( expandedAccount != null && !bag.ExpandedAccountGuids.Contains( expandedAccount.Guid ) )
                    {
                        bag.ExpandedAccountGuids.Add( expandedAccount.Guid );
                    }
                }
            }

            return bag;
        }

        /// <summary>
        /// Applies the add-child visibility rules based on the selected account's authorization.
        /// </summary>
        /// <param name="bag">The bag to update.</param>
        /// <param name="selectedAccount">The currently selected account.</param>
        /// <param name="canEditBlock">Whether the person has EDIT on the block.</param>
        private void ApplyAddChildAuthorization( AccountTreeViewBag bag, FinancialAccountCache selectedAccount, bool canEditBlock )
        {
            var canAddChild = canEditBlock;

            if ( !canAddChild )
            {
                canAddChild = selectedAccount.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
            }

            if ( !canAddChild )
            {
                // Adding a child is allowed when the person can edit any of the selected account's children.
                foreach ( var childAccount in selectedAccount.ChildAccounts )
                {
                    if ( childAccount.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                    {
                        canAddChild = true;
                        break;
                    }
                }
            }

            bag.IsAddChildEnabled = canAddChild;
            bag.IsAddAccountVisible = canEditBlock || canAddChild;
        }

        /// <summary>
        /// Finds the first top-level account the current person is authorized to view, ordered for display.
        /// </summary>
        /// <param name="hideInactiveAccounts">Whether inactive accounts are excluded.</param>
        /// <returns>The first authorized top-level account, or null when none is found.</returns>
        private FinancialAccountCache FindFirstAccount( bool hideInactiveAccounts )
        {
            var rootAccounts = FinancialAccountCache.All()
                .Where( a => !a.ParentAccountId.HasValue && ( !hideInactiveAccounts || a.IsActive ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name );

            foreach ( var account in rootAccounts )
            {
                if ( account.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                {
                    return account;
                }
            }

            return null;
        }

        /// <summary>
        /// Builds the navigate-mode URL for a selected (or to-be-added) account.
        /// </summary>
        /// <param name="accountGuid">The selected account, or an empty Guid when adding.</param>
        /// <param name="parentGuid">The parent account, used when adding a child.</param>
        /// <param name="expandedGuids">The accounts currently expanded in the tree.</param>
        /// <param name="errorMessage">When this returns, holds the reason the URL could not be built.</param>
        /// <param name="forceCurrentPage">When true, always builds the current-page URL and ignores the Detail Page.</param>
        /// <returns>The Detail Page URL (or current-page URL) with the page parameters applied.</returns>
        private string GetNavigationUrl( Guid accountGuid, Guid parentGuid, List<Guid> expandedGuids, out string errorMessage, bool forceCurrentPage = false )
        {
            errorMessage = string.Empty;
            expandedGuids = expandedGuids ?? new List<Guid>();

            var qryParams = new Dictionary<string, string>();

            if ( accountGuid == Guid.Empty )
            {
                // An add action targets a new account; the Detail Page treats AccountId=0 as "new".
                qryParams[PageParameterKey.AccountId] = "0";
            }
            else
            {
                var selectedAccount = FinancialAccountCache.Get( accountGuid );
                if ( selectedAccount == null )
                {
                    errorMessage = "The selected account could not be found.";
                    return string.Empty;
                }

                qryParams[PageParameterKey.AccountId] = selectedAccount.IdKey;
            }

            var parentAccount = parentGuid != Guid.Empty ? FinancialAccountCache.Get( parentGuid ) : null;
            if ( parentAccount != null )
            {
                qryParams[PageParameterKey.ParentAccountId] = parentAccount.IdKey;
            }
            else if ( accountGuid == Guid.Empty )
            {
                // A top-level add has no parent account.
                qryParams[PageParameterKey.ParentAccountId] = "0";
            }

            var expandedIdKeys = expandedGuids
                .Select( guid => FinancialAccountCache.Get( guid )?.IdKey )
                .Where( idKey => idKey.IsNotNullOrWhiteSpace() )
                .Distinct()
                .ToList();

            if ( expandedIdKeys.Any() )
            {
                qryParams[PageParameterKey.ExpandedIds] = string.Join( ",", expandedIdKeys );
            }

            var detailPageReference = new PageReference( GetAttributeValue( AttributeKey.DetailPage ) );
            if ( forceCurrentPage || detailPageReference.PageId <= 0 || detailPageReference.PageId == PageCache.Id )
            {
                return this.GetCurrentPageUrl( qryParams );
            }

            return this.GetLinkedPageUrl( AttributeKey.DetailPage, qryParams );
        }

        /// <summary>
        /// Recursively builds the included accounts at one level of the pruned search tree.
        /// </summary>
        /// <param name="parentId">The parent account Id whose included children are built, or null for the top level.</param>
        /// <param name="childrenByParent">The included accounts grouped by parent Id, so each level is a direct lookup.</param>
        /// <param name="displayPublicName">Whether the public name is shown instead of the internal name.</param>
        /// <returns>The ordered tree items for this level, with their included descendants nested beneath them.</returns>
        private List<TreeItemBag> BuildSearchTreeItems( int? parentId, ILookup<int?, FinancialAccountCache> childrenByParent, bool displayPublicName )
        {
            var levelAccounts = childrenByParent[parentId]
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name );

            var items = new List<TreeItemBag>();
            foreach ( var account in levelAccounts )
            {
                var children = BuildSearchTreeItems( account.Id, childrenByParent, displayPublicName );
                var hasChildren = children.Count > 0;

                items.Add( new TreeItemBag
                {
                    Value = account.Guid.ToString(),
                    Text = displayPublicName ? account.PublicName : account.Name,
                    IsActive = account.IsActive,
                    IconCssClass = "ti ti-file",
                    HasChildren = hasChildren,
                    IsFolder = hasChildren,
                    Children = hasChildren ? children : null
                } );
            }

            return items;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Builds the navigate-mode URL for an account selection or add action.
        /// </summary>
        /// <param name="accountGuid">The selected account, or an empty Guid when adding.</param>
        /// <param name="parentGuid">The parent account, used when adding a child.</param>
        /// <param name="expandedGuids">The accounts currently expanded in the tree.</param>
        /// <returns>The navigation URL, or a bad-request result when it could not be built.</returns>
        [BlockAction]
        public BlockActionResult GetNavigationUrl( Guid accountGuid, Guid parentGuid, List<Guid> expandedGuids )
        {
            var url = GetNavigationUrl( accountGuid, parentGuid, expandedGuids, out var errorMessage );

            if ( url.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( errorMessage.IsNotNullOrWhiteSpace() ? errorMessage : "Could not determine the navigation URL for the selected account." );
            }

            return ActionOk( url );
        }

        /// <summary>
        /// Saves the person's hide-inactive-accounts preference so the active/inactive filter persists across visits.
        /// </summary>
        /// <param name="hideInactiveAccounts">Whether inactive accounts should be hidden.</param>
        /// <returns>An OK result.</returns>
        [BlockAction]
        public BlockActionResult SetHideInactiveAccounts( bool hideInactiveAccounts )
        {
            var preferences = GetBlockTypePersonPreferences();
            preferences.SetValue( PersonPreferenceKey.HideInactiveAccounts, hideInactiveAccounts.ToTrueFalse() );
            preferences.Save();

            return ActionOk();
        }

        /// <summary>
        /// Builds a pruned tree of the accounts matching the search term, keeping each match's ancestor
        /// chain so the result can be shown as a filtered tree.
        /// </summary>
        /// <param name="searchTerm">The text to match against account name, public name, or GL code.</param>
        /// <param name="includeInactive">Whether inactive accounts are included in the results.</param>
        /// <returns>The pruned, nested tree of matches and their ancestors.</returns>
        [BlockAction]
        public BlockActionResult GetSearchTree( string searchTerm, bool includeInactive )
        {
            var term = searchTerm?.Trim();
            if ( term.IsNullOrWhiteSpace() )
            {
                return ActionOk( new List<TreeItemBag>() );
            }

            var accountService = new FinancialAccountService( RockContext );
            var matchQuery = accountService.GetAccountsBySearchTerm( term );
            if ( !includeInactive )
            {
                matchQuery = matchQuery.Where( a => a.IsActive );
            }

            var matchIds = matchQuery.Select( a => a.Id ).ToList();

            // Collect each match plus its ancestor chain so every match keeps its full path in the tree.
            var includedIds = new HashSet<int>();
            foreach ( var matchId in matchIds )
            {
                var account = FinancialAccountCache.Get( matchId );
                if ( account == null )
                {
                    continue;
                }

                includedIds.Add( account.Id );
                foreach ( var ancestorId in account.GetAncestorFinancialAccountIds() )
                {
                    includedIds.Add( ancestorId );
                }
            }

            // Group the included accounts by parent once so the tree assembles without re-scanning the set at each level.
            var childrenByParent = includedIds
                .Select( id => FinancialAccountCache.Get( id ) )
                .Where( a => a != null )
                .ToLookup( a => a.ParentAccountId );

            var displayPublicName = GetAttributeValue( AttributeKey.UsePublicName ).AsBoolean();
            var tree = BuildSearchTreeItems( null, childrenByParent, displayPublicName );

            return ActionOk( tree );
        }

        #endregion Block Actions
    }
}

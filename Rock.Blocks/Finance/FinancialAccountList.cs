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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.FinancialAccountList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Displays a list of financial accounts.
    /// </summary>

    [DisplayName( "Account List" )]
    [Category( "Finance" )]
    [Description( "Displays a list of financial accounts." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the financial account details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "20cbcd56-e896-41de-ad82-0e3862d502b3" )]
    [Rock.SystemGuid.BlockTypeGuid( "57babd60-2a45-43ac-8ed3-b09af79c54ab" )]
    [CustomizedGrid]
    public class FinancialAccountList : RockEntityListBlockType<FinancialAccount>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string AccountId = "AccountId";
            public const string ParentAccountId = "ParentAccountId";
            public const string ExpandedIds = "ExpandedIds";
            public const string TopLevel = "TopLevel";
        }

        private static class PreferenceKey
        {
            public const string FilterAccountName = "filter-account-name";
            public const string FilterCampus = "filter-campus";
            public const string FilterIsPublic = "filter-is-public";
            public const string FilterIsActive = "filter-is-active";
            public const string FilterIsTaxDeductible = "filter-is-tax-deductible";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets the name of the account(s) to include in the result.
        /// </summary>
        /// <value>
        /// The name of the account.
        /// </value>
        protected string FilterAccountName => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterAccountName );

        /// <summary>
        /// Gets the name of the associated campus of account(s) to include in the result.
        /// </summary>
        /// <value>
        /// The name of the account.
        /// </value>
        protected string FilterCampus=> GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterCampus ).FromJsonOrNull<ListItemBag>()?.Value;

        /// <summary>
        /// If true only public accounts are included in the result.
        /// </summary>
        /// <value>
        /// The name of the account.
        /// </value>
        protected string FilterIsPublic => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterIsPublic );

        /// <summary>
        /// If true only active accounts are included in the result.
        /// </summary>
        /// <value>
        /// The name of the account.
        /// </value>
        protected string FilterIsActive => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterIsActive );

        /// <summary>
        /// If true only tax deductible accounts are included in the result.
        /// </summary>
        /// <value>
        /// The name of the account.
        /// </value>
        protected string FilterIsTaxDeductible => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterIsTaxDeductible );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<FinancialAccountListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddDeleteEnabled();
            box.IsDeleteEnabled = GetIsAddDeleteEnabled();
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private FinancialAccountListOptionsBag GetBoxOptions()
        {
            int? parentAccountId = PageParameter( PageParameterKey.AccountId ).AsIntegerOrNull();

            var options = new FinancialAccountListOptionsBag
            {
                GridTitle = parentAccountId.HasValue ? "Child Accounts".FormatAsHtmlTitle() : "Accounts".FormatAsHtmlTitle()
            };

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddDeleteEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add( PageParameterKey.AccountId, "((Key))" );
            queryParams.Add( PageParameterKey.ParentAccountId, PageParameter( PageParameterKey.AccountId ) );
            queryParams.Add( PageParameterKey.ExpandedIds, PageParameter( PageParameterKey.ExpandedIds ) );
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, queryParams )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<FinancialAccount> GetListQueryable( RockContext rockContext )
        {
            return GetAccounts( rockContext );
        }

        /// <inheritdoc/>
        protected override IQueryable<FinancialAccount> GetOrderedListQueryable( IQueryable<FinancialAccount> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( a => a.Order ).ThenBy( f => f.Name );
        }

        /// <summary>
        /// Gets the accounts query after applying filters.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private IQueryable<FinancialAccount> GetAccounts( RockContext rockContext )
        {
            var key = PageParameter( PageParameterKey.AccountId );
            var parentAccountId = Rock.Utility.IdHasher.Instance.GetId( key ) ?? key.AsIntegerOrNull();
            var topLevelOnly = PageParameter( PageParameterKey.TopLevel ).AsBoolean();

            var accountService = new FinancialAccountService( rockContext );
            var accountQuery = accountService.Queryable();

            if ( parentAccountId.HasValue )
            {
                accountQuery = accountQuery.Where( account => account.ParentAccountId == parentAccountId.Value );
            }
            else if ( topLevelOnly )
            {
                accountQuery = accountQuery.Where( account => account.ParentAccountId == null );
            }

            if ( !string.IsNullOrEmpty( FilterAccountName ) )
            {
                accountQuery = accountQuery.Where( account => account.Name.Contains( FilterAccountName ) );
            }

            var campusGuid = FilterCampus.AsGuidOrNull();
            if ( campusGuid.HasValue )
            {
                accountQuery = accountQuery.Where( account => account.Campus.Guid == campusGuid );
            }

            if ( !string.IsNullOrWhiteSpace( FilterIsPublic ) )
            {
                accountQuery = accountQuery.Where( account => ( account.IsPublic ?? false ) == ( FilterIsPublic == "Yes" ) );
            }

            if ( !string.IsNullOrWhiteSpace( FilterIsActive ) )
            {
                accountQuery = accountQuery.Where( account => account.IsActive == ( FilterIsActive == "Yes" ) );
            }

            if ( !string.IsNullOrWhiteSpace( FilterIsTaxDeductible ) )
            {
                accountQuery = accountQuery.Where( account => account.IsTaxDeductible == ( FilterIsTaxDeductible == "Yes" ) );
            }

            return accountQuery;
        }

        /// <inheritdoc/>
        protected override GridBuilder<FinancialAccount> GetGridBuilder()
        {
            return new GridBuilder<FinancialAccount>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "publicName", a => a.PublicName )
                .AddField( "isActive", a => a.IsActive )
                .AddField( "isPublic", a => a.IsPublic )
                .AddField( "isTaxDeductible", a => a.IsTaxDeductible )
                .AddDateTimeField( "startDate", a => a.StartDate )
                .AddDateTimeField( "endDate", a => a.EndDate )
                .AddAttributeFields( GetGridAttributes() );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Changes the ordered position of a single item.
        /// </summary>
        /// <param name="key">The identifier of the item that will be moved.</param>
        /// <param name="beforeKey">The identifier of the item it will be placed before.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult ReorderItem( string key, string beforeKey )
        {
            using ( var rockContext = new RockContext() )
            {
                // Get the queryable and make sure it is ordered correctly.
                var qry = GetListQueryable( rockContext );
                qry = GetOrderedListQueryable( qry, rockContext );

                // Get the entities from the database.
                var items = GetListItems( qry, rockContext );

                if ( !items.ReorderEntity( key, beforeKey ) )
                {
                    return ActionBadRequest( "Invalid reorder attempt." );
                }

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new FinancialAccountService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{FinancialAccount.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete {FinancialAccount.FriendlyTypeName}." );
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        #endregion
    }
}

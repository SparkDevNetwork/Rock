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
using Rock.ViewModels.Blocks.Finance.SavedAccountList;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Displays a list of financial person saved accounts.
    /// </summary>
    [DisplayName( "Saved Account List" )]
    [Category( "Finance" )]
    [Description( "List of a person's saved accounts that can be used to delete an account." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "ad9c4aac-54bb-498d-9bd3-47d8f21b9549" )]
    [Rock.SystemGuid.BlockTypeGuid( "e20b2fe2-2708-4e9a-b9fb-b370e8b0e702" )]
    [CustomizedGrid]
    [ContextAware( typeof( Person ) )]
    public class SavedAccountList : RockEntityListBlockType<FinancialPersonSavedAccount>
    {
        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<SavedAccountListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private SavedAccountListOptionsBag GetBoxOptions()
        {
            var options = new SavedAccountListOptionsBag();

            return options;
        }

        /// <inheritdoc/>
        protected override IQueryable<FinancialPersonSavedAccount> GetListQueryable( RockContext rockContext )
        {
            var currentPerson = GetCurrentPerson();
            var contextEntity = GetContextEntity() as Person;
            var personId = contextEntity?.Id ?? currentPerson?.Id;
            IEnumerable<FinancialPersonSavedAccount> savedAccounts = new List<FinancialPersonSavedAccount>();

            if ( personId.HasValue )
            {
                var entityService = new FinancialPersonSavedAccountService( rockContext );
                savedAccounts = entityService.Queryable()
                    .Include( a => a.FinancialPaymentDetail )
                    .AsNoTracking()
                    .Where( a =>
                            a.FinancialPaymentDetail != null &&
                            a.PersonAlias != null &&
                            a.PersonAlias.PersonId == personId.Value );
            }

            return savedAccounts.AsQueryable();
        }

        protected override IQueryable<FinancialPersonSavedAccount> GetOrderedListQueryable( IQueryable<FinancialPersonSavedAccount> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( a => a.Name );
        }

        /// <inheritdoc/>
        protected override GridBuilder<FinancialPersonSavedAccount> GetGridBuilder()
        {
            return new GridBuilder<FinancialPersonSavedAccount>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "accountNumber", a => a.FinancialPaymentDetail?.AccountNumberMasked )
                .AddTextField( "accountType", a => a.FinancialPaymentDetail?.CurrencyAndCreditCardType );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new FinancialPersonSavedAccountService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{FinancialPersonSavedAccount.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {FinancialPersonSavedAccount.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion
    }
}

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
using Rock.ViewModels.Blocks.Finance.FinancialPersonBankAccountList;
using Rock.Web.Cache;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Displays a list of financial person bank accounts.
    /// </summary>

    [DisplayName( "Bank Account List" )]
    [Category( "Finance" )]
    [Description( "Lists bank accounts for a person." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "30150fa5-a4e9-4767-a320-c9092b8ffd61" )]
    [Rock.SystemGuid.BlockTypeGuid( "e1dce349-2f5b-46ed-9f3d-8812af857f69" )]
    [CustomizedGrid]
    [Rock.Web.UI.ContextAware( typeof( Person ) )]
    public class FinancialPersonBankAccountList : RockEntityListBlockType<FinancialPersonBankAccount>
    {
        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<FinancialPersonBankAccountListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
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
        private FinancialPersonBankAccountListOptionsBag GetBoxOptions()
        {
            var options = new FinancialPersonBankAccountListOptionsBag();

            return options;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<FinancialPersonBankAccount> GetListQueryable( RockContext rockContext )
        {
            var personId = GetPersonForBankAccountList( rockContext )?.Id;
            return base.GetListQueryable( rockContext )
                .Include( a => a.PersonAlias )
                .Where( a => a.PersonAlias.PersonId == personId );
        }

        /// <inheritdoc/>
        protected override GridBuilder<FinancialPersonBankAccount> GetGridBuilder()
        {
            return new GridBuilder<FinancialPersonBankAccount>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "accountNumberMasked", a => a.AccountNumberMasked );
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
            var entityService = new FinancialPersonBankAccountService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{FinancialPersonBankAccount.FriendlyTypeName} not found." );
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

        #region Methods

        /// <summary>
        /// Gets the person object to use for rendering bank account lists.
        /// </summary>
        /// <returns>A <see cref="Person"/> object to use or <c>null</c> if we were unable to determine one.</returns>
        private Person GetPersonForBankAccountList( RockContext rockContext )
        {
            var person = RequestContext.GetContextEntity<Person>();

            if ( person != null )
            {
                return person;
            }

            var personKey = RequestContext.GetPageParameter( "personId" );

            return new PersonService( rockContext ).Get( personKey, !PageCache.Layout.Site.DisablePredictableIds );
        }

        #endregion
    }
}

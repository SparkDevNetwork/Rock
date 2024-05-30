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
using Rock.ViewModels.Blocks.Finance.FinancialStatementTemplateList;
using Rock.Web.Cache;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Displays a list of financial statement templates.
    /// </summary>

    [DisplayName( "Financial Statement Template List" )]
    [Category( "Finance" )]
    [Description( "Displays a list of financial statement templates." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the financial statement template details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "f46cd5a7-baf5-4eeb-8154-a4f4ac886264" )]
    [Rock.SystemGuid.BlockTypeGuid( "2eaf9e5a-f47d-4c58-9aa4-2d340547a35f" )]
    [CustomizedGrid]
    public class FinancialStatementTemplateList : RockEntityListBlockType<FinancialStatementTemplate>
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

        private static class PreferenceKey
        {
            public const string FilterName = "filter-name";
            public const string FilterIncludeInactive = "filter-include-inactive";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets the name of the templates(s) to include in the result.
        /// </summary>
        /// <value>
        /// The name of the template.
        /// </value>
        protected string FilterName => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterName );

        /// <summary>
        /// Determines whether or not to include inactive templates in the result.
        /// </summary>
        /// <value>
        /// The filter include inactive.
        /// </value>
        protected string FilterIncludeInactive => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterIncludeInactive );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<FinancialStatementTemplateListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddOrDeleteEnabled();
            box.IsDeleteEnabled = GetIsAddOrDeleteEnabled();
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
        private FinancialStatementTemplateListOptionsBag GetBoxOptions()
        {
            var options = new FinancialStatementTemplateListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddOrDeleteEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "FinancialStatementTemplateId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<FinancialStatementTemplate> GetListQueryable( RockContext rockContext )
        {
            var financialStatementTemplateService = new FinancialStatementTemplateService( rockContext );
            var queryable = financialStatementTemplateService.Queryable().AsNoTracking();

            // name filter
            if ( !string.IsNullOrEmpty( FilterName ) )
            {
                queryable = queryable.Where( a => a.Name.Contains( FilterName ) );
            }

            bool showInactiveAccounts = FilterIncludeInactive.AsBoolean();
            if ( !showInactiveAccounts )
            {
                queryable = queryable.Where( a => a.IsActive );
            }

            return queryable;
        }

        /// <inheritdoc/>
        protected override IQueryable<FinancialStatementTemplate> GetOrderedListQueryable( IQueryable<FinancialStatementTemplate> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( a => a.Name );
        }

        /// <inheritdoc/>
        protected override GridBuilder<FinancialStatementTemplate> GetGridBuilder()
        {
            return new GridBuilder<FinancialStatementTemplate>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "description", a => a.Description )
                .AddField( "isActive", a => a.IsActive );
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
            using ( var rockContext = new RockContext() )
            {
                var entityService = new FinancialStatementTemplateService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{FinancialStatementTemplate.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete {FinancialStatementTemplate.FriendlyTypeName}." );
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

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
using Rock.ViewModels.Blocks.Finance.FinancialPledgeList;
using Rock.Web.Cache;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Displays a list of financial pledges.
    /// </summary>

    [DisplayName( "Financial Pledge List" )]
    [Category( "Finance" )]
    [Description( "Displays a list of financial pledges." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the financial pledge details.",
        Key = AttributeKey.DetailPage )]

    [BooleanField( "Show Accounts Column",
        Description = "Should the accounts column be displayed.",
        DefaultBooleanValue = true,
        Key = AttributeKey.ShowAccountsColumn,
        Order = 2 )]

    [BooleanField( "Show Last Modified Date Column",
        Description = "Allows the Last Modified Date column to be hidden.",
        DefaultBooleanValue = true,
        Key = AttributeKey.ShowLastModifiedDateColumn,
        Order = 2 )]

    [BooleanField( "Show Group Column",
        Description = "Allows the group column to be hidden.",
        DefaultBooleanValue = false,
        Key = AttributeKey.ShowGroupColumn,
        Order = 3 )]

    [BooleanField( "Limit Pledges To Current Person",
        Description = "Limit the results to pledges for the current person.",
        DefaultBooleanValue = false,
        Key = AttributeKey.LimitPledgesToCurrentPerson,
        Order = 4 )]

    [BooleanField( "Show Account Summary",
        Description = "Should the account summary be displayed at the bottom of the list?",
        DefaultBooleanValue = false,
        Key = AttributeKey.ShowAccountSummary,
        Order = 5 )]

    [Rock.SystemGuid.EntityTypeGuid( "8b1663eb-b5cb-4c78-b0c6-ed14e173e4c0" )]
    [Rock.SystemGuid.BlockTypeGuid( "31fb8c39-80bd-4ea9-a1cb-bf6c4667929b" )]
    [CustomizedGrid]
    public class FinancialPledgeList : RockEntityListBlockType<FinancialPledge>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string ShowAccountsColumn = "ShowAccountsColumn";
            public const string ShowLastModifiedDateColumn = "ShowLastModifiedDateColumn";
            public const string ShowGroupColumn = "ShowGroupColumn";
            public const string LimitPledgesToCurrentPerson = "LimitPledgesToCurrentPerson";
            public const string ShowAccountSummary = "ShowAccountSummary";
            public const string Accounts = "Accounts";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<FinancialPledgeListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
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
        private FinancialPledgeListOptionsBag GetBoxOptions()
        {
            var options = new FinancialPledgeListOptionsBag()
            {
                ShowAccountsColumn = GetAttributeValue(AttributeKey.ShowAccountsColumn).AsBoolean(),
                ShowLastModifiedDateColumn = GetAttributeValue(AttributeKey.ShowLastModifiedDateColumn).AsBoolean(),
                ShowGroupColumn = GetAttributeValue(AttributeKey.ShowGroupColumn).AsBoolean(),
                LimitPledgesToCurrentPerson = GetAttributeValue(AttributeKey.LimitPledgesToCurrentPerson).AsBoolean(),
                ShowAccountSummary = GetAttributeValue(AttributeKey.ShowAccountSummary).AsBoolean(),
            };
            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "PledgeId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<FinancialPledge> GetListQueryable( RockContext rockContext )
        {
            var query = base.GetListQueryable( rockContext )
                    .Include( a => a.PersonAlias )
                    .Include( a => a.Account )
                    .Include( a => a.PledgeFrequencyValue )
                    .Include( a => a.Group );

            /// If the 'LimitPledgesToCurrentPerson' option is enabled, filter by current person
            if ( GetAttributeValue( AttributeKey.LimitPledgesToCurrentPerson ).AsBoolean() )
            {
                var currentPersonId = RequestContext.CurrentPerson?.Id;

                if ( currentPersonId.HasValue )
                {
                    query = query.Where(a => a.PersonAlias.PersonId == currentPersonId.Value);
                }
            }

            return query;
        }

        /// <inheritdoc/>
        protected override GridBuilder<FinancialPledge> GetGridBuilder()
        {
            return new GridBuilder<FinancialPledge>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddField( "id", a => a.Id )
                .AddPersonField( "person", a => a.PersonAlias?.Person )
                .AddTextField( "account", a => a.Account?.Name )
                .AddTextField( "group", a => a.Group?.Name ?? "" )
                .AddField( "totalAmount", a => a.TotalAmount )
                .AddTextField( "pledgeFrequency", a => a.PledgeFrequencyValue?.Value )
                .AddField( "startDate", a => a.StartDate )
                .AddField( "endDate", a => a.EndDate )
                .AddField( "modifiedDate", a => a.ModifiedDateTime )
                .AddAttributeFields( GetGridAttributes() );
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
                var entityService = new FinancialPledgeService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{FinancialPledge.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${FinancialPledge.FriendlyTypeName}." );
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

        /// <summary>
        /// Gets the count of financial pledges.
        /// </summary>
        /// <returns>The count of financial pledges.</returns>
        [BlockAction]
        public BlockActionResult GetPledgeCount()
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new FinancialPledgeService( rockContext );
                var count = entityService.Queryable().Count();
                return ActionOk( count );
            }
        }

        #endregion
    }
}
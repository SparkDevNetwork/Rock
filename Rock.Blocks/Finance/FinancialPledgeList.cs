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
using Rock.Web.UI;

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
        Key = AttributeKey.DetailPage,
        Description = "",
        IsRequired = false )]

    [BooleanField( "Show Account Column",
        Key = AttributeKey.ShowAccountColumn,
        Description = "Allows the account column to be hidden.",
        DefaultBooleanValue = true,
        Category = "",
        Order = 1 )]

    [BooleanField( "Show Last Modified Date Column",
        Key = AttributeKey.ShowLastModifiedDateColumn,
        Description = "Allows the Last Modified Date column to be hidden.",
        DefaultBooleanValue = true,
        Category = "",
        Order = 2 )]

    [BooleanField( "Show Group Column",
        Key = AttributeKey.ShowGroupColumn,
        Description = "Allows the group column to be hidden.",
        DefaultBooleanValue = false,
        Category = "",
        Order = 3 )]

    [BooleanField( "Limit Pledges To Current Person",
        Key = AttributeKey.LimitPledgesToCurrentPerson,
        Description = "Limit the results to pledges for the current person.",
        DefaultBooleanValue = false,
        Category = "",
        Order = 4 )]

    [BooleanField( "Show Account Summary",
        Key = AttributeKey.ShowAccountSummary,
        Description = "Should the account summary be displayed at the bottom of the list?",
        DefaultBooleanValue = true,
        Order = 5 )]

    [AccountsField( "Accounts",
        Key = AttributeKey.Accounts,
        Description = "Limit the results to pledges that match the selected accounts.",
        IsRequired = false,
        DefaultValue = "",
        Category = "",
        Order = 5 )]

    [BooleanField( "Hide Amount",
        Key = AttributeKey.HideAmount,
        Description = "Allows the amount column to be hidden.",
        DefaultBooleanValue = false,
        Category = "",
        Order = 6 )]

    [BooleanField( "Show Person Filter",
        Key = AttributeKey.ShowPersonFilter,
        Description = "Allows person filter to be hidden.",
        DefaultBooleanValue = true,
        Category = "Display Filters",
        Order = 0 )]

    [BooleanField( "Show Account Filter",
        Key = AttributeKey.ShowAccountFilter,
        Description = "Allows account filter to be hidden.",
        DefaultBooleanValue = true,
        Category = "Display Filters",
        Order = 1 )]

    [BooleanField( "Show Date Range Filter",
        Key = AttributeKey.ShowDateRangeFilter,
        Description = "Allows date range filter to be hidden.",
        DefaultBooleanValue = true,
        Category = "Display Filters",
        Order = 2 )]

    [BooleanField( "Show Last Modified Filter",
        Key = AttributeKey.ShowLastModifiedFilter,
        Description = "Allows last modified filter to be hidden.",
        DefaultBooleanValue = true,
        Category = "Display Filters",
        Order = 3 )]

    [ContextAware]

    [Rock.SystemGuid.EntityTypeGuid( "8b1663eb-b5cb-4c78-b0c6-ed14e173e4c0" )]
    [Rock.SystemGuid.BlockTypeGuid( "31fb8c39-80bd-4ea9-a1cb-bf6c4667929b" )]
    [CustomizedGrid]
    public class FinancialPledgeList : RockEntityListBlockType<FinancialPledge>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string ShowAccountColumn = "ShowAccountsColumn";
            public const string ShowLastModifiedDateColumn = "ShowLastModifiedDateColumn";
            public const string ShowGroupColumn = "ShowGroupColumn";
            public const string LimitPledgesToCurrentPerson = "LimitPledgesToCurrentPerson";
            public const string ShowAccountSummary = "ShowAccountSummary";
            public const string Accounts = "Accounts";
            public const string HideAmount = "HideAmount";
            public const string ShowPersonFilter = "ShowPersonFilter";
            public const string ShowAccountFilter = "ShowAccountFilter";
            public const string ShowDateRangeFilter = "ShowDateRangeFilter";
            public const string ShowLastModifiedFilter = "ShowLastModifiedFilter";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PreferenceKey
        {
            public const string DateRange = "Date Range";
            public const string LastModified = "Last Modified";
            public const string Person = "Person";
            public const string Accounts = "Accounts";
            public const string ActiveOnly = "Active Only";
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
                ShowAccountColumn = GetAttributeValue( AttributeKey.ShowAccountColumn ).AsBoolean(),
                ShowLastModifiedDateColumn = GetAttributeValue( AttributeKey.ShowLastModifiedDateColumn ).AsBoolean(),
                ShowGroupColumn = GetAttributeValue( AttributeKey.ShowGroupColumn ).AsBoolean(),
                LimitPledgesToCurrentPerson = GetAttributeValue( AttributeKey.LimitPledgesToCurrentPerson ).AsBoolean(),
                ShowAccountSummary = GetAttributeValue( AttributeKey.ShowAccountSummary ).AsBoolean(),
                HideAmount = GetAttributeValue( AttributeKey.HideAmount ).AsBoolean(),
                ShowPersonFilter = GetAttributeValue( AttributeKey.ShowPersonFilter ).AsBoolean(),
                ShowAccountFilter = GetAttributeValue( AttributeKey.ShowAccountFilter ).AsBoolean(),
                ShowDateRangeFilter = GetAttributeValue( AttributeKey.ShowDateRangeFilter ).AsBoolean(),
                ShowLastModifiedFilter = GetAttributeValue( AttributeKey.ShowLastModifiedFilter ).AsBoolean()
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

            // If the 'LimitPledgesToCurrentPerson' option is enabled, filter by current person
            if ( GetAttributeValue( AttributeKey.LimitPledgesToCurrentPerson ).AsBoolean() )
            {
                var currentPersonId = RequestContext.CurrentPerson?.Id;

                if ( currentPersonId.HasValue )
                {
                    query = query.Where( a => a.PersonAlias.PersonId == currentPersonId.Value );
                }
            }

            // Filter by configured limit accounts if specified
            var accountGuids = GetAttributeValue( AttributeKey.Accounts ).SplitDelimitedValues().AsGuidList();
            if ( accountGuids.Any() )
            {
                query = query.Where( p => accountGuids.Contains( p.Account.Guid ) );
            }

            // Filter by "Active Only" user preference
            var activeOnly = this.GetBlockPersonPreferences().GetValue( PreferenceKey.ActiveOnly ).AsBoolean();
            if ( activeOnly )
            {
                query = query.Where( p => p.StartDate <= RockDateTime.Now && p.EndDate >= RockDateTime.Now );
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
                    return ActionBadRequest( $"Not authorized to delete {FinancialPledge.FriendlyTypeName}." );
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

        /// <summary>
        /// Applies filters and binds the grid with the filtered data.
        /// </summary>
        [BlockAction]
        public BlockActionResult ApplyFilter( string dateRange, string lastModified, string personGuid, List<Guid> accountGuids, bool activeOnly, string attributeFiltersJson )
        {
            using ( var rockContext = new RockContext() )
            {
                var pledges = GetListQueryable( rockContext );

                // Always start with the base query to ensure correct filtering
                var filteredPledges = pledges;

                if ( !string.IsNullOrEmpty( personGuid ) )
                {
                    var person = new PersonAliasService( rockContext ).Get( personGuid.AsGuid() );
                    if ( person != null )
                    {
                        filteredPledges = filteredPledges.Where( p => p.PersonAlias.Person.GivingId == person.Person.GivingId );
                    }
                }

                if ( accountGuids != null && accountGuids.Any() )
                {
                    var accountIds = new FinancialAccountService( rockContext )
                        .GetListByGuids( accountGuids )
                        .Select( a => a.Id )
                        .ToList();

                    filteredPledges = filteredPledges.Where( p => p.AccountId.HasValue && accountIds.Contains( p.AccountId.Value ) );
                }

                var filterDateRange = RockDateTimeHelper.CalculateDateRangeFromDelimitedValues( dateRange );
                if ( filterDateRange.Start.HasValue || filterDateRange.End.HasValue )
                {
                    var filterStartDate = filterDateRange.Start ?? DateTime.MinValue;
                    var filterEndDate = filterDateRange.End ?? DateTime.MaxValue;
                    filteredPledges = filteredPledges.Where( p => p.StartDate >= filterStartDate && p.EndDate <= filterEndDate );
                }

                var filterLastModifiedRange = RockDateTimeHelper.CalculateDateRangeFromDelimitedValues( lastModified );
                if ( filterLastModifiedRange.Start.HasValue || filterLastModifiedRange.End.HasValue )
                {
                    var filterStartDate = filterLastModifiedRange.Start ?? DateTime.MinValue;
                    var filterEndDate = filterLastModifiedRange.End ?? DateTime.MaxValue;
                    filteredPledges = filteredPledges.Where( p => p.ModifiedDateTime >= filterStartDate && p.ModifiedDateTime <= filterEndDate );
                }

                if ( activeOnly )
                {
                    filteredPledges = filteredPledges.Where( p => p.StartDate <= RockDateTime.Now && p.EndDate >= RockDateTime.Now );
                }

                // Apply attribute filters if configured
                var attributeFilters = attributeFiltersJson.FromJsonOrNull<Dictionary<string, List<string>>>() ?? new Dictionary<string, List<string>>();

                if ( attributeFilters.Any() )
                {
                    foreach ( var filter in attributeFilters )
                    {
                        var attribute = AttributeCache.Get( filter.Key );
                        if ( attribute != null )
                        {
                            var attributeValues = new AttributeValueService( rockContext )
                                .Queryable()
                                .Where( v => v.AttributeId == attribute.Id && filter.Value.Contains( v.Value ) )
                                .Select( v => v.EntityId )
                                .ToList();

                            filteredPledges = filteredPledges.Where( p => attributeValues.Contains( p.Id ) );
                        }
                    }
                }

                var result = filteredPledges.ToList()
                    .Select( p => new
                    {
                        p.IdKey,
                        p.Id,
                        Person = new
                        {
                            p.PersonAlias.Person.NickName,
                            p.PersonAlias.Person.LastName,
                            p.PersonAlias.Person.PhotoUrl
                        },
                        Account = p.Account.Name,
                        Group = p.Group != null ? p.Group.Name : "",
                        p.TotalAmount,
                        PledgeFrequency = p.PledgeFrequencyValue != null ? p.PledgeFrequencyValue.Value : null,
                        StartDate = p.StartDate.Year == 1 ? ( DateTime? ) null : p.StartDate,
                        EndDate = p.EndDate.Year == 9999 ? ( DateTime? ) null : p.EndDate,
                        ModifiedDate = p.ModifiedDateTime
                    } )
                    .ToList();

                return ActionOk( new
                {
                    Rows = result
                } );
            }
        }
        #endregion
    }
}

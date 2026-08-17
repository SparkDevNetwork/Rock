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
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Event.RegistrationInstanceDiscountList;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Displays the discounts related to an event registration instance.
    /// </summary>
    [DisplayName( "Registration Instance - Discount List" )]
    [Category( "Event" )]
    [Description( "Displays the discounts related to an event registration instance." )]
    [IconCssClass( "ti ti-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Secondary )]
    [Rock.SystemGuid.EntityTypeGuid( "1F95772E-90B6-4A43-8689-B3A685FA49E1" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "FC4961F9-3AF0-4E83-919C-BFBA7CE92A06" )]
    [Rock.SystemGuid.BlockTypeGuid( "6C8954BF-E221-4B2F-AC3B-612DC16BA27D" )]
    [CustomizedGrid]
    [ContextAware( typeof( RegistrationInstance ) )]
    public class RegistrationInstanceDiscountList : RockListBlockType<TemplateDiscountReport>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string RegistrationInstanceId = "RegistrationInstanceId";
        }

        private static class PreferenceKey
        {
            public const string FilterDateRange = "filter-date-range";
            public const string FilterDiscountCode = "filter-discount-code";
            public const string FilterCodeSearch = "filter-code-search";
        }

        #endregion Keys

        #region Fields

        protected RegistrationInstance _registrationInstance;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the date range filter from person preferences.
        /// </summary>
        private SlidingDateRangeBag FilterDateRange => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterDateRange )
            .ToSlidingDateRangeBagOrNull();

        /// <summary>
        /// Gets the discount code filter from person preferences.
        /// </summary>
        private string FilterDiscountCode => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterDiscountCode );

        /// <summary>
        /// Gets the code search filter from person preferences.
        /// </summary>
        private string FilterCodeSearch => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterCodeSearch );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<RegistrationInstanceDiscountListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private RegistrationInstanceDiscountListOptionsBag GetBoxOptions()
        {
            var options = new RegistrationInstanceDiscountListOptionsBag();
            var registrationInstance = GetRegistrationInstance();

            if ( registrationInstance != null )
            {
                options.ExportTitleName = registrationInstance.Name + " - Discount Codes";
                options.DiscountCodeItems = GetDiscountCodeItems( registrationInstance );
            }

            return options;
        }

        /// <summary>
        /// Gets the list of discount codes available for the registration instance,
        /// formatted as list item bags for the filter dropdown.
        /// </summary>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <returns>A list of discount code items.</returns>
        private List<ListItemBag> GetDiscountCodeItems( RegistrationInstance registrationInstance )
        {
            var discountService = new RegistrationTemplateDiscountService( RockContext );

            return discountService
                .GetDiscountsForRegistrationInstance( registrationInstance.Id )
                .OrderBy( d => d.Code )
                .Select( d => new ListItemBag
                {
                    Value = d.Code,
                    Text = d.Code
                } )
                .ToList();
        }

        /// <inheritdoc/>
        protected override IQueryable<TemplateDiscountReport> GetListQueryable( RockContext rockContext )
        {
            var registrationInstance = GetRegistrationInstance();

            if ( registrationInstance == null )
            {
                return Enumerable.Empty<TemplateDiscountReport>().AsQueryable();
            }

            var discountService = new RegistrationTemplateDiscountService( rockContext );
            IEnumerable<TemplateDiscountReport> data = discountService.GetRegistrationInstanceDiscountCodeReport( registrationInstance.Id );

            // Apply date range filter.
            var dateRange = FilterDateRange?.ToActualDateRange();
            if ( dateRange?.Start != null )
            {
                data = data.Where( r => r.RegistrationDate >= dateRange.Start.Value );
            }

            if ( dateRange?.End != null )
            {
                data = data.Where( r => r.RegistrationDate < dateRange.End.Value );
            }

            // Apply discount code filter (exact match from dropdown).
            var discountCode = FilterDiscountCode;
            if ( discountCode.IsNotNullOrWhiteSpace() )
            {
                data = data.Where( r => r.DiscountCode == discountCode );
            }
            else
            {
                // Apply code search filter only when discount code dropdown is not selected.
                var codeSearch = FilterCodeSearch;
                if ( codeSearch.IsNotNullOrWhiteSpace() )
                {
                    data = data.Where( r => r.DiscountCode != null
                        && r.DiscountCode.IndexOf( codeSearch, StringComparison.OrdinalIgnoreCase ) >= 0 );
                }
            }

            return data.AsQueryable();
        }

        /// <inheritdoc/>
        protected override IQueryable<TemplateDiscountReport> GetOrderedListQueryable( IQueryable<TemplateDiscountReport> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( r => r.RegistrationDate );
        }

        /// <inheritdoc/>
        protected override GridBuilder<TemplateDiscountReport> GetGridBuilder()
        {
            return new GridBuilder<TemplateDiscountReport>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.RegistrationId.ToString() )
                .AddTextField( "registrationId", a => a.RegistrationId.ToString() )
                .AddTextField( "registeredByName", a => a.RegisteredByName )
                .AddDateTimeField( "registrationDate", a => a.RegistrationDate )
                .AddField( "registrantCount", a => a.RegistrantCount )
                .AddTextField( "discountCode", a => a.DiscountCode )
                .AddTextField( "discount", a => a.Discount )
                .AddField( "totalCost", a => a.TotalCost )
                .AddField( "discountQualifiedCost", a => a.DiscountQualifiedCost )
                .AddField( "totalDiscount", a => a.TotalDiscount )
                .AddField( "registrationCost", a => a.RegistrationCost );
        }

        /// <summary>
        /// Gets the registration instance from context entity or page parameter.
        /// </summary>
        /// <returns>The registration instance, or null if not found.</returns>
        private RegistrationInstance GetRegistrationInstance()
        {
            if ( _registrationInstance == null )
            {
                _registrationInstance = RequestContext.GetContextEntity<RegistrationInstance>();

                if ( _registrationInstance == null )
                {
                    var registrationInstanceKey = PageParameter( PageParameterKey.RegistrationInstanceId );

                    if ( registrationInstanceKey.IsNotNullOrWhiteSpace() )
                    {
                        _registrationInstance = new RegistrationInstanceService( RockContext )
                            .Get( registrationInstanceKey, !PageCache.Layout.Site.DisablePredictableIds );
                    }
                }
            }

            return _registrationInstance;
        }

        #endregion

        #region Block Actions

        /// <inheritdoc/>
        public override BlockActionResult GetGridData()
        {
            var qry = GetListQueryable( RockContext );
            qry = GetOrderedListQueryable( qry, RockContext );
            var items = GetListItems( qry, RockContext );

            var gridData = GetGridBuilder().Build( items );

            var response = new RegistrationInstanceDiscountListGetGridDataResponseBag
            {
                GridData = gridData,
                TotalCost = items.Sum( r => r.TotalCost ),
                DiscountQualifiedCost = items.Sum( r => r.DiscountQualifiedCost ),
                TotalDiscount = items.Sum( r => r.TotalDiscount ),
                RegistrationCost = items.Sum( r => r.RegistrationCost ),
                TotalRegistrations = items.Count,
                TotalRegistrants = items.Sum( r => r.RegistrantCount )
            };

            return ActionOk( response );
        }

        #endregion
    }
}

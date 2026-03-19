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
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.ExceptionOccurrenceList;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Lists all exception occurrences matching a specific type and description.
    /// </summary>
    [DisplayName( "Exception Occurrences" )]
    [Category( "Core" )]
    [Description( "Lists all exception occurrences matching a specific type and description." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Detail Page",
        Description = "The page that will show the exception detail.",
        Key = AttributeKey.DetailPage,
        Category = "Linked Pages",
        Order = 1 )]

    #endregion

    [SystemGuid.EntityTypeGuid( "6DCABAB1-CC57-4683-A0EC-326A930171DD" )]
    // was [SystemGuid.BlockTypeGuid( "DBF895CE-FFFC-45DB-88EA-3CA73838EA1B" )]
    [Rock.SystemGuid.BlockTypeGuid( "E3486885-FA88-4B67-88B6-472F1FE4E5E4" )]
    [CustomizedGrid]
    public class ExceptionOccurrenceList : RockEntityListBlockType<ExceptionLog>
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
            public const string ExceptionId = "ExceptionId";
        }

        private static class PreferenceKey
        {
            public const string FilterSite = "filter-site";
            public const string FilterPage = "filter-page";
            public const string FilterUser = "filter-user";
            public const string FilterDateRange = "filter-date-range";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets the site unique identifier to filter by, if any.
        /// </summary>
        private Guid? FilterSiteGuid => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterSite )
            .AsGuidOrNull();

        /// <summary>
        /// Gets the page unique identifier to filter by, if any.
        /// </summary>
        private Guid? FilterPageGuid => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterPage )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        /// <summary>
        /// Gets the person alias unique identifier to filter by, if any.
        /// </summary>
        private Guid? FilterUserPersonAliasGuid => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterUser )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        /// <summary>
        /// Gets the date range to filter by, if any.
        /// </summary>
        private SlidingDateRangeBag FilterDateRange => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterDateRange )
            .ToSlidingDateRangeBagOrNull();

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<ExceptionOccurrenceListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = false;
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
        private ExceptionOccurrenceListOptionsBag GetBoxOptions()
        {
            var options = new ExceptionOccurrenceListOptionsBag();

            // Load the template exception from the page parameter.
            var templateException = GetTemplateException( RockContext );

            if ( templateException != null )
            {
                var shortDescription = templateException.Description?.Truncate( ExceptionLogService.DescriptionGroupingPrefixLength );

                options.ExceptionType = templateException.ExceptionType;
                options.Description = shortDescription;
                options.HeaderText = string.Format( "({0}) {1}", templateException.ExceptionType, shortDescription ?? string.Empty );
            }

            // Populate site items for the filter dropdown.
            options.SiteItems = SiteCache.All()
                .OrderBy( s => s.Name )
                .ToListItemBagList();

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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "ExceptionId", "((Key))" )
            };
        }

        /// <summary>
        /// Gets the template exception from the ExceptionId page parameter.
        /// </summary>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <returns>The template exception, or null if the parameter is missing or invalid.</returns>
        private ExceptionLog GetTemplateException( RockContext rockContext )
        {
            var key = PageParameter( PageParameterKey.ExceptionId );

            if ( key.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new ExceptionLogService( rockContext ).Get( key, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <inheritdoc/>
        protected override IQueryable<ExceptionLog> GetListQueryable( RockContext rockContext )
        {
            var exceptionLogService = new ExceptionLogService( rockContext );

            var queryable = base.GetListQueryable( rockContext )
                .AsNoTracking()
                .Include( e => e.Page )
                .Include( e => e.CreatedByPersonAlias.Person );

            // Load the template exception to determine the filter criteria.
            var templateException = GetTemplateException( rockContext );

            if ( templateException == null )
            {
                return queryable.Where( e => false );
            }

            // Filter to outermost exceptions matching the template description prefix.
            queryable = exceptionLogService.FilterByOutermost( queryable );
            queryable = exceptionLogService.FilterByDescriptionPrefix( queryable, templateException.Description );

            // Apply preference filters.
            var siteGuid = FilterSiteGuid;
            if ( siteGuid.HasValue )
            {
                var siteCache = SiteCache.Get( siteGuid.Value );
                if ( siteCache != null )
                {
                    var siteId = siteCache.Id;
                    queryable = queryable.Where( e => e.SiteId == siteId );
                }
            }

            var pageGuid = FilterPageGuid;
            if ( pageGuid.HasValue )
            {
                var pageCache = PageCache.Get( pageGuid.Value );
                if ( pageCache != null )
                {
                    var pageId = pageCache.Id;
                    queryable = queryable.Where( e => e.PageId == pageId );
                }
            }

            var userPersonAliasGuid = FilterUserPersonAliasGuid;
            if ( userPersonAliasGuid.HasValue )
            {
                var personId = new PersonAliasService( rockContext ).Queryable()
                    .Where( pa => pa.Guid == userPersonAliasGuid.Value )
                    .Select( pa => pa.PersonId )
                    .FirstOrDefault();

                if ( personId != 0 )
                {
                    queryable = queryable.Where( e => e.CreatedByPersonAlias != null && e.CreatedByPersonAlias.PersonId == personId );
                }
            }

            var slidingDateRange = FilterDateRange;
            if ( slidingDateRange != null )
            {
                var dateRange = slidingDateRange.Validate( slidingDateRange ).ActualDateRange;

                if ( dateRange.Start.HasValue )
                {
                    queryable = queryable.Where( e => e.CreatedDateTime.HasValue && e.CreatedDateTime.Value >= dateRange.Start.Value );
                }

                if ( dateRange.End.HasValue )
                {
                    queryable = queryable.Where( e => e.CreatedDateTime.HasValue && e.CreatedDateTime.Value < dateRange.End.Value );
                }
            }

            return queryable;
        }

        /// <inheritdoc/>
        protected override IQueryable<ExceptionLog> GetOrderedListQueryable( IQueryable<ExceptionLog> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( e => e.CreatedDateTime );
        }

        /// <inheritdoc/>
        protected override GridBuilder<ExceptionLog> GetGridBuilder()
        {
            return new GridBuilder<ExceptionLog>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddField( "id", a => a.Id )
                .AddDateTimeField( "createdDateTime", a => a.CreatedDateTime )
                .AddTextField( "pageName", a => a.Page != null ? a.Page.InternalName : a.PageUrl )
                .AddTextField( "fullName", a => a.CreatedByPersonAlias != null && a.CreatedByPersonAlias.Person != null
                    ? a.CreatedByPersonAlias.Person.LastName + ", " + a.CreatedByPersonAlias.Person.NickName
                    : "" )
                .AddTextField( "description", a => a.Description );
        }

        #endregion Methods
    }
}

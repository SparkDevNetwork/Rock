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
using Rock.Enums.Controls;
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
    public class ExceptionOccurrenceList : RockListBlockType<ExceptionOccurrenceList.ExceptionOccurrenceRow>
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

        #region Fields

        /// <summary>
        /// The number of leading description characters selected and displayed for each row, so the entire
        /// nvarchar(max) description is never read for every row.
        /// </summary>
        private const int DescriptionDisplayLength = 255;

        #endregion Fields

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
                options.ExceptionType = templateException.ExceptionType;
                options.Description = templateException.Description;
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
        protected override IQueryable<ExceptionOccurrenceRow> GetListQueryable( RockContext rockContext )
        {
            var exceptionLogService = new ExceptionLogService( rockContext );
            var queryable = exceptionLogService.Queryable();

            // Load the template exception to determine the filter criteria.
            var templateException = GetTemplateException( rockContext );

            if ( templateException == null )
            {
                return SelectExceptionOccurrenceRows( queryable.Where( e => false ) );
            }

            // Filter to outermost exceptions in the same group as the template exception, that is, those sharing
            // its ExceptionType and the first 255 characters of its Description.
            queryable = exceptionLogService.FilterByOutermost( queryable );
            queryable = exceptionLogService.FilterByExceptionGroupHash( queryable, templateException.ExceptionGroupHash );

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

            /*
                8/26/26 - MSE

                The group hash is an INCLUDE column of IX_Outermost_ParentId_CreatedDateTime, not a key column, so
                filtering by hash alone makes SQL Server read every outermost exception in the index. Bounding the
                query by CreatedDateTime turns it into a seek on the index key, with the hash applied as a residual
                predicate. An index keyed on the hash was measured and rejected: it cut this query from 361 logical
                reads to 210, already under 2 ms, for a 45% increase in the index storage this table carries.

                Because the hash is a nullable byte[], EF6 compiles the equality below as
                "[ExceptionGroupHash] = @hash OR ([ExceptionGroupHash] IS NULL AND @hash IS NULL)". Harmless while
                the hash is only a residual predicate and can never actually be null, but that OR would stop it
                being seekable if the hash were ever promoted to an index key, the same shape that keeps
                ExceptionLogService.GetByParentId off a filtered index.

                Reason: Bound the query by CreatedDateTime so it seeks the index key, since the hash is only an INCLUDE column.
            */
            var defaultSlidingDateRange = new SlidingDateRangeBag
            {
                RangeType = SlidingDateRangeType.Last,
                TimeUnit = TimeUnitType.Month,
                TimeValue = 1
            };

            var dateRange = FilterDateRange.Validate( defaultSlidingDateRange ).ActualDateRange;
            var dateTimeStart = dateRange.Start;
            var dateTimeEnd = dateRange.End;

            queryable = queryable.Where( e =>
                e.CreatedDateTime >= dateTimeStart
                && e.CreatedDateTime < dateTimeEnd
            );

            return SelectExceptionOccurrenceRows( queryable );
        }

        /// <inheritdoc/>
        protected override IQueryable<ExceptionOccurrenceRow> GetOrderedListQueryable( IQueryable<ExceptionOccurrenceRow> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( r => r.CreatedDateTime );
        }

        /// <inheritdoc/>
        protected override GridBuilder<ExceptionOccurrenceRow> GetGridBuilder()
        {
            return new GridBuilder<ExceptionOccurrenceRow>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.Id.AsIdKey() )
                .AddField( "id", a => a.Id )
                .AddDateTimeField( "createdDateTime", a => a.CreatedDateTime )
                .AddTextField( "pageName", a => a.PageInternalName ?? a.PageUrl )
                .AddTextField( "fullName", a => a.CreatedByPersonAliasId.HasValue
                    ? a.PersonLastName + ", " + a.PersonNickName
                    : "" )
                .AddTextField( "description", a => a.Description );
        }

        /// <summary>
        /// Projects the exception logs to the columns the grid displays, so the full description, stack trace,
        /// server variables, form and cookies (all nvarchar(max)) are never read for each row.
        /// </summary>
        /// <param name="queryable">The filtered exception logs.</param>
        /// <returns>A queryable of grid rows.</returns>
        private static IQueryable<ExceptionOccurrenceRow> SelectExceptionOccurrenceRows( IQueryable<ExceptionLog> queryable )
        {
            return queryable.Select( e => new ExceptionOccurrenceRow
            {
                Id = e.Id,
                CreatedDateTime = e.CreatedDateTime,
                PageInternalName = e.Page.InternalName,
                PageUrl = e.PageUrl,
                CreatedByPersonAliasId = e.CreatedByPersonAliasId,
                PersonLastName = e.CreatedByPersonAlias.Person.LastName,
                PersonNickName = e.CreatedByPersonAlias.Person.NickName,
                Description = e.Description.Substring( 0, DescriptionDisplayLength )
            } );
        }

        #endregion Methods

        #region Supporting Classes

        /// <summary>
        /// A POCO to represent one exception occurrence row in the grid.
        /// </summary>
        public class ExceptionOccurrenceRow
        {
            /// <summary>
            /// Gets or sets the identifier of the <see cref="ExceptionLog"/>.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the date and time the exception was logged.
            /// </summary>
            public DateTime? CreatedDateTime { get; set; }

            /// <summary>
            /// Gets or sets the internal name of the <see cref="Page"/> the exception occurred on, if any.
            /// </summary>
            public string PageInternalName { get; set; }

            /// <inheritdoc cref="ExceptionLog.PageUrl"/>
            public string PageUrl { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the <see cref="PersonAlias"/> of the person who was logged in when
            /// the exception occurred, if any.
            /// </summary>
            public int? CreatedByPersonAliasId { get; set; }

            /// <summary>
            /// Gets or sets the last name of the person who was logged in when the exception occurred, if any.
            /// </summary>
            public string PersonLastName { get; set; }

            /// <summary>
            /// Gets or sets the nick name of the person who was logged in when the exception occurred, if any.
            /// </summary>
            public string PersonNickName { get; set; }

            /// <summary>
            /// Gets or sets the leading <see cref="DescriptionDisplayLength"/> characters of the exception's
            /// <see cref="ExceptionLog.Description"/>.
            /// </summary>
            public string Description { get; set; }
        }

        #endregion Supporting Classes
    }
}

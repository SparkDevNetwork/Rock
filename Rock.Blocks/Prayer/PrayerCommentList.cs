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
using Rock.Enums.Controls;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Prayer.PrayerCommentList;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Prayer
{
    /// <summary>
    /// Displays a list of prayer comments for the configured top-level group category.
    /// </summary>

    [DisplayName( "Prayer Comment List" )]
    [Category( "Core" )]
    [Description( "Displays a list of prayer comments for the configured top-level group category." )]
    [IconCssClass( "ti ti-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage )]
    [CategoryField( "Category Selection",
        Description = "A top level category. Only prayer requests comments under this category will be shown.",
        AllowMultiple = false,
        EntityTypeName = "Rock.Model.PrayerRequest",
        IsRequired = false,
        Category = "Category Selection",
        Order = 1,
        Key = AttributeKey.PrayerRequestCategory )]
    [Rock.SystemGuid.EntityTypeGuid( "b2f1b644-836d-46a6-86c9-8fbb26d96ea7" )]
    [Rock.SystemGuid.BlockTypeGuid( "3f997da7-ac42-41c9-97f1-2069bb9d9e5c" )]
    [CustomizedGrid( CustomColumnMessage = "Custom columns can access the prayer request through the 'Row' merge field (for example, {{ Row.RequestedByPersonAlias.Person.FullName }} for the requester or {{ Row.Text }} for the request). The request and its requester are pre-loaded, so they resolve without additional queries; referencing other related entities (such as Row.Category or Row.Campus) will run a database query for each row." )]
    public class PrayerCommentList : RockListBlockType<PrayerCommentList.PrayerCommentData>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string PrayerRequestCategory = "PrayerRequestCategory";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PreferenceKey
        {
            public const string FilterDateRange = "filter-date-range";
            public const string FilterCategory = "filter-category";
        }

        #endregion Keys

        #region Properties

        private SlidingDateRangeBag FilterDateRange => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterDateRange )
            .ToSlidingDateRangeBagOrNull();

        protected ListItemBag FilterCategory => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterCategory )
            .FromJsonOrNull<ListItemBag>();

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<PrayerCommentListOptionsBag>();
            var builder = GetGridBuilder();

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
        private PrayerCommentListOptionsBag GetBoxOptions()
        {
            var options = new PrayerCommentListOptionsBag();
            options.IsCategoryFilterVisible = !GetAttributeValue( AttributeKey.PrayerRequestCategory ).AsGuidOrNull().HasValue;
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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "NoteId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<PrayerCommentData> GetListQueryable( RockContext rockContext )
        {
            var prayerCommentQry = GetCommentDataQueryable( rockContext );
            var prayerRequestIdsQry = prayerCommentQry.Select( a => a.EntityId );

            /*
                6/24/26 - MSE

                The requester (RequestedByPersonAlias.Person) is eager-loaded here, within the
                single set-based query that already loads the related prayer requests, so that a
                custom Lava column referencing {{ Row.RequestedByPersonAlias.Person.FullName }}
                resolves from memory instead of lazy-loading a person for every row.

                Reason: Prevent an N+1 query when a custom column displays the requester.
                https://github.com/SparkDevNetwork/Rock/issues/6896
            */
            var prayerRequests = new PrayerRequestService( rockContext ).Queryable()
                .Include( a => a.RequestedByPersonAlias.Person )
                .Where( a => prayerRequestIdsQry.Contains( a.Id ) )
                .ToList();

            // Build a lookup so each comment can be matched to its prayer request
            // in a single pass instead of scanning the full list once per row.
            var prayerRequestsById = prayerRequests.ToDictionary( a => a.Id );

            var prayerComments = prayerCommentQry.AsEnumerable()
               .Select( b =>
               {
                   PrayerRequest prayerRequest = null;

                   if ( b.EntityId.HasValue )
                   {
                       prayerRequestsById.TryGetValue( b.EntityId.Value, out prayerRequest );
                   }

                   return new PrayerCommentData
                   {
                       IdKey = b.IdKey,
                       CreatedDateTime = b.CreatedDateTime,
                       CreatedBy = b.CreatedByPersonAlias != null ? b.CreatedByPersonAlias.Person : null,
                       IsSystem = b.IsSystem,
                       Text = b.Text,
                       PrayerRequest = prayerRequest
                   };
               } );

            return prayerComments.AsQueryable();
        }

        private IQueryable<Note> GetCommentDataQueryable( RockContext rockContext )
        {
            // Filter by Category.  First see if there is a Block Setting, otherwise use the Grid Filter
            CategoryCache categoryFilter = null;
            var blockCategoryGuid = GetAttributeValue( AttributeKey.PrayerRequestCategory ).AsGuidOrNull();
            if ( blockCategoryGuid.HasValue )
            {
                categoryFilter = CategoryCache.Get( blockCategoryGuid.Value );
            }

            if ( categoryFilter == null && FilterCategory != null && Guid.TryParse( FilterCategory.Value, out var categoryGuid ) )
            {
                categoryFilter = CategoryCache.Get( categoryGuid );
            }

            var noteTypeService = new NoteTypeService( rockContext );
            var noteType = noteTypeService.Get( Rock.SystemGuid.NoteType.PRAYER_COMMENT.AsGuid() );

            var noteTypeId = noteType?.Id ?? 0;

            // Get the prayer comments that meet any one of the conditions below:
            //  1. Note is approved
            //  2. NoteType doesn't require approvals
            //  3. Note was created by the viewer.
            var qry = new NoteService( rockContext )
                .GetByNoteTypeId( noteTypeId )
                .AreViewableBy( GetCurrentPerson()?.Id );

            if ( categoryFilter != null )
            {
                // If filtered by category, only show comments for prayer requests in that
                // category or any of its descendant categories. Match on CategoryId using
                // the cached category's Id to avoid a Guid-based join to the Category table.
                var categoryIds = new CategoryService( rockContext )
                    .GetAllDescendents( categoryFilter.Guid )
                    .Select( a => a.Id )
                    .ToList();
                categoryIds.Add( categoryFilter.Id );

                var prayerRequestQry = new PrayerRequestService( rockContext ).Queryable()
                    .Where( a => a.CategoryId.HasValue && categoryIds.Contains( a.CategoryId.Value ) )
                    .Select( a => a.Id );

                qry = qry.Where( a => a.EntityId.HasValue && prayerRequestQry.Contains( a.EntityId.Value ) );
            }


            // Default to the last 3 months so the grid never materializes an unbounded
            // number of rows, since comments accumulate over time. The individual can
            // widen the range from the grid settings.
            var defaultDateRange = new SlidingDateRangeBag
            {
                RangeType = SlidingDateRangeType.Last,
                TimeUnit = TimeUnitType.Month,
                TimeValue = 3
            };

            var dateRange = FilterDateRange.Validate( defaultDateRange ).ActualDateRange;

            // Filter by Date Range
            if ( dateRange.Start.HasValue )
            {
                qry = qry.Where( r => r.CreatedDateTime.HasValue && r.CreatedDateTime >= dateRange.Start.Value );
            }

            if ( dateRange.End.HasValue )
            {
                qry = qry.Where( r => r.CreatedDateTime.HasValue && r.CreatedDateTime < dateRange.End.Value );
            }

            return qry;
        }

        /// <inheritdoc/>
        protected override GridBuilder<PrayerCommentData> GetGridBuilder()
        {
            var gridOptions = new GridBuilderGridOptions<PrayerCommentData>
            {
                /*
                    6/24/26 - MSE

                    The prayer request is exposed as the "Row" merge object for custom Lava
                    columns. Because the request entities (and the requester person) are loaded
                    set-based in GetListQueryable, scalar fields and
                    {{ Row.RequestedByPersonAlias.Person.FullName }} resolve without an
                    additional database call per row. Navigating any other relationship (for
                    example Row.Category or Row.Campus) is not pre-loaded and will run a query
                    per row.

                    Reason: https://github.com/SparkDevNetwork/Rock/issues/6896
                */
                LavaObject = row => row.PrayerRequest
            };

            return new GridBuilder<PrayerCommentData>()
                .WithBlock( this, gridOptions )
                .AddTextField( "idKey", a => a.IdKey )
                .AddPersonField( "createdBy", a => a.CreatedBy )
                .AddTextField( "text", a => a.Text )
                .AddDateTimeField( "time", a => a.CreatedDateTime )
                .AddField( "isSystem", a => a.IsSystem )
                .AddField( "prayerRequestIdKey", a => a.PrayerRequest?.IdKey );

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
            var entityService = new NoteService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{Note.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {Note.FriendlyTypeName}." );
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

        #region Support Classes

        /// <summary>
        /// The temporary data format to use when building the results for the
        /// grid.
        /// </summary>
        public class PrayerCommentData
        {
            /// <summary>
            /// Gets or sets the Id Key.
            /// </summary>
            /// <value>
            /// The Id Key.
            /// </value>
            public string IdKey { get; set; }

            /// <summary>
            /// Gets or sets the prayer request that this comment is attached to.
            /// This is exposed to custom Lava columns as the "Row" merge field so
            /// that administrators can display request details (e.g. the requester
            /// name) without an additional database call per row.
            /// </summary>
            /// <value>
            /// The <see cref="Rock.Model.PrayerRequest"/> the comment is attached to.
            /// </value>
            public PrayerRequest PrayerRequest { get; set; }

            /// <summary>
            /// Gets or sets the Created By.
            /// </summary>
            /// <value>
            /// The Created By.
            /// </value>
            public Person CreatedBy { get; set; }

            /// <summary>
            /// Gets or sets the text.
            /// </summary>
            /// <value>
            /// The text.
            /// </value>
            public string Text { get; set; }

            /// <summary>
            /// Gets or sets the created date and time.
            /// </summary>
            /// <value>
            /// The created date and time.
            /// </value>
            public DateTime? CreatedDateTime { get; set; }

            /// <summary>
            /// Gets or sets a flag indicating if this note is part of the Rock core system/framework.
            /// </summary>
            /// <value>
            /// A <see cref="System.Boolean"/> value that is <c>true</c> if this note is part of the Rock core system/framework; otherwise <c>false</c>.
            /// </value>
            public bool IsSystem { get; set; }
        }

        #endregion
    }
}

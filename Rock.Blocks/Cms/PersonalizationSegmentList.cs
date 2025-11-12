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
using Rock.ViewModels.Blocks.Cms.PersonalizationSegmentList;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a list of personalization segments.
    /// </summary>

    [DisplayName( "Personalization Segment List" )]
    [Category( "CMS" )]
    [Description( "Displays a list of personalization segments." )]
    [IconCssClass( "ti ti-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the personalization segment details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "18cdd594-a0e4-4190-86f5-0f7fa0b0cedc" )]
    [Rock.SystemGuid.BlockTypeGuid( "4d65b168-9fba-4dff-9442-6754bc4afa48" )]
    [CustomizedGrid]
    public class PersonalizationSegmentList : RockListBlockType<PersonalizationSegmentList.PersonalizationSegmentListBag>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
            public const string SegmentPage = "SegmentPage";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// The PersonalizationSegment attributes configured to show on the grid.
        /// </summary>
        private readonly Lazy<List<AttributeCache>> _gridAttributes = new System.Lazy<List<AttributeCache>>( BuildGridAttributes );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<PersonalizationSegmentListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );
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
        private PersonalizationSegmentListOptionsBag GetBoxOptions()
        {
            var options = new PersonalizationSegmentListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var entity = new PersonalizationSegment();

            return entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, new Dictionary<string, string> { ["PersonalizationSegmentId"] = "((Key))", ["autoEdit"] = "true", ["returnUrl"] = this.GetCurrentPageUrl() } ),
                [NavigationUrlKey.SegmentPage] = RequestContext.ResolveRockUrl( "~/admin/cms/personalization-segments/((guid))" ),
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<PersonalizationSegmentListBag> GetListQueryable( RockContext rockContext )
        {
            var personalizationSegmentService = new PersonalizationSegmentService( rockContext );
            var personAliasPersonalizationsSegmentsQry = personalizationSegmentService.GetPersonAliasPersonalizationSegmentQuery();
            var anonymousVisitorPersonId = new PersonService( rockContext ).GetOrCreateAnonymousVisitorPersonId();

            var personalizationSegmentList = personalizationSegmentService.Queryable()
                .AsNoTracking()
                .Include( s => s.FilterDataView )
                .Include( s => s.Categories );

            var scheduleIds = personalizationSegmentList
                .Where( ps => ps.PersistedScheduleId.HasValue )
                .Select( ps => ps.PersistedScheduleId.Value )
                .Distinct()
                .ToList();

            var schedules = new ScheduleService( rockContext )
                .GetByIds( scheduleIds )
                .ToDictionary( s => s.Id, s => s );

            // Server-side projection only (EF-translatable).
            var sqlProjection = personalizationSegmentList
                .Select( a => new
            {
                PersonalizationSegment = a,
                KnownIndividualsCount = a.IsDirty ? -1 : personAliasPersonalizationsSegmentsQry.Where( p => p.PersonalizationEntityId == a.Id && p.PersonAlias.PersonId != anonymousVisitorPersonId ).Count(),
                AnonymousIndividualsCount = a.IsDirty ? -1 : personAliasPersonalizationsSegmentsQry.Where( p => p.PersonalizationEntityId == a.Id && p.PersonAlias.PersonId == anonymousVisitorPersonId ).Count(),
                TimeToUpdateDurationMilliseconds = a.TimeToUpdateDurationMilliseconds ?? 0,
                Categories = a.Categories.Select( c => c.Name ).ToList(),
                FilterDataViewName = a.FilterDataViewId.HasValue ? a.FilterDataView.Name : null,
                PersistedScheduleIntervalMinutes = a.PersistedScheduleIntervalMinutes,
                PersistedScheduleId = a.PersistedScheduleId,
            } );

            var currentPerson = GetCurrentPerson();

            // Client-side only to format the PersistedScheduleInfo string (not SQL-translatable).
            var personalizationSegmentItemQuery = sqlProjection
            .AsEnumerable()
            .Where( a => a.PersonalizationSegment.IsAuthorized( Authorization.VIEW, currentPerson ) )
            .Select( a => new PersonalizationSegmentListBag
            {
                PersonalizationSegment = a.PersonalizationSegment,
                KnownIndividualsCount = a.KnownIndividualsCount,
                AnonymousIndividualsCount = a.AnonymousIndividualsCount,
                TimeToUpdateDurationMilliseconds = a.TimeToUpdateDurationMilliseconds,
                Categories = a.Categories,
                FilterDataViewName = a.FilterDataViewName,
                PersistedScheduleInfo = a.PersistedScheduleId.HasValue
                        ? schedules[a.PersistedScheduleId.Value].FriendlyScheduleText
                        : a.PersistedScheduleIntervalMinutes.HasValue
                            ? $"Every {new Rock.Utility.TimeIntervalSetting( a.PersistedScheduleIntervalMinutes, null )}"
                            : "Not Persisted"
            } );

            return personalizationSegmentItemQuery.AsQueryable();
        }

        /// <inheritdoc/>
        protected override IQueryable<PersonalizationSegmentListBag> GetOrderedListQueryable( IQueryable<PersonalizationSegmentListBag> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( a => a.PersonalizationSegment.Name );
        }

        /// <inheritdoc/>
        protected override List<PersonalizationSegmentListBag> GetListItems( IQueryable<PersonalizationSegmentListBag> queryable, RockContext rockContext )
        {
            var items = queryable.ToList();

            GridAttributeLoader.LoadFor( items, a => a.PersonalizationSegment, _gridAttributes.Value, rockContext );

            return items;
        }

        /// <inheritdoc/>
        protected override GridBuilder<PersonalizationSegmentListBag> GetGridBuilder()
        {
            return new GridBuilder<PersonalizationSegmentListBag>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.PersonalizationSegment.IdKey )
                .AddField( "guid", a => a.PersonalizationSegment.Guid )
                .AddField( "isSecurityDisabled", a => !a.PersonalizationSegment.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                .AddTextField( "name", a => a.PersonalizationSegment.Name )
                .AddTextField( "description", a => a.PersonalizationSegment.Description )
                .AddTextField( "filterDataViewName", a => a.FilterDataViewName )
                .AddTextField( "persistedScheduleInfo", a => a.PersistedScheduleInfo )
                .AddField( "knownIndividualsCount", a => a.KnownIndividualsCount )
                .AddField( "anonymousIndividualsCount", a => a.AnonymousIndividualsCount )
                .AddField( "timeToUpdateDurationMilliseconds", a => a.TimeToUpdateDurationMilliseconds.HasValue ? Math.Round( ( double ) a.TimeToUpdateDurationMilliseconds ) : a.TimeToUpdateDurationMilliseconds )
                .AddField( "categories", a => a.Categories )
                .AddField( "isActive", a => a.PersonalizationSegment.IsActive )
                .AddAttributeFieldsFrom( a => a.PersonalizationSegment, _gridAttributes.Value );
        }

        /// <summary>
        /// Builds the list of grid attributes that should be included on the Grid.
        /// </summary>
        /// <returns>A list of <see cref="AttributeCache"/> objects.</returns>
        private static List<AttributeCache> BuildGridAttributes()
        {
            var entityTypeId = EntityTypeCache.Get<PersonalizationSegment>( false )?.Id;

            if ( entityTypeId.HasValue )
            {
                return AttributeCache.GetOrderedGridAttributes( entityTypeId.Value, string.Empty, string.Empty );
            }

            return new List<AttributeCache>();
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
            var entityService = new PersonalizationSegmentService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{PersonalizationSegment.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {PersonalizationSegment.FriendlyTypeName}." );
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

        #region Helper Classes

        /// <summary>
        /// The data displayed in the Personal Link Section List block.
        /// </summary>
        public class PersonalizationSegmentListBag
        {
            public PersonalizationSegment PersonalizationSegment { get; set; }

            public string FilterDataViewName { get; set; }

            public int? KnownIndividualsCount { get; set; }

            public int? AnonymousIndividualsCount { get; set; }

            public double? TimeToUpdateDurationMilliseconds { get; set; }

            public List<string> Categories { get; set; }

            public string PersistedScheduleInfo { get; set; }
        }

        #endregion
    }
}

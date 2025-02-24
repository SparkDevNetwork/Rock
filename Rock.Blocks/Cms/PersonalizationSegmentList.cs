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
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

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

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<PersonalizationSegmentListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = true;
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
            return GetGridDataSourceList( rockContext );
        }

        protected override IQueryable<PersonalizationSegmentListBag> GetOrderedListQueryable( IQueryable<PersonalizationSegmentListBag> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( a => a.Name );
        }

        /// <inheritdoc/>
        protected override GridBuilder<PersonalizationSegmentListBag> GetGridBuilder()
        {
            return new GridBuilder<PersonalizationSegmentListBag>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddField( "guid", a => a.Guid )
                .AddField( "isSecurityDisabled", a => a.IsSecurityDisabled )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "description", a => a.Description )
                .AddTextField( "filterDataViewName", a => a.FilterDataViewName )
                .AddTextField( "knownIndividualsCount", a => a.KnownIndividualsCount )
                .AddTextField( "anonymousIndividualsCount", a => a.AnonymousIndividualsCount )
                .AddField( "timeToUpdateDurationMilliseconds", a => a.TimeToUpdateDurationMilliseconds )
                .AddField( "categories", a => a.Categories )
                .AddField( "isActive", a => a.IsActive );
        }

        /// <summary>
        /// Gets the grid data source list (ordered)
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private IQueryable<PersonalizationSegmentListBag> GetGridDataSourceList( RockContext rockContext )
        {
            var personalizationSegmentService = new PersonalizationSegmentService( rockContext );
            var personAliasPersonalizationsSegmentsQry = personalizationSegmentService.GetPersonAliasPersonalizationSegmentQuery();
            var anonymousVisitorPersonId = new PersonService( rockContext ).GetOrCreateAnonymousVisitorPersonId();

            var knownIndividualCounts = personAliasPersonalizationsSegmentsQry
                .Where( p => p.PersonAlias.PersonId != anonymousVisitorPersonId )
                .GroupBy( p => p.PersonalizationEntityId )
                .ToDictionary( grp => grp.Key, grp => grp.Count() );

            var anonymousIndividualCounts = personAliasPersonalizationsSegmentsQry
                .Where( p => p.PersonAlias.PersonId == anonymousVisitorPersonId )
                .GroupBy( p => p.PersonalizationEntityId )
                .ToDictionary( grp => grp.Key, grp => grp.Count() );

            var personalizationSegmentList = personalizationSegmentService.Queryable().ToList();

            foreach ( var personalizationSegment in personalizationSegmentList )
            {
                if ( personalizationSegment.TimeToUpdateDurationMilliseconds.HasValue )
                {
                    // Round values to a single digit.
                    personalizationSegment.TimeToUpdateDurationMilliseconds = Math.Round( personalizationSegment.TimeToUpdateDurationMilliseconds.Value, 1 );
                }
            }

            var personalizationSegmentItemQuery = personalizationSegmentList.Select( a => new PersonalizationSegmentListBag
            {
                IdKey = a.IdKey,
                Guid = a.Guid,
                Name = a.Name,
                Description = a.Description,
                FilterDataViewName = a.FilterDataViewId.HasValue ? a.FilterDataView.Name : null,
                IsActive = a.IsActive,
                KnownIndividualsCount = a.IsDirty ? "Loading..." : knownIndividualCounts.ContainsKey( a.Id ) ? knownIndividualCounts[a.Id].ToString() : "0",
                AnonymousIndividualsCount = a.IsDirty ? "Loading..." : anonymousIndividualCounts.ContainsKey( a.Id ) ? anonymousIndividualCounts[a.Id].ToString() : "0",
                TimeToUpdateDurationMilliseconds = a.TimeToUpdateDurationMilliseconds,
                CanDelete = a.IsAuthorized( Authorization.DELETE, this.GetCurrentPerson() ),
                CanEdit = a.IsAuthorized( Authorization.EDIT, this.GetCurrentPerson() ),
                IsSecurityDisabled = !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ),
                Categories = a.Categories.Select( c => c.Name ).ToList(),
            } );

            return personalizationSegmentItemQuery.AsQueryable();
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
            public string IdKey { get; set; }

            public Guid Guid { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public string FilterDataViewName { get; set; }

            public bool IsActive { get; set; }

            public string KnownIndividualsCount { get; set; }

            public string AnonymousIndividualsCount { get; set; }

            public double? TimeToUpdateDurationMilliseconds { get; set; }

            public bool CanDelete { get; set; }

            public bool CanEdit { get; set; }

            public bool IsSecurityDisabled { get; set; }

            public List<string> Categories { get; set; }
        }

        #endregion
    }
}

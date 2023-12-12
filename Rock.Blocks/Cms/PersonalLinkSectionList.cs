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
using Rock.ViewModels.Blocks.Cms.PersonalLinkSectionList;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a list of personal link sections.
    /// </summary>
    [DisplayName( "Personal Link Section List" )]
    [Category( "CMS" )]
    [Description( "Lists personal link section in the system." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the personal link section details.",
        Key = AttributeKey.DetailPage,
        Order = 0 )]

    [BooleanField(
        "Shared Sections",
        Description = "When enabled, only shared sections will be displayed.",
        Key = AttributeKey.SharedSections,
        Order = 1 )]

    [Rock.SystemGuid.EntityTypeGuid( "55429a67-e6c6-42fe-813b-3ea67a575eb0" )]
    [Rock.SystemGuid.BlockTypeGuid( "904db731-4a40-494c-b52c-95cf0f54c21f" )]
    [CustomizedGrid]
    public class PersonalLinkSectionList : RockListBlockType<PersonalLinkSectionList.PersonalLinkSectionListBag>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string SharedSections = "SharedSection";
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
            var box = new ListBlockBox<PersonalLinkSectionListOptionsBag>();
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
        private PersonalLinkSectionListOptionsBag GetBoxOptions()
        {
            var options = new PersonalLinkSectionListOptionsBag()
            {
                IsSharedSections = GetAttributeValue( AttributeKey.SharedSections ).AsBoolean()
            };

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var entity = new PersonalLinkSection();

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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "SectionId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<PersonalLinkSectionListBag> GetListQueryable( RockContext rockContext )
        {
            return GetGridDataSourceList( rockContext );
        }

        /// <inheritdoc/>
        protected override IQueryable<PersonalLinkSectionListBag> GetOrderedListQueryable( IQueryable<PersonalLinkSectionListBag> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( a => a.PersonalLinkSectionOrder == null ? 0 : a.PersonalLinkSectionOrder.Order )
                .ThenBy( a => a.Name );
        }

        /// <inheritdoc/>
        protected override GridBuilder<PersonalLinkSectionListBag> GetGridBuilder()
        {
            return new GridBuilder<PersonalLinkSectionListBag>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "linkCount", a => a.LinkCount.ToString() )
                .AddField( "isShared", a => a.IsShared );
        }

        /// <summary>
        /// Gets the grid data source list (ordered)
        /// </summary>
        /// <param name="queryable">The queryable.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private IQueryable<PersonalLinkSectionListBag> GetGridDataSourceList( RockContext rockContext )
        {
            var limitToSharedSections = GetAttributeValue( AttributeKey.SharedSections ).AsBoolean();
            List<PersonalLinkSection> personalLinkSectionList;
            Dictionary<int, PersonalLinkSectionOrder> currentPersonSectionOrderLookupBySectionId = null;

            if ( limitToSharedSections )
            {
                // only show shared sections in this mode
                var sharedPersonalLinkSectionsQuery = new PersonalLinkSectionService( rockContext ).Queryable().Where( a => a.IsShared );
                personalLinkSectionList = sharedPersonalLinkSectionsQuery.Include( a => a.PersonalLinks ).OrderBy( a => a.Name ).AsNoTracking().ToList();
            }
            else
            {
                // show both shared and non-shared, but don't let shared sections get deleted (even if authorized)
                var personalLinkService = new PersonalLinkService( rockContext );
                if ( personalLinkService.AddMissingPersonalLinkSectionOrders( this.GetCurrentPerson() ) )
                {
                    rockContext.SaveChanges();
                }

                var orderedPersonalLinkSectionsQuery = new PersonalLinkService( rockContext ).GetOrderedPersonalLinkSectionsQuery( this.GetCurrentPerson() );

                personalLinkSectionList = orderedPersonalLinkSectionsQuery
                    .Include( a => a.PersonalLinks )
                    .AsNoTracking()
                    .AsEnumerable()
                    .Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, this.GetCurrentPerson() ) )
                    .ToList();

                // NOTE: We might be making changes when resorting this, so don't use AsNoTracking()
                var sectionOrderQuery = personalLinkService.GetSectionOrderQuery( this.GetCurrentPerson() );
                currentPersonSectionOrderLookupBySectionId = sectionOrderQuery.ToDictionary( k => k.SectionId, v => v );
            }

            var viewModelList = personalLinkSectionList.ConvertAll( a =>
            {
                var personalLinkSectionViewModel = new PersonalLinkSectionListBag
                {
                    IdKey = a.IdKey,
                    Guid = a.Guid,
                    Name = a.Name,
                    LinkCount = a.PersonalLinks.Count( x => x.IsAuthorized( Rock.Security.Authorization.VIEW, this.GetCurrentPerson() ) ),
                    IsShared = a.IsShared,
                    PersonalLinkSectionOrder = currentPersonSectionOrderLookupBySectionId?.GetValueOrNull( a.Id )
                };

                if ( limitToSharedSections )
                {
                    // if we are only showing shared sections, let them edit it if authorized edit
                    personalLinkSectionViewModel.CanEdit = a.IsAuthorized( Authorization.EDIT, this.GetCurrentPerson() );
                }
                else
                {
                    // Don't allow shared sections to be deleted/edited if we showing both shared and non-shared sections
                    personalLinkSectionViewModel.CanEdit = !a.IsShared;
                }

                personalLinkSectionViewModel.CanDelete = personalLinkSectionViewModel.CanEdit;

                return personalLinkSectionViewModel;
            } );

            return viewModelList.AsQueryable();
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
                var entityService = new PersonalLinkSectionService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{PersonalLinkSection.FriendlyTypeName} not found." );
                }

                if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${PersonalLinkSection.FriendlyTypeName}." );
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
        /// Changes the ordered position of a single item.
        /// </summary>
        /// <param name="key">The unique identifier of the item that will be moved.</param>
        /// <param name="beforeKey">The unique identifier of the item it will be placed before.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult ReorderItem( string key, string beforeKey )
        {
            using ( var rockContext = new RockContext() )
            {
                var qry = GetGridDataSourceList( rockContext );
                qry = GetOrderedListQueryable( qry, rockContext );

                // Get the PersonalLinkSectionOrder entities which are used to order the PersonalLinkSections.
                var sectionOrderList = qry.Select( a => a.PersonalLinkSectionOrder ).ToList();

                var entity = sectionOrderList.Find( a => a?.Section?.IdKey == key );
                var beforeEntity = sectionOrderList.Find( a => a?.Section?.IdKey == beforeKey );

                if ( !sectionOrderList.ReorderEntity( entity?.Guid.ToStringSafe(), beforeEntity?.Guid.ToStringSafe() ) )
                {
                    return ActionBadRequest( "Invalid reorder attempt." );
                }

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// The data displayed in the Personal Link Section List block.
        /// </summary>
        public class PersonalLinkSectionListBag
        {
            public string IdKey { get; set; }

            public Guid Guid { get; set; }

            public string Name { get; set; }

            public bool IsShared { get; set; }

            public int LinkCount { get; set; }

            public PersonalLinkSectionOrder PersonalLinkSectionOrder { get; set; }

            public bool CanDelete { get; set; }

            public bool CanEdit { get; set; }
        }

        #endregion
    }
}

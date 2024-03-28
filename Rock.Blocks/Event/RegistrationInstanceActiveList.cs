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
using Rock.UniversalSearch.IndexModels;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Event.RegistrationInstanceActiveList;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Displays a list of active registration instances.
    /// </summary>

    [DisplayName( "Registration Instance Active List" )]
    [Category( "Event" )]
    [Description( "Displays a list of registration instances." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the registration instance details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "3951453c-e9fc-4f43-8b7b-794c5acfcabe" )]
    [Rock.SystemGuid.BlockTypeGuid( "5e899ccb-3c24-4f7d-9843-2f1cb00aed8f" )]
    [CustomizedGrid]
    public class RegistrationInstanceActiveList : RockEntityListBlockType<RegistrationInstance>
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

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<RegistrationInstanceActiveListOptionsBag>();
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
        private RegistrationInstanceActiveListOptionsBag GetBoxOptions()
        {
            var options = new RegistrationInstanceActiveListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var entity = new RegistrationInstance();

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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "RegistrationInstanceId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<RegistrationInstance> GetListQueryable( RockContext rockContext )
        {
            var qry = new RegistrationInstanceService( rockContext )
                    .Queryable()
                    .Where( i =>
                        ( i.StartDateTime <= RockDateTime.Now || !i.StartDateTime.HasValue ) &&
                        ( i.EndDateTime > RockDateTime.Now || !i.EndDateTime.HasValue ) &&
                        i.IsActive );

            return qry;
        }

        /// <inheritdoc/>
        protected override IQueryable<RegistrationInstance> GetOrderedListQueryable( IQueryable<RegistrationInstance> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( i => i.StartDateTime );
        }

        /// <inheritdoc/>
        protected override List<RegistrationInstance> GetListItems( IQueryable<RegistrationInstance> queryable, RockContext rockContext )
        {
            var listItems = base.GetListItems( queryable, rockContext );

            var securedAttendanceItems = listItems
                .AsEnumerable()
                .Where( g => g.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson ) )
                .ToList();
            
            return listItems;
        }

        /// <inheritdoc/>
        protected override GridBuilder<RegistrationInstance> GetGridBuilder()
        {
            return new GridBuilder<RegistrationInstance>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddDateTimeField( "startDateTime", a => a.StartDateTime )
                .AddDateTimeField( "endDateTime", a => a.EndDateTime )
                .AddTextField( "details", a => a.Details )
                .AddField( "registrantsCount", a => a.Registrations.Where( r => !r.IsTemporary ).SelectMany( r => r.Registrants ).Count() )
                .AddField( "isActive", a => a.IsActive )
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
                var entityService = new RegistrationInstanceService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{RegistrationInstance.FriendlyTypeName} not found." );
                }

                if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${RegistrationInstance.FriendlyTypeName}." );
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

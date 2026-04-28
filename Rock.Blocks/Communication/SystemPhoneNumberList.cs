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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Communication.SystemPhoneNumberList;
using Rock.Web.Cache;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Displays a list of system phone numbers.
    /// </summary>

    [DisplayName( "System Phone Number List" )]
    [Category( "Communication" )]
    [Description( "Displays a list of system phone numbers." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the system phone number details.",
        Key = AttributeKey.SystemPhoneNumberDetailPage )]

    [SystemGuid.EntityTypeGuid( "060AC1B7-86A9-4C47-B07A-C140D1E946A3" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "86C70BF7-6BB5-46A4-9925-94AC4FB511D7" )]
    [Rock.SystemGuid.BlockTypeGuid( "72c74d98-d80f-4eee-bd14-6308ea565d7a" )] // the real BlockTypeGuid for SystemPhoneNumberList.ascx.cs
    [CustomizedGrid]
    public class SystemPhoneNumberList : RockEntityListBlockType<SystemPhoneNumber>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string SystemPhoneNumberDetailPage = "SystemPhoneNumberDetailPage";
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
            var box = new ListBlockBox<SystemPhoneNumberListOptionsBag>();
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
        private SystemPhoneNumberListOptionsBag GetBoxOptions()
        {
            var options = new SystemPhoneNumberListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// </summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var entity = new SystemPhoneNumber();

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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.SystemPhoneNumberDetailPage, "SystemPhoneNumberId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<SystemPhoneNumber> GetListQueryable( RockContext rockContext )
        {
            return base.GetListQueryable( rockContext );
        }

        /// <inheritdoc/>
        protected override IQueryable<SystemPhoneNumber> GetOrderedListQueryable( IQueryable<SystemPhoneNumber> queryable, RockContext rockContext )
        {
            return queryable
                .OrderBy( spn => spn.Order )
                .ThenBy( spn => spn.Name )
                .ThenBy( spn => spn.Id );
        }

        /// <inheritdoc/>
        protected override GridBuilder<SystemPhoneNumber> GetGridBuilder()
        {
            return new GridBuilder<SystemPhoneNumber>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "number", a => a.Number )
                .AddField( "isActive", a => a.IsActive )
                .AddField( "isSmsEnabled", a => a.IsSmsEnabled )
                .AddPersonField( "assignedToPersonAlias", a => a.AssignedToPersonAlias?.Person )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) );
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
            var entityService = new SystemPhoneNumberService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{SystemPhoneNumber.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {SystemPhoneNumber.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Reorders the items in the list.
        /// </summary>
        /// <param name="key">The identifier of the item to be moved.</param>
        /// <param name="beforeKey">The identifier of the item it should be placed before.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult ReorderItem( string key, string beforeKey )
        {
            var qry = GetListQueryable( RockContext );
            qry = GetOrderedListQueryable( qry, RockContext );
            var items = GetListItems( qry, RockContext );

            if ( !items.ReorderEntity( key, beforeKey ) )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            RockContext.SaveChanges();
            return ActionOk();
        }

        #endregion
    }
}

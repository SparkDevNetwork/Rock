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
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.RestControllerList;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of rest controllers.
    /// </summary>

    [DisplayName( "Rest Controller List" )]
    [Category( "Core" )]
    [Description( "Displays a list of rest controllers." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the rest controller details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "98008055-f00f-4f6c-ba1d-2414d6dff7aa" )]
    [Rock.SystemGuid.BlockTypeGuid( "a6d8bfd9-0c3d-4f1e-ae0d-325a9c70b4c8" )]
    [CustomizedGrid]
    public class RestControllerList : RockEntityListBlockType<RestController>
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
            var box = new ListBlockBox<RestControllerListOptionsBag>();
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
        private RestControllerListOptionsBag GetBoxOptions()
        {
            var options = new RestControllerListOptionsBag();

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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "Controller", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<RestController> GetListQueryable( RockContext rockContext )
        {
            var service = new RestControllerService( rockContext );

            var qry = service.Queryable().OrderBy( c => c.Name ).AsNoTracking();

            return qry;
        }

        /// <inheritdoc/>
        protected override GridBuilder<RestController> GetGridBuilder()
        {
            return new GridBuilder<RestController>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "className", a => a.ClassName )
                .AddField( "actions", a => a.Actions.Count() )
                .AddField( "actionsWithPublicCachingHeaders", a => a.Actions.Count( x => x.CacheControlHeaderSettings != null
                                                                                        && x.CacheControlHeaderSettings != ""
                                                                                        && x.CacheControlHeaderSettings.Contains( "\"RockCacheablityType\":0" ) ) )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                .AddAttributeFields( GetGridAttributes() );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Refreshes the list of REST controllers.
        /// </summary>
        [BlockAction]
        public BlockActionResult RefreshControllerList()
        {
            RestControllerService.RegisterControllers();

            return ActionOk();
        }

        #endregion
    }
}

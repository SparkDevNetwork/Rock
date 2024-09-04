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
using System.Reflection;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.RestActionList;
using Rock.ViewModels.Controls;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of rest actions.
    /// </summary>

    [DisplayName( "Rest Action List" )]
    [Category( "Core" )]
    [Description( "Displays a list of rest actions." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the rest action details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "c8ee0e9b-7f66-488c-b3a6-357ebc62b174" )]
    [Rock.SystemGuid.BlockTypeGuid( "2eafa987-79c6-4477-a181-63392aa24d20" )]
    [CustomizedGrid]
    public class RestActionList : RockEntityListBlockType<RestAction>
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

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<RestActionListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private RestActionListOptionsBag GetBoxOptions()
        {
            var options = new RestActionListOptionsBag();
            int controllerId = PageParameter( "Controller" ).AsInteger();
            var controller = new RestControllerService( new RockContext() ).Get( controllerId );

            if ( controller != null )
            {
                options.ControllerName = controller.Name.SplitCase();
                var controllerType = Reflection.FindTypes( typeof( Rock.Rest.ApiControllerBase ) )
                    .Where( a => a.Key.Equals( controller.ClassName ) ).Select( a => a.Value ).FirstOrDefault();

                if ( controllerType != null )
                {
                    var obsoleteAttribute = controllerType.GetCustomAttribute<System.ObsoleteAttribute>();
                    if ( obsoleteAttribute != null )
                    {
                        options.ObsoleteWarning = $"Obsolete: {obsoleteAttribute.Message}";
                    }
                }
            }

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

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <inheritdoc/>
        protected override IQueryable<RestAction> GetListQueryable( RockContext rockContext )
        {
            int controllerId = PageParameter( "Controller" ).AsInteger();

            var query = new RestActionService( rockContext ).Queryable()
                .Where( a => a.ControllerId == controllerId )
                .OrderBy( a => a.Method )
                .AsNoTracking();

            return query.ToList().AsQueryable();
        }

        /// <inheritdoc/>
        protected override GridBuilder<RestAction> GetGridBuilder()
        {
            return new GridBuilder<RestAction>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "method", a => a.Method )
                .AddField( "path", a => a.Path.EndsWith( "?key={key}" ) ? a.Path.Replace( "?key={key}", "(id)" ) : a.Path )
                .AddField( "hasCacheHeader", a =>
                    a.CacheControlHeaderSettings != null &&
                    a.CacheControlHeaderSettings.FromJsonOrNull<Rock.Utility.RockCacheability>()?.RockCacheablityType == Rock.Utility.RockCacheablityType.Public )
                .AddField( "cacheHeaderTooltip", a =>
                    a.CacheControlHeaderSettings?.FromJsonOrNull<Rock.Utility.RockCacheability>() )
                .AddTextField( "cacheControlHeaderSettings", a => a.CacheControlHeaderSettings )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) );
        }

        #endregion

        #region Block Actions

        [BlockAction]
        public BlockActionResult SaveActionSettings( ActionSettingsModel model )
        {
            var restAction = new RestActionService( RockContext ).Get( model.ControllerActionId );

            if ( restAction == null )
            {
                return ActionBadRequest( "REST action not found." );
            }

            var cacheability = model.CacheControlHeaderSettings.ToCacheability();

            restAction.CacheControlHeaderSettings = cacheability?.ToJson();
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion
    }

    #region Helper Classes

    public class ActionSettingsModel
    {
        public string ControllerActionId { get; set; }
        public RockCacheabilityBag CacheControlHeaderSettings { get; set; }
    }

    #endregion
}

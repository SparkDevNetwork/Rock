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
using System.Reflection;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.RestActionList;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

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

        #endregion Keys

        #region Fields

        private int _controllerId;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<RestActionListOptionsBag>();
            var builder = GetGridBuilder();

            _controllerId = PageParameter( "Controller" ).AsInteger();

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
        private RestActionListOptionsBag GetBoxOptions()
        {
            var options = new RestActionListOptionsBag();

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
        protected override IQueryable<RestAction> GetListQueryable( RockContext rockContext )
        {
            int controllerId = PageParameter( "Controller" ).AsInteger();

            return new RestActionService( rockContext ).Queryable()
                .Where( a => a.ControllerId == controllerId )
                .AsNoTracking();

        }

        protected override List<RestAction> GetListItems( IQueryable<RestAction> queryable, RockContext rockContext )
        {
            return queryable.ToList();
        }

        protected override GridBuilder<RestAction> GetGridBuilder()
        {
            return new GridBuilder<RestAction>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.Id.ToString() )
                .AddTextField( "method", a => a.Method )
                .AddField( "path", a =>
                {
                    var path = a.Path;
                    if ( path.EndsWith( "?key={key}" ) )
                    {
                        path = path.Replace( "?key={key}", "(id)" );
                    }
                    return path;
                } )
                .AddField( "hasCacheHeader", a => !string.IsNullOrEmpty( a.CacheControlHeaderSettings ) )
                .AddField( "cacheHeaderTooltip", a =>
                {
                    var cacheability = a.CacheControlHeaderSettings.FromJsonOrNull<RockCacheability>();
                    return new
                    {
                        rockCacheabilityType = cacheability?.RockCacheablityType ?? 0,
                        maxAge = cacheability?.MaxAge,
                        sharedMaxAge = cacheability?.SharedMaxAge
                    };
                } )

                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) );
        }

        #endregion

        #region Block Actions

        [BlockAction]
        public BlockActionResult SaveActionSettings( ActionSettingsModel model )
        {
            var rockContext = new RockContext();
            var restAction = GetRestAction( model.ControllerActionId, rockContext );

            if ( restAction == null )
            {
                return ActionBadRequest( "REST action not found." );
            }

            restAction.CacheControlHeaderSettings = model.CacheControlHeaderSettings;

            rockContext.SaveChanges();

            return ActionOk();
        }

        private RestAction GetRestAction( int actionId, RockContext rockContext )
        {
            return new RestActionService( rockContext ).Get( actionId );
        }

        #endregion
    }

    #region Helper Classes

    public class ActionSettingsModel
    {
        public int ControllerActionId { get; set; }
        public string CacheControlHeaderSettings { get; set; }
    }

    #endregion
}

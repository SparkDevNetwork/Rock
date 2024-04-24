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
using Rock.Achievement;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Engagement.AchievementTypeList;
using Rock.Web.Cache;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays a list of achievement types.
    /// </summary>
    [DisplayName( "Achievement Type List" )]
    [Category( "Streaks" )]
    [Description( "Shows a list of all achievement types." )]
    [IconCssClass( "fa fa-list" )]
    //[SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the achievement type details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "e9e67424-1fd8-4a85-9e7b-c919117bde1a" )]
    [Rock.SystemGuid.BlockTypeGuid( "4acfbf3f-3d49-4ae3-b468-529f79da9898" )]
    [CustomizedGrid]
    public class AchievementTypeList : RockListBlockType<AchievementTypeCache>
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
            public const string AchievementTypeId = "AchievementTypeId";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<AchievementTypeListOptionsBag>();
            var builder = GetGridBuilder();

            var isAddDeleteEnabled = GetIsAddDeleteEnabled();
            box.IsAddEnabled = isAddDeleteEnabled;
            box.IsDeleteEnabled = isAddDeleteEnabled;
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
        private AchievementTypeListOptionsBag GetBoxOptions()
        {
            var options = new AchievementTypeListOptionsBag()
            {
                IsAddVisible = !GetAttributeValue( AttributeKey.DetailPage ).IsNullOrWhiteSpace()
            };
            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddDeleteEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var queryParams = new Dictionary<string, string>();
            foreach ( var kvp in QueryParameters() )
            {
                queryParams[kvp.Key] = kvp.Value.ToString();
            }

            queryParams[PageParameterKey.AchievementTypeId] = "((Key))";

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, queryParams )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<AchievementTypeCache> GetListQueryable( RockContext rockContext )
        {
            var filters = new List<KeyValuePair<string, string>>();

            foreach ( string key in RequestContext.QueryString.Keys )
            {
                if ( key.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                var value = RequestContext.QueryString[key];
                filters.Add( new KeyValuePair<string, string>( key, value ) );
            }

            var achievementTypes = AchievementTypeCache.All()
            .Where( at => {
                if ( !filters.Any() )
                {
                    return true;
                }

                var component = at.AchievementComponent;
                return component?.IsRelevantToAllFilters( at, filters ) == true;
            } )
            .OrderBy( at => at.Id )
            .ToList();

            return achievementTypes.AsQueryable();
        }

        /// <inheritdoc/>
        protected override GridBuilder<AchievementTypeCache> GetGridBuilder()
        {
            return new GridBuilder<AchievementTypeCache>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "iconCssClass", a => a.AchievementIconCssClass )
                .AddTextField( "componentName", a => GetComponentName( a ) )
                .AddTextField( "sourceName", a => GetSourceName( a ) )
                .AddField( "isActive", a => a.IsActive );
        }

        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <returns></returns>
        private string GetComponentName( AchievementTypeCache achievementTypeCache )
        {
            return AchievementContainer.GetComponentName( achievementTypeCache.AchievementEntityType.Name );
        }

        /// <summary>
        /// Gets the name of the source.
        /// </summary>
        /// <param name="achievementTypeCache">The achievement type cache.</param>
        /// <returns></returns>
        private string GetSourceName( AchievementTypeCache achievementTypeCache )
        {
            var component = achievementTypeCache.AchievementComponent;

            if ( component != null )
            {
                return component.GetSourceName( achievementTypeCache );
            }

            return string.Empty;
        }

        public Dictionary<string, object> QueryParameters()
        {
            var parameters = new Dictionary<string, object>( StringComparer.OrdinalIgnoreCase );

            foreach ( string param in RequestContext.QueryString.Keys )
            {
                if ( param != null )
                {
                    parameters.Add( param, RequestContext.QueryString[param] );
                }
            }

            return parameters;
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
            var entityService = new AchievementTypeService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{AchievementType.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {AchievementType.FriendlyTypeName}." );
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
    }
}

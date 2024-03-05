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
using Rock.ViewModels.Blocks.Group.Scheduling.GroupMemberScheduleTemplateList;
using Rock.Web.Cache;

namespace Rock.Blocks.Group.Scheduling
{
    /// <summary>
    /// Displays a list of group member schedule templates.
    /// </summary>

    [DisplayName( "Group Member Schedule Template List" )]
    [Category( "Group Scheduling" )]
    [Description( "Lists group member schedule templates." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the group member schedule template details.",
        Key = AttributeKey.DetailPage )]

    [SystemGuid.EntityTypeGuid( "9faac3e9-01dd-4fed-af85-01817cdebf83" )]
    [SystemGuid.BlockTypeGuid( "2b8a5a3d-bf9d-4319-b7e5-06757fa44759" )]
    [CustomizedGrid]
    public class GroupMemberScheduleTemplateList : RockEntityListBlockType<GroupMemberScheduleTemplate>
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
            var box = new ListBlockBox<GroupMemberScheduleTemplateListOptionsBag>();
            var builder = GetGridBuilder();

            var getIsAddDeleteEnabled = GetIsAddDeleteEnabled();
            box.IsAddEnabled = getIsAddDeleteEnabled;
            box.IsDeleteEnabled = getIsAddDeleteEnabled;
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
        private GroupMemberScheduleTemplateListOptionsBag GetBoxOptions()
        {
            var options = new GroupMemberScheduleTemplateListOptionsBag();

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
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "GroupMemberScheduleTemplateId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<GroupMemberScheduleTemplate> GetListQueryable( RockContext rockContext )
        {
            return new GroupMemberScheduleTemplateService( rockContext ).Queryable();
        }

        /// <inheritdoc/>
        protected override IQueryable<GroupMemberScheduleTemplate> GetOrderedListQueryable( IQueryable<GroupMemberScheduleTemplate> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( g => g.Name );
        }

        /// <inheritdoc/>
        protected override GridBuilder<GroupMemberScheduleTemplate> GetGridBuilder()
        {
            return new GridBuilder<GroupMemberScheduleTemplate>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name );
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
                var entityService = new GroupMemberScheduleTemplateService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{GroupMemberScheduleTemplate.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${GroupMemberScheduleTemplate.FriendlyTypeName}." );
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

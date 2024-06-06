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
using Rock.ViewModels.Blocks.Cms.MediaAccountList;
using Rock.Web.Cache;

using static Rock.Blocks.Cms.MediaAccountList;
using static Rock.Blocks.Core.BinaryFileTypeList;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a list of media accounts.
    /// </summary>

    [DisplayName( "Media Account List" )]
    [Category( "CMS" )]
    [Description( "Displays a list of media accounts." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the media account details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "4b445e33-8ae3-4831-a5dc-88ed46d1ccea" )]
    [Rock.SystemGuid.BlockTypeGuid( "baf39b55-c4e5-4eb4-a834-b4f820dd2f42" )]
    [CustomizedGrid]
    public class MediaAccountList : RockListBlockType<MediaAccountData>
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

        /// <summary>
        /// The batch attributes that are configured to show on the grid.
        /// </summary>
        private readonly Lazy<List<AttributeCache>> _gridAttributes = new Lazy<List<AttributeCache>>( BuildGridAttributes );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<MediaAccountListOptionsBag>();
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
        private MediaAccountListOptionsBag GetBoxOptions()
        {
            var options = new MediaAccountListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var entity = new MediaAccount();

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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "MediaAccountId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        private IQueryable<MediaAccount> GetMediaAccountListQueryable( RockContext rockContext )
        {
            var qry = new MediaAccountService( rockContext )
               .Queryable()
               .Include( a => a.ComponentEntityType )
               .Include( "MediaFolders.MediaElements" );

            return qry;
        }

        /// <inheritdoc/>
        protected override IQueryable<MediaAccountData> GetListQueryable( RockContext rockContext )
        {
            return GetMediaAccountListQueryable( rockContext ).Select( b => new MediaAccountData
            {
                MediaAccount = b,
                FolderCount = b.MediaFolders.Count,
                VideoCount = b.MediaFolders.SelectMany( a => a.MediaElements ).Count(),
            } );
        }

        /// <inheritdoc/>
        protected override GridBuilder<MediaAccountData> GetGridBuilder()
        {
            var blockOptions = new GridBuilderGridOptions<MediaAccountData>
            {
                LavaObject = row => row.MediaAccount
            };

            return new GridBuilder<MediaAccountData>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.MediaAccount.IdKey )
                .AddTextField( "name", a => a.MediaAccount.Name )
                .AddTextField( "componentEntityType", a => a.MediaAccount.ComponentEntityType?.FriendlyName )
                .AddDateTimeField( "lastRefreshDateTime", a => a.MediaAccount.LastRefreshDateTime )
                .AddField( "folderCount", a => a.FolderCount )
                .AddField( "videoCount", a => a.VideoCount )
                .AddAttributeFieldsFrom( a => a.MediaAccount, _gridAttributes.Value );
        }

        /// <summary>
        /// Builds the list of grid attributes that should be included on the Grid.
        /// </summary>
        /// <remarks>
        /// The default implementation returns only attributes that are not qualified.
        /// </remarks>
        /// <returns>A list of <see cref="AttributeCache"/> objects.</returns>
        private static List<AttributeCache> BuildGridAttributes()
        {
            var entityTypeId = EntityTypeCache.Get<MediaAccount>( false )?.Id;

            if ( entityTypeId.HasValue )
            {
                return AttributeCache.GetOrderedGridAttributes( entityTypeId, string.Empty, string.Empty );
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
            using ( var rockContext = new RockContext() )
            {
                var entityService = new MediaAccountService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{MediaAccount.FriendlyTypeName} not found." );
                }

                if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete {MediaAccount.FriendlyTypeName}." );
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

        #region Support Classes

        /// <summary>
        /// The temporary data format to use when building the results for the
        /// grid.
        /// </summary>
        public class MediaAccountData
        {
            /// <summary>
            /// Gets or sets the whole media account object from the database.
            /// </summary>
            /// <value>
            /// The whole media account object from the database.
            /// </value>
            public MediaAccount MediaAccount { get; set; }

            /// <summary>
            /// Gets or sets the number of folder in this media account.
            /// </summary>
            /// <value>
            /// The number of folder in this media account.
            /// </value>
            public int FolderCount { get; set; }

            /// <summary>
            /// Gets or sets the number of video in this media account.
            /// </summary>
            /// <value>
            /// The number of video in this media account.
            /// </value>
            public int VideoCount { get; set; }
        }

        #endregion
    }
}

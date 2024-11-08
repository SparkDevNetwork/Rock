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
using Rock.ViewModels.Blocks.Core.BinaryFileList;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of binary files.
    /// </summary>

    [DisplayName( "Binary File List" )]
    [Category( "Core" )]
    [Description( "Displays a list of all binary files." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the binary file details.",
        Key = AttributeKey.DetailPage )]

    [BinaryFileTypeField( "Binary File Type",
        Key = AttributeKey.BinaryFileType )]

    [Rock.SystemGuid.EntityTypeGuid( "67d1cc46-c871-46e7-affd-1b1b23eeea84" )]
    [Rock.SystemGuid.BlockTypeGuid( "69a45481-467b-47ef-9838-4462e5615216" )]
    [CustomizedGrid]
    public class BinaryFileList : RockEntityListBlockType<BinaryFile>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string BinaryFileType = "BinaryFileType";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class UserPreferenceKey
        {
            public const string FileName = "File Name";
            public const string MimeType = "Mime Type";
            public const string IncludeTemporary = "Include Temporary";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// BinaryFileType cache, should be accessed using the GetBinaryFileType method.
        /// </summary>
        private BinaryFileType _binaryFileType;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<BinaryFileListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddDeleteEnabled();
            box.IsDeleteEnabled = GetIsAddDeleteEnabled();
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
        private BinaryFileListOptionsBag GetBoxOptions()
        {
            var options = new BinaryFileListOptionsBag()
            {
                BinaryFileTypeName = GetBinaryFileType()?.Name
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
            BinaryFileType binaryFileType = GetBinaryFileType();
            var queryParams = new Dictionary<string, string>
            {
                ["BinaryFileId"] = "((Key))",
                ["BinaryFileTypeId"] = binaryFileType == null ? "0" : binaryFileType.Id.ToString(),
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, queryParams )
            };
        }

        /// <summary>
        /// Gets the type of the binary file using the attribute setting.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private BinaryFileType GetBinaryFileType()
        {
            if ( _binaryFileType == null && Guid.TryParse( GetAttributeValue( AttributeKey.BinaryFileType ), out var binaryFileTypeGuid ) )
            {
                var service = new BinaryFileTypeService( new RockContext() );
                _binaryFileType = service.Get( binaryFileTypeGuid );
            }

            return _binaryFileType;
        }

        /// <inheritdoc/>
        protected override IQueryable<BinaryFile> GetListQueryable( RockContext rockContext )
        {
            var binaryFileTypeGuid = GetAttributeValue( AttributeKey.BinaryFileType ).AsGuid();
            var preferences = GetBlockPersonPreferences();
            var queryable = new BinaryFileService( rockContext ).Queryable().Where( f => f.BinaryFileType.Guid == binaryFileTypeGuid );

            var includeTemp = preferences.GetValue( UserPreferenceKey.IncludeTemporary ).AsBoolean();
            if ( !includeTemp )
            {
                queryable = queryable.Where( f => !f.IsTemporary );
            }

            string fileName = preferences.GetValue( UserPreferenceKey.FileName );
            if ( !string.IsNullOrWhiteSpace( fileName ) )
            {
                queryable = queryable.Where( f => f.FileName.Contains( fileName ) );
            }

            string type = preferences.GetValue( UserPreferenceKey.MimeType );
            if ( !string.IsNullOrWhiteSpace( type ) )
            {
                queryable = queryable.Where( f => f.MimeType.Contains( type ) );
            }

            return queryable;
        }

        /// <inheritdoc/>
        protected override IQueryable<BinaryFile> GetOrderedListQueryable( IQueryable<BinaryFile> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( f => f.FileName );
        }

        /// <inheritdoc/>
        protected override GridBuilder<BinaryFile> GetGridBuilder()
        {
            return new GridBuilder<BinaryFile>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "fileName", a => a.FileName )
                .AddTextField( "mimeType", a => a.MimeType )
                .AddDateTimeField( "lastModified", a => a.ModifiedDateTime )
                .AddField( "isSystem", a => a.IsSystem );
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
                var entityService = new BinaryFileService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{BinaryFile.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete {BinaryFile.FriendlyTypeName}." );
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                var guid = entity.Guid;
                var clearDeviceCache = entity.BinaryFileType.Guid.Equals( Rock.SystemGuid.BinaryFiletype.CHECKIN_LABEL.AsGuid() );

                entityService.Delete( entity );
                rockContext.SaveChanges();

                if ( clearDeviceCache )
                {
                    Rock.CheckIn.KioskDevice.Clear();
                    Rock.CheckIn.KioskLabel.Remove( guid );
                }

                return ActionOk();
            }
        }

        #endregion
    }
}

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
using System.Linq;
using System.Web.Http;

using Rock.Rest.Filters;
using Rock.Storage.AssetStorage;
using Rock.Web.Cache.Entities;
using Rock.Web.UI.Controls;

namespace Rock.Rest.Controllers
{
    public partial class AssetStorageProvidersController
    {
        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="assetFolderId">The asset folder identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/AssetStorageProviders/GetChildren" )]
        [Rock.SystemGuid.RestActionGuid( "4D7B4AE1-82F3-46B9-99E3-BAE03B2EDFAA" )]
        public IQueryable<TreeViewItem> GetChildren( string assetFolderId )
        {
            var treeViewItemList = new List<TreeViewItem>();
            if ( assetFolderId == "0" )
            {
                foreach ( var assetStorageProviderCache in AssetStorageProviderCache.All().Where( a => a.IsActive ) )
                {
                    var component = assetStorageProviderCache.AssetStorageComponent;
                    var rootFolder = component.GetRootFolder( assetStorageProviderCache.ToEntity() );

                    var treeViewItem = new TreeViewItem();
                    treeViewItem.Id = Uri.EscapeDataString( $"{assetStorageProviderCache.Id},{rootFolder},{true}" );
                    treeViewItem.IconCssClass = component.IconCssClass;
                    treeViewItem.Name = assetStorageProviderCache.Name;
                    treeViewItem.HasChildren = true;
                    treeViewItemList.Add( treeViewItem );
                }
            }
            else
            {
                var assetFolderIdParts = assetFolderId.Split( ',' ).ToArray();
                if ( assetFolderIdParts.Length > 0 )
                {
                    int assetStorageProviderId = assetFolderIdParts[0].AsInteger();
                    var assetStorageProviderCache = AssetStorageProviderCache.Get( assetStorageProviderId );

                    Asset asset = new Asset { Key = string.Empty, Type = AssetType.Folder };
                    if ( assetFolderIdParts.Length > 1 && assetFolderIdParts[1].Length > 0 )
                    {
                        var scrubbedFileName = System.Text.RegularExpressions.Regex.Replace( assetFolderIdParts[1], "[" + System.Text.RegularExpressions.Regex.Escape( string.Concat( System.IO.Path.GetInvalidPathChars() ) ) + "]", string.Empty, System.Text.RegularExpressions.RegexOptions.CultureInvariant );
                        var scrubbedFilePath = System.IO.Path.GetDirectoryName( scrubbedFileName ).Replace( '\\', '/' );
                        scrubbedFileName = System.IO.Path.GetFileName( scrubbedFileName );
                        scrubbedFileName = System.Text.RegularExpressions.Regex.Replace( scrubbedFileName, "[" + System.Text.RegularExpressions.Regex.Escape( string.Concat( System.IO.Path.GetInvalidFileNameChars() ) ) + "]", string.Empty, System.Text.RegularExpressions.RegexOptions.CultureInvariant );

                        var scrubbedFileNameAndPath = $"{scrubbedFilePath}/{scrubbedFileName}";
                        asset.Key = scrubbedFileNameAndPath;
                    }
                    else
                    {
                        asset.Key = string.Empty;
                    }

                    var component = assetStorageProviderCache.AssetStorageComponent;
                    var folderAssets = component.ListFoldersInFolder( assetStorageProviderCache.ToEntity(), asset );
                    foreach ( Asset folderAsset in folderAssets )
                    {
                        var treeViewItem = new TreeViewItem();
                        treeViewItem.Id = Uri.EscapeDataString( $"{assetStorageProviderCache.Id},{folderAsset.Key}" );
                        treeViewItem.IconCssClass = "fa fa-folder";
                        treeViewItem.Name = folderAsset.Name;

                        // NOTE: This is not very performant. We should see if we can get a bool response from providers instead of getting the entire folder list for each subfolder.
                        //treeViewItem.HasChildren = component.ListFoldersInFolder( assetStorageProvider, folderAsset ).Any();

                        // This is fast but will show the triangle for each folder. If no data the triangle disappears after clicking it.
                        treeViewItem.HasChildren = true;

                        treeViewItemList.Add( treeViewItem );
                    }
                }
            }

            return treeViewItemList.AsQueryable();
        }

        /// <summary>
        /// Gets the folders.
        /// </summary>
        /// <param name="assetStorageProviderId">The asset storage system identifier.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/AssetStorageProviders/GetFolders" )]
        [Rock.SystemGuid.RestActionGuid( "8A2E7EC6-2A38-41AC-9A83-B74FF4B7FD45" )]
        public List<Asset> GetFolders( int assetStorageProviderId, string path )
        {
            var assetStorageProviderCache = AssetStorageProviderCache.Get( assetStorageProviderId );

            var component = assetStorageProviderCache.AssetStorageComponent;

            List<Asset> assets = component.ListFoldersInFolder( assetStorageProviderCache.ToEntity(), new Asset { Key = path } );

            return assets;
        }

        /// <summary>
        /// Gets the files.
        /// </summary>
        /// <param name="assetStorageProviderId">The asset storage system identifier.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/AssetStorageProviders/GetFiles" )]
        [Rock.SystemGuid.RestActionGuid( "40DEFE35-2196-4A11-BD08-BCFFCE1C4240" )]
        public List<Asset> GetFiles( int assetStorageProviderId, string path )
        {
            var assetStorageProviderCache = AssetStorageProviderCache.Get( assetStorageProviderId );

            var component = assetStorageProviderCache.AssetStorageComponent;

            List<Asset> assets = component.ListFilesInFolder( assetStorageProviderCache.ToEntity(), new Asset { Key = path } );


            return assets;
        }
    }
}
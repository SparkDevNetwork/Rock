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

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Storage.AssetStorage;
using Rock.Web.UI.Controls;

namespace Rock.Rest.Controllers
{
    [RockGuid( "485fa8f3-5804-47b8-a84c-9813f2e16b2e" )]
    public partial class AssetStorageProvidersController
    {
        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="assetFolderId">The asset folder identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/AssetStorageProviders/GetChildren" )]
        [RockGuid( "4d7b4ae1-82f3-46b9-99e3-bae03b2edfaa" )]
        public IQueryable<TreeViewItem> GetChildren( string assetFolderId )
        {
            var assetStorageService = new AssetStorageProviderService( new RockContext() );
            var treeViewItemList = new List<TreeViewItem>();
            if ( assetFolderId == "0" )
            {
                foreach ( var assetStorageProvider in assetStorageService.GetActiveNoTracking() )
                {
                    var component = assetStorageProvider.GetAssetStorageComponent();
                    var rootFolder = component.GetRootFolder( assetStorageProvider );

                    var treeViewItem = new TreeViewItem();
                    treeViewItem.Id = Uri.EscapeDataString( $"{assetStorageProvider.Id.ToString()},{rootFolder},{true}" );
                    treeViewItem.IconCssClass = component.IconCssClass;
                    treeViewItem.Name = assetStorageProvider.Name;
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
                    var assetStorageProvider = assetStorageService.GetNoTracking( assetStorageProviderId );

                    Asset asset = new Asset { Key = string.Empty, Type = AssetType.Folder };
                    if ( assetFolderIdParts.Length > 1 )
                    {
                        asset.Key = assetFolderIdParts[1];
                    }

                    var component = assetStorageProvider.GetAssetStorageComponent();
                    var folderAssets = component.ListFoldersInFolder( assetStorageProvider, asset );
                    foreach ( Asset folderAsset in folderAssets )
                    {
                        var treeViewItem = new TreeViewItem();
                        treeViewItem.Id = Uri.EscapeDataString( $"{assetStorageProvider.Id},{folderAsset.Key}" );
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
        [RockGuid( "8a2e7ec6-2a38-41ac-9a83-b74ff4b7fd45" )]
        public List<Asset> GetFolders( int assetStorageProviderId, string path )
        {
            var assetStorageProviderService = ( AssetStorageProviderService ) Service;
            var assetStorageProvider = assetStorageProviderService.Get( assetStorageProviderId );

            assetStorageProvider.LoadAttributes();
            var component = assetStorageProvider.GetAssetStorageComponent();

            List<Asset> assets = component.ListFoldersInFolder( assetStorageProvider, new Asset { Key = path } );

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
        [RockGuid( "40defe35-2196-4a11-bd08-bcffce1c4240" )]
        public List<Asset> GetFiles( int assetStorageProviderId, string path )
        {

            var assetStorageProviderService = ( AssetStorageProviderService ) Service;
            var assetStorageProvider = assetStorageProviderService.Get( assetStorageProviderId );

            assetStorageProvider.LoadAttributes();
            var component = assetStorageProvider.GetAssetStorageComponent();

            List<Asset> assets = component.ListFilesInFolder( assetStorageProvider, new Asset { Key = path } );


            return assets;
        }
    }
}

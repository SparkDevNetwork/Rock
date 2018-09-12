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
    public partial class AssetStorageSystemsController
    {
        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="assetFolderId">The asset folder identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/AssetStorageSystems/GetChildren" )]
        public IQueryable<TreeViewItem> GetChildren( string assetFolderId )
        {
            var assetStorageService = new AssetStorageSystemService( new RockContext() );
            var treeViewItemList = new List<TreeViewItem>();
            if ( assetFolderId == "0" )
            {
                foreach ( var assetStorageSystem in assetStorageService.GetActiveNoTracking() )
                {
                    var component = assetStorageSystem.GetAssetStorageComponent();
                    var treeViewItem = new TreeViewItem();
                    treeViewItem.Id = assetStorageSystem.Id.ToString();
                    treeViewItem.IconCssClass = component.IconCssClass;
                    treeViewItem.Name = assetStorageSystem.Name;
                    treeViewItem.HasChildren = true;
                    treeViewItemList.Add( treeViewItem );
                }
            }
            else
            {
                var assetFolderIdParts = assetFolderId.Split(',').ToArray();
                if ( assetFolderIdParts.Length > 0 )
                {
                    int assetStorageSystemId = assetFolderIdParts[0].AsInteger();
                    var assetStorageSystem = assetStorageService.GetNoTracking( assetStorageSystemId );

                    Asset asset = new Asset { Key = string.Empty, Type = AssetType.Folder };
                    if ( assetFolderIdParts.Length > 1 )
                    {
                        asset.Key = assetFolderIdParts[1];
                    }

                    var component = assetStorageSystem.GetAssetStorageComponent();
                    var folderAssets = component.ListFoldersInFolder( assetStorageSystem, asset );
                    foreach ( Asset folderAsset in folderAssets )
                    {
                        var treeViewItem = new TreeViewItem();
                        treeViewItem.Id = Uri.EscapeDataString( $"{assetStorageSystem.Id},{folderAsset.Key}" );
                        treeViewItem.IconCssClass = "fa fa-folder";
                        treeViewItem.Name = folderAsset.Name;

                        // NOTE: For performance reasons, we'll just assume it has Child Folders
                        // If we want it to be more accurate, change it to something like:
                        // treeViewItem.HasChildren = component.ListFoldersInFolder( assetStorageSystem, folderAsset ).Any();
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
        /// <param name="assetStorageSystemId">The asset storage system identifier.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/AssetStorageSystems/GetFolders" )]
        public List<Asset> GetFolders( int assetStorageSystemId, string path )
        {
            var assetStorageSystemService = ( AssetStorageSystemService ) Service;
            var assetStorageSystem = assetStorageSystemService.Get( assetStorageSystemId );

            assetStorageSystem.LoadAttributes();
            var component = assetStorageSystem.GetAssetStorageComponent();

            List<Asset> assets = component.ListFoldersInFolder( assetStorageSystem, new Asset { Key = path } );

            return assets;
        }

        /// <summary>
        /// Gets the files.
        /// </summary>
        /// <param name="assetStorageSystemId">The asset storage system identifier.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/AssetStorageSystems/GetFiles" )]
        public List<Asset> GetFiles( int assetStorageSystemId, string path )
        {

            var assetStorageSystemService = ( AssetStorageSystemService ) Service;
            var assetStorageSystem = assetStorageSystemService.Get( assetStorageSystemId );

            assetStorageSystem.LoadAttributes();
            var component = assetStorageSystem.GetAssetStorageComponent();

            List<Asset> assets = component.ListFilesInFolder( assetStorageSystem, new Asset { Key = path } );


            return assets;
        }
    }
}

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
            return GetAssetFolderChildren( assetFolderId, GetPerson() );
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="assetFolderId">The asset folder identifier.</param>
        /// <param name="person">The person requesting the children, used to check VIEW on each provider.</param>
        /// <returns></returns>
        internal static IQueryable<TreeViewItem> GetAssetFolderChildren( string assetFolderId, Rock.Model.Person person )
        {
            var treeViewItemList = new List<TreeViewItem>();
            if ( assetFolderId == "0" )
            {
                /*
                    11/20/2026 - MSE

                    The REST action grant only says the person may use this endpoint. Each asset
                    storage provider has its own VIEW rule, so only list the providers the person
                    can view, and return nothing when a provider they cannot view is expanded.

                    Reason: Anyone with the REST action could browse every provider's folders.
                */
                foreach ( var assetStorageProviderCache in AssetStorageProviderCache.All().Where( a => a.IsActive && a.IsAuthorized( Rock.Security.Authorization.VIEW, person ) ) )
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
                    if ( assetStorageProviderCache == null || !assetStorageProviderCache.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                    {
                        return new List<TreeViewItem>().AsQueryable();
                    }

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
            var assetStorageProviderCache = GetViewableProvider( assetStorageProviderId, GetPerson() );

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
            var assetStorageProviderCache = GetViewableProvider( assetStorageProviderId, GetPerson() );

            var component = assetStorageProviderCache.AssetStorageComponent;

            List<Asset> assets = component.ListFilesInFolder( assetStorageProviderCache.ToEntity(), new Asset { Key = path } );


            return assets;
        }

        /// <summary>
        /// Gets the asset storage provider if the person is allowed to view it.
        /// </summary>
        /// <param name="assetStorageProviderId">The asset storage provider identifier.</param>
        /// <param name="person">The person making the request.</param>
        /// <returns>The asset storage provider.</returns>
        /// <exception cref="HttpResponseException">Not found when the provider does not exist, forbidden when the person cannot view it.</exception>
        private static AssetStorageProviderCache GetViewableProvider( int assetStorageProviderId, Rock.Model.Person person )
        {
            var provider = AssetStorageProviderCache.Get( assetStorageProviderId );
            if ( provider == null )
            {
                throw new HttpResponseException( System.Net.HttpStatusCode.NotFound );
            }

            /*
                11/20/2026 - MSE

                The REST action grant only says the person may use this endpoint. The provider
                has its own VIEW rule, which must be checked before listing its folders or files.

                Reason: Anyone with the REST action could list any provider's folders and files.
            */
            if ( !provider.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
            {
                throw new HttpResponseException( System.Net.HttpStatusCode.Forbidden );
            }

            return provider;
        }
    }
}
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
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.OData;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Storage.AssetStorage;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Rest.Controllers
{
    public partial class AssetStorageSystemsController
    {



        /// <summary>
        /// Gets the folders.
        /// </summary>
        /// <param name="assetStorageSystemId">The asset storage system identifier.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/AssetStorageSystems/GetFolders" )]
        public IQueryable<TreeViewItem> GetFolders(int assetStorageSystemId, string path )
        {
            List<TreeViewItem> groupNameList = new List<TreeViewItem>();

            var assetStorageSystemService = ( AssetStorageSystemService ) Service;
            var assetStorageSystem = assetStorageSystemService.Get( assetStorageSystemId );

            assetStorageSystem.LoadAttributes();
            var component = assetStorageSystem.GetAssetStorageComponent();



            List<Asset> assets = component.ListFoldersInFolder( assetStorageSystem, new Asset { Key = path } );

            return groupNameList.AsQueryable();
        }

        /// <summary>
        /// Gets the files.
        /// </summary>
        /// <param name="assetStorageSystemId">The asset storage system identifier.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route("api/AssetStorageSystems/GetFiles")]
        public List<Asset> GetFiles(int assetStorageSystemId, string path)
        {

            List<TreeViewItem> groupNameList = new List<TreeViewItem>();

            var assetStorageSystemService = ( AssetStorageSystemService ) Service;
            var assetStorageSystem = assetStorageSystemService.Get( assetStorageSystemId );

            assetStorageSystem.LoadAttributes();
            var component = assetStorageSystem.GetAssetStorageComponent();

            List<Asset> assets = component.ListFilesInFolder( assetStorageSystem, new Asset { Key = path } );


            return assets;
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="assetStorageSystemId">The asset storage system identifier.</param>
        /// <param name="path">The path.</param>
        /// <param name="isStorageSystem">if set to <c>true</c> [is storage system].</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route("api/AssetStoragesystems/GetChildren")]
        public IQueryable GetChildren( int assetStorageSystemId, string path, bool isStorageSystem )
        {
            return null;
        }
    }
}

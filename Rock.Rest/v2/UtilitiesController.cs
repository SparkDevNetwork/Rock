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

using Rock.Rest.Filters;
using Rock.ViewModels.Utility;
using System.Web.Http;
using Rock.Web.Cache;

namespace Rock.Rest.v2
{
    /// <summary>
    /// API controller for the /api/v2/Utilities endpoints.
    /// </summary>
    /// <seealso cref="Rock.Rest.ApiControllerBase" />
    [RoutePrefix( "api/v2/Utilities" )]
    [Rock.SystemGuid.RestControllerGuid( "AE3ABE89-D40C-4EB6-BAAE-C477DAAD71AD" )]
    public partial class UtilitiesController : ApiControllerBase
    {
        /// <summary>
        /// Get a flat list of all the folders and subfolders in a given root folder, excluding a given folder and its children.
        /// </summary>
        /// <param name="options">The options that describe which folders to load and not load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent all the folders.</returns>
        [HttpPost]
        [System.Web.Http.Route( "GetImageFileExtensions" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "D0609BC8-CFDB-426C-A2A2-2685BEB63527" )]
        public IHttpActionResult GetImageFileExtensions()
        {
            return Ok( GlobalAttributesCache.Get().GetValue( "ContentImageFiletypeWhitelist" ) );
        }
    }
}

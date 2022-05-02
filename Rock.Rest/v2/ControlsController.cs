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

using Rock.Data;
using Rock.Rest.Filters;
using Rock.Rest.v2.Options;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

using System.Linq;
using System.Web.Http;

namespace Rock.Rest.v2
{
    /// <summary>
    /// Provides API endpoints for the Controls controller.
    /// </summary>
    [RoutePrefix( "api/v2/Controls" )]
    [RockGuid( "815b51f0-b552-47fd-8915-c653eedd5b67" )]
    public class ControlsController : ApiControllerBase
    {
        /// <summary>
        /// Gets the entity types that can be displayed in the entity type picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A collection of view models that represent the tree items.</returns>
        [HttpPost]
        [System.Web.Http.Route( "EntityTypePickerGetEntityTypes" )]
        [Authenticate]
        [RockGuid( "afdd3d40-5856-478b-a41a-0539127f0631" )]
        public IHttpActionResult EntityTypePickerGetEntityTypes( [FromBody] EntityTypePickerGetEntityTypesOptions options )
        {
            using ( var rockContext = new RockContext() )
            {
                var items = EntityTypeCache.All( rockContext )
                    .Where( t => t.IsEntity )
                    .OrderByDescending( t => t.IsCommon )
                    .ThenBy( t => t.FriendlyName )
                    .Select( t => new ListItemBag
                    {
                        Value = t.Guid.ToString(),
                        Text = t.FriendlyName,
                        Category = t.IsCommon ? "Common" : "All Entities"
                    } )
                    .ToList();

                return Ok( items );
            }
        }
    }
}

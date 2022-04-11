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
using System.Linq;
using System.Web.Http;

using Rock.Data;
using Rock.Rest.Filters;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Rest.v2.Controls
{
    /// <summary>
    /// Provides API endpoints for the DefinedValuePicker control.
    /// </summary>
    /// <seealso cref="Rock.Rest.v2.Controls.ControlsControllerBase" />
    [RoutePrefix( "api/v2/Controls/DefinedValuePicker" )]
    [RockGuid( "684ECDA2-50FD-4C78-B524-FB3E48ED9842" )]
    public class DefinedValuePickerController : ControlsControllerBase
    {
        /// <summary>
        /// Gets the child items that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="guid">The unique identifier of the defined type whose values are to be retrieved.</param>
        /// <param name="includeInactive"><c>true</c> if inactive defined values should be included; otherwise <c>false</c>.</param>
        /// <returns>A collection of view models that represent the defined values.</returns>
        [HttpGet]
        [System.Web.Http.Route( "definedValues/{guid:guid}" )]
        [Authenticate]
        [RockGuid( "4F8DA074-780F-4A07-B47E-3373A5D18928" )]
        public IHttpActionResult GetDefinedValues( Guid guid, bool includeInactive = false )
        {
            using ( var rockContext = new RockContext() )
            {
                var definedType = DefinedTypeCache.Get( guid );

                if ( definedType == null || !definedType.IsAuthorized( Rock.Security.Authorization.VIEW, RockRequestContext.CurrentPerson ) )
                {
                    return NotFound();
                }

                var definedValues = definedType.DefinedValues
                    .Where( v => v.IsAuthorized( Rock.Security.Authorization.VIEW, RockRequestContext.CurrentPerson )
                        && ( includeInactive || v.IsActive ) )
                    .OrderBy( v => v.Order )
                    .ThenBy( v => v.Value )
                    .Select( v => new ListItemBag
                    {
                        Value = v.Guid.ToString(),
                        Text = v.Value
                    } )
                    .ToList();

                return Ok( definedValues );
            }
        }
    }
}

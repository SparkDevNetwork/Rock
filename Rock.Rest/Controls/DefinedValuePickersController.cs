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
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Rest.Controls
{
    /// <summary>
    /// Controller used by Defined Value Pickers in the Rock UI
    /// </summary>
    public class DefinedValuePickersController : ControlsControllerBase
    {
        /// <summary>
        /// Gets the defined values.
        /// </summary>
        /// <param name="definedTypeGuid">The defined type unique identifier.</param>
        /// <param name="IsActive">if set to <c>true</c> [is active].</param>
        /// <returns></returns>
        [Authenticate]
        [HttpGet]
        [Route( "api/v2/Controls/DefinedValuePickers/{definedTypeGuid}" )]
        public IEnumerable<DefinedValueViewModel> GetDefinedValues( Guid definedTypeGuid, [FromUri] bool IsActive = true )
        {
            var definedType = DefinedTypeCache.Get( definedTypeGuid );

            if ( definedType == null )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.NotFound, "The defined type was not found" );
                throw new HttpResponseException( errorResponse );
            }

            var currentPerson = GetPerson();

            if ( !definedType.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.Unauthorized, "Unauthorized" );
                throw new HttpResponseException( errorResponse );
            }

            var viewModels = definedType
                .DefinedValues
                .Where( dv => dv.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) );

            if ( IsActive )
            {
                viewModels = viewModels.Where( dv => dv.IsActive );
            }

            return viewModels.Select( dv => new DefinedValueViewModel
            {
                Guid = dv.Guid,
                Value = dv.Value,
                Description = dv.Description
            } );
        }

        /// <summary>
        /// Defined Value View Model
        /// </summary>
        public sealed class DefinedValueViewModel
        {
            /// <summary>
            /// Gets or sets the unique identifier.
            /// </summary>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// Gets or sets the description.
            /// </summary>
            public string Description { get; set; }
        }
    }
}

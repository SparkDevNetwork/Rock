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
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Obsidian.Controllers.Controls
{
    /// <summary>
    /// Defined Value Picker
    /// </summary>
    public class DefinedValuePickerController : ObsidianController
    {
        /// <summary>
        /// Gets the defined values.
        /// </summary>
        /// <param name="definedTypeGuid">The defined type unique identifier.</param>
        /// <param name="IsActive">if set to <c>true</c> [is active].</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/obsidian/v1/controls/definedvaluepicker/{definedTypeGuid}" )]
        public IEnumerable<DefinedValueViewModel> GetDefinedValues( Guid definedTypeGuid, [FromUri] bool IsActive = true )
        {
            IEnumerable<DefinedValueCache> viewModels = DefinedTypeCache.Get(definedTypeGuid)?.DefinedValues;

            if ( viewModels == null )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.NotFound, "The defined type was not found" );
                throw new HttpResponseException( errorResponse );
            }

            if ( IsActive )
            {
                viewModels = viewModels.Where( dv => dv.IsActive );
            }

            return viewModels.Select( dv => new DefinedValueViewModel
            {
                Guid = dv.Guid,
                Value = dv.Value
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
        }
    }
}

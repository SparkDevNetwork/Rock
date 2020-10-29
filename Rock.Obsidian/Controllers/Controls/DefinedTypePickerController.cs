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
using System.Web.Http;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Obsidian.Controllers.Controls
{
    /// <summary>
    /// Defined Type Picker
    /// </summary>
    public class DefinedTypePickerController : ObsidianController
    {
        /// <summary>
        /// Gets the defined types.
        /// </summary>
        /// <param name="IsActive">if set to <c>true</c> [is active].</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/obsidian/v1/controls/definedtypepicker" )]
        public IEnumerable<DefinedTypeViewModel> GetDefinedTypes( [FromUri] bool IsActive = true )
        {
            IEnumerable<DefinedTypeCache> viewModels = DefinedTypeCache.All();

            if ( IsActive )
            {
                viewModels = viewModels.Where( dt => dt.IsActive );
            }

            return viewModels.Select( dt => new DefinedTypeViewModel
            {
                Guid = dt.Guid,
                Name = dt.Name
            } );
        }
    }

    /// <summary>
    /// Defined Type View Model
    /// </summary>
    public sealed class DefinedTypeViewModel
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }
    }
}

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
using System.Web.Http;
using Rock.Rest.Filters;

namespace Rock.Obsidian.Controllers.Controls
{
    /// <summary>
    /// Defined Value Picker
    /// </summary>
    public class DateController : ObsidianController
    {
        /// <summary>
        /// Gets the defined values.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException">
        /// </exception>
        [Authenticate]
        [HttpGet]
        [System.Web.Http.Route( "api/obsidian/v1/format/date" )]
        public string GetFormattedDate( [FromUri] DateTime value, [FromUri] string format = "MM/dd/yyyy" )
        {
            return value.ToString( format );
        }
    }
}

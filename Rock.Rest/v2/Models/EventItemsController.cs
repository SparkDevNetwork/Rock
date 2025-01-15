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

using System.Net;

using Microsoft.AspNetCore.Mvc;

using Rock.Rest.Filters;
using Rock.Security;
using Rock.ViewModels.Rest.Models;

namespace Rock.Rest.v2.Models
{
#if WEBFORMS
    using FromBodyAttribute = System.Web.Http.FromBodyAttribute;
    using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
    using IActionResult = System.Web.Http.IHttpActionResult;
    using RouteAttribute = System.Web.Http.RouteAttribute;
#endif

    public partial class EventItemsController : ApiControllerBase
    {
        /// <summary>
        /// Creates a new item in the database.
        /// </summary>
        /// <param name="value">The item to be created.</param>
        /// <returns>An object that contains the new identifier values.</returns>
        [HttpPost]
        [Authenticate]
        [Secured( Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_READ )]
        [Route( "" )]
        [ProducesResponseType( HttpStatusCode.Created, Type = typeof( CreatedAtResponseBag ) )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "1bb0244c-40f1-565d-8de4-7b0e3de2e742" )]
        public IActionResult PostItem( [FromBody] Rock.Model.EventItem value )
        {
            var helper = new CrudEndpointHelper<Rock.Model.EventItem, Rock.Model.EventItemService>( this );

            // We want to ignore security when creating new items because there
            // is no EventCalendar linkage yet to check security against.
            helper.IsSecurityIgnored = true;

            return helper.Create( value );
        }
    }
}

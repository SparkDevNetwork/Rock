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

using Rock.Model;
using Rock.Rest.Filters;

using System.Collections.Generic;

#if WEBFORMS
using System.Web.Http;
using IActionResult = System.Web.Http.IHttpActionResult;
#endif

namespace Rock.Rest.v2.Models
{
    /// <summary>
    /// Provides API endpoints for the Groups controller.
    /// </summary>
    [RoutePrefix( "api/v2/models/groups" )]
    [Rock.SystemGuid.RestControllerGuid( "999c734f-e206-4a25-8145-1213b4ffd8a9" )]
    public partial class GroupsController : ApiControllerBase
    {
        [HttpGet]
        [Authenticate]
        [Route( "{key}" )]
        [SystemGuid.RestActionGuid( "54736156-88af-461a-b311-e8dd48b24019" )]
        public IActionResult GetItem( string key )
        {
            return new RestApiHelper<Group, GroupService>( this ).Get( key );
        }

        [HttpPost]
        [Authenticate]
        [Route( "" )]
        [SystemGuid.RestActionGuid( "e0da3476-518f-4729-9f67-ca0dff4937d2" )]
        public IActionResult PostItem( [FromBody] Group value )
        {
            return new RestApiHelper<Group, GroupService>( this ).Create( value );
        }

        [HttpPut]
        [Authenticate]
        [Route( "{key}" )]
        [SystemGuid.RestActionGuid( "0d91b319-bc43-4ab7-b6af-17ad21b1dec6" )]
        public IActionResult PutItem( string key, [FromBody] Group value )
        {
            return new RestApiHelper<Group, GroupService>( this ).Update( key, value );
        }

        [HttpPatch]
        [Authenticate]
        [Route( "{key}" )]
        [SystemGuid.RestActionGuid( "38ddc3c7-1a05-4460-acfc-71b604d1b7ad" )]
        public IActionResult PatchItem( string key, [FromBody] Dictionary<string, object> values )
        {
            return new RestApiHelper<Group, GroupService>( this ).Patch( key, values );
        }

        [HttpDelete]
        [Authenticate]
        [Route( "{key}" )]
        [SystemGuid.RestActionGuid( "b740d84c-6b71-4860-b380-d5ecf5d405b8" )]
        public IActionResult DeleteItem( string key )
        {
            return new RestApiHelper<Group, GroupService>( this ).Delete( key );
        }

        [HttpGet]
        [Authenticate]
        [Route( "{key}/attributevalues" )]
        [SystemGuid.RestActionGuid( "3a03bd5a-a32a-4ac5-bae2-cda1a3141d11" )]
        public IActionResult GetAttributeValues( string key )
        {
            return new RestApiHelper<Group, GroupService>( this ).GetAttributeValues( key );
        }

        [HttpPatch]
        [Authenticate]
        [Route( "{key}/attributevalues" )]
        [SystemGuid.RestActionGuid( "a4fa9111-c524-4a24-aa1b-b9587ed988f0" )]
        public IActionResult PatchAttributeValues( string key, [FromBody] Dictionary<string, string> values )
        {
            return new RestApiHelper<Group, GroupService>( this ).PatchAttributeValues( key, values );
        }
    }
}

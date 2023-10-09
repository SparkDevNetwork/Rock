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

using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;

#if WEBFORMS
using System.Web.Http;

using IActionResult = System.Web.Http.IHttpActionResult;
using RouteAttribute = System.Web.Http.RouteAttribute;
#endif

namespace Rock.Rest.v2.Models
{
    /// <summary>
    /// Provides API endpoints for the Groups controller.
    /// </summary>
    [RoutePrefix( "api/v2/models/groups" )]
    [SecurityAction( "UnrestrictedView", "Allows viewing entities regardless of per-entity security authorization." )]
    [SecurityAction( "UnrestrictedEdit", "Allows editing entities regardless of per-entity security authorization." )]
    [Rock.SystemGuid.RestControllerGuid( "999c734f-e206-4a25-8145-1213b4ffd8a9" )]
    public partial class GroupsController : ApiControllerBase
    {
        /// <summary>
        /// Gets a single item from the database.
        /// </summary>
        /// <param name="key">The key as either an Id, Guid or IdKey value.</param>
        /// <returns>The requested item.</returns>
        [HttpGet]
        [Authenticate]
        [Route( "{key}" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( Group ) )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound   )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "54736156-88af-461a-b311-e8dd48b24019" )]
        public IActionResult GetItem( string key )
        {
            return new RestApiHelper<Group, GroupService>( this ).Get( key );
        }

        /// <summary>
        /// Creates a new item in the database.
        /// </summary>
        /// <param name="value">The item to be created.</param>
        /// <returns>An object that contains the new identifier values.</returns>
        [HttpPost]
        [Authenticate]
        [Route( "" )]
        [ProducesResponseType( HttpStatusCode.Created, Type = typeof( CreatedAtResult ) )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "e0da3476-518f-4729-9f67-ca0dff4937d2" )]
        public IActionResult PostItem( [FromBody] Group value )
        {
            return new RestApiHelper<Group, GroupService>( this ).Create( value );
        }

        /// <summary>
        /// Performs a full update of the item. All property values must be
        /// specified.
        /// </summary>
        /// <param name="key">The key as either an Id, Guid or IdKey value.</param>
        /// <param name="value">The item that represents all the new values.</param>
        /// <returns>An empty response.</returns>
        [HttpPut]
        [Authenticate]
        [Route( "{key}" )]
        [ProducesResponseType( HttpStatusCode.NoContent )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "0d91b319-bc43-4ab7-b6af-17ad21b1dec6" )]
        public IActionResult PutItem( string key, [FromBody] Group value )
        {
            return new RestApiHelper<Group, GroupService>( this ).Update( key, value );
        }

        /// <summary>
        /// Performs a partial update of the item. Only specified property keys
        /// will be updated.
        /// </summary>
        /// <param name="key">The key as either an Id, Guid or IdKey value.</param>
        /// <param name="values">An object that identifies the properties and values to be updated.</param>
        /// <returns>An empty response.</returns>
        [HttpPatch]
        [Authenticate]
        [Route( "{key}" )]
        [ProducesResponseType( HttpStatusCode.NoContent )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "38ddc3c7-1a05-4460-acfc-71b604d1b7ad" )]
        public IActionResult PatchItem( string key, [FromBody] Dictionary<string, object> values )
        {
            return new RestApiHelper<Group, GroupService>( this ).Patch( key, values );
        }

        /// <summary>
        /// Deletes a single item from the database.
        /// </summary>
        /// <param name="key">The key as either an Id, Guid or IdKey value.</param>
        /// <returns>An empty response.</returns>
        [HttpDelete]
        [Authenticate]
        [Route( "{key}" )]
        [ProducesResponseType( HttpStatusCode.NoContent )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "b740d84c-6b71-4860-b380-d5ecf5d405b8" )]
        public IActionResult DeleteItem( string key )
        {
            return new RestApiHelper<Group, GroupService>( this ).Delete( key );
        }

        /// <summary>
        /// Gets all the attribute values for the specified item.
        /// </summary>
        /// <param name="key">The key as either an Id, Guid or IdKey value.</param>
        /// <returns>An array of objects that represent all the attribute values.</returns>
        [HttpGet]
        [Authenticate]
        [Route( "{key}/attributevalues" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( Dictionary<string, AttributeValueRestBag> ) )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "3a03bd5a-a32a-4ac5-bae2-cda1a3141d11" )]
        public IActionResult GetAttributeValues( string key )
        {
            return new RestApiHelper<Group, GroupService>( this ).GetAttributeValues( key );
        }

        /// <summary>
        /// Performs a partial update of attribute values for the item. Only
        /// attributes specified will be updated.
        /// </summary>
        /// <param name="key">The key as either an Id, Guid or IdKey value.</param>
        /// <param name="values">An object that identifies the attribute keys and raw values to be updated.</param>
        /// <returns>An empty response.</returns>
        [HttpPatch]
        [Authenticate]
        [Route( "{key}/attributevalues" )]
        [ProducesResponseType( HttpStatusCode.NoContent )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "a4fa9111-c524-4a24-aa1b-b9587ed988f0" )]
        public IActionResult PatchAttributeValues( string key, [FromBody] Dictionary<string, string> values )
        {
            return new RestApiHelper<Group, GroupService>( this ).PatchAttributeValues( key, values );
        }

        [HttpPost]
        [Authenticate]
        [Route( "search" )]
        [SystemGuid.RestActionGuid( "2568b739-a6c9-4d91-9bed-a3485c51954b" )]
        public IActionResult PostSearch( [FromBody] SearchQueryBag query )
        {
            using ( var rockContext = new RockContext() )
            {
                bool enforceEntitySecurity = false;
                var service = new GroupService( rockContext );
                var config = _parsingConfig;

                IQueryable<Group> qry = service.Queryable();

                // Simulate the system where clause.
                if ( "".IsNotNullOrWhiteSpace() )
                {
                    qry = qry.Where( config, "Id != 0" );
                }

                //  Simulate security checks.
                if ( enforceEntitySecurity )
                {
                    qry = qry.ToList().AsQueryable();
                }

                IQueryable resultQry = qry;

                // Simulate system group clause.
                if ( "".IsNotNullOrWhiteSpace() )
                {
                    resultQry = resultQry.GroupBy( config, "" );
                }

                // Simulate system select clause.
                if ( "".IsNotNullOrWhiteSpace() )
                {
                    resultQry = resultQry.Select( config, "new { Id, Name, Guid }" );
                }

                if ( query.Where.IsNotNullOrWhiteSpace() )
                {
                    qry = qry.Where( config, query.Where );
                }

                if ( query.Group.IsNotNullOrWhiteSpace() )
                {
                    resultQry = resultQry.GroupBy( config, query.Group );
                }

                if ( query.Select.IsNotNullOrWhiteSpace() )
                {
                    resultQry = resultQry.Select( config, query.Select );
                }

                // Apply either the system or user order by clause.
                if ( query.Order.IsNotNullOrWhiteSpace() )
                {
                    resultQry = resultQry.OrderBy( config, query.Order );
                }

                if ( query.Skip.HasValue )
                {
                    resultQry = resultQry.Skip( query.Skip.Value );
                }

                if ( query.Take.HasValue )
                {
                    resultQry = resultQry.Take( query.Take.Value );
                }

                if ( _searchFormatter == null )
                {
                    var formatter = Utility.ApiPickerJsonMediaTypeFormatter.CreateV2Formatter();
                    if ( formatter.SerializerSettings.ContractResolver is Newtonsoft.Json.Serialization.DefaultContractResolver defaultContractResolver )
                    {
                        defaultContractResolver.NamingStrategy.ProcessDictionaryKeys = true;
                    }

                    _searchFormatter = formatter;
                }

                return Content( HttpStatusCode.OK, resultQry.ToDynamicList(), _searchFormatter );
            }
        }
        private static System.Net.Http.Formatting.JsonMediaTypeFormatter _searchFormatter;
        private readonly static ParsingConfig _parsingConfig = new ParsingConfig
        {
            DisableMemberAccessToIndexAccessorFallback = true
        };
    }

    public class SearchQueryBag
    {
        public string Where { get; set; }

        public string Select { get; set; }

        public string Group { get; set; }

        public string Order { get; set; }

        public int? Take { get; set; }

        public int? Skip { get; set; }
    }
}

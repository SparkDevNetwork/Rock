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
using System.Net;

using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Web.Cache;
using Rock.ViewModels.Core;

using System.Web.Http.Results;

#if WEBFORMS
using System.Web.Http;

using IActionResult = System.Web.Http.IHttpActionResult;
using RouteAttribute = System.Web.Http.RouteAttribute;
#endif

namespace Rock.Rest.v2.Models.Data
{
    /// <summary>
    /// Provides data API endpoints for Groups.
    /// </summary>
    [RoutePrefix( "api/v2/models/groups/data" )]
    [SecurityAction( "UnrestrictedView", "Allows viewing entities regardless of per-entity security authorization." )]
    [SecurityAction( "UnrestrictedEdit", "Allows editing entities regardless of per-entity security authorization." )]
    [Rock.SystemGuid.RestControllerGuid( "999c734f-e206-4a25-8145-1213b4ffd8a9" )]
    public partial class GroupsDataController : ApiControllerBase
    {
        /// <summary>
        /// Gets a single item from the database.
        /// </summary>
        /// <param name="id">The identifier as either an Id, Guid or IdKey value.</param>
        /// <returns>The requested item.</returns>
        [HttpGet]
        [Authenticate]
        [Route( "{id}" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( Group ) )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound   )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "54736156-88af-461a-b311-e8dd48b24019" )]
        public IActionResult GetItem( string id )
        {
            return new RestApiHelper<Group, GroupService>( this ).Get( id );
        }

        /// <summary>
        /// Creates a new item in the database.
        /// </summary>
        /// <param name="value">The item to be created.</param>
        /// <returns>An object that contains the new identifier values.</returns>
        [HttpPost]
        [Authenticate]
        [Route( "" )]
        [ProducesResponseType( HttpStatusCode.Created, Type = typeof( CreatedAtResultBag ) )]
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
        /// <param name="id">The identifier as either an Id, Guid or IdKey value.</param>
        /// <param name="value">The item that represents all the new values.</param>
        /// <returns>An empty response.</returns>
        [HttpPut]
        [Authenticate]
        [Route( "{id}" )]
        [ProducesResponseType( HttpStatusCode.NoContent )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "0d91b319-bc43-4ab7-b6af-17ad21b1dec6" )]
        public IActionResult PutItem( string id, [FromBody] Group value )
        {
            return new RestApiHelper<Group, GroupService>( this ).Update( id, value );
        }

        /// <summary>
        /// Performs a partial update of the item. Only specified property keys
        /// will be updated.
        /// </summary>
        /// <param name="id">The identifier as either an Id, Guid or IdKey value.</param>
        /// <param name="values">An object that identifies the properties and values to be updated.</param>
        /// <returns>An empty response.</returns>
        [HttpPatch]
        [Authenticate]
        [Route( "{id}" )]
        [ProducesResponseType( HttpStatusCode.NoContent )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "38ddc3c7-1a05-4460-acfc-71b604d1b7ad" )]
        public IActionResult PatchItem( string id, [FromBody] Dictionary<string, object> values )
        {
            return new RestApiHelper<Group, GroupService>( this ).Patch( id, values );
        }

        /// <summary>
        /// Deletes a single item from the database.
        /// </summary>
        /// <param name="id">The identifier as either an Id, Guid or IdKey value.</param>
        /// <returns>An empty response.</returns>
        [HttpDelete]
        [Authenticate]
        [Route( "{id}" )]
        [ProducesResponseType( HttpStatusCode.NoContent )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "b740d84c-6b71-4860-b380-d5ecf5d405b8" )]
        public IActionResult DeleteItem( string id )
        {
            return new RestApiHelper<Group, GroupService>( this ).Delete( id );
        }

        /// <summary>
        /// Gets all the attribute values for the specified item.
        /// </summary>
        /// <param name="id">The identifier as either an Id, Guid or IdKey value.</param>
        /// <returns>An array of objects that represent all the attribute values.</returns>
        [HttpGet]
        [Authenticate]
        [Route( "{id}/attributevalues" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( Dictionary<string, AttributeValueRestBag> ) )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "3a03bd5a-a32a-4ac5-bae2-cda1a3141d11" )]
        public IActionResult GetAttributeValues( string id )
        {
            return new RestApiHelper<Group, GroupService>( this ).GetAttributeValues( id );
        }

        /// <summary>
        /// Performs a partial update of attribute values for the item. Only
        /// attributes specified will be updated.
        /// </summary>
        /// <param name="id">The identifier as either an Id, Guid or IdKey value.</param>
        /// <param name="values">An object that identifies the attribute keys and raw values to be updated.</param>
        /// <returns>An empty response.</returns>
        [HttpPatch]
        [Authenticate]
        [Route( "{id}/attributevalues" )]
        [ProducesResponseType( HttpStatusCode.NoContent )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "a4fa9111-c524-4a24-aa1b-b9587ed988f0" )]
        public IActionResult PatchAttributeValues( string id, [FromBody] Dictionary<string, string> values )
        {
            return new RestApiHelper<Group, GroupService>( this ).PatchAttributeValues( id, values );
        }

        [HttpPost]
        [Authenticate]
        [Route( "search/{searchKey}" )]
        [SystemGuid.RestActionGuid( "2568b739-a6c9-4d91-9bed-a3485c51954b" )]
        public IActionResult PostSearch( [FromBody] EntitySearchQueryBag query, string searchKey )
        {
            try
            {
                var entityType = EntityTypeCache.Get<Group>();
                var entitySearch = EntitySearchCache.GetByEntityTypeAndKey( entityType, searchKey );

                if ( entitySearch == null )
                {
                    return NotFound(); // TODO: "Search key not found."
                }

                if ( !entitySearch.IsAuthorized( Rock.Security.Authorization.VIEW, RockRequestContext.CurrentPerson ) )
                {
                    return NotFound(); // TODO: Not authorized.
                }

                var results = EntitySearchService.GetSearchResults( entitySearch, query );

                if ( _searchFormatter == null )
                {
                    var formatter = Utility.ApiPickerJsonMediaTypeFormatter.CreateV2Formatter();
                    if ( formatter.SerializerSettings.ContractResolver is Newtonsoft.Json.Serialization.DefaultContractResolver defaultContractResolver )
                    {
                        defaultContractResolver.NamingStrategy.ProcessDictionaryKeys = true;
                    }

                    _searchFormatter = formatter;
                }

                return Content( HttpStatusCode.OK, results, _searchFormatter );
            }
            catch ( System.Exception ex )
            {
                ExceptionLogService.LogException( ex );
                var error = new HttpError( ex.Message );

                return new NegotiatedContentResult<HttpError>( HttpStatusCode.InternalServerError, error, this );
            }
        }

        private static System.Net.Http.Formatting.JsonMediaTypeFormatter _searchFormatter;
    }
}

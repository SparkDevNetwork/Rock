//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
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

using Microsoft.AspNetCore.Mvc;

using Rock.Rest.Filters;
using Rock.Security;
using Rock.ViewModels.Core;
using Rock.ViewModels.Rest.Models;

namespace Rock.Rest.v2.Models
{
#if WEBFORMS
    using FromBodyAttribute = System.Web.Http.FromBodyAttribute;
    using IActionResult = System.Web.Http.IHttpActionResult;
    using RoutePrefixAttribute = System.Web.Http.RoutePrefixAttribute;
    using RouteAttribute = System.Web.Http.RouteAttribute;
    using HttpGetAttribute = System.Web.Http.HttpGetAttribute;
    using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
    using HttpPutAttribute = System.Web.Http.HttpPutAttribute;
    using HttpPatchAttribute = System.Web.Http.HttpPatchAttribute;
    using HttpDeleteAttribute = System.Web.Http.HttpDeleteAttribute;
#endif

    /// <summary>
    /// Provides data API endpoints for Communication Flow Instance Recipients.
    /// </summary>
    [RoutePrefix( "api/v2/models/communicationflowinstancerecipients" )]
    [Rock.SystemGuid.RestControllerGuid( "0539c0c4-1af7-5c99-98b9-3a9f8b2065d3" )]
    public partial class CommunicationFlowInstanceRecipientsController : ApiControllerBase
    {
        /// <summary>
        /// Gets a single item from the database.
        /// </summary>
        /// <param name="id">The identifier as either an Id, Guid or IdKey value.</param>
        /// <returns>The requested item.</returns>
        [HttpGet]
        [Route( "{id}" )]
        [Authenticate]
        [Secured( Security.Authorization.EXECUTE_READ )]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( Rock.Model.CommunicationFlowInstanceRecipient ) )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "ce4e849e-1918-5e16-a793-8c1d92d44c79" )]
        public IActionResult GetItem( string id )
        {
            var helper = new CrudEndpointHelper<Rock.Model.CommunicationFlowInstanceRecipient, Rock.Model.CommunicationFlowInstanceRecipientService>( this );

            helper.IsSecurityIgnored = IsCurrentPersonAuthorized( Security.Authorization.EXECUTE_UNRESTRICTED_READ );

            return helper.Get( id );
        }

        /// <summary>
        /// Creates a new item in the database.
        /// </summary>
        /// <param name="value">The item to be created.</param>
        /// <returns>An object that contains the new identifier values.</returns>
        [HttpPost]
        [Route( "" )]
        [Authenticate]
        [Secured( Security.Authorization.EXECUTE_WRITE )]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_UNRESTRICTED_READ )]
        [ProducesResponseType( HttpStatusCode.Created, Type = typeof( CreatedAtResponseBag ) )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "79238b8e-96aa-5729-b883-c9a313fcdb88" )]
        public IActionResult PostItem( [FromBody] Rock.Model.CommunicationFlowInstanceRecipient value )
        {
            var helper = new CrudEndpointHelper<Rock.Model.CommunicationFlowInstanceRecipient, Rock.Model.CommunicationFlowInstanceRecipientService>( this );

            helper.IsSecurityIgnored = IsCurrentPersonAuthorized( Security.Authorization.EXECUTE_UNRESTRICTED_WRITE );

            return helper.Create( value );
        }

        /// <summary>
        /// Performs a full update of the item. All property values must be
        /// specified.
        /// </summary>
        /// <param name="id">The identifier as either an Id, Guid or IdKey value.</param>
        /// <param name="value">The item that represents all the new values.</param>
        /// <returns>An empty response.</returns>
        [HttpPut]
        [Route( "{id}" )]
        [Authenticate]
        [Secured( Security.Authorization.EXECUTE_WRITE )]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_UNRESTRICTED_READ )]
        [ProducesResponseType( HttpStatusCode.NoContent )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "ceb0346b-2f5b-5834-82f2-49c3b424e1c7" )]
        public IActionResult PutItem( string id, [FromBody] Rock.Model.CommunicationFlowInstanceRecipient value )
        {
            var helper = new CrudEndpointHelper<Rock.Model.CommunicationFlowInstanceRecipient, Rock.Model.CommunicationFlowInstanceRecipientService>( this );

            helper.IsSecurityIgnored = IsCurrentPersonAuthorized( Security.Authorization.EXECUTE_UNRESTRICTED_WRITE );

            return helper.Update( id, value );
        }

        /// <summary>
        /// Performs a partial update of the item. Only specified property keys
        /// will be updated.
        /// </summary>
        /// <param name="id">The identifier as either an Id, Guid or IdKey value.</param>
        /// <param name="values">An object that identifies the properties and values to be updated.</param>
        /// <returns>An empty response.</returns>
        [HttpPatch]
        [Route( "{id}" )]
        [Authenticate]
        [Secured( Security.Authorization.EXECUTE_WRITE )]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_UNRESTRICTED_READ )]
        [ProducesResponseType( HttpStatusCode.NoContent )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "b434a21d-76ed-5ce0-b535-5732d26cd729" )]
        public IActionResult PatchItem( string id, [FromBody] Dictionary<string, object> values )
        {
            var helper = new CrudEndpointHelper<Rock.Model.CommunicationFlowInstanceRecipient, Rock.Model.CommunicationFlowInstanceRecipientService>( this );

            helper.IsSecurityIgnored = IsCurrentPersonAuthorized( Security.Authorization.EXECUTE_UNRESTRICTED_WRITE );

            return helper.Patch( id, values );
        }

        /// <summary>
        /// Deletes a single item from the database.
        /// </summary>
        /// <param name="id">The identifier as either an Id, Guid or IdKey value.</param>
        /// <returns>An empty response.</returns>
        [HttpDelete]
        [Route( "{id}" )]
        [Authenticate]
        [Secured( Security.Authorization.EXECUTE_WRITE )]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_UNRESTRICTED_READ )]
        [ProducesResponseType( HttpStatusCode.NoContent )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "99799df8-fbb5-5224-a5a4-c86fe4f8b34e" )]
        public IActionResult DeleteItem( string id )
        {
            var helper = new CrudEndpointHelper<Rock.Model.CommunicationFlowInstanceRecipient, Rock.Model.CommunicationFlowInstanceRecipientService>( this );

            helper.IsSecurityIgnored = IsCurrentPersonAuthorized( Security.Authorization.EXECUTE_UNRESTRICTED_WRITE );

            return helper.Delete( id );
        }

        /// <summary>
        /// Gets all the attribute values for the specified item.
        /// </summary>
        /// <param name="id">The identifier as either an Id, Guid or IdKey value.</param>
        /// <returns>An array of objects that represent all the attribute values.</returns>
        [HttpGet]
        [Route( "{id}/attributevalues" )]
        [Authenticate]
        [Secured( Security.Authorization.EXECUTE_READ )]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( Dictionary<string, ModelAttributeValueBag> ) )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "9c6e78bb-78c7-564d-8f85-45dd4e605bb2" )]
        public IActionResult GetAttributeValues( string id )
        {
            var helper = new CrudEndpointHelper<Rock.Model.CommunicationFlowInstanceRecipient, Rock.Model.CommunicationFlowInstanceRecipientService>( this );

            helper.IsSecurityIgnored = IsCurrentPersonAuthorized( Security.Authorization.EXECUTE_UNRESTRICTED_READ );

            return helper.GetAttributeValues( id );
        }

        /// <summary>
        /// Performs a partial update of attribute values for the item. Only
        /// attributes specified will be updated.
        /// </summary>
        /// <param name="id">The identifier as either an Id, Guid or IdKey value.</param>
        /// <param name="values">An object that identifies the attribute keys and raw values to be updated.</param>
        /// <returns>An empty response.</returns>
        [HttpPatch]
        [Route( "{id}/attributevalues" )]
        [Authenticate]
        [Secured( Security.Authorization.EXECUTE_WRITE )]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_UNRESTRICTED_READ )]
        [ProducesResponseType( HttpStatusCode.NoContent )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "47e876b2-fc00-5e5e-945f-636b6b4e7232" )]
        public IActionResult PatchAttributeValues( string id, [FromBody] Dictionary<string, string> values )
        {
            var helper = new CrudEndpointHelper<Rock.Model.CommunicationFlowInstanceRecipient, Rock.Model.CommunicationFlowInstanceRecipientService>( this );

            helper.IsSecurityIgnored = IsCurrentPersonAuthorized( Security.Authorization.EXECUTE_UNRESTRICTED_WRITE );

            return helper.PatchAttributeValues( id, values );
        }

        /// <summary>
        /// Performs a search of items using the specified user query.
        /// </summary>
        /// <param name="query">Query options to be applied.</param>
        /// <returns>An array of objects returned by the query.</returns>
        [HttpPost]
        [Route( "search" )]
        [Authenticate]
        [Secured( Security.Authorization.EXECUTE_UNRESTRICTED_READ )]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_READ, Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( object ) )]
        [SystemGuid.RestActionGuid( "86f04306-abe7-581c-80ca-5d1d483147c9" )]
        public IActionResult PostSearch( [FromBody] EntitySearchQueryBag query )
        {
            var helper = new CrudEndpointHelper<Rock.Model.CommunicationFlowInstanceRecipient, Rock.Model.CommunicationFlowInstanceRecipientService>( this );

            return helper.Search( query );
        }

        /// <summary>
        /// Performs a search of items using the specified system query.
        /// </summary>
        /// <param name="searchKey">The key that identifies the entity search query to execute.</param>
        /// <returns>An array of objects returned by the query.</returns>
        [HttpGet]
        [Route( "search/{searchKey}" )]
        [Authenticate]
        [Secured( Security.Authorization.EXECUTE_READ )]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( object ) )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "fa7da4e3-b463-5470-85e3-1ef5033047b1" )]
        public IActionResult GetSearchByKey( string searchKey )
        {
            var helper = new CrudEndpointHelper<Rock.Model.CommunicationFlowInstanceRecipient, Rock.Model.CommunicationFlowInstanceRecipientService>( this );

            helper.IsSecurityIgnored = IsCurrentPersonAuthorized( Security.Authorization.EXECUTE_UNRESTRICTED_READ );

            return helper.Search( searchKey, null );
        }

        /// <summary>
        /// Performs a search of items using the specified system query.
        /// </summary>
        /// <param name="query">Additional query refinement options to be applied.</param>
        /// <param name="searchKey">The key that identifies the entity search query to execute.</param>
        /// <returns>An array of objects returned by the query.</returns>
        [HttpPost]
        [Route( "search/{searchKey}" )]
        [Authenticate]
        [Secured( Security.Authorization.EXECUTE_READ )]
        [ExcludeSecurityActions( Security.Authorization.EXECUTE_WRITE, Security.Authorization.EXECUTE_UNRESTRICTED_WRITE )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( object ) )]
        [ProducesResponseType( HttpStatusCode.BadRequest )]
        [ProducesResponseType( HttpStatusCode.NotFound )]
        [ProducesResponseType( HttpStatusCode.Unauthorized )]
        [SystemGuid.RestActionGuid( "8d0bb65b-c1ab-5055-be5e-1217d4910f67" )]
        public IActionResult PostSearchByKey( string searchKey, [FromBody] EntitySearchQueryBag query )
        {
            var helper = new CrudEndpointHelper<Rock.Model.CommunicationFlowInstanceRecipient, Rock.Model.CommunicationFlowInstanceRecipientService>( this );

            helper.IsSecurityIgnored = IsCurrentPersonAuthorized( Security.Authorization.EXECUTE_UNRESTRICTED_READ );

            return helper.Search( searchKey, query );
        }
    }
}

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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Users REST API
    /// </summary>
    public partial class UserLoginsController
    {
        /// <summary>
        /// Tests if a username is available
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpGet]
        [System.Web.Http.Route( "api/userlogins/available/{username}" )]
        [System.Web.Http.Route( "api/userlogins/available" )]
        [Rock.SystemGuid.RestActionGuid( "037C3806-0A80-4029-ACAE-269DFC702EA8" )]
        [Authenticate, Secured]
        public bool Available( string username )
        {
            return ( ( UserLoginService ) Service ).Exists( username ) == false;
        }

        /// <summary>
        /// Gets the user logins the current person is authorized to view.
        /// </summary>
        /// <returns>A queryable of user logins, empty when the current person is not authorized to view them.</returns>
        [Authenticate, Secured]
        [EnableQuery]
        public override IQueryable<UserLogin> Get()
        {
            return FilterByEntityTypeAuthorization( base.Get() );
        }

        /// <summary>
        /// Gets the user login with the specified identifier when the current person is authorized to view it.
        /// </summary>
        /// <param name="id">The identifier of the user login.</param>
        /// <returns>The user login.</returns>
        /// <exception cref="HttpResponseException">Thrown with a 404 status when the user login does not exist or the current person is not authorized to view it.</exception>
        [Authenticate, Secured]
        [ActionName( "GetById" )]
        public override UserLogin GetById( int id )
        {
            EnsureEntityTypeAuthorizedOrNotFound();

            return base.GetById( id );
        }

        /// <summary>
        /// Gets the user login with the specified key when the current person is authorized to view it.
        /// </summary>
        /// <param name="key">The identifier of the user login.</param>
        /// <returns>The user login.</returns>
        /// <exception cref="HttpResponseException">Thrown with a 404 status when the user login does not exist or the current person is not authorized to view it.</exception>
        [Authenticate, Secured]
        [EnableQuery]
        public override UserLogin Get( [FromODataUri] int key )
        {
            EnsureEntityTypeAuthorizedOrNotFound();

            return base.Get( key );
        }

        /// <summary>
        /// Gets the user logins that have a particular attribute value and that the current person is authorized to view.
        /// </summary>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="value">The attribute value to match.</param>
        /// <param name="caseSensitive">If set to <c>true</c>, the value is matched case sensitively.</param>
        /// <returns>A queryable of matching user logins, empty when the current person is not authorized to view them.</returns>
        [Authenticate, Secured]
        [ActionName( "GetByAttributeValue" )]
        [EnableQuery]
        public override IQueryable<UserLogin> GetByAttributeValue( [FromUri] int? attributeId = null, [FromUri] string attributeKey = null, [FromUri] string value = null, [FromUri] bool caseSensitive = false )
        {
            return FilterByEntityTypeAuthorization( base.GetByAttributeValue( attributeId, attributeKey, value, caseSensitive ) );
        }

        /// <summary>
        /// Gets the user logins in the selected data view that the current person is authorized to view.
        /// </summary>
        /// <param name="id">The data view identifier.</param>
        /// <returns>A queryable of user logins, empty when the current person is not authorized to view them.</returns>
        [Authenticate, Secured]
        [ActionName( "DataView" )]
        [EnableQuery]
        public override IQueryable<UserLogin> GetDataView( int id )
        {
            return FilterByEntityTypeAuthorization( base.GetDataView( id ) );
        }

        /// <summary>
        /// Determines if the user login is in the data view and the current person is authorized to view it.
        /// </summary>
        /// <param name="dataViewId">The data view identifier.</param>
        /// <param name="entityId">The user login identifier.</param>
        /// <returns><c>true</c> if the user login is in the data view and the current person is authorized to view it; otherwise <c>false</c>.</returns>
        [Authenticate, Secured]
        [ActionName( "InDataView" )]
        [EnableQuery]
        [HttpGet]
        public override bool InDataView( int dataViewId, int entityId )
        {
            return IsAuthorizedToViewEntityType() && base.InDataView( dataViewId, entityId );
        }

        /// <summary>
        /// Gets the user logins followed by the specified person that the current person is authorized to view.
        /// </summary>
        /// <param name="personId">The identifier of the person whose followed user logins are returned.</param>
        /// <param name="personAliasId">The alias identifier of the person whose followed user logins are returned, used when <paramref name="personId"/> is not provided.</param>
        /// <returns>A queryable of followed user logins, empty when the current person is not authorized to view them.</returns>
        [Authenticate, Secured]
        [ActionName( "FollowedItems" )]
        [EnableQuery]
        public override IQueryable<UserLogin> GetFollowedItems( int? personId = null, int? personAliasId = null )
        {
            return FilterByEntityTypeAuthorization( base.GetFollowedItems( personId, personAliasId ) );
        }

        /// <summary>
        /// Posts the specified value.
        /// POST using <see cref="Rock.Model.UserLoginWithPlainTextPassword"/> and set PlainTextPassword to set a password.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [HttpPost]
        [Authenticate, Secured]
        public override System.Net.Http.HttpResponseMessage Post( UserLogin value )
        {
            if ( ( ( UserLoginService ) Service ).Exists( value.UserName ) )
            {
                return ControllerContext.Request.CreateResponse( HttpStatusCode.Conflict, "The username already exists." );
            }

            // Store current value, as SetPassword() will always set this to false.
            bool? passwordChangeRequired = value.IsPasswordChangeRequired;

            SetCredentialsFromRest( value );

            if ( passwordChangeRequired != null )
            {
                value.IsPasswordChangeRequired = passwordChangeRequired;
            }

            return base.Post( value );
        }

        /// <summary>
        /// Puts the specified identifier.
        /// PUT using <see cref="Rock.Model.UserLoginWithPlainTextPassword"/> and set PlainTextPassword to set a password.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="value">The value.</param>
        [Authenticate, Secured]
        public override void Put( int id, UserLogin value )
        {
            // Store current value, as SetPassword() will always set this to false.
            bool? passwordChangeRequired = value.IsPasswordChangeRequired;

            SetCredentialsFromRest( value );

            if ( passwordChangeRequired != null )
            {
                value.IsPasswordChangeRequired = passwordChangeRequired;
            }

            base.Put( id, value );
        }

        /// <summary>
        /// Applies the plain text password and API key from the request body to the user login when the caller supplied them.
        /// </summary>
        /// <param name="value">The user login deserialized from the request body.</param>
        private void SetCredentialsFromRest( UserLogin value )
        {
            /*
                10/13/26 - JMH

                Neither credential is part of the REST contract, so the model binder never reads them
                and responses never return them. Read them from the raw body so callers can still set them.

                Reason: Keep credentials write-only over REST.
            */
            JObject body = null;

            this.ActionContext.Request.Content.ReadAsStreamAsync().ContinueWith( a =>
            {
                a.Result.Position = 0;
                StreamReader sr = new StreamReader( a.Result );

                body = JsonConvert.DeserializeObject<JObject>( sr.ReadToEnd() );
            } ).Wait();

            var plainTextPassword = body?.ToObject<UserLoginWithPlainTextPassword>()?.PlainTextPassword;
            if ( !string.IsNullOrWhiteSpace( plainTextPassword ) )
            {
                ( this.Service as UserLoginService ).SetPassword( value, plainTextPassword );
                value.IsLockedOut = false; // unlock the user account if they reset their password.
            }

            var apiKey = body?.GetValue( nameof( UserLogin.ApiKey ), StringComparison.OrdinalIgnoreCase )?.ToString();
            if ( !string.IsNullOrWhiteSpace( apiKey ) )
            {
                value.ApiKey = apiKey;
            }
        }

        /// <summary>
        /// Determines whether the current person is authorized to view the user login entity type.
        /// </summary>
        /// <returns><c>true</c> if the current person has an explicit view rule on the user login entity type, or can edit user logins when no such rule applies; otherwise <c>false</c>.</returns>
        private bool IsAuthorizedToViewEntityType()
        {
            /*
                10/14/26 - JMH

                Individual user logins don't carry their own security rules, so every record resolves
                to the same answer as the entity type. Checking once keeps the result a database query
                that OData options can still be applied to.

                The global default allows view for everyone, so a plain view check never restricts anyone.
                Only view rules set directly on the entity type count. Without one, fall back to edit,
                which matches who can already create and update user logins.

                Reason: Check security once so queries stay composable with OData, and don't let the global default view grant expose user logins.
            */
            var entityTypeSecurity = new UserLogin();
            var person = GetPerson();

            return Rock.Security.Authorization.AuthorizedForEntity( entityTypeSecurity, Rock.Security.Authorization.VIEW, person, false )
                ?? entityTypeSecurity.IsAuthorized( Rock.Security.Authorization.EDIT, person );
        }

        /// <summary>
        /// Limits the query based on whether the current person is authorized to view the user login entity type.
        /// </summary>
        /// <param name="query">The user login query to limit.</param>
        /// <returns>The original query when the current person is authorized to view the entity type, otherwise an empty query.</returns>
        private IQueryable<UserLogin> FilterByEntityTypeAuthorization( IQueryable<UserLogin> query )
        {
            return IsAuthorizedToViewEntityType() ? query : Enumerable.Empty<UserLogin>().AsQueryable();
        }

        /// <summary>
        /// Ensures the current person is authorized to view the user login entity type.
        /// </summary>
        /// <exception cref="HttpResponseException">Thrown with a 404 status when the current person is not authorized to view the user login entity type.</exception>
        private void EnsureEntityTypeAuthorizedOrNotFound()
        {
            if ( !IsAuthorizedToViewEntityType() )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }
        }
    }
}
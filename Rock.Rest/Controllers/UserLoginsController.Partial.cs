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
using System.Net;
using System.Net.Http;
using System.Web.Http;

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
    }
}
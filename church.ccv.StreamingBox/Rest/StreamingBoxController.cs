// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.IO;
using System.Net.Http;
using System.Text;
using Rock.Model;
using Newtonsoft.Json;
using System.Net;
using System.Web.Http;
using System.Web.Routing;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Data;

namespace chuch.ccv.StreamingBox.Rest
{
    public class StreamingBoxController : Rock.Rest.ApiControllerBase
    {
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route( "api/StreamingBox/Login" )]
        [Authenticate, Secured]
        public HttpResponseMessage Login( [FromBody]LoginParameters loginParameters )
        {
            // default to an internal error
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            HttpContent httpContent = null;

            do
            {
                RockContext rockContext = new RockContext( );

                // require login parameters
                if( loginParameters == null ) break;

                // verify their user login
                var userLoginService = new UserLoginService( new Rock.Data.RockContext() );
                var userLogin = userLoginService.GetByUserName( loginParameters.Username );

                if ( userLogin == null || userLogin.EntityType == null ) { statusCode = HttpStatusCode.Unauthorized; break; }


                // verify their password
                var component = AuthenticationContainer.GetComponent(userLogin.EntityType.Name);
                if ( component == null || component.IsActive == false ) { statusCode = HttpStatusCode.Unauthorized; break; }
                
                if ( component.Authenticate( userLogin, loginParameters.Password ) == false ) { statusCode = HttpStatusCode.Unauthorized; break; }
                
                
                // ensure there's a person associated with this login.
                if ( userLogin.PersonId.HasValue == false ) { statusCode = HttpStatusCode.Unauthorized; break; }
                
                // if this account hasn't been confirmed yet, do not let them login. This prevents someone from
                // trying to do a password reset on a stolen email address.
                if( userLogin.IsConfirmed == false ) { statusCode = HttpStatusCode.Unauthorized; break; }

                // get their alias ID, as that's needed
                var personAliasService = new PersonAliasService( rockContext );
                if( personAliasService == null ) break;
                
                int? personAliasId = personAliasService.GetPrimaryAliasId( userLogin.PersonId.Value );
                if( personAliasId.HasValue == false ) break;
                
                // all good! build and return the response
                httpContent = new StringContent( JsonConvert.SerializeObject( new {  PersonAliasId = personAliasId } ), Encoding.UTF8, "application/json" );
                statusCode = HttpStatusCode.OK;
            }
            while( false );
            
            // build and return the response
            HttpResponseMessage response = new HttpResponseMessage( ) { StatusCode = statusCode, Content = httpContent };
            return response;
        }
    }
}

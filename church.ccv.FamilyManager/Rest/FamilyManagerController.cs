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
using church.ccv.FamilyManager;
using church.ccv.FamilyManager.Models;
using System.Web.Http;
using System.Web.Routing;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Data;
using Rock;
using System.Linq;
using Rock.Rest.Controllers;
using System.Collections.Generic;
using Rock.Web.Cache;

namespace chuch.ccv.FamilyManager.Rest
{
    public class FamilyManagerController : Rock.Rest.ApiControllerBase
    {
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route( "api/FamilyManager/LaunchData" )]
        [Authenticate, Secured]
        public HttpResponseMessage GetLaunchData(  )
        {
            CoreData coreData = Launch.BuildCoreData( );

            StringContent restContent = new StringContent( JsonConvert.SerializeObject( coreData ), Encoding.UTF8, "application/json" );
            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = restContent
            };
        }
        
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route( "api/FamilyManager/Login" )]
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
                

                // verify they can use family manager
                GroupService groupService = new GroupService( rockContext );
                var familyManagerGroup = groupService.GetByGuid( Guids.FAMILY_MANAGER_AUTHORIZED_GROUP.AsGuid( ) );

                // if they're a member of the "Family Manager" group, they're good.
                var groupMember = familyManagerGroup.Members.Where( m => m.PersonId == userLogin.PersonId ).SingleOrDefault( );
                if ( groupMember == null ) { statusCode = HttpStatusCode.Unauthorized; break; }

                // get their alias ID, as that's needed
                var personAliasService = new PersonAliasService( rockContext );
                if( personAliasService == null ) break;
                
                int? personAliasId = personAliasService.GetPrimaryAliasId( userLogin.PersonId.Value );
                if( personAliasId.HasValue == false ) break;
                
                // all good! build and return the response
                httpContent = new StringContent( JsonConvert.SerializeObject( new {  PersonAliasId = personAliasId } ), Encoding.UTF8, "application/json" );
                statusCode = HttpStatusCode.Created;
            }
            while( false );
            
            // build and return the response
            HttpResponseMessage response = new HttpResponseMessage( ) { StatusCode = statusCode, Content = httpContent };
            return response;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route( "api/FamilyManager/GetFamily" )]
        [Authenticate, Secured]
        public HttpResponseMessage GetFamily( int familyId )
        {
            HttpStatusCode statusCode;
            HttpContent httpContent = null;
            
            Core.FamilyReturnObject familyReturnObj = Core.GetFamily( familyId, out statusCode );
            if( familyReturnObj != null )
            {
                // package it up and we're done
                httpContent = new StringContent( JsonConvert.SerializeObject( familyReturnObj ), Encoding.UTF8, "application/json" );
            }
            
            // build and return the response
            HttpResponseMessage response = new HttpResponseMessage( ) { StatusCode = statusCode, Content = httpContent };
            return response;
        }

        [HttpGet]
        [System.Web.Http.Route( "api/FamilyManager/GetPersonForEdit" )]
        [Authenticate, Secured]
        public HttpResponseMessage GetPersonForEdit( int personId )
        {
            // default to an internal error
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            HttpContent httpContent = null;
            
            Core.PersonReturnObject personReturnObj = Core.GetPersonForEdit( personId, out statusCode );
            if( personReturnObj != null )
            {
                // package it up and we're done
                httpContent = new StringContent( JsonConvert.SerializeObject( personReturnObj ), Encoding.UTF8, "application/json" );
            }

            // build and return the response
            HttpResponseMessage response = new HttpResponseMessage( ) { StatusCode = statusCode, Content = httpContent };
            return response;
        }

        /// <summary>
        /// Supports: 
        /// Editing Existing Person in Existing Family
        /// Adding New Person to Existing Family
        /// Adding New Person to New Family
        /// Will return a Family Manager friendly representation of the family this person is being edited within.
        /// </summary>
        [HttpPost]
        [System.Web.Http.Route( "api/FamilyManager/UpdateOrAddPerson" )]
        [Authenticate, Secured]
        public HttpResponseMessage UpdateOrAddPerson( [FromBody]UpdatePersonBody updatePersonBody )
        {
            // default to an internal error
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            HttpContent httpContent = null;

            do
            {
                System.Web.HttpContext.Current.Items.Add( "CurrentPerson", GetPerson() );

                int personId = Core.UpdateOrAddPerson( updatePersonBody, out statusCode );
                if( statusCode != HttpStatusCode.NoContent ) break;
                
                // if noContent was returned, we know it worked. Now get the family so we can provide an updated
                // family (with this updated person) to Family Manager
                Core.FamilyReturnObject familyReturnObj = null;

                // if they were sent to us already IN a family, get that one.
                if ( updatePersonBody.FamilyId > 0 )
                {
                    familyReturnObj = Core.GetFamily( updatePersonBody.FamilyId, out statusCode );
                    if( statusCode != HttpStatusCode.OK ) break;
                }
                else
                {
                    // the only way familyId would be 0 is if this was a new person in a new family.
                    // And that means they're only in one family, so get it.

                    // the person resulted in a new family being created, so get that one.
                    PersonService personService = new PersonService( new RockContext( ) );
                    IQueryable<Group> families = personService.GetFamilies( personId );
                    if( families.Count( ) != 1 ) { statusCode = HttpStatusCode.NotFound; break; }

                    // now get the Family Manager friendly model
                    familyReturnObj = Core.GetFamily( families.ToList( )[ 0 ].Id, out statusCode );
                    if( statusCode != HttpStatusCode.OK ) break;
                }
                
                httpContent = httpContent = new StringContent( JsonConvert.SerializeObject( familyReturnObj ), Encoding.UTF8, "application/json" );
            }
            while( false );
            
            // build and return the response
            HttpResponseMessage response = new HttpResponseMessage( ) { StatusCode = statusCode, Content = httpContent };
            return response;
        }
    }
}

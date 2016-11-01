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
            RockContext rockContext = new RockContext( );

            bool valid = false;

            var userLoginService = new UserLoginService( new Rock.Data.RockContext() );
            var userLogin = userLoginService.GetByUserName( loginParameters.Username );
            if ( userLogin != null && userLogin.EntityType != null) 
            {
                var component = AuthenticationContainer.GetComponent(userLogin.EntityType.Name);
                if ( component != null && component.IsActive)
                {
                    if ( component.Authenticate( userLogin, loginParameters.Password ) )
                    {
                        // ensure there's a person associated with this login.
                        if( userLogin.PersonId.HasValue )
                        {
                            // so we know they have a valid login / password. Now check their family manager status
                            GroupService groupService = new GroupService( rockContext );
                            var familyManagerGroup = groupService.GetByGuid( Guids.FAMILY_MANAGER_AUTHORIZED_GROUP.AsGuid( ) );

                            // if they're a member of the "Family Manager" group, they're good.
                            var groupMember = familyManagerGroup.Members.Where( m => m.PersonId == userLogin.PersonId ).SingleOrDefault( );
                            if ( groupMember != null )
                            {
                                valid = true;
                            }
                        }
                    }
                }
            }

            HttpResponseMessage response;
            if ( !valid )
            {
                response = new HttpResponseMessage( HttpStatusCode.Unauthorized );
            }
            else
            {
                // return that their login was created, and give them the person ID they need to use.
                StringContent restContent = new StringContent( JsonConvert.SerializeObject( new {  PersonId = userLogin.PersonId.Value } ), Encoding.UTF8, "application/json" );
                
                response = new HttpResponseMessage( )
                {
                    StatusCode = HttpStatusCode.Created,
                    Content = restContent
                };
            }

            return response;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route( "api/FamilyManager/GetFamily" )]
        [Authenticate, Secured]
        public HttpResponseMessage GetFamily( int familyId )
        {
            HttpResponseMessage response = new HttpResponseMessage( ) { StatusCode = HttpStatusCode.NotFound };

            // first, get the family's metadata
            GroupsController groupController = new GroupsController( );
            GroupsController.FamilySearchResult familyResult = groupController.GetFamily( familyId );
            if( familyResult != null && familyResult.Id == familyId )
            {
                // next, get the family group
                Rock.Model.Group familyGroup = groupController.GetById( familyId );

                if ( familyGroup != null && familyGroup.Id == familyId )
                {
                    // now get the guests for this family
                    IQueryable<GroupsController.GuestFamily> guestFamilies = groupController.GetGuestsForFamily( familyId );

                    // and finally package it all up into something FamilyManager can use.
                    var familyReturnObj = new
                    {
                        Family = familyResult,
                        GuestFamilies = guestFamilies,
                        FamilyGroupObject = familyGroup
                    };

                    StringContent restContent = new StringContent( JsonConvert.SerializeObject( familyReturnObj ), Encoding.UTF8, "application/json" );
                    response = new HttpResponseMessage( )
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = restContent
                    };
                }
            }
            
            return response;
        }
    }
}

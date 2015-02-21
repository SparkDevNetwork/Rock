// <copyright>
// Copyright 2013 by the Spark Development Network
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web.Http;
using System.Web.Http.OData;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;

namespace Rock.Rest
{
    /*
     * NOTE: We could have inherited from System.Web.Http.OData.ODataController, but that changes 
     * the response format from vanilla REST to OData format. That breaks existing Rock Rest clients.
     * 
     */

    /// <summary>
    /// Base ApiController for Rock REST endpoints
    /// Supports ODataV3 Queries and ODataRouting 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [ODataRouting]
    public class ApiControllerBase : ApiController
    {
        /// <summary>
        /// Gets the peron alias.
        /// </summary>
        /// <returns></returns>
        protected virtual Rock.Model.Person GetPerson()
        {
            if ( Request.Properties.Keys.Contains( "Person" ) )
            {
                return Request.Properties["Person"] as Person;
            }

            var principal = ControllerContext.Request.GetUserPrincipal();
            if ( principal != null && principal.Identity != null )
            {
                var userLoginService = new Rock.Model.UserLoginService( new RockContext() );
                var userLogin = userLoginService.GetByUserName( principal.Identity.Name );

                if ( userLogin != null )
                {
                    var person = userLogin.Person;
                    Request.Properties.Add( "Person", person );
                    return userLogin.Person;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the person alias.
        /// </summary>
        /// <returns></returns>
        protected virtual Rock.Model.PersonAlias GetPersonAlias()
        {
            var person = GetPerson();
            if ( person != null )
            {
                return person.PrimaryAlias;
            }

            return null;
        }

    }
}
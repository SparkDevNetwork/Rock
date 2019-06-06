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
    /// <seealso cref="System.Web.Http.ApiController" />
    [ODataRouting]
    public class ApiControllerBase : ApiController
    {

        private static bool firstTime = true;

        /// <summary>
        /// Gets the currently logged in Person
        /// </summary>
        /// <returns></returns>
        protected virtual Rock.Model.Person GetPerson()
        {

            if(firstTime)
            {
                firstTime = false;

                try
                {

                    string logFile = System.Web.HttpContext.Current.Server.MapPath( "~/App_Data/Logs/RoutingLog.txt" );

                    using (System.IO.FileStream fs = new System.IO.FileStream( logFile, System.IO.FileMode.Append, System.IO.FileAccess.Write ))
                    {
                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter( fs ))
                        {

                            try
                            {
                                WriteToLogStream( sw, "Routes:" );
                                foreach (var route in RequestContext.Configuration.Routes)
                                {
                                    try
                                    {
                                        if (route is IReadOnlyCollection<System.Web.Http.Routing.IHttpRoute>)
                                        {
                                            var subroutes = (IReadOnlyCollection<System.Web.Http.Routing.IHttpRoute>)route;

                                            foreach (var subroute in subroutes)
                                            {
                                                WriteToLogStream( sw, "    " + subroute.RouteTemplate );
                                            }
                                        }
                                        else
                                        {
                                            WriteToLogStream( sw, route.RouteTemplate );
                                        }
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                            catch
                            {
                            }
                            try
                            {
                                WriteToLogStream( sw, "Dlls:" );
                                foreach (var dll in AppDomain.CurrentDomain.GetAssemblies())
                                {
                                    var name = "unknown";
                                    var location = "unknown";
                                    try { name = dll.FullName; }
                                    catch
                                    {
                                    }
                                    try { location = dll.Location; }
                                    catch
                                    {
                                    }
                                    WriteToLogStream( sw, name + ": " + location );
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                catch
                {

                }
            }


            return GetPerson( null );
        }

        private static void WriteToLogStream( System.IO.StreamWriter sw, string message )
        {
            sw.WriteLine( string.Format( "{0} - {1}", RockDateTime.Now.ToString(), message ) );
        }

        /// <summary>
        /// Gets the currently logged in Person
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        protected virtual Rock.Model.Person GetPerson( RockContext rockContext )
        {
            if ( Request.Properties.Keys.Contains( "Person" ) )
            {
                return Request.Properties["Person"] as Person;
            }

            var principal = ControllerContext.Request.GetUserPrincipal();
            if ( principal != null && principal.Identity != null )
            {
                if ( principal.Identity.Name.StartsWith( "rckipid=" ) )
                {
                    var personService = new Model.PersonService( rockContext ?? new RockContext() );
                    Rock.Model.Person impersonatedPerson = personService.GetByImpersonationToken( principal.Identity.Name.Substring( 8 ), false, null );
                    if ( impersonatedPerson != null )
                    {
                        return impersonatedPerson;
                    }
                }
                else
                {
                    var userLoginService = new Rock.Model.UserLoginService( rockContext ?? new RockContext() );
                    var userLogin = userLoginService.GetByUserName( principal.Identity.Name );

                    if ( userLogin != null )
                    {
                        var person = userLogin.Person;
                        Request.Properties.Add( "Person", person );
                        return userLogin.Person;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the primary person alias of the currently logged in person
        /// </summary>
        /// <returns></returns>
        protected virtual Rock.Model.PersonAlias GetPersonAlias()
        {
            return GetPersonAlias( null );
        }

        /// <summary>
        /// Gets the primary person alias of the currently logged in person
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        protected virtual Rock.Model.PersonAlias GetPersonAlias( RockContext rockContext )
        {
            var person = GetPerson( rockContext );
            if ( person != null )
            {
                return person.PrimaryAlias;
            }

            return null;
        }
    }
}

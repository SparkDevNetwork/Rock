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
using System.Reflection;
using System.Web.Http;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    ///
    /// </summary>
    public partial class RestControllersController
    {
        /// <summary>
        /// Ensures that rest controllers have been registered to the Rock Database
        /// </summary>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/RestControllers/EnsureRestControllers" )]
        [Rock.SystemGuid.RestActionGuid( "4D3BBFFD-292C-415A-BED2-2A8F8F9FC469" )]
        public bool EnsureRestControllers()
        {
            RestControllerService.RegisterControllers();

            return true;
        }

        /// <summary>
        /// Gets the list of Controller Names, with an option to include obsolete controller.
        /// </summary>
        /// <param name="includeObsolete">if set to <c>true</c> [include obsolete].</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/RestControllers/RestControllerNames" )]
        [Rock.SystemGuid.RestActionGuid( "CE1D0D07-CB40-467B-8616-8D070786C175" )]
        public List<string> RestControllerNames( bool includeObsolete )
        {
            // NOTE: This is intentionally not a method parameter because it
            // is meant to be a temporary hack to allow the old REST API
            // browser to exclude v2 API endpoints. In the future this will
            // change / be removed.
            var onlyIncludeV1 = Request.GetQueryString( "v" ) == "v1";

            var restControllerClassList = this.Get().OrderBy( a => a.Name )
                .Select( a => new
                {
                    a.ClassName,
                    a.Name
                } )
                .Where( a => !onlyIncludeV1 || !a.ClassName.Contains( ".v2." ) )
                .ToList();

            if ( includeObsolete )
            {
                // if includingObsolete, we don't have to check, so just return list
                return restControllerClassList.Select( a => a.Name ).ToList();
            }
            else
            {
                var pluginAssemblies = Reflection.GetPluginAssemblies();

                var controllerNames = new List<string>();
                foreach ( var restControllerClass in restControllerClassList )
                {
                    // see if we can figure out the Type from the currently executing assembly (Rock.Rest.dll)
                    var controllerType = Type.GetType( restControllerClass.ClassName, false );
                    if ( controllerType == null )
                    {
                        // if we can't find it from the currently executing assembly (Rock.Rest.dll), look in the plugin assemblies
                        foreach ( var pluginAssembly in pluginAssemblies )
                        {
                            controllerType = pluginAssembly.GetType( restControllerClass.ClassName, false );
                            if ( controllerType != null )
                            {
                                break;
                            }
                        }
                    }

                    if ( controllerType != null )
                    {
                        if ( controllerType.GetCustomAttribute<ObsoleteAttribute>() == null )
                        {
                            // if the controller type doesn't have [Obsolete], then add it to the list
                            controllerNames.Add( restControllerClass.Name );
                        }
                    }
                    else
                    {
                        // probably shouldn't happen, but if we weren't able to find the controller type, just include it
                        controllerNames.Add( restControllerClass.Name );
                    }
                }

                return controllerNames;
            }
        }
    }
}
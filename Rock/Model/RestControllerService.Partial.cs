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
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.RestControllerService"/> entity objects.
    /// </summary>
    public partial class RestControllerService 
    {
        /// <summary>
        /// Gets the API identifier.
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <param name="controllerName">Name of the controller.</param>
        /// <returns></returns>
        public static string GetApiId( MethodInfo methodInfo, string httpMethod, string controllerName )
        {
            var strippedClassname = Regex.Replace( methodInfo.ToString(), @"((?:(?:\w+)?\.(?<name>[a-z_A-Z]\w+))+)", "${name}" );
            return $"{httpMethod}{controllerName}^{strippedClassname}";
        }

        /// <summary>
        /// Registers the controllers.
        /// </summary>
        public static void RegisterControllers()
        {
            /*
             * 12/19/2019 BJW
             *
             * There was an issue with the SecuredAttribute not calculating API ID the same as was being calculated here.
             * This caused the secured attribute to sometimes not find the RestAction record and thus not find the
             * appropriate permissions (Auth table). The new method "GetApiId" is used in both places as a standardized
             * API ID generator to ensure that this does not occur. The following code has also been modified to gracefully
             * update any old style API IDs and update them to the new format without losing any foreign key associations, such
             * as permissions.
             *
             * See task for detailed background: https://app.asana.com/0/474497188512037/1150703513867003/f
             */

            // Controller Class Name => New Format Id => Old Format Id
            var controllerApiIdMap = new Dictionary<string, Dictionary<string, string>>();

            var rockContext = new RockContext();
            var restControllerService = new RestControllerService( rockContext );
            var discoveredControllers = new List<RestController>();

            var config = GlobalConfiguration.Configuration;
            var explorer = config.Services.GetApiExplorer();

            foreach ( var apiDescription in explorer.ApiDescriptions )
            {
                var reflectedHttpActionDescriptor = ( ReflectedHttpActionDescriptor ) apiDescription.ActionDescriptor;
                var action = apiDescription.ActionDescriptor;
                var name = action.ControllerDescriptor.ControllerName;
                var method = apiDescription.HttpMethod.Method;

                var controller = discoveredControllers.Where( c => c.Name == name ).FirstOrDefault();
                if ( controller == null )
                {
                    controller = new RestController
                    {
                        Name = name,
                        ClassName = action.ControllerDescriptor.ControllerType.FullName
                    };

                    discoveredControllers.Add( controller );
                    controllerApiIdMap[controller.ClassName] = new Dictionary<string, string>();
                }

                var apiIdMap = controllerApiIdMap[controller.ClassName];
                var apiId = GetApiId( reflectedHttpActionDescriptor.MethodInfo, method, controller.Name );

                // Because we changed the format of the stored ApiId, it is possible some RestAction records will have the old
                // style Id, which is apiDescription.ID
                apiIdMap[apiId] = apiDescription.ID;

                controller.Actions.Add( new RestAction
                {
                    ApiId = apiId,
                    Method = method,
                    Path = apiDescription.RelativePath
                } );
            }

            var actionService = new RestActionService( rockContext );
            foreach(var discoveredController in discoveredControllers)
            {
                var apiIdMap = controllerApiIdMap[discoveredController.ClassName];

                var controller = restControllerService.Queryable( "Actions" )
                    .Where( c => c.Name == discoveredController.Name ).FirstOrDefault();
                if ( controller == null )
                {
                    controller = new RestController { Name = discoveredController.Name };
                    restControllerService.Add( controller );
                }
                controller.ClassName = discoveredController.ClassName;

                foreach(var discoveredAction in discoveredController.Actions)
                {
                    var newFormatId = discoveredAction.ApiId;
                    var oldFormatId = apiIdMap[newFormatId];

                    var action = controller.Actions.Where( a => a.ApiId == newFormatId || a.ApiId == oldFormatId ).FirstOrDefault();

                    if ( action?.ApiId == oldFormatId )
                    {
                        // Update the ID to the new format
                        action.ApiId = newFormatId;
                    }

                    if ( action == null )
                    {
                        action = new RestAction { ApiId = newFormatId };
                            controller.Actions.Add( action );
                    }
                    action.Method = discoveredAction.Method;
                    action.Path = discoveredAction.Path;
                }

                var actions = discoveredController.Actions.Select( d => d.ApiId).ToList();
                foreach( var action in controller.Actions.Where( a => !actions.Contains(a.ApiId)).ToList())
                {
                    actionService.Delete( action );
                    controller.Actions.Remove(action);
                }
            }

            var controllers = discoveredControllers.Select( d => d.Name ).ToList();
            foreach ( var controller in restControllerService.Queryable().Where( c => !controllers.Contains( c.Name ) ).ToList() )
            {
                restControllerService.Delete( controller );
            }

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Gets the Guid for the RestController that has the specified Id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Guid? GetGuid( int id )
        {
            var cacheItem = RestControllerCache.Get( id );
            if ( cacheItem != null )
            {
                return cacheItem.Guid;
            }

            return null;
        }

    }
}

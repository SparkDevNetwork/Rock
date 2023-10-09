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
using System.Web.Http.Controllers;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.RestControllerService"/> entity objects.
    /// </summary>
    public partial class RestControllerService
    {
        private class DiscoveredControllerFromReflection
        {
            public string Name { get; set; }
            public string ClassName { get; set; }
            public Guid? ReflectedGuid { get; set; }
            public List<DiscoveredRestAction> DiscoveredRestActions { get; set; } = new List<DiscoveredRestAction>();
            public override string ToString()
            {
                return ClassName;
            }
        }

        private class DiscoveredRestAction
        {
            public string ApiId { get; set; }
            public string Method { get; set; }
            public string Path { get; set; }
            public Guid? ReflectedGuid { get; set; }
            public override string ToString()
            {
                return ApiId;
            }
        }

        /// <summary>
        /// Registers the controllers.
        /// </summary>
        public static void RegisterControllers()
        {
            /*
             * 05/13/2022 MDP/DMV
             * 
             * In addition to the 12/19/2019 BJW note, we also added a RockGuid attribute to 
             * controllers and methods (except for inherited methods). This will prevent
             * loosing security on methods that have changed their signature. 
             * 
             * 
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
            var discoveredControllers = new List<DiscoveredControllerFromReflection>();

            var config = GlobalConfiguration.Configuration;
            var explorer = config.Services.GetApiExplorer();

            if ( explorer.ApiDescriptions.Count == 0 )
            {
                // Just in case ApiDescriptions wasn't populated, exit and don't do anything
                return;
            }

            // Look for any duplicate rest action unique identifiers and log
            // them for the developer to look into.
            var duplicateActionIdentifiers = explorer.ApiDescriptions
                .Select( d => d.ActionDescriptor as ReflectedHttpActionDescriptor )
                .Where( d => d != null )
                .Select( d => new
                {
                    d.MethodInfo,
                    d.MethodInfo?.GetCustomAttribute<Rock.SystemGuid.RestActionGuidAttribute>( inherit: false )?.Guid,
                    d.ControllerDescriptor.ControllerType,
                    d.MethodInfo.Name
                } )
                .Where( d => d.Guid.HasValue )
                .GroupBy( d => d.Guid.Value )
                // Only include groups with duplicate guids.
                .Where( g => g.Count() > 1 )
                // Only include groups where the duplicates aren't the same method.
                .Where( g => g.GroupBy( d => d.MethodInfo ).Count() > 1 )
                .ToList();

            foreach ( var duplicateIdentifier in duplicateActionIdentifiers )
            {
                var methods = duplicateIdentifier.Select( d => $"{d.ControllerType.FullName}.{d.Name}" ).JoinStrings( " and " );

                ExceptionLogService.LogException( new Exception( $"Found duplicate rest action guid '{duplicateIdentifier.Key}' on {methods}" ) );
            }

            foreach ( var apiDescription in explorer.ApiDescriptions )
            {
                var reflectedHttpActionDescriptor = ( ReflectedHttpActionDescriptor ) apiDescription.ActionDescriptor;
                var action = apiDescription.ActionDescriptor;
                var name = action.ControllerDescriptor.ControllerName;
                var className = action.ControllerDescriptor.ControllerType.FullName;
                var method = apiDescription.HttpMethod.Method;
                var controllerRockGuid = action.ControllerDescriptor.ControllerType.GetCustomAttribute<Rock.SystemGuid.RestControllerGuidAttribute>( inherit: false )?.Guid;

                var controller = discoveredControllers
                    .Where( c => c.ClassName == className
                        || ( c.ReflectedGuid.HasValue && controllerRockGuid.HasValue && c.ReflectedGuid.Value == controllerRockGuid.Value ) )
                    .OrderByDescending( c => c.ReflectedGuid.HasValue && controllerRockGuid.HasValue && c.ReflectedGuid.Value == controllerRockGuid.Value )
                    .FirstOrDefault();

                if ( controller == null )
                {
                    controller = new DiscoveredControllerFromReflection
                    {
                        Name = name,
                        ClassName = action.ControllerDescriptor.ControllerType.FullName
                    };

                    if ( controllerRockGuid.HasValue )
                    {
                        controller.ReflectedGuid = controllerRockGuid.Value;
                    }
                    else
                    {
                        controller.ReflectedGuid = null;
                    }

                    discoveredControllers.Add( controller );
                    controllerApiIdMap[controller.ClassName] = new Dictionary<string, string>();
                }

                var apiIdMap = controllerApiIdMap[controller.ClassName];
                var apiId = GetApiId( reflectedHttpActionDescriptor.MethodInfo, method, controller.Name, out Guid? restActionGuid );

                // Because we changed the format of the stored ApiId, it is possible some RestAction records will have the old
                // style Id, which is apiDescription.ID
                apiIdMap[apiId] = apiDescription.ID;

                var restAction = new DiscoveredRestAction
                {
                    ApiId = apiId,
                    Method = method,
                    Path = apiDescription.RelativePath
                };

                if ( restActionGuid.HasValue )
                {
                    restAction.ReflectedGuid = restActionGuid.Value;
                }
                else
                {
                    restAction.ReflectedGuid = null;
                }

                controller.DiscoveredRestActions.Add( restAction );
            }

            if ( !discoveredControllers.Any() )
            {
                // Just in case discoveredControllers somehow is empty, exit and don't do anything
                return;
            }

            var actionService = new RestActionService( rockContext );
            foreach ( var discoveredController in discoveredControllers )
            {
                var apiIdMap = controllerApiIdMap[discoveredController.ClassName];

                var controller = restControllerService.Queryable( "Actions" )
                    .Where( c => c.ClassName == discoveredController.ClassName
                        || ( discoveredController.ReflectedGuid.HasValue && c.Guid == discoveredController.ReflectedGuid.Value ) )
                    .OrderByDescending( c => discoveredController.ReflectedGuid.HasValue && c.Guid == discoveredController.ReflectedGuid.Value )
                    .FirstOrDefault();

                if ( controller == null )
                {
                    controller = new RestController
                    {
                        Name = discoveredController.Name,
                    };

                    restControllerService.Add( controller );
                }

                controller.ClassName = discoveredController.ClassName;

                if ( discoveredController.ReflectedGuid.HasValue )
                {
                    if ( controller.Guid != discoveredController.ReflectedGuid.Value )
                    {
                        controller.Guid = discoveredController.ReflectedGuid.Value;
                    }
                }

                foreach ( var discoveredAction in discoveredController.DiscoveredRestActions )
                {
                    var newFormatId = discoveredAction.ApiId;
                    var oldFormatId = apiIdMap[newFormatId];

                    var action = controller.Actions.Where( a =>
                        a.ApiId == newFormatId
                        || a.ApiId == oldFormatId
                        || ( discoveredAction.ReflectedGuid.HasValue && a.Guid == discoveredAction.ReflectedGuid.Value ) ).FirstOrDefault();

                    if ( action == null )
                    {
                        action = new RestAction { ApiId = newFormatId };
                        controller.Actions.Add( action );
                    }

                    action.Method = discoveredAction.Method;
                    action.Path = discoveredAction.Path;

                    if ( action.ApiId != newFormatId )
                    {
                        // Update the ID to the new format
                        // This will also take care of method signature changes
                        action.ApiId = newFormatId;
                    }

                    if ( discoveredAction.ReflectedGuid.HasValue )
                    {
                        if ( action.Guid != discoveredAction.ReflectedGuid.Value )
                        {
                            action.Guid = discoveredAction.ReflectedGuid.Value;
                        }
                    }
                }

                var actionApiIds = discoveredController.DiscoveredRestActions.Select( d => d.ApiId ).ToList();
                var actionGuids = discoveredController.DiscoveredRestActions.Select( d => d.ReflectedGuid ).ToList();
                var actionsToRemove = controller.Actions
                    .Where( a => !actionApiIds.Contains( a.ApiId ) && !actionGuids.Contains( a.Guid ) )
                    .ToList();
                foreach ( var action in actionsToRemove )
                {
                    actionService.Delete( action );
                    controller.Actions.Remove( action );
                }
            }

            var controllerNames = discoveredControllers.Select( d => d.ClassName ).ToList();
            var controllerGuids = discoveredControllers.Select( d => d.ReflectedGuid ).ToList();
            var controllersToRemove = restControllerService.Queryable()
                .Where( c => !controllerNames.Contains( c.ClassName ) && !controllerGuids.Contains( c.Guid ) )
                .ToList();
            foreach ( var controller in controllersToRemove )
            {
                restControllerService.Delete( controller );
            }

            try
            {
                rockContext.SaveChanges();
            }
            catch ( Exception thrownException )
            {
                // if the exception was due to a duplicate Guid, throw as a duplicateGuidException. That'll make it easier to troubleshoot.
                var duplicateGuidException = Rock.SystemGuid.DuplicateSystemGuidException.CatchDuplicateSystemGuidException( thrownException, null );
                if ( duplicateGuidException != null )
                {
                    throw duplicateGuidException;
                }
                else
                {
                    throw;
                }
            }
        }
    }
}

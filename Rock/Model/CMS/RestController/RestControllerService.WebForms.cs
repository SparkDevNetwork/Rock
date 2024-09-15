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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;

using Rock.Data;
using Rock.SystemGuid;

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
            public MethodInfo MethodInfo { get; set; }
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

            foreach ( var apiDescription in explorer.ApiDescriptions )
            {
                var reflectedHttpActionDescriptor = ( ReflectedHttpActionDescriptor ) apiDescription.ActionDescriptor;
                var action = apiDescription.ActionDescriptor;
                var name = action.ControllerDescriptor.ControllerName;
                var fullClassName = action.ControllerDescriptor.ControllerType.FullName;
                var method = apiDescription.HttpMethod.Method;
                var controllerRockGuid = action.ControllerDescriptor.ControllerType.GetCustomAttribute<Rock.SystemGuid.RestControllerGuidAttribute>( inherit: false )?.Guid;

                // Look for a previously discovered controller first by Guid
                // and then by class name. We can't use just name because
                // those are no longer unique.
                var controller = discoveredControllers
                    .Where( c => c.ClassName == fullClassName
                        || ( controllerRockGuid.HasValue && c.ReflectedGuid == controllerRockGuid ) )
                    .OrderByDescending( c => c.ReflectedGuid == controllerRockGuid )
                    .FirstOrDefault();

                if ( controller == null )
                {
                    controller = new DiscoveredControllerFromReflection
                    {
                        Name = name,
                        ClassName = fullClassName
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

                // The API Explorer will sometimes return the same method twice.
                // No idea why. Skip the duplicates.
                if ( controller.DiscoveredRestActions.Any( a => a.Method == method && a.MethodInfo == reflectedHttpActionDescriptor.MethodInfo ) )
                {
                    continue;
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
                    MethodInfo = reflectedHttpActionDescriptor.MethodInfo,
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

            if ( System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
            {
                // Look for any duplicate controller guids for diagnostic logging.
                var duplicateControllerGuids = discoveredControllers
                    .Where( c => c.ReflectedGuid.HasValue )
                    .GroupBy( c => c.ReflectedGuid.Value )
                    .Where( c => c.Count() > 1 )
                    .ToList();

                foreach ( var duplicateController in duplicateControllerGuids )
                {
                    var duplicateNames = duplicateController.Select( c => c.ClassName ).JoinStrings( ", " );

                    Debug.WriteLine( $"Detected duplicate controller guid '{duplicateController.Key}': {duplicateNames}" );

                    // Only throw an exception if a debugger is attached. This
                    // reduces the risk of causing issues on a production system
                    // that happens to have web.config configured in debug mode.
                    if ( Debugger.IsAttached )
                    {
                        ExceptionLogService.LogException( new DuplicateSystemGuidException( $"Detected duplicate controller guid '{duplicateController.Key}': {duplicateNames}" ) );
                    }
                }

                // Look for any duplicate action guids for diagnostic logging.
                var duplicateActionGuids = discoveredControllers
                    .SelectMany( c => c.DiscoveredRestActions )
                    .Where( a => a.ReflectedGuid.HasValue )
                    .GroupBy( a => a.ReflectedGuid.Value )
                    .Where( a => a.Count() > 1 )
                    .ToList();

                foreach ( var duplicateAction in duplicateActionGuids )
                {
                    var duplicateNames = duplicateAction.Select( a => $"[{a.Method}]{a.MethodInfo.ReflectedType}.{a.MethodInfo.Name}" ).JoinStrings( ", " );

                    Debug.WriteLine( $"Detected duplicate action guid '{duplicateAction.Key}': {duplicateNames}" );

                    // Only throw an exception if a debugger is attached. This
                    // reduces the risk of causing issues on a production system
                    // that happens to have web.config configured in debug mode.
                    if ( Debugger.IsAttached )
                    {
                        ExceptionLogService.LogException( new DuplicateSystemGuidException( $"Detected duplicate action guid '{duplicateAction.Key}': {duplicateNames}" ) );
                    }
                }
            }

            // First create any new controllers we will need in the next phase.
            var allDatabaseControllers = restControllerService.Queryable().ToList();

            foreach ( var discoveredController in discoveredControllers )
            {
                var controller = allDatabaseControllers
                    .Where( c => c.ClassName == discoveredController.ClassName
                        || ( discoveredController.ReflectedGuid.HasValue && c.Guid == discoveredController.ReflectedGuid ) )
                    .OrderByDescending( c => c.Guid == discoveredController.ReflectedGuid )
                    .FirstOrDefault();

                if ( controller == null )
                {
                    controller = new RestController
                    {
                        Name = discoveredController.Name,
                    };

                    restControllerService.Add( controller );
                    allDatabaseControllers.Add( controller );
                }

                controller.ClassName = discoveredController.ClassName;

                if ( discoveredController.ReflectedGuid.HasValue )
                {
                    if ( controller.Guid != discoveredController.ReflectedGuid.Value )
                    {
                        controller.Guid = discoveredController.ReflectedGuid.Value;
                    }
                }
            }

            // Save all changes to the REST controllers.
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

            var actionService = new RestActionService( rockContext );
            var allDatabaseActions = actionService.Queryable().ToList();

            foreach ( var discoveredController in discoveredControllers )
            {
                var apiIdMap = controllerApiIdMap[discoveredController.ClassName];

                var controller = allDatabaseControllers
                    .Where( c => c.ClassName == discoveredController.ClassName
                        || ( discoveredController.ReflectedGuid.HasValue && c.Guid == discoveredController.ReflectedGuid ) )
                    .OrderByDescending( c => c.Guid == discoveredController.ReflectedGuid )
                    .FirstOrDefault();

                // Shouldn't happen, but bail on this action just in case.
                if ( controller == null )
                {
                    continue;
                }

                foreach ( var discoveredAction in discoveredController.DiscoveredRestActions )
                {
                    var newFormatId = discoveredAction.ApiId;
                    var oldFormatId = apiIdMap[newFormatId];

                    var action = allDatabaseActions.Where( a =>
                        a.ApiId == newFormatId
                        || a.ApiId == oldFormatId
                        || ( discoveredAction.ReflectedGuid.HasValue && a.Guid == discoveredAction.ReflectedGuid.Value ) ).FirstOrDefault();

                    if ( action == null )
                    {
                        action = new RestAction
                        {
                            ApiId = newFormatId,
                            ControllerId = controller.Id
                        };
                        controller.Actions.Add( action );
                    }

                    action.Method = discoveredAction.Method;
                    action.Path = discoveredAction.Path;

                    // Make sure existing actions are still attached to the
                    // correct controller. We can only safely do this if we
                    // have an action unique identifier.
                    if ( discoveredAction.ReflectedGuid.HasValue && action.ControllerId != controller.Id )
                    {
                        action.ControllerId = controller.Id;
                    }

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
            }

            // Delete any actions that no longer exist.
            // NOTE: In the future this won't be safe since we might have API
            // endpoints in two different Rock servers on two different .NET
            // frameworks.
            var allDiscoveredActions = discoveredControllers
                .SelectMany( c => c.DiscoveredRestActions )
                .ToList();
            var actionsToDelete = allDatabaseActions
                .Where( a => !allDiscoveredActions.Any( da => da.ApiId == a.ApiId || da.ReflectedGuid == a.Guid ) )
                .ToList();
            actionService.DeleteRange( actionsToDelete );

            // Delete any controllers that no longer exist.
            var controllers = discoveredControllers.Select( d => d.ClassName ).ToList();
            foreach ( var controller in restControllerService.Queryable().Where( c => !controllers.Contains( c.ClassName ) ).ToList() )
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

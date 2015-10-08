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
using System.Web.Http;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.RestControllerService"/> entity objects.
    /// </summary>
    public partial class RestControllerService 
    {
        /// <summary>
        /// Registers the controllers.
        /// </summary>
        public static void RegisterControllers()
        {
            var rockContext = new RockContext();
            var restControllerService = new RestControllerService( rockContext );

            var existingControllers = restControllerService.Queryable( "Actions" ).ToList();
            var discoveredControllers = new List<RestController>();

            var config = GlobalConfiguration.Configuration;
            var explorer = config.Services.GetApiExplorer();
            foreach(var apiDescription in explorer.ApiDescriptions)
            {
                var action = apiDescription.ActionDescriptor;
                var name = action.ControllerDescriptor.ControllerName;

                var controller = discoveredControllers.Where( c => c.Name == name ).FirstOrDefault();
                if ( controller == null )
                {
                    controller = new RestController
                    {
                        Name = name,
                        ClassName = action.ControllerDescriptor.ControllerType.FullName
                    };

                    discoveredControllers.Add( controller );
                }

                controller.Actions.Add( new RestAction
                {
                    ApiId = apiDescription.ID,
                    Method = apiDescription.HttpMethod.Method,
                    Path = apiDescription.RelativePath
                } );
            }

            var actionService = new RestActionService( rockContext );
            foreach(var discoveredController in discoveredControllers)
            {
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
                    var action = controller.Actions.Where( a => a.ApiId == discoveredAction.ApiId ).FirstOrDefault();
                    {
                        if ( action == null )
                        {
                            action = new RestAction { ApiId = discoveredAction.ApiId };
                            controller.Actions.Add( action );
                        }
                        action.Method = discoveredAction.Method;
                        action.Path = discoveredAction.Path;
                    }
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
            var cacheItem = Rock.Web.Cache.RestControllerCache.Read( id );
            if ( cacheItem != null )
            {
                return cacheItem.Guid;
            }

            return null;
        }

    }
}

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
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

using Microsoft.Extensions.DependencyInjection;

namespace Rock.Rest.Handler
{
    /// <summary>
    /// Custom HttpControllerActivator that uses dependency injection to
    /// activate the controller.
    /// </summary>
    class RockDependencyControllerActivator : IHttpControllerActivator
    {
        /// <inheritdoc/>
        public IHttpController Create( HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType )
        {
            if ( !( request.Properties["RockServiceProvider"] is IServiceProvider serviceProvider ) )
            {
                return null;
            }

            return ActivatorUtilities.CreateInstance( serviceProvider, controllerType ) as IHttpController;
        }
    }
}

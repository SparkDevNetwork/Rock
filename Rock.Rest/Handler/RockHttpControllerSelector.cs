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
using System.Net.Http;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http;
using System.Linq;

namespace Rock.Rest.Handler
{
    /// <summary>
    /// Custom controller selector that handles duplicate class names. Web Api
    /// out of the box does not allow duplicate class names, even if they have
    /// different namespaces and different routes.
    /// </summary>
    internal class RockHttpControllerSelector : DefaultHttpControllerSelector
    {
        private const string ControllerKey = "controller";
        private readonly HttpConfiguration _configuration;
        private readonly Lazy<Dictionary<string, HttpControllerDescriptor>> _controllerTypes;
        private readonly Lazy<Dictionary<string, HttpControllerDescriptor>> _duplicateControllerTypes;
        private readonly Lazy<Dictionary<string, HttpControllerDescriptor>> _prefixedDuplicateControllers;

        public RockHttpControllerSelector( HttpConfiguration configuration ) : base( configuration )
        {
            _configuration = configuration;
            _controllerTypes = new Lazy<Dictionary<string, HttpControllerDescriptor>>( GetControllerTypes );
            _duplicateControllerTypes = new Lazy<Dictionary<string, HttpControllerDescriptor>>( GetDuplicateControllerTypes );
            _prefixedDuplicateControllers = new Lazy<Dictionary<string, HttpControllerDescriptor>>( GetPrefixedDuplicateControllers );
        }

        public override HttpControllerDescriptor SelectController( HttpRequestMessage request )
        {
            var path = request.RequestUri.AbsolutePath.Trim( '/' );

            foreach ( var prefixedController in _prefixedDuplicateControllers.Value )
            {
                if ( path.Equals( prefixedController.Key, StringComparison.OrdinalIgnoreCase ) || path.StartsWith( prefixedController.Key + "/", StringComparison.OrdinalIgnoreCase ) )
                {
                    return prefixedController.Value;
                }
            }

            // Handle duplicate controller names.
            var routeData = request.GetRouteData();
            if ( routeData.Values != null && routeData.Values.TryGetValue( ControllerKey, out var cn ) && _duplicateControllerTypes.Value.ContainsKey( cn.ToString() ) )
            {
                return _duplicateControllerTypes.Value[cn.ToString()];
            }

            return base.SelectController( request );
        }

        private Dictionary<string, HttpControllerDescriptor> GetControllerTypes()
        {
            var result = new Dictionary<string, HttpControllerDescriptor>( base.GetControllerMapping() );

            foreach ( var item in _duplicateControllerTypes.Value )
            {
                result.TryAdd( item.Key, item.Value );
            }

            return result;
        }

        private Dictionary<string, HttpControllerDescriptor> GetPrefixedDuplicateControllers()
        {
            var prefixes = new Dictionary<string, HttpControllerDescriptor>();

            foreach ( var kvp in _duplicateControllerTypes.Value )
            {
                var parts = kvp.Key.Split( new[] { '-' }, 2 );

                if ( parts.Length == 2 )
                {
                    prefixes.TryAdd( parts[1], kvp.Value );
                }
            }

            return prefixes;
        }

        private Dictionary<string, HttpControllerDescriptor> GetDuplicateControllerTypes()
        {
            var assembliesResolver = _configuration.Services.GetAssembliesResolver();
            var controllersResolver = _configuration.Services.GetHttpControllerTypeResolver();
            var controllerTypes = controllersResolver.GetControllerTypes( assembliesResolver );

            var groupedByName = controllerTypes
                .GroupBy( t => t.Name.Substring( 0, t.Name.Length - ControllerSuffix.Length ), StringComparer.OrdinalIgnoreCase )
                .Where( x => x.Count() > 1 );

            var result = new Dictionary<string, HttpControllerDescriptor>();

            foreach ( var controllerTypeGroup in groupedByName )
            {
                foreach ( var controllerType in controllerTypeGroup )
                {
                    var prefix = controllerType.GetCustomAttribute<System.Web.Http.RoutePrefixAttribute>()?.Prefix;
                    var controllerDescriptor = new HttpControllerDescriptor( _configuration, controllerTypeGroup.Key, controllerType );

                    if ( prefix.IsNotNullOrWhiteSpace() )
                    {
                        result.TryAdd( $"{controllerTypeGroup.Key}-{prefix}", controllerDescriptor );
                    }
                    else
                    {
                        result.TryAdd( controllerTypeGroup.Key, controllerDescriptor );
                    }
                }
            }

            return result;
        }

        public override IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            return _controllerTypes.Value;
        }
    }
}

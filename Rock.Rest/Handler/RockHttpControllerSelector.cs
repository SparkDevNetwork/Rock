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
        private readonly Lazy<Dictionary<string, PrefixedControllerSegment>> _prefixedControllers;

        /// <summary>
        /// Initializes a new instance of the <see cref="RockHttpControllerSelector"/> class
        /// using the specified HTTP configuration.
        /// </summary>
        /// <param name="configuration">The HTTP configuration that provides settings and services for controller selection.</param>
        public RockHttpControllerSelector( HttpConfiguration configuration ) : base( configuration )
        {
            _configuration = configuration;
            _controllerTypes = new Lazy<Dictionary<string, HttpControllerDescriptor>>( GetControllerTypes );
            _duplicateControllerTypes = new Lazy<Dictionary<string, HttpControllerDescriptor>>( GetDuplicateControllerTypes );
            _prefixedControllers = new Lazy<Dictionary<string, PrefixedControllerSegment>>( GetPrefixedControllers );
        }

        /// <inheritdoc/>
        public override HttpControllerDescriptor SelectController( HttpRequestMessage request )
        {
            var path = request.RequestUri.AbsolutePath.Trim( '/' );

            if ( path.IsNotNullOrWhiteSpace() )
            {
                var prefixedController = GetPrefixedController( _prefixedControllers.Value, path.Split( '/' ) );

                if ( prefixedController != null )
                {
                    return prefixedController;
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

        /// <summary>
        /// Builds a dictionary of route prefixes mapped to their corresponding
        /// controller segments.
        /// </summary>
        /// <returns>A dictionary that represens the route table of controllers with prefixes.</returns>
        private Dictionary<string, PrefixedControllerSegment> GetPrefixedControllers()
        {
            var prefixes = new Dictionary<string, PrefixedControllerSegment>();
            var assembliesResolver = _configuration.Services.GetAssembliesResolver();
            var controllersResolver = _configuration.Services.GetHttpControllerTypeResolver();
            var controllerTypes = controllersResolver.GetControllerTypes( assembliesResolver );

            foreach ( var controllerType in controllerTypes )
            {
                var prefix = controllerType.GetCustomAttribute<System.Web.Http.RoutePrefixAttribute>()?.Prefix?.Trim( '/' );

                if ( prefix.IsNotNullOrWhiteSpace() )
                {
                    var controllerName = controllerType.Name.Substring( 0, controllerType.Name.Length - ControllerSuffix.Length );
                    var controllerDescriptor = new HttpControllerDescriptor( _configuration, controllerName, controllerType );

                    AddPrefixedController( prefixes, prefix.Split( '/' ), controllerDescriptor );
                }
            }

            return prefixes;
        }

        /// <summary>
        /// Adds a controller descriptor to the specified prefix hierarchy,
        /// creating any missing prefix segments as needed.
        /// </summary>
        /// <remarks>
        /// If intermediate prefix segments do not exist in the hierarchy,
        /// they are created automatically. The controller descriptor is
        /// assigned to the last segment in the provided path.
        /// </remarks>
        /// <param name="prefixes">A dictionary mapping prefix segment names to their corresponding controller segment objects. Used to build or traverse the prefix hierarchy.</param>
        /// <param name="segments">A span of strings representing the ordered segments of the prefix path. Each segment is processed to build the hierarchy.</param>
        /// <param name="controllerDescriptor">The controller descriptor to associate with the final segment in the prefix path.</param>
        private void AddPrefixedController( Dictionary<string, PrefixedControllerSegment> prefixes, Span<string> segments, HttpControllerDescriptor controllerDescriptor )
        {
            var segment = segments[0];

            if ( !prefixes.TryGetValue( segment, out var prefix) )
            {
                prefix = new PrefixedControllerSegment( segment );
                prefixes.Add( segment, prefix );
            }

            if ( segments.Length == 1 )
            {
                prefix.ControllerDescriptor = controllerDescriptor;
            }
            else
            {
                AddPrefixedController( prefix.Children, segments.Slice( 1 ), controllerDescriptor );
            }
        }

        /// <summary>
        /// Retrieves the controller descriptor that matches the specified
        /// route segments and their associated prefixes.
        /// </summary>
        /// <param name="prefixes">A dictionary mapping route segment prefixes to their corresponding controller segment information.</param>
        /// <param name="segments">A span of route segments to match against the available prefixes.</param>
        /// <returns>The <see cref="HttpControllerDescriptor"/> that corresponds to the matched route segments and prefixes; or <see langword="null"/> if no matching controller is found.</returns>
        private HttpControllerDescriptor GetPrefixedController( Dictionary<string, PrefixedControllerSegment> prefixes, Span<string> segments )
        {
            var segment = segments[0];

            if ( prefixes.TryGetValue( segment, out var prefix ) )
            {
                if ( segments.Length == 1 )
                {
                    return prefix.ControllerDescriptor;
                }
                else
                {
                    // Look for a child prefix with a more exact match, otherwise
                    // return this prefix's controller descriptor.
                    var controller = GetPrefixedController( prefix.Children, segments.Slice( 1 ) );

                    return controller ?? prefix.ControllerDescriptor;
                }
            }

            return null;
        }

        private Dictionary<string, HttpControllerDescriptor> GetDuplicateControllerTypes()
        {
            var assembliesResolver = _configuration.Services.GetAssembliesResolver();
            var controllersResolver = _configuration.Services.GetHttpControllerTypeResolver();
            var controllerTypes = controllersResolver.GetControllerTypes( assembliesResolver );

            var groupedByName = controllerTypes
                .GroupBy( t => t.Name.Substring( 0, t.Name.Length - ControllerSuffix.Length ), StringComparer.OrdinalIgnoreCase )
                .Where( x => x.Count() > 1 );

            var result = new Dictionary<string, HttpControllerDescriptor>( StringComparer.OrdinalIgnoreCase );

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

        /// <inheritdoc/>
        public override IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            return _controllerTypes.Value;
        }

        private class PrefixedControllerSegment
        {
            public string Segment { get; }

            public HttpControllerDescriptor ControllerDescriptor { get; set; }

            public Dictionary<string, PrefixedControllerSegment> Children { get; }

            public PrefixedControllerSegment( string segment )
            {
                Segment = segment;
                Children = new Dictionary<string, PrefixedControllerSegment>( StringComparer.OrdinalIgnoreCase );
            }
        }
    }
}

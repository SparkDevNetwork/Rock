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

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rock.Tests.Rest
{
    /// <summary>
    /// Tests that block actions can only be run from a POST request. A GET
    /// can be started by a link from another site and still carries the
    /// authentication cookie, so a block action route must never accept GET.
    /// </summary>
    /// <remarks>
    /// The Web API types are read by name because this project does not
    /// reference System.Web.Http directly.
    /// </remarks>
    [TestClass]
    public class BlockActionRouteTests
    {
        private static readonly string[] _blockActionRoutePrefixes = new[]
        {
            "api/v2/blockactions/",
            "api/blocks/action/"
        };

        private static readonly string[] _webApiVerbPrefixes = new[]
        {
            "GET", "POST", "PUT", "DELETE", "HEAD", "OPTIONS", "PATCH"
        };

        [TestMethod]
        public void BlockActionRoutes_AreFound()
        {
            var routes = GetBlockActionRoutes();

            // The v2 route and the v1 routes.
            Assert.IsTrue( routes.Count >= 2, $"Expected to find the block action routes but found {routes.Count}." );
        }

        [TestMethod]
        public void BlockActionRoutes_OnlyAllowPost()
        {
            foreach ( var route in GetBlockActionRoutes() )
            {
                var verbs = GetSupportedVerbs( route.Method );

                CollectionAssert.AreEquivalent( new[] { "POST" }, verbs.ToArray(), $"{route.Method.DeclaringType.Name}.{route.Method.Name} ({route.Template}) must only allow POST." );
            }
        }

        #region Support Methods

        /// <summary>
        /// Gets every Rock.Rest action method whose route is a block action route.
        /// </summary>
        /// <returns>The action methods and their full route templates.</returns>
        private static List<(MethodInfo Method, string Template)> GetBlockActionRoutes()
        {
            var routes = new List<(MethodInfo Method, string Template)>();
            var restAssembly = Assembly.Load( "Rock.Rest" );

            foreach ( var type in GetLoadableTypes( restAssembly ) )
            {
                var prefix = GetAttributeValue( type, "System.Web.Http.RoutePrefixAttribute", "Prefix" );

                foreach ( var method in type.GetMethods( BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly ) )
                {
                    var template = GetAttributeValue( method, "System.Web.Http.RouteAttribute", "Template" );

                    if ( template == null )
                    {
                        continue;
                    }

                    // A template that starts with ~/ ignores the prefix.
                    var fullTemplate = template.StartsWith( "~/" ) || prefix == null
                        ? template.TrimStart( '~', '/' )
                        : $"{prefix.TrimEnd( '/' )}/{template}";

                    if ( _blockActionRoutePrefixes.Any( p => fullTemplate.StartsWith( p, StringComparison.OrdinalIgnoreCase ) ) )
                    {
                        routes.Add( (method, fullTemplate) );
                    }
                }
            }

            return routes;
        }

        /// <summary>
        /// Gets the HTTP verbs Web API allows for the action method. This
        /// follows the Web API rules: verb attributes win, then a verb at the
        /// start of the method name, otherwise only POST.
        /// </summary>
        /// <param name="method">The action method.</param>
        /// <returns>The supported HTTP verbs in upper case.</returns>
        private static List<string> GetSupportedVerbs( MethodInfo method )
        {
            var verbs = new List<string>();

            foreach ( var attribute in method.GetCustomAttributes( true ) )
            {
                var attributeType = attribute.GetType();
                var isVerbProvider = attributeType.GetInterfaces().Any( i => i.FullName == "System.Web.Http.Controllers.IActionHttpMethodProvider" );

                if ( !isVerbProvider )
                {
                    continue;
                }

                var httpMethods = ( System.Collections.IEnumerable ) attributeType.GetProperty( "HttpMethods" ).GetValue( attribute );

                foreach ( var httpMethod in httpMethods )
                {
                    verbs.Add( httpMethod.ToString().ToUpperInvariant() );
                }
            }

            if ( verbs.Any() )
            {
                return verbs.Distinct().ToList();
            }

            var nameVerb = _webApiVerbPrefixes.FirstOrDefault( v => method.Name.StartsWith( v, StringComparison.OrdinalIgnoreCase ) );

            return new List<string> { nameVerb ?? "POST" };
        }

        /// <summary>
        /// Gets a string property from the first attribute with the given type name.
        /// </summary>
        /// <param name="member">The type or method to read the attribute from.</param>
        /// <param name="attributeTypeName">The full name of the attribute type.</param>
        /// <param name="propertyName">The name of the property to read.</param>
        /// <returns>The property value or <c>null</c> if the attribute was not found.</returns>
        private static string GetAttributeValue( MemberInfo member, string attributeTypeName, string propertyName )
        {
            var attribute = member.GetCustomAttributes( false )
                .FirstOrDefault( a => a.GetType().FullName == attributeTypeName );

            return attribute?.GetType().GetProperty( propertyName ).GetValue( attribute ) as string;
        }

        /// <summary>
        /// Gets the types in the assembly that could be loaded.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>The types that could be loaded.</returns>
        private static IEnumerable<Type> GetLoadableTypes( Assembly assembly )
        {
            try
            {
                return assembly.GetTypes();
            }
            catch ( ReflectionTypeLoadException ex )
            {
                return ex.Types.Where( t => t != null );
            }
        }

        #endregion
    }
}

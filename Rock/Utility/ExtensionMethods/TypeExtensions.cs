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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Rock
{
    /// <summary>
    /// Handy <see cref="Type"/> extensions.
    /// </summary>
    public static partial class ExtensionMethods
    {
        #region Type Extensions

        /// <summary>
        /// Determines whether <paramref name="type"/> is a descendent of <paramref name="baseType"/>.
        /// This test takes into account <paramref name="baseType"/> being a generic type definition.
        /// </summary>
        /// <param name="type">The type to be tested.</param>
        /// <param name="baseType">The base type that <paramref name="type"/> must descend from.</param>
        /// <returns>
        ///   <c>true</c> if <paramref name="type"/> is a descendent of <paramref name="baseType"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsDescendentOf( this Type type, Type baseType )
        {
            //
            // If the base type is not a generic type definition we can just do
            // a single assignable check.
            //
            if ( !baseType.IsGenericTypeDefinition )
            {
                return baseType.IsAssignableFrom( type );
            }

            //
            // Base type is a generic type definition, such as List<>, so we
            // need to use some custom logic.
            //
            while ( type != null && type != typeof( object ) )
            {
                var typeOrGenericType = type.IsGenericType ? type.GetGenericTypeDefinition() : type;

                if ( typeOrGenericType == baseType )
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }

        /// <summary>
        /// Gets the generic type arguments associated with the given
        /// <paramref name="genericType"/> which must be an ancestor of <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to inspect.</param>
        /// <param name="genericType">The generic type ancestor whose arguments are to be retrieved.</param>
        /// <returns>An array of <see cref="Type" /> objects.</returns>
        /// <exception cref="ArgumentException">Must be a generic type definition. - genericType</exception>
        /// <exception cref="ArgumentException">Type was not a descendend. - genericType</exception>
        public static Type[] GetGenericArgumentsOfBaseType( this Type type, Type genericType )
        {
            if ( !genericType.IsGenericTypeDefinition )
            {
                throw new ArgumentException( "Must be a generic type definition.", nameof( genericType ) );
            }

            while ( type != null && type != typeof( object ) )
            {
                if ( type.IsGenericType && type.GetGenericTypeDefinition() == genericType )
                {
                    return type.GetGenericArguments();
                }

                type = type.BaseType;
            }

            throw new ArgumentException( "Type was not a descendend.", nameof( genericType ) );
        }

        #endregion Type Extensions
    }
}

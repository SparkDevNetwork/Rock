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
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.CustomTypeProviders;
using System.Reflection;
using System.Runtime.CompilerServices;

using Rock.Data;

namespace Rock.Core.EntitySearch
{
    /// <summary>
    /// Custom type provider for the Dynamic LINQ library used by Entity Search.
    /// This allows us to add in custom extension methods that should be available
    /// during entity search operations.
    /// </summary>
    class DynamicLinqCustomTypeProvider : IDynamicLinkCustomTypeProvider
    {
        #region Fields

        /// <summary>
        /// The default provider to use for resolving types.
        /// </summary>
        private readonly DefaultDynamicLinqCustomTypeProvider _defaultProvider;

        /// <summary>
        /// The cached custom types.
        /// </summary>
        private static HashSet<Type> _cachedCustomTypes;

        /// <summary>
        /// The cached extension methods.
        /// </summary>
        private static Dictionary<Type, List<MethodInfo>> _cachedExtensionMethods;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicLinqCustomTypeProvider"/> class.
        /// </summary>
        public DynamicLinqCustomTypeProvider()
        {
            _defaultProvider = new DefaultDynamicLinqCustomTypeProvider( true );
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public HashSet<Type> GetCustomTypes()
        {
            if ( _cachedCustomTypes == null )
            {
                _cachedCustomTypes = new HashSet<Type>
                {
                    typeof( DynamicLinqQueryExtensionMethods )
                };
            }

            return _cachedCustomTypes;
        }

        /// <inheritdoc />
        public Dictionary<Type, List<MethodInfo>> GetExtensionMethods()
        {
            if ( _cachedExtensionMethods == null )
            {
                var customTypes = GetCustomTypes();
                var extensionMethods = new Dictionary<Type, List<MethodInfo>>();

                foreach ( var customType in customTypes )
                {
                    var methods = customType.GetMethods( BindingFlags.Static | BindingFlags.Public )
                        .Where( m => m.IsDefined( typeof( ExtensionAttribute ), false ) );

                    foreach ( var method in methods )
                    {
                        var parameters = method.GetParameters();
                        var type = parameters[0].ParameterType;
                        List<Type> typesToPopulate;

                        // Dynamic LINQ does not support extension method inheritence.
                        // Meaning, if the extension method is for IEntity, it will not
                        // be matched for a Person. So we need to trick it and register
                        // the method for all types that implement IEntity.
                        if ( type == typeof( IEntity ) )
                        {
                            typesToPopulate = Reflection.FindTypes( typeof( IEntity ) ).Values.ToList();
                        }
                        else
                        {
                            typesToPopulate = new List<Type> { type };
                        }

                        foreach ( var methodType in typesToPopulate )
                        {
                            if ( !extensionMethods.TryGetValue( methodType, out var typeMethods ) )
                            {
                                typeMethods = new List<MethodInfo>();
                                extensionMethods.Add( methodType, typeMethods );
                            }

                            typeMethods.Add( method );
                        }
                    }
                }

                _cachedExtensionMethods = extensionMethods;
            }

            return _cachedExtensionMethods;
        }

        /// <inheritdoc />
        public Type ResolveType( string typeName )
        {
            return _defaultProvider.ResolveType( typeName );
        }

        /// <inheritdoc />
        public Type ResolveTypeBySimpleName( string simpleTypeName )
        {
            return _defaultProvider.ResolveTypeBySimpleName( simpleTypeName );
        }

        #endregion
    }
}

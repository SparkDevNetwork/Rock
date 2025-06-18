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
using System.Reflection;

using Rock.SystemGuid;

namespace Rock.Extension
{
    /// <summary>
    /// A loader for light components that are simple C# classes. This handles
    /// the actual loading of the types using reflection so that it can be
    /// registered as a singleton by the containers can be registered scoped.
    /// </summary>
    /// <typeparam name="TComponent">The type of component to be loaded.</typeparam>
    internal class LightComponentLoader<TComponent>
        where TComponent : LightComponent
    {
        /// <summary>
        /// The dictionary of component types that have been detected in the
        /// loaded assemblies. This will only include types that are available
        /// to this running instance.
        /// </summary>
        private readonly Lazy<IReadOnlyDictionary<Guid, Type>> _typesByGuid = new Lazy<IReadOnlyDictionary<Guid, Type>>( LoadTypes );

        /// <summary>
        /// The dictionary of component types that have been detected in the
        /// running system.
        /// </summary>
        public IReadOnlyDictionary<Guid, Type> TypesByGuid => _typesByGuid.Value;

        /// <summary>
        /// Load the types using reflection. This is called lazily so that
        /// we don't waste CPU cycles at startup until we actually need them.
        /// The components must be decorated with an <see cref="EntityTypeGuidAttribute"/>
        /// in order to be included in the list.
        /// </summary>
        /// <returns>A dictionary of component types with the key being the component unique identifier.</returns>
        private static IReadOnlyDictionary<Guid, Type> LoadTypes()
        {
            try
            {
                var sortedTypes = Reflection.FindTypes( typeof( TComponent ) );
                var typesByGuid = new Dictionary<Guid, Type>();

                foreach ( var type in sortedTypes.Values )
                {
                    var entityTypeGuidAttribute = type.GetCustomAttribute<EntityTypeGuidAttribute>();

                    if ( entityTypeGuidAttribute == null )
                    {
                        continue;
                    }

                    typesByGuid.TryAdd( entityTypeGuidAttribute.Guid, type );
                }

                return typesByGuid;
            }
            catch ( Exception ex )
            {
                System.Diagnostics.Debug.WriteLine( $"Error loading entity container types: {ex.Message}" );

                return new Dictionary<Guid, Type>();
            }
        }
    }
}

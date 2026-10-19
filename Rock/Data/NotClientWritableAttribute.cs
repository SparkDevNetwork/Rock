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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace Rock.Data
{
    /// <summary>
    /// Marks a model property that is owned by Rock (for example by a storage
    /// provider) and must never be set by a client through the generic REST
    /// endpoints. v1 and v2 PATCH reject a body that names the property, v1
    /// and v2 PUT keep the stored value, and v1 POST and v2 Create clear the
    /// value so Rock sets it when the record is saved. Server code can still
    /// set the property.
    /// </summary>
    /// <remarks>
    /// Only place this on nullable or reference type properties, because
    /// <see cref="ClearValues(DbContext, object, Type)"/> sets the value to null.
    /// </remarks>
    [AttributeUsage( AttributeTargets.Property, Inherited = true, AllowMultiple = false )]
    internal sealed class NotClientWritableAttribute : System.Attribute
    {
        /// <summary>
        /// The names of the marked properties, cached by model type.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, IReadOnlyCollection<string>> _propertyNamesByType = new ConcurrentDictionary<Type, IReadOnlyCollection<string>>();

        /// <summary>
        /// Gets the names of the properties on the model type that are marked
        /// with <see cref="NotClientWritableAttribute"/>.
        /// </summary>
        /// <param name="modelType">The model type, such as <c>typeof( BinaryFile )</c>.</param>
        /// <returns>The property names, empty when the model has none.</returns>
        internal static IReadOnlyCollection<string> GetPropertyNames( Type modelType )
        {
            return _propertyNamesByType.GetOrAdd( modelType, t => new HashSet<string>(
                t.GetProperties( BindingFlags.Public | BindingFlags.Instance )
                    .Where( p => p.IsDefined( typeof( NotClientWritableAttribute ), true ) )
                    .Select( p => p.Name ),
                StringComparer.Ordinal ) );
        }

        /// <summary>
        /// Puts the stored value back on a tracked, modified entity after a
        /// client's values were copied onto it. Callers pass the model type
        /// (<c>typeof( T )</c>), never the runtime type of a tracked entity,
        /// so EF proxy types do not matter. Does nothing, and does not touch
        /// the context, when the model has no marked properties.
        /// </summary>
        /// <param name="context">The context that tracks the entity.</param>
        /// <param name="entity">The tracked entity.</param>
        /// <param name="modelType">The model type.</param>
        internal static void RestoreOriginalValues( DbContext context, object entity, Type modelType )
        {
            var names = GetPropertyNames( modelType );

            if ( names.Count == 0 )
            {
                return;
            }

            var entry = context.Entry( entity );

            foreach ( var name in names )
            {
                if ( entry.CurrentValues.PropertyNames.Contains( name ) )
                {
                    entry.CurrentValues[name] = entry.OriginalValues[name];
                }
            }
        }

        /// <summary>
        /// Clears client supplied values on a newly added entity so Rock sets
        /// them when the record is inserted. Does nothing, and does not touch
        /// the context, when the model has no marked properties.
        /// </summary>
        /// <param name="context">The context that tracks the entity.</param>
        /// <param name="entity">The added entity.</param>
        /// <param name="modelType">The model type.</param>
        internal static void ClearValues( DbContext context, object entity, Type modelType )
        {
            var names = GetPropertyNames( modelType );

            if ( names.Count == 0 )
            {
                return;
            }

            var entry = context.Entry( entity );

            foreach ( var name in names )
            {
                if ( entry.CurrentValues.PropertyNames.Contains( name ) )
                {
                    entry.CurrentValues[name] = null;
                }
            }
        }
    }
}

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

using Rock.Model;

namespace Rock.Data
{
    /// <summary>
    /// Marks a model property that holds a <see cref="BinaryFile"/> identifier
    /// which a client may only point at a file it uploaded. The v1 and v2 REST
    /// write endpoints refuse the save when a marked property is set to a file
    /// other than the one already stored or a temporary file uploaded by the
    /// current person. Clearing a nullable value is always allowed. Server code can
    /// still set the property.
    /// </summary>
    /// <remarks>
    /// Only place this on <c>int</c> or <c>int?</c> properties that hold a binary file identifier.
    /// </remarks>
    [AttributeUsage( AttributeTargets.Property, Inherited = true, AllowMultiple = false )]
    internal sealed class UploadedBinaryFileOnlyAttribute : System.Attribute
    {
        /// <summary>
        /// The names of the marked properties, cached by model type.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, IReadOnlyCollection<string>> _propertyNamesByType = new ConcurrentDictionary<Type, IReadOnlyCollection<string>>();

        /// <summary>
        /// Gets the names of the properties on the model type that are marked
        /// with <see cref="UploadedBinaryFileOnlyAttribute"/>.
        /// </summary>
        /// <param name="modelType">The model type, such as <c>typeof( Site )</c>.</param>
        /// <returns>The property names, empty when the model has none.</returns>
        internal static IReadOnlyCollection<string> GetPropertyNames( Type modelType )
        {
            return _propertyNamesByType.GetOrAdd( modelType, t => new HashSet<string>(
                t.GetProperties( BindingFlags.Public | BindingFlags.Instance )
                    .Where( p => p.IsDefined( typeof( UploadedBinaryFileOnlyAttribute ), true ) )
                    .Select( p => p.Name ),
                StringComparer.Ordinal ) );
        }

        /// <summary>
        /// Determines whether the person may save the binary file values a
        /// client put on a tracked entity. Each marked value must be empty,
        /// the value already stored, or a temporary file the person uploaded,
        /// the same rule the detail blocks apply. Callers pass the model type
        /// (<c>typeof( T )</c>), never the runtime type of a tracked entity,
        /// so EF proxy types do not matter. Returns <c>true</c>, and does not
        /// touch the context, when the model has no marked properties.
        /// </summary>
        /// <param name="context">The context that tracks the entity. It must be a <see cref="RockContext"/> when the model has marked properties.</param>
        /// <param name="entity">The tracked entity, either added or loaded from the database.</param>
        /// <param name="modelType">The model type.</param>
        /// <param name="person">The person saving the entity, usually the current person.</param>
        /// <returns><c>true</c> if every marked value may be saved; otherwise <c>false</c>.</returns>
        internal static bool AreValuesAllowed( DbContext context, object entity, Type modelType, Person person )
        {
            var names = GetPropertyNames( modelType );

            if ( names.Count == 0 )
            {
                return true;
            }

            var entry = context.Entry( entity );

            // A new record has nothing stored yet, so every file it names must
            // be the person's own upload.
            var hasStoredValues = entry.State == EntityState.Modified || entry.State == EntityState.Unchanged;

            // The lookup is a projection that EF does not track, so it is safe
            // to run on the context that holds the pending changes.
            var binaryFileService = new BinaryFileService( ( RockContext ) context );

            foreach ( var name in names )
            {
                if ( !entry.CurrentValues.PropertyNames.Contains( name ) )
                {
                    continue;
                }

                var binaryFileId = ( int? ) entry.CurrentValues[name];
                var storedBinaryFileId = hasStoredValues ? ( int? ) entry.OriginalValues[name] : null;

                if ( !binaryFileService.IsUploadedBinaryFileAllowedForPerson( binaryFileId, storedBinaryFileId, person ) )
                {
                    return false;
                }
            }

            return true;
        }
    }
}

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
using System.Collections;

using Rock.Data;

namespace Rock.Utility
{
    /// <summary>
    /// This is a special use equality comparer used when serializing objects
    /// to a JSON stream. This helps to prevent circular references when working
    /// on a set of objects that used .AsNoTracking() since Entity Framework
    /// will create a new proxy object for each reference to the same entity.
    /// </summary>
    internal class EntityReferenceEqualityComparer : IEqualityComparer
    {
        /// <inheritdoc/>
        public new bool Equals( object x, object y )
        {
            // If both objects are entity objects and both have been saved to
            // or loaded from the database, then perform a custom compare.
            if ( x is IEntity xEntity && y is IEntity yEntity && xEntity.Id != 0 && yEntity.Id != 0 )
            {
                // The entities are the considered the same if they have the
                // same TypeId (entity type) and Id (entity identifier).
                return xEntity.TypeId == yEntity.TypeId
                    && xEntity.Id == yEntity.Id;
            }

            // Otherwise fall back to generic comparison.
            return x == y;
        }

        /// <inheritdoc/>
        public int GetHashCode( object obj )
        {
            return obj.GetHashCode();
        }
    }
}

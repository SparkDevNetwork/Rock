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
using Rock.Data;

namespace Rock
{
    /// <summary>
    /// Provides conversion methods to convert to and from the Rock
    /// <see cref="EntityContextState"/> enumeration and the native
    /// platform enumeration.
    /// </summary>
    public static class EntityContextStateExtensions
    {
        /// <summary>
        /// Converts the native EF6 entity state enumeration into the Rock
        /// <see cref="EntityContextState"/> enumeration.
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <returns>The entity state as a Rock enumeration value.</returns>
        public static EntityContextState ToEntityContextState( this System.Data.Entity.EntityState entityState )
        {
            switch ( entityState )
            {
                case System.Data.Entity.EntityState.Unchanged:
                    return EntityContextState.Unchanged;

                case System.Data.Entity.EntityState.Added:
                    return EntityContextState.Added;

                case System.Data.Entity.EntityState.Deleted:
                    return EntityContextState.Deleted;

                case System.Data.Entity.EntityState.Modified:
                    return EntityContextState.Modified;

                case System.Data.Entity.EntityState.Detached:
                default:
                    return EntityContextState.Detached;
            }
        }
    }
}

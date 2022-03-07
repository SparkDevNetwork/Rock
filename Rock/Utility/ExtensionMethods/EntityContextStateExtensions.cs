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

#if REVIEW_NET5_0_OR_GREATER
using EFEntityState = Microsoft.EntityFrameworkCore.EntityState;
#else
using EFEntityState = System.Data.Entity.EntityState;
#endif

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
        public static EntityContextState ToEntityContextState( this EFEntityState entityState )
        {
            switch ( entityState )
            {
                case EFEntityState.Unchanged:
                    return EntityContextState.Unchanged;

                case EFEntityState.Added:
                    return EntityContextState.Added;

                case EFEntityState.Deleted:
                    return EntityContextState.Deleted;

                case EFEntityState.Modified:
                    return EntityContextState.Modified;

                case EFEntityState.Detached:
                default:
                    return EntityContextState.Detached;
            }
        }
    }
}

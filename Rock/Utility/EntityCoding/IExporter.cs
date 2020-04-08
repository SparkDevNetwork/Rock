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
using System.Collections.Generic;

using Rock.Data;

namespace Rock.Utility.EntityCoding
{
    /// <summary>
    /// Defines the interface methods that are available to all exporter components.
    /// </summary>
    public interface IExporter
    {
        /// <summary>
        /// Determines whether the path to an entity should be considered critical. A critical
        /// entity is one that MUST exist on the target system in order for the export/import to
        /// succeed, as such a critical entity is always included.
        /// </summary>
        /// <param name="path">The path to the queued entity object that is being checked.</param>
        /// <returns><c>true</c> if the path is critical; otherwise, <c>false</c>.</returns>
        bool IsPathCritical( EntityPath path );

        /// <summary>
        /// Determines if the entity at the given path requires a new Guid value when it's imported
        /// onto the target system. On import, if an entity of that type and Guid already exists then
        /// it is not imported and a reference to the existing entity is used instead.
        /// </summary>
        /// <param name="path">The path to the queued entity object that is being checked.</param>
        /// <returns><c>true</c> if the path requires a new Guid value; otherwise, <c>false</c></returns>
        bool DoesPathNeedNewGuid( EntityPath path );

        /// <summary>
        /// Determines if the property at the given path should be followed to it's referenced entity.
        /// This is called for both referenced entities and child entities.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        bool ShouldFollowPathProperty( EntityPath path );

        /// <summary>
        /// Gets any custom references for the entity at the given path.
        /// </summary>
        /// <param name="parentEntity">The entity that will later be encoded.</param>
        /// <param name="path">The path to the parent entity.</param>
        /// <returns>A collection of references that should be applied to the encoded entity.</returns>
        ICollection<Reference> GetUserReferencesForPath( IEntity parentEntity, EntityPath path );
    }
}

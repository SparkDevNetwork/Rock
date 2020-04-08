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
using System.Linq;

using Rock.Data;

namespace Rock.Utility.EntityCoding
{
    /// <summary>
    /// Defines the rules for exporting a WorkflowType.
    /// </summary>
    /// <seealso cref="EntityCoding.IExporter" />
    public class WorkflowTypeExporter : IExporter
    {
        /// <summary>
        /// Determines if the entity at the given path requires a new Guid value when it's imported
        /// onto the target system. On import, if an entity of that type and Guid already exists then
        /// it is not imported and a reference to the existing entity is used instead.
        /// </summary>
        /// <param name="path">The path to the queued entity object that is being checked.</param>
        /// <returns>
        ///   <c>true</c> if the path requires a new Guid value; otherwise, <c>false</c>
        /// </returns>
        public bool DoesPathNeedNewGuid( EntityPath path )
        {
            return ( path == "" ||
                path == "AttributeTypes" ||
                path == "AttributeTypes.AttributeQualifiers" ||
                path == "ActivityTypes" ||
                path == "ActivityTypes.AttributeTypes" ||
                path == "ActivityTypes.AttributeTypes.AttributeQualifiers" ||
                path == "ActivityTypes.ActionTypes" ||
                path == "ActivityTypes.ActionTypes.AttributeValues" ||
                path == "ActivityTypes.ActionTypes.WorkflowFormId" ||
                path == "ActivityTypes.ActionTypes.WorkflowFormId.FormAttributes" );
        }

        /// <summary>
        /// Gets any custom references for the entity at the given path.
        /// </summary>
        /// <param name="parentEntity">The entity that will later be encoded.</param>
        /// <param name="path">The path to the parent entity.</param>
        /// <returns>
        /// A collection of references that should be applied to the encoded entity.
        /// </returns>
        public ICollection<Reference> GetUserReferencesForPath( IEntity parentEntity, EntityPath path )
        {
            if ( path == "" )
            {
                return new List<Reference>
                {
                    Reference.UserDefinedReference( "CategoryId", "WorkflowCategory" )
                };
            }

            return null;
        }

        /// <summary>
        /// Determines whether the path to an entity should be considered critical. A critical
        /// entity is one that MUST exist on the target system in order for the export/import to
        /// succeed, as such a critical entity is always included.
        /// </summary>
        /// <param name="path">The path to the queued entity object that is being checked.</param>
        /// <returns>
        ///   <c>true</c> if the path is critical; otherwise, <c>false</c>.
        /// </returns>
        public bool IsPathCritical( EntityPath path )
        {
            return ( DoesPathNeedNewGuid( path ) );
        }

        /// <summary>
        /// Determines if the property at the given path should be followed to it's referenced entity.
        /// This is called for both referenced entities and child entities.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public bool ShouldFollowPathProperty( EntityPath path )
        {
            if ( path == "CategoryId" )
            {
                return false;
            }

            if ( path.Count > 0 )
            {
                var lastComponent = path.Last();

                if ( lastComponent.Entity.TypeName == "Rock.Model.DefinedType" && lastComponent.PropertyName == "DefinedValues" )
                {
                    return false;
                }
            }

            return true;
        }
    }
}

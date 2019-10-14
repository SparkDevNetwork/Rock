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
using System.Text;

using Rock.Data;

namespace Rock.Utility.EntityCoding
{
    /// <summary>
    /// Tracks entities and related information of entities that are queued up to be encoded.
    /// </summary>
    public class QueuedEntity
    {
        #region Properties

        /// <summary>
        /// The entity that is queued up for processing.
        /// </summary>
        public IEntity Entity { get; private set; }

        /// <summary>
        /// A list of all paths that we took to reach this entity.
        /// </summary>
        public List<EntityPath> ReferencePaths { get; private set; }

        /// <summary>
        /// Gets or sets the referenced entities.
        /// </summary>
        public List<KeyValuePair<string, IEntity>> ReferencedEntities { get; private set; }

        /// <summary>
        /// Gets or sets the user references.
        /// </summary>
        public Dictionary<string, Reference> UserReferences { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this queued entity requires a new Guid
        /// value be generated for it on import.
        /// </summary>
        public bool RequiresNewGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this queued entity is critical to the import
        /// process and must be included in the export data.
        /// </summary>
        public bool IsCritical { get; set; }

        /// <summary>
        /// During the encode process this will be filled in with the encoded
        /// entity data so that we can keep a link between the IEntity and the
        /// encoded data until we are done.
        /// </summary>
        public EncodedEntity EncodedEntity { get; set; }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Initialize a new queued entity for the given access path.
        /// </summary>
        /// <param name="entity">The entity that is going to be placed in the queue.</param>
        /// <param name="path">The initial path used to reach this entity.</param>
        public QueuedEntity( IEntity entity, EntityPath path )
        {
            Entity = entity;
            ReferencePaths = new List<EntityPath>
            {
                path
            };
            ReferencedEntities = new List<KeyValuePair<string, IEntity>>();
            UserReferences = new Dictionary<string, Reference>();
        }

        /// <summary>
        /// Add a new entity path reference to this existing entity.
        /// </summary>
        /// <param name="path">The path that can be used to reach this entity.</param>
        public void AddReferencePath( EntityPath path )
        {
            ReferencePaths.Add( path );
        }

        /// <summary>
        /// This is used for debug output to display the entity information and the path(s)
        /// that we took to find it.
        /// </summary>
        /// <returns>A string that describes this queued entity.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            EntityPath primaryPath = ReferencePaths[0];

            sb.AppendFormat( "{0} {1}", Entity.TypeName, Entity.Guid );
            foreach ( var p in ReferencePaths )
            {
                sb.AppendFormat( "\n\tPath" );
                foreach ( var e in p )
                {
                    sb.AppendFormat( "\n\t\t{0} {2} {1}", e.Entity.TypeName, e.PropertyName, e.Entity.Guid );
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Checks if this entity can be reached by the property path. For example if exporting a
        /// WorkflowType and you want to check if this entity is a WorkflowActionType for this
        /// workflow then you would use the property path "ActivityTypes.ActionTypes".
        /// </summary>
        /// <param name="propertyPath">The period delimited list of properties to reach this entity.</param>
        /// <returns>true if this entity can be reached by the property path, false if not.</returns>
        public bool ContainsPropertyPath( string propertyPath )
        {
            var properties = propertyPath.Split( '.' );

            foreach ( var path in ReferencePaths )
            {
                if ( path.Count == properties.Length )
                {
                    int i = 0;
                    for ( i = 0; i < path.Count; i++ )
                    {
                        if ( path[i].PropertyName != properties[i] )
                        {
                            break;
                        }
                    }

                    if ( i == path.Count )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion
    }
}

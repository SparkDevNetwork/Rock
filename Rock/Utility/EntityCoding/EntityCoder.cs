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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Utility.EntityCoding
{
    /// <summary>
    /// A helper class for importing / exporting entities into and out of Rock.
    /// </summary>
    public class EntityCoder : CodingHelper
    {
        #region Properties

        /// <summary>
        /// The list of entities that are queued up to be encoded. This is
        /// an ordered list and the entities will be encoded/decoded in this
        /// order.
        /// </summary>
        public List<QueuedEntity> Entities { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new Helper object for facilitating the export/import of entities.
        /// </summary>
        /// <param name="rockContext">The RockContext to work in when exporting or importing.</param>
        public EntityCoder( RockContext rockContext )
            : base( rockContext )
        {
            Entities = new List<QueuedEntity>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a root/primary entity to the queue list.
        /// </summary>
        /// <param name="entity">The entity that is to be included in the export.</param>
        /// <param name="exporter">The exporter that will handle processing for this entity.</param>
        public void EnqueueEntity( IEntity entity, IExporter exporter )
        {
            EnqueueEntity( entity, new EntityPath(), true, exporter );
        }

        /// <summary>
        /// Process the queued list of entities that are waiting to be encoded. This
        /// encodes all entities, generates new Guids for any entities that need them,
        /// and then maps all references to the new Guids.
        /// </summary>
        /// <returns>
        /// A DataContainer that is ready for JSON export.
        /// </returns>
        public ExportedEntitiesContainer GetExportedEntities()
        {
            var container = new ExportedEntitiesContainer();

            //
            // Find out if we need to give new Guid values to any entities.
            //
            foreach ( var queuedEntity in Entities )
            {
                queuedEntity.EncodedEntity = Export( queuedEntity );

                if ( queuedEntity.ReferencePaths[0].Count == 0 || queuedEntity.RequiresNewGuid )
                {
                    queuedEntity.EncodedEntity.GenerateNewGuid = true;
                }
            }

            //
            // Convert to a data container.
            //
            foreach ( var queuedEntity in Entities )
            {
                container.Entities.Add( queuedEntity.EncodedEntity );

                if ( queuedEntity.ReferencePaths.Count == 1 && queuedEntity.ReferencePaths[0].Count == 0 )
                {
                    container.RootEntities.Add( queuedEntity.EncodedEntity.Guid );
                }
            }

            return container;
        }

        /// <summary>
        /// Export the given entity into an EncodedEntity object. This can be used later to
        /// reconstruct the entity.
        /// </summary>
        /// <param name="queuedEntity">The queued entity.</param>
        /// <returns>
        /// The exported data that can be imported.
        /// </returns>
        protected EncodedEntity Export( QueuedEntity queuedEntity )
        {
            EncodedEntity encodedEntity = new EncodedEntity();
            Type entityType = GetEntityType( queuedEntity.Entity );

            encodedEntity.Guid = queuedEntity.Entity.Guid;
            encodedEntity.EntityType = entityType.FullName;

            //
            // Generate the standard properties and references.
            //
            foreach ( var property in GetEntityProperties( queuedEntity.Entity ) )
            {
                //
                // Don't encode IEntity properties, we should have the Id encoded instead.
                //
                if ( typeof( IEntity ).IsAssignableFrom( property.PropertyType ) )
                {
                    continue;
                }

                //
                // Don't encode IEnumerable properties. Those should be included as
                // their own entities to be encoded later.
                //
                if ( property.PropertyType.GetInterface( "IEnumerable" ) != null &&
                    property.PropertyType.GetGenericArguments().Length == 1 &&
                    typeof( IEntity ).IsAssignableFrom( property.PropertyType.GetGenericArguments()[0] ) )
                {
                    continue;
                }

                encodedEntity.Properties.Add( property.Name, property.GetValue( queuedEntity.Entity ) );
            }

            //
            // Run any post-process transforms.
            //
            foreach ( var processor in FindEntityProcessors( entityType ) )
            {
                var data = processor.ProcessExportedEntity( queuedEntity.Entity, encodedEntity, this );

                if ( data != null )
                {
                    encodedEntity.AddTransformData( processor.Identifier.ToString(), data );
                }
            }

            //
            // Generate the references to other entities.
            //
            foreach ( var x in queuedEntity.ReferencedEntities )
            {
                encodedEntity.MakePropertyIntoReference( x.Key, x.Value );
            }

            //
            // Add in the user references.
            //
            encodedEntity.References.AddRange( queuedEntity.UserReferences.Values );

            return encodedEntity;
        }

        /// <summary>
        /// Adds an entity to the queue list. This provides circular reference checking as
        /// well as ensuring that proper order is maintained for all entities.
        /// </summary>
        /// <param name="entity">The entity that is to be included in the export.</param>
        /// <param name="path">The entity path that lead to this entity being encoded.</param>
        /// <param name="entityIsCritical">True if the entity is critical, that is referenced directly.</param>
        /// <param name="exporter">The exporter that will handle processing for this entity.</param>
        protected void EnqueueEntity( IEntity entity, EntityPath path, bool entityIsCritical, IExporter exporter )
        {
            //
            // These are system generated rows, we should never try to backup or restore them.
            //
            if ( entity.TypeName == "Rock.Model.EntityType" || entity.TypeName == "Rock.Model.FieldType" )
            {
                return;
            }

            //
            // If the entity is already in our path that means we are beginning a circular
            // reference so we can just ignore this one.
            //
            if ( path.Where( e => e.Entity.Guid == entity.Guid ).Any() )
            {
                return;
            }

            //
            // Find the entities that this entity references, in other words entities that must
            // exist before this one can be created, and queue them up.
            //
            var referencedEntities = FindReferencedEntities( entity, path, exporter );
            foreach ( var r in referencedEntities )
            {
                if ( r.Value != null )
                {
                    var refPath = path + new EntityPathComponent( entity, r.Key );
                    EnqueueEntity( r.Value, refPath, true, exporter );
                }
            }

            //
            // If we already know about the entity, add a reference to it and return.
            //
            var queuedEntity = Entities.Where( e => e.Entity.Guid == entity.Guid ).FirstOrDefault();
            if ( queuedEntity == null )
            {
                queuedEntity = new QueuedEntity( entity, path.Clone() );
                Entities.Add( queuedEntity );
            }
            else
            {
                //
                // We have already visited this entity from the same parent. Not sure why we are here.
                //
                if ( path.Any() && queuedEntity.ReferencePaths.Where( r => r.Any() && r.Last().Entity.Guid == path.Last().Entity.Guid ).Any() )
                {
                    return;
                }

                queuedEntity.AddReferencePath( path.Clone() );
            }

            //
            // Add any new referenced properties/entities that may have been supplied for this
            // entity, as it's possible that has changed based on the path we took to get here.
            //
            foreach ( var r in referencedEntities )
            {
                if ( !queuedEntity.ReferencedEntities.Any( e => e.Key == r.Key && e.Value == r.Value ) )
                {
                    queuedEntity.ReferencedEntities.Add( new KeyValuePair<string, IEntity>( r.Key, r.Value ) );
                }
            }

            //
            // Mark the entity as critical if it's a root entity or is otherwise specified as critical.
            //
            if ( path.Count == 0 || entityIsCritical || exporter.IsPathCritical( path ) )
            {
                queuedEntity.IsCritical = true;
            }

            //
            // Mark the entity as requiring a new Guid value if so indicated.
            //
            if ( exporter.DoesPathNeedNewGuid( path ) )
            {
                queuedEntity.RequiresNewGuid = true;
            }

            //
            // Find the entities that this entity has as children. This is usually the many side
            // of a one-to-many reference (such as a Workflow has many WorkflowActions, this would
            // get a list of the WorkflowActions).
            //
            var children = FindChildEntities( entity, path, exporter );
            children.ForEach( e => EnqueueEntity( e.Value, path + new EntityPathComponent( entity, e.Key ), false, exporter ) );

            //
            // Allow the exporter a chance to add custom reference values.
            //
            var userReferences = exporter.GetUserReferencesForPath( entity, path );
            if ( userReferences != null && userReferences.Any() )
            {
                foreach ( var r in userReferences )
                {
                    queuedEntity.UserReferences.AddOrReplace( r.Property, r );
                }
            }
        }

        /// <summary>
        /// Find entities that this object references directly. These are entities that must be
        /// created before this entity can be re-created.
        /// </summary>
        /// <param name="parentEntity">The parent entity whose references we need to find.</param>
        /// <param name="path">The property path that led us to this final property.</param>
        /// <param name="exporter">The object that handles filtering during an export process.</param>
        /// <returns>A dictionary that identify the property names and entites to be followed.</returns>
        protected List<KeyValuePair<string, IEntity>> FindReferencedEntities( IEntity parentEntity, EntityPath path, IExporter exporter )
        {
            var references = new List<KeyValuePair<string, IEntity>>();
            var properties = GetEntityProperties( parentEntity );

            //
            // Take a stab at any properties that end in "Id" and likely reference another
            // entity, such as a property called "WorkflowId" probably references the Workflow
            // entity and should be linked by Guid.
            //
            foreach ( var property in properties )
            {
                if ( property.Name.EndsWith( "Id" ) && ( property.PropertyType == typeof( int ) || property.PropertyType == typeof( Nullable<int> ) ) )
                {
                    var entityProperty = parentEntity.GetType().GetProperty( property.Name.Substring( 0, property.Name.Length - 2 ) );

                    if ( entityProperty == null || !typeof( IEntity ).IsAssignableFrom( entityProperty.PropertyType ) )
                    {
                        continue;
                    }

                    var value = ( IEntity ) entityProperty.GetValue( parentEntity );

                    if ( exporter.ShouldFollowPathProperty( path + new EntityPathComponent( parentEntity, property.Name ) ) )
                    {
                        references.Add( new KeyValuePair<string, IEntity>( property.Name, value ) );
                    }
                    else
                    {
                        references.Add( new KeyValuePair<string, IEntity>( property.Name, null ) );
                    }
                }
            }

            //
            // Allow for processors to adjust the list of children.
            //
            foreach ( var processor in FindEntityProcessors( GetEntityType( parentEntity ) ) )
            {
                processor.EvaluateReferencedEntities( parentEntity, references, this );
            }

            return references;
        }

        /// <summary>
        /// Generate the list of entities that reference this parent entity. These are entities that
        /// must be created after this entity has been created.
        /// </summary>
        /// <param name="parentEntity">The parent entity to find reverse-references to.</param>
        /// <param name="path">The property path that led us to this final property.</param>
        /// <param name="exporter">The object that handles filtering during an export process.</param>
        /// <returns>A list of KeyValuePairs that identify the property names and entites to be followed.</returns>
        protected List<KeyValuePair<string, IEntity>> FindChildEntities( IEntity parentEntity, EntityPath path, IExporter exporter )
        {
            List<KeyValuePair<string, IEntity>> children = new List<KeyValuePair<string, IEntity>>();

            var properties = GetEntityProperties( parentEntity );

            //
            // Take a stab at any properties that are an ICollection<IEntity> and treat those
            // as child entities.
            //
            foreach ( var property in properties )
            {
                if ( property.PropertyType.GetInterface( "IEnumerable" ) != null && property.PropertyType.GetGenericArguments().Length == 1 )
                {
                    if ( typeof( IEntity ).IsAssignableFrom( property.PropertyType.GetGenericArguments()[0] ) && exporter.ShouldFollowPathProperty( path + new EntityPathComponent( parentEntity, property.Name ) ) )
                    {
                        IEnumerable childEntities = ( IEnumerable ) property.GetValue( parentEntity );

                        foreach ( IEntity childEntity in childEntities )
                        {
                            children.Add( new KeyValuePair<string, IEntity>( property.Name, childEntity ) );
                        }
                    }
                }
            }

            //
            // We also need to pull in any attribute values. We have to pull attributes as well
            // since we might not have an actual value for that attribute yet and would need
            // it to pull the default value and definition.
            //
            if ( parentEntity is IHasAttributes attributedEntity )
            {
                if ( attributedEntity.Attributes == null )
                {
                    attributedEntity.LoadAttributes( RockContext );
                }

                foreach ( var item in attributedEntity.Attributes )
                {
                    var attrib = new AttributeService( RockContext ).Get( item.Value.Guid );

                    children.Add( new KeyValuePair<string, IEntity>( "Attributes", attrib ) );

                    var value = new AttributeValueService( RockContext ).Queryable()
                        .Where( v => v.AttributeId == attrib.Id && v.EntityId == attributedEntity.Id )
                        .FirstOrDefault();
                    if ( value != null )
                    {
                        children.Add( new KeyValuePair<string, IEntity>( "AttributeValues", value ) );
                    }
                }
            }

            //
            // Allow for processors to adjust the list of children.
            //
            foreach ( var processor in FindEntityProcessors( GetEntityType( parentEntity ) ) )
            {
                processor.EvaluateChildEntities( parentEntity, children, this );
            }

            return children;
        }

        #endregion
    }
}

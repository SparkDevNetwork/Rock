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
using System.Linq;

using Rock;
using Rock.Data;
using Rock.Model;

namespace Rock.Utility.EntityCoding
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Utility.EntityCoding.CodingHelper" />
    public class EntityDecoder : CodingHelper
    {
        #region Properties

        /// <summary>
        /// The map of original Guids to newly generated Guids.
        /// </summary>
        private Dictionary<Guid, Guid> GuidMap { get; set; }

        /// <summary>
        /// Contains the values provided by the user to be used during import.
        /// </summary>
        public Dictionary<string, IEntity> UserValues { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new Helper object for facilitating the export/import of entities.
        /// </summary>
        /// <param name="rockContext">The RockContext to work in when exporting or importing.</param>
        public EntityDecoder( RockContext rockContext )
            : base( rockContext )
        {
            GuidMap = new Dictionary<Guid, Guid>();
            UserValues = new Dictionary<string, IEntity>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Attempt to import the container of +ities into the Rock database. Creates
        /// a transaction inside the RockContext to perform all the entity creation so
        /// if an error occurs everything will be left in a clean state.
        /// </summary>
        /// <param name="container">The container of all the encoded entities.</param>
        /// <param name="dryRun">If true then we only attempt the import, nothing is actually saved.</param>
        /// <param name="messages">Any messages, errors or otherwise, that should be displayed to the user.</param>
        /// <returns>true if the import succeeded, false if it did not.</returns>
        public bool Import( ExportedEntitiesContainer container, bool dryRun, out List<string> messages )
        {
            messages = new List<string>();

            //
            // Ensure we know about all referenced entity types.
            //
            var missingTypes = container.GetMissingEntityTypes();
            if ( missingTypes.Any() )
            {
                messages.Add( string.Format( "The following EntityTypes are unknown and indicate you may be missing a plug-in: <ul><li>{0}</li></ul>", string.Join( "</li><li>", missingTypes ) ) );
                return false;
            }

            //
            // Generate a new Guid if we were asked to.
            //
            foreach ( var encodedEntity in container.Entities )
            {
                if ( encodedEntity.GenerateNewGuid )
                {
                    MapNewGuid( encodedEntity.Guid );
                }
            }

            using ( var transaction = RockContext.Database.BeginTransaction() )
            {
                try
                {
                    //
                    // Walk each encoded entity and either verify an existing entity or
                    // create a new entity.
                    //
                    foreach ( var encodedEntity in container.Entities )
                    {
                        Type entityType = FindEntityType( encodedEntity.EntityType );
                        Guid entityGuid = FindMappedGuid( encodedEntity.Guid );
                        var entity = GetExistingEntity( encodedEntity.EntityType, entityGuid );

                        if ( entity == null )
                        {
                            try
                            {
                                entity = CreateNewEntity( encodedEntity );
                            }
                            catch ( Exception e )
                            {
                                throw new Exception( String.Format( "Error importing encoded entity: {0}", encodedEntity.ToJson() ), e );
                            }

                            messages.Add( string.Format( "Created: {0}, {1}", encodedEntity.EntityType, entityGuid ) );
                        }
                        else
                        {
                            messages.Add( string.Format( "Found Existing: {0}, {1}", encodedEntity.EntityType, entityGuid ) );
                        }
                    }

                    //
                    // Either commit the transaction or roll it back if we are doing a dry run.
                    //
                    if ( !dryRun )
                    {
                        transaction.Commit();
                    }
                    else
                    {
                        transaction.Rollback();
                    }

                    return true;
                }
                catch ( Exception e )
                {
                    transaction.Rollback();

                    for ( Exception ex = e; ex != null; ex = ex.InnerException )
                    {
                        messages.Add( ex.Message + "\n" + ex.StackTrace );
                    }

                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the user defined value supplied to the decoder.
        /// </summary>
        /// <param name="key">The key that identifies the user value.</param>
        /// <returns>An object that was provided by the user or null.</returns>
        public object GetUserDefinedValue( string key )
        {
            if ( !UserValues.ContainsKey( key ) )
            {
                return null;
            }

            return UserValues[key];
        }

        /// <summary>
        /// Finds and returns a Guid from the mapping dictionary. If no mapping
        /// exists then the original Guid is returned.
        /// </summary>
        /// <param name="oldGuid">The original Guid value to map from.</param>
        /// <returns>The Guid value that should be used, may be the same as oldGuid.</returns>
        public Guid FindMappedGuid( Guid oldGuid )
        {
            return GuidMap.ContainsKey( oldGuid ) ? GuidMap[oldGuid] : oldGuid;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Creates a new entity in the database from the encoded information. The entity
        /// is saved before being returned.
        /// </summary>
        /// <param name="encodedEntity">The encoded entity information to create the new entity from.</param>
        /// <returns>A reference to the new entity.</returns>
        protected IEntity CreateNewEntity( EncodedEntity encodedEntity )
        {
            Type entityType = Reflection.FindType( typeof( IEntity ), encodedEntity.EntityType );
            var service = Reflection.GetServiceForEntityType( entityType, RockContext );

            if ( service != null )
            {
                var addMethod = service.GetType().GetMethod( "Add", new Type[] { entityType } );

                if ( addMethod != null )
                {
                    IEntity entity = ( IEntity ) Activator.CreateInstance( entityType );

                    RestoreEntityProperties( entity, encodedEntity );
                    entity.Guid = FindMappedGuid( encodedEntity.Guid );

                    //
                    // Do custom pre-save processing.
                    //
                    foreach ( var processor in FindEntityProcessors( entityType ) )
                    {
                        processor.ProcessImportedEntity( entity, encodedEntity, encodedEntity.GetTransformData( processor.Identifier.ToString() ), this );
                    }

                    //
                    // Special handling of AttributeQualifier because Guids may not be the same
                    // across installations and the AttributeId+Key columns make up a unique key.
                    //
                    if ( encodedEntity.EntityType == "Rock.Model.AttributeQualifier" )
                    {
                        var reference = encodedEntity.References.Where( r => r.Property == "AttributeId" ).First();
                        var attribute = GetExistingEntity( "Rock.Model.Attribute", FindMappedGuid( new Guid( ( string ) reference.Data ) ) );
                        string key = ( string ) encodedEntity.Properties["Key"];

                        var existingEntity = new AttributeQualifierService( RockContext )
                            .GetByAttributeId( attribute.Id )
                            .Where( a => a.Key == key )
                            .FirstOrDefault();

                        if ( existingEntity != null )
                        {
                            if ( entity.Guid != encodedEntity.Guid )
                            {
                                throw new Exception( "AttributeQualifier marked for new Guid but conflicting value already exists." );
                            }

                            GuidMap.AddOrReplace( encodedEntity.Guid, existingEntity.Guid );

                            return existingEntity;
                        }
                    }

                    //
                    // Special handling of Attribute's. The guid's might be different but if the entity type,
                    // entity qualifiers and key are the same, assume it's the same.
                    //
                    else if ( encodedEntity.EntityType == "Rock.Model.Attribute" )
                    {
                        var attribute = ( Rock.Model.Attribute ) entity;
                        var existingEntity = new AttributeService( RockContext )
                            .GetByEntityTypeId( attribute.EntityTypeId )
                            .Where( a => a.EntityTypeQualifierColumn == attribute.EntityTypeQualifierColumn && a.EntityTypeQualifierValue == attribute.EntityTypeQualifierValue && a.Key == attribute.Key )
                            .FirstOrDefault();

                        if ( existingEntity != null )
                        {
                            if ( entity.Guid != encodedEntity.Guid )
                            {
                                throw new Exception( "Attribute marked for new Guid but conflicting value already exists." );
                            }

                            GuidMap.AddOrReplace( encodedEntity.Guid, existingEntity.Guid );

                            return existingEntity;
                        }
                    }

                    //
                    // Special handling of AttributeValue's. The guid's might be different but if the attribute Id
                    // and entity Id are the same, assume it's the same.
                    //
                    else if ( encodedEntity.EntityType == "Rock.Model.AttributeValue" )
                    {
                        var attributeReference = encodedEntity.References.Where( r => r.Property == "AttributeId" ).First();
                        var attribute = GetExistingEntity( "Rock.Model.Attribute", FindMappedGuid( new Guid( ( string ) attributeReference.Data ) ) );
                        var entityReference = encodedEntity.References.Where( r => r.Property == "EntityId" ).First();
                        var entityRef = GetExistingEntity( entityReference.EntityType, FindMappedGuid( new Guid( ( string ) entityReference.Data ) ) );

                        var existingEntity = new AttributeValueService( RockContext )
                            .Queryable().Where( a => a.AttributeId == attribute.Id && a.EntityId == entityRef.Id )
                            .FirstOrDefault();

                        if ( existingEntity != null )
                        {
                            if ( entity.Guid != encodedEntity.Guid )
                            {
                                throw new Exception( "AttributeValue marked for new Guid but conflicting value already exists." );
                            }

                            GuidMap.AddOrReplace( encodedEntity.Guid, existingEntity.Guid );

                            return existingEntity;
                        }
                    }

                    addMethod.Invoke( service, new object[] { entity } );
                    RockContext.SaveChanges( true );

                    return entity;
                }
            }

            throw new Exception( string.Format( "Failed to create new database entity for {0}_{1}", encodedEntity.EntityType, encodedEntity.Guid ) );
        }

        /// <summary>
        /// Restore the property information from encodedEntity into the newly created entity.
        /// </summary>
        /// <param name="entity">The blank entity to be populated.</param>
        /// <param name="encodedEntity">The encoded entity data.</param>
        protected void RestoreEntityProperties( IEntity entity, EncodedEntity encodedEntity )
        {
            foreach ( var property in GetEntityProperties( entity ) )
            {
                //
                // If this is a plain property, just set the value.
                //
                if ( encodedEntity.Properties.ContainsKey( property.Name ) )
                {
                    var value = encodedEntity.Properties[property.Name];

                    //
                    // If this is a Guid, see if we need to remap it.
                    //
                    Guid? guidValue = null;
                    if ( value is Guid )
                    {
                        guidValue = ( Guid ) value;
                        value = FindMappedGuid( guidValue.Value );
                    }
                    else if ( value is string )
                    {
                        guidValue = ( ( string ) value ).AsGuidOrNull();
                        if ( guidValue.HasValue && guidValue.Value != FindMappedGuid( guidValue.Value ) )
                        {
                            value = FindMappedGuid( guidValue.Value ).ToString();
                        }
                    }

                    property.SetValue( entity, ChangeType( property.PropertyType, value ) );
                }
            }

            //
            // Restore all references.
            //
            foreach ( var reference in encodedEntity.References )
            {
                reference.Restore( entity, this );
            }
        }

        /// <summary>
        /// Creates a new map entry for the oldGuid. This generates a new Guid and
        /// stores a reference between the two.
        /// </summary>
        /// <param name="oldGuid">The original Guid value to be mapped from.</param>
        /// <returns>A new Guid value that should be used in place of oldGuid.</returns>
        protected Guid MapNewGuid( Guid oldGuid )
        {
            GuidMap.Add( oldGuid, Guid.NewGuid() );

            return GuidMap[oldGuid];
        }

        #endregion
    }
}

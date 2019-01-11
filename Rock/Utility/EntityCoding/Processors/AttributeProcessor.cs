using System;
using System.Collections.Generic;
using Rock;
using Rock.Data;

namespace Rock.Utility.EntityCoding.Processors
{
    /// <summary>
    /// Handles processing of Attribute entities.
    /// </summary>
    public class AttributeProcessor : EntityProcessor<Rock.Model.Attribute>
    {
        /// <summary>
        /// The unique identifier for this entity processor. This is used to identify the correct
        /// processor to use when importing so we match the one used during export.
        /// </summary>
        public override Guid Identifier { get { return new Guid( "67c72560-e047-495a-ad75-09ed920dae8a" ); } }

        /// <summary>
        /// Evaluate the list of referenced entities. This is a list of key value pairs that identify
        /// the property that the reference came from as well as the referenced entity itself. Implementations
        /// of this method may add or remove from this list. For example, an AttributeValue has
        /// the entity it is referencing in a EntityId column, but there is no general use information for
        /// what kind of entity it is. The processor can provide that information.
        /// </summary>
        /// <param name="entity">The parent entity of the references.</param>
        /// <param name="references"></param>
        /// <param name="helper">The helper class for this export.</param>
        /// <exception cref="System.Exception"></exception>
        protected override void EvaluateReferencedEntities( Rock.Model.Attribute entity, List<KeyValuePair<string, IEntity>> references, EntityCoder helper )
        {
            //
            // Only process if the entity qualifier column looks to be a reference to an Entity.
            //
            if ( entity.EntityTypeQualifierColumn == null || !entity.EntityTypeQualifierColumn.EndsWith( "Id" ) )
            {
                return;
            }

            //
            // Get the EntityType and Id of the qualifiers.
            //
            int? entityId = entity.EntityTypeQualifierValue.AsIntegerOrNull();
            var entityType = helper.FindEntityType( entity.EntityType.Name );

            if ( !entityId.HasValue || entityType == null )
            {
                return;
            }

            //
            // Try to get the property mentioned by the qualifier.
            //
            var property = entityType.GetProperty( entity.EntityTypeQualifierColumn.ReplaceLastOccurrence( "Id", string.Empty ) );

            if ( property == null )
            {
                return;
            }

            //
            // If the property is of type IEntity, then it is a reference we want to add.
            //
            if ( typeof( IEntity ).IsAssignableFrom( property.PropertyType ) )
            {
                var target = helper.GetExistingEntity( property.PropertyType.FullName, entityId.Value );

                if ( target != null )
                {
                    references.Add( new KeyValuePair<string, IEntity>( "EntityTypeQualifierValue", target ) );
                }
                else
                {
                    throw new Exception( string.Format( "Could not find referenced qualifier of Attribute {0}", entity.Guid ) );
                }
            }
        }
    }
}

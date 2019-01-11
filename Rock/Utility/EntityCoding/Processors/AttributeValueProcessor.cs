using System;
using System.Collections.Generic;
using Rock.Data;
using Rock.Model;

namespace Rock.Utility.EntityCoding.Processors
{
    /// <summary>
    /// Handles processing of AttributeValue entities.
    /// </summary>
    public class AttributeValueProcessor : EntityProcessor<AttributeValue>
    {
        /// <summary>
        /// The unique identifier for this entity processor. This is used to identify the correct
        /// processor to use when importing so we match the one used during export.
        /// </summary>
        public override Guid Identifier { get { return new Guid( "0c733598-46b1-4f59-854d-c5477dcfea17" ); } }

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
        protected override void EvaluateReferencedEntities( AttributeValue entity, List<KeyValuePair<string, IEntity>> references, EntityCoder helper )
        {
            if ( !entity.EntityId.HasValue || entity.Attribute == null )
            {
                return;
            }

            var target = helper.GetExistingEntity( entity.Attribute.EntityType.Name, entity.EntityId.Value );
            if ( target != null )
            {
                references.Add( new KeyValuePair<string, IEntity>( "EntityId", target ) );
            }
            else
            {
                throw new Exception( string.Format( "Cannot export AttributeValue {0} because we cannot determine what entity it references.", entity.Guid ) );
            }
        }
    }
}

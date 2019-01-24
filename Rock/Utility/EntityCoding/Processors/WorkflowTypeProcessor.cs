using System;
using System.Collections.Generic;
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;

namespace Rock.Utility.EntityCoding.Processors
{
    /// <summary>
    /// Handle processing of Workflow Type entities.
    /// </summary>
    /// <seealso cref="EntityCoding.EntityProcessor{Rock.Model.WorkflowType}" />
    class WorkflowTypeProcessor : EntityProcessor<WorkflowType>
    {
        /// <summary>
        /// The unique identifier for this entity processor. This is used to identify the correct
        /// processor to use when importing so we match the one used during export.
        /// </summary>
        public override Guid Identifier { get { return new Guid( "d924193f-bd22-4dd7-b203-4399673dcd32" ); } }

        /// <summary>
        /// Evaluate the list of child entities. This is a list of key value pairs that identify
        /// the property that the child came from as well as the child entity itself. Implementations
        /// of this method may add or remove from this list. For example, a WorkflowActionForm has
        /// it's actions encoded in a single string. This must processed to include any other
        /// objects that should exist (such as a DefinedValue for the button type).
        /// </summary>
        /// <param name="entity">The parent entity of the children.</param>
        /// <param name="children">The child entities and what properties of the parent they came from.</param>
        /// <param name="helper">The helper class for this export.</param>
        protected override void EvaluateChildEntities( WorkflowType entity, List<KeyValuePair<string, IEntity>> children, EntityCoder helper )
        {
            var attributeService = new AttributeService( helper.RockContext );

            var items = attributeService
                .GetByEntityTypeId( new Model.Workflow().TypeId ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "WorkflowTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( entity.Id.ToString() ) )
                .ToList();

            //
            // We have to special process the attributes since we modify them.
            //
            foreach ( var item in items )
            {
                children.Add( new KeyValuePair<string, IEntity>( "AttributeTypes", item ) );
            }
        }
    }
}

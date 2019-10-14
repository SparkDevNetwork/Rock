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

namespace Rock.Utility.EntityCoding.Processors
{
    /// <summary>
    /// Handles processing of Workflow Activity Type entities.
    /// </summary>
    public class WorkflowActivityTypeProcessor : EntityProcessor<WorkflowActivityType>
    {
        /// <summary>
        /// The unique identifier for this entity processor. This is used to identify the correct
        /// processor to use when importing so we match the one used during export.
        /// </summary>
        public override Guid Identifier { get { return new Guid( "e67efa08-fd93-4278-83db-55397f8dc9f0" ); } }

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
        protected override void EvaluateChildEntities( WorkflowActivityType entity, List<KeyValuePair<string, IEntity>> children, EntityCoder helper )
        {
            var attributeService = new AttributeService( helper.RockContext );

            var items = attributeService
                .GetByEntityTypeId( new WorkflowActivity().TypeId ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "ActivityTypeId", StringComparison.OrdinalIgnoreCase ) &&
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

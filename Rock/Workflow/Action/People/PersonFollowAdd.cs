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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Data.Entity;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Field;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Add Person to Follow Entity
    /// </summary>
    [ActionCategory( "People" )]
    [Description( "Adds a person to follow an entity." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Person Follow Add" )]

    [WorkflowAttribute( "Person", "Workflow attribute that contains the person who is following the entity.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [EntityTypeField( "Entity Type", "Workflow attribute that contains the entity type to follow.", true, "", 1, "EntityType" )]
    [WorkflowTextOrAttribute( "Entity To Follow", "Attribute Value", "The Entity Id or Guid or an attribute that contains the entity to follow. <span class='tip tip-lava'></span>", true, "", "", 2, "Entity" )]
    public class PersonFollowAdd : ActionComponent
    {
        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            // get person
            PersonAlias personAlias = null;
            string personAttributeValue = GetAttributeValue( action, "Person" );
            Guid? guidPersonAttribute = personAttributeValue.AsGuidOrNull();
            if ( guidPersonAttribute.HasValue )
            {
                var attributePerson = AttributeCache.Read( guidPersonAttribute.Value, rockContext );
                if ( attributePerson != null || attributePerson.FieldType.Class != "Rock.Field.Types.PersonFieldType" )
                {
                    Guid? attributePersonValue = action.GetWorklowAttributeValue( guidPersonAttribute.Value ).AsGuidOrNull();
                    if ( attributePersonValue.HasValue )
                    {
                        personAlias = new PersonAliasService( rockContext ).Queryable()
                            .Where( a => a.Guid.Equals( attributePersonValue.Value ) )
                            .FirstOrDefault();
                        if ( personAlias == null )
                        {
                            errorMessages.Add( string.Format( "Person could not be found for selected value ('{0}')!", guidPersonAttribute.ToString() ) );
                            return false;
                        }
                    }
                }
            }

            //get entity type
            EntityTypeCache entityType = null;
            Guid? guidEntityType = GetAttributeValue( action, "EntityType" ).AsGuidOrNull();
            if ( guidEntityType.HasValue )
            {
                entityType = EntityTypeCache.Read( guidEntityType.Value );
                if ( entityType == null )
                {
                    errorMessages.Add( string.Format( "Entity Type could not be found for selected value ('{0}')!", guidPersonAttribute.ToString() ) );
                    return false;
                }
            }

            //get entity
            IEntity entityToFollow = GetEntity( entityType, rockContext, action );
            if ( entityToFollow != null )
            {
                var followingService = new FollowingService( rockContext );

                var following = followingService.Queryable()
                    .FirstOrDefault( f =>
                        f.EntityTypeId == entityType.Id &&
                        f.EntityId == entityToFollow.Id &&
                        f.PersonAlias.Person.Id == personAlias.PersonId );

                if ( following == null )
                {
                    following = new Following();
                    following.EntityTypeId = entityType.Id;
                    following.EntityId = entityToFollow.Id;
                    following.PersonAliasId = personAlias.Id;
                    followingService.Add( following );
                    rockContext.SaveChanges();
                }

                return true;
            }
            else
            {
                action.AddLogEntry( "Invalid Value for Entity attribute" );

                return false;
            }

        }

        /// <summary>
        /// Gets the entity to follow
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        private IEntity GetEntity( EntityTypeCache entityType, RockContext rockContext, WorkflowAction action )
        {
            string entityValue = GetAttributeValue( action, "Entity" ).ResolveMergeFields( GetMergeFields( action ) );

            // If an ID was specified, just use that as the entity id to follow
            int? intEntity = entityValue.AsIntegerOrNull();
            if ( intEntity.HasValue )
            {
                return new EntityTypeService( rockContext ).GetEntity( entityType.Id, intEntity.Value );
            }

            Guid? guidEntity = entityValue.AsGuidOrNull();
            if ( guidEntity.HasValue )
            {
                // If the value is a Guid, it could either be a guid of an attribute, or the entity's guid.
                // Check for an attribute first.
                var attribute = AttributeCache.Read( guidEntity.Value, rockContext );
                if ( attribute != null )
                {
                    // It was for an attribute. That attribute's field type needs to be an IEntityFieldType (like person or group)
                    var entityFieldType = attribute.FieldType.Field as IEntityFieldType;
                    if ( entityFieldType != null )
                    {
                        return entityFieldType.GetEntity( action.GetWorklowAttributeValue( guidEntity.Value ) );
                    }
                }
                else
                {
                    // It was not for an attribute, so it must be the entity's guid
                    return new EntityTypeService( rockContext ).GetEntity( entityType.Id, guidEntity.Value );
                }
            }

            return null;
        }
    }
}

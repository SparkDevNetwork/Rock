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

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Field;
using Rock.Model;
using Rock.Web.Cache;

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
                var attributePerson = AttributeCache.Get( guidPersonAttribute.Value, rockContext );
                if ( attributePerson != null && attributePerson.FieldType.Class == "Rock.Field.Types.PersonFieldType" )
                {
                    Guid? attributePersonValue = action.GetWorklowAttributeValue( guidPersonAttribute.Value ).AsGuidOrNull();
                    if ( attributePersonValue.HasValue )
                    {
                        personAlias = new PersonAliasService( rockContext ).Queryable()
                            .Where( a => a.Guid.Equals( attributePersonValue.Value ) )
                            .FirstOrDefault();
                    }
                }
            }
            if ( personAlias == null )
            {
                errorMessages.Add( string.Format( "Person could not be found for selected value ('{0}')!", guidPersonAttribute.ToString() ) );
                return false;
            }

            //get entity type
            EntityTypeCache entityType = null;
            Guid? guidEntityType = GetAttributeValue( action, "EntityType" ).AsGuidOrNull();
            if ( guidEntityType.HasValue )
            {
                entityType = EntityTypeCache.Get( guidEntityType.Value );
                if ( entityType == null )
                {
                    errorMessages.Add( string.Format( "Entity Type could not be found for selected value ('{0}')!", guidPersonAttribute.ToString() ) );
                    return false;
                }
            }

            var followingService = new FollowingService( rockContext );

            //get entity(ies)
            List<IEntity> entitiesToFollow = GetEntities( entityType, rockContext, action );
            if ( entitiesToFollow != null && entitiesToFollow.Any() )
            {
                foreach ( var entityToFollow in entitiesToFollow )
                {
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
                    }
                }

                rockContext.SaveChanges();

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
        private List<IEntity> GetEntities( EntityTypeCache entityType, RockContext rockContext, WorkflowAction action )
        {
            var entityTypeService = new EntityTypeService( rockContext );

            List<IEntity> entities = new List<IEntity>();

            string entityValue = GetAttributeValue( action, "Entity" ).ResolveMergeFields( GetMergeFields( action ) );

            // If an ID was specified, just use that as the entity id to follow
            int? intEntity = entityValue.AsIntegerOrNull();
            if ( intEntity.HasValue )
            {
                AddEntityById( entityTypeService, entityType.Id, intEntity.Value, entities );
            }

            Guid? guidEntity = entityValue.AsGuidOrNull();
            if ( guidEntity.HasValue )
            {
                // If the value is a Guid, it could either be a guid of an attribute, or the entity's guid.
                // Check for an attribute first.
                var attribute = AttributeCache.Get( guidEntity.Value, rockContext );
                if ( attribute != null )
                {
                    // It was for an attribute, get the value
                    string attributeValue = action.GetWorklowAttributeValue( guidEntity.Value );

                    // First check if that attribute's field type is an IEntityFieldType (like person or group)
                    var entityFieldType = attribute.FieldType.Field as IEntityFieldType;
                    if ( entityFieldType != null )
                    {
                        var entity = entityFieldType.GetEntity( attributeValue );
                        if ( entity != null )
                        {
                            entities.Add( entity );
                        }
                    }
                    else 
                    {
                        // not an entity attribute... maybe its a list of ints or guids
                        var values = attributeValue.SplitDelimitedValues();
                        foreach( int intValue in values.AsIntegerList() )
                        {
                            AddEntityById( entityTypeService, entityType.Id, intValue, entities );
                        }
                        foreach( Guid guidValue in values.AsGuidList() )
                        { 
                            AddEntityByGuid( entityTypeService, entityType.Id, guidValue, entities );
                        }
                    }
                }
                else
                {
                    // It was not for an attribute, so it must be the entity's guid
                    AddEntityByGuid( entityTypeService, entityType.Id, guidEntity.Value, entities );
                }
            }

            return entities;
        }

        private void AddEntityById( EntityTypeService service, int entityTypeId, int? entityId, List<IEntity> entities )
        {
            if ( entityId.HasValue )
            {
                var entity = service.GetEntity( entityTypeId, entityId.Value );
                if ( entity != null )
                {
                    entities.Add( entity );
                }
            }
        }

        private void AddEntityByGuid( EntityTypeService service, int entityTypeId, Guid? entityGuid, List<IEntity> entities )
        {
            if ( entityGuid.HasValue )
            {
                var entity = service.GetEntity( entityTypeId, entityGuid.Value );
                if ( entity != null )
                {
                    entities.Add( entity );
                }
            }
        }
    }
}

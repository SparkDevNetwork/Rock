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
    [WorkflowTextOrAttribute( "Entity To Follow", "Attribute Value", "The Entity Id or an attribute that contains the person or group to follow. <span class='tip tip-lava'></span>", true, "", "", 2, "Entity" )]
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
            int entityId = 0;
            string attributeEntity = GetAttributeValue( action, "Entity" ).ResolveMergeFields( GetMergeFields( action ) );
            int? intEntity = attributeEntity.AsIntegerOrNull();
            Guid? guidEntity = attributeEntity.AsGuidOrNull();
            if ( intEntity.HasValue )
            {
                entityId = intEntity.Value;
            }
            else if ( guidEntity.HasValue )
            {
                var attribute = AttributeCache.Read( guidEntity.Value, rockContext );
                if ( attribute != null )
                {
                    Guid? entityValue = action.GetWorklowAttributeValue( guidEntity.Value ).AsGuidOrNull();
                    if ( entityValue == null )
                    {
                        action.AddLogEntry( "Invalid valid Value for Entity attribute" );
                        return false;
                    }
                    switch ( attribute.FieldType.Class )
                    {
                        case "Rock.Field.Types.PersonFieldType":
                            var person = new PersonAliasService( rockContext ).Queryable()
                                          .Where( a => a.Guid.Equals( entityValue.Value ) )
                                          .Select( a => a.Person )
                                          .FirstOrDefault();
                            if ( person == null )
                            {
                                action.AddLogEntry( "InValid Entity: Person not found" );
                                return false;
                            }
                            entityId = person.Id;
                            break;
                        case "Rock.Field.Types.GroupFieldType":
                            var group = new GroupService( rockContext ).Get( entityValue.Value );
                            if ( group == null )
                            {
                                action.AddLogEntry( "InValid Entity: Group not found" );
                                return false;
                            }
                            entityId = group.Id;
                            break;
                    }
                }
            }
            else
            {
                action.AddLogEntry( "Invalid valid Value for Entity attribute" );
                return false;
            }

            var followingService = new FollowingService( rockContext );
            var following = new Following();
            following.EntityTypeId = entityType.Id;
            following.EntityId = entityId;
            following.PersonAliasId = personAlias.Id;
            followingService.Add( following );
            rockContext.SaveChanges();

            return true;
        }
    }
}

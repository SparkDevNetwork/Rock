﻿// <copyright>
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
using Rock.Field.Types;
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

    #region Block Attributes

    [WorkflowAttribute(
        "Person",
        Description = "Workflow attribute that contains the person who is following the entity.",
        IsRequired = true,
        DefaultValue = "",
        Category = "",
        Order = 0,
        Key = AttributeKey.Person,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType" } )]

    [EntityTypeField(
        "Entity Type",
        Description = "Workflow attribute that contains the entity type to follow.",
        IsRequired = true,
        DefaultValue = "",
        Order = 1,
        Key = AttributeKey.EntityType )]

    [WorkflowTextOrAttribute( "Entity To Follow", "Attribute Value", "The Entity Id or Guid or an attribute that contains the entity to follow. <span class='tip tip-lava'></span>", true, "", "", 2, AttributeKey.Entity )]

    [TextField(
        "Purpose Key",
        Description = "The custom purpose to identify the type of Following.  Leave blank if you are unsure of the desired behavior since some components (such as SendFollowingEvents) expect the PurposeKey to be blank.",
        IsRequired = false,
        DefaultValue = "",
        Order = 2,
        Key = AttributeKey.PurposeKey )]

    #endregion
    public class PersonFollowAdd : ActionComponent
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string Person = "Person";
            public const string EntityType = "EntityType";
            public const string Entity = "Entity";
            public const string PurposeKey = "PurposeKey";
        }

        #endregion Attribute Keys
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

            // Get the Person entity from Attribute.
            PersonAlias personAlias = null;
            string personAttributeValue = GetAttributeValue( action, AttributeKey.Person );
            Guid? guidPersonAttribute = personAttributeValue.AsGuidOrNull();
            if ( guidPersonAttribute.HasValue )
            {
                var attributePerson = AttributeCache.Get( guidPersonAttribute.Value, rockContext );
                if ( attributePerson != null && attributePerson.FieldType.Class == "Rock.Field.Types.PersonFieldType" )
                {
                    Guid? attributePersonValue = action.GetWorkflowAttributeValue( guidPersonAttribute.Value ).AsGuidOrNull();
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

            // Get the entity type from Attribute.
            EntityTypeCache entityType = null;
            Guid? guidEntityType = GetAttributeValue( action, AttributeKey.EntityType ).AsGuidOrNull();
            if ( guidEntityType.HasValue )
            {
                entityType = EntityTypeCache.Get( guidEntityType.Value );
                if ( entityType == null )
                {
                    errorMessages.Add( string.Format( "Entity Type could not be found for selected value ('{0}')!", guidPersonAttribute.ToString() ) );
                    return false;
                }
            }

            string purposeKey = GetAttributeValue( action, AttributeKey.PurposeKey );

            var followingService = new FollowingService( rockContext );

            // Get the entity or entities that should be in Following.
            List<IEntity> entitiesToFollow = GetEntities( entityType, rockContext, action );
            if ( entitiesToFollow != null && entitiesToFollow.Any() )
            {
                foreach ( var entityToFollow in entitiesToFollow )
                {
                    // Query the FollowingService to find whether the requested Following already exists.
                    var following = followingService.Queryable()
                        .FirstOrDefault( f =>
                            f.EntityTypeId == entityType.Id &&
                            f.EntityId == entityToFollow.Id &&

                            // The result must have a null / empty PurposeKey in Following & null / empty PurposeKey text field
                            // or have a Following PurposeKey that matches the value in the PurposeKey text field.
                            ( ( string.IsNullOrEmpty( f.PurposeKey ) && string.IsNullOrEmpty( purposeKey ) ) || f.PurposeKey == purposeKey ) &&
                            f.PersonAlias.Person.Id == personAlias.PersonId );

                    if ( following == null )
                    {
                        following = new Following();
                        following.EntityTypeId = entityType.Id;
                        following.EntityId = entityToFollow.Id;
                        following.PersonAliasId = personAlias.Id;
                        /*
                            8/10/2021 - CWR
                            If the Workflow creator wants to include a custom purpose text value for a Following add, they can add text to this "Purpose Key" TextField.
                            Some components (such as SendFollowingEvents) expect the PurposeKey to be blank, so be aware.
                            
                            See here for the introduction of PurposeKey to Following:
                            https://github.com/SparkDevNetwork/Rock/commit/f92f4502772f6a19edc945b5c021115ad11d48a9
                        */

                        // If the PurposeKey is not null or empty, add a PurposeKey to the Following.
                        if ( !string.IsNullOrEmpty( purposeKey ) )
                        {
                            following.PurposeKey = purposeKey;
                        }

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

            string entityValue = GetAttributeValue( action, AttributeKey.Entity ).ResolveMergeFields( GetMergeFields( action ) );

            // If an ID was specified, just use that as the Entity ID to follow.
            int? intEntity = entityValue.AsIntegerOrNull();
            if ( intEntity.HasValue )
            {
                AddEntityById( entityTypeService, entityType.Id, intEntity.Value, entities );
            }

            Guid? guidEntity = entityValue.AsGuidOrNull();
            if ( guidEntity.HasValue )
            {
                // If the value is a Guid, it could either be a Guid of an Attribute or Entity.
                // Check if it is a Guid for an Attribute first.
                var attribute = AttributeCache.Get( guidEntity.Value, rockContext );
                if ( attribute != null )
                {
                    // It was for an Attribute, so get the value.
                    string attributeValue = action.GetWorkflowAttributeValue( guidEntity.Value );

                    // First check if that Attribute's field type is an IEntityFieldType (like Person or Group).
                    var entityFieldType = attribute.FieldType.Field as IEntityFieldType;
                    if ( entityFieldType != null )
                    {
                        if ( entityFieldType is Rock.Field.Types.PersonFieldType && EntityTypeCache.Get<Model.PersonAlias>().Id == entityType.Id )
                        {
                            var guidValue = attributeValue.AsGuidOrNull();
                            if ( guidValue.HasValue )
                            {
                                // It was not the Guid for an Attribute, so it must be the Guid for an Entity.
                                AddEntityByGuid( entityTypeService, entityType.Id, guidValue.Value, entities );
                            }
                        }
                        else
                        {
                            var entity = entityFieldType.GetEntity( attributeValue );
                            if ( entity != null )
                            {
                                entities.Add( entity );
                            }
                        }
                    }
                    else
                    {
                        // This Attribute is not a recognized Entity field.  Check to see if it is a list of Integers or Guids.
                        var values = attributeValue.SplitDelimitedValues();
                        foreach ( int intValue in values.AsIntegerList() )
                        {
                            AddEntityById( entityTypeService, entityType.Id, intValue, entities );
                        }

                        foreach ( Guid guidValue in values.AsGuidList() )
                        {
                            AddEntityByGuid( entityTypeService, entityType.Id, guidValue, entities );
                        }
                    }
                }
                else
                {
                    // It was not the Guid for an Attribute, so it must be the Guid for an Entity.
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

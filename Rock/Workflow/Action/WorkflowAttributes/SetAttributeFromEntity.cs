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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sets an attribute value to the entity that workflow is being acted on (Action's entity parameter).
    /// </summary>
    [ActionCategory( "Workflow Attributes" )]
    [Description( "Sets an attribute value to the entity that workflow is being acted on (Action's entity parameter)." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Attribute Set from Entity" )]

    [WorkflowAttribute( "Attribute", "The attribute to set the value of.", true, "", "", 1 )]
    [BooleanField( "Entity Is Required", "Should an error be returned if the entity is missing or not a valid entity type?", true, "", 2 )]
    [BooleanField( "Use Id instead of Guid", "Most entity attribute field types expect the Guid of the entity (which is used by default). Select this option if the entity's Id should be used instead (should be rare).", false, "", 3, "UseId" )]
    [CodeEditorField( "Lava Template", "By default this action will set the attribute value equal to the guid (or id) of the entity that was passed in for processing. If you include a lava template here, the action will instead set the attribute value to the output of this template. The mergefield to use for the entity is 'Entity.' For example, use {{ Entity.Name }} if the entity has a Name property. <span class='tip tip-lava'></span>", Web.UI.Controls.CodeEditorMode.Lava, Web.UI.Controls.CodeEditorTheme.Rock, 200, false, "", "", 4 )]
    public class SetAttributeFromEntity : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            if ( entity != null && entity is IEntity )
            {
                Guid guid = GetAttributeValue( action, "Attribute" ).AsGuid();
                if ( !guid.IsEmpty() )
                {
                    var attribute = AttributeCache.Get( guid, rockContext );
                    if ( attribute != null )
                    {
                        // If a lava template was specified, use that to set the attribute value
                        string lavaTemplate = GetAttributeValue( action, "LavaTemplate" );
                        if ( !string.IsNullOrWhiteSpace( lavaTemplate ) )
                        {
                            var mergeFields = GetMergeFields( action );
                            mergeFields.Add( "Entity", entity );
                            string parsedValue = lavaTemplate.ResolveMergeFields( mergeFields );
                            SetWorkflowAttributeValue( action, guid, parsedValue );
                        }
                        else
                        {
                            // Person + PersonFieldType is handled differently since it needs to be stored as PersonAlias.Guid
                            if ( entity is Person && attribute.FieldTypeId == FieldTypeCache.Get( SystemGuid.FieldType.PERSON.AsGuid(), rockContext ).Id )
                            {
                                var primaryAlias = GetPrimaryPersonAlias( ( Person ) entity, rockContext, errorMessages );
                                if ( primaryAlias != null )
                                {
                                    SetWorkflowAttributeValue( action, guid, primaryAlias.Guid.ToString() );
                                    return true;
                                }
                            }
                            // If the attribute is an Entity FieldType, store the value as EntityType.Guid|Entity.Id
                            else if ( attribute.FieldTypeId == FieldTypeCache.Get( SystemGuid.FieldType.ENTITY.AsGuid(), rockContext ).Id )
                            {
                                // Person + EntityFieldType is handled differently since it needs to be stored as PersonAlias's EntityType.Guid
                                EntityTypeCache entityType = entity is Person
                                    ? EntityTypeCache.Get( SystemGuid.EntityType.PERSON_ALIAS )
                                    : EntityTypeCache.Get( entity.GetType(), rockContext: rockContext );

                                if ( entityType == null )
                                {
                                    errorMessages.Add( "Unable to find the entity type. Type=" + entity.GetType().FullName );
                                }
                                else
                                {
                                    // Person + EntityFieldType is handled differently since it needs to be stored as PersonAlias.Id
                                    int? entityId = entity is Person
                                        ? GetPrimaryPersonAlias( ( Person ) entity, rockContext, errorMessages )?.Id
                                        : ( ( IEntity ) entity ).Id;

                                    if ( entityId.HasValue )
                                    {
                                        var value = $"{entityType.Guid}|{entityId.ToStringSafe()}";
                                        SetWorkflowAttributeValue( action, guid, value );
                                    }
                                }
                            }
                            else
                            {
                                if ( GetAttributeValue( action, "UseId" ).AsBoolean() )
                                {
                                    SetWorkflowAttributeValue( action, guid, ( (IEntity)entity ).Id.ToString() );
                                }
                                else
                                {
                                    SetWorkflowAttributeValue( action, guid, ( (IEntity)entity ).Guid.ToString() );
                                }
                            }
                        }

                        return true;
                    }
                    else
                    {
                        errorMessages.Add( "Unable to find the attribute from the attribute GUID=" + guid.ToStringSafe() );
                    }
                }
                else
                {
                    errorMessages.Add( "Unable to find the attribute GUID." );
                }
            }
            else
            {
                if ( !GetAttributeValue( action, "EntityIsRequired" ).AsBoolean( true ) )
                {
                    return true;
                }

                errorMessages.Add( "No entity was specified or the entity is not a Rock Entity." );
            }

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );
            return false;
        }

        /// <summary>
        /// Gets the primary person alias.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        private PersonAlias GetPrimaryPersonAlias( Person person, RockContext rockContext, List<string> errorMessages )
        {
            var personAlias = new PersonAliasService( rockContext )
                .Queryable()
                .AsNoTracking()
                .FirstOrDefault( a => a.AliasPersonId == person.Id );

            if ( personAlias == null )
            {
                errorMessages.Add( "Person Entity: Could not determine person's primary alias. PersonId=" + person.Id );
            }

            return personAlias;
        }
    }
}
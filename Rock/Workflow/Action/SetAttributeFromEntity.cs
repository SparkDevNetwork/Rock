﻿// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sets an attribute value to the entity that workflow is being acted on (Action's entity parameter).
    /// </summary>
    [Description( "Sets an attribute value to the entity that workflow is being acted on (Action's entity parameter)." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Set Attribute From Entity" )]

    [WorkflowAttribute( "Attribute", "The attribute to set the value of.")]
    [BooleanField("Use Id instead of Guid", "Most entity attribute field types expect the Guid of the entity (which is used by default). Select this option if the entity's Id should be used instead (should be rare).", false, "", 1, "UseId" )]
    public class SetAttributeToEntity : ActionComponent
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
                    var attribute = AttributeCache.Read( guid, rockContext );
                    if ( attribute != null )
                    {
                        // Person is handled special since it needs the person alias id
                        if ( entity is Person && attribute.FieldTypeId == FieldTypeCache.Read( SystemGuid.FieldType.PERSON.AsGuid(), rockContext ).Id )
                        {
                            var person = (Person)entity;

                            var primaryAlias = new PersonAliasService( rockContext ).Queryable().FirstOrDefault( a => a.AliasPersonId == person.Id );
                            if ( primaryAlias != null )
                            {
                                SetWorkflowAttributeValue( action, guid, primaryAlias.Guid.ToString() );
                                return true;
                            }
                            else
                            {
                                errorMessages.Add( "Could not determine person primary alias!" );
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
                            return true;
                        }
                    }
                    else
                    {
                        errorMessages.Add( "Invalid attribute!" );
                    }
                }
                else
                {
                    errorMessages.Add( "Invalid attribute!" );
                }
            }
            else
            {
                errorMessages.Add( "The entity is null or not a Rock IEntity." );
            }

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );
            return false;
        }

    }
}
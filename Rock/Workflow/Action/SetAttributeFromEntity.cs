// <copyright>
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

            Guid guid = GetAttributeValue( action, "Attribute" ).AsGuid();
            if ( !guid.IsEmpty() )
            {
                var attribute = AttributeCache.Read( guid, rockContext );
                if ( attribute != null )
                {
                    if ( entity != null )
                    {
                        if ( entity is Person && attribute.FieldTypeId == FieldTypeCache.Read( SystemGuid.FieldType.PERSON.AsGuid(), rockContext ).Id )
                        {
                            var person = entity as Person;

                            var primaryAlias = new PersonAliasService(rockContext).Queryable().FirstOrDefault( a => a.AliasPersonId == person.Id );
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
                        else if ( entity is Group && attribute.FieldTypeId == FieldTypeCache.Read( SystemGuid.FieldType.GROUP.AsGuid(), rockContext ).Id )
                        {
                            var group = entity as Group;
                            SetWorkflowAttributeValue( action, guid, group.Id.ToString() );
                            return true;
                        }
                        else
                        {
                            errorMessages.Add( "The attribute is not the correct type for the entity!" );
                        }
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

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );

            return false;
        }

    }
}
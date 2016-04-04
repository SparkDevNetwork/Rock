﻿// <copyright>
// Copyright by the Spark Development Network
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
using System.Web;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sets an attribute equal to the person who created workflow (if known).
    /// </summary>
    [ActionCategory( "Set Workflow Attribute" )]
    [Description( "Sets an attribute to the person who created the workflow (if known)." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Set Attribute to Initiator" )]

    [WorkflowAttribute( "Person Attribute", "The attribute to set to the initiator.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.PersonFieldType" } )]
    public class SetAttributeToInitiator : ActionComponent
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

            if ( action.Activity.Workflow.InitiatorPersonAliasId.HasValue )
            {
                var personAlias = new PersonAliasService( rockContext ).Get( action.Activity.Workflow.InitiatorPersonAliasId.Value );
                if ( personAlias != null )
                {
                    // Get the attribute to set
                    Guid guid = GetAttributeValue( action, "PersonAttribute" ).AsGuid();
                    if ( !guid.IsEmpty() )
                    {
                        var personAttribute = AttributeCache.Read( guid, rockContext );
                        if ( personAttribute != null )
                        {
                            // If this is a person type attribute
                            if ( personAttribute.FieldTypeId == FieldTypeCache.Read( SystemGuid.FieldType.PERSON.AsGuid(), rockContext ).Id )
                            {
                                SetWorkflowAttributeValue( action, guid, personAlias.Guid.ToString() );
                            }
                            else if ( personAttribute.FieldTypeId == FieldTypeCache.Read( SystemGuid.FieldType.TEXT.AsGuid(), rockContext ).Id )
                            {
                                SetWorkflowAttributeValue( action, guid, personAlias.Person.FullName );
                            }
                        }
                    }
                }
            }

            return true;
        }

    }
}
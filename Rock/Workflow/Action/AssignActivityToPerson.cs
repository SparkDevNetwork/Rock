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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Assigns the current activity to the selected person.
    /// </summary>
    [ActionCategory( "Workflow Control" )]
    [Description( "Assigns the current activity to the selected person." )]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Assign Activity to Person" )]

    [PersonField( "Person", "The person to assign this activity to.")]
    public class AssignActivityToPerson : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow action.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            Guid? personAliasGuid = GetAttributeValue( action, "Person" ).AsGuidOrNull();
            if ( personAliasGuid.HasValue )
            {
                var personAlias = new PersonAliasService( rockContext ).Queryable( "Person" )
                    .Where( a => a.Guid.Equals( personAliasGuid.Value ) )
                    .FirstOrDefault();

                if (personAlias != null)
                {
                    action.Activity.AssignedPersonAlias = personAlias;
                    action.Activity.AssignedPersonAliasId = personAlias.Id;
                    action.Activity.AssignedGroup = null;
                    action.Activity.AssignedGroupId = null;
                    action.AddLogEntry( string.Format( "Assigned activity to '{0}' ({1})",  personAlias.Person.FullName, personAlias.Person.Id ) );
                    return true;
                }

            }

            return false;
        }
    }
}
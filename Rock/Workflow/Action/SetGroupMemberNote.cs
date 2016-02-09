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
    /// Sets an group member's note.
    /// </summary>
    [ActionCategory( "Group Update" )]
    [Description( "Sets a person's group member note in a specified group." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Set Group Member Note" )]

    [WorkflowAttribute( "Person", "Workflow attribute that contains the person to update in the group.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.PersonFieldType" } )]

    [WorkflowAttribute( "Group", "Workflow Attribute that contains the group the person is in.", true, "", "", 1, null,
        new string[] { "Rock.Field.Types.GroupFieldType" } )]

    [WorkflowTextOrAttribute( "Note", "Attribute Value", "Text or workflow attribute that contains the text to set the group member note to.", true, "", "", 2, "Note",
        new string[] { "Rock.Field.Types.TextFieldType" } )]
    public class SetGroupMemberNote : ActionComponent
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

            Guid? groupGuid = null;
            Person person = null;
            string noteValue = string.Empty;

            // get the group attribute
            Guid groupAttributeGuid = GetAttributeValue( action, "Group" ).AsGuid();

            if ( !groupAttributeGuid.IsEmpty() )
            {
                groupGuid = action.GetWorklowAttributeValue( groupAttributeGuid ).AsGuidOrNull();

                if ( !groupGuid.HasValue )
                {
                    errorMessages.Add( "The group could not be found!" );
                }
            }

            // get person alias guid
            Guid personAliasGuid = Guid.Empty;
            string personAttribute = GetAttributeValue( action, "Person" );

            Guid guid = personAttribute.AsGuid();
            if ( !guid.IsEmpty() )
            {
                var attribute = AttributeCache.Read( guid, rockContext );
                if ( attribute != null )
                {
                    string value = action.GetWorklowAttributeValue( guid );
                    personAliasGuid = value.AsGuid();
                }

                if ( personAliasGuid != Guid.Empty )
                {
                    person = new PersonAliasService( rockContext ).Queryable()
                                    .Where( p => p.Guid.Equals( personAliasGuid ) )
                                    .Select( p => p.Person )
                                    .FirstOrDefault();
                }
                else
                {
                    errorMessages.Add( "The person could not be found!" );
                }
            }

            // get group member note
            noteValue = GetAttributeValue( action, "Note" );
            guid = noteValue.AsGuid();
            if ( guid.IsEmpty() )
            {
                noteValue = noteValue.ResolveMergeFields( GetMergeFields( action ) );
            }
            else
            {
                var workflowAttributeValue = action.GetWorklowAttributeValue( guid );

                if ( workflowAttributeValue != null )
                {
                    noteValue = workflowAttributeValue;
                }
            }

            // set note
            if ( groupGuid.HasValue && person != null )
            {
                var groupMember = new GroupMemberService( rockContext ).Queryable()
                                .Where( m => m.Group.Guid == groupGuid && m.PersonId == person.Id ).FirstOrDefault();

                groupMember.Note = noteValue;
                rockContext.SaveChanges();
            }

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );

            return true;
        }
    }
}
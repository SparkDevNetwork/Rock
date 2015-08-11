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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Communication;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Prompts user for attribute values
    /// </summary>
    [Description( "Prompts user for attribute values" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "User Entry Form" )]
    public class UserEntryForm : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            if ( !action.LastProcessedDateTime.HasValue &&
                action.ActionType != null &&
                action.ActionType.WorkflowForm != null &&
                action.ActionType.WorkflowForm.NotificationSystemEmailId.HasValue )
            {
                if ( action.Activity != null && ( action.Activity.AssignedPersonAliasId.HasValue || action.Activity.AssignedGroupId.HasValue ) )
                {
                    var recipients = new List<RecipientData>();
                    var workflowMergeFields = GetMergeFields( action );

                    if ( action.Activity.AssignedPersonAliasId.HasValue)
                    {
                        var person = new PersonAliasService( rockContext ).Queryable()
                            .Where( a => a.Id == action.Activity.AssignedPersonAliasId.Value )
                            .Select( a => a.Person )
                            .FirstOrDefault();

                        if ( person != null && !string.IsNullOrWhiteSpace( person.Email ) )
                        {
                            recipients.Add( new RecipientData( person.Email, CombinePersonMergeFields( person, workflowMergeFields ) ) );
                            action.AddLogEntry( string.Format( "Form notification sent to '{0}'", person.FullName ) );
                        }
                    }

                    if ( action.Activity.AssignedGroupId.HasValue )
                    {
                        var personList = new GroupMemberService(rockContext).GetByGroupId( action.Activity.AssignedGroupId.Value )
                            .Where( m =>
                                m.GroupMemberStatus == GroupMemberStatus.Active &&
                                m.Person.Email != "" )
                            .Select( m => m.Person )
                            .ToList();

                        foreach( var person in personList)
                        {
                            recipients.Add( new RecipientData( person.Email, CombinePersonMergeFields( person, workflowMergeFields ) ) );
                            action.AddLogEntry( string.Format( "Form notification sent to '{0}'", person.FullName ) );
                        }
                    }

                    if ( recipients.Count > 0 )
                    {
                        var systemEmail = new SystemEmailService( rockContext ).Get( action.ActionType.WorkflowForm.NotificationSystemEmailId.Value );
                        if ( systemEmail != null )
                        {
                            var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "InternalApplicationRoot" );
                            Email.Send( systemEmail.Guid, recipients, appRoot, string.Empty, false );
                        }
                        else
                        {
                            action.AddLogEntry( "Could not find the selected notification system email", true );
                        }
                    }
                    else
                    {
                        action.AddLogEntry( "Could not send form notification due to no assigned person or group member not having email address", true );
                    }
                }
                else
                {
                    action.AddLogEntry( "Could not send form notification due to no assigned person or group", true );
                }
            }

            return false;
        }

        private Dictionary<string, object> CombinePersonMergeFields( Person person, Dictionary<string,object> mergeFields)
        {
            var personFields = new Dictionary<string, object>();
            personFields.Add( "Person", person );

            foreach(var keyVal in mergeFields)
            {
                personFields.Add( keyVal.Key, keyVal.Value );
            }

            return personFields;
        }
    }
}
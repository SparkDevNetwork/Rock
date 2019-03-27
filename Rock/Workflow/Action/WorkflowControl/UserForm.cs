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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Prompts user for attribute values
    /// </summary>
    [ActionCategory( "Workflow Control" )]
    [Description( "Prompts user for attribute values" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Form" )]

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

            var actionType = action.ActionTypeCache;
            if ( !action.LastProcessedDateTime.HasValue &&
                actionType != null &&
                actionType.WorkflowForm != null &&
                actionType.WorkflowForm.NotificationSystemEmailId.HasValue )
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
                        // The email may need to reference activity Id, so we need to save here.
                        WorkflowService workflowService = new WorkflowService( rockContext );
                        workflowService.PersistImmediately( action );

                        var systemEmail = new SystemEmailService( rockContext ).Get( action.ActionTypeCache.WorkflowForm.NotificationSystemEmailId.Value );
                        if ( systemEmail != null )
                        {
                            var emailMessage = new RockEmailMessage( systemEmail );
                            emailMessage.SetRecipients( recipients );
                            emailMessage.CreateCommunicationRecord = false;
                            emailMessage.AppRoot = GlobalAttributesCache.Get().GetValue( "InternalApplicationRoot" ) ?? string.Empty;
                            emailMessage.Send();
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
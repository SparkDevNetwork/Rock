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
    /// </summary>
    [ActionCategory( "Workflow Control" )]
    [Description( "Prompts user for attribute values" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Form" )]

    [Rock.SystemGuid.EntityTypeGuid( "486DC4FA-FCBC-425F-90B0-E606DA8A9F68")]
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

            /*
             * 2020-01-30: DL
             * Workflow Form instances created prior to v1.10.2 may hold a reference to a SystemEmail (deprecated) rather than a SystemCommunication,
             * Processing has been added here to maintain backward-compatibility, with the SystemCommunication setting being preferred if it exists.
             */
#pragma warning disable CS0618 // Type or member is obsolete
            var sendNotification = !action.LastProcessedDateTime.HasValue &&
                actionType != null &&
                actionType.WorkflowForm != null &&
                ( actionType.WorkflowForm.NotificationSystemCommunicationId.HasValue );
#pragma warning restore CS0618 // Type or member is obsolete

            if ( sendNotification )
            {
                if ( action.Activity != null && ( action.Activity.AssignedPersonAliasId.HasValue || action.Activity.AssignedGroupId.HasValue ) )
                {
                    var recipients = new List<RockMessageRecipient>();
                    var workflowMergeFields = GetMergeFields( action );

                    if ( action.Activity.AssignedPersonAliasId.HasValue)
                    {
                        var person = new PersonAliasService( rockContext ).Queryable()
                            .Where( a => a.Id == action.Activity.AssignedPersonAliasId.Value )
                            .Select( a => a.Person )
                            .FirstOrDefault();

                        if ( person != null && !string.IsNullOrWhiteSpace( person.Email ) )
                        {
                            recipients.Add( new RockEmailMessageRecipient( person, CombinePersonMergeFields( person, workflowMergeFields ) ) );
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
                            recipients.Add( new RockEmailMessageRecipient( person, CombinePersonMergeFields( person, workflowMergeFields ) ) );
                            action.AddLogEntry( string.Format( "Form notification sent to '{0}'", person.FullName ) );
                        }
                    }

                    if ( recipients.Count > 0 )
                    {
                        // The email may need to reference activity Id, so we need to save here.
                        WorkflowService workflowService = new WorkflowService( rockContext );
                        workflowService.PersistImmediately( action );

                        // Create and send the notification email.
                        RockEmailMessage emailMessage = null;

                        if ( action.ActionTypeCache.WorkflowForm.NotificationSystemCommunicationId.HasValue )
                        {
                            var systemCommunication = new SystemCommunicationService( rockContext ).Get( action.ActionTypeCache.WorkflowForm.NotificationSystemCommunicationId.Value );

                            if ( systemCommunication != null )
                            {
                                emailMessage = new RockEmailMessage( systemCommunication );
                            }
                        }

                        if ( emailMessage != null )
                        {
                            emailMessage.SetRecipients( recipients );
                            emailMessage.CreateCommunicationRecord = false;

                            /*
                                [2024-06-20] - DJL

                                Changed the AppRoot from InternalApplicationRoot to PublicApplicationRoot.

                                Reason:
                                    To ensure that links embedded in the email are accessible to both
                                    internal and external recipients.
                             */
                            emailMessage.AppRoot = GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ) ?? string.Empty;

                            emailMessage.Send();
                        }
                        else
                        {
                            action.AddLogEntry( "Could not find the selected notification system communication.", true );
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

            // Always return false. Special logic for User Form will be handled in the WorkflowEntry block.
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
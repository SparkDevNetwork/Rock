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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tasks
{
    /// <summary>
    /// Launches a Group Member PlacedElsewhere workflow
    /// </summary>-
    public sealed class LaunchGroupMemberPlacedElsewhereWorkflow : BusStartedTask<LaunchGroupMemberPlacedElsewhereWorkflow.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            var rockContext = new RockContext();
            var workflowType = WorkflowTypeCache.Get( message.WorkflowTypeId );
            if ( workflowType != null && ( workflowType.IsActive ?? true ) )
            {
                var workflow = Rock.Model.Workflow.Activate( workflowType, message.GroupMemberWorkflowTriggerName );

                if ( workflow.AttributeValues != null )
                {
                    if ( workflow.AttributeValues.ContainsKey( "Group" ) )
                    {
                        var group = new GroupService( rockContext ).Get( message.GroupId.Value );
                        if ( group != null )
                        {
                            workflow.AttributeValues["Group"].Value = group.Guid.ToString();
                        }
                    }

                    if ( workflow.AttributeValues.ContainsKey( "Person" ) )
                    {
                        var person = new PersonService( rockContext ).Get( message.PersonId.Value );
                        if ( person != null && person.PrimaryAlias != null )
                        {
                            workflow.AttributeValues["Person"].Value = person.PrimaryAlias.Guid.ToString();
                        }
                    }

                    if ( workflow.AttributeValues.ContainsKey( "Note" ) )
                    {
                        workflow.AttributeValues["Note"].Value = message.Note;
                    }

                    // populate any other workflow attributes that match up with the group member's attributes
                    foreach ( var attributeKey in message.GroupMemberAttributeValues.Keys )
                    {
                        if ( workflow.AttributeValues.ContainsKey( attributeKey ) )
                        {
                            workflow.AttributeValues[attributeKey].Value = message.GroupMemberAttributeValues[attributeKey];
                        }
                    }
                }

                List<string> workflowErrors;
                new Rock.Model.WorkflowService( rockContext ).Process( workflow, out workflowErrors );
            }
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the workflow type identifier.
            /// </summary>
            /// <value>
            /// The workflow type identifier.
            /// </value>
            public int WorkflowTypeId { get; set; }

            /// <summary>
            /// Gets or sets the group member workflow trigger name.
            /// </summary>
            /// <value>
            /// The group member workflow trigger name.
            /// </value>
            public string GroupMemberWorkflowTriggerName { get; set; }

            /// <summary>
            /// Gets or sets the group identifier.
            /// </summary>
            /// <value>
            /// The group identifier.
            /// </value>
            public int? GroupId { get; set; }

            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            /// <value>
            /// The person identifier.
            /// </value>
            public int? PersonId { get; set; }

            /// <summary>
            /// Gets or sets the group member status name.
            /// </summary>
            /// <value>
            /// The group member status name.
            /// </value>
            public string GroupMemberStatusName { get; set; }

            /// <summary>
            /// Gets or sets the group member role name.
            /// </summary>
            /// <value>
            /// The group member role name.
            /// </value>
            public string GroupMemberRoleName { get; set; }

            /// <summary>
            /// Gets or sets the group member attribute values.
            /// </summary>
            /// <value>
            /// The group member attribute values.
            /// </value>
            public Dictionary<string, string> GroupMemberAttributeValues { get; set; }

            /// <summary>
            /// Gets or sets the note.
            /// </summary>
            /// <value>
            /// The note.
            /// </value>
            public string Note { get; set; }
        }
    }
}
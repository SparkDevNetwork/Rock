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
using System.Collections.Generic;
using System.Linq;
using Rock.Data;
using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Launches a Group Member PlacedElsewhere workflow
    /// </summary>
    public class GroupMemberPlacedElsewhereTransaction : ITransaction
    {
        private GroupMemberWorkflowTrigger Trigger { get; set; }

        private int? GroupId { get; set; }

        private int? PersonId { get; set; }

        private string GroupMemberStatusName { get; set; }

        private string GroupMemberRoleName { get; set; }

        private Dictionary<string, string> GroupMemberAttributeValues { get; set; }

        private string Note { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMemberPlacedElsewhereTransaction" /> class.
        /// </summary>
        /// <param name="groupMember">The group member of the current group they are in (before being deleted and processed) </param>
        /// <param name="note">The note.</param>
        /// <param name="trigger">The GroupMemberWorkflowTrigger.</param>
        public GroupMemberPlacedElsewhereTransaction( GroupMember groupMember, string note, GroupMemberWorkflowTrigger trigger )
        {
            this.Trigger = trigger;
            this.GroupId = groupMember.GroupId;
            this.PersonId = groupMember.PersonId;
            this.GroupMemberStatusName = groupMember.GroupMemberStatus.ConvertToString();
            this.GroupMemberRoleName = groupMember.GroupRole.ToString();
            groupMember.LoadAttributes();
            this.GroupMemberAttributeValues = groupMember.AttributeValues.ToDictionary( k => k.Key, v => v.Value.Value );
            this.Note = note;
        }

        /// <summary>
        /// Execute method to launch the workflow
        /// </summary>
        public void Execute()
        {
            var rockContext = new RockContext();
            LaunchWorkflow( rockContext, Trigger.WorkflowTypeId, Trigger.Name );
        }

        /// <summary>
        /// Launches the workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="workflowTypeId">The workflow type identifier.</param>
        /// <param name="name">The name.</param>
        private void LaunchWorkflow( RockContext rockContext, int workflowTypeId, string name )
        {
            var workflowTypeService = new WorkflowTypeService( rockContext );
            var workflowType = workflowTypeService.Get( workflowTypeId );
            if ( workflowType != null )
            {
                var workflow = Rock.Model.Workflow.Activate( workflowType, name );

                if ( workflow.AttributeValues != null )
                {
                    if ( workflow.AttributeValues.ContainsKey( "Group" ) )
                    {
                        var group = new GroupService( rockContext ).Get( GroupId.Value );
                        if ( group != null )
                        {
                            workflow.AttributeValues["Group"].Value = group.Guid.ToString();
                        }
                    }

                    if ( workflow.AttributeValues.ContainsKey( "Person" ) )
                    {
                        var person = new PersonService( rockContext ).Get( PersonId.Value );
                        if ( person != null && person.PrimaryAlias != null )
                        {
                            workflow.AttributeValues["Person"].Value = person.PrimaryAlias.Guid.ToString();
                        }
                    }

                    if ( workflow.AttributeValues.ContainsKey( "Note" ) )
                    {
                        workflow.AttributeValues["Note"].Value = Note;
                    }

                    // populate any other workflow attributes that match up with the group member's attributes
                    foreach ( var attributeKey in this.GroupMemberAttributeValues.Keys )
                    {
                        if ( workflow.AttributeValues.ContainsKey( attributeKey ) )
                        {
                            workflow.AttributeValues[attributeKey].Value = GroupMemberAttributeValues[attributeKey];
                        }
                    }
                }

                List<string> workflowErrors;
                new Rock.Model.WorkflowService( rockContext ).Process( workflow, out workflowErrors );
            }
        }
    }
}
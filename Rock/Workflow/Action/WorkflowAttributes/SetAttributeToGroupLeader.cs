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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;

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
    [ActionCategory( "Workflow Attributes" )]
    [Description( "Sets an attribute to the leader of the group provided. Returns the first person in a role marked 'Is Leader'." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Attribute Set to Group Leader" )]

    [WorkflowAttribute("Group", "The attribute containing the group to get the leader for.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.GroupFieldType", "Rock.Field.Types.GroupFieldType" })]

    [WorkflowAttribute( "Leader", "The attribute to set to the group loader.", true, "", "", 1, null,
        new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.PersonFieldType" } )]

    public class SetAttributeToGroupLeader : ActionComponent
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

            // get the group attribute
            Guid groupAttributeGuid = GetAttributeValue(action, "Group").AsGuid();

            if ( !groupAttributeGuid.IsEmpty() )
            {
                Guid? groupGuid = action.GetWorklowAttributeValue( groupAttributeGuid ).AsGuidOrNull();

                if ( groupGuid.HasValue )
                {
                    var groupLeader = new GroupMemberService(rockContext).Queryable().AsNoTracking()
                                       .Where(g => g.Group.Guid == groupGuid && g.GroupRole.IsLeader)
                                       .Select(g => g.Person).FirstOrDefault();

                    if ( groupLeader != null )
                    {
                        // Get the attribute to set
                        Guid leaderGuid = GetAttributeValue(action, "Leader").AsGuid();
                        if ( !leaderGuid.IsEmpty() )
                        {
                            var personAttribute = AttributeCache.Get(leaderGuid, rockContext);
                            if ( personAttribute != null )
                            {
                                // If this is a person type attribute
                                if ( personAttribute.FieldTypeId == FieldTypeCache.Get(SystemGuid.FieldType.PERSON.AsGuid(), rockContext).Id )
                                {
                                    SetWorkflowAttributeValue(action, leaderGuid, groupLeader.PrimaryAlias.Guid.ToString());
                                }
                                else if ( personAttribute.FieldTypeId == FieldTypeCache.Get(SystemGuid.FieldType.TEXT.AsGuid(), rockContext).Id )
                                {
                                    SetWorkflowAttributeValue(action, leaderGuid, groupLeader.FullName);
                                }
                            }
                        }
                    }
                }
                else
                {
                    errorMessages.Add("The group could not be found!");
                }                
            }

            errorMessages.ForEach(m => action.AddLogEntry(m, true));

            return true;
        }

    }
}
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
using System.Data.Entity;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Workflow;
using Rock.Workflow.Action.CheckIn;

namespace org.lakepointe.Checkin.Workflow.Action.Checkin
{
    /// <summary>
    /// Removes (or excludes) the groups for each selected family member that specify requirements they don't meet.
    /// </summary>
    [ActionCategory( "LPC Check-In" )]
    [Description( "Removes (or excludes) the groups for each selected family member that have group requirements they don't meet." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter Groups By Requirements" )]
    [BooleanField( "Remove", "Select 'Yes' if groups should be removed.  Select 'No' if they should just be marked as excluded.", true )]
    public class FilterGroupsByRequirements : CheckInActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState == null )
            {
                return false;
            }

            var family = checkInState.CheckIn.CurrentFamily;
            if ( family != null )
            {
                var remove = GetAttributeValue( action, "Remove" ).AsBoolean();

                foreach ( var person in family.People )
                {
                    foreach ( var groupType in person.GroupTypes.ToList() )
                    {
                        foreach ( var group in groupType.Groups.ToList() )
                        {
                            if ( !group.Group.SchedulingMustMeetRequirements )
                            {
                                continue;  // if group doesn't require group requirements, don't worry about it here
                            }

                            // Note to self. Check-in doesn't effectively differentiate roles within a group. If a person is in the group in multiple
                            // roles and those roles have different requirements, that person could conceivably meet the requirements for one role
                            // but not meet the requirements for another. We arbitrarily decided that if they meet the requirements for any role,
                            // they will be allowed to check in. Hopefully, the design of our volunteer teams will preclude this being an issue.
                            // Note that if it does turn out to be an issue, we can flip the logic to not let them check in if they fail to meet
                            // the requirements for any role. We didn't choose that path here because (a) we don't expect to run into that scenario
                            // and (b) it increases resistance for enabling people to serve.                SNS 20231115

                            // group.Group isn't fully fleshed out. In particular, group.Group.Members is empty.
                            var groupMembers = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                .Where( m =>
                                m.GroupId == group.Group.Id         // member of this group
                                && m.PersonId == person.Person.Id   // who are this person
                                && !m.IsArchived                    // who has not been archived
                                && m.GroupMemberStatus == GroupMemberStatus.Active );  // and whose status is Active (not Inactive or Pending)

                            // filter this group if there are no group members who don't have any unmet requirements
                            var filterThisGroup = !groupMembers.Where( m => !m.GroupMemberRequirements.Where( r => !r.RequirementMetDateTime.HasValue ).Any() ).Any();

                            if ( filterThisGroup )
                            {
                                if ( remove )
                                {
                                    groupType.Groups.Remove( group );
                                }
                                else
                                {
                                    group.ExcludedByFilter = true;
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}
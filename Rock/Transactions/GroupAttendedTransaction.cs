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
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Transactions
{
    /// <summary>
    /// Launches a group attendance workflow
    /// </summary>
    public class GroupAttendedTransaction : ITransaction
    {
        private int? GroupTypeId;
        private int? GroupId;
        private int? PersonAliasId;
        private DateTime? AttendanceDateTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMemberChangeTransaction"/> class.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public GroupAttendedTransaction( DbEntityEntry entry )
        {
            if ( entry.State != EntityState.Deleted )
            {
                // Get the attendance record
                var attendance = entry.Entity as Attendance;

                // If attendance record is valid and the DidAttend is selected
                if ( attendance != null &&  ( attendance.DidAttend ?? false ) )
                {
                    // Save for all adds
                    bool valid = entry.State == EntityState.Added;

                    // If not an add, check previous DidAttend value
                    if ( !valid )
                    {
                        var dbProperty = entry.Property( "DidAttend" );
                        if ( dbProperty != null )
                        {
                            // Only use changes where DidAttend was previously not true
                            valid = !(dbProperty.OriginalValue as bool? ?? false);
                        }
                    }

                    if ( valid )
                    {
                        var occ = attendance.Occurrence;
                        if (occ == null )
                        {
                            occ = new AttendanceOccurrenceService(new RockContext()).Get(attendance.OccurrenceId);
                        }

                        if (occ != null)
                        {
                            // Save the values
                            GroupId = occ.GroupId;
                            AttendanceDateTime = occ.OccurrenceDate;
                            PersonAliasId = attendance.PersonAliasId;

                            if (occ.Group != null)
                            {
                                GroupTypeId = occ.Group.GroupTypeId;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Execute method to check for any workflows to launch.
        /// </summary>
        public void Execute()
        {
            // Verify that valid ids were saved
            if ( GroupId.HasValue && PersonAliasId.HasValue )
            {
                // Get all the triggers from cache
                var cachedTriggers = GroupMemberWorkflowTriggerService.GetCachedTriggers();

                // If any triggers exist
                if ( cachedTriggers != null && cachedTriggers.Any() )
                {
                    // Get the triggers associated to the group that was checked into
                    var groupTriggers = cachedTriggers
                        .Where( w =>
                            w.TriggerType == GroupMemberWorkflowTriggerType.MemberAttendedGroup &&
                            w.GroupId.HasValue &&
                            w.GroupId.Value == GroupId.Value )
                        .OrderBy( w => w.Order )
                        .ToList();

                    // Get any triggers associated to a group type ( if any are found, will then filter by group type )
                    var groupTypeTriggers = cachedTriggers
                        .Where( w =>
                            w.TriggerType == GroupMemberWorkflowTriggerType.MemberAttendedGroup &&
                            w.GroupTypeId.HasValue )
                        .OrderBy( t => t.Order )
                        .ToList();

                    if ( groupTriggers.Any() || groupTypeTriggers.Any() )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            // If there were any group type triggers, will now need to read the group's group type id
                            // and then further filter these triggers by the current txn's group type
                            if ( groupTypeTriggers.Any() )
                            {
                                // Get the current txn's group type id
                                if ( !GroupTypeId.HasValue )
                                {
                                    GroupTypeId = new GroupService( rockContext )
                                        .Queryable().AsNoTracking()
                                        .Where( g => g.Id == GroupId.Value )
                                        .Select( g => g.GroupTypeId )
                                        .FirstOrDefault();
                                }

                                // Further filter the group type triggers by the group type id
                                groupTypeTriggers = groupTypeTriggers
                                    .Where( t =>
                                        t.GroupTypeId.HasValue &&
                                        t.GroupTypeId.Equals( GroupTypeId ) )
                                    .OrderBy( t => t.Order )
                                    .ToList();
                            }

                            // Combine group and grouptype triggers
                            var triggers = groupTriggers.Union( groupTypeTriggers ).ToList();

                            // If any triggers were found
                            if ( triggers.Any() )
                            {
                                // Loop through triggers and launch appropriate workflow
                                foreach ( var trigger in triggers )
                                {
                                    bool launchIt = true;

                                    var qualifierParts = ( trigger.TypeQualifier ?? "" ).Split( new char[] { '|' } );

                                    // Check to see if trigger is only specific to first time visitors
                                    if ( qualifierParts.Length > 4 && qualifierParts[4].AsBoolean() )
                                    {
                                        // Get the person from person alias
                                        int personId = new PersonAliasService( rockContext )
                                            .Queryable().AsNoTracking()
                                            .Where( a => a.Id == PersonAliasId.Value )
                                            .Select( a => a.PersonId )
                                            .FirstOrDefault();

                                        // Check if there are any other attendances for this group/person and if so, do not launch workflow
                                        if ( new AttendanceService( rockContext )
                                            .Queryable().AsNoTracking(  )
                                            .Count(a => a.Occurrence.GroupId.HasValue &&
                                                a.Occurrence.GroupId.Value == GroupId.Value &&
                                                a.PersonAlias != null &&
                                                a.PersonAlias.PersonId == personId &&
                                                a.DidAttend.HasValue &&
                                                a.DidAttend.Value) > 1 )
                                        {
                                            launchIt = false;
                                        }
                                    }

                                    // If first time flag was not specified, or this is a first time visit, launch the workflow
                                    if ( launchIt )
                                    {
                                        LaunchWorkflow( rockContext, trigger.WorkflowTypeId, trigger.Name );
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void LaunchWorkflow( RockContext rockContext, int workflowTypeId, string name )
        {
            var workflowType = WorkflowTypeCache.Get( workflowTypeId );
            if ( workflowType != null && ( workflowType.IsActive ?? true ) )
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
                        var personAlias = new PersonAliasService( rockContext ).Get( PersonAliasId.Value );
                        if ( personAlias != null )
                        {
                            workflow.AttributeValues["Person"].Value = personAlias.Guid.ToString();
                        }
                    }

                    if ( AttendanceDateTime.HasValue && workflow.AttributeValues.ContainsKey( "AttendanceDateTime" ) )
                    {
                        workflow.AttributeValues["AttendanceDateTime"].Value = AttendanceDateTime.Value.ToString( "o" );
                    }
                }

                List<string> workflowErrors;
                new Rock.Model.WorkflowService( rockContext ).Process( workflow, null, out workflowErrors );
            }
        }


    }
}
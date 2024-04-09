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
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tasks
{
    /// <summary>
    /// Launches a group attendance workflow
    /// </summary>
    public sealed class LaunchMemberAttendedGroupWorkflow : BusStartedTask<LaunchMemberAttendedGroupWorkflow.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            // Verify that valid ids were saved
            if ( message.GroupId.HasValue && message.PersonAliasId.HasValue )
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
                            w.GroupId.Value == message.GroupId.Value )
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
                                if ( !message.GroupTypeId.HasValue )
                                {
                                    message.GroupTypeId = new GroupService( rockContext )
                                        .Queryable()
                                        .AsNoTracking()
                                        .Where( g => g.Id == message.GroupId.Value )
                                        .Select( g => g.GroupTypeId )
                                        .FirstOrDefault();
                                }

                                // Further filter the group type triggers by the group type id
                                groupTypeTriggers = groupTypeTriggers
                                    .Where( t =>
                                        t.GroupTypeId.HasValue &&
                                        t.GroupTypeId.Equals( message.GroupTypeId ) )
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

                                    var qualifierParts = ( trigger.TypeQualifier ?? string.Empty ).Split( new char[] { '|' } );

                                    // Check to see if trigger is only specific to first time visitors
                                    var isLaunchOnce = qualifierParts.Length > 4 && qualifierParts[4].AsBoolean();
                                    if ( isLaunchOnce )
                                    {
                                        // Get the person from person alias, must match person because alias used in attendance record might be different
                                        int personId = new PersonAliasService( rockContext )
                                            .Queryable()
                                            .AsNoTracking()
                                            .Where( a => a.Id == message.PersonAliasId.Value )
                                            .Select( a => a.PersonId )
                                            .FirstOrDefault();

                                        // Get the attendance record, skip the trigger if one is not found (shouldn't happen)
                                        var attendanceService = new AttendanceService( rockContext );
                                        var attendance = attendanceService.Get( message.AttendanceId.Value );
                                        if ( attendance == null )
                                        {
                                            continue;
                                        }

                                        // Look for any prior attendances of this group by this person.
                                        var hasPriorGroupAttendance = attendanceService
                                            .Queryable()
                                            .AsNoTracking()
                                            .Any( a => a.Id < attendance.Id
                                                && a.Occurrence.GroupId.HasValue
                                                && a.Occurrence.GroupId.Value == message.GroupId.Value
                                                && a.PersonAlias != null
                                                && a.PersonAlias.PersonId == personId
                                                && a.DidAttend.HasValue
                                                && a.DidAttend.Value);

                                        // Because the launchOnce config is true
                                        // Launch the workflow only if this is the first attendance for the person & group.
                                        launchIt = !hasPriorGroupAttendance;
                                    }

                                    // If first time flag was not specified, or this is a first time visit, launch the workflow
                                    if ( launchIt )
                                    {
                                        LaunchWorkflow( rockContext, trigger.WorkflowTypeId, trigger.Name, message );
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void LaunchWorkflow( RockContext rockContext, int workflowTypeId, string name, Message message )
        {
            var workflowType = WorkflowTypeCache.Get( workflowTypeId );
            if ( workflowType != null && ( workflowType.IsActive ?? true ) )
            {
                var workflow = Rock.Model.Workflow.Activate( workflowType, name );

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
                        var personAlias = new PersonAliasService( rockContext ).Get( message.PersonAliasId.Value );
                        if ( personAlias != null )
                        {
                            workflow.AttributeValues["Person"].Value = personAlias.Guid.ToString();
                        }
                    }

                    if ( message.AttendanceDateTime.HasValue && workflow.AttributeValues.ContainsKey( "AttendanceDateTime" ) )
                    {
                        workflow.AttributeValues["AttendanceDateTime"].Value = message.AttendanceDateTime.Value.ToString( "o" );
                    }
                }

                List<string> workflowErrors;
                new Rock.Model.WorkflowService( rockContext ).Process( workflow, null, out workflowErrors );
            }
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the group type identifier.
            /// </summary>
            public int? GroupTypeId { get; set; }

            /// <summary>
            /// Gets or sets the group identifier.
            /// </summary>
            public int? GroupId { get; set; }

            /// <summary>
            /// Gets or sets the person alias identifier.
            /// </summary>
            public int? PersonAliasId { get; set; }

            /// <summary>
            /// Gets or sets the attendance date time.
            /// </summary>
            public DateTime? AttendanceDateTime { get; set; }

            /// <summary>
            /// Gets or sets the attendance identifier.
            /// </summary>
            /// <value>
            /// The attendance identifier.
            /// </value>
            public int? AttendanceId { get; set; }
        }
    }
}
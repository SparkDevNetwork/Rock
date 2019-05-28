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

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Transactions
{
    /// <summary>
    /// Launches a Workflow after a Scheduled Attendance record is Declined if the GroupType has a Cancellation Workflow configured
    /// </summary>
    public class GroupScheduleCancellationTransaction : ITransaction
    {
        /// <summary>
        /// Gets or sets the attendance identifier.
        /// </summary>
        /// <value>
        /// The attendance identifier.
        /// </value>
        private int AttendanceId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupScheduleCancellationTransaction"/> class.
        /// </summary>
        /// <param name="attendanceId">The attendance identifier.</param>
        public GroupScheduleCancellationTransaction( Attendance attendance )
        {
            this.AttendanceId = attendance.Id;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            LaunchWorkflow();
        }

        /// <summary>
        /// Launches the workflow.
        /// </summary>
        private void LaunchWorkflow()
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            var attendanceInfo = attendanceService.GetSelect(
                this.AttendanceId,
                s => new
                {
                    Attendance = s,
                    GroupGuid = s.Occurrence.Group.Guid,
                    s.PersonAlias.Person,
                    s.Occurrence.Group.GroupType.ScheduleCancellationWorkflowTypeId
                } );

            var attendance = attendanceInfo?.Attendance;
            var person = attendanceInfo?.Person;
            var groupGuid = attendanceInfo?.GroupGuid;

            if ( attendanceInfo?.ScheduleCancellationWorkflowTypeId == null )
            {
                // either attendance record doesn't exist, or it doesn't have a ScheduleCancellationWorkflowType defined, so nothing to do
                return;
            }

            if (!attendance.IsScheduledPersonDeclined())
            {
                // attendance record wasn't actually canceled, or isn't canceled anymore, so nothing to do
            }

            var workflowType = WorkflowTypeCache.Get( attendanceInfo.ScheduleCancellationWorkflowTypeId.Value );
            if ( workflowType != null && ( workflowType.IsActive ?? true ) )
            {
                var workflow = Rock.Model.Workflow.Activate( workflowType, attendanceInfo.Person?.FullName );

                if ( workflow.AttributeValues != null )
                {
                    if ( workflow.AttributeValues.ContainsKey( "Group" ) )
                    {
                        workflow.AttributeValues["Group"].Value = groupGuid.ToString();
                    }

                    if ( workflow.AttributeValues.ContainsKey( "Attendance" ) )
                    {
                        workflow.AttributeValues["Attendance"].Value = attendance.Guid.ToString();
                    }

                    if ( workflow.AttributeValues.ContainsKey( "Person" ) )
                    {
                        if ( person != null )
                        {
                            workflow.AttributeValues["Person"].Value = person.PrimaryAlias?.Guid.ToString();
                        }
                    }
                }

                List<string> workflowErrors;
                new Rock.Model.WorkflowService( rockContext ).Process( workflow, attendance, out workflowErrors );
            }
        }
    }
}
// <copyright>
// Copyright by Central Christian Church
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

using com.centralaz.RoomManagement.Attribute;
using com.centralaz.RoomManagement.Model;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace com.centralaz.RoomManagement.Jobs
{
    [SlidingDateRangeField( "Date Range", "The range of reservations to fire a workflow for.", required: true )]
    [BooleanField( "Include only reservations that start in date range", key: "StartsInDateRange" )]
    [WorkflowTypeField( "Workflow Type", "The workflow type to fire for eligible reservations", required: true )]
    [EnumsField( "Reservation Statuses", "The reservation statuses to filter by", typeof( ReservationApprovalState ), false, "", "" )]
    [DisallowConcurrentExecution]
    public class FireWorkflowFromReservationInDateRange : IJob
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="FireWorkflowFromReservationInDateRange"/> class.
        /// </summary>
        public FireWorkflowFromReservationInDateRange()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( dataMap.Get( "DateRange" ) != null ? dataMap.Get( "DateRange" ).ToString() : "-1||" );
            var startsInDateRange = dataMap.Get( "StartsInDateRange" ).ToString().AsBoolean();

            // Check for the configured states and limit query to those
            var states = new List<ReservationApprovalState>();

            foreach ( string stateVal in ( dataMap.Get( "Status" ).ToString() ?? "2" ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
            {
                var state = stateVal.ConvertToEnumOrNull<ReservationApprovalState>();
                if ( state != null )
                {
                    states.Add( state.Value );
                }
            }

            var rockContext = new RockContext();
            WorkflowType workflowType = null;
            Guid? workflowTypeGuid = dataMap.Get( "WorkflowType" ).ToString().AsGuidOrNull();
            if ( workflowTypeGuid.HasValue )
            {
                var workflowTypeService = new WorkflowTypeService( rockContext );
                workflowType = workflowTypeService.Get( workflowTypeGuid.Value );
            }

            if ( workflowType != null )
            {
                var reservationService = new ReservationService( rockContext );
                var reservationQuery = reservationService.Queryable();
                var reservationList = new List<Reservation>();

                if ( states.Any() )
                {
                    reservationQuery = reservationQuery.Where( r => states.Contains( r.ApprovalState ) );
                }

                var reservations = reservationQuery.ToList();
                if ( startsInDateRange )
                {
                    reservationList = reservations
                        .Select( r => new
                        {
                            Reservation = r,
                            ReservationDateTimes = r.GetReservationTimes( dateRange.Start ?? DateTime.MinValue, dateRange.End ?? DateTime.MaxValue )
                        } )
                        .Where( r => r.ReservationDateTimes.Any() )
                        .Select( r => r.Reservation )
                        .ToList();

                }
                else
                {
                    reservationList = reservations
                        .Select( r => new
                        {
                            Reservation = r,
                            ReservationDateTimes = r.GetReservationTimes( dateRange.Start.HasValue ? dateRange.Start.Value.AddMonths( -1 ) : DateTime.MinValue, dateRange.End.HasValue ? dateRange.End.Value.AddMonths( 1 ) : DateTime.MaxValue )
                        } )
                        .Where( r => r.ReservationDateTimes.Any( rdt =>
                            ( ( rdt.StartDateTime > ( dateRange.Start ?? DateTime.MinValue ) ) || ( rdt.EndDateTime > ( dateRange.Start ?? DateTime.MinValue ) ) ) &&
                            ( ( rdt.StartDateTime < ( dateRange.End ?? DateTime.MaxValue ) ) || ( rdt.EndDateTime < ( dateRange.End ?? DateTime.MaxValue ) ) ) )
                            )
                        .Select( r => r.Reservation )
                        .ToList();
                }

                foreach ( var reservation in reservationList )
                {
                    try
                    {
                        var workflowService = new WorkflowService( rockContext );
                        var workflow = Rock.Model.Workflow.Activate( workflowType, reservation.Name );
                        workflow.LoadAttributes();
                        workflow.SetAttributeValue( "ReservationId", reservation.Id.ToString() );
                        List<string> workflowErrors;
                        if ( workflowService.Process( workflow, out workflowErrors ) )
                        {
                            if ( workflow.IsPersisted || workflowType.IsPersisted )
                            {
                                if ( workflow.Id == 0 )
                                {
                                    workflowService.Add( workflow );
                                }

                                rockContext.WrapTransaction( () =>
                                {
                                    rockContext.SaveChanges();
                                    workflow.SaveAttributeValues( rockContext );
                                    foreach ( var activity in workflow.Activities )
                                    {
                                        activity.SaveAttributeValues( rockContext );
                                    }
                                } );
                            }
                        }

                    }
                    catch ( Exception ex )
                    {
                    }
                }
            }

            rockContext.SaveChanges();
        }
    }
}
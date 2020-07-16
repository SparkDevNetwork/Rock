// <copyright>
// Copyright by BEMA Information Technologies
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
using System.Linq;
using System.Reflection;
using com.bemaservices.HrManagement.Model;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;

namespace com.bemaservices.HrManagement.Workflow.Action
{
    /// <summary>
    /// Sets an entity property.
    /// </summary>
    [ActionCategory( "BEMA Services > HR Management" )]
    [Description( "Adds or Updates a PTO Request." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "PTO Request Update" )]

    // Existing PtoRequest
    [WorkflowAttribute( "Existing Pto Request", "The Pto Request to update.",
        false, "", "", 0, PTO_REQUEST_ATTRIBUTE_KEY, new string[] { "com.bemaservices.HrManagement.Field.Types.PtoRequestFieldType" } )]

    // Input Fields
    [WorkflowTextOrAttribute( "Allocation", "Attribute Value", "The allocation or an attribute that contains the allocation of the pto request. <span class='tip tip-lava'></span>",
        true, "", "", 1, ALLOCATION_KEY, new string[] { "com.bemaservices.HrManagement.Field.Types.PtoAllocationFieldType" } )]
    [WorkflowTextOrAttribute( "Start Date", "Attribute Value", "The start date or an attribute that contains the start date of the pto request. <span class='tip tip-lava'></span>",
        true, "", "", 2, STARTDATE_KEY, new string[] { "Rock.Field.Types.DateFieldType" } )]
    [WorkflowTextOrAttribute( "End Date", "Attribute Value", "The end date or an attribute that contains the end date of the pto request. <span class='tip tip-lava'></span>",
        false, "", "", 3, ENDDATE_KEY, new string[] { "Rock.Field.Types.DateFieldType" } )]
    [WorkflowTextOrAttribute( "Hours", "Attribute Value", "The hours per day or an attribute that contains the hours per day of the pto request. <span class='tip tip-lava'></span>",
        true, "", "", 4, HOURS_KEY, new string[] { "Rock.Field.Types.DecimalFieldType", "Rock.Field.Types.SelectSingleFieldType" } )]
    [WorkflowTextOrAttribute( "Reason", "Attribute Value", "The reason or an attribute that contains the reason of the pto request. <span class='tip tip-lava'></span>",
        false, "", "", 5, PTO_REASON_KEY, new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.MemoFieldType" } )]
    [WorkflowTextOrAttribute( "Approver", "Attribute Value", "The approver or an attribute that contains the approver of the pto request. <span class='tip tip-lava'></span>",
        false, "", "", 6, APPROVER_KEY, new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [WorkflowTextOrAttribute( "Approval State", "Attribute Value", "The Approval State or an attribute that contains the Approval State of the pto request. <span class='tip tip-lava'></span>",
        true, "", "", 7, APPROVAL_STATE_KEY, new string[] { "Rock.Field.Types.SelectSingleFieldType" } )]
    [WorkflowTextOrAttribute( "Exclude Weekends", "Attribute Value", "Whether to Include weekends, or an attribute that contains whether or not to incldue weekends. <span class='tip tip-lava'></span>",
        true, "False", "", 8, EXCLUDE_WEEKENDS_KEY, new string[] { "Rock.Field.Types.Boolean" } )]

    public class PtoRequestUpdate : ActionComponent
    {
        private const string PTO_REQUEST_ATTRIBUTE_KEY = "PTO_REQUEST_ATTRIBUTE_KEY";
        private const string STARTDATE_KEY = "STARTDATE_KEY";
        private const string ENDDATE_KEY = "ENDDATE_KEY";
        private const string HOURS_KEY = "HOURS_KEY";
        private const string PTO_REASON_KEY = "PTO_REASON_KEY";
        private const string ALLOCATION_KEY = "ALLOCATION_KEY";
        private const string APPROVER_KEY = "APPROVER_KEY";
        private const string APPROVAL_STATE_KEY = "APPROVAL_STATE_KEY";
        private const string EXCLUDE_WEEKENDS_KEY = "EXCLUDE_WEEKENDS_KEY";

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
            PtoAllocation ptoAllocation = null;
            Person approver = null;
            Guid? ptoRequestGuid = null;
            PtoRequest ptoRequest = null;
            var ptoRequestService = new PtoRequestService( rockContext );

            // get the existing pto request
            Guid ptoRequestAttributeGuid = GetAttributeValue( action, PTO_REQUEST_ATTRIBUTE_KEY ).AsGuid();

            if ( !ptoRequestAttributeGuid.IsEmpty() )
            {
                ptoRequestGuid = action.GetWorklowAttributeValue( ptoRequestAttributeGuid ).AsGuidOrNull();

                if ( ptoRequestGuid.HasValue )
                {
                    ptoRequest = ptoRequestService.Get( ptoRequestGuid.Value );
                }
            }

            Guid allocationGuid = Guid.Empty;
            string allocationAttribute = GetAttributeValue( action, ALLOCATION_KEY );

            Guid guid = allocationAttribute.AsGuid();
            if ( !guid.IsEmpty() )
            {
                var attribute = AttributeCache.Get( guid, rockContext );
                if ( attribute != null )
                {
                    string value = action.GetWorklowAttributeValue( guid );
                    allocationGuid = value.AsGuid();
                }

                if ( allocationGuid != Guid.Empty )
                {
                    ptoAllocation = new PtoAllocationService( rockContext ).Queryable()
                                    .Where( p => p.Guid.Equals( allocationGuid ) )
                                    .FirstOrDefault();
                }
                else
                {
                    errorMessages.Add( "The PTO Allocation could not be found!" );
                }
            }

            Guid approverAliasGuid = Guid.Empty;
            string approverAttributeString = GetAttributeValue( action, APPROVER_KEY );

            Guid approverGuid = approverAttributeString.AsGuid();
            if ( !approverGuid.IsEmpty() )
            {
                var approverAttribute = AttributeCache.Get( approverGuid, rockContext );
                if ( approverAttribute != null )
                {
                    string value = action.GetWorklowAttributeValue( approverGuid );
                    approverAliasGuid = value.AsGuid();
                }

                if ( approverAliasGuid != Guid.Empty )
                {
                    approver = new PersonAliasService( rockContext ).Queryable()
                                    .Where( p => p.Guid.Equals( approverAliasGuid ) )
                                    .Select( p => p.Person )
                                    .FirstOrDefault();
                }
            }

            var mergeFields = GetMergeFields( action );
            var startDate = GetAttributeValue( action, STARTDATE_KEY, true ).ResolveMergeFields( mergeFields ).AsDateTime();
            if ( !startDate.HasValue )
            {
                errorMessages.Add( "The start date could not be found!" );
            }

            var hours = GetAttributeValue( action, HOURS_KEY, true ).ResolveMergeFields( mergeFields ).AsDecimalOrNull();
            if ( !hours.HasValue )
            {
                errorMessages.Add( "The hours could not be found!" );
            }

            var endDate = GetAttributeValue( action, ENDDATE_KEY, true ).ResolveMergeFields( mergeFields ).AsDateTime();
            string reason = GetAttributeValue( action, PTO_REASON_KEY, true ).ResolveMergeFields( mergeFields );
            var approvalState = GetAttributeValue( action, APPROVAL_STATE_KEY, true ).ResolveMergeFields( mergeFields ).ConvertToEnum<PtoRequestApprovalState>( PtoRequestApprovalState.Pending );
            var excludeWeekends = GetAttributeValue( action, EXCLUDE_WEEKENDS_KEY, true ).ResolveMergeFields( mergeFields ).AsBoolean();

            if ( ptoAllocation != null && hours.HasValue && startDate.HasValue )
            {

                if ( ptoRequest == null )
                {
                    ptoRequest = new PtoRequest();
                    ptoRequestService.Add( ptoRequest );
                }

                var oldApprovalState = ptoRequest.PtoRequestApprovalState;

                if ( ptoRequest != null )
                {
                    ptoRequest.PtoAllocation = ptoAllocation;
                    ptoRequest.PtoAllocationId = ptoAllocation.Id;
                    ptoRequest.RequestDate = startDate.Value;
                    ptoRequest.Hours = hours.Value;
                    ptoRequest.Reason = reason;
                    ptoRequest.PtoRequestApprovalState = approvalState;

                    if ( approver != null && ptoRequest.PtoRequestApprovalState == PtoRequestApprovalState.Approved && oldApprovalState != PtoRequestApprovalState.Approved )
                    {
                        ptoRequest.ApproverPersonAlias = approver.PrimaryAlias;
                        ptoRequest.ApproverPersonAliasId = approver.PrimaryAliasId.Value;
                    }

                    if ( endDate.HasValue && endDate > startDate )
                    {

                        var requestDate = startDate.Value.AddDays( 1 );
                        while ( requestDate <= endDate.Value )
                        {
                            if( !excludeWeekends || ( requestDate.DayOfWeek != DayOfWeek.Saturday && requestDate.DayOfWeek != DayOfWeek.Sunday ) )
                            {

                                var additionalPtoRequest = new PtoRequest();
                                additionalPtoRequest.PtoAllocation = ptoAllocation;
                                additionalPtoRequest.PtoAllocationId = ptoAllocation.Id;
                                additionalPtoRequest.RequestDate = requestDate;
                                additionalPtoRequest.Hours = hours.Value;
                                additionalPtoRequest.Reason = reason;
                                additionalPtoRequest.PtoRequestApprovalState = approvalState;

                                if ( approver != null && additionalPtoRequest.PtoRequestApprovalState == PtoRequestApprovalState.Approved )
                                {
                                    ptoRequest.ApproverPersonAlias = approver.PrimaryAlias;
                                    ptoRequest.ApproverPersonAliasId = approver.PrimaryAliasId.Value;
                                }

                                ptoRequestService.Add( additionalPtoRequest );

                            }

                            requestDate = requestDate.AddDays( 1 );
                        }
                    }
                    rockContext.SaveChanges();
                }
                else
                {
                    errorMessages.Add( "The pto request could not be created!" );
                }
            }

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );

            return true;
        }

    }
}

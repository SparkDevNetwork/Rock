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
using Rock.Data;
using Rock.Logging;
using Rock.Model;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to update Group Members' Group Requirement statuses for requirements that are calculated from SQL or DataView
    /// </summary>
    [DisplayName( "Calculate Group Requirements" )]
    [Description( "Calculate Group Requirements for group members that are in groups that have group requirements." )]

    public class CalculateGroupRequirements : RockJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public CalculateGroupRequirements()
        {
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            // Lists for warnings of skipped groups, workflows, or people from the job.
            List<string> skippedGroupNames = new List<string>();
            List<string> skippedWorkflowNames = new List<string>();
            List<string> skippedPersonIds = new List<string>();
            List<int> groupRequirementsCalculatedPersonIds = new List<int>();

            // Get the list of group requirements that are based on a DataView or SQL.
            var rockContext = new RockContext();
            var groupRequirementService = new GroupRequirementService( rockContext );
            var groupRequirements = groupRequirementService.Queryable()
                .Where( a => a.GroupRequirementType.RequirementCheckType != RequirementCheckType.Manual )
                .AsNoTracking()
                .Include( i => i.GroupRequirementType )
                .Include( a => a.GroupRequirementType.DataView )
                .Include( a => a.GroupRequirementType.WarningDataView )
                .AsNoTracking()
                .ToList();

            foreach ( var groupRequirement in groupRequirements )
            {
                // Create a new data context for each requirement to ensure performance is scalable.
                rockContext = new RockContext();

                var groupMemberRequirementService = new GroupMemberRequirementService( rockContext );
                var groupMemberService = new GroupMemberService( rockContext );
                var groupService = new GroupService( rockContext );

                // Only calculate group requirements for Active groups (if an inactive group becomes active again, this job will take care of re-calculating the requirements again).
                var groupQuery = groupService.Queryable().Where( a => a.IsActive );
                if ( groupRequirement.GroupId.HasValue )
                {
                    groupQuery = groupQuery.Where( g => g.Id == groupRequirement.GroupId );
                }
                else if ( groupRequirement.GroupTypeId.HasValue )
                {
                    groupQuery = groupQuery.Where( g => g.GroupTypeId == groupRequirement.GroupTypeId );
                }
                else
                {
                    // Shouldn't happen, but Group Requirement doesn't have a groupId or a GroupTypeId.
                    break;
                }

                var groupIdNameList = groupQuery.Select( a => new { a.Id, a.Name } ).OrderBy( g => g.Name ).ToList();

                foreach ( var groupIdName in groupIdNameList )
                {
                    this.UpdateLastStatusMessage( $"Calculating group requirement '{groupRequirement.GroupRequirementType.Name}' for {groupIdName.Name} (Id:{groupIdName.Id})" );
                    try
                    {
                        var currentDateTime = RockDateTime.Now;
                        var qryGroupMemberRequirementsAlreadyOK = groupMemberRequirementService.Queryable().Where( a => a.GroupRequirementId == groupRequirement.Id && a.GroupMember.GroupId == groupIdName.Id );

                        if ( groupRequirement.GroupRequirementType.CanExpire && groupRequirement.GroupRequirementType.ExpireInDays.HasValue )
                        {
                            // Group requirement can expire: don't recalculate members that already met the requirement within the expire days (unless they are flagged with a warning)
                            var expireDaysCount = groupRequirement.GroupRequirementType.ExpireInDays.Value;
                            qryGroupMemberRequirementsAlreadyOK = qryGroupMemberRequirementsAlreadyOK.Where( a => !a.RequirementWarningDateTime.HasValue && a.RequirementMetDateTime.HasValue && SqlFunctions.DateDiff( "day", a.RequirementMetDateTime, currentDateTime ) < expireDaysCount );
                        }
                        else
                        {
                            // No Expiration: don't recalculate members that already met the requirement
                            qryGroupMemberRequirementsAlreadyOK = qryGroupMemberRequirementsAlreadyOK.Where( a => a.RequirementMetDateTime.HasValue );
                        }

                        // Only run the group requirements calculation on group members that are not inactive.
                        var groupMemberQry = groupMemberService.Queryable().Where( gm => gm.GroupMemberStatus != GroupMemberStatus.Inactive );

                        if ( groupRequirement.GroupId.HasValue )
                        {
                            groupMemberQry = groupMemberQry.Where( g => g.GroupId == groupRequirement.GroupId );
                        }
                        else if ( groupRequirement.GroupTypeId.HasValue )
                        {
                            groupMemberQry = groupMemberQry.Where( g => ( g.Group.GroupTypeId == groupRequirement.GroupTypeId ) && g.GroupId == groupIdName.Id );
                        }
                        else
                        {
                            // Should not happen, but break if Group Requirement doesn't have a GroupId or a GroupTypeId.
                            break;
                        }

                        var groupMembersThatDoNotMeetRequirementsPersonQry = groupMemberQry.Where( a => !qryGroupMemberRequirementsAlreadyOK.Any( r => r.GroupMemberId == a.Id ) ).Select( a => a.Person );

                        var personGroupRequirementStatuses = groupRequirement.PersonQueryableMeetsGroupRequirement( rockContext, groupMembersThatDoNotMeetRequirementsPersonQry, groupIdName.Id, groupRequirement.GroupRoleId ).ToList();

                        foreach ( var personGroupRequirementStatus in personGroupRequirementStatuses )
                        {
                            try
                            {
                                // Use a fresh rockContext per result so that ChangeTracker doesn't get bogged down.
                                using ( var rockContextUpdate = new RockContext() )
                                {
                                    groupRequirement.UpdateGroupMemberRequirementResult( rockContextUpdate, personGroupRequirementStatus.PersonId, groupIdName.Id, personGroupRequirementStatus.MeetsGroupRequirement );

                                    bool shouldRunNotMetWorkflow = personGroupRequirementStatus.MeetsGroupRequirement == MeetsGroupRequirement.NotMet &&
                                        groupRequirement.GroupRequirementType.ShouldAutoInitiateDoesNotMeetWorkflow &&
                                        groupRequirement.GroupRequirementType.DoesNotMeetWorkflowTypeId.HasValue;
                                    bool shouldRunWarningWorkflow = personGroupRequirementStatus.MeetsGroupRequirement == MeetsGroupRequirement.MeetsWithWarning &&
                                        groupRequirement.GroupRequirementType.ShouldAutoInitiateWarningWorkflow &&
                                        groupRequirement.GroupRequirementType.WarningWorkflowTypeId.HasValue;

                                    if ( shouldRunNotMetWorkflow || shouldRunWarningWorkflow )
                                    {
                                        // Get the full name of the group member person to add to the workflow name.
                                        var personForWorkflow = groupMembersThatDoNotMeetRequirementsPersonQry.FirstOrDefault( p => p.Id == personGroupRequirementStatus.PersonId )?.FullName;
                                        var workflowName = personForWorkflow + " (" + groupRequirement.GroupRequirementType.Name + ")";

                                        try
                                        {
                                            // Only one of these two should be possible by the logic of the Requirement Card.
                                            if ( shouldRunNotMetWorkflow )
                                            {
                                                var workflowTypeCache = WorkflowTypeCache.Get( groupRequirement.GroupRequirementType.DoesNotMeetWorkflowTypeId.Value );
                                                workflowName = $"({workflowTypeCache.Name}) {workflowName}";
                                                LaunchRequirementWorkflow( rockContextUpdate, workflowTypeCache, workflowName, personGroupRequirementStatus, groupIdName.Id, shouldRunNotMetWorkflow, false );
                                            }
                                            else if ( shouldRunWarningWorkflow )
                                            {
                                                var workflowTypeCache = WorkflowTypeCache.Get( groupRequirement.GroupRequirementType.WarningWorkflowTypeId.Value );
                                                workflowName = $"({workflowTypeCache.Name}) {workflowName}";
                                                LaunchRequirementWorkflow( rockContextUpdate, workflowTypeCache, workflowName, personGroupRequirementStatus, groupIdName.Id, false, shouldRunWarningWorkflow );
                                            }
                                        }
                                        catch ( Exception ex )
                                        {
                                            // Record workflow exception as warning or debug for RockLog instead of creating multiple exception logs and ending.
                                            Log( RockLogLevel.Warning, $"Could not launch workflow: '{workflowName}' with group requirement: '{groupRequirement}' for person.Id: {personGroupRequirementStatus.PersonId} so the workflow was skipped." );
                                            Log( RockLogLevel.Debug, ex, "Error when launching workflow for requirement." );

                                            skippedWorkflowNames.Add( workflowName, true );
                                        }
                                    }

                                    rockContextUpdate.SaveChanges();

                                    // Add the calculated person's ID to the list (if it is not already there) after it was successfully calculated.
                                    groupRequirementsCalculatedPersonIds.Add( personGroupRequirementStatus.PersonId, true );
                                }
                            }
                            catch ( Exception ex )
                            {
                                // Record group member 'Person' exception as warning or debug for RockLog and continue job instead of adding to exception logs and ending.
                                Log( RockLogLevel.Warning, $"Could not update group requirement result: '{groupRequirement}' for Person.Id: {personGroupRequirementStatus.PersonId} in Group: '{groupIdName.Name}' so the person was skipped." );
                                Log( RockLogLevel.Debug, ex, "Error when calculating person for group requirement." );

                                skippedPersonIds.Add( personGroupRequirementStatus.PersonId.ToString(), true );
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        // Record group exception as warning or debug for RockLog and continue job instead of adding to exception logs and ending.
                        Log( RockLogLevel.Warning, $"Could not update group when calculating group requirement: '{groupRequirement}' in Group '{groupIdName.Name}' (Group.Id: {groupIdName.Id}) so the group was skipped." );
                        Log( RockLogLevel.Debug, ex, "Error when calculating group for requirement." );

                        skippedGroupNames.Add( groupIdName.Name, true );
                    }
                }
            }

            JobSummary jobSummary = new JobSummary();
            jobSummary.Successes.Add( $"{groupRequirements.Count} group {"requirement".PluralizeIf( groupRequirements.Count != 1 )} " +
                $"re-calculated for {groupRequirementsCalculatedPersonIds.Distinct().Count()} " +
                $"{"person".PluralizeIf( groupRequirementsCalculatedPersonIds.Distinct().Count() != 1 )}." );

            bool jobHasWarnings = skippedGroupNames.Any() || skippedPersonIds.Any() || skippedWorkflowNames.Any();
            if ( jobHasWarnings )
            {
                if ( skippedGroupNames.Any() )
                {
                    jobSummary.Warnings.Add( "Skipped groups: " );
                    jobSummary.Warnings.AddRange( skippedGroupNames.Take( 10 ) );
                }

                if ( skippedPersonIds.Any() )
                {
                    jobSummary.Warnings.Add( "Skipped PersonIds: " );
                    jobSummary.Warnings.Add( skippedPersonIds.Take( 10 ).ToList().AsDelimited( ", " ) );
                }

                if ( skippedWorkflowNames.Any() )
                {
                    jobSummary.Warnings.Add( "Skipped workflows: " );
                    jobSummary.Warnings.AddRange( skippedWorkflowNames.Take( 10 ) );
                }

                jobSummary.Warnings.Add( "Enable 'Warning' or 'Debug' logging level for 'Jobs' domain in Rock Logs and re-run this job to get a full list of issues." );

                string errorMessage = "Calculate Group Requirements completed with warnings";

                this.Result = jobSummary.ToString();
                throw new RockJobWarningException( errorMessage, new Exception( jobSummary.ToString() ) );
            }
            else
            {
                this.UpdateLastStatusMessage( jobSummary.ToString() );
            }
        }

        private void LaunchRequirementWorkflow( RockContext rockContext, WorkflowTypeCache workflowTypeCache, string workflowName, PersonGroupRequirementStatus status, int groupId, bool shouldRunNotMetWorkflow, bool shouldRunWarningWorkflow )
        {
            if ( workflowTypeCache != null && ( workflowTypeCache.IsActive ?? false ) )
            {
                GroupMemberRequirementService groupMemberRequirementService = new GroupMemberRequirementService( rockContext );
                var groupMemberRequirement = groupMemberRequirementService
                    .GetByPersonIdRequirementIdGroupIdGroupRoleId( status.PersonId, status.GroupRequirement.Id, groupId, status.GroupRequirement.GroupRoleId );
                if ( groupMemberRequirement == null )
                {
                    var groupMemberIds = new GroupMemberService( rockContext ).GetByGroupIdAndPersonId( groupId, status.PersonId );
                    var groupMember = groupMemberIds.OrderBy( a => a.GroupRole.IsLeader ).FirstOrDefault();
                    groupMemberRequirement = new GroupMemberRequirement
                    {
                        GroupRequirementId = status.GroupRequirement.Id,
                        GroupMemberId = groupMember.Id
                    };
                    rockContext.SaveChanges();

                    // Get the just-added Group Member Requirement in case we need to update it with a workflow ID.
                    groupMemberRequirement = groupMemberRequirementService
                            .GetByPersonIdRequirementIdGroupIdGroupRoleId( status.PersonId, status.GroupRequirement.Id, groupId, status.GroupRequirement.GroupRoleId );
                }

                if ( ( shouldRunNotMetWorkflow && groupMemberRequirement.DoesNotMeetWorkflowId == null ) ||
                    ( shouldRunWarningWorkflow && groupMemberRequirement.WarningWorkflowId == null ) )
                {
                    Rock.Model.Workflow workflow;

                    var workflowService = new WorkflowService( rockContext );
                    workflow = Rock.Model.Workflow.Activate( workflowTypeCache, workflowName, rockContext );
                    workflow.SetAttributeValue( "Person", groupMemberRequirement?.GroupMember.Person.PrimaryAlias.Guid );
                    new WorkflowService( rockContext ).Process( workflow, groupMemberRequirement, out var workflowErrors );

                    if ( shouldRunNotMetWorkflow )
                    {
                        groupMemberRequirement.DoesNotMeetWorkflowId = workflow.Id;
                    }
                    else if ( shouldRunWarningWorkflow )
                    {
                        groupMemberRequirement.WarningWorkflowId = workflow.Id;
                    }

                    rockContext.SaveChanges();
                }
            }
        }

        private class JobSummary
        {
            public const string SUCCESS_ICON = "<i class='fa fa-circle text-success'></i> ";
            public const string WARNING_ICON = "<i class='fa fa-circle text-warning'></i> ";
            public const string ERROR_ICON = "<i class='fa fa-circle text-error'></i> ";

            public JobSummary()
            {
                Successes = new List<string>();
                Warnings = new List<string>();
                Errors = new List<string>();
            }

            public List<string> Successes { get; set; }

            public List<string> Warnings { get; set; }

            public List<string> Errors { get; set; }

            /// <summary>
            /// Aggregates successes, warnings, and errors with icon prefixes into an HTML string.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                if ( Successes.Any() )
                {
                    sb.Append( SUCCESS_ICON );
                    foreach ( var success in Successes )
                    {
                        sb.AppendLine( success );
                    }
                }

                if ( Warnings.Any() )
                {
                    sb.Append( WARNING_ICON );
                    foreach ( var warning in Warnings )
                    {
                        sb.AppendLine( warning );
                    }
                }

                if ( Errors.Any() )
                {
                    sb.Append( ERROR_ICON );
                    foreach ( var error in Errors )
                    {
                        sb.AppendLine( error );
                    }
                }

                return sb.ToString().ConvertCrLfToHtmlBr();
            }
        }
    }
}

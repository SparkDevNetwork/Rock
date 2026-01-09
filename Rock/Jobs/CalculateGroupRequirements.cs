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
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;

using Microsoft.Extensions.Logging;

using Rock.Attribute;
using Rock.Data;
using Rock.Logging;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to update Group Members' Group Requirement statuses for requirements that are calculated from SQL or DataView
    /// </summary>
    [DisplayName( "Calculate Group Requirements" )]
    [Description( "Calculate Group Requirements for group members that are in groups that have group requirements." )]

    [BooleanField( "Bypass Data View Cache",
        Key = AttributeKey.BypassDataViewCache,
        Description = "This is an experimental setting that will be removed in a future version of Rock. Whether to bypass the Data View cache when calculating group requirements.",
        IsRequired = false,
        DefaultBooleanValue = false,
        Order = 0 )]

    [RockLoggingCategory]
    public class CalculateGroupRequirements : RockJob
    {
        /// <summary>
        /// Attribute Keys for the <see cref="CalculateGroupRequirements"/> job.
        /// </summary>
        private static class AttributeKey
        {
            public const string BypassDataViewCache = "BypassDataViewCache";
        }

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
            var bypassDataViewCache = GetAttributeValue( AttributeKey.BypassDataViewCache ).AsBoolean();

            // Lists for warnings of skipped groups, workflows, or people from the job.
            List<string> skippedGroupNames = new List<string>();
            List<string> skippedWorkflowNames = new List<string>();
            List<string> unsuccessfulWorkflowNames = new List<string>();
            List<string> skippedPersonIds = new List<string>();
            List<int> groupRequirementsCalculatedPersonIds = new List<int>();

            // Get the list of group requirements that are based on a DataView or SQL.
            var rockContext = new RockContext();
            var groupRequirementService = new GroupRequirementService( rockContext );
            var groupMemberRequirementService = new GroupMemberRequirementService( rockContext );


            int insertedCount = 0;
            int updatedCount = 0;
            int deletedCount = 0;
            int successfulNotMetCount = 0;
            int successfulWarningCount = 0;

            var groupRequirements = groupRequirementService.Queryable()
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
                        List<int> applicablePersonIds = new List<int>();

                        // If this group requirement has an "AppliesToDataView" than we'll need to populate applicablePersonIds with a list of person Ids returned from the DataView.
                        if ( groupRequirement.AppliesToDataViewId.HasValue )
                        {
                            /*
                                4/17/2024 - JPH

                                The "Bypass Data View Cache" attribute is a temporary setting being introduced to properly test
                                a new approach of calculating group requirements in bulk, using the newly-introduced data view cache.
                                Using this new cache will be the default behavior of the job, whereas this setting will allow a given
                                partner to easily fall back to the old behavior if the cached approach proves to be problematic in any way.

                                This attribute will be removed in a future version of Rock (and the following if/else block will go away)
                                once we're sure of which path to take in the long run.

                                Reason: Option to fall back to old behavior
                            */
                            if ( bypassDataViewCache )
                            {
                                var appliesToDataViewPersonService = new PersonService( rockContext );
                                var appliesToDataViewParamExpression = appliesToDataViewPersonService.ParameterExpression;
                                var appliesToDataViewWhereExpression = groupRequirement.AppliesToDataView.GetExpression( appliesToDataViewPersonService, appliesToDataViewParamExpression );
                                applicablePersonIds = appliesToDataViewPersonService.Get( appliesToDataViewParamExpression, appliesToDataViewWhereExpression ).Select( p => p.Id ).ToList();
                            }
                            else
                            {
                                var appliesToDataViewCache = DataViewCache.Get( groupRequirement.AppliesToDataViewId.Value );
                                applicablePersonIds = appliesToDataViewCache.GetVolatileEntityIds().ToList();
                            }
                        }

                        List<int> meetsPersonIdList = new List<int>();
                        List<int> warningPersonIdList = new List<int>();

                        if ( groupRequirement.GroupRequirementType.RequirementCheckType == RequirementCheckType.Dataview )
                        {
                            var personService = new PersonService( rockContext );
                            var paramExpression = personService.ParameterExpression;

                            // If a warning data view exists, we'll populate warningPersonIdList with the results.
                            if ( groupRequirement.GroupRequirementType.WarningDataViewId.HasValue )
                            {
                                if ( bypassDataViewCache )
                                {
                                    var warningDataViewWhereExpression = groupRequirement.GroupRequirementType.WarningDataView.GetExpression( personService, paramExpression );
                                    warningPersonIdList = personService.Get( paramExpression, warningDataViewWhereExpression ).Where( p => applicablePersonIds.Contains( p.Id ) ).Select( p => p.Id ).ToList();
                                }
                                else
                                {
                                    var warningDataViewCache = DataViewCache.Get( groupRequirement.GroupRequirementType.WarningDataViewId.Value );
                                    warningPersonIdList = warningDataViewCache.GetVolatileEntityIds().ToList();
                                }
                            }

                            // If a warning data view exists, we'll populate meetsPersonIdList with the results.
                            if ( groupRequirement.GroupRequirementType.DataViewId.HasValue )
                            {
                                if ( bypassDataViewCache )
                                {
                                    var meetsDataViewWhereExpression = groupRequirement.GroupRequirementType.DataView.GetExpression( personService, paramExpression );
                                    meetsPersonIdList = personService.Get( paramExpression, meetsDataViewWhereExpression ).Where( p => applicablePersonIds.Contains( p.Id ) ).Select( p => p.Id ).ToList();
                                }
                                else
                                {
                                    var meetsDataViewWhereExpression = DataViewCache.Get( groupRequirement.GroupRequirementType.DataViewId.Value );
                                    meetsPersonIdList = meetsDataViewWhereExpression.GetVolatileEntityIds().ToList();
                                }
                            }
                        }
                        else if ( groupRequirement.GroupRequirementType.RequirementCheckType == RequirementCheckType.Sql )
                        {
                            var targetGroup = new GroupService( rockContext ).Get( groupIdName.Id );
                            Rock.Model.Person personMergeField = null;

                            // If there is only one applicable person than we can resolve the person merge field
                            if ( applicablePersonIds.Count == 1 )
                            {
                                var personId = applicablePersonIds[0];
                                personMergeField = new PersonService( rockContext ).GetNoTracking( personId );
                            }

                            string metFormattedSql = groupRequirement.GroupRequirementType.SqlExpression.ResolveMergeFields( groupRequirement.GroupRequirementType.GetMergeObjects( targetGroup, personMergeField ) );
                            string warningFormattedSql = groupRequirement.GroupRequirementType.WarningSqlExpression.ResolveMergeFields( groupRequirement.GroupRequirementType.GetMergeObjects( targetGroup, personMergeField ) );
                            try
                            {
                                var tableResult = DbService.GetDataTable( metFormattedSql, System.Data.CommandType.Text, null );
                                if ( tableResult.Columns.Count > 0 )
                                {
                                    // Populate the meetsPersonIdList with the results from the Query.
                                    meetsPersonIdList = tableResult.Rows.OfType<System.Data.DataRow>().Select( r => Convert.ToInt32( r[0] ) ).ToList();

                                    // if a Warning SQL was specified, get a list of PersonIds that should have a warning with their status
                                    if ( !string.IsNullOrWhiteSpace( warningFormattedSql ) )
                                    {
                                        var warningTableResult = DbService.GetDataTable( warningFormattedSql, System.Data.CommandType.Text, null );
                                        if ( warningTableResult.Columns.Count > 0 )
                                        {
                                            warningPersonIdList = warningTableResult.Rows.OfType<System.Data.DataRow>().Select( r => Convert.ToInt32( r[0] ) ).ToList();
                                        }
                                    }
                                }
                            }
                            catch ( Exception ex )
                            {
                                // Exception occurred (probably due to bad SQL)
                                ExceptionLogService.LogException( ex, System.Web.HttpContext.Current );
                            }
                        }

                        // Prepare the parameters needed to execute the spUpdateGroupMemberRequirements Stored Procedure.
                        var parameters = new Dictionary<string, object>
                        {
                            { "GroupRequirementId", groupRequirement.Id },
                            { "GroupId", groupIdName.Id },
                            { "AppliesToPersonIds", applicablePersonIds.ConvertToIdListParameter( "AppliesToPersonIds" ) },
                            { "WarningPersonIds", warningPersonIdList.ConvertToIdListParameter( "WarningPersonIds" ) },
                            { "MeetsPersonIds", meetsPersonIdList.ConvertToIdListParameter( "MeetsPersonIds" ) }
                        };

                        var mergeResult = new DbService( new RockContext() ).GetDataTableFromSqlCommand( "spUpdateGroupMemberRequirements", System.Data.CommandType.StoredProcedure, parameters );

                        // Update counts based on the results from the Stored Procedure execution.
                        insertedCount += mergeResult.Rows.Count > 0
                            ? mergeResult.Rows[0]["InsertedCount"].ToIntSafe()
                            : 0;
                        updatedCount += mergeResult.Rows.Count > 0
                            ? mergeResult.Rows[0]["UpdatedCount"].ToIntSafe()
                            : 0;
                        deletedCount += mergeResult.Rows.Count > 0
                            ? mergeResult.Rows[0]["DeletedCount"].ToIntSafe()
                            : 0;

                        WorkflowTypeCache doesNotMeetWorkflowType = null;
                        WorkflowTypeCache warningWorkflowType = null;

                        if ( groupRequirement.GroupRequirementType.DoesNotMeetWorkflowTypeId.HasValue )
                        {
                            doesNotMeetWorkflowType = WorkflowTypeCache.Get( groupRequirement.GroupRequirementType.DoesNotMeetWorkflowTypeId.Value );

                        }
                        if ( groupRequirement.GroupRequirementType.WarningWorkflowTypeId.HasValue )
                        {
                            warningWorkflowType = WorkflowTypeCache.Get( groupRequirement.GroupRequirementType.WarningWorkflowTypeId.Value );
                        }

                        // Loop over the results from the Stored Procedure and launch workflows if specified.
                        foreach ( DataRow result in mergeResult.Rows )
                        {
                            int groupMemberRequirementId = result["GroupMemberRequirementId"].ToIntSafe();
                            int groupMemberId = result["GroupMemberId"].ToIntSafe();
                            int personId = result["PersonId"].ToIntSafe();
                            string workflowTypeQualifier = result["WorkflowType"].ToStringSafe();
                            string nickName = result["NickName"].ToStringSafe();
                            string lastName = result["LastName"].ToStringSafe();
                            int? suffixValueId = result["SuffixValueId"] != null
                                ? result["SuffixValueId"].ToIntSafe()
                                : ( int? ) null;
                            int? recordTypeValueId = result["RecordTypeValueId"] != null
                                ? result["RecordTypeValueId"].ToIntSafe()
                                : ( int? ) null;

                            GroupMemberRequirement groupMemberRequirement = groupMemberRequirementService.Get( groupMemberRequirementId );
                            WorkflowTypeCache workflowType;

                            bool wasWorkflowSuccessful = true;
                            string personFullName = Rock.Model.Person.FormatFullName( nickName, lastName, suffixValueId, recordTypeValueId );

                            if ( workflowTypeQualifier == "NotMet" && doesNotMeetWorkflowType != null )
                            {
                                workflowType = doesNotMeetWorkflowType;
                            }
                            else if ( workflowTypeQualifier == "Warning" && warningWorkflowType != null )
                            {
                                workflowType = warningWorkflowType;
                            }
                            else
                            {
                                continue;
                            }

                            var workflowName = $"({workflowType.Name}) {personFullName} ({groupRequirement.GroupRequirementType.Name})";

                            try
                            {
                                LaunchRequirementWorkflow( rockContext, workflowType, workflowName, groupIdName.Id, personId, groupRequirement, groupMemberRequirement, workflowTypeQualifier, out wasWorkflowSuccessful );

                                if ( wasWorkflowSuccessful )
                                {
                                    if ( workflowTypeQualifier == "NotMet" )
                                    {
                                        successfulNotMetCount++;
                                    }
                                    else if ( workflowTypeQualifier == "Warning" )
                                    {
                                        successfulWarningCount++;
                                    }
                                }
                                else
                                {
                                    unsuccessfulWorkflowNames.Add( workflowName, true );
                                }
                            }
                            catch ( Exception ex )
                            {
                                // Record workflow exception as warning or debug for RockLog instead of creating multiple exception logs and ending.
                                Logger.LogWarning( $"Could not launch workflow: '{workflowName}' with group requirement: '{groupRequirement}' for person.Id: {personId} so the workflow was skipped." );
                                Logger.LogDebug( ex, "Error when launching workflow for requirement." );

                                skippedWorkflowNames.Add( workflowName, true );
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        // Record group exception as warning or debug for RockLog and continue job instead of adding to exception logs and ending.
                        Logger.LogWarning( $"Could not update group when calculating group requirement: '{groupRequirement}' in Group '{groupIdName.Name}' (Group.Id: {groupIdName.Id}) so the group was skipped." );
                        Logger.LogDebug( ex, "Error when calculating group for requirement." );

                        skippedGroupNames.Add( groupIdName.Name, true );
                    }
                }
            }

            JobSummary jobSummary = new JobSummary();
            jobSummary.Successes.Add( $"Checked {groupRequirements.Count} Group {"Requirement".PluralizeIf( groupRequirements.Count != 1 )}." );
            jobSummary.Successes.Add( $"Inserted {insertedCount} Group Member {"Requirement".PluralizeIf( insertedCount != 1 )}." );
            jobSummary.Successes.Add( $"Updated {updatedCount} Group Member {"Requirement".PluralizeIf( updatedCount != 1 )}." );
            jobSummary.Successes.Add( $"Deleted {deletedCount} Group Member {"Requirement".PluralizeIf( deletedCount != 1 )}." );
            jobSummary.Successes.Add( $"Launched {successfulNotMetCount} \"Does Not Meet Requirement\" {"Workflow".PluralizeIf( successfulNotMetCount != 1 )}." );
            jobSummary.Successes.Add( $"Launched {successfulWarningCount} \"Warning Requirement\" {"Workflow".PluralizeIf( successfulWarningCount != 1 )}." );

            bool jobHasWarnings = skippedGroupNames.Any() || skippedPersonIds.Any() || skippedWorkflowNames.Any() || unsuccessfulWorkflowNames.Any();
            if ( jobHasWarnings )
            {
                if ( skippedGroupNames.Any() )
                {
                    jobSummary.Warnings.Add( "Skipped groups: " );
                    jobSummary.Warnings.AddRange( skippedGroupNames.Take( 10 ) );
                }

                if ( skippedWorkflowNames.Any() )
                {
                    jobSummary.Warnings.Add( "Skipped workflows: " );
                    jobSummary.Warnings.AddRange( skippedWorkflowNames.Take( 10 ) );
                }

                if ( unsuccessfulWorkflowNames.Any() )
                {
                    jobSummary.Warnings.Add( "Unsuccessful workflows: " );
                    jobSummary.Warnings.AddRange( unsuccessfulWorkflowNames.Take( 10 ) );
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

        private void LaunchRequirementWorkflow( RockContext rockContext, WorkflowTypeCache workflowTypeCache, string workflowName, int groupId, int personId, GroupRequirement groupRequirement, GroupMemberRequirement groupMemberRequirement, string workflowType, out bool wasWorkflowSuccessful )
        {
            wasWorkflowSuccessful = true;

            var workflow = Rock.Model.Workflow.Activate( workflowTypeCache, workflowName, rockContext );
            var personAliasGuid = new PersonAliasService( rockContext ).GetPrimaryAliasGuid( personId );

            workflow.SetAttributeValue( "Person", personAliasGuid );

            new WorkflowService( rockContext ).Process( workflow, out var workflowErrors );

            if ( workflowErrors?.Any() == true )
            {
                Logger.LogWarning( $"Encountered workflow errors when calculating group requirement: '{groupRequirement}' for workflow type: '{workflowTypeCache.Name}' for Person.Id: {personId} in Group.Id: {groupId}: {string.Join( "; ", workflowErrors )}" );

                wasWorkflowSuccessful = false;
            }

            if ( workflowType == "NotMet" )
            {
                groupMemberRequirement.DoesNotMeetWorkflowId = workflow.Id;
            }
            else if ( workflowType == "Warning" )
            {
                groupMemberRequirement.WarningWorkflowId = workflow.Id;
            }

            rockContext.SaveChanges();
        }

        private class JobSummary
        {
            public const string SUCCESS_ICON = "<i class='ti ti-circle-filled text-success'></i> ";
            public const string WARNING_ICON = "<i class='ti ti-circle-filled text-warning'></i> ";
            public const string ERROR_ICON = "<i class='ti ti-circle-filled text-error'></i> ";

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

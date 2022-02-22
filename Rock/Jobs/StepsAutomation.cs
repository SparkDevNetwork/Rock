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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;

using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Automate Steps From Dataviews
    /// </summary>
    [DisplayName( "Steps Automation" )]
    [Description( "Creates steps for people within a dataview." )]

    [IntegerField(
        "Duplicate Prevention Day Range",
        description: "This setting will keep additional step records from being added if a step was already added within the number of days provided.",
        required: false,
        defaultValue: 7,
        order: 1,
        key: AttributeKey.DuplicatePreventionDayRange )]

    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for the sql operations to complete. Leave blank to use the default for this job (180).",
        IsRequired = false,
        DefaultIntegerValue = AttributeDefaultValue.CommandTimeout,
        Category = "General",
        Order = 7 )]

    [DisallowConcurrentExecution]
    public class StepsAutomation : IJob
    {
        #region Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        public static class AttributeKey
        {
            /// <summary>
            /// The duplicate prevention day range
            /// </summary>
            public const string DuplicatePreventionDayRange = "DuplicatePreventionDayRange";

            /// <summary>
            /// The SQL Command Timeout
            /// </summary>
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <summary>
        /// Default Values for Attributes
        /// </summary>
        private static class AttributeDefaultValue
        {
            public const int CommandTimeout = 180;
        }

        private DateTime _lastProgressUpdate = DateTime.MinValue;

        #endregion Keys

        #region Constructors

        /// <summary>
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public StepsAutomation()
        {
        }

        #endregion Constructors

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            _sqlCommandTimeoutSeconds = context.JobDetail.JobDataMap.GetString( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? AttributeDefaultValue.CommandTimeout;

            // Use concurrent safe data structures to track the count and errors
            var errors = new ConcurrentBag<string>();
            var addedResults = new ConcurrentBag<int>();
            var updatedResults = new ConcurrentBag<int>();

            // Get the step type view query
            var stepTypeViews = GetStepTypeViews().OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();

            // Get the day threshold for adding new steps
            var minDaysBetweenSteps = GetDuplicatePreventionDayRange( context );

            // Loop through each step type and create steps based on what is in the dataview
            foreach ( var stepTypeView in stepTypeViews )
            {
                ProcessStepType( context, stepTypeView, minDaysBetweenSteps, addedResults, updatedResults, out var errorsFromThisStepType );
                if ( errorsFromThisStepType != null && errorsFromThisStepType.Any() )
                {
                    errorsFromThisStepType.ForEach( errors.Add );
                }
            }

            // Set the results for the job log
            var totalAdded = addedResults.Sum();
            var totalUpdated = updatedResults.Sum();
            context.Result = $"{totalAdded} step{( totalAdded == 1 ? "" : "s" )} added. {totalUpdated} step{( totalUpdated == 1 ? "" : "s" )} updated.";

            if ( errors.Any() )
            {
                ThrowErrors( context, errors );
            }
        }

        private int _sqlCommandTimeoutSeconds = AttributeDefaultValue.CommandTimeout;

        /// <summary>
        /// Processes the step type. Add steps for everyone in the dataview
        /// </summary>
        /// <param name="jobContext">The job context.</param>
        /// <param name="stepTypeView">The step type view.</param>
        /// <param name="minDaysBetweenSteps">The minimum days between steps.</param>
        /// <param name="addedResults">The added results.</param>
        /// <param name="updatedResults">The updated results.</param>
        /// <param name="errorMessages">The error message.</param>
        private void ProcessStepType(
            IJobExecutionContext jobContext,
            StepTypeView stepTypeView,
            int minDaysBetweenSteps,
            ConcurrentBag<int> addedResults,
            ConcurrentBag<int> updatedResults,
            out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            var rockContextGetList = new RockContext();
            rockContextGetList.Database.CommandTimeout = _sqlCommandTimeoutSeconds;

            // Steps are created with a status of "complete", so if we need to know the status id
            var stepStatusId = stepTypeView.CompletedStepStatusIds.FirstOrDefault();

            if ( stepStatusId == default )
            {
                errorMessages.Add( $"The Step Type with id {stepTypeView.StepTypeId} does not have a valid Complete Status to use" );
                return;
            }

            // Get the dataview configured for the step type
            var dataViewService = new DataViewService( rockContextGetList );
            var dataview = dataViewService.Get( stepTypeView.AutoCompleteDataViewId );

            if ( dataview == null )
            {
                errorMessages.Add( $"The dataview {stepTypeView.AutoCompleteDataViewId} for step type {stepTypeView.StepTypeId} did not resolve" );
                return;
            }

            // We can use the dataview to get the person alias id query
            var dataViewGetQueryArgs = new DataViewGetQueryArgs
            {
                DbContext = rockContextGetList,
                DatabaseTimeoutSeconds = _sqlCommandTimeoutSeconds
            };

            IQueryable<IEntity> dataviewQuery;
            try
            {
                dataviewQuery = dataview.GetQuery( dataViewGetQueryArgs );
            }
            catch ( Exception ex )
            {
                errorMessages.Add( ex.Message );
                ExceptionLogService.LogException( ex );
                return;
            }

            if ( dataviewQuery == null )
            {
                errorMessages.Add( $"Generating a query for dataview {stepTypeView.AutoCompleteDataViewId} for step type {stepTypeView.StepTypeId} was not successful" );
                return;
            }

            // This query contains person ids in the dataview
            var personIdQuery = dataviewQuery.AsNoTracking().Select( e => e.Id );

            // Get the query for people that cannot get a new step
            var personIdsThatCannotGetStepQuery = GetPersonIdsThatCannotGetStepQuery( rockContextGetList, stepTypeView, minDaysBetweenSteps );

            // Subtract the people that cannot get a new step
            personIdQuery = personIdQuery.Except( personIdsThatCannotGetStepQuery );

            // If there are prerequisites, then subtract the people that cannot get the step because of unmet prerequisites
            if ( stepTypeView.PrerequisiteStepTypeIds.Any() )
            {
                var personIdsThatHaveMetPrerequisitesQuery = GetPersonIdsThatHaveMetPrerequisitesQuery( rockContextGetList, stepTypeView );
                personIdQuery = personIdQuery.Intersect( personIdsThatHaveMetPrerequisitesQuery );
            }

            // Convert to person aliases ids
            var personAliasService = new PersonAliasService( rockContextGetList );
            var personInfoList = personAliasService.GetPrimaryAliasQuery()
                .Where( a => personIdQuery.Contains( a.PersonId ) )
                .Select( a => new
                {
                    PersonId = a.PersonId,
                    PrimaryAliasId = a.Id
                } )
                .ToList();

            // Add or update steps for each of the remaining aliases that have met all the conditions
            var stepServiceGetList = new StepService( rockContextGetList );
            var now = RockDateTime.Now;
            var addedCount = 0;
            var updatedCount = 0;

            // Query for existing incomplete steps for the people
            // Just in case the Person is has more than one incomplete step for this step type, just use the latest one.
            // It should clean it self up on subsequent runs since the other steps for this person wouldn't have been marked complete yet
            var existingIncompleteStepIdsByPersonId = stepServiceGetList.Queryable()
                .Where( s =>
                    s.StepTypeId == stepTypeView.StepTypeId &&
                    personIdQuery.Contains( s.PersonAlias.PersonId ) &&
                    !s.CompletedDateTime.HasValue )
                .Select( a => new
                {
                    a.PersonAlias.PersonId,
                    StepId = a.Id
                } )
                .ToList()
                .GroupBy( a => a.PersonId )
                .ToDictionary( k => k.Key, v => v.Max( s => s.StepId ) );

            long totalCount = personInfoList.Count;
            long progressCount = 0;
            foreach ( var personIdInfo in personInfoList )
            {
                var personId = personIdInfo.PersonId;
                var personPrimaryAliasId = personIdInfo.PrimaryAliasId;

                var existingStepId = existingIncompleteStepIdsByPersonId.GetValueOrNull( personId );

                using ( var rockContextLoop = new RockContext() )
                {
                    var stepServiceLoop = new StepService( rockContextLoop );
                    Step step;
                    if ( existingStepId.HasValue )
                    {
                        step = stepServiceLoop.Get( existingStepId.Value );
                    }
                    else
                    {
                        step = new Step
                        {
                            StepTypeId = stepTypeView.StepTypeId,
                            Caption = stepTypeView.Name,
                            StartDateTime = now,
                            PersonAliasId = personPrimaryAliasId
                        };
                    }

                    step.CompletedDateTime = now;
                    step.StepStatusId = stepStatusId;

                    if ( !existingStepId.HasValue )
                    {
                        stepServiceLoop.AddWithoutValidation( step );
                        addedCount++;
                    }
                    else
                    {
                        updatedCount++;
                    }

                    rockContextLoop.SaveChanges();
                }

                progressCount++;

                // Update the progress every 5 seconds
                if ( ( RockDateTime.Now - _lastProgressUpdate ).TotalSeconds >= 5 )
                {
                    try
                    {
                        jobContext.UpdateLastStatusMessage( $"Processing {stepTypeView.Name } steps : {progressCount}/{totalCount}" );
                    }
                    catch ( Exception ex )
                    {
                        // ignore, but write to debug output
                        System.Diagnostics.Debug.WriteLine( $"Error updating LastStatusMessage for ProcessStepType loop: {ex}" );
                    }
                    finally
                    {
                        _lastProgressUpdate = RockDateTime.Now;
                    }
                }
            }

            addedResults.Add( addedCount );
            updatedResults.Add( updatedCount );
        }

        #region Data Helpers

        /// <summary>
        /// Gets the step type view query. All active step types that have a person based
        /// dataview configured
        /// </summary>
        /// <returns></returns>
        private List<StepTypeView> GetStepTypeViews()
        {
            var personEntityTypeId = EntityTypeCache.Get<Person>().Id;
            var rockContext = new RockContext();
            rockContext.Database.CommandTimeout = _sqlCommandTimeoutSeconds;
            var stepTypeService = new StepTypeService( rockContext );

            var views = stepTypeService.Queryable().AsNoTracking()
                .Where( st =>
                     st.StepProgram.IsActive &&
                     st.IsActive &&
                     st.AutoCompleteDataViewId.HasValue &&
                     st.AutoCompleteDataView.EntityTypeId == personEntityTypeId )
                .Select( st => new StepTypeView
                {
                    StepTypeId = st.Id,
                    Name = st.Name,
                    Order = st.Order,
                    StepProgramId = st.StepProgramId,
                    AllowMultiple = st.AllowMultiple,
                    AutoCompleteDataViewId = st.AutoCompleteDataViewId.Value,
                    PrerequisiteStepTypeIds = st.StepTypePrerequisites.Select( stp => stp.PrerequisiteStepTypeId ),

                    // Get a list of the step statuses, but only take 1 so that it doesn't cause a second database call
                    CompletedStepStatusIds = st.StepProgram.StepStatuses
                        .OrderBy( ss => ss.Order )
                        .Where( ss =>
                            ss.IsCompleteStatus &&
                            ss.IsActive )
                        .Select( ss => ss.Id )
                        .Take( 1 )
                } )
                .ToList();

            return views;
        }

        /// <summary>
        /// These are people that cannot have new step because they already
        /// have one and are within the minimum date range.
        /// </summary>
        /// <param name="stepTypeView">The step type view.</param>
        /// <param name="rockContext"></param>
        /// <param name="minDaysBetweenSteps"></param>
        /// <returns></returns>
        private IQueryable<int> GetPersonIdsThatCannotGetStepQuery( RockContext rockContext, StepTypeView stepTypeView, int minDaysBetweenSteps )
        {
            // We are querying for people that will ultimately be excluded from getting a new
            // step created from this job.
            var stepService = new StepService( rockContext );
            var query = stepService.Queryable().AsNoTracking().Where( s => s.StepTypeId == stepTypeView.StepTypeId );

            if ( stepTypeView.AllowMultiple )
            {
                // If allow multiple and completed date is within the minDaysBetweenSteps timeframe
                var minStepDate = minDaysBetweenSteps >= 1 ?
                    RockDateTime.Now.AddDays( 0 - minDaysBetweenSteps ) :
                    DateTime.MinValue;

                query = query.Where( s => s.CompletedDateTime.HasValue && s.CompletedDateTime >= minStepDate );
            }
            else
            {
                // If not allow multiple and has a completed date at all
                query = query.Where( s => s.CompletedDateTime.HasValue );
            }

            return query.Select( s => s.PersonAlias.PersonId );
        }

        /// <summary>
        /// Get a query for people that have met prerequisites
        /// </summary>
        /// <param name="rockContext"></param>
        /// <param name="stepTypeView"></param>
        /// <returns></returns>
        private IQueryable<int> GetPersonIdsThatHaveMetPrerequisitesQuery( RockContext rockContext, StepTypeView stepTypeView )
        {
            var stepService = new StepService( rockContext );

            // We are querying for people that have met all the prerequisites for this step type
            // This method should not be called for stepTypes that do not have prerequisites
            // because that would be a query for everyone in the database
            var firstStepTypeId = stepTypeView.PrerequisiteStepTypeIds.First();
            var prerequisiteCount = stepTypeView.PrerequisiteStepTypeIds.Count();

            // Aliases that have completed the first prerequisite
            var query = stepService.Queryable().AsNoTracking()
                .Where( s =>
                    s.StepStatus.IsCompleteStatus &&
                    s.StepTypeId == firstStepTypeId )
                .Select( s => s.PersonAlias.PersonId );

            for ( var i = 1; i < prerequisiteCount; i++ )
            {
                var stepTypeId = stepTypeView.PrerequisiteStepTypeIds.ElementAt( i );

                // Aliases that have completed this subsequent prerequisite
                var subquery = stepService.Queryable().AsNoTracking()
                    .Where( s =>
                        s.StepStatus.IsCompleteStatus &&
                        s.StepTypeId == stepTypeId )
                    .Select( s => s.PersonAlias.PersonId );

                // Find the intersection (people in the main query who have also met this prerequisite)
                query = query.Intersect( subquery );
            }

            return query;
        }

        #endregion Data Helpers

        #region Job State Helpers

        /// <summary>
        /// Gets the duplicate prevention day range.
        /// </summary>
        /// <param name="jobExecutionContext">The job execution context.</param>
        /// <returns></returns>
        private int GetDuplicatePreventionDayRange( IJobExecutionContext jobExecutionContext )
        {
            var days = jobExecutionContext.JobDetail.JobDataMap.GetString( AttributeKey.DuplicatePreventionDayRange ).AsInteger();
            return days;
        }

        /// <summary>
        /// Throws the errors.
        /// </summary>
        /// <param name="jobExecutionContext">The job execution context.</param>
        /// <param name="errors">The errors.</param>
        private void ThrowErrors( IJobExecutionContext jobExecutionContext, IEnumerable<string> errors )
        {
            var sb = new StringBuilder();

            if ( !jobExecutionContext.Result.ToStringSafe().IsNullOrWhiteSpace() )
            {
                sb.AppendLine();
            }

            sb.AppendLine( string.Format( "{0} Errors: ", errors.Count() ) );

            foreach ( var error in errors )
            {
                sb.AppendLine( error );
            }

            var errorMessage = sb.ToString();
            jobExecutionContext.Result += errorMessage;

            var exception = new Exception( errorMessage );
            var httpContext = HttpContext.Current;
            ExceptionLogService.LogException( exception, httpContext );

            throw exception;
        }

        #endregion Job State Helpers

        #region SQL Views

        /// <summary>
        /// Selected properties from a <see cref="StepType"/> related query
        /// </summary>
        private class StepTypeView
        {
            /// <summary>
            /// Gets or sets the step type identifier.
            /// </summary>
            /// <value>
            /// The step type identifier.
            /// </value>
            public int StepTypeId { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>The name.</value>
            public string Name { get; internal set; }

            /// <summary>
            /// Gets or sets the order.
            /// </summary>
            /// <value>The order.</value>
            public int Order { get; internal set; }

            /// <summary>
            /// Gets or sets the step program identifier.
            /// </summary>
            /// <value>
            /// The step program identifier.
            /// </value>
            public int StepProgramId { get; set; }

            /// <summary>
            /// Gets or sets the completed step status ids.
            /// </summary>
            /// <value>
            /// The completed step status ids.
            /// </value>
            public IEnumerable<int> CompletedStepStatusIds { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [allow multiple].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [allow multiple]; otherwise, <c>false</c>.
            /// </value>
            public bool AllowMultiple { get; set; }

            /// <summary>
            /// Gets or sets a value indicating the dataview id.
            /// </summary>
            /// <value>
            ///   The dataview ID
            /// </value>
            public int AutoCompleteDataViewId { get; set; }

            /// <summary>
            /// Prerequisite StepTypeIds
            /// </summary>
            public IEnumerable<int> PrerequisiteStepTypeIds { get; set; }
        }

        #endregion SQL Views
    }
}

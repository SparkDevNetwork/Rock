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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        description: "If duplicates are enabled above, this setting will keep steps from being added if a previous step was within the number of days provided.",
        required: false,
        defaultValue: 7,
        order: 1,
        key: AttributeKey.DuplicatePreventionDayRange )]

    [DisallowConcurrentExecution]
    public class StepsAutomation : IJob
    {
        #region Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The duplicate prevention day range
            /// </summary>
            public const string DuplicatePreventionDayRange = "DuplicatePreventionDayRange";
        }

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
            // Use concurrent safe data structures to track the count and errors
            var errors = new ConcurrentBag<string>();
            var results = new ConcurrentBag<int>();

            // Get the step type view query
            var stepTypeViews = GetStepTypeViews();

            // Get the day threshold for adding new steps
            var minDaysBetweenSteps = GetDuplicatePreventionDayRange( context );

            // Loop through each step type and create steps based on what is in the dataview
            Parallel.ForEach( stepTypeViews, stepTypeView =>
            {
                var stepsAdded = ProcessStepType( stepTypeView, minDaysBetweenSteps, out var errorsFromThisStepType );

                if ( errorsFromThisStepType != null && errorsFromThisStepType.Any() )
                {
                    errorsFromThisStepType.ForEach( errors.Add );
                }

                results.Add( stepsAdded );
            } );

            // Set the results for the job log
            var total = results.Sum();
            context.Result = $"{total} steps added";

            if ( errors.Any() )
            {
                ThrowErrors( context, errors );
            }
        }

        /// <summary>
        /// Processes the step type. Add steps for everyone in the dataview
        /// </summary>
        /// <param name="stepTypeView">The step type view.</param>
        /// <param name="errorMessages">The error message.</param>
        /// <param name="minDaysBetweenSteps"></param>
        /// <returns></returns>
        private int ProcessStepType( StepTypeView stepTypeView, int minDaysBetweenSteps, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            var rockContext = new RockContext();

            // Steps are created with a status of "complete", so if we need to know the status id
            var stepStatusId = stepTypeView.CompletedStepStatusIds.FirstOrDefault();

            if ( stepStatusId == default )
            {
                errorMessages.Add( $"The Step Type with id {stepTypeView.StepTypeId} does not have a valid Complete Status to use" );
                return 0;
            }

            // Get the dataview configured for the step type
            var dataViewService = new DataViewService( rockContext );
            var dataview = dataViewService.Get( stepTypeView.AutoCompleteDataViewId );

            if ( dataview == null )
            {
                errorMessages.Add( $"The dataview {stepTypeView.AutoCompleteDataViewId} for step type {stepTypeView.StepTypeId} did not resolve" );
                return 0;
            }

            // We can use the dataview to get the person alias id query
            var dataviewQuery = dataview.GetQuery( null, rockContext, null, out var dataviewQueryErrors );

            if ( dataviewQueryErrors != null && dataviewQueryErrors.Any() )
            {
                errorMessages.AddRange( dataviewQueryErrors );
                return 0;
            }

            if ( dataviewQuery == null )
            {
                errorMessages.Add( $"Generating a query for dataview {stepTypeView.AutoCompleteDataViewId} for step type {stepTypeView.StepTypeId} was not successful" );
                return 0;
            }

            // This query contains person ids in the dataview
            var personIdQuery = dataviewQuery.AsNoTracking().Select( e => e.Id );

            // Get the query for person aliases that cannot get a new step
            var peopleThatCannotGetStepQuery = GetPeopleThatCannotGetStepQuery( rockContext, stepTypeView, minDaysBetweenSteps );

            // Subtract the people that cannot get a new step
            personIdQuery = personIdQuery.Except( peopleThatCannotGetStepQuery );

            // If there are prerequisites, then subtract the people that cannot be the step because of unmet prerequisites
            if ( stepTypeView.PrerequisiteStepTypeIds.Any() )
            {
                var peopleThatHaveMetPrerequisitesQuery = GetPeopleThatHaveMetPrerequisitesQuery( rockContext, stepTypeView );
                personIdQuery = personIdQuery.Intersect( peopleThatHaveMetPrerequisitesQuery );
            }

            // Convert to person aliases ids
            var personAliasService = new PersonAliasService( rockContext );
            var personAliasIds = personAliasService.Queryable().AsNoTracking()
                .Where( pa => personIdQuery.Contains( pa.PersonId ) )
                .Select( pa => pa.Id )
                .Distinct()
                .ToList();

            // Add steps for each of the remaining aliases that have met all the conditions
            var stepService = new StepService( rockContext );
            var now = RockDateTime.Now;
            var count = 0;

            foreach ( var personAliasId in personAliasIds )
            {
                var step = new Step
                {
                    StepTypeId = stepTypeView.StepTypeId,
                    CompletedDateTime = now,
                    StartDateTime = now,
                    StepStatusId = stepStatusId,
                    PersonAliasId = personAliasId
                };

                stepService.AddWithoutValidation( step );
                count++;

                if ( count % 100 == 0 )
                {
                    rockContext.SaveChanges();
                }
            }

            rockContext.SaveChanges();
            return count;
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
        private IQueryable<int> GetPeopleThatCannotGetStepQuery( RockContext rockContext, StepTypeView stepTypeView, int minDaysBetweenSteps )
        {
            var stepService = new StepService( rockContext );
            var minStepDate = DateTime.MinValue;

            // We are querying for people that will ultimately be excluded from getting a new
            // step created from this job.
            // If duplicates are not allowed, then we want to find anyone with a step ever
            // If duplicates are allowed and a day range is set, then it is within that timeframe.
            if ( stepTypeView.AllowMultiple && minDaysBetweenSteps >= 1 )
            {
                minStepDate = RockDateTime.Now.AddDays( 0 - minDaysBetweenSteps );
            }

            var query = stepService.Queryable().AsNoTracking()
                .Where( s =>
                    s.StepTypeId == stepTypeView.StepTypeId &&
                    ( !s.CompletedDateTime.HasValue || s.CompletedDateTime.Value >= minStepDate ) )
                .Select( s => s.PersonAlias.PersonId );

            return query;
        }

        /// <summary>
        /// Get a query for people that have met prerequisites
        /// </summary>
        /// <param name="rockContext"></param>
        /// <param name="stepTypeView"></param>
        /// <returns></returns>
        private IQueryable<int> GetPeopleThatHaveMetPrerequisitesQuery( RockContext rockContext, StepTypeView stepTypeView )
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

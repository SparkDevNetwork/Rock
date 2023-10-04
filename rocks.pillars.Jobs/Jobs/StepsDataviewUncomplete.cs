// <copyright>
// Copyright Pillars Inc.
// </copyright>
//
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Field.Types;
using Rock.Model;
using Rock.Web.Cache;

namespace rocks.pillars.Jobs.Jobs
{
    /// <summary>
    /// Job that executes CSharp code.
    /// </summary>
    [StepProgramField("Step Program", "The program who's steps should be be uncompleted if person is no longer in the step's associated auto-complete data view.", true, "", "", 0)]
    [StepProgramStepStatusField("New Status", "Optional status to set any existing completed steps to. Leave blank to delete all steps for person who is not in data view.", false, "", "", 1)]

    [DisallowConcurrentExecution]
    public class StepsDataviewUncomplete : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            // Use concurrent safe data structures to track the count and errors
            var errors = new ConcurrentBag<string>();
            var results = new ConcurrentBag<int>();

            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var stepProgramGuid = dataMap.GetString("StepProgram").AsGuidOrNull();

            if ( !stepProgramGuid.HasValue )
            {
                errors.Add( $"A valid step program is required" );
            }
            else
            {
                StepStatus stepStatus = null;
                var stepStatusValue = dataMap.GetString( "NewStatus" );
                StepProgramStepStatusFieldType.ParseDelimitedGuids( stepStatusValue, out var unused, out var stepStatusGuid );
                if ( stepStatusGuid.HasValue )
                {
                    stepStatus = new StepStatusService( new RockContext() ).Get( stepStatusGuid.Value );
                }

                // Get the step type view query
                var stepTypeViews = GetStepTypeViews( stepProgramGuid.Value );

                // Loop through each step type and create steps based on what is in the dataview
                Parallel.ForEach( stepTypeViews, stepTypeView =>
                {
                    var stepsAdded = ProcessStepType( stepTypeView, stepStatus, out var errorsFromThisStepType );

                    if ( errorsFromThisStepType != null && errorsFromThisStepType.Any() )
                    {
                        errorsFromThisStepType.ForEach( errors.Add );
                    }

                    results.Add( stepsAdded );
                } );

                // Set the results for the job log
                var total = results.Sum();
                var action = stepStatus == null ? "deleted" : "updated";
                context.Result = $"{total} steps {action}";
            }

            if ( errors.Any() )
            {
                ThrowErrors( context, errors );
            }
        }

        private int ProcessStepType( StepTypeView stepTypeView, StepStatus newStepStatus, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            var rockContext = new RockContext();

            // Get the dataview configured for the step type
            var dataViewService = new DataViewService( rockContext );
            var dataview = dataViewService.Get( stepTypeView.AutoCompleteDataViewId );

            if ( dataview == null )
            {
                errorMessages.Add( $"The dataview {stepTypeView.AutoCompleteDataViewId} for step type {stepTypeView.StepTypeId} did not resolve" );
                return 0;
            }

            // We can use the dataview to get the person alias id query
            var dvArgs = new DataViewGetQueryArgs
            {
                DbContext = rockContext
            };
            var dataviewQuery = dataview.GetQuery( dvArgs );

            if ( dataviewQuery == null )
            {
                errorMessages.Add( $"Generating a query for dataview {stepTypeView.AutoCompleteDataViewId} for step type {stepTypeView.StepTypeId} was not successful" );
                return 0;
            }

            // This query contains person ids in the dataview
            var personIdQry = dataviewQuery.AsNoTracking().Select( e => e.Id );

            var stepService = new StepService( rockContext );
            var stepQry = stepService.Queryable()
                .Where( s =>
                    s.StepTypeId == stepTypeView.StepTypeId &&
                    s.PersonAlias != null &&
                    !personIdQry.Contains( s.PersonAlias.PersonId ) );

            if ( newStepStatus != null )
            {
                stepQry = stepQry.Where( s => s.CompletedDateTime.HasValue );
            }

            var steps = stepQry.ToList();

            foreach( var step in steps )
            {
                if ( newStepStatus != null )
                {
                    step.CompletedDateTime = null;
                    step.StepStatusId = newStepStatus.Id;
                }
                else
                {
                    stepService.Delete( step );
                }
            }
        
            rockContext.SaveChanges();
            return steps.Count;
        }

        /// <summary>
        /// Gets the step type view query. All active step types that have a person based
        /// dataview configured
        /// </summary>
        /// <returns></returns>
        private List<StepTypeView> GetStepTypeViews(Guid stepProgramGuid)
        {
            var personEntityTypeId = EntityTypeCache.Get<Person>().Id;
            var rockContext = new RockContext();
            var stepTypeService = new StepTypeService( rockContext );

            var views = stepTypeService.Queryable().AsNoTracking()
                .Where( st =>
                    st.StepProgram.Guid == stepProgramGuid &&
                    st.StepProgram.IsActive &&
                    st.IsActive &&
                    st.AutoCompleteDataViewId.HasValue &&
                    st.AutoCompleteDataView.EntityTypeId == personEntityTypeId )
                .Select( st => new StepTypeView
                {
                    StepTypeId = st.Id,
                    AutoCompleteDataViewId = st.AutoCompleteDataViewId.Value
                } )
                .ToList();

            return views;
        }

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
            /// Gets or sets a value indicating the dataview id.
            /// </summary>
            /// <value>
            ///   The dataview ID
            /// </value>
            public int AutoCompleteDataViewId { get; set; }

        }

    }
}

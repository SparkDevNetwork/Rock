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
using System.Data.Entity;
using System.Linq;
using System.Text;

using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Lms;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Job that updates Learning Program Completion Data.
    /// </summary>
    [DisplayName( "Update Program Completions" )]
    [Description( "Job that updates Learning Program Completion Data." )]

    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeoutSeconds,
        Description = "Maximum amount of time (in seconds) to wait for the SQL operations to complete. Leave blank to use the default for this job (180).",
        IsRequired = false,
        DefaultIntegerValue = 60 * 3,
        Order = 1 )]

    public class UpdateProgramCompletions : RockJob
    {
        /// <summary>
        /// Keys to use for Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string CommandTimeoutSeconds = "CommandTimeoutSeconds";
        }

        /// <inheritdoc/>
        public override void Execute()
        {
            JobState state;

            using ( var rockContext = CreateRockContext() )
            {
                state = InitializeJobState( rockContext );
            }

            // Loop over each participant that is in progress and update or
            // create the completion record.
            foreach ( var activeParticipantAndProgram in state.ActiveParticipantsAndPrograms )
            {
                using ( var individualRockContext = CreateRockContext() )
                {
                    AddOrUpdateProgramCompletion( activeParticipantAndProgram, state, individualRockContext );
                }
            }

            UpdateLastStatusMessage( GetJobResultText( state ) );
        }

        /// <summary>
        /// Get the job state object that will contain the information and
        /// cache used while the job is running.
        /// </summary>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>A new instance of <see cref="JobState"/> that contains the job data.</returns>
        private JobState InitializeJobState( RockContext rockContext )
        {
            var participantService = new LearningParticipantService( rockContext );
            var completionService = new LearningProgramCompletionService( rockContext );
            var courseService = new LearningCourseService( rockContext );

            // Find all courses that belong to programs which are being
            // tracked. We don't filter by active because we still want
            // to process records if the admin just turned the course
            // off today.
            var trackedCourseIdQry = courseService.Queryable()
                .Where( c => c.LearningProgram.IsCompletionStatusTracked )
                .Select( c => c.Id );

            // A query that includes all completed programs and the person
            // that completed it.
            var completedProgramQry = completionService.Queryable()
                .Where( lpc => lpc.CompletionStatus == CompletionStatus.Completed )
                .Select( lpc => new
                {
                    lpc.PersonAlias.PersonId,
                    lpc.LearningProgramId
                } );

            // Find all participants and associated programs that have not
            // already been completed.
            var activeParticipantsAndPrograms = participantService.Queryable()
                .Where( p => trackedCourseIdQry.Contains( p.LearningClass.LearningCourseId )
                    && p.Person.PrimaryAliasId.HasValue
                    && !completedProgramQry.Any( c => c.PersonId == p.PersonId && c.LearningProgramId == p.LearningClass.LearningCourse.LearningProgramId ) )
                .Select( p => new ParticipantAndProgram
                {
                    PersonId = p.Person.Id,
                    PrimaryAliasId = p.Person.PrimaryAliasId.Value,
                    LearningProgramId = p.LearningClass.LearningCourse.LearningProgramId
                } )
                .Distinct()
                .ToList();

            // Create a lookup table to hold all course identifiers for
            // a program. In this case, we exclude inactive courses because
            // this lookup is used to determine if the course is complete.
            // If the course is inactive from a long time ago then we want
            // to ignore it, and if it was made inactive just now then us
            // ignoring it is also safe since it would be considered completed.
            var activeCourseIdLookup = courseService.Queryable()
                .Where( lc => lc.IsActive )
                .Select( lc => new
                {
                    lc.Id,
                    lc.LearningProgramId
                } )
                .ToList()
                .GroupBy( lc => lc.LearningProgramId )
                .ToDictionary( grp => grp.Key, grp => grp.Select( g => g.Id ).ToList() );

            // Load all the course equivalent records. This is a dictionary of
            // courses and the list of other course ids that are considered
            // equivalent - including recursive equivalents.
            var courseEquivalentIdLookup = GetCourseEquivalentIds( rockContext );

            return new JobState( activeParticipantsAndPrograms, activeCourseIdLookup, courseEquivalentIdLookup );
        }

        /// <summary>
        /// Add a new or update an existing program completion record for the
        /// participant.
        /// </summary>
        /// <param name="participantAndProgram">The participant and program to be processed.</param>
        /// <param name="state">The job state object.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        private static void AddOrUpdateProgramCompletion( ParticipantAndProgram participantAndProgram, JobState state, RockContext rockContext )
        {
            var programCompletionService = new LearningProgramCompletionService( rockContext );

            // Look for an existing program completion record.
            var programCompletion = programCompletionService
                .Queryable()
                .Where( lpc => lpc.CompletionStatus == CompletionStatus.Pending )
                .FirstOrDefault( lpc => lpc.PersonAlias.PersonId == participantAndProgram.PersonId
                    && lpc.LearningProgramId == participantAndProgram.LearningProgramId );

            // This shouldn't happen. If it does, we want to throw an error so
            // it will be seen and fixed. If a person has a program completion
            // record for this program with a status of "Completed" then we should
            // be skipping their participant records already.
            if ( programCompletion != null && programCompletion.CompletionStatus == CompletionStatus.Completed )
            {
                throw new InvalidOperationException( $"Attempt to update already complete record Id #{programCompletion.Id}." );
            }

            // Find all participant records for this person and program.
            var participation = new LearningParticipantService( rockContext )
                .Queryable()
                .Include( lp => lp.LearningClass )
                .Where( lp => lp.LearningClass.LearningCourse.LearningProgramId == participantAndProgram.LearningProgramId
                    && lp.Person.Id == participantAndProgram.PersonId )
                .ToList();

            // If there is not an existing completion record then create a
            // new one.
            if ( programCompletion == null )
            {
                programCompletion = new LearningProgramCompletion
                {
                    LearningProgramId = participantAndProgram.LearningProgramId,
                    PersonAliasId = participantAndProgram.PrimaryAliasId,
                    StartDate = participation.Min( lp => lp.CreatedDateTime ) ?? RockDateTime.Now,
                    CompletionStatus = CompletionStatus.Pending
                };

                programCompletionService.Add( programCompletion );
                state.ProgramsStarted++;
            }

            // Set the campus if we don't already have a campus value.
            if ( !programCompletion.CampusId.HasValue )
            {
                programCompletion.CampusId = participation
                    .OrderBy( p => p.CreatedDateTime )
                    .FirstOrDefault( p => p.LearningClass.CampusId.HasValue )
                    ?.LearningClass.CampusId;
            }

            // Find all active courses for this program.
            if ( !state.ActiveCourseIdLookup.TryGetValue( participantAndProgram.LearningProgramId, out var activeCourseIds ) )
            {
                activeCourseIds = new List<int>();
                state.ActiveCourseIdLookup.Add( participantAndProgram.LearningProgramId, activeCourseIds );
            }

            // Find all the course identifiers that they have passed
            var passedCourseIds = participation
                .Where( p => p.LearningCompletionStatus == LearningCompletionStatus.Pass )
                .Select( p => p.LearningClass.LearningCourseId )
                .ToList();

            var hasPassedAllCourses = activeCourseIds.All( courseId =>
            {
                if ( passedCourseIds.Contains( courseId ) )
                {
                    return true;
                }

                // Try and get the course identifiers that are considered equivalent
                // to this course.
                if ( state.CourseEquivalentIdLookup.TryGetValue( courseId, out var equivalentCourseIds ) )
                {
                    // If any of the equivalent courses have been passed then
                    // courseId is also considered to be passed.
                    if ( passedCourseIds.Any( passedCourseId => equivalentCourseIds.Contains( passedCourseId ) ) )
                    {
                        return true;
                    }
                }

                return false;
            } );

            // If they have passed all courses then mark the record as completed.
            if ( hasPassedAllCourses )
            {
                programCompletion.CompletionStatus = CompletionStatus.Completed;
                programCompletion.EndDate = participation
                    .Where( p => p.LearningCompletionDateTime.HasValue )
                    .Max( p => p.LearningCompletionDateTime );

                state.ProgramsCompleted++;
            }

            rockContext.WrapTransaction( () =>
            {
                // First save so we get a completion Id.
                rockContext.SaveChanges();

                // Update all participation records to point to this completion
                // unless they already have a completion.
                foreach ( var participant in participation.Where( p => !p.LearningProgramCompletionId.HasValue ) )
                {
                    participant.LearningProgramCompletionId = programCompletion.Id;
                }

                rockContext.SaveChanges();
            } );

            if ( programCompletion.CompletionStatus == CompletionStatus.Completed )
            {
                // TODO: Add cache for LMS to make this faster.
                var program = new LearningProgramService( rockContext ).Get( participantAndProgram.LearningProgramId );

                if ( program?.CompletionWorkflowTypeId != null )
                {
                    LaunchCompletionWorkflow( programCompletion, program, rockContext );
                    state.CompletionWorkflowsLaunched++;
                }
            }
        }

        /// <summary>
        /// Launch the completion workflow the completion record.
        /// </summary>
        /// <param name="completion">The record that was just completed.</param>
        /// <param name="program">The program the record belongs to.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        private static void LaunchCompletionWorkflow( LearningProgramCompletion completion, LearningProgram program, RockContext rockContext )
        {
            var personAliasGuid = new PersonAliasService( rockContext ).Queryable()
                .Where( pa => pa.Id == completion.PersonAliasId )
                .Select( pa => pa.Guid )
                .FirstOrDefault();

            var workflowAttributes = new Dictionary<string, string>
            {
                ["Person"] = personAliasGuid.ToString()
            };

            var workflowType = WorkflowTypeCache.Get( program.CompletionWorkflowTypeId.Value, rockContext );
            completion.LaunchWorkflow( completion.LearningProgram.CompletionWorkflowTypeId, null, workflowAttributes, null );
        }

        /// <summary>
        /// Get the result text that describes what happened during the job run.
        /// </summary>
        /// <param name="state">The job state object.</param>
        /// <returns>A string of text that describes the job results.</returns>
        private static string GetJobResultText( JobState state )
        {
            var completedProgramsText = "program".PluralizeIf( state.ProgramsCompleted != 1 );
            var newProgramsText = "program".PluralizeIf( state.ProgramsStarted != 1 );
            var completedWasOrWere = state.ProgramsCompleted == 1 ? "was" : "were";
            var newProgramsWasOrWere = state.ProgramsStarted == 1 ? "was" : "were";
            var workflowsText = "workflow".PluralizeIf( state.CompletionWorkflowsLaunched != 1 );
            var results = new StringBuilder();

            // Set the job result text.
            if ( state.ProgramsCompleted > 0 && state.ProgramsStarted > 0 )
            {
                results.AppendLine( $"{state.ProgramsCompleted} {completedProgramsText} {completedWasOrWere} completed and {state.ProgramsStarted} {newProgramsText} {newProgramsWasOrWere} started." );
            }
            else if ( state.ProgramsCompleted > 0 )
            {
                results.AppendLine( $"{state.ProgramsCompleted} {completedProgramsText} {completedWasOrWere} completed." );
            }
            else if ( state.ProgramsStarted > 0 )
            {
                results.AppendLine( $"{state.ProgramsStarted} {newProgramsText} {newProgramsWasOrWere} started." );
            }
            else
            {
                results.AppendLine( "No completions to add or update." );
            }

            results.AppendLine( $"{state.CompletionWorkflowsLaunched} completion {workflowsText} launched." );

            return results.ToString();
        }

        /// <summary>
        /// Creates a new <see cref="RockContext"/> configured for this job.
        /// </summary>
        /// <returns>An instance of <see cref="RockContext"/>.</returns>
        private RockContext CreateRockContext()
        {
            var rockContext = new RockContext();

            rockContext.Database.CommandTimeout = GetAttributeValue( AttributeKey.CommandTimeoutSeconds ).AsIntegerOrNull() ?? 180;

            return rockContext;
        }

        /// <summary>
        /// Load all course equivalent records and create a lookup table
        /// that represents the primary course identifier and then a list of
        /// other courses that are considered equivalent to this course.
        /// </summary>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>A dictionary whose key is a course identifier and value is a list of equivalent course identifiers.</returns>
        private static Dictionary<int, List<int>> GetCourseEquivalentIds( RockContext rockContext )
        {
            var lookupTable = new Dictionary<int, List<int>>();

            var courseEquivalents = new LearningCourseRequirementService( rockContext )
                .Queryable()
                .Where( lcr => lcr.RequirementType == RequirementType.Equivalent )
                .Select( lcr => new
                {
                    PrimaryCourseId = lcr.LearningCourseId,
                    EquivalentCourseId = lcr.RequiredLearningCourseId
                } )
                .ToList()
                .Select( e => (e.PrimaryCourseId, e.EquivalentCourseId) )
                .ToList();

            foreach ( var courseId in courseEquivalents.Select( c => c.PrimaryCourseId ).Distinct() )
            {
                lookupTable.Add( courseId, GetRecursiveEquivalentCourseIds( courseEquivalents, courseId ) );
            }

            return lookupTable;
        }

        /// <summary>
        /// Get all course identifiers that are considered equivalent to the
        /// <paramref name="primaryCourseId"/>. This method is recursive and
        /// will handle cases of A => B => C => D => A. If "A" is passed as
        /// the <paramref name="primaryCourseId"/> then "B", "C", and "D" will
        /// be returned.
        /// </summary>
        /// <param name="courseEquivalents"></param>
        /// <param name="primaryCourseId"></param>
        /// <param name="equivalentCourseIds"></param>
        /// <returns></returns>
        private static List<int> GetRecursiveEquivalentCourseIds( List<(int PrimaryCourseId, int EquivalentCourseId)> courseEquivalents, int primaryCourseId, List<int> equivalentCourseIds = null )
        {
            if ( equivalentCourseIds == null )
            {
                equivalentCourseIds = new List<int>();
            }

            foreach ( var id in courseEquivalents.Where( ce => ce.PrimaryCourseId == primaryCourseId ).Select( ce => ce.EquivalentCourseId ) )
            {
                if ( !equivalentCourseIds.Contains( id ) && id != primaryCourseId )
                {
                    equivalentCourseIds.Add( id );
                    GetRecursiveEquivalentCourseIds( courseEquivalents, id, equivalentCourseIds );
                }
            }

            return equivalentCourseIds;
        }

        #region Support Classes

        private class ParticipantAndProgram
        {
            public int PersonId { get; set; }

            public int PrimaryAliasId { get; set; }

            public int LearningProgramId { get; set; }
        }

        private class JobState
        {
            public List<ParticipantAndProgram> ActiveParticipantsAndPrograms { get; }

            public int ProgramsCompleted { get; set; }

            public int ProgramsStarted { get; set; }

            public int CompletionWorkflowsLaunched { get; set; }

            public Dictionary<int, List<int>> ActiveCourseIdLookup { get; }

            public Dictionary<int, List<int>> CourseEquivalentIdLookup { get; }

            public JobState( List<ParticipantAndProgram> activeParticipantsAndPrograms, Dictionary<int, List<int>> activeCourseIdLookup, Dictionary<int, List<int>> courseEquivalentIdLookup )
            {
                ActiveParticipantsAndPrograms = activeParticipantsAndPrograms;
                ActiveCourseIdLookup = activeCourseIdLookup;
                CourseEquivalentIdLookup = courseEquivalentIdLookup;
            }
        }

        #endregion
    }
}

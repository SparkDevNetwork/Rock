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
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Update Program Completions" )]
    [Description( "Job that updates Learning Program Completion Data." )]

    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeoutSeconds,
        Description = "Maximum amount of time (in seconds) to wait for the sql operations to complete. Leave blank to use the default for this job (180).",
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
            public const string RockContextDisablePrePostProcessingThreshold = "RockContextDisablePrePostProcessingThreshold";
        }

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public UpdateProgramCompletions()
        {
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            int commandTimeoutSeconds = this.GetAttributeValue( AttributeKey.CommandTimeoutSeconds ).AsIntegerOrNull() ?? 180;
            var resultsBuilder = new StringBuilder();
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeoutSeconds;

                var participantService = new LearningParticipantService( rockContext );
                var completionService = new LearningProgramCompletionService( rockContext );
                var courseService = new LearningCourseService( rockContext );

                var trackedCourses = courseService.Queryable()
                    .AsNoTracking()
                    .Include( c => c.LearningProgram )
                    .Where( c => c.LearningProgram.IsCompletionStatusTracked )
                    .Select( c => new
                    {
                        LearningCourseId = c.Id,
                        c.LearningProgramId
                    } );

                // All participant records for any of the tracked courses that don't already have a Program Completion record or have a 'Pending' completion status.
                var participants = participantService.Queryable()
                    .Include( p => p.Person )
                    .Include( p => p.LearningProgramCompletion )
                    .Include( p => p.LearningClass )
                    .Include( p => p.LearningClass.LearningCourse )
                    .Include( p => p.LearningGradingSystemScale )
                    .Where( p => trackedCourses.Any( t => t.LearningCourseId == p.LearningClass.LearningCourseId ) )
                    .Where( p => p.Person.PrimaryAliasId.HasValue );

                var completionData = participants.Select( p => new
                {
                    // Defer to existing values for fields other than EndDate and Status.
                    Id = p.LearningProgramCompletion != null ? p.LearningProgramCompletion.Id : 0,
                    LearningProgramId = p.LearningProgramCompletion != null ? p.LearningProgramCompletion.LearningProgramId : p.LearningClass.LearningCourse.LearningProgramId,
                    PersonAliasId = p.LearningProgramCompletion != null ? p.LearningProgramCompletion.PersonAliasId : ( int ) p.Person.PrimaryAliasId,

                    // Get the ParticipantId for syncing the LearningProgramCompletionId to the LearningParticipant (for new records).
                    ParticipantId = p.Id,

                    CampusId = p.LearningProgramCompletion != null ? p.LearningProgramCompletion.CampusId : p.LearningClass.CampusId,

                    // The current completionData status.
                    CompletionStatus = p.LearningProgramCompletion != null ? ( Enums.Lms.CompletionStatus? ) p.LearningProgramCompletion.CompletionStatus : null,

                    // The oldest enrollment date (GroupMember.CreatedDateTime) for the student.
                    MinClassCreatedDateTime = p.LearningProgramCompletion != null ? p.LearningProgramCompletion.StartDate : participants
                        .Where( p2 =>
                            p2.PersonId == p.PersonId &&
                            p2.LearningClass.LearningCourse.LearningProgramId == p.LearningClass.LearningCourse.LearningProgramId &&
                            p2.CreatedDateTime.HasValue
                        )
                        .Min( p2 => p2.LearningClass.CreatedDateTime ),

                    // The most recent completionData date for a course within the completion (and for the student).
                    MaxClassCompletionDateTime = participants
                        .Where( p2 =>
                            p2.PersonId == p.PersonId &&
                            p2.LearningClass.LearningCourse.LearningProgramId == p.LearningClass.LearningCourse.LearningProgramId &&
                            p2.LearningCompletionDateTime.HasValue
                        )
                        .Max( p2 => p2.LearningCompletionDateTime ),

                    // Whether the student has passed all courses in the completion.
                    AllCoursesPassed = participants.Where( p2 =>
                            p2.PersonId == p.PersonId &&
                            p2.LearningClass.LearningCourse.LearningProgramId == p.LearningClass.LearningCourse.LearningProgramId )
                        .All( p2 => p2.LearningCompletionDateTime.HasValue && p2.LearningGradingSystemScale.IsPassing )
                } )
                .ToList();

                var now = RockDateTime.Now;

                // Get a list of Program Completions that should be marked 'Completed'.
                var existingProgramsCompleted = completionData
                    .Where( p => p.AllCoursesPassed )
                    .Where( p => p.Id > 0 )
                    .Where( p => p.CompletionStatus != Enums.Lms.CompletionStatus.Completed )
                    .Select( p => new LearningProgramCompletion
                    {
                        Id = p.Id,
                        LearningProgramId = p.LearningProgramId,
                        PersonAliasId = p.PersonAliasId,
                        CampusId = p.CampusId,
                        StartDate = p.MinClassCreatedDateTime.HasValue ? p.MinClassCreatedDateTime.Value : now,
                        EndDate = p.MaxClassCompletionDateTime,
                        CompletionStatus = p.AllCoursesPassed ? Enums.Lms.CompletionStatus.Completed : Enums.Lms.CompletionStatus.Completed
                    } )
                    // The LearningParticipantIds will cause some duplication - make it distinct by ProgramCompletionId.
                    .DistinctBy( c => c.Id );

                // Get a list of Program Completions that need to be created with a 'Pending' status.
                var programsToCreate = completionData
                    .Where( p => p.Id == 0 )
                    .Select( p => new LearningProgramCompletion
                    {
                        LearningProgramId = p.LearningProgramId,
                        PersonAliasId = p.PersonAliasId,
                        CampusId = p.CampusId,
                        StartDate = p.MinClassCreatedDateTime.HasValue ? p.MinClassCreatedDateTime.Value : now,
                        EndDate = p.MaxClassCompletionDateTime.HasValue ? (DateTime?)p.MaxClassCompletionDateTime.Value : null,
                        CompletionStatus = Enums.Lms.CompletionStatus.Pending
                    } )
                    // The LearningParticipantIds will cause some duplication - make it distinct by ProgramId and PersonAliasId.
                    .DistinctBy( pc => new { pc.LearningProgramId, pc.PersonAliasId } );

                var completedProgramsText = "completion".PluralizeIf( existingProgramsCompleted.Count() != 1 );
                var newProgramsText = "completion".PluralizeIf( programsToCreate.Count() != 1 );
                var completedWasText = "was".PluralizeIf( existingProgramsCompleted.Count() != 1 );
                var newProgramsWasText = "was".PluralizeIf( programsToCreate.Count() != 1 );

                // Set the job result text.
                if ( existingProgramsCompleted.Any() && programsToCreate.Any() )
                {
                    resultsBuilder.AppendLine( $"{existingProgramsCompleted.Count()} {completedProgramsText} {completedWasText} updated to 'Completed' and {programsToCreate.Count()} {newProgramsText} {newProgramsWasText} created as 'Pending'." );
                }
                else if ( existingProgramsCompleted.Any() )
                {
                    resultsBuilder.AppendLine( $"{existingProgramsCompleted.Count()} {completedProgramsText} {completedWasText} updated to 'Completed'." );
                }
                else if ( programsToCreate.Any() )
                {
                    resultsBuilder.AppendLine( $"{programsToCreate.Count()} {newProgramsText} {newProgramsWasText} created as 'Pending'." );
                }
                else
                {
                    resultsBuilder.AppendLine( "No completions to add or update." );
                }

                var completionWorkflowsLaunched = 0;

                // If there are changes to be made perform them.
                if ( existingProgramsCompleted.Any() || programsToCreate.Any() )
                {
                    rockContext.WrapTransaction( () =>
                    {
                        // There were records to be added.
                        if ( programsToCreate.Any() )
                        {
                            completionService.AddRange( programsToCreate );
                        }

                        rockContext.SaveChanges();

                        var participantIds = completionData.Where( c => c.Id == 0 ).Select( c => c.ParticipantId );

                        var participantsData = participantService.Queryable()
                            .Where( p => participantIds.Contains( p.Id ) )
                            .ToList()
                            .Join(
                                completionData.Where( c => c.Id == 0 ),
                                p => p.Id,
                                c => c.ParticipantId,
                                ( p, c ) => new
                                {
                                    Participant = p,
                                    CompletionData = c
                                }
                            );

                        // Get the newly created programs so we can ensure the LearningParticipant links to the completion.
                        var allProgramCompletionIds = completionData
                            .Where( c => c.Id > 0 )
                            .Select( c => c.Id )
                            .Concat( programsToCreate.Select( p => p.Id ) )
                            .ToList()
                            .Distinct();

                        // Need to materialize now for use by completed programs to update
                        // and for later use by participantsData loop (for linking completion to participant).
                        var learningProgramCompletions = new LearningProgramCompletionService( rockContext )
                            .Queryable()
                            .Where( c => allProgramCompletionIds.Contains( c.Id ) )
                            .ToList();

                        // Uupdate the CompletionStatus and EndDate of newly completed programs.
                        var completedProgramsToUpdate = learningProgramCompletions.Where( p => existingProgramsCompleted.Any( e => e.Id == p.Id && p.CompletionStatus != Enums.Lms.CompletionStatus.Completed ) );
                        foreach ( var programCompletion in completedProgramsToUpdate )
                        {
                            programCompletion.CompletionStatus = Enums.Lms.CompletionStatus.Completed;
                            if ( !programCompletion.EndDate.HasValue )
                            {
                                programCompletion.EndDate = completionData.FirstOrDefault( c => c.Id == c.Id )?.MaxClassCompletionDateTime ?? now;
                            }
                        }

                        // Ensure the LearningParticipant record refers to the appropriate LearningProgramCompletion
                        // and ensure the ProgramCompletionStatus is completed if necessary.
                        foreach ( var data in participantsData )
                        {
                            var programCompletion = learningProgramCompletions
                                .FirstOrDefault( pc => pc.PersonAliasId == data.CompletionData.PersonAliasId && pc.LearningProgramId == data.CompletionData.LearningProgramId );

                            // Ensure the Participant is linked to the program.
                            data.Participant.LearningProgramCompletionId = programCompletion.Id;
                        }

                        // Again save changes before launching any workflows.
                        rockContext.SaveChanges();

                        var personAliasIds = completionData
                            .Select( c => c.PersonAliasId )
                            .Distinct()
                            .ToList();

                        var persons = new PersonService( rockContext )
                            .Queryable()
                            .Where( p => p.PrimaryAliasId.HasValue && personAliasIds.Contains( ( int ) p.PrimaryAliasId ) )
                            .ToList();

                        // For any newly completed programs launch the configured workflow (if any).
                        foreach ( var completedProgramData in existingProgramsCompleted )
                        {
                            var completion = learningProgramCompletions.FirstOrDefault( p => p.Id == completedProgramData.LearningProgramId );

                            // Skip the workflow launch if there's no completion or workflow.
                            if (
                                completion == null ||
                                !completion.LearningProgram.CompletionWorkflowTypeId.HasValue )
                            {
                                continue;
                            }

                            var person = persons.FirstOrDefault( p => p.PrimaryAliasId == completedProgramData.PersonAliasId );
                            var workflowAttributes = new Dictionary<string, string>
                            {
                                {"Person", person.ToJson() }
                            };

                            var workflow = WorkflowTypeCache.Get( completion.LearningProgram.CompletionWorkflowTypeId.Value );
                            completion.LaunchWorkflow( completion.LearningProgram.CompletionWorkflowTypeId, workflow?.Name, workflowAttributes, null );
                            completionWorkflowsLaunched++;
                        }

                    } );
                }

                var workflowsText = "workflow".PluralizeIf( completionWorkflowsLaunched != 1 );

                resultsBuilder.AppendLine( $"{completionWorkflowsLaunched} completion {workflowsText} launched." );

                this.UpdateLastStatusMessage( resultsBuilder.ToString() );
            }
        }
    }
}

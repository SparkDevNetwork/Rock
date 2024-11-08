﻿// <copyright>
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
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Enums.Lms;
using Rock.ViewModels.Blocks.Lms.LearningActivityComponent;

namespace Rock.Model
{
    public partial class LearningParticipantService
    {
        /// <summary>
        /// <para>Gets a list of <see cref="LearningActivity">Activities</see> for the course that the <see cref="LearningParticipant"/>
        /// is enrolled in.</para>.
        /// This method will return only assignments for the role type (e.g. Student assignments when the Participant role type is not a leader).
        /// </summary>
        /// <param name="participantId">The Id of the <see cref="LearningParticipant"/> for which to get activities.</param>
        /// <returns>Queryable of LearningActivities for the given participant and class.</returns>
        public IQueryable<LearningActivity> GetActivities( int participantId )
        {
            return Queryable()
                .Include( p => p.LearningClass )
                .Include( p => p.LearningClass.LearningActivities )
                .Include( p => p.LearningClass.LearningSemester )
                .Include( p => p.GroupRole )
                .Where( p => p.Id == participantId )
                .SelectMany( p =>
                    p.LearningClass.LearningActivities
                    .Where( a => a.AssignTo == ( p.GroupRole.IsLeader ? AssignTo.Facilitator : AssignTo.Student ) )
                );
        }

        /// <summary>
        /// Gets a list of <see cref="LearningActivityCompletion" /> for the specified <paramref name="activityId"/>.
        /// </summary>
        /// <param name="activityId">The identifier of the <see cref="LearningActivity"/> for which to retreive the list of activity completions.</param>
        /// <returns>A <c>Queryable</c> of LearningActivityCompletion for the specified <see cref="LearningActivity"/> identifier.</returns>
        public IQueryable<LearningActivityCompletion> GetActivityCompletions( int activityId )
        {
            var rockContext = ( RockContext ) Context;

            var activity = new LearningActivityService( rockContext ).Get( activityId );

            // Get all the activities for this class.
            var students = Queryable()
                .Include( p => p.Person )
                .Include( p => p.LearningClass )
                .Include( p => p.LearningClass.LearningSemester )
                .Where( p => p.LearningClassId == activity.LearningClassId )
                .ToList();

            // Get any of the completions that exist for this participant.
            var existingCompletions = Queryable()
                .AsNoTracking()
                .Include( c => c.LearningActivities )
                .Where( c => c.LearningClassId == activity.LearningClassId )
                .SelectMany( c => c.LearningActivities )
                .ToList();

            var completions = new List<LearningActivityCompletion>();

            foreach ( var student in students )
            {
                var existingCompletion = existingCompletions.FirstOrDefault( c => c.LearningActivityId == activity.Id );

                if ( existingCompletion != null )
                {
                    completions.Add( existingCompletion );
                }
                else
                {
                    // If a completion doesn't exist create it using default values.
                    completions.Add( LearningActivityCompletionService.GetNew( activity, student ) );
                }
            }

            // Return a list of completions specific to this participant.
            return completions.AsQueryable();
        }

        /// <summary>
        /// Gets a list of classes for the specified <see cref="Person"/>.
        /// </summary>
        /// <param name="personId">The identifier of the person for whom to get classes.</param>
        /// <param name="includePerson"><c>true</c> if the Person navigation property should be included; otherwise <c>false</c>.</param>
        /// <returns>A IQueryable of LearningParticipant records.</returns>
        public IQueryable<LearningParticipant> GetClasses( int personId, bool includePerson = false )
        {
            var baseQuery = Queryable()
                .Include( p => p.LearningClass )
                .Include( p => p.LearningClass.LearningCourse )
                .Where( p => p.PersonId == personId );

            return includePerson ? baseQuery.Include( p => p.Person ) : baseQuery;
        }

        /// <summary>
        /// Gets a list of facilitator <see cref="LearningActivityParticipantBag"/> for the specified
        /// <see cref="LearningClass"/> and (optionally) for one or more <see cref="Person" /> identifiers.
        /// </summary>
        /// <param name="classId">The identifier of the class to search within.</param>
        /// <param name="personIds">Optional list of Person identifiers to search for.</param>
        /// <returns>A list of <see cref="LearningActivityParticipantBag"/> that is the result of the search.</returns>
        public IEnumerable<LearningActivityParticipantBag> GetFacilitatorBags( int classId, params int[] personIds )
        {
            return GetParticipantBags( classId, true, personIds );
        }

        /// <summary>
        /// Gets the participantId of the facilitator in the specified class matching the provided <see cref="Person"/>
        /// identifier or <c>0</c> if not found.
        /// </summary>
        /// <param name="personId">The <see cref="Person"/> identifier of the participant to retrieve.</param>
        /// <param name="classId">The identifier of the <see cref="LearningClass"/> within which to search for the participant.</param>
        /// <returns></returns>
        public int GetFacilitatorId( int personId, int classId )
        {
            return GetParticipant( personId, classId )
                .Select( p => p.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets a list of <see cref="LearningParticipant">Facilitators</see> for a specified <see cref="LearningClass"/>
        /// </summary>
        /// <param name="classId">The identifier of the <see cref="LearningClass"/> to return facilitators for.</param>
        /// <returns>Queryable of LearningParticipants that have a Group role of IsLeader.</returns>
        public IQueryable<LearningParticipant> GetFacilitators( int classId )
        {
            return GetParticipants( classId ).Where( a => a.GroupRole.IsLeader );
        }

        /// <summary>
        /// Gets all facilitators for a specified <see cref="LearningCourse"/> and <see cref="LearningSemester"/>.
        /// </summary>
        /// <param name="courseId">The identifier of the course to get facilitators for.</param>
        /// <param name="semesterId">The identifier of the semester to get facilitators for.</param>
        /// <param name="includePerson"><c>true</c> to include the Person data.</param>
        /// <returns>An IQueryable of LearningParticipant containing only Facilitators for the specified course and semester.</returns>
        public IQueryable<LearningParticipant> GetFacilitators( int courseId, int semesterId, bool includePerson = true )
        {
            var baseQuery = Queryable()
                .Include( c => c.LearningClass )
                .Include( c => c.GroupRole )
                .Where( c => c.LearningClass.LearningCourseId == courseId )
                .Where( c => c.LearningClass.LearningSemesterId == semesterId )
                .Where( c => c.GroupRole.IsLeader == true );

            return includePerson ? baseQuery.Include( c => c.Person ) : baseQuery;
        }

        /// <summary>
        /// Gets the participant in the specified class matching the provided <see cref="LearningParticipant"/> Guid.
        /// </summary>
        /// <param name="participantGuid">The <see cref="Person"/> of the participant to retrieve.</param>
        /// <param name="classId">The identifier of the <see cref="LearningClass"/> within which to search for the participant.</param>
        /// <returns></returns>
        public IQueryable<LearningParticipant> GetParticipant( Guid participantGuid, int classId )
        {
            return Queryable()
                .Include( a => a.Person )
                .Include( a => a.GroupRole )
                .Where( p => p.LearningClassId == classId && p.Guid == participantGuid );
        }

        /// <summary>
        /// Gets the participant in the specified class matching the provided <see cref="Person"/> identifier.
        /// </summary>
        /// <param name="personId">The <see cref="Person"/> identifier of the participant to retrieve.</param>
        /// <param name="classId">The identifier of the <see cref="LearningClass"/> within which to search for the participant.</param>
        /// <returns></returns>
        public IQueryable<LearningParticipant> GetParticipant( int personId, int classId )
        {
            return Queryable()
                .Include( a => a.Person )
                .Include( a => a.GroupRole )
                .Where( p => p.LearningClassId == classId && p.PersonId == personId );
        }

        /// <summary>
        /// Gets a list of <see cref="LearningActivityParticipantBag"/> for the specified
        /// <see cref="LearningClass"/> and (optionally) for one or more <see cref="Person" /> identifiers.
        /// </summary>
        /// <param name="classId">The identifier of the class to search within.</param>
        /// <param name="facilitatorsOnly">When <c>true</c> only group members whose role is leader will be returned;
        ///     otherwise all participants are returned.</param>
        /// <param name="personIds">Optional list of Person identifiers to search for.</param>
        /// <returns>A list of <see cref="LearningActivityParticipantBag"/> that is the result of the search.</returns>
        public IEnumerable<LearningActivityParticipantBag> GetParticipantBags( int classId, bool facilitatorsOnly = false, params int[] personIds )
        {
            // Filter by PersonIds if any were provided. Otherwise return all participants.
            var baseQuery = personIds?.Any() == true ?
                GetParticipants( classId )
                    .Where( p => personIds.Contains( p.PersonId ) ) :
                GetParticipants( classId );

            // Optionally filter by facilitators only.
            if ( facilitatorsOnly )
            {
                baseQuery = baseQuery.Where( p => p.GroupRole.IsLeader );
            }

            return baseQuery
                .Select( p => new
                {
                    p.Id,
                    p.Person.Email,
                    IsFacilitator = p.GroupRole.IsLeader,
                    p.Person.NickName,
                    p.Person.LastName,
                    p.Person.SuffixValueId,
                    RoleName = p.GroupRole.Name,
                    p.Guid
                } )
                .ToList()
                .Select( p => new LearningActivityParticipantBag
                {
                    Email = p.Email,
                    Guid = p.Guid,
                    IdKey = Utility.IdHasher.Instance.GetHash( p.Id ),
                    IsFacilitator = p.IsFacilitator,
                    Name = Person.FormatFullName( p.NickName, p.LastName, p.SuffixValueId ),
                    RoleName = p.RoleName
                } );
        }

        /// <summary>
        /// Gets a list of <see cref="LearningParticipant">Participants</see> for a specified <see cref="LearningClass"/>
        /// </summary>
        /// <param name="classId">The identifier of the class for which to retreive participants.</param>
        /// <param name="includeGradingScales">Whether to include the list of <see cref="LearningGradingSystemScale"/> for the course.</param>
        /// <returns>Queryable of LearningParticipants.</returns>
        public IQueryable<LearningParticipant> GetParticipants( int classId, bool includeGradingScales = false )
        {
            return includeGradingScales ?
                 Queryable()
                    .AsNoTracking()
                    .Include( a => a.LearningGradingSystemScale )
                    .Include( a => a.Person )
                    .Include( a => a.GroupRole )
                    .Where( a => a.LearningClassId == classId ) :
                 Queryable()
                    .AsNoTracking()
                    .Include( a => a.Person )
                    .Include( a => a.GroupRole )
                    .Where( a => a.LearningClassId == classId );
        }

        /// <summary>
        /// Gets a list of <see cref="LearningParticipant">Students</see> for a specified <see cref="LearningClass"/>
        /// </summary>
        /// <param name="classId">The identifier of the <see cref="LearningClass"/> to return students for.</param>
        /// <returns>Queryable of LearningParticipants that have a Group role of not IsLeader.</returns>
        public IQueryable<LearningParticipant> GetStudents( int classId )
        {
            return GetParticipants( classId, true ).Where( a => !a.GroupRole.IsLeader );
        }

        /// <summary>
        /// Gets a list of <see cref="LearningActivity">LearningActivities</see> matching the specified <paramref name="classId">LearningClassId</paramref>.
        /// Includes the <see cref="LearningActivityCompletion">LearningActivityCompletions</see> the specified <paramref name="personId"/>.
        /// </summary>
        /// <param name="classId">The identifier of the <see cref="LearningClass"/> for which to retreive activities.</param>
        /// <param name="personId">The person for whom to include <see cref="LearningActivityCompletion"/> records.</param>
        /// <param name="learningActivityId">The identifier of the <see cref="LearningActivity"/> to get the completion for.</param>
        /// <returns>A <c>Queryable</c> of LearningActivity for the specified LearningClass identifier.</returns>
        public LearningActivityCompletion GetStudentActivity( int classId, int personId, int learningActivityId )
        {
            // Get the participant based on class and person identifiers.
            var participant = Queryable()
                .Include( p => p.LearningClass )
                .Include( p => p.LearningClass.LearningActivities )
                .Include( p => p.LearningClass.LearningSemester )
                .Include( p => p.LearningClass.LearningSemester.LearningProgram )
                .FirstOrDefault( p => p.PersonId == personId && p.LearningClassId == classId );

            // If the participant has a completion for this activity return it.
            var existingCompletion = participant.LearningActivities.FirstOrDefault( c => c.LearningActivityId == learningActivityId );
            if ( existingCompletion != null )
            {
                return existingCompletion;
            }

            // Otherwise get the default completion for this activity.
            return LearningActivityCompletionService.GetNew( new LearningActivityService( ( RockContext ) Context ).Get( learningActivityId ), participant );
        }

        /// <summary>
        /// Gets a list of <see cref="LearningActivityCompletion" /> for all activities in the class for the specified <paramref name="learningParticipantId" />.
        /// </summary>
        /// <param name="learningParticipantId">The identifier of the <see cref="LearningParticipant"/> for whom to get the learning plan.</param>
        /// <returns>A <c>Queryable</c> of LearningActivityCompletion for the specified LearningClass identifier.</returns>
        public List<LearningActivityCompletion> GetStudentLearningPlan( int learningParticipantId )
        {
            // Get the participant based on the LearningParticipant identifier.
            var participant = Queryable()
                .Include( p => p.LearningClass )
                .Include( p => p.LearningClass.LearningSemester )
                .Include( p => p.LearningClass.LearningSemester.LearningProgram )
                .FirstOrDefault( p => p.Id == learningParticipantId );

            return GetStudentLearningPlan( participant );
        }

        /// <summary>
        /// Gets a list of <see cref="LearningActivityCompletion" /> for all activities in the specified <paramref name="classId">LearningClassId</paramref>.
        /// Includes the <see cref="LearningActivityCompletion">LearningActivityCompletions</see> the specified <paramref name="personId"/>.
        /// </summary>
        /// <param name="classId">The identifier of the <see cref="LearningClass"/> for which to retreive activities.</param>
        /// <param name="personId">The person for whom to include <see cref="LearningActivityCompletion"/> records.</param>
        /// <returns>A <c>Queryable</c> of LearningActivityCompletion for the specified LearningClass identifier.</returns>
        public List<LearningActivityCompletion> GetStudentLearningPlan( int classId, int personId )
        {
            // Get the participant based on class and person identifiers.
            var participant = Queryable()
                .Include( p => p.LearningClass )
                .Include( p => p.LearningClass.LearningSemester )
                .Include( p => p.LearningClass.LearningSemester.LearningProgram )
                .FirstOrDefault( p => p.PersonId == personId && p.LearningClassId == classId );

            return GetStudentLearningPlan( participant );
        }

        /// <summary>
        /// Gets a list of <see cref="LearningActivityCompletion" /> for all activities in the class for the specified <paramref name="student" />.
        /// If a the <see cref="LearningParticipant"/> doesn't have a persisted <see cref="LearningActivityCompletion" /> then a new instance is generated 
        /// (but not tracked) with default values.
        /// </summary>
        /// <param name="student">The<see cref="LearningParticipant"/> for which to retreive activities.</param>
        /// <returns>A <c>Queryable</c> of LearningActivityCompletions - one for each <see cref="LearningActivity"/> in the class.</returns>
        private List<LearningActivityCompletion> GetStudentLearningPlan( LearningParticipant student )
        {
            // Get all the activities for this class.
            var activities = Queryable()
                .Include( p => p.LearningClass )
                .Where( p => p.Id == student.Id )
                .SelectMany( p => p.LearningClass.LearningActivities )
                .ToList();

            // Get any of the completions that exist for this participant.
            var existingCompletions = Queryable()
                .Include( c => c.LearningActivities )
                .Where( c => c.Id == student.Id )
                .SelectMany( c => c.LearningActivities )
                .ToList();

            var completions = new List<LearningActivityCompletion>();

            foreach ( var activity in activities )
            {
                var existingCompletion = existingCompletions.FirstOrDefault( c => c.LearningActivityId == activity.Id );

                if ( existingCompletion != null )
                {
                    completions.Add( existingCompletion );
                }
                else
                {
                    // If a completion doesn't exist create it using default values.
                    completions.Add( LearningActivityCompletionService.GetNew( activity, student) );
                }
            }

            // Return a list of completions specific to this participant.
            return completions
                .OrderBy( a => a.LearningActivity.Order )
                .ThenBy( a => a.LearningActivityId )
                .ToList();
        }

    }
}
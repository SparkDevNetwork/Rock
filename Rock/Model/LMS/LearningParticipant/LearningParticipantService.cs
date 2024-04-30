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
using System.Data.Entity;
using System.Linq;
using Rock.Data;
using Rock.Enums.Lms;
using Rock.Utility;

namespace Rock.Model
{
    public partial class LearningParticipantService
    {
        /// <summary>
        /// Gets the <see cref="Rock.Model.LearningParticipant">participants</see> for the class idKey specified.
        /// </summary>
        /// <param name="idKey">The idKey of the <see cref="LearningClass">class</see> for which to retrieve participants.</param>
        /// <returns>A queryable of the class participants.</returns>
        public IQueryable<LearningParticipant> GetClassParticipants( string idKey )
        {
            var id = IdHasher.Instance.GetId( idKey ).ToIntSafe();
            return id > 0 ? Enumerable.Empty<LearningParticipant>().AsQueryable() : GetClassParticipants( id );
        }

        /// <summary>
        /// Gets the <see cref="Rock.Model.LearningParticipant">participants</see> for the classId specified.
        /// </summary>
        /// <param name="classId">The id of the <see cref="LearningClass">class</see> for which to retrieve participants.</param>
        /// <returns>Queryable of the class participants.</returns>
        public IQueryable<LearningParticipant> GetClassParticipants( int classId )
        {
            return Queryable().Where( p => p.LearningClassId == classId );
        }

        /// <summary>
        /// Gets a list of <see cref="LearningParticipant">Students</see> for a specified <see cref="LearningClass"/>
        /// </summary>
        /// <param name="classId">The identifier of the <see cref="LearningClass"/> to return students for.</param>
        /// <returns>Queryable of LearningParticipants that have a Group role of not IsLeader.</returns>
        public IQueryable<LearningParticipant> GetStudents( int classId )
        {
            return GetParticipants( a => !a.GroupRole.IsLeader && a.LearningClassId == classId, true );
        }

        /// <summary>
        /// Gets a list of <see cref="LearningParticipant">Facilitators</see> for a specified <see cref="LearningClass"/>
        /// </summary>
        /// <param name="classId">The identifier of the <see cref="LearningClass"/> to return facilitators for.</param>
        /// <returns>Queryable of LearningParticipants that have a Group role of IsLeader.</returns>
        public IQueryable<LearningParticipant> GetFacilitators(int classId)
        {
            return GetParticipants( a => a.GroupRole.IsLeader && a.LearningClassId == classId );
        }

        /// <summary>
        /// Gets a list of <see cref="LearningParticipant">Participants</see> for a specified <see cref="LearningClass"/>
        /// </summary>
        /// <param name="filterPredicate">The predicate by which to filter the <see cref="LearningParticipant"/>.</param>
        /// <param name="includeGradingScales">Whether to include the list of <see cref="LearningGradingSystemScale"/> for the course.</param>
        /// <returns>Queryable of LearningParticipants.</returns>
        public IQueryable<LearningParticipant> GetParticipants(Func<LearningParticipant, bool> filterPredicate, bool includeGradingScales = false )
        {
            return includeGradingScales ?
                 Queryable()
                    .Include( a => a.LearningGradingSystemScale )
                    .Include( a => a.Person )
                    .Include( a => a.GroupRole )
                    .Where( filterPredicate )
                    .AsQueryable() :
                 Queryable()
                    .Include( a => a.Person )
                    .Include( a => a.GroupRole )
                    .Where( filterPredicate )
                    .AsQueryable();
        }

        /// <summary>
        /// <para>Gets a list of <see cref="LearningActivity">Activities</see> for the course that the <see cref="LearningParticipant"/>
        /// is enrolled in.</para>
        /// These are the activity templates. For instance data use <seealso cref="GetActivityCompletions"/>.
        /// This method will return only assignments for the role type (e.g. Student assignments when the Participant role type is not a leader).
        /// </summary>
        /// <param name="participantId">The Id of the <see cref="LearningParticipant"/> for which to get activities.</param>
        /// <returns>Queryable of LearningActivities for the given participant and class.</returns>
        public IQueryable<LearningActivity> GetActivities( int participantId )
        {
            return Queryable()
                .Include( p => p.LearningClass )
                .Include( p => p.LearningClass.LearningActivities )
                .Include( p => p.GroupRole )
                .Where( p => p.Id == participantId )
                .SelectMany( p =>
                    p.LearningClass.LearningActivities
                    .Where( a => a.AssignTo == ( p.GroupRole.IsLeader ? AssignTo.Facilitator : AssignTo.Student ) )
                );
        }

        /// <summary>
        /// <para>Gets a list of <see cref="LearningActivityCompletion">Activities</see> for the course that the <see cref="LearningParticipant"/>
        /// is enrolled in.</para>
        /// These are the activity instances. For template data use <seealso cref="GetActivities"/>.
        /// </summary>
        /// <param name="participantId">The Id of the <see cref="LearningParticipant"/> for which to get activities.</param>
        /// <returns>Queryable of LearningActivitCompletions for the given participant and class.</returns>
        public IQueryable<LearningActivityCompletion> GetActivityCompletions( int participantId )
        {
            return Queryable()
                .Include( p => p.LearningActivities )
                .Where( p => p.Id == participantId )
                .SelectMany( p => p.LearningActivities );
        }

        /// <summary>
        /// Adds <see cref="LearningActivityCompletion">activity completions</see> for the <see cref="LearningParticipant">participant</see> .
        /// If any activity completion already exists it will be not be recreated.
        /// </summary>
        /// <param name="learningParticipantId">The identifier of the <see cref="LearningParticipant"/> for which to add the completion records.</param>
        /// <returns>The list of <see cref="LearningActivityCompletion"/> records to be saved when the Context is saved.</returns>
        public List<LearningActivityCompletion> AddActivityCompletions( int learningParticipantId )
        {
            var rockContext = ( RockContext ) Context;

            var completionsToAdd = GetActivities( learningParticipantId )
                .AsNoTracking()
                // If there happen to be some existing completions do not recreate them.
                .Where( a => !a.LearningActivityCompletions.Any( c => c.StudentId == learningParticipantId && c.LearningActivityId == a.Id ) )
                .Select( a => new LearningActivityCompletion
                {
                    StudentId = learningParticipantId,
                    LearningActivityId = a.Id,
                    AvailableDate = a.AvailableDateCalculated,
                    DueDate = a.DueDateCalculated,
                    NotificationCommunicationId = a.LearningClass.LearningCourse.SystemCommunicationId
                } )
                .ToList();

            new LearningActivityCompletionService( rockContext ).AddRange( completionsToAdd );

            return completionsToAdd;
        }
    }
}
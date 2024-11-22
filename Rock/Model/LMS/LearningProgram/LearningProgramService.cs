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
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Enums.Lms;
using Rock.Utility;

namespace Rock.Model
{
    public partial class LearningProgramService
    {
        /// <summary>
        /// Deletes the <see cref="LearningProgram"/> for the specified <paramref name="programId"/>.
        /// Includes deleting related data like <see cref="LearningActivity"/>,
        /// <see cref="LearningClassAnnouncement"/>, <see cref="LearningClassContentPage"/>
        /// and <see cref="LearningParticipant"/> records.
        /// </summary>
        /// <param name="programId">The identifier of the <see cref="LearningProgram"/> to delete.</param>
        public void Delete( int programId )
        {
            var rockContext = ( RockContext ) Context;
            rockContext.WrapTransaction( () =>
            {
                var learningClassService = new LearningClassService( rockContext );
                var classes = learningClassService
                    .Queryable()
                    .Include( c => c.LearningActivities )
                    .Include( c => c.LearningParticipants )
                    .Include( c => c.ContentPages )
                    .Include( c => c.Announcements )
                    .Include( c => c.LearningCourse )
                    .Include( c => c.LearningCourse.LearningCourseRequirements )
                    .Where( c => c.LearningCourse.LearningProgramId == programId );

                learningClassService.DeleteRange( classes );

                var program = Queryable()
                    .Include( p => p.LearningCourses)
                    .Include( p => p.LearningSemesters )
                    .Include( p => p.LearningProgramCompletions )
                    .FirstOrDefault( p => p.Id == programId );

                base.Delete( program );
            } );
        }

        /// <summary>
        /// Get a list of <see cref="LearningSemester"/> for the specified Learning Program.
        /// </summary>
        /// <param name="learningProgramId">The </param>
        /// <returns></returns>
        public IQueryable<LearningSemester> GetSemesters( int learningProgramId )
        {
            return Queryable()
                .Where( p => p.Id == learningProgramId )
                .Include( p => p.LearningSemesters )
                .SelectMany( p => p.LearningSemesters );
        }

        /// <summary>
        /// Gets the default <see cref="LearningSemester"/> for the specified Learning Program.
        /// </summary>
        /// <param name="learningProgramId">The identifier of the <see cref="LearningProgram"/> to get the default semester for.</param>
        /// <returns></returns>
        public LearningSemester GetDefaultSemester( int learningProgramId )
        {
            var now = RockDateTime.Now;
            return learningProgramId > 0 ? Queryable()
                .Where( p => p.Id == learningProgramId )
                .Include( p => p.LearningSemesters )
                .Select( p => p.LearningSemesters.FirstOrDefault( s=> !s.EndDate.HasValue || s.EndDate >= now ) )
                .FirstOrDefault() :
                default;
        }

        /// <summary>
        /// Gets a list of several Key Performance Indicators for the specified Learning Program.
        /// </summary>
        /// <param name="learningProgramId">The identifier of the learning program for which to get the KPIs.</param>
        /// <returns>An object containing the KPI values.</returns>
        public LearningProgramKpis GetProgramKpis( int learningProgramId )
        {
            var context = ( RockContext ) Context;
            var now = RockDateTime.Now;

            // Get the class and student data in aggregate together.
            var classAndStudentData = new LearningClassService( context )
                .Queryable()
                .AsNoTracking()
                .Where( c => c.IsActive )
                .Where( c => c.LearningCourse.LearningProgramId == learningProgramId )
                .Where( c => ( !c.LearningSemester.EndDate.HasValue || c.LearningSemester.EndDate >= now ) )
                .Where( c => ( !c.LearningSemester.StartDate.HasValue || c.LearningSemester.StartDate <= now ) )
                .Select( c => new
                {
                    ClassId = c.Id,
                    PersonIds = c.LearningParticipants
                        .Where( p => !p.GroupRole.IsLeader )
                        .Where( p => p.LearningCompletionStatus == LearningCompletionStatus.Incomplete )
                        .Select( s => s.PersonId )
                } ).ToList();

            var completions = new LearningProgramCompletionService( context )
                .Queryable()
                .AsNoTracking()
                .Count( p => p.LearningProgramId == learningProgramId && p.CompletionStatus == CompletionStatus.Completed );

            return new LearningProgramKpis
            {
                ActiveClasses = classAndStudentData.Select( c => c.ClassId ).Distinct().Count(),
                ActiveStudents = classAndStudentData.SelectMany( c => c.PersonIds ).Distinct().Count(),
                Completions = completions
            };
        }

        /// <summary>
        /// Gets a list of active, public programs, optionally filtered to the specified categoryIds and optionally with completion status for the specified person.
        /// </summary>
        /// <param name="includeCompletionsForPersonId">The identifier of the <see cref="Person"/> to include completion status for.</param>
        /// <param name="publicOnly"><c>true</c> to include <see cref="LearningProgram"/> records whose IsPublic property is true; <c>false</c> to include regardless of IsPublic.</param>
        /// <param name="categoryGuids">The optional list of category Guids to filter for.</param>
        /// <returns>An enumerable of PublicLearningProgramBag.</returns>
        public IQueryable<PublicLearningProgramBag> GetPublicPrograms( int includeCompletionsForPersonId = 0, bool publicOnly = true, params Guid[] categoryGuids )
        {
            var baseQuery = Queryable()
                .AsNoTracking()
                .Include( p => p.ImageBinaryFile )
                .Include( p => p.Category )
                .Where( p =>
                    p.IsActive
                    && ( p.IsPublic || !publicOnly ) );

            if ( categoryGuids.Any() )
            {
                baseQuery = baseQuery.Where( p => p.Category != null && categoryGuids.Contains( p.Category.Guid ) );
            }

            if ( includeCompletionsForPersonId > 0 )
            {
                // If we should include completion status then get those values first and return the program bag queryable.
                var personCompletions = new LearningProgramCompletionService( ( RockContext ) Context )
                .Queryable()
                .AsNoTracking()
                .Include( c => c.PersonAlias )
                .Where( c => c.PersonAlias.PersonId == includeCompletionsForPersonId ) ?? default;

                return baseQuery
                    .Select( p => new PublicLearningProgramBag
                    {
                        Entity = p,
                        Category = p.Category.Name,
                        CategoryColor = p.Category.HighlightColor,
                        CompletionStatus = personCompletions
                            .FirstOrDefault( c => c.LearningProgramId == p.Id )
                            .CompletionStatus,
                        ImageFileGuid = p.ImageBinaryFile.Guid
                    } );
            }
            else
            {
                // If we don't need to include completion status return the program bag queryable.
                return baseQuery.Select( p => new PublicLearningProgramBag
                {
                    Entity = p,
                    Category = p.Category.Name,
                    CategoryColor = p.Category.HighlightColor,
                    ImageFileGuid = p.ImageBinaryFile.Guid
                } );
            }
        }

        #region Nested Classes

        /// <summary>
        /// Represents the Lava enabled data sent to the public programs list block.
        /// </summary>
        public class PublicLearningProgramBag : RockDynamic
        {
            /// <summary>
            /// Gets or sets the Learning Program entity for this bag.
            /// </summary>
            public LearningProgram Entity { get; set; }

            /// <summary>
            /// Gets or sets the category.
            /// </summary>
            public string Category { get; set; }

            /// <summary>
            /// Gets or sets the highlight color of the category.
            /// </summary>
            public string CategoryColor { get; set; }

            /// <summary>
            /// Gets or sets the completion status of the program for the current person.
            /// </summary>
            public CompletionStatus? CompletionStatus { get; set; }

            /// <summary>
            /// Gets or sets the link to the course details.
            /// </summary>
            public string CoursesLink { get; set; }

            /// <summary>
            /// Gets or sets the Guid for the Image file of this Program.
            /// </summary>
            public Guid? ImageFileGuid { get; set; }
        }

        #endregion
    }
}
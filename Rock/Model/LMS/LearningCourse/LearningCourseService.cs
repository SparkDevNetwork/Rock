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
    public partial class LearningCourseService
    {
        /// <summary>
        /// Gets a list of active, public courses for the specified program.
        /// </summary>
        /// <param name="programId">The identifier of the <see cref="LearningProgram"/> for which to return courses.</param>
        /// <param name="forPersonId">The identifier of the <see cref="Person"/> to include completion status for.</param>
        /// <returns>An enumerable of PublicLearningCourseBag.</returns>
        public List<PublicLearningCourseBag> GetPublicCourses( int programId, int forPersonId )
        {
            var rockContext = ( RockContext ) Context;
            var orderedPersonCompletions = new LearningParticipantService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Include( p => p.LearningClass )
                .Include( p => p.LearningClass.LearningCourse )
                .Where( p => p.PersonId == forPersonId )
                // If the student has taken the class multiple times take in this order:
                // 'Pass' - 'Incomplete' - 'Fail'.
                .OrderBy( p =>
                            LearningCompletionStatus.Pass == p.LearningCompletionStatus ? 0 :
                            LearningCompletionStatus.Incomplete == p.LearningCompletionStatus ? 1 :
                            2 // Fail
                        );

            //  Get all Semesters for the program.
            //  Include the classes for joining to the Course.
            var semesters = new LearningSemesterService( rockContext )
                .Queryable()
                .Include( s => s.LearningClasses )
                .Where( s => s.LearningProgramId == programId );

            var now = RockDateTime.Now;
            var results = Queryable()
                .Include( c => c.ImageBinaryFile )
                .Include( c => c.LearningProgram )
                .Include( c => c.Category )
                .Include( c => c.LearningCourseRequirements )
                .Where( c => c.IsActive && c.IsPublic && c.LearningProgramId == programId )
                .Select( c => new PublicLearningCourseBag
                {
                    Entity = c,
                    Category = c.Category.Name,
                    CategoryColor = c.Category.HighlightColor,

                    // Get the person's completion status for this course.
                    LearningCompletionStatus = orderedPersonCompletions
                        .FirstOrDefault( p => p.LearningClass.LearningCourseId == c.Id )
                        .LearningCompletionStatus,
                    ImageFileGuid = c.ImageBinaryFile.Guid,

                    // Get the nearest semester with open enrollment and a future start date for this course.
                    NextSemester = semesters.FirstOrDefault( s =>
                        (s.EnrollmentCloseDate == null || s.EnrollmentCloseDate >= now) &&
                        s.StartDate >= now &&
                        s.LearningClasses.Any( sc => sc.LearningCourseId == c.Id )
                        ),

                    // Only Prerequisites where the course completions for the student aren't 'Passed'.
                    UnmetPrerequisites = c.LearningCourseRequirements
                        .Where(r =>
                            !orderedPersonCompletions.Any( comp =>
                                comp.LearningCompletionStatus == LearningCompletionStatus.Pass &&
                                comp.LearningClass.LearningCourseId == r.RequiredLearningCourseId &&
                                r.RequirementType == RequirementType.Prerequisite))
                        .ToList()
                } )
                .ToList()
                // Sort in memory (after calling ToList).
                .OrderBy( c => c.Entity.Order)
                .ThenBy( c => c.Entity.Id );

            var requiredCourseIds = results.SelectMany( c => c.UnmetPrerequisites.Select( r => r.RequiredLearningCourseId ) );
            var requiredCourses = Queryable().Where( c => requiredCourseIds.Contains( c.Id ) );

            return results.ToList();
        }

        #region Nested Classes

        /// <summary>
        /// Represents the Lava enabled data sent to the public programs list block.
        /// </summary>
        public class PublicLearningCourseBag : RockDynamic
        {
            /// <summary>
            /// Gets or sets the Learning Program entity for this bag.
            /// </summary>
            public LearningCourse Entity { get; set; }

            /// <summary>
            /// Gets or sets the category.
            /// </summary>
            public string Category { get; set; }

            /// <summary>
            /// Gets or sets the highlight color of the category.
            /// </summary>
            public string CategoryColor { get; set; }

            /// <summary>
            /// Gets or sets the completion status of the course for the current person.
            /// </summary>
            public LearningCompletionStatus? LearningCompletionStatus { get; set; }

            /// <summary>
            /// Gets or sets the link to the course details.
            /// </summary>
            public string CourseDetailsLink { get; set; }

            /// <summary>
            /// Gets or sets the link to enroll in the course.
            /// </summary>
            public string CourseEnrollmentLink { get; set; }

            /// <summary>
            /// Gets or sets the Guid for the Image file of this Program.
            /// </summary>
            public Guid? ImageFileGuid { get; set; }

            /// <summary>
            /// Gets or sets the next semester where a class is available for the course
            /// and the enrollment close date is in the future or null.
            /// </summary>
            public LearningSemester NextSemester { get; set; }

            /// <summary>
            /// Gets or sets the link for the prerequisite course if any.
            /// </summary>
            public string PrerequisiteEnrollmentLink { get; set; }

            /// <summary>
            /// Gets or sets a list of LearningCourseRequirements where the Person hasn't yet Passed the Prerequisite.
            /// </summary>
            public List<LearningCourseRequirement> UnmetPrerequisites { get; set; }
        }

        #endregion
    }
}
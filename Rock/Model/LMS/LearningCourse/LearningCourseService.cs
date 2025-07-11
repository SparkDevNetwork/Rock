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
using Rock.Lava;
using Rock.Utility;
using Rock.ViewModels.Blocks.Lms.LearningCourseRequirement;

namespace Rock.Model
{
    public partial class LearningCourseService
    {
        /// <summary>
        /// Get a list of required courses for the specified course. 
        /// </summary>
        /// <param name="courseId">The identifier of the <see cref="LearningCourse"/> to get requirements for.</param>
        /// <returns>A list of <see cref="LearningCourseRequirementBag"/> records.</returns>
        public LearningCourse GetCourseWithRequirements( int courseId )
        {
            var course = Queryable()
                .Include( a => a.LearningProgram )
                .Include( a => a.LearningClasses )
                .Include( a => a.LearningCourseRequirements.Select( cr => cr.RequiredLearningCourse ) )
                .FirstOrDefault( a => a.Id == courseId );

            return course;
        }

        /// <summary>
        /// Gets the details for a public course.
        /// </summary>
        /// <param name="courseId">The identifier of the course to get.</param>
        /// <param name="person">The <see cref="Person"/> to include completion status for.</param>
        /// <param name="publicOnly"><c>true</c> to include <see cref="LearningClass"/> records whose IsPublic property is true; <c>false</c> to include regardless of IsPublic.</param>
        /// <param name="semesterStartFrom">Optional filter for the next session Semester Start. Only Start Dates greater than this date will be included.</param>
        /// <param name="semesterStartTo">Optional filter for the next session Semester Start. Only Start Dates less than this date will be included.</param>
        /// <returns>A <see cref="PublicLearningCourseBag"/> containing the data necessary for rendering the course details.</returns>
        public PublicLearningCourseBag GetPublicCourseDetails( int courseId, Person person, bool publicOnly = true, DateTime? semesterStartFrom = null, DateTime? semesterStartTo = null )
        {
            var rockContext = ( RockContext ) Context;
            var participantService = new LearningParticipantService( rockContext );
            var hasPersonId = person?.Id.ToIntSafe() > 0;
            var now = RockDateTime.Now;
            var studentRoleGuid = SystemGuid.GroupRole.GROUPROLE_LMS_CLASS_STUDENT.AsGuid();
            var facilitatorRoleGuid = SystemGuid.GroupRole.GROUPROLE_LMS_CLASS_FACILITATOR.AsGuid();

            var orderedPersonCompletions = !hasPersonId
                ? new List<LearningParticipant>().AsQueryable().Where( lp => false )
                : new LearningParticipantService( rockContext )
                    .GetClassesForStudent( person.Id )
                    .AsNoTracking()
                    // If the student has taken the class multiple times take in this order:
                    // 'Pass' - 'Incomplete' - 'Fail'.
                    .OrderBy( p =>
                                LearningCompletionStatus.Pass == p.LearningCompletionStatus ? 0 :
                                LearningCompletionStatus.Incomplete == p.LearningCompletionStatus ? 1 :
                                2 // Fail
                            );

            var course = Queryable()
                .AsNoTracking()
                .Include( c => c.ImageBinaryFile )
                .Include( c => c.LearningProgram )
                .Include( c => c.LearningProgram.ImageBinaryFile )
                .Include( c => c.Category )
                .Include( c => c.LearningCourseRequirements )
                .Include( c => c.LearningClasses )
                .Include( c => c.LearningClasses.Select( lc => lc.LearningGradingSystem.LearningGradingSystemScales ) )
                .Where( c => c.IsActive && c.Id == courseId )
                .Where( c => !publicOnly || c.IsPublic )
                .Select( c => new PublicLearningCourseBag
                {
                    AllowHistoricalAccess = c.AllowHistoricalAccess,
                    Category = c.Category.Name,
                    CategoryColor = c.Category.HighlightColor,
                    CourseCode = c.CourseCode,
                    CourseRequirements = c.LearningCourseRequirements
                        .Where( lcr => lcr.RequirementType != RequirementType.Equivalent)
                        .ToList(),
                    Credits = c.Credits,
                    Description = c.Description,
                    Id = c.Id,
                    ImageFileGuid = c.ImageBinaryFile.Guid,
                    IsEnrolled = hasPersonId
                        && orderedPersonCompletions.Any( lp => lp.LearningClass.LearningCourseId == c.Id ),

                    // Get the person's completion status for this course.
                    LearningCompletionStatus = !hasPersonId ?
                        null :
                        ( LearningCompletionStatus? ) orderedPersonCompletions
                        .FirstOrDefault( p => p.LearningClass.LearningCourseId == c.Id )
                        .LearningCompletionStatus,
                    IsCompletionOnly = !c.LearningClasses.Any( lc =>
                        lc.LearningGradingSystem.LearningGradingSystemScales
                            .Select( s => s.Id )
                            .Count() > 1
                    ),
                    ProgramInfo = new LearningProgramService.PublicLearningProgramBag
                    {
                        Id = c.LearningProgramId,
                        PublicName = c.LearningProgram.PublicName,
                        Summary = c.LearningProgram.Summary,
                        Category = c.LearningProgram.Category.Name,
                        CategoryColor = c.LearningProgram.Category.HighlightColor,
                        ConfigurationMode = c.LearningProgram.ConfigurationMode,
                        ImageFileGuid = c.LearningProgram.ImageBinaryFile.Guid
                    },
                    PublicName = c.PublicName,
                    Order = c.Order,
                    Summary = c.Summary
                } )
                .FirstOrDefault();

            if ( course == null )
            {
                // This can happen if a course is still being configured.
                return default;
            }

            course.IdKey = IdHasher.Instance.GetHash( course.Id );
            course.ProgramInfo.IdKey = IdHasher.Instance.GetHash( course.ProgramInfo.Id );

            // Get the available classes
            // Include only the active classes (and optionally public only) 
            // Include courses that have exceeded their max student count
            // (in case the Lava Template wants to specially handle requests to be added).
            var classesQuery = new LearningClassService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Include( c => c.LearningSemester )
                .Include( c => c.GroupLocations )
                .Include( c => c.LearningGradingSystem )
                .Include( c => c.Schedule )
                .Include( c => c.Campus )
                .Where( c => c.LearningCourseId == courseId )
                .Where( c => c.IsActive && ( !publicOnly || c.IsPublic ) );

            /*
	            12/12/2024 - JC

	            If the individual is currently enrolled,
                but enrollment is closed - still include
                their class as they'd have no way to access their course-work.

	            Reason: Maintain access to enrolled courses after enrollment closes.
            */
            // If the individual is enrolled include their class;
            // otherwise restrict to only classes that are still available for enrollment.
            if ( hasPersonId )
            {
                classesQuery = classesQuery.Where( c =>
                    !c.LearningSemester.EnrollmentCloseDate.HasValue
                    || c.LearningSemester.EnrollmentCloseDate >= now
                    || c.LearningParticipants.Any( p => p.PersonId == person.Id ) );
            }
            else
            {
                classesQuery = classesQuery.Where( c =>
                    !c.LearningSemester.EnrollmentCloseDate.HasValue
                    || c.LearningSemester.EnrollmentCloseDate >= now );
            }

            if ( semesterStartFrom.HasValue )
            {
                // Filter by semester Start Date OR if they are enrolled.
                classesQuery = classesQuery
                    .Where( c => c.LearningSemester.StartDate.HasValue && c.LearningSemester.StartDate >= semesterStartFrom.Value
                    || c.LearningParticipants.Any( p => p.PersonId == person.Id ) );
            }

            if ( semesterStartTo.HasValue )
            {
                // Filter by semester Start Date OR if they are enrolled.
                classesQuery = classesQuery
                    .Where( c => c.LearningSemester.StartDate.HasValue && c.LearningSemester.StartDate <= semesterStartTo.Value
                    || c.LearningParticipants.Any( p => p.PersonId == person.Id ) );
            }

            var classes = classesQuery.ToList();

            // Get the distinct Semesters for the course and project them into
            // a new PublicLearningSemesterBag ordered by semester start date.
            course.Semesters = classes
                .Where( c => c.LearningSemesterId.HasValue )
                .Select( c => c.LearningSemester )
                .DistinctBy( s => s.Id )
                .ToList()
                .OrderBy( s => s.StartDate )
                .Select( s => new PublicLearningSemesterBag
                {
                    Id = s.Id,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    EnrollmentCloseDate = s.EnrollmentCloseDate,
                    Name = s.Name,
                    AvailableClasses = classes
                        .Where( c => c.LearningSemesterId == s.Id )
                        .Select( c => new PublicLearningClassBag
                        {
                            Campus = c.Campus,
                            GradingSystem = c.LearningGradingSystem,
                            Id = c.Id,
                            Name = c.Name,
                            GroupLocation = c.GroupLocations.FirstOrDefault(),
                            Order = c.Order,
                            Schedule = c.Schedule
                        } )
                        .ToList()
                } )
                .ToList();

            if ( hasPersonId )
            {
                course.UnmetPrerequisites = GetUnmetCourseRequirements( person.Id, course.CourseRequirements );
            }

            // Get all the facilitators and this person's participant records in one query.
            var classIds = classes.Select( c => c.Id );
            var facilitatorsAndPerson = participantService
                .Queryable()
                .Include( p => p.LearningGradingSystemScale )
                .Where( p => classIds.Contains( p.LearningClassId ) )
                .AreFacilitatorsOrPerson( person?.Id );

            foreach ( var semester in course?.Semesters )
            {
                semester.IdKey = IdHasher.Instance.GetHash( semester.Id );

                foreach ( var availableClass in semester.AvailableClasses )
                {
                    var classEntity = classes.FirstOrDefault( c => c.Id == availableClass.Id );
                    availableClass.IdKey = classEntity.IdKey;

                    if ( hasPersonId )
                    {
                        availableClass.CanEnroll = participantService.CanEnroll( classEntity, person, course.UnmetPrerequisites, out var errorKey );
                        availableClass.EnrollmentErrorKey = errorKey;

                        // Set the IsEnrolled flags for both the semester and class.
                        var isEnrolledInClass = errorKey.Equals( "already_enrolled", StringComparison.OrdinalIgnoreCase );
                        availableClass.IsEnrolled = isEnrolledInClass;
                        semester.IsEnrolled = semester.IsEnrolled || isEnrolledInClass;

                        availableClass.StudentParticipant = facilitatorsAndPerson
                            .AreStudents()
                            .FirstOrDefault( p => p.LearningClassId == classEntity.Id && p.PersonId == person.Id );
                    }

                    var facilitatorPersonIds = facilitatorsAndPerson
                        .AreFacilitators()
                        .Where( p => p.LearningClassId == classEntity.Id )
                        .Select( p => p.PersonId )
                        .ToArray();

                    availableClass.Facilitators = participantService.GetFacilitatorBags( classEntity.Id, facilitatorPersonIds )
                        .Select( p => new PublicFacilitatorBag
                        {
                            ParticipantIdKey = p.IdKey,
                            ParticipantId = IdHasher.Instance.GetId( p.IdKey ).ToIntSafe(),
                            Name = p.Name,
                            Role = p.RoleName,
                            Email = p.Email
                        } )
                        .ToList();
                }

                // By default sort the classes by the campus name.
                semester.AvailableClasses = semester.AvailableClasses.OrderBy( c => c.Order ).ToList();
            }

            return course;
        }

        /// <summary>
        /// Gets a list of active, public courses for the specified program.
        /// </summary>
        /// <param name="programId">The identifier of the <see cref="LearningProgram"/> for which to return courses.</param>
        /// <param name="personId">The identifier of the <see cref="Person"/> to include completion status for.</param>
        /// <param name="publicOnly"><c>true</c> to include <see cref="LearningCourse"/> and <see cref="LearningClass"/> records whose IsPublic property is true; <c>false</c> to include regardless of IsPublic.</param>
        /// <param name="semesterStartFrom">Optional filter for the next session Semester Start. Only Start Dates greater than this date will be included.</param>
        /// <param name="semesterStartTo">Optional filter for the next session Semester Start. Only Start Dates less than this date will be included.</param>
        /// <returns>An enumerable of PublicLearningCourseBag.</returns>
        public List<PublicLearningCourseBag> GetPublicCourses( int programId, int? personId, bool publicOnly = true, DateTime? semesterStartFrom = null, DateTime? semesterStartTo = null )
        {
            var now = RockDateTime.Now;
            var rockContext = ( RockContext ) Context;
            var studentRoleGuid = SystemGuid.GroupRole.GROUPROLE_LMS_CLASS_STUDENT.AsGuid();
            var facilitatorRoleGuid = SystemGuid.GroupRole.GROUPROLE_LMS_CLASS_FACILITATOR.AsGuid();
            var orderedPersonCompletions = !personId.HasValue
                ? new List<LearningParticipant>().AsQueryable().Where( lp => false )
                : new LearningParticipantService( rockContext )
                    .GetClassesForStudent( personId.Value )
                    .AsNoTracking()
                    // If the student has taken the class multiple times take in this order:
                    // 'Pass' - 'Incomplete' - 'Fail'.
                    .OrderBy( p =>
                                LearningCompletionStatus.Pass == p.LearningCompletionStatus ? 0 :
                                LearningCompletionStatus.Incomplete == p.LearningCompletionStatus ? 1 :
                                2 // Fail
                            );

            // Get all Semesters for the program.
            // Include the active (optionally public only) classes for joining to the Course.
            // Include courses that have exceeded their max student count
            // (in case the Lava Template wants to specially handle requests to be added).
            var semesters = new LearningClassService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Include( c => c.LearningSemester )
                .Include( c => c.LearningSemester.LearningClasses )
                .Where( c => c.LearningSemester.LearningProgramId == programId )
                .Where( c => ( c.IsPublic || !publicOnly ) && c.IsActive )
                .Select( c => c.LearningSemester )
                .Where( s => s.LearningProgramId == programId );

            if ( semesterStartFrom.HasValue )
            {
                semesters = semesters
                    .Where( s => s.StartDate.HasValue && s.StartDate >= semesterStartFrom.Value );
            }

            if ( semesterStartTo.HasValue )
            {
                semesters = semesters
                   .Where( s => s.StartDate.HasValue && s.StartDate <= semesterStartTo.Value );
            }

            var unmetPrerequisiteTypes = new List<RequirementType> { RequirementType.Prerequisite, RequirementType.Equivalent };

            var courses = Queryable()
                .AsNoTracking()
                .Include( c => c.ImageBinaryFile )
                .Include( c => c.LearningProgram )
                .Include( c => c.LearningProgram.Category )
                .Include( c => c.LearningProgram.ImageBinaryFile )
                .Include( c => c.LearningClasses )
                .Include( c => c.LearningClasses.Select( lc => lc.LearningGradingSystem.LearningGradingSystemScales ) )
                .Include( c => c.Category )
                .Include( c => c.LearningCourseRequirements )
                .Where( c =>
                    c.IsActive
                    && c.LearningProgramId == programId
                    && ( c.IsPublic || !publicOnly ) )
                .ToList()
                .OrderBy( c => c.Order )
                .Select( c => new PublicLearningCourseBag
                {
                    AllowHistoricalAccess = c.AllowHistoricalAccess,
                    Category = c.Category?.Name,
                    CategoryColor = c.Category?.HighlightColor,
                    CourseCode = c.CourseCode,
                    CourseRequirements = c.LearningCourseRequirements?.ToList() ?? default,
                    Credits = c.Credits,
                    Description = c.Description,
                    Id = c.Id,
                    ImageFileGuid = c.ImageBinaryFile?.Guid,
                    IsEnrolled = personId.HasValue
                        && orderedPersonCompletions.Any( lp => lp.LearningClass.LearningCourseId == c.Id ),

                    // Get the person's completion status for this course.
                    LearningCompletionStatus = !personId.HasValue ?
                        null :
                        orderedPersonCompletions
                        .FirstOrDefault( p => p.LearningClass.LearningCourseId == c.Id )?
                        .LearningCompletionStatus,
                    IsCompletionOnly = !c.LearningClasses.Any( lc =>
                        lc.LearningGradingSystem.LearningGradingSystemScales
                            .Select( s => s.Id )
                            .Count() > 1
                    ),
                    CompletionScaleName = !personId.HasValue ?
                        null :
                        orderedPersonCompletions
                        .FirstOrDefault( p => p.LearningClass.LearningCourseId == c.Id )?
                        .LearningGradingSystemScale?.Name,
                    ProgramInfo = new LearningProgramService.PublicLearningProgramBag
                    {
                        Id = c.LearningProgramId,
                        PublicName = c.LearningProgram?.PublicName,
                        Summary = c.LearningProgram?.Summary,
                        Category = c.LearningProgram?.Category?.Name,
                        CategoryColor = c.LearningProgram?.Category?.HighlightColor,
                        ConfigurationMode = c.LearningProgram?.ConfigurationMode ?? ConfigurationMode.AcademicCalendar,
                        ImageFileGuid = c.LearningProgram?.ImageBinaryFile?.Guid
                    },
                    PublicName = c.PublicName,
                    Order = c.Order,
                    Semesters = semesters.Where( s => s.LearningClasses.Any( c2 => c2.LearningCourseId == c.Id ) )
                        .Select( s => new PublicLearningSemesterBag
                        {
                            Id = s.Id,
                            StartDate = s.StartDate,
                            EndDate = s.EndDate,
                            EnrollmentCloseDate = s.EnrollmentCloseDate
                        } )
                        .ToList(),
                    Summary = c.Summary,

                    // Only Prerequisites/Equivalents where the course completions for the student aren't 'Passed'.
                    UnmetPrerequisites = c.LearningCourseRequirements?
                        .Where( r =>
                            unmetPrerequisiteTypes.Contains( r.RequirementType ) &&
                            (
                                !personId.HasValue
                                || !orderedPersonCompletions.Any( comp =>
                                comp.LearningCompletionStatus == LearningCompletionStatus.Pass &&
                                comp.LearningClass.LearningCourseId == r.RequiredLearningCourseId ) ) )
                        .ToList() ?? default
                } )
                .ToList();

            // Set the IdKeys for the entities.
            foreach ( var course in courses )
            {
                course.ProgramInfo.IdKey = IdHasher.Instance.GetHash( course.ProgramInfo.Id );
                course.IdKey = IdHasher.Instance.GetHash( course.Id );

                foreach ( var s in course.Semesters )
                {
                    s.IdKey = IdHasher.Instance.GetHash( s.Id );
                }
            }

            if ( courses.Any( c => c.CourseRequirements.Any() ) )
            {
                // Get the required courses in a single query.
                var requiredCourseIds = courses.SelectMany( c => c.CourseRequirements.Select( r => r.RequiredLearningCourseId ) );
                var requiredCourses = Queryable().Where( c => requiredCourseIds.Contains( c.Id ) );

                // Then match them up based on the course id of the required course.
                foreach ( var course in courses )
                {
                    if ( !course.CourseRequirements.Any() )
                    {
                        continue;
                    }

                    course.CourseRequirements.ForEach( cr =>
                        cr.RequiredLearningCourse =
                            requiredCourses.FirstOrDefault( r => r.Id == cr.RequiredLearningCourseId ) );
                }
            }

            return courses.ToList();
        }

        /// <summary>
        /// Gets a list of LearningCourseRequirements where the <see cref="Person"/> specified by the <paramref name="personId"/>
        /// hasn't completed the course.
        /// </summary>
        /// <param name="personId">The identifier of the <see cref="Person"/> to check requirement completions for.</param>
        /// <param name="courseRequirements">The List of <see cref="LearningCourseRequirement"/> records to check completions for.</param>
        /// <returns>A List of <see cref="LearningCourseRequirement"/> records that haven't been completed by the <see cref="Person"/>.</returns>
        public List<LearningCourseRequirement> GetUnmetCourseRequirements( int? personId, IEnumerable<LearningCourseRequirement> courseRequirements )
        {
            var completedClasses = personId.HasValue
                ? new LearningParticipantService( ( RockContext ) Context ).GetClassesForStudent( personId.Value ).ToList()
                : new List<LearningParticipant>();

            return courseRequirements
                .Where( cr => cr.RequirementType == RequirementType.Prerequisite
                    && !completedClasses.Any( c => c.LearningClass.LearningCourseId == cr.RequiredLearningCourseId && c.LearningCompletionStatus == LearningCompletionStatus.Pass ) )
                .ToList();
        }

        #region Nested Classes for Lava

        /// <summary>
        /// Represents the Lava enabled data sent to the public courses list block.
        /// </summary>
        public class PublicLearningCourseBag : LavaDataObject
        {
            /// <summary>
            /// Gets or sets whether this course is allowed to show historical access.
            /// </summary>
            public bool AllowHistoricalAccess { get; set; }

            /// <summary>
            /// Gets or sets the category.
            /// </summary>
            public string Category { get; set; }

            /// <summary>
            /// Gets or sets the highlight color of the category.
            /// </summary>
            public string CategoryColor { get; set; }

            /// <summary>
            /// Gets or sets the code for the course.
            /// </summary>
            public string CourseCode { get; set; }

            /// <summary>
            /// Gets or sets the link to the course details.
            /// </summary>
            public string CourseDetailsLink { get; set; }

            /// <summary>
            /// Gets or sets the list of course requirements for the requested course.
            /// </summary>
            public List<LearningCourseRequirement> CourseRequirements { get; set; }

            /// <summary>
            /// Gets or sets the number of credits awarded for successful completion of this
            /// <see cref="LearningCourse"/>.
            /// </summary>
            public int Credits { get; set; }

            /// <summary>
            /// Gets or sets the description.
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// Gets or sets the description as an HTML string.
            /// </summary>
            public string DescriptionAsHtml { get; set; }

            /// <summary>
            /// Gets or sets the identifier for the <see cref="LearningCourse"/>.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the IdKey for the <see cref="LearningCourse"/>.
            /// </summary>
            public string IdKey { get; set; }

            /// <summary>
            /// Gets or sets the Guid for the Image file of this Program.
            /// </summary>
            public Guid? ImageFileGuid { get; set; }

            /// <summary>
            /// Determines if the current person is enrolled as a student in
            /// this course. This includes past classes of the course.
            /// </summary>
            public bool IsEnrolled { get; set; }

            /// <summary>
            /// Gets or sets the completion status of the course for the current person.
            /// </summary>
            /// <remarks>
            /// If the Individual has ever Passed the Course this will show 'Passed';
            /// otherwise the most recent occurrence (if any) will show: 'Incomplete' or 'Failed'.
            /// </remarks>
            public LearningCompletionStatus? LearningCompletionStatus { get; set; }

            /// <summary>
            /// Gets or sets the boolean indicating if all grading for this course is done by completion only.
            /// </summary>
            /// <remarks>
            /// To determine if the Grading System is "Completion" we check the number of Grading System Scales.
            /// If there is only one Scale, than we conclude it is "Completion". If there are any Classes in this
            /// Course that are not found to be "Completion" than this value will be set to false.
            /// </remarks>
            public bool IsCompletionOnly { get; set; }

            /// <summary>
            /// Gets or sets the name of the grading system scale for "Completion Only" courses.
            /// </summary>
            /// <remarks>
            /// Only intended for "Completion Only" courses, where it is guaranteed there is a single scale.
            /// </remarks>
            public string CompletionScaleName { get; set; }

            /// <summary>
            /// Gets or sets the sort order for the <see cref="LearningCourse"/>.
            /// </summary>
            public int Order { get; set; }

            /// <summary>
            /// Gets or sets the
            /// <see cref="LearningProgramService.PublicLearningProgramBag"/>
            /// for the <see cref="LearningCourse"/>.
            /// </summary>
            public LearningProgramService.PublicLearningProgramBag ProgramInfo { get; set; }

            /// <summary>
            /// Gets or sets the Public Name for the <see cref="LearningCourse"/>.
            /// </summary>
            public string PublicName { get; set; }

            /// <summary>
            ///  Gets or sets the current and future semesters that have classes available for enrollment.
            /// </summary>
            public List<PublicLearningSemesterBag> Semesters { get; set; }

            /// <summary>
            /// Gets or sets the Summary for the <see cref="LearningCourse"/>.
            /// </summary>
            public string Summary { get; set; }

            /// <summary>
            /// Gets or sets a list of LearningCourseRequirements where the Person hasn't yet Passed the Prerequisite.
            /// </summary>
            public List<LearningCourseRequirement> UnmetPrerequisites { get; set; }
        }

        /// <summary>
        /// The Semester for displaying on public pages.
        /// </summary>
        public class PublicLearningSemesterBag : LavaDataObject
        {
            /// <summary>
            ///  Gets or sets the list of available classes.
            /// </summary>
            public List<PublicLearningClassBag> AvailableClasses { get; set; }

            /// <summary>
            /// Gets or sets the end date for classes in this <see cref="LearningSemester"/>.
            /// </summary>
            public DateTime? EndDate { get; set; }

            /// <summary>
            /// Gets or sets the enrollment close date for classes in this <see cref="LearningSemester"/>.
            /// </summary>
            public DateTime? EnrollmentCloseDate { get; set; }

            /// <summary>
            /// Gets or sets the identifier for the <see cref="LearningSemester"/>.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the IdKey for the <see cref="LearningSemester"/>.
            /// </summary>
            public string IdKey { get; set; }

            /// <summary>
            /// Gets or sets whether the individual is enrolled in a class for this <see cref="LearningSemester"/>.
            /// </summary>
            public bool IsEnrolled { get; set; }

            /// <summary>
            /// Gets or sets the name of the <see cref="LearningSemester"/>.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the start date for classes in this <see cref="LearningSemester"/>.
            /// </summary>
            public DateTime? StartDate { get; set; }
        }

        /// <summary>
        /// The <see cref="LearningClass"/> for displaying on public pages.
        /// </summary>
        public class PublicLearningClassBag : LavaDataObject
        {
            /// <summary>
            /// Gets or sets the <see cref="Campus"/> that the class meets at or is associated with.
            /// </summary>
            public Campus Campus { get; set; }

            /// <summary>
            /// Gets or sets whether the current person can enroll in this class.
            /// </summary>
            public bool CanEnroll { get; set; }

            /// <summary>
            /// Gets or sets the enrollment error key (if any).
            /// </summary>
            /// <remarks>
            /// (one of: 'unmet_course_requirements', 'class_full', 'enrollment_closed', 'already_enrolled').
            /// </remarks>
            public string EnrollmentErrorKey { get; set; }

            /// <summary>
            /// Gets or sets the link to the enrollment page for this class.
            /// </summary>
            public string EnrollmentLink { get; set; }

            /// <summary>
            /// Gets or sets whether the individual is enrolled in this <see cref="LearningClass"/>.
            /// </summary>
            public bool IsEnrolled { get; set; }

            /// <summary>
            /// Gets or sets the list of Facilitators for this class.
            /// </summary>
            public List<PublicFacilitatorBag> Facilitators { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="LearningGradingSystem "/> the class uses.
            /// </summary>
            public LearningGradingSystem GradingSystem { get; set; }

            /// <summary>
            /// Gets or sets the identifier for the <see cref="LearningClass"/>.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the IdKey for the <see cref="LearningClass"/>.
            /// </summary>
            public string IdKey { get; set; }

            /// <summary>
            /// Gets or sets the Location for the LearningClass to meet.
            /// </summary>
            public GroupLocation GroupLocation { get; set; }

            /// <summary>
            /// Gets the value of <c>GroupLocation.Location.ToString( preferName: true )</c>; otherwise <c>string.Empty</c>.
            /// </summary>
            public string Location => GroupLocation?.Location?.ToString( true ) ?? string.Empty;

            /// <summary>
            /// Gets or sets the Name for the LearningClass.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the sort order for the <see cref="LearningClass"/>.
            /// </summary>
            public int Order { get; set; }

            /// <summary>
            /// Gets or sets the schedule that the class meets.
            /// </summary>
            public Schedule Schedule { get; set; }

            /// <summary>
            /// Gets or sets the current <see cref="Person"/> <see cref="LearningParticipant"/> record for this class.
            /// </summary>
            public LearningParticipant StudentParticipant { get; set; }

            /// <summary>
            /// Gets or sets the link to the workspace page for this class.
            /// </summary>
            public string WorkspaceLink { get; set; }
        }

        /// <summary>
        /// The <see cref="LearningParticipant">facilitator</see> for displaying on public pages.
        /// </summary>
        public class PublicFacilitatorBag : LavaDataObject
        {
            /// <summary>
            ///  Gets or sets the email of the facilitator.
            /// </summary>
            public string Email { get; set; }

            /// <summary>
            /// Gets or sets the name of the facilitator.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the identifier for the facilitator's <see cref="LearningParticipant"/> record.
            /// </summary>
            public int ParticipantId { get; set; }

            /// <summary>
            /// Gets or sets the IdKey for the facilitator's <see cref="LearningParticipant"/> record.
            /// </summary>
            public string ParticipantIdKey { get; set; }

            /// <summary>
            /// Gets or sets the name of the facilitator's role.
            /// </summary>
            public string Role { get; set; }
        }

        #endregion
    }
}

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

using Microsoft.Extensions.Logging;

using Rock.Attribute;
using Rock.Enums.Lms;
using Rock.Model;
using Rock.ViewModels.Blocks.Lms.PublicLearningClassEnrollment;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Lms
{
    /// <summary>
    /// Displays the details for a public learning course.
    /// </summary>
    [DisplayName( "Public Learning Class Enrollment" )]
    [Category( "LMS" )]
    [Description( "Allows the current person or other registrant to enroll in a learning class." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [CodeEditorField( "Header Lava Template",
        Key = AttributeKey.HeaderLavaTemplate,
        Description = "The Lava template to use to show a header above the various state templates. Merge fields include: LearningClass, Facilitators, Registrant, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = AttributeDefault.HeaderLavaTemplate,
        Order = 1 )]

    [CodeEditorField( "Confirmation Lava Template",
        Key = AttributeKey.ConfirmationLavaTemplate,
        Description = "The Lava template to use when displaying the confirmation messaging to the individual. Merge fields include: UnmetRequirements, LearningClass, Facilitators, Registrant, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = AttributeDefault.ConfirmationLavaTemplate,
        Order = 2 )]

    [CodeEditorField( "Completion Lava Template",
        Key = AttributeKey.CompletionLavaTemplate,
        Description = "The Lava template to use to show the completed message. Merge fields include: UnmetRequirements, LearningClass, Facilitators, Registrant, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = AttributeDefault.CompletionLavaTemplate,
        Order = 3 )]

    [CodeEditorField( "Enrollment Error Lava Template",
        Key = AttributeKey.EnrollmentErrorLavaTemplate,
        Description = "The Lava template to use when the individual is not able to enroll. Merge fields include: ErrorKey (one of: 'unmet_course_requirements', 'class_full', 'enrollment_closed'), UnmetRequirements, Facilitators, LearningClass, Registrant, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = AttributeDefault.EnrollmentErrorLavaTemplate,
        Order = 4 )]

    [LinkedPage( "Course Detail Page",
        Description = "Page to use for links back to the course detail.",
        Key = AttributeKey.CourseDetailPage,
        IsRequired = false,
        Order = 5 )]

    [LinkedPage( "Class Workspace Page",
        Description = "Page to use for links to the class workspace.",
        Key = AttributeKey.ClassWorkspacePage,
        IsRequired = false,
        Order = 6 )]

    [LinkedPage( "Course List Page",
        Description = "Page to use for links back to the course list.",
        Key = AttributeKey.CourseListPage,
        IsRequired = false,
        Order = 7 )]

    [Rock.SystemGuid.EntityTypeGuid( "4F9F2B15-14EF-47AB-858B-641858674AC7" )]
    [Rock.SystemGuid.BlockTypeGuid( "E80F9006-3C00-4F36-839E-7A0883F9E229" )]
    public class PublicLearningClassEnrollment : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string CompletionLavaTemplate = "CompletionLavaTemplate";
            public const string ConfirmationLavaTemplate = "ConfirmationLavaTemplate";
            public const string CourseDetailPage = "CourseDetailPage";
            public const string CourseListPage = "CourseListPage";
            public const string ClassWorkspacePage = "ClassWorkspacePage";
            public const string EnrollmentErrorLavaTemplate = "EnrollmentErrorLavaTemplate";
            public const string HeaderLavaTemplate = "HeaderLavaTemplate";
        }

        private static class NavigationUrlKey
        {
            public const string CourseDetailPage = "CourseDetailPage";
            public const string CourseListPage = "CourseListPage";
            public const string ClassWorkspacePage = "ClassWorkspacePage";
        }

        private static class PageParameterKey
        {
            /// <summary>
            /// The LearningClassIdKey of the LearningClass to be enrolled in.
            /// </summary>
            public const string LearningClassId = "LearningClassId";

            /// <summary>
            /// The PersonAliasIdKey of the person to register (overrides CurrentPerson).
            /// </summary>
            public const string RegistrantId = "RegistrantId";
        }

        private static class AttributeDefault
        {
            public const string HeaderLavaTemplate = @"
//- Styles
{% stylesheet %}
        .page-header-section {
            height: 300px; 
        align-items: center; 
        border-radius: 12px; 
        background-image: url('/GetImage.ashx?guid={{LearningClass.LearningCourse.ImageBinaryFile.Guid}}'); 
        background-size: cover;
        background-position: center;
        position: relative;
        margin-bottom: 32px;
    }
    
    .header-block {
        height: max-content;
        position: absolute;
        left: 50%;
        bottom: -24px;
        -webkit-transform: translateY(-15%);
        transform: translateY(-15%);
        transform: translatex(-50%);
        background-color: white; 
        border-radius: 12px; 
        width: 80%;
        display: flex;
        flex-direction: column;
        gap: 1rem;
        align-items: center;
        justify-content: center;
        overflow: hidden;
        padding: 1.5rem; 
    }
    
    .page-sub-header {
        overflow: hidden;
        display: -webkit-box;
        -webkit-box-orient: vertical;
        -webkit-line-clamp: 4;
        text-overflow: ellipsis;
        text-align: center;
        overflow: hidden;
        padding-bottom: 0;
    }

{% endstylesheet %}

<div class=""page-header-section"">
    <div class=""header-block text-center border border-gray-400"">
        <h1 class=""text-bold"">
            {{ LearningClass.LearningCourse.PublicName }}
        </h1>
        <p class=""page-sub-header"">
            {{ LearningClass.LearningCourse.Summary }}
        </p>
    </div>
</div>
";
            public const string ConfirmationLavaTemplate = @"
{% stylesheet %}
    .confirmation-details {
        width: 545px;
    }

    .enrollment-participant-row,
    .enrollment-class-row {
        border: 1px solid var(--theme-light);
        padding: 8px;
    }
{% endstylesheet %}

{% assign facilitatorCount = Facilitators | Size %}

{% assign facilitatorsText = '' %}
{% for f in Facilitators %}
    {%- capture name -%}
        {{f | Property:'Name'}}{%- unless forloop.last -%}, {% endunless %}
    {%- endcapture -%}

    {% assign facilitatorsText = facilitatorsText | Append:name %}
{% endfor %}

{% if facilitatorsText == empty %}
    {% assign facilitatorsText = 'TBD' %}
{% endif %}

{% assign credits = LearningClass.LearningCourse.Credits | AsInteger %}
{% assign location = LearningClass.GroupLocations | First %}
{% assign locationNameLength = location.Name | Size %}
{% assign schedule = LearningClass.Schedule %}
{% assign scheduleNameLength = schedule.Name | Size %}
{% assign hasLocation = locationNameLength > 0 %}
{% assign hasSchedule = scheduleNameLength > 0 %}

<div class=""confirmation-container d-flex flex-column justify-content-center"">
    <h3 class=""enrollment-review-header text-center"">Enrollment Review</h3>
    <div class=""enrollment-review-sub-header text-center"">Please review class details before confirming enrollment:</div>

    <div class=""confirmation-details d-flex flex-column mt-3"">
        <h5 class=""participant-details-header"">Participant Details</h5>
        <div class=""enrollment-participant-row participant-name d-flex justify-content-between"">
            <div class=""field-title participant-name"">
                Name
            </div>
            <div class=""field-value participant-name"">
                {{Registrant.FullName}}
            </div>
        </div>
        <div class=""enrollment-participant-row participant-email d-flex justify-content-between"">
            <div class=""field-title participant-email"">
                Email
            </div>
            <div class=""field-value participant-email"">
                {{Registrant.Email}}
            </div>
        </div>

        <h5 class=""class-details-header"">Class Details</h5>
        <div class=""enrollment-class-row course-code d-flex justify-content-between"">
            <div class=""field-title course-name"">
                Course Name
            </div>
            <div class=""field-value course-name"">
                {{LearningClass.LearningCourse.PublicName}}
            </div>
        </div>
        {% if LearningClass.LearningCourse.CourseCode != empty %}
        <div class=""enrollment-class-row course-code d-flex justify-content-between"">
            <div class=""field-title course-code"">
                Course Code
            </div>
            <div class=""field-value course-code"">
                {{LearningClass.LearningCourse.CourseCode}}
            </div>
        </div>
        {% endif %}

        <div class=""enrollment-class-row course-configuration d-flex justify-content-between"">
            <div class=""field-title course-code"">
                Course Configuration
            </div>
            <div class=""field-value course-code"">
                {% if LearningClass.LearningCourse.LearningProgram.ConfigurationMode == 0 %}
                    Academic Calendar
                {% elseif LearningClass.LearningCourse.LearningProgram.ConfigurationMode == 1 %}
                    On-Demand
                {% endif %}
            </div>
        </div>

        <div class=""enrollment-class-row facilitator d-flex justify-content-between"">
            <div class=""field-title facilitator"">
                {{ 'Facilitator' | PluralizeForQuantity:facilitatorCount }}:
            </div>
            <div class=""field-value facilitator"">
                {{facilitatorsText}}
            </div>
        </div>

        {% if credits > 0 %}
            <div class=""enrollment-class-row credits d-flex justify-content-between"">
                <div class=""field-title credits"">
                    Credits
                </div>
                <div class=""field-value credits"">
                    {{LearningClass.LearningCourse.Credits}}
                </div>
            </div>
        {% endif %}
    </div>

    <div class=""enrollment-class-row grading-system d-flex justify-content-between"">
        <div class=""field-title grading-system"">
            Grading System
        </div>
        <div class=""field-value grading-system"">
            {{LearningClass.LearningGradingSystem.Name}}
        </div>
    </div>

    <div class=""enrollment-class-row semester d-flex justify-content-between"">
        <div class=""field-title semester"">
            Semester
        </div>
        <div class=""field-value semester"">
            {{LearningClass.LearningSemester.Name}}
        </div>
    </div>

    {% if hasLocation %}
        <div class=""enrollment-class-row location d-flex justify-content-between"">
            <div class=""field-title location"">
                Location
            </div>
            <div class=""field-value location"">
                {{location.Name}}
            </div>
        </div>
    {% endif %}

    {% if hasSchedule %}
        <div class=""enrollment-class-row schedule d-flex justify-content-between"">
            <div class=""field-title schedule"">
                Schedule
            </div>
            <div class=""field-value schedule"">
                {{schedule.Name}}
            </div>
        </div>
    {% endif %}

    {% if LearningClass.LearningSemester.StartDate %}
        <div class=""enrollment-class-row location d-flex justify-content-between"">
            <div class=""field-title location"">
                Starts
            </div>
            <div class=""field-value location"">
                {{LearningClass.LearningSemester.StartDate |  Date:'sd' }}
            </div>
        </div>
    {% endif %}

</div>

";
            public const string CompletionLavaTemplate = @"
<div class=""completion-container d-flex flex-column justify-content-center"">
    <i class=""fa fa-check-circle fa-4x text-success text-center""></i>
    <h3 class=""completion-header text-center"">Successfully Enrolled!</h3>
    <div class=""completion-sub-header text-center"">
        You are now enrolled in this class.
        Check your email for a confirmation with your enrollment details.
        Click “Go to Class Workspace” to begin your learning experience.
    </div>
</div>
";
            public const string EnrollmentErrorLavaTemplate = @"
<div class=""error-container d-flex flex-column justify-content-center"">
    <i class=""fa fa-exclamation-triangle fa-4x text-danger text-center""></i>
    <h3 class=""error-header text-center"">Cannot Enroll in Class</h3>
    <div class=""error-sub-header text-center"">
        {% case ErrorKey %}
        {% when 'unmet_course_requirements' %}
            You have not completed the following 
            {{ 'prerequisite' | PluralizeForQuantity:UnmetRequirements }} for this course:
            <ul class=""d-inline-block"">
                {% for requirement in UnmetRequirements %}
                <li>{{requirement.RequiredLearningCourse.Name}}</li>
                {% endfor %}
            </ul>
        {% when 'class_full' %}
            This class has reached it's capacity. Please go back to the Course Detail and try again.
        {% when 'enrollment_closed' %}
            Enrollment is closed for this class.
        {% when 'already_enrolled' %}
            You're already enrolled in this class.
        {% else %}
            Something went wrong with your enrollment. Please contact the facilitator for further support.
        {% endcase %}
    </div>
</div>
";
        }

        #endregion Keys

        #region Properties

        private string CompletionLavaTemplate => GetAttributeValue( AttributeKey.CompletionLavaTemplate );

        private string ConfirmationLavaTemplate => GetAttributeValue( AttributeKey.ConfirmationLavaTemplate );

        private string EnrollmentErrorLavaTemplate => GetAttributeValue( AttributeKey.EnrollmentErrorLavaTemplate );

        private string HeaderLavaTemplate => GetAttributeValue( AttributeKey.HeaderLavaTemplate );


        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new PublicLearningClassEnrollmentBlockBox();

            SetBoxInitialEntityState( box );


            return box;
        }

        private Person GetRegistrant( Person currentPerson )
        {
            var registrantId = PageParameter( PageParameterKey.RegistrantId );
            Person registrant = null;

            if ( registrantId.IsNotNullOrWhiteSpace() )
            {
                registrant = new PersonAliasService( RockContext ).GetSelect( registrantId, p => p.Person );
            }

            return registrant ?? currentPerson;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls( LearningClass learningClass )
        {
            // Add only the parameters necessary for each page.
            var courseListParams = new Dictionary<string, string>
            {
                ["LearningProgramId"] = learningClass.LearningCourse.LearningProgram.IdKey
            };
            var courseDetailParams = new Dictionary<string, string>
            {
                ["LearningProgramId"] = learningClass.LearningCourse.LearningProgram.IdKey,
                ["LearningCourseId"] = learningClass.LearningCourse.IdKey
            };


            var courseWorkspaceParams = new Dictionary<string, string>
            {
                ["LearningProgramId"] = learningClass.LearningCourse.LearningProgram.IdKey,
                ["LearningCourseId"] = learningClass.LearningCourse.IdKey,
                ["LearningClassId"] = learningClass.IdKey
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.CourseDetailPage] = this.GetLinkedPageUrl( AttributeKey.CourseDetailPage, courseDetailParams ),
                [NavigationUrlKey.CourseListPage] = this.GetLinkedPageUrl( AttributeKey.CourseListPage, courseListParams ),
                [NavigationUrlKey.ClassWorkspacePage] = this.GetLinkedPageUrl( AttributeKey.ClassWorkspacePage, courseWorkspaceParams )
            };
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( PublicLearningClassEnrollmentBlockBox box )
        {
            var currentPerson = GetCurrentPerson();
            var registrant = GetRegistrant( currentPerson );

            if (registrant == null )
            {
                box.ErrorMessage = "It looks like we don't have the information needed to enroll someone for this class. Please make sure you're logged in and try again.";
                return;
            }

            // Get the merge fields using the current person and registrant.
            var mergeFields = GetMergeFields( currentPerson, registrant, out var learningClass, out var _ );
            box.NavigationUrls = GetBoxNavigationUrls( learningClass );

            if ( learningClass.Id == 0 )
            {
                box.ErrorMessage = "It seems the course information is missing. Please select a course and try enrolling again.";
                return;
            }

            box.HeaderHtml = HeaderLavaTemplate.ResolveMergeFields( mergeFields );
            box.ConfirmationHtml = ConfirmationLavaTemplate.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Gets the merge fields for the specified <paramref name="currentPerson"/> and <paramref name="registrant"/>.
        /// </summary>
        /// <param name="currentPerson">The person currently viewing this block.</param>
        /// <param name="registrant">The person being registered for the class.</param>
        /// <param name="learningClass">The LearningClass model that was added to the merge fields.</param>
        /// <param name="unmetRequirements">THe list of <see cref="LearningCourseRequirement"/> records that haven't been completed by the <paramref name="registrant"/>.</param>
        /// <returns>A Dictionary&lt;string, object&gt; with all merge fields.</returns>
        private Dictionary<string, object> GetMergeFields( Person currentPerson, Person registrant, out LearningClass learningClass, out List<LearningCourseRequirement> unmetRequirements )
        {
            var learningClassService = new LearningClassService( RockContext );
            var learningClassId = learningClassService.GetSelect( PageParameter( PageParameterKey.LearningClassId ), c => c.Id );
            learningClass =
                learningClassId == 0 ?
                default :
                learningClassService
               .Queryable()
               .Include( c => c.LearningCourse )
               .Include( c => c.LearningCourse.LearningProgram )
               .Include( c => c.LearningCourse.ImageBinaryFile )
               .Include( c => c.LearningCourse.LearningCourseRequirements )
               .Include( c => c.GroupLocations )
               .Include( c => c.Schedule )
               .Include( c => c.LearningSemester )
               .Include( c => c.LearningGradingSystem )
               .FirstOrDefault( c => c.Id == learningClassId );

            var participantService = new LearningParticipantService( RockContext );
            var facilitators = participantService.GetFacilitatorBags( learningClass.Id );

            var mergeFields = this.RequestContext.GetCommonMergeFields( currentPerson );
            mergeFields.Add( "Registrant", registrant );
            mergeFields.Add( "LearningClass", learningClass );
            mergeFields.Add( "Facilitators", facilitators );

            unmetRequirements =
                learningClassId == 0 ?
                default :
                new LearningCourseService( RockContext )
                .GetUnmetCourseRequirements( registrant.Id, learningClass.LearningCourse.LearningCourseRequirements );

            mergeFields.Add( "UnmetRequirements", unmetRequirements );

            return mergeFields;
        }

        /// <summary>
        /// Gets a new student LearningParticipant based on the page parameters.
        /// </summary>
        /// <returns>A New <see cref="LearningParticipant"/> record for the registrant specified by the page parameters.</returns>
        private LearningParticipant GetNewLearningParticipant( Person registrant )
        {
            var classService = new LearningClassService( RockContext );

            var classIdKey = PageParameter( PageParameterKey.LearningClassId );
            var classId = classService.GetSelect( classIdKey, p => p.Id );
            var studentRole = classService.GetClassRoles( classId ).FirstOrDefault( r => !r.IsLeader );

            return new LearningParticipant
            {
                PersonId = registrant.Id,
                LearningClassId = classId,
                GroupId = classId,
                GroupRoleId = studentRole.Id.ToIntSafe(),
                LearningCompletionStatus = LearningCompletionStatus.Incomplete,
                LearningGradePercent = 0
            };
        }

        private bool IsValid( LearningClass learningClass, Person registrant, List<LearningCourseRequirement> unmetRequirements, out string errorKey )
        {
            errorKey = string.Empty;

            if (learningClass.LearningSemester.EnrollmentCloseDate.HasValue && learningClass.LearningSemester.EnrollmentCloseDate.Value.IsPast() )
            {
                errorKey = "enrollment_closed";
                return false;
            }

            var participantService = new LearningParticipantService( RockContext );
            var studentCount = participantService.GetStudents( learningClass.Id ).Count();
            if ( studentCount >= learningClass.LearningCourse.MaxStudents )
            {
                errorKey = "class_full";
                return false;
            }

            // Already enrolled (as a student).
            var alreadyEnrolled = participantService.Queryable().Any( p =>
                p.PersonId == registrant.Id
                && p.LearningClassId == learningClass.Id
                && !p.GroupRole.IsLeader );

            if ( alreadyEnrolled )
            {
                errorKey = "already_enrolled";
                return false;
            }

            if ( unmetRequirements.Any() )
            {
                errorKey = "unmet_course_requirements";
                return false;
            }

            return true;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Attempts to enroll the registrant in the class.
        /// </summary>
        /// <returns>The resolved completion template if successful; otherwise the resolved enrollment error template.</returns>
        [BlockAction]
        public BlockActionResult Enroll()
        {
            var currentPerson = GetCurrentPerson();
            var registrant = GetRegistrant( currentPerson );
            var mergeFields = GetMergeFields( currentPerson, registrant, out var learningClass, out var unmetRequirements );

            try
            {
                if (!IsValid( learningClass, registrant, unmetRequirements, out var errorMessage ) )
                {
                    mergeFields.Add( "ErrorKey", errorMessage);
                    return ActionBadRequest( EnrollmentErrorLavaTemplate.ResolveMergeFields( mergeFields ) );
                }

                var learningParticipantService = new LearningParticipantService( RockContext );
                var newLearningParticipant = GetNewLearningParticipant( registrant );

                learningParticipantService.Add( newLearningParticipant );

                RockContext.SaveChanges();

                if ( newLearningParticipant.Id > 0 )
                {
                    return ActionOk( CompletionLavaTemplate.ResolveMergeFields( mergeFields ) );
                }
            }
            catch ( Exception ex )
            {
                Logger.LogError( ex, $"Unable to add LearningParticipant to class from enrollment page for registrant with ID {registrant.Id}." );
                return ActionBadRequest( EnrollmentErrorLavaTemplate.ResolveMergeFields( mergeFields ) );
            }

            return ActionBadRequest( EnrollmentErrorLavaTemplate.ResolveMergeFields( mergeFields ) );
        }

        #endregion
    }
}

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
        IsRequired = false,
        DefaultValue = AttributeDefault.HeaderLavaTemplate,
        Order = 1 )]

    [CodeEditorField( "Confirmation Lava Template",
        Key = AttributeKey.ConfirmationLavaTemplate,
        Description = "The Lava template to use when displaying the confirmation messaging to the individual. Merge fields include: ErrorKey (one of: 'unmet_course_requirements', 'class_full', 'enrollment_closed', 'already_enrolled'), UnmetRequirements, LearningClass, Facilitators, Registrant, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = false,
        DefaultValue = AttributeDefault.ConfirmationLavaTemplate,
        Order = 2 )]

    [CodeEditorField( "Completion Lava Template",
        Key = AttributeKey.CompletionLavaTemplate,
        Description = "The Lava template to use to show the completed message. Merge fields include: UnmetRequirements, LearningClass, Facilitators, Registrant, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = false,
        DefaultValue = AttributeDefault.CompletionLavaTemplate,
        Order = 3 )]

    [CodeEditorField( "Enrollment Error Lava Template",
        Key = AttributeKey.EnrollmentErrorLavaTemplate,
        Description = "The Lava template to use when the individual is not able to enroll. Merge fields include: ErrorKey (one of: 'unmet_course_requirements', 'class_full', 'enrollment_closed', 'already_enrolled'), UnmetRequirements, Facilitators, LearningClass, Registrant, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", EditorMode = CodeEditorMode.Lava,
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
<div class=""hero-section"">
    <div class=""hero-section-image"" style=""background-image: url('/GetImage.ashx?guid={{ LearningClass.LearningCourse.ImageBinaryFile.Guid }}')""></div>
    <div class=""hero-section-content"">
        <h1 class=""hero-section-title""> {{ LearningClass.LearningCourse.PublicName }} </h1>
        <p class=""hero-section-description""> {{ LearningClass.LearningCourse.Summary }} </p>
    </div>
</div>
";
            public const string ConfirmationLavaTemplate = @"
//-Variable Assignments
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


{% stylesheet %}
    .confirmation-details {
        width: 100%;
        max-width: 545px;
    }
    
    .detail-table {
        border: 1px solid var(--color-interface-softer);
        width: 100%;
    }
    
    .detail-row {
        border-bottom: 1px solid var(--color-interface-softer);
        padding: 8px;
        width: 100%;
    }
    
    .detail-row:last-child {
        border-bottom: none;
    }

    
{% endstylesheet %}



<div class=""d-flex flex-column w-100 justify-content-center gap-4 my-4"">
    
    //- 1 REVIEW HEADING
    <div>
        <h3 class=""text-center"">Enrollment Review</h3>
        <div class=""text-center"">Please review class details before confirming enrollment:</div>
    </div>
    
    //- 2 TABLE
    <div class=""d-flex flex-column mt-3 gap-3"">
        <div>
            <h5>Participant Details</h5>
            <div class=""detail-table"">
                <div class=""detail-row d-flex justify-content-between"">
                    <div class=""field-title"">
                        Name
                    </div>
                    <div class=""field-value"">
                        {{Registrant.FullName}}
                    </div>
                </div>
                <div class=""detail-row participant-email d-flex justify-content-between"">
                    <div class=""field-title"">
                        Email
                    </div>
                    <div class=""field-value"">
                        {{Registrant.Email}}
                    </div>
                </div>
            </div>
        </div>
        
        <div>
            <h5>Class Details</h5>
            <div class=""detail-table"">
                <div class=""detail-row  d-flex justify-content-between"">
                    <div class=""field-title"">
                        Course Name
                    </div>
                    <div class=""field-value"">
                        {{LearningClass.LearningCourse.PublicName}}
                    </div>
                </div>
                {% if LearningClass.LearningCourse.CourseCode != empty %}
                <div class=""detail-row  d-flex justify-content-between"">
                    <div class=""field-title"">
                        Course Code
                    </div>
                    <div class=""field-value"">
                        {{LearningClass.LearningCourse.CourseCode}}
                    </div>
                </div>
                {% endif %}
        
                <div class=""detail-row d-flex justify-content-between"">
                    <div class=""field-title"">
                        Course Configuration
                    </div>
                    <div class=""field-value "">
                        {% if LearningClass.LearningCourse.LearningProgram.ConfigurationMode == 0 %}
                            Academic Calendar
                        {% elseif LearningClass.LearningCourse.LearningProgram.ConfigurationMode == 1 %}
                            On-Demand
                        {% endif %}
                    </div>
                </div>
        
                <div class=""detail-row d-flex justify-content-between"">
                    <div class=""field-title"">
                        {{ 'Facilitator' | PluralizeForQuantity:facilitatorCount }}:
                    </div>
                    <div class=""field-value"">
                        {{facilitatorsText}}
                    </div>
                </div>
        
                {% if credits > 0 %}
                    <div class=""detail-row d-flex justify-content-between"">
                        <div class=""field-title"">
                            Credits
                        </div>
                        <div class=""field-value"">
                            {{LearningClass.LearningCourse.Credits}}
                        </div>
                    </div>
                {% endif %}
        
                <div class=""detail-row d-flex justify-content-between"">
                    <div class=""field-title"">
                        Grading System
                    </div>
                    <div class=""field-value"">
                        {{LearningClass.LearningGradingSystem.Name}}
                    </div>
                </div>
        
                <div class=""detail-row d-flex justify-content-between"">
                    <div class=""field-title"">
                        Semester
                    </div>
                    <div class=""field-value"">
                        {{LearningClass.LearningSemester.Name}}
                    </div>
                </div>
        
                {% if hasLocation %}
                    <div class=""detail-row d-flex justify-content-between"">
                        <div class=""field-title"">
                            Location
                        </div>
                        <div class=""field-value"">
                            {{location.Name}}
                        </div>
                    </div>
                {% endif %}
        
                {% if hasSchedule %}
                    <div class=""detail-row d-flex justify-content-between"">
                        <div class=""field-title"">
                            Schedule
                        </div>
                        <div class=""field-value"">
                            {{schedule.Name}}
                        </div>
                    </div>
                {% endif %}
        
                {% if LearningClass.LearningSemester.StartDate %}
                    <div class=""detail-row d-flex justify-content-between"">
                        <div class=""field-title"">
                            Starts
                        </div>
                        <div class=""field-value"">
                            {{LearningClass.LearningSemester.StartDate |  Date:'sd' }}
                        </div>
                    </div>
                {% endif %}
            </div>
        </div>
    </div>

</div>

";
            public const string CompletionLavaTemplate = @"
<div class=""completion-container d-flex flex-column justify-content-center my-5"">
    <i class=""fa fa-check-circle fa-4x text-success text-center""></i>
    <h3 class=""completion-header text-center"">Successfully Enrolled!</h3>
    <div class=""completion-sub-header text-center"">
        You are now enrolled in this class.
        Click “Go to Class Workspace” to begin your learning experience.
    </div>
</div>
";
            public const string EnrollmentErrorLavaTemplate = @"
<div class=""error-container d-flex flex-column justify-content-center my-5"">
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

            var classWorkspaceParams = new Dictionary<string, string>
            {
                ["LearningProgramId"] = learningClass.LearningCourse.LearningProgram.IdKey,
                ["LearningCourseId"] = learningClass.LearningCourse.IdKey,
                ["LearningClassId"] = learningClass.IdKey
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.CourseDetailPage] = this.GetLinkedPageUrl( AttributeKey.CourseDetailPage, courseDetailParams ),
                [NavigationUrlKey.CourseListPage] = this.GetLinkedPageUrl( AttributeKey.CourseListPage, courseListParams ),
                [NavigationUrlKey.ClassWorkspacePage] = this.GetLinkedPageUrl( AttributeKey.ClassWorkspacePage, classWorkspaceParams )
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
            var mergeFields = GetMergeFields( currentPerson, registrant, out var learningClass, out var unmetRequirements );
            box.NavigationUrls = GetBoxNavigationUrls( learningClass );

            if ( learningClass.Id == 0 )
            {
                box.ErrorMessage = "It seems the course information is missing. Please select a course and try enrolling again.";
                return;
            }

            box.HeaderHtml = HeaderLavaTemplate.ResolveMergeFields( mergeFields );

            if ( !new LearningParticipantService( RockContext ).CanEnroll( learningClass, registrant, unmetRequirements, out var errorMessage ) )
            {
                mergeFields.Add( "ErrorKey", errorMessage );
            }

            // If already enrolled show the completion rather than the confirmation screen.
            var alreadyEnrolledErrorKey = "already_enrolled";
            if ( errorMessage.Equals( alreadyEnrolledErrorKey, StringComparison.OrdinalIgnoreCase ) )
            {
                box.CompletionHtml = CompletionLavaTemplate.ResolveMergeFields( mergeFields );
            }
            else
            {
                box.ConfirmationHtml = ConfirmationLavaTemplate.ResolveMergeFields( mergeFields );
            }
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
            var parameterValue = PageParameter( PageParameterKey.LearningClassId );
            var learningClassId = learningClassService.GetSelect( parameterValue, c => c.Id );
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
            var learningParticipantService = new LearningParticipantService( RockContext );

            try
            {
                if (!learningParticipantService.CanEnroll( learningClass, registrant, unmetRequirements, out var errorMessage ) )
                {
                    mergeFields.Add( "ErrorKey", errorMessage);
                    return ActionBadRequest( EnrollmentErrorLavaTemplate.ResolveMergeFields( mergeFields ) );
                }

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

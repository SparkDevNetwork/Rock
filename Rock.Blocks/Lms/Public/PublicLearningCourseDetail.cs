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

using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Cms.StructuredContent;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks.Lms.PublicLearningCourseDetail;
using Rock.Web;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Lms
{
    /// <summary>
    /// Displays the details for a public learning course.
    /// </summary>

    [DisplayName( "Public Learning Course Detail" )]
    [Category( "LMS" )]
    [Description( "Displays the details of a particular public learning course." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]


    [CodeEditorField( "Lava Template",
        Key = AttributeKey.CourseDetailTemplate,
        Description = "The Lava template to use to render the page. Merge fields include: Course, Program, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = false,
        DefaultValue = AttributeDefault.CourseDetailTemplate,
        Order = 1 )]

    [LinkedPage( "Workspace Page",
        Description = "Page to link to when the individual would like more details.",
        Key = AttributeKey.ClassWorkspacePage,
        Order = 2 )]

    [SlidingDateRangeField( "Next Session Date Range",
        Description = "Filter to limit the display of upcoming sessions.",
        Order = 3,
        IsRequired = false,
        Key = AttributeKey.NextSessionDateRange )]

    [LinkedPage( "Enrollment Page",
        Description = "The page that will enroll the student in the course.",
        Key = AttributeKey.CourseEnrollmentPage,
        Order = 4 )]

    [BooleanField(
        "Public Only",
        Description = "If selected, all non-public classes will be excluded.",
        IsRequired = false,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = AttributeKey.PublicOnly,
        Order = 5 )]

    [Rock.SystemGuid.EntityTypeGuid( "c5d5a151-038e-4295-a03c-63196883f68e" )]
    [Rock.SystemGuid.BlockTypeGuid( "b0dce130-0c91-4aa0-8161-57e8fa523392" )]
    public class PublicLearningCourseDetail : RockBlockType, IBreadCrumbBlock
    {
        #region Keys

        private static class AttributeKey
        {
            public const string CourseEnrollmentPage = "CourseEnrollmentPage";
            public const string CourseDetailTemplate = "CourseListTemplate";
            public const string ClassWorkspacePage = "DetailPage";
            public const string NextSessionDateRange = "NextSessionDateRange";
            public const string PublicOnly = "PublicOnly";
            public const string ShowCompletionStatus = "ShowCompletionStatus";
        }

        private static class PageParameterKey
        {
            public const string LearningProgramId = "LearningProgramId";
            public const string LearningCourseId = "LearningCourseId";
        }

        private static class AttributeDefault
        {
            public const string CourseDetailTemplate = @"
//- Variable Assignments
{% assign requirementTypes = Course.CourseRequirements | Distinct:'RequirementType' %}
{% assign prerequisitesText = Course.CourseRequirements | Where:'RequirementType','Prerequisite' |
Select:'RequiredLearningCourse' | Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
{% assign facilitatorCount = Course.Facilitators | Size %}
{% assign facilitators = Course.Facilitators | Join:', ' | ReplaceLast:',',' and' | Default:'TBD' %}


//- Styles

<style>

    @media (max-width: 991px) {
        .course-side-panel {
            padding-left: 0;
        }
        .card {
            margin-bottom: 24px;
        }
        
    }
    
    @media (max-width: 767px) {
        h1 {
            font-size: 28px;
        }
        .card {
            margin-bottom: 24px;
        }
    }

</style>


<div class=""d-flex flex-column gap-4"">
    
    <div class=""hero-section"">
        <div class=""hero-section-image"" style=""background-image: url('/GetImage.ashx?guid={{ Course.Entity.ImageBinaryFile.Guid }}')""></div>
        <div class=""hero-section-content"">
            <h1 class=""hero-section-title""> {{ Course.Entity.PublicName }} </h1>
            <p class=""hero-section-description""> {{ Course.Entity.Summary }} </p>
        </div>
    </div>

    <div>

        <div class=""row"">

            <div class=""col-xs-12 col-sm-12 col-md-8""> //- LEFT CONTAINER

                <div class=""card rounded-lg""> //-COURSE DESCRIPTION

                    <div class=""card-body"">
                        <div class=""card-title"">
                            <h4 class=""m-0"">Course Description</h4>
                        </div>
                        <div class=""card-text"">
                            {% if Course.Entity.CourseCode != empty %}
                            <div class=""text-gray-600"">
                                <span class=""text-bold"">Course Code: </span>
                                <span>{{Course.Entity.CourseCode}}</span>
                            </div>
                            {% endif %}

                            <div class=""text-gray-600"">
                                <span class=""text-bold"">Credits: </span>
                                <span>{{Course.Entity.Credits}}</span>
                            </div>

                            <div class=""pb-3 text-gray-600"""">
                                <span class=""text-bold"">Prerequisites: </span>
                                <span>
                                    {{ prerequisitesText }}
                                </span>
                            </div>

                            <div class=""pt-3 border-top border-gray-200"">
                                <span>{{Course.DescriptionAsHtml}}</span>
                            </div>
                        </div>
                    </div>

                </div>

            </div>


            <div class=""col-xs-12 col-sm-12 col-md-4""> //- RIGHT CONTAINER

                <div>

                    {% case Course.LearningCompletionStatus %}

                        {% when 'Incomplete' %}
                        <div class=""card rounded-lg"">
                            <div class=""card-body"">
                                <div class=""card-title d-flex align-items-center"">
                                    <i class=""fa fa-user-check mr-2""></i>
                                    <h4 class=""m-0"">Currently Enrolled</h4>
                                </div>
                                <div class=""card-text text-muted"">
                                    <p>You are currently enrolled in this course.</p>
                                </div>
                                <div class=""mt-3"">
                                    <a class=""btn btn-info"" href=""{{ Course.ClassWorkspaceLink }}"">View Course Workspace</a>
                                </div>
                            </div>
                            
                        </div>


                        {% when 'Pass' or 'Fail' %}
                            {% if CanShowHistoricalAccess == true %}

                            <div class=""card rounded-lg"">
                                
                                <div class=""card-body"">
                                    <div class=""card-title d-flex align-items-center"">
                                        <i class=""fa fa-rotate-left mr-2""></i>
                                        <h4 class=""m-0"">History</h4>
                                    </div>
                                    <div class=""text-muted"">You completed this class on {{
                                        Course.MostRecentParticipation.LearningCompletionDateTime | Date: 'MMMM dd, yyyy' }}</div>
            
                                    <div class=""mt-3"">
                                        <a href=""{{ Course.ClassWorkspaceLink }}"">View Class Work</a>
                                    </div>
                                </div>
                                
                            </div>
                            
                            {% else %}
                            
                            <div class=""card rounded-lg"">
                                
                                <div class=""card-body"">
                                    <div class=""card-title d-flex align-items-center"">
                                        <i class=""fa fa-rotate-left mr-2""></i>
                                        <h4 class=""m-0"">History</h4>
                                    </div>
                                    <div class=""text-muted"">You completed this class on {{
                                        Course.MostRecentParticipation.LearningCompletionDateTime | Date: 'MMMM dd, yyyy' }}</div>
            
                                    <div class=""mt-3"">
                                        <div class=""text-muted"">
                                            <p class=""text-bold mb-0"">Grade</p>
                                            <p class=""mb-0"">{{ Course.MostRecentParticipation.LearningGradingSystemScale.Name }}</p>
                                        </div>
                                    </div>
                                </div>
                                
                            </div>
                            
                            
                            {% endif %}

                        {% else %}

                        <div class=""card rounded-lg"">

                            <div class=""card-body"">
                                <div class=""card-title d-flex align-items-center"">
                                    <i class=""fa fa-calendar-alt mr-2""></i>
                                    <h4 class=""m-0"">Upcoming Schedule</h4>
                                </div>

                                <div class=""card-text d-flex flex-column gap-1"">
                                    {% if Course.Program.ConfigurationMode == ""AcademicCalendar"" %}
                                    <div class=""text-muted"">
                                        <p class=""text-bold mb-0"">Next Session Semester: </p>
                                        {% if Course.NextSemester.Name %}
                                        <p>{{ Course.NextSemester.Name }}</p>
                                        {% else %}
                                        <p>TBD</p>
                                        {% endif %}
                                    </div>
                                    {% endif %}

                                    <div class=""text-muted"">
                                        <p class=""text-bold mb-0"">{{ 'Facilitator' | PluralizeForQuantity:facilitatorCount
                                            }}:</p>
                                        <p>{{ facilitators }}</p>
                                    </div>

                                    {% for requirementType in requirementTypes %}

                                    {% assign requirementsText = Course.CourseRequirements |
                                    Where:'RequirementType',requirementType | Select:'RequiredLearningCourse' |
                                    Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
                                    <div class=""text-muted"">
                                        <div class=""d-flex align-items-center"">
                                            <p class=""text-bold mb-0"">{{ requirementType | Pluralize }}</p>
                                            {% if Course.UnmetPrerequisites %}
                                            <i class=""fa fa-check-circle text-success ml-2""></i>
                                            {% else %}
                                            <i class=""fa fa-exclamation-circle text-danger ml-2""></i>
                                            {% endif %}
                                        </div>
                                        <p>{{ requirementsText }}</p>
                                    </div>

                                    {% endfor %}

                                    <div class=""mt-3"">

                                        {% if ErrorKey == 'enrollment_closed' or ErrorKey == 'class_full' %}
                                            <p class=""text-danger"">Enrollment is closed for this class.</p>
                                            {% else %}
                                                <a class=""btn btn-info"" href=""{{ Course.CourseEnrollmentLink }}"">Enroll</a>
                                        {% endif %}

                                    </div>

                                </div>
                            </div>
                        </div>

                    {% endcase %}

                </div>
            </div>
        </div>
    </div>
</div>
";
        }

        #endregion Keys

        /// <summary>
        /// Whether the ShowCompletionStatus block setting is configured to "Show".
        /// </summary>
        /// <returns><c>true</c> if the completion status should be shown; otherwise <c>false</c>.</returns>
        private bool ShowCompletionStatus => GetAttributeValue( AttributeKey.ShowCompletionStatus ) == "Show";

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new PublicLearningCourseDetailBlockBox();

            SetBoxInitialEntityState( box );

            return box;
        }

        /// <inheritdoc/>
        protected override string GetInitialHtmlContent()
        {
            return GetHtmlContent( true );
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( PublicLearningCourseDetailBlockBox box )
        {
            box.CourseHtml = GetHtmlContent( false );
        }

        /// <summary>
        /// Gets the HTML content for the block.
        /// </summary>
        /// <param name="includeNextSessionFiltering">Optional filter to restrict courses to those within the block setting's "Next Session Date Range".</param>
        /// <returns>The resolved lava template.</returns>
        private string GetHtmlContent( bool includeNextSessionFiltering = false )
        {
            var courseId = RequestContext.PageParameterAsId( PageParameterKey.LearningCourseId );
            var currentPerson = GetCurrentPerson();
            var learningCourseService = new LearningCourseService( RockContext );
            var semesterDateRange = includeNextSessionFiltering ?
                RockDateTimeHelper.CalculateDateRangeFromDelimitedValues( GetAttributeValue( AttributeKey.NextSessionDateRange ), RockDateTime.Now ) :
                null;

            var publicOnly = GetAttributeValue( AttributeKey.PublicOnly ).AsBoolean();

            var course = includeNextSessionFiltering ?
                learningCourseService.GetPublicCourseDetails( courseId, currentPerson?.Id, publicOnly, semesterDateRange.Start, semesterDateRange.End ) :
                learningCourseService.GetPublicCourseDetails( courseId, currentPerson?.Id, publicOnly );

            course.DescriptionAsHtml = new StructuredContentHelper( course.Entity?.Description ?? string.Empty ).Render();

            var enrolledOrNextClassIdKey = course.MostRecentParticipation?.LearningClassId > 0 ?
                IdHasher.Instance.GetHash( course.MostRecentParticipation.LearningClassId ) :
                course.NextSemester?.LearningClasses?.FirstOrDefault()?.IdKey;
            var queryParams = new Dictionary<string, string>
            {
                [PageParameterKey.LearningProgramId] = course.Program.IdKey,
                [PageParameterKey.LearningCourseId] = course.Entity.IdKey,
                ["LearningClassId"] = enrolledOrNextClassIdKey
            };

            course.ClassWorkspaceLink = this.GetLinkedPageUrl( AttributeKey.ClassWorkspacePage, queryParams );
            course.CourseEnrollmentLink = this.GetLinkedPageUrl( AttributeKey.CourseEnrollmentPage, queryParams );

            var mergeFields = this.RequestContext.GetCommonMergeFields( currentPerson );
            mergeFields.Add( "Course", course );
            mergeFields.Add( "ShowCompletionStatus", ShowCompletionStatus );

            if ( currentPerson != null )
            {
                var learningClassService = new LearningClassService( RockContext );
                var learningClassId = learningClassService.GetSelect( enrolledOrNextClassIdKey, c => c.Id );

                var learningClass =
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

                // Allow historical access if the course allows it and the class is not over.
                var hasSemester = learningClass.LearningSemester != null;
                var canShowHistoricalAccess = learningClass.LearningCourse.AllowHistoricalAccess
                    && hasSemester
                    && ( !learningClass.LearningSemester.EndDate.HasValue
                    || learningClass.LearningSemester.EndDate.Value.IsFuture() );

                mergeFields.Add( "CanShowHistoricalAccess", canShowHistoricalAccess );

                var unmetRequirements =
                learningClassId == 0 ?
                default :
                new LearningCourseService( RockContext )
                .GetUnmetCourseRequirements( currentPerson.Id, learningClass.LearningCourse.LearningCourseRequirements );

                if ( !new LearningParticipantService( RockContext ).CanEnroll( learningClass, currentPerson, unmetRequirements, out var errorMessage ) )
                {
                    // Don't treat already enrolled as an error for this block.
                    // They should be able to navigate to the workspace from here.
                    var alreadyEnrolledErrorKey = "already_enrolled";
                    if ( !errorMessage.Equals( alreadyEnrolledErrorKey, System.StringComparison.OrdinalIgnoreCase ) )
                    {
                        mergeFields.Add( "ErrorKey", errorMessage );
                    }
                }
            }

            var template = GetAttributeValue( AttributeKey.CourseDetailTemplate ) ?? string.Empty;
            return template.ResolveMergeFields( mergeFields );
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var entityKey = pageReference.GetPageParameter( PageParameterKey.LearningCourseId ) ?? "";
            var entityName = entityKey.Length > 0 ? new LearningCourseService( RockContext ).GetSelect( entityKey, p => p.Name ) : "New Course";

            // Include only the parameters necessary to construct the breadcrumb
            // (prevent unused/unnecessary query string parameters). 
            var includedParamKeys = new[] { "learningprogramid", "learningcourseid" };
            var paramsToInclude = pageReference.Parameters.Where( kv => includedParamKeys.Contains( kv.Key.ToLower() ) ).ToDictionary( kv => kv.Key, kv => kv.Value );

            var breadCrumbPageRef = new PageReference( pageReference.PageId, pageReference.RouteId, paramsToInclude );
            var breadCrumb = new BreadCrumbLink( entityName ?? "Course Description", breadCrumbPageRef );

            return new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb>
                {
                    breadCrumb
                }
            };
        }

        #endregion
    }
}

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
using System.Linq;

using Rock.Attribute;
using Rock.Cms.StructuredContent;
using Rock.Model;
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
        Description = "The Lava template to use to render the page. Merge fields include: CourseInfo, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>",
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
        Description = "Filter to limit the display of upcoming sessions based on Start Date.",
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
            public const string CourseDetailTemplate = "CourseDetailTemplate";
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
        <div class=""hero-section-image"" style=""background-image: url('/GetImage.ashx?guid={{ CourseInfo.ImageFileGuid}}')""></div>
        <div class=""hero-section-content"">
            <h1 class=""hero-section-title""> {{ CourseInfo.PublicName }} </h1>
            <p class=""hero-section-description""> {{ CourseInfo.Summary }} </p>
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
                            {% if CourseInfo.CourseCode != empty %}
                            <div class=""text-gray-600"">
                                <span class=""text-bold"">Course Code: </span>
                                <span>{{CourseInfo.CourseCode}}</span>
                            </div>
                            {% endif %}

                            <div class=""text-gray-600"">
                                <span class=""text-bold"">Credits: </span>
                                <span>{{CourseInfo.Credits}}</span>
                            </div>

                            <div class=""pb-3 text-gray-600"">
                                {% if CourseInfo.CourseRequirements != empty %}
                                        {% assign requirementsText = CourseInfo.CourseRequirements |
                                        Where:'RequirementType','Prerequisite' | Select:'RequiredLearningCourse' |
                                        Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
                                        <div class=""text-gray-600"">
                                            <div class=""d-flex align-items-center"">
                                                <p class=""text-bold mb-0"">Prerequisites: <span class=""text-normal"">{{requirementsText}}</span></p>
                                                {% if CourseInfo.UnmetPrerequisites != empty %}
                                                    <i class=""fa fa-exclamation-circle text-danger ml-2""></i>
                                                {% else %}
                                                    <i class=""fa fa-check-circle text-success ml-2""></i>
                                                {% endif %}
                                            </div>
                                            
                                        </div>
                                {% endif %}
                            </div>

                            <div class=""pt-3 border-top border-gray-200"">
                                <span>{{CourseInfo.DescriptionAsHtml}}</span>
                            </div>
                        </div>
                    </div>

                </div>

            </div>

            <div class=""col-xs-12 col-sm-12 col-md-4""> //- RIGHT CONTAINER
                {% assign hasClasses = false %}
                {% for semesterInfo in CourseInfo.Semesters %}
                    {% for classInfo in semesterInfo.AvailableClasses %}
                        {% assign hasClasses = true %}
						
						//-CURRENTLY ENROLLED
						{% if classInfo.IsEnrolled %} 
							<div class=""card rounded-lg mb-4"">
								<div class=""card-body"">
									<div class=""card-title d-flex align-items-center"">
										<i class=""fa fa-user-check mr-2""></i>
										<h4 class=""m-0"">Currently Enrolled</h4>
									</div>
									<div class=""card-text text-muted"">
										<p>You are currently enrolled in this course.</p>
									</div>
									<div>
										<a class=""btn btn-info"" href=""{{ classInfo.WorkspaceLink }}"">View Class Workspace</a>
									</div>
								</div>
							</div>
                        {% elseif classInfo.StudentParticipant and classInfo.StudentParticipant.LearningCompletionStatus != ""Incomplete"" %}
							
                            //- HISTORICAL ACCESS
                            {% if CourseInfo.AllowHistoricalAccess == true %}
                            
                                <div class=""card rounded-lg mb-4"">
                                    
                                    <div class=""card-body"">
                                        <div class=""card-title d-flex align-items-center"">
                                            <i class=""fa fa-rotate-left mr-2""></i>
                                            <h4 class=""m-0"">History</h4>
                                        </div>
                                        <div class=""text-muted"">You completed this class on {{
                                            classInfo.StudentParticipant.LearningCompletionDateTime | Date: 'sd' }}</div>
                            
                                        <div>
                                            <a href=""{{ classInfo.WorkspaceLink }}"">View Class Workspace</a>
                                        </div>
                                    </div>
                                    
                                </div>
							 
							//- NO HISTORICAL ACCESS
							{% else %}
                                
                                <div class=""card rounded-lg mb-4"">
                                    
                                    <div class=""card-body"">
                                        <div class=""card-title d-flex align-items-center"">
                                            <i class=""fa fa-rotate-left mr-2""></i>
                                            <h4 class=""m-0"">History</h4>
                                        </div>
                                        <div class=""text-muted"">You completed this class on {{
                                            classInfo.StudentParticipant.LearningCompletionDateTime | Date: 'sd' }}</div>
                            
                                        <div class=""mt-3"">
                                            <div class=""text-muted"">
                                                <p class=""text-bold mb-0"">Grade</p>
                                                <p class=""mb-0"">{{ classInfo.StudentParticipant.LearningGradingSystemScale.Name }}</p>
                                            </div>
                                        </div>
                                    </div>
                                    
                                </div>
                                
                            {% endif %}
                        {% endif %}
                    {% endfor %}
                {% endfor %}
                                
                {% if hasClasses == false %} //-NO UPCOMING SEMESTERS OR CLASSES
                    <div class=""card rounded-lg"">
                        <div class=""card-body"">
                            <h4 class=""mt-0"">No Available Upcoming Classes</h4>
                            <p class=""text-gray-600"">Please check back again later.</p>
                        </div>
                    </div>
                {% else %}
                    <div class=""card rounded-lg"">
                        <div class=""card-body"">
                            <h4 class=""card-title mt-0""><i class=""fa fa-chalkboard-teacher mr-2""></i>Classes</h4>
                            
                            //-SCOPING TO CLASS DETAILS
                            
                            {% for semesterInfo in CourseInfo.Semesters %}
                                {% assign semesterStartDate = semesterInfo.StartDate | Date: 'sd' %}
                                {% assign semesterEndDate = semesterInfo.EndDate | Date: 'sd' %}
                                {% if CourseInfo.ProgramInfo.ConfigurationMode == ""AcademicCalendar"" %}
                                    <div>
                                        <h5 class=""mt-0 mb-0"">{{semesterInfo.Name}}</h5>
                                        {% if semesterStartDate and semesterEndDate %}
                                            <p class=""text-gray-600"">{{semesterStartDate}}-{{semesterEndDate}}</p>
                                        {% elseif semesterEndDate != empty %}
                                            <p class=""text-gray-600"">{{semesterStartDate}}</p>
                                        {% else %}
                                            <p class=""text-gray-600"">Date Pending</p>
                                        {% endif %}
                                    </div>
                                {% endif %}
                                        
                                        {% for classInfo in semesterInfo.AvailableClasses %}
                                            {% assign facilitatorsText = classInfo.Facilitators | Select:'Name' | Join:', ' | ReplaceLast:',',' and' | Default:'TBD' %}
                                           
                                            <div class=""card rounded-lg bg-gray-100 mb-4"">
                                                <div class=""card-body pb-0"">
                                                    <div class=""d-grid grid-flow-row gap-0 mb-3""> //-BEGIN CLASS DETAILS
                                                        
                                                        {% if CourseInfo.ProgramInfo.ConfigurationMode == ""AcademicCalendar"" %}
                                                            <p class=""text-bold"">{{classInfo.Entity.Name}}
                                                            {% if classInfo.IsEnrolled %}
                                                                <span class=""text-normal"">(Enrolled)</span>
                                                            {% endif %}
                                                            </p>
                                                        {% else %}
                                                            <h4 class=""mt-0"">Class Details</h4>
                                                        {% endif %}
                                                        
                                                        <div class=""d-flex flex-column"">
                                                            {% if facilitatorsText %}
                                                                <div class=""text-gray-600"">
                                                                    <p class=""text-bold mb-0"">Facilitators: 
                                                                    <p>{{facilitatorsText}}</p>
                                                                </div>
                                                            {% endif %}
                                                            
                                                            {% if classInfo.Campus %}
                                                                <div class=""text-gray-600"">
                                                                    <p class=""text-bold mb-0"">Campus: 
                                                                    <p>{{classInfo.Campus}}</p>
                                                                </div>
                                                            {% endif %}
                                                            {% if classInfo.Location %}
                                                                <div class=""text-gray-600"">
                                                                    <p class=""text-bold mb-0"">Location: 
                                                                    <p>{{classInfo.Location}}</p>
                                                                </div>
                                                            {% endif %}
                                                            {% if classInfo.Schedule %}
                                                                <div class=""text-gray-600"">
                                                                    <p class=""text-bold mb-0"">Schedule: 
                                                                    <p>{{classInfo.Schedule}}</p>
                                                                </div>
                                                            {% endif %}
                                                            
                                                            {% if classInfo.StudentParticipant and classInfo.StudentParticipant.LearningCompletionStatus == ""Incomplete"" %}
                                                                
                                                                {% if semesterInfo.IsEnrolled and classInfo.IsEnrolled == false %}
                                                                    <p class=""text-danger"">You've already enrolled in a class this semester.</p>
                                                                {% endif %}
                                                            
                                                            {% else %}
                                                            
                                                                {% if classInfo.CanEnroll == false %}
                                                                    {% if classInfo.EnrollmentErrorKey == 'class_full' %}
                                                                        <p class=""text-danger"">Class is full.</p>
                                                                    {% endif %}
                                                                
                                                                {% else %}
                                                                    {% if CourseInfo.ProgramInfo.ConfigurationMode != ""AcademicCalendar"" or semesterInfo.IsEnrolled  == false %}
                                                                        <div>
                                                                            <a class=""btn btn-default"" href=""{{ classInfo.EnrollmentLink }}"">Enroll</a>
                                                                        </div>
                                                                    {% endif %}
                                                                {% endif %}
                                                            {% endif %}
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        {% endfor %}
                                    
                            {% endfor %}
                        
                        </div>
                    </div>        
                    
                {% endif %}

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
            return GetHtmlContent();
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( PublicLearningCourseDetailBlockBox box )
        {
            box.CourseHtml = GetHtmlContent();
        }

        /// <summary>
        /// Gets the HTML content for the block.
        /// </summary>
        /// <param name="includeNextSessionFiltering">Optional filter to restrict courses to those within the block setting's "Next Session Date Range".</param>
        /// <returns>The resolved lava template.</returns>
        private string GetHtmlContent()
        {
            var courseId = RequestContext.PageParameterAsId( PageParameterKey.LearningCourseId );
            var currentPerson = GetCurrentPerson();
            var learningCourseService = new LearningCourseService( RockContext );
            var semesterDateRange = RockDateTimeHelper.CalculateDateRangeFromDelimitedValues(
                GetAttributeValue( AttributeKey.NextSessionDateRange ), RockDateTime.Now );

            var publicOnly = GetAttributeValue( AttributeKey.PublicOnly ).AsBoolean();

            var course = learningCourseService.GetPublicCourseDetails(
                courseId,
                currentPerson,
                publicOnly,
                semesterDateRange.Start,
                semesterDateRange.End );

            course.DescriptionAsHtml = new StructuredContentHelper( course.Description ?? string.Empty ).Render();

            AddClassSpecificProperties( course );

            var mergeFields = this.RequestContext.GetCommonMergeFields( currentPerson );
            mergeFields.Add( "CourseInfo", course );
            mergeFields.Add( "ShowCompletionStatus", ShowCompletionStatus );

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
            var paramsToInclude = pageReference.Parameters
                .Where( kv => includedParamKeys.Contains( kv.Key.ToLower() ) )
                .ToDictionary( kv => kv.Key, kv => kv.Value );

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

        /// <summary>
        /// Adds class specific properties that require configuration from the block.
        /// </summary>
        /// <param name="course"></param>
        private void AddClassSpecificProperties( LearningCourseService.PublicLearningCourseBag course )
        {
            foreach ( var semester in course.Semesters )
            {
                foreach ( var availableClass in semester.AvailableClasses )
                {
                    var queryParams = new Dictionary<string, string>
                    {
                        [PageParameterKey.LearningProgramId] = course.ProgramInfo.IdKey,
                        [PageParameterKey.LearningCourseId] = course.IdKey,
                        ["LearningClassId"] = availableClass.IdKey
                    };

                    availableClass.EnrollmentLink = this.GetLinkedPageUrl( AttributeKey.CourseEnrollmentPage, queryParams );
                    availableClass.WorkspaceLink = this.GetLinkedPageUrl( AttributeKey.ClassWorkspacePage, queryParams );
                }
            }
        }

        #endregion
    }
}

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
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.Lms.PublicLearningCourseList;
using Rock.Web;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Lms
{
    /// <summary>
    /// Displays a list of public learning courses.
    /// </summary>
    [DisplayName( "Public Learning Course List" )]
    [Category( "LMS" )]
    [Description( "Displays a list of public learning courses." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [CodeEditorField( "Lava Template",
        Key = AttributeKey.LavaTemplate,
        Description = "The Lava template to use to render the page. Merge fields include: Program, Courses, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = false,
        DefaultValue = AttributeDefault.CourseListTemplate,
        Order = 1 )]

    [LinkedPage( "Detail Page",
        Description = "Page to link to when the individual would like more details.",
        Key = AttributeKey.DetailPage,
        Order = 2 )]

    [CustomDropdownListField(
        "Show Completion Status",
        Key = AttributeKey.ShowCompletionStatus,
        Description = "Determines if the individual's completion status should be shown.",
        ListSource = "Show,Hide",
        IsRequired = true,
        DefaultValue = "Show",
        Order = 3 )]

    [SlidingDateRangeField( "Next Session Date Range",
        Description = "Filter to limit the display of upcoming sessions.",
        Order = 4,
        IsRequired = false,
        Key = AttributeKey.NextSessionDateRange )]

    [BooleanField(
        "Public Only",
        Description = "If selected, all non-public courses will be excluded.",
        IsRequired = false,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = AttributeKey.PublicOnly,
        Order = 5 )]

    [Rock.SystemGuid.EntityTypeGuid( "4356febe-5efd-421a-bfc4-05942b6bd910" )]
    [Rock.SystemGuid.BlockTypeGuid( "5d6ba94f-342a-4ec1-b024-fc5046ffe14d" )]
    public class PublicLearningCourseList : RockBlockType, IBreadCrumbBlock
    {
        #region Keys

        private static class AttributeKey
        {
            public const string CourseEnrollmentPage = "CourseEnrollmentPage";
            public const string LavaTemplate = "LavaTemplate";
            public const string DetailPage = "DetailPage";
            public const string NextSessionDateRange = "NextSessionDateRange";
            public const string PublicOnly = "PublicOnly";
            public const string ShowCompletionStatus = "ShowCompletionStatus";
        }

        private static class PageParameterKey
        {
            public const string LearningProgramId = "LearningProgramId";
        }

        private static class AttributeDefault
        {
            public const string CourseListTemplate = @"
//- Styles
<style>
    
    .lms-grid {
        display: grid;
        grid-template-columns: 1fr 1fr 1fr;
        gap: 1.5rem;
    }
    
    .card-img-h {
        height: 180px
    }
    
    .card {
        display: grid;
        grid-row: auto / span 5;
        grid-template-rows: subgrid;
    }
    
    .credits {
        flex-shrink: 0;
        white-space: nowrap;
    }

    @media (max-width: 767px) {
        .lms-grid {
           grid-template-columns: 1fr; 
        }
        h1 {
            font-size: 28px;
        }
    }
    
    @media (min-width: 768px) and (max-width: 1023px) {
        .lms-grid {
           grid-template-columns: 1fr 1fr; 
        }
    }
    
</style>


<div>
	
	<div class=""hero-section"">
        <div class=""hero-section-image"" style=""background-image: url('/GetImage.ashx?guid={{ Program.ImageBinaryFile.Guid }}')""></div>
        <div class=""hero-section-content"">
            <h1 class=""hero-section-title""> {{ Program.Name }} </h1>
            <p class=""hero-section-description""> {{ Program.Summary }} </p>
        </div>
    </div>
    <div class=""center-block text-center mt-4 mb-4"">
        <div class=""d-flex flex-column gap-2"">
            <h3> Courses Available </h3>
            <p class=""text-muted""> The following training courses are available for enrollment. </p>
        </div>
    </div>
	
	<div> //- Main Body - Container for course grid

        <div class=""lms-grid""> //- Grid for Cards

            {% for course in Courses %}

            <div class=""card"">
                //-1 IMAGE
                {% if course.ImageFileGuid %}
                    <img src=""/GetImage.ashx?guid={{ course.ImageFileGuid }}"" class=""card-img-top card-img-h object-cover"" alt=""course image"" />
                        
                    {% else %}
                        <div class=""d-flex justify-content-center align-items-center card-img-top card-img-h"">
                            <i class=""fa fa-image fa-2x text-gray-200""></i>
                        </div>
                        
                {% endif %}
                
                //- 2 CARD HEADER

                    <div class=""card-body d-flex justify-content-between align-items-start pt-0 pb-0"">
                        <h4 class=""card-title mb-0"">{{ course.Entity.PublicName }}</h4>
                        {% if course.Entity.Credits > 0 %}
        			        <div class=""d-flex w-auto"">
        			            <p class=""credits w-auto text-muted mb-0"">Credits: {{ course.Entity.Credits }}</p>
        			        </div>
    			        {% endif %}
                    </div>
                

                
                //- 3 BODY TEXT
                <div class=""card-body pt-0 pb-0"">

                    <p class=""line-clamp-3"">
                        {{ course.Entity.Summary }}    
                    </p>
                </div>
                
                //- 4 CATEGORY
			        <div class=""card-body pt-0 pb-0"">
			            <div class=""badge badge-default"">{{ course.Category }}</div>
            		</div>
                
                //-5 FOOTER
                <div class=""card-footer bg-transparent d-flex justify-content-between"">
                    <a href=""{{ course.CourseDetailsLink }}"" class=""btn btn-default"">Learn More</a>
                    
                    {% if ShowCompletionStatus %}
                                            
                        {% if course.LearningCompletionStatus == 'Incomplete' %}
                            <div class=""d-flex align-items-center"">
                                <h4 class=""m-0""><span class=""label label-warning"">Enrolled</span></h4>
                            </div>
                                
                            {% elseif course.LearningCompletionStatus == 'Fail' %}
                            <div class=""d-flex align-items-center"">
                                <h4 class=""m-0""><span class=""label label-danger"">Failed</span></h4>
                            </div>
                            
                            {% elseif course.LearningCompletionStatus == 'Pass' %}
                                <div class=""d-flex align-items-center"">
                                    <h4 class=""m-0""><span class=""label label-success"">Passed</span></h4>
                                </div>
                        {% endif %}
    
                    {% endif %}
                </div>

            </div>
            
            {% endfor %}
            
        </div>
    </div>
	
</div>";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new PublicLearningCourseListBlockBox();

            SetBoxInitialEntityState( box );

            return box;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( PublicLearningCourseListBlockBox box )
        {
            box.CoursesHtml = GetInitialHtmlContent();
        }

        /// <summary>
        /// Provide html to the block for it's initial rendering.
        /// </summary>
        /// <returns>The HTML content to initially render.</returns>
        protected override string GetInitialHtmlContent()
        {
            var programId = RequestContext.PageParameterAsId( PageParameterKey.LearningProgramId );
            var program = new LearningProgramService( RockContext )
                .Queryable()
                .Include( p => p.ImageBinaryFile )
                .FirstOrDefault( p => p.Id == programId );

            var courses = GetCourses( programId, RockContext );

            var queryParams = new Dictionary<string, string>
            {
                [PageParameterKey.LearningProgramId] = PageParameter( PageParameterKey.LearningProgramId ),
                ["LearningCourseId"] = "((Key))"
            };

            var courseDetailUrlTemplate = this.GetLinkedPageUrl( AttributeKey.DetailPage, queryParams );
            var courseEnrollmentUrlTemplate = this.GetLinkedPageUrl( AttributeKey.CourseEnrollmentPage, queryParams );

            foreach ( var course in courses )
            {
                // If there are unmet requirements include the link for enrollment to that course.
                var prerequisiteCourseIdKey = course.UnmetPrerequisites?.FirstOrDefault()?.IdKey ?? string.Empty;
                course.CourseDetailsLink = courseDetailUrlTemplate.Replace( "((Key))", course.Entity.IdKey );
                course.PrerequisiteEnrollmentLink = courseEnrollmentUrlTemplate.Replace( "((Key))", prerequisiteCourseIdKey );
                course.CourseEnrollmentLink = courseEnrollmentUrlTemplate.Replace( "((Key))", course.Entity.IdKey );
            }

            var mergeFields = this.RequestContext.GetCommonMergeFields();
            mergeFields.Add( "Program", program );
            mergeFields.Add( "Courses", courses );
            mergeFields.Add( "ShowCompletionStatus", ShowCompletionStatus() );

            var template = GetAttributeValue( AttributeKey.LavaTemplate ) ?? string.Empty;
            return template.ResolveMergeFields( mergeFields );
        }

        private bool ShowCompletionStatus()
        {
            return GetAttributeValue( AttributeKey.ShowCompletionStatus ) == "Show";
        }

        private List<Rock.Model.LearningCourseService.PublicLearningCourseBag> GetCourses( int programId, RockContext rockContext )
        {
            var publicOnly = GetAttributeValue( AttributeKey.PublicOnly ).AsBoolean();
            var semesterDates = RockDateTimeHelper.CalculateDateRangeFromDelimitedValues( GetAttributeValue( AttributeKey.NextSessionDateRange ), RockDateTime.Now );

            return new LearningCourseService( rockContext ).GetPublicCourses( programId, GetCurrentPerson()?.Id, publicOnly, semesterDates.Start, semesterDates.End );
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbText = "Courses";

            // Include only the parameters necessary to construct the breadcrumb
            // (prevent unused/unnecessary query string parameters). 
            var includedParamKeys = new[] { "learningprogramid" };
            var paramsToInclude = pageReference.Parameters.Where( kv => includedParamKeys.Contains( kv.Key.ToLower() ) ).ToDictionary( kv => kv.Key, kv => kv.Value );

            var breadCrumbPageRef = new PageReference( pageReference.PageId, pageReference.RouteId, paramsToInclude );
            var breadCrumb = new BreadCrumbLink( breadCrumbText ?? "", breadCrumbPageRef );

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

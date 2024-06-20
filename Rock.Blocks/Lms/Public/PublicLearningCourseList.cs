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

    [LinkedPage( "Detail Page",
        Description = "The page that will show the course details.",
        Key = AttributeKey.DetailPage )]

    [LinkedPage( "Enrollment Page",
        Description = "The page that will enroll the student in the course.",
        Key = AttributeKey.CourseEnrollmentPage )]

    [CodeEditorField( "Course List Template",
        Key = AttributeKey.CourseListTemplate,
        Description = "The lava template showing the courses list. Merge fields include: Program, Courses, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = AttributeDefault.CourseListTemplate,
        Order = 1 )]
    [Rock.SystemGuid.EntityTypeGuid( "4356febe-5efd-421a-bfc4-05942b6bd910" )]
    [Rock.SystemGuid.BlockTypeGuid( "5d6ba94f-342a-4ec1-b024-fc5046ffe14d" )]
    public class PublicLearningCourseList : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string CourseEnrollmentPage = "CourseEnrollmentPage";
            public const string CourseListTemplate = "CourseListTemplate";
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string LearningProgramId = "LearningProgramId";
        }

        private static class AttributeDefault
        {
            public const string CourseListTemplate = @"
//- Variables
{% assign imageFileNameLength = Program.ImageBinaryFile.Guid | Size %}

//- Styles
{% stylesheet %}
    .page-container {
        display: flex;
        flex-direction: column;
        margin-bottom: 12px;
    }
    
    .page-header-section {
        {% if imageFileNameLength > 0 %}
            height: 280px; 
        {% endif %}
        align-items: center; 
        border-radius: 12px; 
        background-image: url('/GetImage.ashx?guid={{Program.ImageBinaryFile.Guid}}'); 
        background-size: cover;
    }
    
    .header-block {
        display: flex;
        flex-direction: column;
        position: relative;
        left: 10%;
        {% if imageFileNameLength > 0 %}
            bottom: -85%;
            -webkit-transform: translateY(-30%);
            transform: translateY(-30%);
        {% endif %}
        background-color: white; 
        border-radius: 12px; 
        width: 80%; 
    }
    
    .page-sub-header {
        padding-left: 10%; 
        padding-right: 10%; 
        padding-bottom: 12px;
        margin-bottom: 12px;
    }
    
    .course-item-container {
        max-width: 300px;
        background-color: white; 
        border-radius: 12px;
        margin: 8px;
        display: flex;
        flex-direction: column;
        justify-content: space-between;
    }
{% endstylesheet %}
<div class=""page-container"">
	<div class=""page-header-section mb-5"">
		<div class=""header-block text-center"">
			<h2>
				{{ Program.Name }}
			</h2>
			<div class=""page-sub-header"">
				{{ Program.Summary }}
			</div>
		</div>
	</div>
	
	<div class=""course-list-header-section center-block text-center mb-4"">
		<span class=""course-list-header h5"">
			Courses
		</span>
	</div>
	
	<div class=""course-list-container d-flex flex-fill"">
		{% for course in Courses %}
		<div class=""course-item-container"">
			<div class=""course-item-middle p-3"">
			
				<h4 class=""course-name"">
					{{ course.Entity.PublicName }}
				</h4>
				<div class=""course-category d-flex justify-content-between mb-2"">
				    {% if course.Category and course.Category <> '' %}
					    <span class=""badge badge-info"">{{ course.Category }}</span>
					{% else %}
					    <span> </span>
				    {% endif %}
				    {% if course.Entity.Credits %}
				        <span class=""badge"" style=""background-color: #ddedf2; color: #546a71;"">Credits: {{ course.Entity.Credits }}</span>
				    {% endif %}
				</div>
				<div class=""course-summary text-muted"">
					{{ course.Entity.Summary }} 
				</div>
			</div>
		
			<div class=""course-item-footer d-flex flex-column mt-4 p-3"">
                <div class=""course-next-session text-muted mb-3"">
                    <div class=""text-bold"">Next Session Starts</div>
                    <ul><li>{{ course.NextSemester.StartDate | Date:'MMMM dd, yyyy' }}</li></ul>
                </div>
                
                <div class=""d-flex justify-content-between"">
    				<a class=""btn btn-default"" href=""{{ course.CourseDetailsLink }}"">Learn More</a>
    				
    				{% if course.LearningCompletionStatus == 'Pass' %}
    					<span class=""badge badge-success p-2"" style=""line-height: normal;"">Completed</span>
    				{% elseif course.LearningCompletionStatus == 'Incomplete' %}
    					<span class=""badge badge-info p-2"" style=""line-height: normal;"">Enrolled</span>
    				{% elseif course.UnmetPrerequisites != empty %}
                        <a class=""text-muted"" href=""{{ course.PrerequisiteEnrollmentLink }}"">Prerequisites Not Met</a>
                    {% else %}
                        <a class=""text-bold ml-3"" href=""{{ course.CourseEnrollmentLink }}"">Enroll</a>
    				{% endif %}
			    </div>
			</div>
		</div>
		{% endfor %}
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
            var programId = PageParameterAsId( PageParameterKey.LearningProgramId );
            var rockContext = new RockContext();
            var program = new LearningProgramService( rockContext )
                .Queryable()
                .Include( p => p.ImageBinaryFile )
                .FirstOrDefault( p => p.Id == programId );

            var courses = new LearningCourseService( rockContext ).GetPublicCourses( programId, GetCurrentPerson().Id );

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

            var template = GetAttributeValue( AttributeKey.CourseListTemplate ) ?? string.Empty;
            box.CoursesHtml = template.ResolveMergeFields( mergeFields );
        }

        #endregion
    }
}

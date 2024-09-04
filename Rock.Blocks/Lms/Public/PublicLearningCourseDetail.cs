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
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks.Lms.PublicLearningCourseDetail;
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
        Description = "The lava template to use to render the page. Merge fields include: Course, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = AttributeDefault.CourseDetailTemplate,
        Order = 1 )]

    [LinkedPage( "Workspace Page",
        Description = "Page to link to when the individual would like more details.",
        Key = AttributeKey.ClassWorkspacePage,
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
        Description = "Filter to limit the display of upcming sessions.",
        Order = 4,
        IsRequired = false,
        Key = AttributeKey.NextSessionDateRange )]

    [LinkedPage( "Enrollment Page",
        Description = "The page that will enroll the student in the course.",
        Key = AttributeKey.CourseEnrollmentPage,
        Order = 5 )]

    [Rock.SystemGuid.EntityTypeGuid( "c5d5a151-038e-4295-a03c-63196883f68e" )]
    [Rock.SystemGuid.BlockTypeGuid( "b0dce130-0c91-4aa0-8161-57e8fa523392" )]
    public class PublicLearningCourseDetail : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string CourseEnrollmentPage = "CourseEnrollmentPage";
            public const string CourseDetailTemplate = "CourseListTemplate";
            public const string ClassWorkspacePage = "DetailPage";
            public const string NextSessionDateRange = "NextSessionDateRange";
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
{% assign prerequisitesText = Course.CourseRequirements | Where:'RequirementType','Prerequisite' | Select:'RequiredLearningCourse' | Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
{% assign facilitatorCount = Course.Facilitators | Size %}
{% assign facilitators = Course.Facilitators | Join:', ' | ReplaceLast:',',' and' | Default:'TBD' %}
{% assign imageFileNameLength = Course.ImageFileGuid | Size %}

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
            background-image: url('/GetImage.ashx?guid={{Course.ImageFileGuid}}'); 
            background-size: cover;
        {% endif %}
        align-items: center; 
        border-radius: 12px; 
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
    
    .page-main-content {
        margin-top: 30px;   
    }
    
    .course-detail-container {
        background-color: white; 
        border-radius: 12px;
        padding: 12px;
        display: flex;
        flex-direction: column;
    }
    
    .course-status-sidebar-container {
        padding: 12px; 
        margin-left: 12px;
        background-color: white; 
        border-radius: 12px;
        width: 300px;
    }
{% endstylesheet %}
<div class=""page-container"">
	<div class=""page-header-section mb-5"">
		<div class=""header-block text-center"">
			<h2>
				{{ Course.Entity.PublicName }}
			</h2>
			<div class=""page-sub-header"">
				{{ Course.Entity.Summary }}
			</div>
		</div>
	</div>
	
	<div class=""page-main-content d-flex"">
		<div class=""course-detail-container text-muted"">
			<div class=""description-header h4"">Course Description</div>
			
			<div class=""course-item-pair-container course-code"">
				<span class=""text-bold"">Course Code: </span>
				<span>{{Course.Entity.CourseCode}}</span>
			</div>
			
			<div class=""course-item-pair-container credits"">
				<span class=""text-bold"">Credits: </span>
				<span>{{Course.Entity.Credits}}</span>
			</div>
			
			<div class=""course-item-pair-container prerequisites"">
				<span class=""text-bold"">Prerequisites: </span>
				
				<span>
					{{ prerequisitesText }}
				</span>
			</div>
			
			<div class=""course-item-pair-container description"">
				<span>{{Course.DescriptionAsHtml}}</span>
			</div>
		</div>
		
		
		<div class=""course-side-panel d-flex flex-column"">
			<div class=""course-status-sidebar-container"">
				
			{% case Course.LearningCompletionStatus %}
			{% when 'Incomplete' %} 
				<div class=""sidebar-header text-bold"">Currently Enrolled</div>
				<div class=""sidebar-value text-muted"">You are currently enrolled in this course.</div>
					
				<div class=""side-bar-action mt-3"">
					<a class=""btn btn-info"" href=""{{ Course.ClassWorkspaceLink }}"">View Course</a>
				</div>
				
                {% for requirementType in requirementTypes %}
                	{% assign requirementsText = Course.CourseRequirements | Where:'RequirementType',requirementType | Select:'RequiredLearningCourse' | Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
                				<div class=""sidebar-header text-bold"">{{ requirementType | Pluralize }}</div>
                				<div class=""sidebar-value text-muted"">{{ requirementsText }}</div>
                {% endfor %}
			{% when 'Passed' %} 
				<div class=""sidebar-header text-bold"">History</div>
				<div class=""sidebar-value text-muted"">You completed this class on {{ Course.MostRecentParticipation.LearningCompletionDateTime | Date: 'MMMM dd, yyyy' }}</div>
				
				<div class=""side-bar-action mt-3"">
					<a href=""{{ Course.ClassWorkspaceLink }}"">View Class Work</a>
				</div>
				
                {% for requirementType in requirementTypes %}
                	{% assign requirementsText = Course.CourseRequirements | Where:'RequirementType',requirementType | Select:'RequiredLearningCourse' | Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
                				<div class=""sidebar-header text-bold"">{{ requirementType | Pluralize }}</div>
                				<div class=""sidebar-value text-muted"">{{ requirementsText }}</div>
                {% endfor %}
			{% else %} 
                {% for requirementType in requirementTypes %}
                	{% assign requirementsText = Course.CourseRequirements | Where:'RequirementType',requirementType | Select:'RequiredLearningCourse' | Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
                				<div class=""sidebar-header text-bold"">{{ requirementType | Pluralize }}</div>
                				<div class=""sidebar-value text-muted"">{{ requirementsText }}</div>
                {% endfor %}
				
				<div class=""sidebar-upcoming-schedule h4"">Upcoming Schedule</div>
				
				<div class=""side-bar-header-value-pair text-muted"">
					<div class=""sidebar-header text-bold"">Next Session Semester: </div>
					<div class=""sidebar-value"">{{ Course.NextSemester.Name }}</div>
				</div>
				
				<div class=""side-bar-header-value-pair text-muted"">
					<div class=""sidebar-header text-bold"">{{ 'Instructor' | PluralizeForQuantity:facilitatorCount }}:</div>
					<div class=""sidebar-value"">{{ facilitators }}</div>
				</div>
				<div class=""side-bar-action mt-3"">
					<a class=""btn btn-info"" href=""{{ Course.CourseEnrollmentLink }}"">Enroll</a>
				</div>
			{% endcase %}
			</div>
		</div>
	</div>
</div>";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new PublicLearningCourseDetailBlockBox();

            SetBoxInitialEntityState( box );

            return box;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( PublicLearningCourseDetailBlockBox box )
        {
            var courseId = RequestContext.PageParameterAsId( PageParameterKey.LearningCourseId );
            var currentPerson = GetCurrentPerson();
            var rockContext = new RockContext();
            var course = new LearningCourseService( rockContext ).GetPublicCourseDetails( courseId, currentPerson.Id );

            course.DescriptionAsHtml = new StructuredContentHelper( course.Entity?.Description ?? string.Empty ).Render();

            var enrolledClassIdKey = course.MostRecentParticipation?.LearningClassId > 0 ?
                IdHasher.Instance.GetHash( course.MostRecentParticipation.LearningClassId ) :
                course.NextSemester?.LearningClasses?.FirstOrDefault()?.IdKey;
            var queryParams = new Dictionary<string, string>
            {
                [PageParameterKey.LearningProgramId] = PageParameter( PageParameterKey.LearningProgramId ),
                [PageParameterKey.LearningCourseId] = course.Entity.IdKey,
                ["LearningClassId"] = enrolledClassIdKey
            };

            course.ClassWorkspaceLink = this.GetLinkedPageUrl( AttributeKey.ClassWorkspacePage, queryParams );
            course.CourseEnrollmentLink = this.GetLinkedPageUrl( AttributeKey.CourseEnrollmentPage, queryParams );

            var mergeFields = this.RequestContext.GetCommonMergeFields( currentPerson );
            mergeFields.Add( "Course", course );
            mergeFields.Add( "ShowCompletionStatus", ShowCompletionStatus() );

            var template = GetAttributeValue( AttributeKey.CourseDetailTemplate ) ?? string.Empty;
            box.CourseHtml = template.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Provide html to the block for it's initial rendering.
        /// </summary>
        /// <returns>The HTML content to initially render.</returns>
        protected override string GetInitialHtmlContent()
        {
            var courseId = RequestContext.PageParameterAsId( PageParameterKey.LearningCourseId );
            var currentPerson = GetCurrentPerson();
            var rockContext = new RockContext();
            var semesterDates = RockDateTimeHelper.CalculateDateRangeFromDelimitedValues( GetAttributeValue( AttributeKey.NextSessionDateRange ), RockDateTime.Now );
            var course = new LearningCourseService( rockContext ).GetPublicCourseDetails( courseId, currentPerson.Id, semesterDates.Start, semesterDates.End );

            course.DescriptionAsHtml = new StructuredContentHelper( course.Entity?.Description ?? string.Empty ).Render();

            var enrolledClassIdKey = course.MostRecentParticipation?.LearningClassId > 0 ?
                IdHasher.Instance.GetHash( course.MostRecentParticipation.LearningClassId ) :
                course.NextSemester?.LearningClasses?.FirstOrDefault()?.IdKey;
            var queryParams = new Dictionary<string, string>
            {
                [PageParameterKey.LearningProgramId] = PageParameter( PageParameterKey.LearningProgramId ),
                [PageParameterKey.LearningCourseId] = course.Entity.IdKey,
                ["LearningClassId"] = enrolledClassIdKey
            };

            course.ClassWorkspaceLink = this.GetLinkedPageUrl( AttributeKey.ClassWorkspacePage, queryParams );
            course.CourseEnrollmentLink = this.GetLinkedPageUrl( AttributeKey.CourseEnrollmentPage, queryParams );

            var mergeFields = this.RequestContext.GetCommonMergeFields( currentPerson );
            mergeFields.Add( "Course", course );
            mergeFields.Add( "ShowCompletionStatus", ShowCompletionStatus() );

            var template = GetAttributeValue( AttributeKey.CourseDetailTemplate ) ?? string.Empty;
            return template.ResolveMergeFields( mergeFields );
        }
        private bool ShowCompletionStatus()
        {
            return GetAttributeValue( AttributeKey.ShowCompletionStatus ) == "Show";
        }

        #endregion
    }
}

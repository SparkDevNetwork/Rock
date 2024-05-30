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

    [LinkedPage( "Class Workspace Page",
        Description = "The page that will show the class workspace.",
        Key = AttributeKey.ClassWorkspacePage )]

    [LinkedPage( "Enrollment Page",
        Description = "The page that will enroll the student in the course.",
        Key = AttributeKey.CourseEnrollmentPage )]

    [CodeEditorField( "Course Detail Template",
        Key = AttributeKey.CourseDetailTemplate,
        Description = "The lava template showing the course detail.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = AttributeDefault.CourseDetailTemplate,
        Order = 1 )]

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
        }

        private static class PageParameterKey
        {
            public const string LearningProgramId = "LearningProgramId";
            public const string LearningCourseId = "LearningCourseId";
        }

        private static class AttributeDefault
        {
            public const string CourseDetailTemplate = @"
<div class=""page-container d-flex flex-column mb-3"">
	<div class=""page-header-section mb-5"" style=""align-items: center; border-radius: 12px; background-image: url('/GetImage.ashx?guid={{Course.ImageFileGuid}}'); background-size: cover;"">
		<div class=""d-flex flex-column header-block text-center"" style=""position: relative; bottom: -40px; background-color: white; border-radius: 12px; width: 80%; margin-top: 130px; margin-left: 10%; margin-right: 10%;"">
			<h2>
				{{ Course.Entity.PublicName }}
			</h2>
			<div class=""page-sub-header"" style=""padding-left: 20%; padding-right: 20%; padding-bottom: 12px;"">
				{{ Course.Entity.Summary }}
			</div>
		</div>
	</div>
	
	{% assign prerequisitesText = Course.CourseRequirements | Where:'RequirementType','Prerequisite' | Select:'RequiredLearningCourse' | Select:'PublicName' | Join:', ' | ReplaceLast:',',' and' | Default:'None' %}
	{% assign facilitatorCount = Course.Facilitators | Size %}
	{% assign facilitators = Course.Facilitators | Join:', ' | ReplaceLast:',',' and' | Default:'TBD' %}
	
	<div class=""page-main-content d-flex mt-2"">
		<div class=""course-detail-container d-flex flex-column text-muted p-3 mr-3"" style=""background-color: white; border-radius: 12px;"">
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
			<div class=""course-status-sidebar-container p-3 mb-2"" style=""background-color: white; border-radius: 12px;"">
				
			{% case Course.LearningCompletionStatus %}
			{% when 'Incomplete' %} 
				<div class=""sidebar-header text-bold"">Currently Enrolled</div>
				<div class=""sidebar-value text-muted"">You are currently enrolled in this course.</div>
					
				<div class=""side-bar-action mt-3"">
					<a class=""btn btn-info"" href=""{{ Course.ClassWorkspaceLink }}"">View Course</a>
				</div>
				
				<div class=""sidebar-header text-bold"">Prerequisites</div>
				<div class=""sidebar-value text-muted"">{{ prerequisitesText }}</div>
			{% when 'Passed' %} 
				<div class=""sidebar-header text-bold"">History</div>
				<div class=""sidebar-value text-muted"">You completed this class on {{ Course.MostRecentParticipation.LearningCompletionDateTime | Date: 'MMMM dd, yyyy' }}</div>
				
				<div class=""side-bar-action mt-3"">
					<a href=""{{ Course.ClassWorkspaceLink }}"">View Class Work</a>
				</div>
				
				<div class=""sidebar-header text-bold"">Prerequisites</div>
				<div class=""sidebar-value text-muted"">{{ prerequisitesText }}</div>
			{% else %} 
				<div class=""sidebar-header text-bold"">Prerequisites</div>
				<div class=""sidebar-value text-muted mb-4"">{{ prerequisitesText }}</div>
				
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
            var courseId = PageParameterAsId( PageParameterKey.LearningCourseId );
            var currentPerson = GetCurrentPerson();
            var rockContext = new RockContext();
            var course = new LearningCourseService( rockContext ).GetPublicCourseDetails( courseId, currentPerson.Id );

            course.DescriptionAsHtml = new StructuredContentHelper( course.Entity.Description ).Render();

            var enrolledClassIdKey = course.MostRecentParticipation?.LearningClassId > 0 ?
                IdHasher.Instance.GetHash( course.MostRecentParticipation.LearningClassId ) :
                course.NextSemester.LearningClasses.FirstOrDefault()?.IdKey;
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

            var template = GetAttributeValue( AttributeKey.CourseDetailTemplate ) ?? string.Empty;
            box.CourseHtml = template.ResolveMergeFields( mergeFields );
        }

        #endregion
    }
}

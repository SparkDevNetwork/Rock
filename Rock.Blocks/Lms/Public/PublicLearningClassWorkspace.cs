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

using Rock.Attribute;
using Rock.Cms.StructuredContent;
using Rock.Data;
using Rock.Enums.Lms;
using Rock.Lms;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks.Lms.LearningActivityCompletionDetail;
using Rock.ViewModels.Blocks.Lms.LearningActivityComponent;
using Rock.ViewModels.Blocks.Lms.LearningActivityDetail;
using Rock.ViewModels.Blocks.Lms.LearningClassAnnouncementDetail;
using Rock.ViewModels.Blocks.Lms.LearningClassContentPageDetail;
using Rock.ViewModels.Blocks.Lms.LearningClassDetail;
using Rock.ViewModels.Blocks.Lms.PublicLearningClassWorkspace;
using Rock.ViewModels.Utility;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Lms
{
    /// <summary>
    /// The main block for interacting with enrolled classes.
    /// </summary>
    [DisplayName( "Public Learning Class Workspace" )]
    [Category( "LMS" )]
    [Description( "The main block for interacting with enrolled classes." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Facilitator Portal Page",
        Description = "The page that will be navigated to when clicking the facilitator portal link.",
        Key = AttributeKey.FacilitatorPortalPage,
        Order = 1 )]

    [CodeEditorField( "Lava Header Template",
        Key = AttributeKey.HeaderTemplate,
        Description = "The lava template to use to render the header on the page. Merge fields include: Course, Activities, Announcements, Facilitators, ContentPages and other Common Merge Fields. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = AttributeDefault.HeaderTemplate,
        Order = 2 )]

    [IntegerField( "The Number of Notifications to Show",
        Key = AttributeKey.NumberOfNotificationsToShow,
        Description = "The number of notifications to show on the class overview page",
        IsRequired = true,
        DefaultIntegerValue = 3,
        Order = 3 )]

    [CustomDropdownListField(
        "Show Grades",
        Key = AttributeKey.ShowGrades,
        Description = "Select 'Show' to show grades on the class overview page; 'Hide' to not show any grades.",
        ListSource = "Show,Hide",
        IsRequired = true,
        DefaultValue = "Show",
        Order = 4 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "1bf70976-85ac-43d3-b98a-0b87a2ffd9b6" )]
    [Rock.SystemGuid.BlockTypeGuid( "55f2e89b-de57-4e24-ac6c-576956fb97c5" )]
    public class PublicLearningClassWorkspace : RockBlockType
    {
        #region Keys

        private static class AttributeDefault
        {
            public const string HeaderTemplate = @"
{% assign imageFileNameLength = Course.ImageFileGuid | Size %}

//- Styles
{% stylesheet %}
    .header-container {
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
{% endstylesheet %}
<div class=""header-container"">
	<div class=""page-header-section mb-5"">
		<div class=""header-block text-center"">
			<h2>
				{{ Course.PublicName }}
			</h2>
			<div class=""page-sub-header"">
				{{ Course.Summary }}
			</div>
		</div>
	</div>
</div>
";
        }

        private static class AttributeKey
        {
            public const string FacilitatorPortalPage = "FacilitatorPortalPage";
            public const string HeaderTemplate = "HeaderTemplate";
            public const string NumberOfNotificationsToShow = "NumberOfNotificationsToShow";
            public const string ShowGrades = "ShowGrades";
        }

        private static class PageParameterKey
        {
            public const string LearningClassId = "LearningClassId";
            public const string LearningCourseId = "LearningCourseId";
            public const string LearningProgramId = "LearningProgramId";
        }

        private static class NavigationUrlKey
        {
            public const string FacilitatorPortalPage = "FacilitatorPortalPage";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Determines whether the ShowGrades attribute is configured to show grades.
        /// </summary>
        /// <returns><c>true</c> if grades should be shown; otherwise <c>false</c></returns>
        bool AreGradesShown => GetAttributeValue( AttributeKey.ShowGrades ) == "Show";
        string HeaderTemplate => GetAttributeValue( AttributeKey.HeaderTemplate ) ?? AttributeDefault.HeaderTemplate;
        int NumberOfNotificationsToShow => GetAttributeValue( AttributeKey.NumberOfNotificationsToShow ).AsInteger();

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            GetHtmlContent( out var box );
            return box;
        }

        /// <inheritdoc/>
        protected override string GetInitialHtmlContent()
        {
            return GetHtmlContent( out var _ );
        }

        /// <summary>
        /// Gets the resolved Lava template for the block.
        /// </summary>
        /// <param name="box">The initialized <see cref="PublicLearningClassWorkspaceBox"/> for the block.</param>
        /// <returns>The resolved HeaderHTML for the block.</returns>
        private string GetHtmlContent( out PublicLearningClassWorkspaceBox box )
        {
            box = GetPublicLearningClassWorkspaceBox( out var course );

            var mergeFields = this.RequestContext.GetCommonMergeFields();
            mergeFields.Add( "Course", course );
            mergeFields.Add( "Activities", box.Activities );
            mergeFields.Add( "Announcements", box.Announcements );
            mergeFields.Add( "ContentPages", box.ContentPages );
            mergeFields.Add( "Facilitators", box.Facilitators );

            box.HeaderHtml = HeaderTemplate.ResolveMergeFields( mergeFields );
            return box.HeaderHtml;
        }

        /// <summary>
        /// Gets a list of student completion bag records for the student and class.
        /// </summary>
        /// <param name="currentPerson">The person the activities belong to.</param>
        /// <param name="classId">The identifier of the <see cref="LearningClass"/> for which to get activities.</param>
        /// <returns>A <see cref="List{LearningActivityCompletionBag}"/> for the student and class.</returns>
        private List<LearningActivityCompletionBag> GetStudentActivities( Person currentPerson, int classId )
        {
            var studentActivities = new List<LearningActivityCompletionBag>();

            var rockContext = new RockContext();

            var bags = new LearningParticipantService( rockContext )
                .GetParticipantBags( classId, false, currentPerson.Id );

            var currentPersonBag = bags.FirstOrDefault();

            var scales = new LearningClassService( rockContext )
                .GetClassScales( classId )
                .ToList()
                .OrderByDescending( s => s.ThresholdPercentage );

            var now = RockDateTime.Now;

            var activityCompletionService = new LearningActivityCompletionService( rockContext );

            var activities = activityCompletionService.GetClassActivities( currentPerson.Id, classId )
                .Include( c => c.LearningActivity.LearningClass )
                .Include( c => c.LearningActivity.LearningClass.LearningSemester )
                .Where( a => a.LearningActivity.AssignTo == AssignTo.Student )
                .ToList()
                .OrderBy( a => a.LearningActivity.Order );

            // Get the necessary properties for all the binary files at once.
            var binaryFileIds = activities.Where( a => a.BinaryFileId.HasValue && a.BinaryFileId > 0 ).Select( a => a.BinaryFileId.Value ).ToList();
            var binaryFiles = new BinaryFileService( rockContext ).GetByIds( binaryFileIds ).Select( b => new
            {
                b.Id,
                b.FileName,
                b.Guid
            } );

            // Get all the components once rather than loading each one inside the foreach loop.
            var components = LearningActivityContainer.Instance.Components;

            // We need to track the previous completion for activities that become available upon completion of the previous.
            LearningActivityCompletionBag previousActivityCompletion = null;

            foreach ( var activity in activities )
            {
                var activityComponent = components.FirstOrDefault( c => c.Value.Value.EntityType.Id == activity.LearningActivity.ActivityComponentId ).Value.Value;

                // If the student hasn't yet completed then scrub the component config of any information not permissible for the student to view (e.g. correct answers).
                var configurationToSend =
                    activity.IsStudentCompleted ?
                    activity.LearningActivity.ActivityComponentSettingsJson :
                    activityComponent.StudentScrubbedConfiguration( activity.LearningActivity.ActivityComponentSettingsJson );

                var activityComponentBag = new LearningActivityComponentBag
                {
                    Name = activityComponent?.Name,
                    ComponentUrl = activityComponent?.ComponentUrl,
                    HighlightColor = activityComponent?.HighlightColor,
                    IconCssClass = activityComponent?.IconCssClass,
                    IdKey = activityComponent?.EntityType.IdKey,
                    Guid = activityComponent?.EntityType.Guid.ToString()
                };

                var activityDescriptionAsHtml = activity.LearningActivity.Description.IsNullOrWhiteSpace() ?
                    string.Empty :
                    new StructuredContentHelper( activity.LearningActivity.Description ).Render();

                var binaryFile = !activity.BinaryFileId.HasValue ?
                    null :
                    binaryFiles
                        .Where( b => b.Id == activity.BinaryFileId )
                        .Select( b => new ListItemBag { Text = b.FileName, Value = b.Guid.ToString() } )
                        .FirstOrDefault();

                var activityBag = new LearningActivityBag
                {
                    ActivityComponent = activityComponentBag,
                    ActivityComponentSettingsJson = configurationToSend,
                    AssignTo = activity.LearningActivity.AssignTo,
                    AvailableDateCalculated = activity.LearningActivity.AvailableDateCalculated,
                    AvailabilityCriteria = activity.LearningActivity.AvailabilityCriteria,
                    AvailableDateDefault = activity.LearningActivity.AvailableDateDefault,
                    AvailableDateOffset = activity.LearningActivity.AvailableDateOffset,
                    CurrentPerson = currentPersonBag,
                    Description = activity.LearningActivity.Description,
                    DescriptionAsHtml = activityDescriptionAsHtml,
                    DueDateCalculated = activity.LearningActivity.DueDateCalculated,
                    DueDateCriteria = activity.LearningActivity.DueDateCriteria,
                    DueDateDefault = activity.LearningActivity.DueDateDefault,
                    DueDateDescription = activity.LearningActivity.DueDateDescription,
                    DueDateOffset = activity.LearningActivity.DueDateOffset,
                    IdKey = activity.IdKey,
                    IsStudentCommentingEnabled = activity.LearningActivity.IsStudentCommentingEnabled,
                    Name = activity.LearningActivity.Name,
                    Order = activity.LearningActivity.Order,
                    Points = activity.LearningActivity.Points
                };

                var isPreviousMethodCalculation = activityBag.AvailabilityCriteria == AvailabilityCriteria.AfterPreviousCompleted;
                var isPreviousActivityCompleted = previousActivityCompletion == null || previousActivityCompletion.IsStudentCompleted | previousActivityCompletion.IsFacilitatorCompleted;

                var isActivityAvailable =
                    activityBag.AvailabilityCriteria == AvailabilityCriteria.AlwaysAvailable ||
                    ( activityBag.AvailableDateCalculated.HasValue && activityBag.AvailableDateCalculated.Value <= DateTime.Now ) ||
                    ( isPreviousMethodCalculation && isPreviousActivityCompleted );

                var availableDate =
                    isPreviousMethodCalculation && isPreviousActivityCompleted ?
                    previousActivityCompletion?.CompletedDate :
                    activity.AvailableDateTime;

                var activityCompletion = new LearningActivityCompletionBag
                {
                    IdKey = activity.IdKey,
                    ActivityBag = activityBag,
                    ActivityComponentCompletionJson = activity.ActivityComponentCompletionJson,
                    AvailableDate = availableDate,
                    BinaryFile = binaryFile,
                    CompletedDate = activity.CompletedDateTime,
                    DueDate = activity.DueDate,
                    FacilitatorComment = activity.FacilitatorComment,
                    GradeName = activity.GetGrade( scales )?.Name,
                    GradeText = activity.GetGradeText( scales ),
                    IsAvailable = isActivityAvailable,
                    IsGradePassing = activity.LearningActivity.Points == 0 || activity.GetGrade( scales ).IsPassing,
                    IsFacilitatorCompleted = activity.IsFacilitatorCompleted,
                    IsStudentCompleted = activity.IsStudentCompleted,
                    PointsEarned = activity.PointsEarned,
                    Student = currentPersonBag,
                    StudentComment = activity.StudentComment,
                    WasCompletedOnTime = activity.WasCompletedOnTime
                };

                studentActivities.Add( activityCompletion );

                previousActivityCompletion = activityCompletion;
            }

            return studentActivities;
        }

        /// <summary>
        /// Gets a populated <see cref="PublicLearningClassWorkspaceBox"/> for the block.
        /// </summary>
        /// <param name="course">The Course with it's details.</param>
        /// <returns>A <see cref="PublicLearningClassWorkspaceBox"/>.</returns>
        private PublicLearningClassWorkspaceBox GetPublicLearningClassWorkspaceBox( out LearningCourse course )
        {
            var classIdKey = PageParameter( PageParameterKey.LearningClassId );
            var courseIdKey = PageParameter( PageParameterKey.LearningCourseId );
            var courseId = IdHasher.Instance.GetId( courseIdKey );
            var classId = IdHasher.Instance.GetId( classIdKey ).ToIntSafe();

            course = new LearningCourseService( RockContext ).Queryable()
                .AsNoTracking()
                .Include( c => c.ImageBinaryFile )
                .Include( c => c.LearningProgram )
                .FirstOrDefault( c => c.Id == courseId );

            // Initialize the box with the course properties.
            var box =
                course == null ?
                new PublicLearningClassWorkspaceBox() :
                new PublicLearningClassWorkspaceBox
                {
                    ClassIdKey = classIdKey,
                    CourseIdKey = courseIdKey,
                    CourseImageGuid = course.ImageBinaryFile?.Guid,
                    CourseName = course.PublicName,
                    CourseSummary = course.Summary,
                    ProgramConfigurationMode = course.LearningProgram.ConfigurationMode,
                    NumberOfNotificationsToShow = NumberOfNotificationsToShow,
                    ShowGrades = AreGradesShown
                };

            if ( box.CourseName.IsNullOrWhiteSpace() )
            {
                box.ErrorMessage = $"The {LearningCourse.FriendlyTypeName} was not found.";
                return new PublicLearningClassWorkspaceBox();
            }

            if ( classId == 0 )
            {
                box.ErrorMessage = $"The Class was not found.";
                return box;
            }

            box.ContentPages = new LearningClassContentPageService( RockContext ).Queryable()
                .Where( c => c.LearningClassId == classId )
                .Select( c => new
                {
                    c.Id,
                    c.LearningClassId,
                    c.Title,
                    c.StartDateTime,
                    c.Content
                } )
                .ToList()
                .Select( c => new LearningClassContentPageBag
                {
                    Content = new StructuredContentHelper( c.Content ).Render(),
                    IdKey = IdHasher.Instance.GetHash( c.Id ),
                    LearningClassId = c.LearningClassId,
                    StartDateTime = c.StartDateTime,
                    Title = c.Title
                } )
                .ToList();

            box.NavigationUrls = GetBoxNavigationUrls();

            // Get the student and current persons bags.
            var currentPerson = GetCurrentPerson();

            var participantService = new LearningParticipantService( RockContext );

            box.Activities = GetStudentActivities( currentPerson, classId );
            box.Facilitators = participantService.GetFacilitatorBags( classId )
                .ToList()
                .Select( f => new LearningClassFacilitatorBag
                {
                    FacilitatorEmail = f.Email,
                    FacilitatorName = f.Name,
                    FacilitatorRole = f.RoleName,
                    Facilitator = new ListItemBag
                    {
                        Value = f.Guid.ToString(),
                        Text = f.Name
                    }
                } )
                .ToList();

            var currentPersonGuid = currentPerson.Guid.ToString();

            box.IsCurrentPersonFacilitator = box.Facilitators.Any( f => f.Facilitator.Value == currentPersonGuid );

            var participantIdKey = box.Activities.Select( a => a.Student.IdKey ).FirstOrDefault();
            var participant = participantService.GetInclude( participantIdKey, p => p.LearningGradingSystemScale );

            if ( box.ShowGrades && participant?.LearningGradingSystemScale != null )
            {
                box.CurrentGrade = new ViewModels.Blocks.Lms.LearningGradingSystemScaleDetail.LearningGradingSystemScaleBag
                {
                    Name = participant.LearningGradingSystemScale.Name,
                    IsPassing = participant.LearningGradingSystemScale.IsPassing,
                    Description = participant.LearningGradingSystemScale.Description
                };
            }

            box.Announcements = new LearningClassAnnouncementService( RockContext ).GetForClass( classId ).ToList()
                .Select( a => new LearningClassAnnouncementBag
                {
                    CommunicationMode = a.CommunicationMode,
                    CommunicationSent = a.CommunicationSent,
                    Description = new StructuredContentHelper( a.Description ).Render(),
                    PublishDateTime = a.PublishDateTime,
                    Title = a.Title
                } )
                .OrderByDescending( a => a.PublishDateTime )
                .ToList();

            SetNotifications( box );

            return box;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var queryParams = new Dictionary<string, string>
            {
                [PageParameterKey.LearningProgramId] = PageParameter( PageParameterKey.LearningProgramId ),
                [PageParameterKey.LearningCourseId] = PageParameter( PageParameterKey.LearningCourseId ),
                [PageParameterKey.LearningClassId] = PageParameter( PageParameterKey.LearningClassId )
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.FacilitatorPortalPage] = this.GetLinkedPageUrl( AttributeKey.FacilitatorPortalPage, queryParams )
            };
        }

        /// <summary>
        /// Sets the notifications on the box based on the box.Activities.
        /// </summary>
        /// <param name="box">The box to set the Notifications property on.</param>
        private void SetNotifications( PublicLearningClassWorkspaceBox box )
        {
            // Get the available activities that the student hasn't yet completed.
            box.Notifications = box.Activities
                .Where( a => ( a.IsAvailable && !a.IsStudentCompleted ) )
                .Select( a => new PublicLearningClassWorkspaceNotificationBag
                {
                    Content = a.IsDueSoon ? $"Due on {a.DueDate.ToShortDateString()}" : $"Available on {a.AvailableDate.ToShortDateString()}",
                    LabelText = a.IsDueSoon ? "Due Soon" : "Available",
                    LabelType = a.IsDueSoon ? "warning" : "default",
                    NotificationDateTime = a.DueDate ?? DateTime.MaxValue,
                    Title = a.ActivityBag.Name
                } )
                .ToList();

            var nextAvailableActivity = box.Activities.OrderBy( a => a.AvailableDate )
                    .FirstOrDefault( a => !a.IsStudentCompleted && !a.IsAvailable );

            // If there's no available activities that the student hasn't completed.
            // Then show them their next avaiable activity.
            if ( !box.Notifications.Any() && nextAvailableActivity != null )
            {
                box.Notifications.Add( new PublicLearningClassWorkspaceNotificationBag
                {
                    Content = nextAvailableActivity.ActivityBag.Name,
                    LabelText = "Available Soon",
                    LabelType = "default",
                    NotificationDateTime = nextAvailableActivity.AvailableDate ?? DateTime.MaxValue,
                    Title = nextAvailableActivity.ActivityBag.Name
                } );
            }

            // Get any activities that have facilitator comments.
            var activityNotifications = box.Activities
                .Where( a => a.IsFacilitatorCompleted && a.FacilitatorComment.IsNotNullOrWhiteSpace() )
                .Select( a => new PublicLearningClassWorkspaceNotificationBag
                {
                    Content = $"A facilitator commented on {a.ActivityBag.ActivityComponent.Name}: {a.ActivityBag.Name}.",
                    LabelText = "Notification",
                    LabelType = a.IsDueSoon ? "warning" : "default",
                    NotificationDateTime = a.CompletedDate ?? DateTime.MaxValue,
                    Title = "Facilitator Comment"
                } )
                .ToList();

            box.Notifications.AddRange( activityNotifications.OrderBy( a => a.NotificationDateTime ).ToList() );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Performs the student completion logic for an activity.
        /// </summary>
        /// <param name="activityCompletionBag">The <see cref="LearningActivityCompletionBag" /> for the student.</param>
        /// <returns>The <see cref="LearningActivityCompletionBag" /> with any updates applied.</returns>
        [BlockAction]
        public BlockActionResult CompleteActivity( LearningActivityCompletionBag activityCompletionBag )
        {
            using ( var rockContext = new RockContext() )
            {
                var completionId = IdHasher.Instance.GetId( activityCompletionBag.IdKey );
                var currentPersonId = GetCurrentPerson().Id;

                // Verify that the current person is the student for this activity completion.
                var activity = new LearningActivityCompletionService( rockContext ).Queryable()
                    .Include( a => a.LearningActivity )
                    .Include( a => a.Student )
                    .FirstOrDefault( a => a.Id == completionId && a.Student.PersonId == currentPersonId );

                if ( activity == null )
                {
                    return ActionBadRequest( $"No {LearningActivityCompletion.FriendlyTypeName} was found." );
                }

                activity.BinaryFileId = activityCompletionBag.BinaryFile.GetEntityId<BinaryFile>( rockContext );
                activity.IsStudentCompleted = true;
                activity.StudentComment = activityCompletionBag.StudentComment;

                // Only allow student updating completion and points if this hasn't yet been completed by the facilitator.
                if ( !activity.IsFacilitatorCompleted )
                {
                    var components = LearningActivityContainer.Instance.Components;
                    var activityComponent = components.FirstOrDefault( c => c.Value.Value.EntityType.Id == activity.LearningActivity.ActivityComponentId ).Value.Value;

                    activity.ActivityComponentCompletionJson = activityComponent.GetCompletionJsonToPersist(
                        activityCompletionBag.ActivityComponentCompletionJson,
                        activity.LearningActivity.ActivityComponentSettingsJson );

                    activity.PointsEarned = activityComponent.CalculatePointsEarned(
                        activity.LearningActivity.ActivityComponentSettingsJson,
                        activityCompletionBag.ActivityComponentCompletionJson,
                        activity.LearningActivity.Points
                    );
                }
                
                if ( !activity.CompletedByPersonAliasId.HasValue )
                {
                    var currentPerson = GetCurrentPerson().PrimaryAliasId;
                }

                if ( !activity.CompletedDateTime.HasValue )
                {
                    var now = RockDateTime.Now;
                    activity.CompletedDateTime = now;
                    activity.WasCompletedOnTime = activity.DueDate.HasValue && activity.DueDate >= now;
                }

                rockContext.SaveChanges();

                activityCompletionBag.IsStudentCompleted = true;
                activityCompletionBag.CompletedDate = activity.CompletedDateTime;
                activityCompletionBag.WasCompletedOnTime = activity.WasCompletedOnTime;

                // Return the raw component settings so a grade can be computed (if applicable).
                activityCompletionBag.ActivityBag.ActivityComponentSettingsJson = activity.LearningActivity.ActivityComponentSettingsJson;

                // Include the updated activity completion in the response.
                // if the activity checks the completion JSON for historical configuration
                // we'll want to ensure it's provided now.
                activityCompletionBag.ActivityComponentCompletionJson = activity.ActivityComponentCompletionJson;

                return ActionOk( activityCompletionBag );
            }
        }

        #endregion

    }
}
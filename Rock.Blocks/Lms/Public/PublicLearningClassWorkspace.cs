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
using Rock.ViewModels.Blocks.Lms.LearningClassActivityCompletionDetail;
using Rock.ViewModels.Blocks.Lms.LearningActivityComponent;
using Rock.ViewModels.Blocks.Lms.LearningClassActivityDetail;
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
        IsRequired = false,
        DefaultValue = AttributeDefault.HeaderTemplate,
        Order = 2 )]

    [IntegerField( "Default Number of Notification/Announcement Items to Show",
        Key = AttributeKey.NumberOfNotificationsToShow,
        Description = "The default number of notifications and announcements to show on the class overview page.",
        IsRequired = true,
        DefaultIntegerValue = 5,
        Order = 3 )]

    [BooleanField(
        "Show Grades",
        Key = AttributeKey.ShowGrades,
        Description = "Determines if grades will be shown on the class overview page.",
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        IsRequired = true,
        DefaultBooleanValue = true,
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
<div class=""hero-section mb-5"">
    <div class=""hero-section-image"" style=""background-image: url('/GetImage.ashx?guid={{ Course.ImageBinaryFile.Guid }}')""></div>
    <div class=""hero-section-content"">
        <h1 class=""hero-section-title""> {{ Course.PublicName }} </h1>
        <p class=""hero-section-description""> {{ Course.Summary }} </p>
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
        bool AreGradesShown => GetAttributeValue( AttributeKey.ShowGrades ).AsBoolean();

        /// <summary>
        /// The Lava template to use for the header.
        /// </summary>
        string HeaderTemplate => GetAttributeValue( AttributeKey.HeaderTemplate ) ?? AttributeDefault.HeaderTemplate;

        /// <summary>
        /// The number of notifications to show.
        /// </summary>
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
        /// <returns>A <see cref="List{LearningClassActivityCompletionBag}"/> for the student and class.</returns>
        private List<LearningClassActivityCompletionBag> GetStudentActivities( Person currentPerson, int classId )
        {
            var studentActivities = new List<LearningClassActivityCompletionBag>();

            var learningParticipantService = new LearningParticipantService( RockContext );

            var bags = learningParticipantService
                .GetParticipantBags( classId, false, currentPerson.Id );

            // Defer the current person participantData bag to the student one (if multiple present).
            // If they are also a facilitator we'll want to use the studentId for getting activities.
            var currentPersonParticipantBag = bags.OrderBy( p => p.IsFacilitator ).FirstOrDefault();

            // If the current person is a facilitator ensure
            // they have the access necessary for the instructor portal.
            currentPersonParticipantBag.IsFacilitator = bags.Any( p => p.IsFacilitator );

            var scales = new LearningClassService( RockContext )
                .GetClassScales( classId )
                .ToList()
                .OrderByDescending( s => s.ThresholdPercentage );

            var now = RockDateTime.Now;

            var activityCompletionService = new LearningClassActivityCompletionService( RockContext );

            var activities = learningParticipantService
                .GetStudentLearningPlan( classId, currentPerson.Id );

            // Get the necessary properties for all the binary files at once.
            var binaryFileIds = activities.Where( a => a.BinaryFileId.HasValue && a.BinaryFileId > 0 ).Select( a => a.BinaryFileId.Value ).ToList();
            var binaryFiles = new BinaryFileService( RockContext ).GetByIds( binaryFileIds ).Select( b => new
            {
                b.Id,
                b.FileName,
                b.Guid
            } );

            // Get all the components once rather than loading each one inside the foreach loop.
            var components = LearningActivityContainer.Instance.Components.Values.ToList();

            // For any graded activities get the PersonAlias for all graders.
            var personAliasIds = activities.Where( a => a.GradedByPersonAliasId.HasValue && a.GradedByPersonAliasId > 0 )
                .Select( a => ( int ) a.GradedByPersonAliasId )
                .Distinct()
                .ToList();
            var personAliases = new PersonAliasService( RockContext ).GetByIds( personAliasIds );

            // We need to track the previous completion for activities that become available upon completion of the previous.
            LearningClassActivityCompletionBag previousActivityCompletion = null;

            foreach ( var activity in activities )
            {
                var activityComponent = components.FirstOrDefault( c => c.Value.EntityType.Id == activity.LearningClassActivity.LearningActivity.ActivityComponentId ).Value;
                var componentData = activity.LearningClassActivity.LearningActivity.ActivityComponentSettingsJson.FromJsonOrNull<Dictionary<string, string>>()
                    ?? new Dictionary<string, string>();

                var activityComponentBag = new LearningActivityComponentBag
                {
                    Name = activityComponent?.Name,
                    ComponentUrl = activityComponent?.ComponentUrl,
                    ComponentConfiguration = activityComponent.GetActivityConfiguration( activity.LearningClassActivity, componentData, PresentedFor.Student, RockContext, RequestContext ),
                    HighlightColor = activityComponent?.HighlightColor,
                    IconCssClass = activityComponent?.IconCssClass,
                    IdKey = activityComponent?.EntityType.IdKey,
                    Guid = activityComponent?.EntityType.Guid.ToString()
                };

                var binaryFile = !activity.BinaryFileId.HasValue ?
                    null :
                    binaryFiles
                        .Where( b => b.Id == activity.BinaryFileId )
                        .Select( b => new ListItemBag { Text = b.FileName, Value = b.Guid.ToString() } )
                        .FirstOrDefault();

                // AvailableDateTime should have been calculated for the student
                // in the call to LearningParticipantService.GetStudentLearningPlan().
                var activityBag = new LearningClassActivityBag
                {
                    ActivityComponent = activityComponentBag,
                    AssignTo = activity.LearningClassActivity.AssignTo,
                    AvailableDateCalculated = activity.AvailableDateTime,
                    AvailabilityCriteria = activity.LearningClassActivity.AvailabilityCriteria,
                    AvailableDateDefault = activity.LearningClassActivity.AvailableDateDefault,
                    AvailableDateOffset = activity.LearningClassActivity.AvailableDateOffset,
                    CurrentPerson = currentPersonParticipantBag,
                    Description = activity.LearningClassActivity.LearningActivity.Description,
                    DueDateCalculated = activity.LearningClassActivity.DueDateCalculated,
                    DueDateCriteria = activity.LearningClassActivity.DueDateCriteria,
                    DueDateDefault = activity.LearningClassActivity.DueDateDefault,
                    DueDateDescription = activity.LearningClassActivity.DueDateDescription,
                    DueDateOffset = activity.LearningClassActivity.DueDateOffset,
                    IdKey = activity.IdKey,
                    IsStudentCommentingEnabled = activity.LearningClassActivity.IsStudentCommentingEnabled,
                    Name = activity.LearningClassActivity.Name,
                    Order = activity.LearningClassActivity.Order,
                    Points = activity.LearningClassActivity.Points
                };

                var isPreviousMethodCalculation = activityBag.AvailabilityCriteria == AvailabilityCriteria.AfterPreviousCompleted;
                var isPreviousActivityCompleted = previousActivityCompletion == null || previousActivityCompletion.IsStudentCompleted | previousActivityCompletion.IsFacilitatorCompleted;

                // If the student has already completed it (and the available date was later changed)
                // or it's always available
                // or it's available date is passed.
                // or the previous activity is completed AND
                //    this activity is configured to become available after the previous activity is completed.
                var isActivityAvailable =
                    activity.IsStudentCompleted ||
                    activityBag.AvailabilityCriteria == AvailabilityCriteria.AlwaysAvailable ||
                    ( activityBag.AvailableDateCalculated.HasValue && activityBag.AvailableDateCalculated.Value <= DateTime.Now ) ||
                    ( isPreviousMethodCalculation && isPreviousActivityCompleted );

                var availableDate =
                    isPreviousMethodCalculation && isPreviousActivityCompleted ?
                    previousActivityCompletion?.CompletedDate :
                    activity.AvailableDateTime?.ToRockDateTimeOffset();

                var grade = activity.GetGrade( scales );
                var hasPassingGrade = grade?.IsPassing ?? false;

                var completionData = activity.ActivityComponentCompletionJson.FromJsonOrNull<Dictionary<string, string>>()
                    ?? new Dictionary<string, string>();

                var activityCompletion = new LearningClassActivityCompletionBag
                {
                    IdKey = activity.IdKey,
                    ClassActivityBag = activityBag,
                    CompletionValues = activityComponent.GetCompletionValues( activity, completionData, componentData, PresentedFor.Student, RockContext, RequestContext ),
                    AvailableDate = availableDate,
                    BinaryFile = binaryFile,
                    CompletedDate = activity.CompletedDateTime?.ToRockDateTimeOffset(),
                    DueDate = activity.DueDate,
                    FacilitatorComment = activity.FacilitatorComment,
                    GradedByPersonAlias = activity.GradedByPersonAlias.ToListItemBag(),
                    GradeColor = grade?.HighlightColor,
                    GradeName = grade?.Name,
                    GradeText = activity.GetGradeText( scales ),
                    IsAvailable = isActivityAvailable,
                    IsGradePassing = activity.LearningClassActivity.Points == 0 || hasPassingGrade,
                    IsFacilitatorCompleted = activity.IsFacilitatorCompleted,
                    IsLate = activity.IsLate,
                    IsStudentCompleted = activity.IsStudentCompleted,
                    LearningClassActivityIdKey = activity.LearningClassActivity.IdKey,
                    PointsEarned = activity.PointsEarned,
                    RequiresScoring = activity.RequiresGrading,
                    Student = currentPersonParticipantBag,
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
            var courseService = new LearningCourseService( RockContext );
            var courseId = courseService.GetSelect( courseIdKey, c => c.Id, !PageCache.Layout.Site.DisablePredictableIds );
            var classId = new LearningClassService( RockContext ).GetSelect( classIdKey, c => c.Id, !PageCache.Layout.Site.DisablePredictableIds );

            course = courseService.Queryable()
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
                box.ErrorMessage = $"The {LearningClass.FriendlyTypeName} was not found.";
                return box;
            }

            // Get the student and current persons bags.
            var currentPerson = GetCurrentPerson();
            var mergeFields = RequestContext.GetCommonMergeFields();

            var participantService = new LearningParticipantService( RockContext );
            var currentPersonIsEnrolled = participantService.GetParticipants( classId ).Any( s => s.PersonId == currentPerson.Id );

            // Facilitators are also considered enrolled.
            if ( !currentPersonIsEnrolled )
            {
                box.ErrorMessage = $"You must be enrolled to view the class workspace.";
                return box;
            }

            box.EnableAnnouncements = course.EnableAnnouncements;

            var now = RockDateTime.Now;
            box.ContentPages = new LearningClassContentPageService( RockContext ).Queryable()
                .Where( c => c.LearningClassId == classId && ( !c.StartDateTime.HasValue || c.StartDateTime <= now ) )
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
                    Content = new StructuredContentHelper( c.Content )
                        .Render()
                        .ResolveMergeFields( mergeFields ),
                    IdKey = IdHasher.Instance.GetHash( c.Id ),
                    LearningClassId = c.LearningClassId,
                    StartDateTime = c.StartDateTime,
                    Title = c.Title
                } )
                .ToList();

            box.NavigationUrls = GetBoxNavigationUrls();

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

            box.IsCurrentPersonFacilitator = box.Activities.Any( a => a.ClassActivityBag.CurrentPerson.IsFacilitator );

            var participantIdKey = box.Activities.Select( a => a.Student.IdKey ).FirstOrDefault();
            var participantData = participantService.GetSelect( participantIdKey, p => new
            {
                p.LearningGradingSystemScale,
                p.LearningCompletionDateTime,
                SemesterEndDate = p.LearningClass.LearningSemester.EndDate,
                p.CommunicationPreference
            } );

            box.ClassCompletionDate = participantData?.LearningCompletionDateTime;

            if ( participantData != null && ( participantData.CommunicationPreference == CommunicationType.Email || participantData.CommunicationPreference == CommunicationType.SMS ) )
            {
                box.CommunicationPreference = ( Rock.Enums.Communication.CommunicationType ) participantData.CommunicationPreference;
            }
            else
            {
                box.CommunicationPreference = ( Rock.Enums.Communication.CommunicationType ) currentPerson.CommunicationPreference;
            }

                // Allow historical access if the course allows it and the class is not over.
                var canShowHistoricalAccess = course.AllowHistoricalAccess
                    && participantData != null
                    && ( !participantData.SemesterEndDate.HasValue
                    || participantData.SemesterEndDate.Value.IsFuture() );

            var hasCompletedClass = participantData != null && participantData.LearningCompletionDateTime.HasValue;
            if ( !canShowHistoricalAccess && hasCompletedClass && !participantData.LearningCompletionDateTime.Value.IsToday() )
            {
                // If the class doesn't allow historical access and the student didn't complete today.
                box.ErrorMessage = "This class has ended and is no longer available for viewing.";
            }

            if ( box.ShowGrades && participantData?.LearningGradingSystemScale != null )
            {
                box.CurrentGrade = new ViewModels.Blocks.Lms.LearningGradingSystemScaleDetail.LearningGradingSystemScaleBag
                {
                    Name = participantData.LearningGradingSystemScale.Name,
                    IsPassing = participantData.LearningGradingSystemScale.IsPassing,
                    Description = participantData.LearningGradingSystemScale.Description
                };
            }

            box.Announcements = new LearningClassAnnouncementService( RockContext ).GetForClass( classId ).ToList()
                .Select( a => new LearningClassAnnouncementBag
                {
                    CommunicationMode = a.CommunicationMode,
                    CommunicationSent = a.CommunicationSent,
                    Description = new StructuredContentHelper( a.Description )
                        .Render()
                        .ResolveMergeFields( mergeFields ),
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
            // Get the available activities that the student or facilitator haven't yet completed.
            box.Notifications = box.Activities
                .Where( a => ( a.IsAvailable && !a.IsStudentCompleted && !a.IsFacilitatorCompleted ) )
                .Select( a => new PublicLearningClassWorkspaceNotificationBag
                {
                    Content = a.IsDueSoon || a.IsLate ? $"Due {a.DueDate?.DateTime.ToElapsedString()}" : a.AvailableDate.HasValue ? $"Available {a.AvailableDate?.DateTime.ToElapsedString()}" : "Available",
                    LabelText = a.IsDueSoon ? "Due Soon" : a.IsLate ? "Late" : "Available",
                    LabelType = a.IsDueSoon ? "warning" : a.IsLate ? "danger" : "success",
                    NotificationDateTime = a.IsDueSoon || a.IsLate ? a.DueDate : a.AvailableDate,
                    Title = a.ClassActivityBag.Name
                } )
                .ToList();

            var nextAvailableActivity = box.Activities.OrderBy( a => a.AvailableDate )
                    .FirstOrDefault( a => !a.IsStudentCompleted && !a.IsFacilitatorCompleted && !a.IsAvailable );

            // If there's no available activities that the student hasn't completed.
            // Then show them their next available activity.
            if ( !box.Notifications.Any() && nextAvailableActivity != null )
            {
                box.Notifications.Add( new PublicLearningClassWorkspaceNotificationBag
                {
                    Content = nextAvailableActivity.ClassActivityBag.Description,
                    LabelText = "Available Soon",
                    LabelType = "default",
                    NotificationDateTime = nextAvailableActivity.AvailableDate ?? DateTime.MaxValue,
                    Title = nextAvailableActivity.ClassActivityBag.Name
                } );
            }

            // Get any activities that have facilitator comments.
            var activityNotifications = box.Activities
                .Where( a => a.FacilitatorComment.IsNotNullOrWhiteSpace() )
                .Select( a => new PublicLearningClassWorkspaceNotificationBag
                {
                    Content = $"A facilitator commented on {a.ClassActivityBag.ActivityComponent.Name}: {a.ClassActivityBag.Name}.",
                    LabelText = "Comment",
                    LabelType = "default",
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
        /// <param name="activityCompletionBag">The <see cref="LearningClassActivityCompletionBag" /> for the student.</param>
        /// <returns>The <see cref="LearningClassActivityCompletionBag" /> with any updates applied.</returns>
        [BlockAction]
        public BlockActionResult CompleteActivity( LearningClassActivityCompletionBag activityCompletionBag )
        {
            var completionId = IdHasher.Instance.GetId( activityCompletionBag.IdKey ).ToIntSafe();
            var classActivityId = IdHasher.Instance.GetId( activityCompletionBag.LearningClassActivityIdKey ).ToIntSafe();
            var classId = RequestContext.PageParameterAsId( PageParameterKey.LearningClassId );
            var currentPerson = GetCurrentPerson();

            if ( currentPerson == null )
            {
                // Shouldn't be possible.
                return ActionBadRequest( $"You must be logged in to complete activities." );
            }

            var activityCompletionService = new LearningClassActivityCompletionService( RockContext );
            var participantService = new LearningParticipantService( RockContext );

            var isStudent = participantService.Queryable()
                .Any( p =>
                    p.PersonId == currentPerson.Id
                    && p.LearningClassId == classId
                    && !p.GroupRole.IsLeader );

            if ( !isStudent )
            {
                return ActionBadRequest( $"Only active students may complete activities." );
            }

            // Verify that the current person is the student for this activity completion.
            var completion = completionId == 0
                ? participantService.GetStudentActivity( classId, currentPerson.Id, classActivityId )
                : activityCompletionService.Queryable()
                    .Include( a => a.LearningClassActivity )
                    .Include( a => a.Student )
                    .FirstOrDefault( a => a.Id == completionId && a.Student.PersonId == currentPerson.Id );

            if ( completion == null )
            {
                return ActionBadRequest( $"No {LearningClassActivityCompletion.FriendlyTypeName} was found." );
            }

            completion.BinaryFileId = activityCompletionBag.BinaryFile.GetEntityId<BinaryFile>( RockContext );
            completion.StudentComment = activityCompletionBag.StudentComment;

            // It's important that the WasCompletedOnTime is set before the
            // IsStudentCompleted bool. Activity.IsLate property uses this bit.
            if ( !completion.CompletedDateTime.HasValue )
            {
                var now = RockDateTime.Now;
                completion.CompletedDateTime = now;
                completion.WasCompletedOnTime = !completion.IsLate;
            }

            if ( !completion.CompletedByPersonAliasId.HasValue )
            {
                completion.CompletedByPersonAliasId = GetCurrentPerson()?.PrimaryAliasId;

                if ( completion.LearningClassActivity.AssignTo == AssignTo.Student )
                {
                    completion.IsStudentCompleted = true;
                    activityCompletionBag.IsStudentCompleted = true;
                }
                else
                {
                    completion.IsFacilitatorCompleted = true;
                    activityCompletionBag.IsFacilitatorCompleted = true;
                }
            }

            var activityComponent = LearningActivityContainer.Instance.Components.Values
                .FirstOrDefault( c => c.Value.EntityType.Id == completion.LearningClassActivity.LearningActivity.ActivityComponentId )
                .Value;

            var componentData = completion.LearningClassActivity.LearningActivity.ActivityComponentSettingsJson.FromJsonOrNull<Dictionary<string, string>>()
                ?? new Dictionary<string, string>();
            Dictionary<string, string> completionData = null;

            // Only allow student updating completion and points if this hasn't yet been graded by a facilitator.
            if ( !completion.GradedByPersonAliasId.HasValue )
            {
                completionData = activityComponent.GetCompletionData( completion,
                    activityCompletionBag.CompletionValues,
                    componentData,
                    PresentedFor.Student,
                    RockContext,
                    RequestContext )
                    ?? new Dictionary<string, string>();

                completion.ActivityComponentCompletionJson = completionData.ToJson();

                completion.PointsEarned = activityComponent.CalculatePointsEarned( completion,
                    completionData,
                    componentData,
                    completion.LearningClassActivity.Points,
                    RockContext,
                    RequestContext );
            }

            completionData = completionData
                ?? completion.ActivityComponentCompletionJson.FromJsonOrNull<Dictionary<string, string>>()
                ?? new Dictionary<string, string>();

            // Let the Activity component decide if it needs to be graded.
            completion.RequiresGrading = activityComponent.RequiresGrading( completion, completionData, componentData, RockContext, RequestContext );
            activityCompletionBag.RequiresScoring = completion.RequiresGrading;

            if ( completion.Id == 0 )
            {
                activityCompletionService.Add( completion );
            }

            RockContext.SaveChanges();

            var scales = new LearningClassService( RockContext )
                .GetClassScales( classId )
                .ToList()
                .OrderByDescending( s => s.ThresholdPercentage );

            activityCompletionBag.IdKey = completion.IdKey;
            activityCompletionBag.CompletedDate = completion.CompletedDateTime;
            activityCompletionBag.WasCompletedOnTime = completion.WasCompletedOnTime;

            var grade = completion.GetGrade( scales );
            if ( grade != null )
            {
                activityCompletionBag.GradeName = grade.Name;
                activityCompletionBag.GradeColor = grade.HighlightColor;
            }

            // Update the bag so the student can see the results.
            activityCompletionBag.CompletionValues = activityComponent.GetCompletionValues( completion,
                completionData,
                componentData,
                PresentedFor.Student,
                RockContext,
                RequestContext );

            return ActionOk( activityCompletionBag );
        }

        [BlockAction]
        public BlockActionResult UpdateCommunicationPreference( CommunicationType communicationType, string learningClassIdKey )
        {
            var currentPerson = GetCurrentPerson();
            var learningClassService = new LearningClassService( RockContext );
            var learningClass = learningClassService.Get( learningClassIdKey );

            if ( learningClass == null )
            {
                return ActionBadRequest( "Could not find the specified Class" );
            }

            var learningParticipants = learningClass.LearningParticipants.Where( l => l.PersonId == currentPerson.Id ).ToList();

            foreach ( var participant in learningParticipants )
            {
                participant.CommunicationPreference = communicationType;
            }

            RockContext.SaveChanges();
            return ActionOk();
        }

        #endregion

    }
}
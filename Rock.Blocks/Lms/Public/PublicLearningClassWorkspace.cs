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
using Rock.Data;
using Rock.Lms;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks.Lms.LearningActivityCompletionDetail;
using Rock.ViewModels.Blocks.Lms.LearningActivityComponent;
using Rock.ViewModels.Blocks.Lms.LearningActivityDetail;
using Rock.ViewModels.Blocks.Lms.LearningClassDetail;
using Rock.ViewModels.Blocks.Lms.PublicLearningClassWorkspace;
using Rock.ViewModels.Utility;

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
        Description = "The page that will be navigated to when clicking facilitator portal link.",
        Key = AttributeKey.FacilitatorPortalPage,
        Order = 1 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "1bf70976-85ac-43d3-b98a-0b87a2ffd9b6" )]
    [Rock.SystemGuid.BlockTypeGuid( "55f2e89b-de57-4e24-ac6c-576956fb97c5" )]
    public class PublicLearningClassWorkspace : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string FacilitatorPortalPage = "FacilitatorPortalPage";
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

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var classIdKey = PageParameter( PageParameterKey.LearningClassId );
            var courseIdKey = PageParameter( PageParameterKey.LearningCourseId );
            var courseId = IdHasher.Instance.GetId( courseIdKey );
            var classId = IdHasher.Instance.GetId( classIdKey ).ToIntSafe();

            // Initialize the box with the course properties.
            var box = new LearningCourseService( RockContext ).Queryable()
                .AsNoTracking()
                .Include( c => c.ImageBinaryFile )
                .Where( c => c.Id == courseId )
                .Select( c => new PublicLearningClassWorkspaceBox
                {
                    ClassIdKey = classIdKey,
                    CourseIdKey = courseIdKey,
                    CourseImageGuid = c.ImageBinaryFile.Guid,
                    CourseName = c.PublicName,
                    CourseSummary = c.Summary
                } ).FirstOrDefault() ?? new PublicLearningClassWorkspaceBox();


            if ( box.CourseName.IsNullOrWhiteSpace() )
            {
                box.ErrorMessage = $"The {LearningCourse.FriendlyTypeName} was not found.";
                return box;
            }

            if ( classId == 0 )
            {
                box.ErrorMessage = $"The Class was not found.";
                return box;
            }

            box.NavigationUrls = GetBoxNavigationUrls();

            // Get the student and current persons bags.
            var currentPerson = GetCurrentPerson();

            box.Activities = GetStudentActivities( currentPerson, classId );
            box.Facilitators = new LearningParticipantService( RockContext )
                .GetFacilitatorBags( classId )
                .ToList()
                .Select( f => new LearningClassFacilitatorBag
                {
                    FacilitatorName = f.Name,
                    FacilitatorRole = f.RoleName,
                    Facilitator = new ListItemBag
                    {
                        Value = f.Guid.ToString(),
                        Text = f.Name
                    }
                } )
                .ToList();

            return box;
        }

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

            var activities = activityCompletionService.GetClassActivities( currentPerson.Id, classId ).ToList().OrderBy( a => a.LearningActivity.Order);

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

            foreach ( var activity in activities )
            {
                var activityComponent = components.FirstOrDefault( c => c.Value.Value.EntityType.Id == activity.LearningActivity.ActivityComponentId ).Value.Value;

                // Ensure the configuration is scrubbed of any information the component deems not permissible for the student to view (e.g. correct answers).
                var studentScrubbedConfiguration = activityComponent.StudentScrubbedConfiguration( activity.LearningActivity.ActivityComponentSettingsJson );

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
                    ActivityComponentSettingsJson = studentScrubbedConfiguration,
                    AssignTo = activity.LearningActivity.AssignTo,
                    CurrentPerson = currentPersonBag,
                    Description = activity.LearningActivity.Description,
                    DescriptionAsHtml = activityDescriptionAsHtml,
                    IsStudentCommentingEnabled = activity.LearningActivity.IsStudentCommentingEnabled,
                    Name = activity.LearningActivity.Name,
                    Order = activity.LearningActivity.Order,
                    Points = activity.LearningActivity.Points
                };

                studentActivities.Add( new LearningActivityCompletionBag
                {
                    IdKey = activity.IdKey,
                    ActivityBag = activityBag,
                    ActivityComponentCompletionJson = activity.ActivityComponentCompletionJson,
                    AvailableDate = activity.AvailableDate,
                    BinaryFile = binaryFile,
                    CompletedDate = activity.CompletedDate,
                    DueDate = activity.DueDate,
                    FacilitatorComment = activity.FacilitatorComment,
                    GradeText = activity.GradeText( scales ),
                    IsGradePassing = activity.LearningActivity.Points == 0 || activity.Grade( scales ).IsPassing,
                    IsFacilitatorCompleted = activity.IsFacilitatorCompleted,
                    IsStudentCompleted = activity.IsStudentCompleted,
                    PointsEarned = activity.PointsEarned,
                    Student = currentPersonBag,
                    StudentComment = activity.StudentComment,
                    WasCompletedOnTime = activity.WasCompletedOnTime
                } );
            }

            return studentActivities;
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

        #endregion

        #region Block Actions


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

                activity.ActivityComponentCompletionJson = activityCompletionBag.ActivityComponentCompletionJson;
                activity.BinaryFileId = activityCompletionBag.BinaryFile.GetEntityId<BinaryFile>( rockContext );
                activity.IsStudentCompleted = true;

                var currentPerson = GetCurrentPerson();
                activity.CompletedByPersonAliasId = currentPerson.PrimaryAliasId;

                if ( !activity.CompletedDate.HasValue )
                {
                    var now = RockDateTime.Now;
                    activity.CompletedDate = now;
                    activity.WasCompletedOnTime = activity.DueDate > now;
                }

                rockContext.SaveChanges();

                activityCompletionBag.IsStudentCompleted = true;
                activityCompletionBag.CompletedDate = activity.CompletedDate;
                activityCompletionBag.WasCompletedOnTime = activity.WasCompletedOnTime;

                // Return the raw component settings so a grade can be computed (if applicable).
                activityCompletionBag.ActivityBag.ActivityComponentSettingsJson = activity.LearningActivity.ActivityComponentSettingsJson;

                return ActionOk( activityCompletionBag );
            }
        }


        #endregion
    }
}
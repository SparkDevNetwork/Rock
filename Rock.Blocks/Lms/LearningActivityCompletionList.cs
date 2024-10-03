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
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Lms.LearningActivityCompletionList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Lms
{
    /// <summary>
    /// Displays a list of learning activity completions.
    /// </summary>

    [DisplayName( "Learning Activity Completion List" )]
    [Category( "LMS" )]
    [Description( "Displays a list of learning activity completions." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the learning activity completion details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "152fea00-5721-4cb2-897f-1b6829f4b7c4" )]
    [Rock.SystemGuid.BlockTypeGuid( "ef1a5cdd-6769-4ffc-b826-55c194b01897" )]
    [CustomizedGrid]
    public class LearningActivityCompletionList : RockEntityListBlockType<LearningActivityCompletion>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string LearningActivityId = "LearningActivityId";
            public const string AutoEdit = "autoEdit";
            public const string ReturnUrl = "returnUrl";
            public const string LearningActivityCompletionId = "LearningActivityCompletionId";
            public const string LearningProgramId = "LearningProgramId";
            public const string LearningCourseId = "LearningCourseId";
            public const string LearningClassId = "LearningClassId";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<LearningActivityCompletionListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = 5;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private LearningActivityCompletionListOptionsBag GetBoxOptions()
        {
            var options = new LearningActivityCompletionListOptionsBag();

            var classId = RequestContext.PageParameterAsId( PageParameterKey.LearningClassId );
            options.Students = new LearningParticipantService( RockContext ).GetParticipantBags( classId )
                .Where( p => !p.IsFacilitator )
                .Select( p => new ListItemBag
                {
                    Text = p.Name,
                    Value = p.Guid.ToString()
                } ).ToList();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var queryParams = new Dictionary<string, string>
            {
                [PageParameterKey.AutoEdit] = "true",
                [PageParameterKey.LearningActivityCompletionId] = "((Key))",
                [PageParameterKey.LearningProgramId] = PageParameter( PageParameterKey.LearningProgramId ),
                [PageParameterKey.LearningCourseId] = PageParameter( PageParameterKey.LearningCourseId ),
                [PageParameterKey.LearningClassId] = PageParameter( PageParameterKey.LearningClassId ),
                [PageParameterKey.LearningActivityId] = PageParameter( PageParameterKey.LearningActivityId )
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, queryParams )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<LearningActivityCompletion> GetListQueryable( RockContext rockContext )
        {
            var activityId = RequestContext.PageParameterAsId( PageParameterKey.LearningActivityId );

            return base.GetListQueryable( rockContext )
                .Include( a => a.Student )
                .Include( a => a.LearningActivity )
                .Include( a => a.Student.Person )
                .Include( a => a.LearningActivity.LearningClass.LearningSemester )
                .Include( a => a.Student.LearningClass.LearningGradingSystem.LearningGradingSystemScales )
                .Where( a => a.LearningActivityId == activityId );
        }

        /// <inheritdoc/>
        protected override GridBuilder<LearningActivityCompletion> GetGridBuilder()
        {
            return new GridBuilder<LearningActivityCompletion>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddPersonField( "student", a => a.Student?.Person )
                .AddField( "studentGuid", a => a.Student.Guid )
                .AddField( "completionDate", a => a.CompletedDateTime )
                .AddField( "dueDate", a => a.DueDate )
                .AddField( "pointsEarned", a => a.PointsEarned )
                .AddField( "points", a => a.LearningActivity.Points )
                .AddField( "grade", a => a.GetGradeText() )
                .AddField( "gradePercent", a => a.GradePercent.ToIntSafe() )
                .AddField( "requiresScoring", a => a.RequiresScoring )
                .AddField( "isPassingGrade", a => a.GetGrade()?.IsPassing )
                .AddField( "isLate", a => a.IsLate )
                .AddField( "isCompleted", a => a.CompletedDateTime.HasValue )
                .AddField( "hadExtension", a => a.HadExtension )
                .AddField( "dueDateCalculated", a => a.LearningActivity.DueDateCalculated )
                .AddTextField( "facilitatorComment", a => a.FacilitatorComment )
                .AddTextField( "studentComment", a => a.StudentComment );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Adds a <see cref="LearningActivityCompletion"/> record for the participant with the matching Guid.
        /// </summary>
        /// <param name="participantGuid">The Guid of the participant to add the completion activity for.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult AddStudent( Guid participantGuid )
        {
            if ( !GetIsAddEnabled() )
            {
                return ActionBadRequest( $"Not authorized to edit {LearningActivity.FriendlyTypeName}." );
            }

            var activity = new LearningActivityService( RockContext ).GetInclude( PageParameter( PageParameterKey.LearningActivityId ), a => a.LearningClass.LearningSemester );

            if ( activity == null )
            {
                return ActionBadRequest( $"The required {LearningActivity.FriendlyTypeName} was not found." );
            }

            var participant = new LearningParticipantService( RockContext ).GetParticipant( participantGuid, activity.LearningClassId )
                .Select( p => new
                {
                    p.Id,
                    EnrollmentDate = p.CreatedDateTime
                } )
                .FirstOrDefault();

            if ( participant == null )
            {
                return ActionBadRequest( $"The required {LearningParticipant.FriendlyTypeName} was not found." );
            }

            var completionService = new LearningActivityCompletionService( RockContext );

            var hasCompletion = completionService.Queryable().Any( c => c.Student.Id == participant.Id && c.LearningActivityId == activity.Id );

            if ( hasCompletion )
            {
                return ActionBadRequest( $"The {LearningActivityCompletion.FriendlyTypeName} already exists." );
            }

            var programCommunicationId = new LearningProgramService( RockContext ).GetSelect( PageParameter( PageParameterKey.LearningProgramId ), p => p.SystemCommunicationId );
            var completion = completionService.GetNew( activity, participant.Id, participant.EnrollmentDate, programCommunicationId );
            completionService.Add( completion );

            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion
    }
}

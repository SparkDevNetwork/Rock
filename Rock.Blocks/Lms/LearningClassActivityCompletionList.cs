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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Lms.LearningClassActivityCompletionList;
using Rock.ViewModels.Utility;

namespace Rock.Blocks.Lms
{
    /// <summary>
    /// Displays a list of learning class activity completions.
    /// </summary>
    [DisplayName( "Learning Class Activity Completion List" )]
    [Category( "LMS" )]
    [Description( "Displays a list of learning class activity completions." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the learning class activity completion details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "152fea00-5721-4cb2-897f-1b6829f4b7c4" )]
    [Rock.SystemGuid.BlockTypeGuid( "ef1a5cdd-6769-4ffc-b826-55c194b01897" )]
    [CustomizedGrid]
    public class LearningClassActivityCompletionList : RockEntityListBlockType<LearningClassActivityCompletion>
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
            public const string LearningClassActivityId = "LearningClassActivityId";
            public const string AutoEdit = "autoEdit";
            public const string ReturnUrl = "returnUrl";
            public const string LearningClassActivityCompletionId = "LearningClassActivityCompletionId";
            public const string LearningProgramId = "LearningProgramId";
            public const string LearningCourseId = "LearningCourseId";
            public const string LearningClassId = "LearningClassId";
            public const string LearningParticipantId = "LearningParticipantId";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<LearningClassActivityCompletionListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
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
        private LearningClassActivityCompletionListOptionsBag GetBoxOptions()
        {
            var options = new LearningClassActivityCompletionListOptionsBag();

            var learningClass = new LearningClassService( RockContext )
                .Get( PageParameter( PageParameterKey.LearningClassId ), !PageCache.Layout.Site.DisablePredictableIds );

            options.CanViewGrades = learningClass.IsAuthorized( Authorization.VIEW_GRADES, GetCurrentPerson() );
            options.Students = new LearningParticipantService( RockContext ).GetParticipantBags( learningClass.Id )
                .Where( p => !p.IsFacilitator )
                .Select( p => new ListItemBag
                {
                    Text = p.Name,
                    Value = p.Guid.ToString()
                } ).ToList();

            return options;
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
                [PageParameterKey.LearningClassActivityCompletionId] = "((Key))",
                [PageParameterKey.LearningProgramId] = PageParameter( PageParameterKey.LearningProgramId ),
                [PageParameterKey.LearningCourseId] = PageParameter( PageParameterKey.LearningCourseId ),
                [PageParameterKey.LearningClassId] = PageParameter( PageParameterKey.LearningClassId ),
                [PageParameterKey.LearningClassActivityId] = PageParameter( PageParameterKey.LearningClassActivityId ),
                [PageParameterKey.LearningParticipantId] = "((LearningParticipantId))"
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, queryParams )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<LearningClassActivityCompletion> GetListQueryable( RockContext rockContext )
        {
            var learningClassActivityId = RequestContext.PageParameterAsId( PageParameterKey.LearningClassActivityId );

            return new LearningClassActivityCompletionService( rockContext ).Queryable()
                .Where( c => c.LearningClassActivityId == learningClassActivityId );
        }

        /// <inheritdoc/>
        protected override IQueryable<LearningClassActivityCompletion> GetOrderedListQueryable( IQueryable<LearningClassActivityCompletion> queryable, RockContext rockContext )
        {
            return queryable
                .OrderBy( c => c.Student.Person.NickName )
                .ThenBy( c => c.Student.Person.LastName );
        }

        protected override List<LearningClassActivityCompletion> GetListItems( IQueryable<LearningClassActivityCompletion> queryable, RockContext rockContext )
        {
            return queryable.ToList()
                .Where( lcac => lcac.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .ToList();
        }

        /// <inheritdoc/>
        protected override GridBuilder<LearningClassActivityCompletion> GetGridBuilder()
        {
            var learningClass = new LearningClassService( RockContext ).Get( PageParameter(PageParameterKey.LearningClassId ) );

            var canViewGrades = learningClass != null && learningClass.IsAuthorized( Authorization.VIEW_GRADES, GetCurrentPerson() );

            return new GridBuilder<LearningClassActivityCompletion>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "key", a => a.Id == 0 ? $"{a.StudentId}|LearningParticipantId" : $"{a.IdKey}|Key" )
                .AddPersonField( "student", a => a.Student?.Person )
                .AddField( "studentGuid", a => a.Student.Guid )
                .AddField( "completionDate", a => a.CompletedDateTime )
                .AddField( "dueDate", a => a.DueDate )
                .AddField( "pointsEarned", a => canViewGrades ? a.PointsEarned : null )
                .AddField( "points", a => !canViewGrades ? 0 : a.LearningClassActivity.Points )
                .AddField( "grade", a => !canViewGrades || a.RequiresGrading || a.LearningClassActivity.Points == 0 ? null : a.GetGradeText() )
                .AddField( "gradePercent", a => !canViewGrades ? 0 : a.GradePercent.ToIntSafe() )
                .AddField( "requiresScoring", a => a.RequiresGrading )
                .AddField( "isPassingGrade", a => !canViewGrades ? null : a.GetGrade()?.IsPassing )
                .AddField( "isLate", a => a.IsLate )
                .AddField( "isCompleted", a => a.CompletedDateTime.HasValue )
                .AddField( "hadExtension", a => a.HadExtension )
                .AddField( "dueDateCalculated", a => a.LearningClassActivity.DueDateCalculated )
                .AddTextField( "facilitatorComment", a => a.FacilitatorComment )
                .AddTextField( "studentComment", a => a.StudentComment );
        }

        #endregion

        #region Block Actions

        #endregion
    }
}

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
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Lms.LearningCourseList;
using Rock.Web.Cache;

namespace Rock.Blocks.Lms
{
    /// <summary>
    /// Displays a list of learning courses.
    /// </summary>
    [DisplayName( "Learning Course List" )]
    [Category( "LMS" )]
    [Description( "Displays a list of learning courses." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the learning course details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "e882d582-bc31-4b68-945b-d0d44a2ce5bc" )]
    [Rock.SystemGuid.BlockTypeGuid( "d0afdf98-4afc-4e4f-a6e2-07ca4e7358e8" )]
    [CustomizedGrid]
    public class LearningCourseList : RockEntityListBlockType<LearningCourse>
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
            public const string LearningProgramId = "LearningProgramId";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<LearningCourseListOptionsBag>();
            var builder = GetGridBuilder();

            var isEditAuthorized = GetIsAddEnabled();
            box.IsAddEnabled = isEditAuthorized;
            box.IsDeleteEnabled = true;
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
        private LearningCourseListOptionsBag GetBoxOptions()
        {
            var options = new LearningCourseListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var entity = new LearningCourse
            {
                LearningProgramId = RequestContext.PageParameterAsId( PageParameterKey.LearningProgramId )
            };

            return entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var queryParams = new Dictionary<string, string>
            {
                { PageParameterKey.LearningProgramId, PageParameter( PageParameterKey.LearningProgramId ) },
                { "LearningCourseId", "((Key))" }
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, queryParams )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<LearningCourse> GetListQueryable( RockContext rockContext )
        {
            var learningProgramId = RequestContext.PageParameterAsId( PageParameterKey.LearningProgramId );

            // Eagerly load the program so it can be checked for Authorization.
            return new LearningCourseService( rockContext ).Queryable()
                .Include( c => c.LearningProgram )
                .Where( c => c.LearningProgramId == learningProgramId );
        }

        /// <inheritdoc/>
        protected override List<LearningCourse> GetListItems( IQueryable<LearningCourse> queryable, RockContext rockContext )
        {
            return queryable.ToList()
                .Where( c => c.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .ToList();
        }

        /// <inheritdoc/>
        protected override GridBuilder<LearningCourse> GetGridBuilder()
        {
            return new GridBuilder<LearningCourse>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "course", a => a.Name )
                .AddTextField( "summary", a => a.Summary )
                .AddTextField( "category", a => a.CategoryId.HasValue ? CategoryCache.Get( a.CategoryId.Value )?.Name : null )
                .AddTextField( "categoryIconCssClass", a => a.CategoryId.HasValue ? CategoryCache.Get( a.CategoryId.Value )?.IconCssClass : null )
                .AddTextField( "categoryColor", a => a.CategoryId.HasValue ? CategoryCache.Get( a.CategoryId.Value )?.HighlightColor : null )
                .AddField( "isPublic", a => a.IsPublic )
                .AddField( "isActive", a => a.IsActive )
                .AddTextField( "code", a => a.CourseCode )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Changes the ordered position of a single item.
        /// </summary>
        /// <param name="key">The identifier of the item that will be moved.</param>
        /// <param name="beforeKey">The identifier of the item it will be placed before.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult ReorderItem( string key, string beforeKey )
        {
            // Get the queryable and make sure it is ordered correctly.
            var qry = GetListQueryable( RockContext );
            qry = GetOrderedListQueryable( qry, RockContext );

            // Get the entities from the database.
            var items = GetListItems( qry, RockContext );

            if ( !items.ReorderEntity( key, beforeKey ) )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var courseService = new LearningCourseService( RockContext );

            var course = courseService
                    .GetInclude( key, c => c.LearningCourseRequirements, !PageCache.Layout.Site.DisablePredictableIds );

            if ( course == null )
            {
                return ActionBadRequest( $"{LearningCourse.FriendlyTypeName} not found." );
            }

            if ( !course.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {LearningCourse.FriendlyTypeName}." );
            }

            var requiredCourseService = new LearningCourseRequirementService( RockContext );
            var coursesDependingOnThisCourse = requiredCourseService
                .Queryable()
                .Where( c => c.RequiredLearningCourseId == course.Id )
                .Select( c => c.LearningCourse.Name );

            // If other courses are dependent on this one, don't allow deletion.
            if ( coursesDependingOnThisCourse.Count() > 0 )
            {
                var errorMessage = string.Format(
                    "This {0} is required by {1}: {2}.",
                    LearningCourse.FriendlyTypeName,
                    "course".PluralizeIf( coursesDependingOnThisCourse.Count() != 1 ),
                    coursesDependingOnThisCourse.JoinStringsWithCommaAnd() );
                return ActionBadRequest( errorMessage );
            }

            // Delete everything or nothing.
            RockContext.WrapTransaction( () =>
            {
                var classService = new LearningClassService( RockContext );
                // Include related entities that should be deleted along with the class.
                var classesForCourse = classService.Queryable()
                    .Include( c => c.LearningClassActivities )
                    .Include( c => c.LearningParticipants )
                    .Include( c => c.ContentPages )
                    .Include( c => c.Announcements )
                    .Where( c => c.LearningCourseId == course.Id );

                // Remove any classes for this course
                foreach ( var courseClass in classesForCourse )
                {
                    classService.Delete( courseClass );
                }

                // Remove any course requirements from this course.
                requiredCourseService.DeleteRange( course.LearningCourseRequirements );

                // Finally remove this course.
                courseService.Delete( course );
                RockContext.SaveChanges();
            } );

            return ActionOk();
        }

        #endregion
    }
}

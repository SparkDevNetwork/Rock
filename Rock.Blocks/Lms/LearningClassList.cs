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
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Lms.LearningClassList;
using Rock.Web.Cache;

namespace Rock.Blocks.Lms
{
    /// <summary>
    /// Displays a list of learning classes.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This list block was created for use by multiple pages in different contexts.
    ///         It's designed to be used on a page with a Learning Program Detail block or
    ///         on a page with a Course Detail block which provides additional filtering.
    ///     </para>
    /// </remarks>
    [DisplayName( "Learning Class List" )]
    [Category( "LMS" )]
    [Description( "Displays a list of learning classes." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [BooleanField(
        "Show Location Column",
        Key = AttributeKey.ShowLocationColumn,
        Description = "Determines if the Location column should be visible.",
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        IsRequired = true,
        DefaultBooleanValue = false,
        Order = 1 )]

    [BooleanField(
        "Show Schedule Column",
        Key = AttributeKey.ShowScheduleColumn,
        Description = "Determines if the Schedule column should be visible.",
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        IsRequired = true,
        DefaultBooleanValue = false,
        Order = 2 )]

    [BooleanField(
        "Show Semester Column",
        Key = AttributeKey.ShowSemesterColumn,
        Description = "Determines if the Semester column should be visible when the configuration is 'Academic Calendar'.",
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        IsRequired = true,
        DefaultBooleanValue = false,
        Order = 3 )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the learning class details.",
        Key = AttributeKey.DetailPage,
        Order = 4 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "ab72d147-d4ca-4ff5-ab49-696319cb9844" )]
    [Rock.SystemGuid.BlockTypeGuid( "340f6cc1-8c38-4579-9383-a6168680194a" )]
    [CustomizedGrid]
    public class LearningClassList : RockEntityListBlockType<LearningClass>
    {
        #region Keys

        private const string ShowHideListSource = "Yes^Show,No^Hide";

        private static class DisplayMode
        {
            public const string AcademicCalendarOnly = "AcademicCalendarOnly";
            public const string Always = "Always";
        }

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string ShowSemesterColumn = "ShowSemesterColumn";
            public const string ShowLocationColumn = "ShowLocationColumn";
            public const string ShowScheduleColumn = "ShowScheduleColumn";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string LearningProgramId = "LearningProgramId";
            public const string LearningCourseId = "LearningCourseId";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<LearningClassListOptionsBag>();

            var isAddEnabled = GetIsAddEnabled();
            box.IsAddEnabled = isAddEnabled;
            box.IsDeleteEnabled = true;
            box.ExpectedRowCount = 5;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            var builder = GetGridBuilder();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private LearningClassListOptionsBag GetBoxOptions()
        {
            var options = new LearningClassListOptionsBag();

            var courseKey = PageParameter( PageParameterKey.LearningCourseId ) ?? string.Empty;
            var isNewCourse = courseKey == "0";
            var course = !isNewCourse && courseKey.Length > 0 ? new LearningCourseService( RockContext ).GetInclude( courseKey, c => c.LearningProgram ) : null;
            
            var programKey = PageParameter( PageParameterKey.LearningProgramId ) ?? string.Empty;
            var isNewProgram = programKey == "0";
            var isOnDemandConfigurationMode = false;
            if (!isNewProgram && programKey.Length > 0)
            {
                var configurationMode = new LearningProgramService( RockContext )
                    .GetSelect( programKey, p => p.ConfigurationMode, !PageCache.Layout.Site.DisablePredictableIds );

                isOnDemandConfigurationMode = configurationMode == Enums.Lms.ConfigurationMode.OnDemandLearning;
            }
            
            // Only add the course column if the results aren't filtered to a course already.
            options.HasValidCourse = course?.Id > 0;
            options.ShowCourseColumn = !options.HasValidCourse;

            options.ShowLocationColumn = GetAttributeValue( AttributeKey.ShowLocationColumn ).AsBoolean();
            options.ShowScheduleColumn = GetAttributeValue( AttributeKey.ShowScheduleColumn ).AsBoolean();
            options.ShowSemesterColumn = GetAttributeValue( AttributeKey.ShowSemesterColumn ).AsBoolean() && !isOnDemandConfigurationMode;

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var entity = new LearningClass
            {
                LearningCourseId = RequestContext.PageParameterAsId( PageParameterKey.LearningCourseId )
            };

            return entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var courseKey = PageParameter( PageParameterKey.LearningCourseId ) ?? string.Empty;
            var queryParams = new Dictionary<string, string>
            {
                [PageParameterKey.LearningProgramId] = PageParameter( PageParameterKey.LearningProgramId ),
                [PageParameterKey.LearningCourseId] = courseKey.Length > 0 ? courseKey : "((LearningCourseIdKey))",
                ["LearningClassId"] = "((Key))"
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, queryParams )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<LearningClass> GetListQueryable( RockContext rockContext )
        {
            // Eagerly load the LearningProgram in case it needs to be checked for VIEW authorization.
            var baseQuery = new LearningClassService( rockContext )
                .Queryable()
                .Include( c => c.LearningCourse )
                .Include( c => c.LearningCourse.LearningProgram )
                .Include( c => c.LearningSemester )
                .Include( c => c.LearningParticipants )
                .Include( c => c.LearningParticipants.Select( p => p.LearningClassActivityCompletions ));

            var programId = RequestContext.PageParameterAsId( PageParameterKey.LearningProgramId );
            if ( programId > 0 )
            {
                baseQuery = baseQuery.Where( c => c.LearningCourse.LearningProgramId == programId );
            }

            var courseId = RequestContext.PageParameterAsId( PageParameterKey.LearningCourseId );
            if ( courseId > 0 )
            {
                // When there's a course defined include all classes.
                baseQuery = baseQuery.Where( c => c.LearningCourseId == courseId );
            }
            else
            {
                // When there's no course selected show only active classes.
                baseQuery = baseQuery.Where( c => c.IsActive );
            }

            return baseQuery;
        }

        /// <inheritdoc/>
        protected override IQueryable<LearningClass> GetOrderedListQueryable( IQueryable<LearningClass> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( c => c.LearningCourse.Name ).ThenBy( c => c.Name );
        }

        /// <inheritdoc/>
        protected override List<LearningClass> GetListItems( IQueryable<LearningClass> queryable, RockContext rockContext )
        {
            return queryable.ToList()
                .Where( lc => lc.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .ToList();
        }

        /// <inheritdoc/>
        protected override GridBuilder<LearningClass> GetGridBuilder()
        {
            var grid = new GridBuilder<LearningClass>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddField( "facilitators", a => a.LearningParticipants
                    .Where( p => p.GroupRole.IsLeader )
                    .Select( p => p.Person.FullName )
                    .OrderBy( p => p )
                    .JoinStrings( ", " ) )
                .AddField( "students", a => a.LearningParticipants.Count( p => !p.GroupRole?.IsLeader ?? false ) )
                .AddTextField( "course", a => a.LearningCourse.Name )
                .AddTextField( "learningCourseIdKey", a => a.LearningCourse.IdKey )
                .AddTextField( "code", a => a.LearningCourse.CourseCode )
                .AddTextField( "className", a => a.Name )
                .AddField( "isActive", a => a.IsActive )
                .AddField( "isPublic", a => a.IsPublic )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) );

            if ( GetAttributeValue( AttributeKey.ShowSemesterColumn ).AsBoolean() )
            {
                grid.AddTextField( "semester", a => a.LearningSemester?.Name );
            }

            if ( GetAttributeValue( AttributeKey.ShowLocationColumn ).AsBoolean() )
            {
                grid.AddTextField( "location", a => a.GroupLocations?.FirstOrDefault()?.Location.Name );
            }

            if ( GetAttributeValue( AttributeKey.ShowScheduleColumn ).AsBoolean() )
            {
                grid.AddTextField( "schedule", a => a.Schedule?.Name );
            }

            return grid;
        }

        #endregion

        #region Block Actions
        
        /// <summary>
        /// Copy the Class to create as a new Class
        /// </summary>
        [BlockAction]
        public BlockActionResult Copy( string key )
        {
            if ( key.IsNullOrWhiteSpace() )
            {
                return ActionNotFound();
            }

            var learningClassService = new LearningClassService( RockContext );
            var copiedEntity = learningClassService.Copy( key );
            var currentPageParams = new Dictionary<string, string>
            {
                [PageParameterKey.LearningProgramId] = PageParameter( PageParameterKey.LearningProgramId )
            };

            var courseIdPageParameter = PageParameter( PageParameterKey.LearningCourseId );

            // When creating the return url only include the LearningCourseId
            // if it's present for the current page ( avoid an empty query string param).
            if ( courseIdPageParameter.IsNotNullOrWhiteSpace() )
            {
                currentPageParams.Add( PageParameterKey.LearningCourseId, courseIdPageParameter );
            }

            var returnUrl = this.GetCurrentPageUrl( currentPageParams );

            // After getting the returnUrl ensure that the detail page includes the LearningCourseIdKey.
            if ( courseIdPageParameter.IsNullOrWhiteSpace() )
            {
                var courseIdKeyFromCopiedClass = IdHasher.Instance.GetHash( copiedEntity.LearningCourseId );
                currentPageParams.AddOrReplace( PageParameterKey.LearningCourseId, courseIdKeyFromCopiedClass );
            }

            // Add the LearningClassId, autoEdit and returnUrl to the current page parameters.
            var queryParams = new Dictionary<string, string>( currentPageParams )
            {
                ["LearningClassId"] = copiedEntity.IdKey,
                ["autoEdit"] = true.ToString(),
                ["returnUrl"] = returnUrl
            };

            return ActionContent( System.Net.HttpStatusCode.Created, this.GetLinkedPageUrl( AttributeKey.DetailPage, queryParams ) );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new LearningClassService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{LearningClass.FriendlyTypeName} not found." );
                }

                if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete {LearningClass.FriendlyTypeName}." );
                }

                entityService.Delete( entity.Id );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Determines if the specific class <paramref name="key"/> has any activity completions.
        /// </summary>
        /// <param name="key">The identifier of the class to be evaluated.</param>
        /// <returns><c>true</c> if the class has activity completion records; otherwise <c>false</c>.</returns>
        [BlockAction]
        public BlockActionResult HasStudentCompletions( string key )
        {
            var classId = IdHasher.Instance.GetId( key );
            var hasCompletions = new LearningParticipantService( RockContext )
                .Queryable()
                .Any( p =>
                    p.LearningClassId == classId
                    && p.LearningClassActivityCompletions.Any() );

            return ActionOk( hasCompletions );
        }

        #endregion
    }
}

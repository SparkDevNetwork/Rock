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
using Rock.ViewModels.Blocks.Lms.LearningClassList;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Lms
{
    /// <summary>
    /// Displays a list of learning classes.
    /// </summary>

    [DisplayName( "Learning Class List" )]
    [Category( "LMS" )]
    [Description( "Displays a list of learning classes." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ContextAware( typeof( LearningProgram ), typeof( LearningCourse ) )]

    #region Block Attributes

    [CustomDropdownListField(
        "Show Location Column",
        Key = AttributeKey.ShowLocationColumn,
        Description = "Select 'Show' to show the 'Location'.",
        ListSource = ShowHideListSource,
        IsRequired = true,
        DefaultValue = "No",
        Order = 1 )]

    [CustomDropdownListField(
        "Show Schedule Column",
        Key = AttributeKey.ShowScheduleColumn,
        Description = "Select 'Show' to show the 'Schedule' column.",
        ListSource = ShowHideListSource,
        IsRequired = true,
        DefaultValue = "No",
        Order = 2 )]

    [CustomDropdownListField(
        "Show Semester Column",
        Key = AttributeKey.ShowSemesterColumn,
        Description = "Select 'Show' to show the 'Semester' column when the configuration is 'Academic Calendar'.",
        ListSource = ShowHideListSource,
        IsRequired = true,
        DefaultValue = "No",
        Order = 3 )]

    [CustomDropdownListField(
        "Display Mode",
        Key = AttributeKey.DisplayMode,
        Description = "Select 'Show only Acadmemic Calendar Mode' to show the block only when the configuration mode is 'Academic Calendar'.",
        ListSource = DisplayModeListSource,
        IsRequired = true,
        DefaultValue = "AcademicCalendarOnly",
        Order = 4 )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the learning class details.",
        Key = AttributeKey.DetailPage,
        Order = 5 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "ab72d147-d4ca-4ff5-ab49-696319cb9844" )]
    [Rock.SystemGuid.BlockTypeGuid( "340f6cc1-8c38-4579-9383-a6168680194a" )]
    [CustomizedGrid]
    public class LearningClassList : RockEntityListBlockType<LearningClass>
    {
        #region Keys

        private const string ShowHideListSource = "Yes^Show,No^Hide";
        private const string DisplayModeListSource = "AcademicCalendarOnly^Show only Acadmemic Calendar Mode,Always^Always show the block";

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
            public const string DisplayMode = "DisplayMode";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<LearningClassListOptionsBag>();

            var isEditEnabled = GetIsEditEnabled();
            box.IsAddEnabled = isEditEnabled;
            box.IsDeleteEnabled = isEditEnabled;
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

            // Only add the course column if the results aren't filtered to a course already.
            options.ShowCourseColumn = RequestContext.GetContextEntity<LearningCourse>() == null;

            options.ShowLocationColumn = GetAttributeValue( AttributeKey.ShowLocationColumn ).AsBoolean();
            options.ShowScheduleColumn = GetAttributeValue( AttributeKey.ShowScheduleColumn ).AsBoolean();
            options.ShowSemesterColumn = GetAttributeValue( AttributeKey.ShowSemesterColumn ).AsBoolean();

            // Show the block if the block setting for ShowOnlyInAcademicCalendarMode is false
            // or the program context entity is academic calendar mode.
            var showOnlyForAcademicCalendarMode = GetAttributeValue( AttributeKey.DisplayMode ).ToStringSafe() == DisplayMode.AcademicCalendarOnly;
            options.ShowBlock = !showOnlyForAcademicCalendarMode || ContextEntityIsAcademicCalendarMode();

            return options;
        }

        private bool ContextEntityIsAcademicCalendarMode()
        {
            var programContextEntity = RequestContext.GetContextEntity<LearningProgram>();
            if ( programContextEntity?.ConfigurationMode == ConfigurationMode.AcademicCalendar )
            {
                return true;
            }

            var courseContextEntity = RequestContext.GetContextEntity<LearningCourse>();
            if (courseContextEntity?.LearningProgram?.ConfigurationMode == ConfigurationMode.AcademicCalendar )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsEditEnabled()
        {
            var entity = new LearningClass();

            return entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "LearningClassId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<LearningClass> GetListQueryable( RockContext rockContext )
        {
            var baseQuery = base.GetListQueryable( rockContext )
                .Include( c => c.LearningCourse )
                .Include( c => c.LearningSemester )
                .Include( c => c.LearningParticipants );

            // If the Page is includes a LearningProgram block and the Page Context Entity is set
            // then filter the courses to that program.
            var programContextEntity = RequestContext.GetContextEntity<LearningProgram>();
            if ( programContextEntity?.Id > 0 )
            {
                baseQuery = baseQuery.Where( c => c.LearningCourse.LearningProgramId == programContextEntity.Id );
            }

            // If the Page is includes a LearningCourse block and the Page Context Entity is set
            // then filter the courses to that course.
            var courseContextEntity = RequestContext.GetContextEntity<LearningCourse>();
            if ( courseContextEntity?.Id > 0 )
            {
                baseQuery = baseQuery.Where( c => c.LearningCourseId == courseContextEntity.Id );
            }

            return baseQuery;
        }

        /// <inheritdoc/>
        protected override GridBuilder<LearningClass> GetGridBuilder()
        {
            var grid = new GridBuilder<LearningClass>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddField( "facilitators", a => a.LearningParticipants.Where( p => p.GroupRole.IsLeader ).Select( p => p.Person.FullName ).JoinStrings( ", " ) )
                .AddTextField( "category", a => a.LearningCourse.CategoryId.HasValue ? CategoryCache.Get( a.LearningCourse.CategoryId.Value )?.Name : null)
                .AddTextField( "categoryColor", a => a.LearningCourse.CategoryId.HasValue ? CategoryCache.Get( a.LearningCourse.CategoryId.Value )?.HighlightColor : null )
                .AddField( "students", a => a.LearningParticipants.Count( p => !p.GroupRole?.IsLeader ?? false) )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) );

            // Only add the course column if the results aren't filtered to a course already.
            if ( RequestContext.GetContextEntity<LearningCourse>() == null )
            {
                grid.AddTextField( "course", a => a.LearningCourse.Name );
                grid.AddTextField( "code", a => a.LearningCourse.CourseCode );
            }

            if ( GetAttributeValue( AttributeKey.ShowSemesterColumn ).AsBoolean() )
            {
                grid.AddTextField( "semester", a => a.GroupLocations?.FirstOrDefault()?.Location.Name );
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
                    return ActionBadRequest( $"Not authorized to delete ${LearningClass.FriendlyTypeName}." );
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        #endregion
    }
}

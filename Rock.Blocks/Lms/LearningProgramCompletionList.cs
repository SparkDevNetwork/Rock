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
using Rock.Enums.Lms;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Lms.LearningProgramCompletionList;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Lms
{
    /// <summary>
    /// Displays a list of learning program completions.
    /// </summary>
    [DisplayName( "Learning Program Completion List" )]
    [Category( "LMS" )]
    [Description( "Displays a list of learning program completions." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ContextAware( typeof( LearningProgram ) )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the learning program completion details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "a4e78cb9-b53c-4188-bb9a-4435c051916d" )]
    [Rock.SystemGuid.BlockTypeGuid( "ce703eb6-028f-47fc-a09a-ad8f72c12cbc" )]
    [CustomizedGrid]
    public class LearningProgramCompletionList : RockEntityListBlockType<LearningProgramCompletion>
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

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<LearningProgramCompletionListOptionsBag>();
            var builder = GetGridBuilder();

            box.ExpectedRowCount = 10;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private LearningProgramCompletionListOptionsBag GetBoxOptions()
        {
            var options = new LearningProgramCompletionListOptionsBag();

            var learningProgramIsAcademicMode = RequestContext.GetContextEntity<LearningProgram>()?.ConfigurationMode == ConfigurationMode.AcademicCalendar;

            options.ShowSemesterColumn = learningProgramIsAcademicMode;

            return options;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "LearningProgramCompletionId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<LearningProgramCompletion> GetListQueryable( RockContext rockContext )
        {
            var queryable = base.GetListQueryable( rockContext )
                .Include( a => a.PersonAlias )
                .Include( a => a.PersonAlias.Person )
                .Include( a => a.LearningProgram );

            var contextProgram = RequestContext.GetContextEntity<LearningProgram>();

            if ( contextProgram?.Id > 0 )
            {
                queryable = queryable.Where( a => a.LearningProgramId == contextProgram.Id );
            }

            return queryable;
        }

        /// <inheritdoc/>
        protected override GridBuilder<LearningProgramCompletion> GetGridBuilder()
        {
            var grid = new GridBuilder<LearningProgramCompletion>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddPersonField( "individual", a => a.PersonAlias?.Person )
                .AddTextField( "campus", a => a.CampusId.HasValue ? CampusCache.Get( a.CampusId.Value )?.Name : null )
                .AddDateTimeField( "startDate", a => a.StartDate )
                .AddDateTimeField( "endDate", a => a.EndDate )
                .AddField( "status", a => a.CompletionStatus );

            if ( RequestContext.GetContextEntity<LearningProgram>()?.ConfigurationMode == ConfigurationMode.AcademicCalendar )
            {
                grid.AddTextField( "semester", a => a.LearningProgram.LearningSemesters?
                    .FirstOrDefault( s =>
                        s.StartDate >= a.StartDate &&
                        s.EndDate <= a.EndDate
                    )?.Name
                );
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
                var entityService = new LearningProgramCompletionService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{LearningProgramCompletion.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${LearningProgramCompletion.FriendlyTypeName}." );
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

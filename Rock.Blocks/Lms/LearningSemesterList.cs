﻿// <copyright>
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
using Rock.ViewModels.Blocks.Lms.LearningSemesterList;
using Rock.Web.Cache;

namespace Rock.Blocks.Lms
{
    /// <summary>
    /// Displays a list of learning semesters.
    /// </summary>
    [DisplayName( "Learning Semester List" )]
    [Category( "LMS" )]
    [Description( "Displays a list of learning semesters." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the learning semester details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "928978c0-9695-454d-9e17-33f12f278f78" )]
    [Rock.SystemGuid.BlockTypeGuid( "c89c7f15-fb8a-43d4-9afb-5e40e397f246" )]
    [CustomizedGrid]
    public class LearningSemesterList : RockEntityListBlockType<LearningSemester>
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
            var box = new ListBlockBox<LearningSemesterListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
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
        private LearningSemesterListOptionsBag GetBoxOptions()
        {
            var options = new LearningSemesterListOptionsBag();

            var progam = new LearningProgramService( RockContext ).Get( PageParameter( PageParameterKey.LearningProgramId ), !PageCache.Layout.Site.DisablePredictableIds );

            if ( progam != null )
            {
                options.CanEditProgram = progam.IsAuthorized( Authorization.EDIT, GetCurrentPerson() );
                options.LearningProgramIdKey = Rock.Utility.IdHasher.Instance.GetHash( progam.Id );
                options.LearningProgramName = progam.Name;
            }

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var entity = new LearningSemester
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
                [PageParameterKey.LearningProgramId] = PageParameter( PageParameterKey.LearningProgramId ),
                ["LearningSemesterId"] = "((Key))"
            };
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, queryParams )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<LearningSemester> GetListQueryable( RockContext rockContext )
        {
            var learningProgramId = RequestContext.PageParameterAsId( PageParameterKey.LearningProgramId );

            return new LearningSemesterService( rockContext ).Queryable()
                .Include( a => a.LearningClasses )
                .Where( ls => ls.LearningProgramId == learningProgramId );
        }

        /// <inheritdoc/>
        protected override IQueryable<LearningSemester> GetOrderedListQueryable( IQueryable<LearningSemester> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( a => a.StartDate );
        }

        /// <inheritdoc/>
        protected override List<LearningSemester> GetListItems( IQueryable<LearningSemester> queryable, RockContext rockContext )
        {
            return queryable.ToList()
                .Where( ls => ls.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .ToList();
        }

        /// <inheritdoc/>
        protected override GridBuilder<LearningSemester> GetGridBuilder()
        {
            return new GridBuilder<LearningSemester>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddDateTimeField( "startDate", a => a.StartDate )
                .AddDateTimeField( "endDate", a => a.EndDate )
                .AddDateTimeField( "closeDate", a => a.EnrollmentCloseDate )
                .AddField( "classCount", a => a.LearningClasses.Count() );
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
                var entityService = new LearningSemesterService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{LearningSemester.FriendlyTypeName} not found." );
                }

                if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete {LearningSemester.FriendlyTypeName}." );
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

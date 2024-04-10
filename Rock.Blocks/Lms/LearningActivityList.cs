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
using Rock.ViewModels.Blocks.Lms.LearningActivityList;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Lms
{
    /// <summary>
    /// Displays a list of learning activities.
    /// </summary>

    [DisplayName( "Learning Activity List" )]
    [Category( "LMS" )]
    [Description( "Displays a list of learning activities." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ContextAware( typeof( LearningCourse ), typeof( LearningClass ) )]

    [Rock.SystemGuid.EntityTypeGuid( "6359866e-4864-4baa-8a7f-394522e9ef0f" )]
    [Rock.SystemGuid.BlockTypeGuid( "5ceb6ec7-69f5-43b6-a74f-144a57f9b465" )]
    [CustomizedGrid]
    public class LearningActivityList : RockEntityListBlockType<LearningActivity>
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
            var box = new ListBlockBox<LearningActivityListOptionsBag>();
            var builder = GetGridBuilder();

            var isEditEnabled = GetIsAddEnabled();
            box.IsAddEnabled = isEditEnabled;
            box.IsDeleteEnabled = isEditEnabled;
            box.ExpectedRowCount = 3;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private LearningActivityListOptionsBag GetBoxOptions()
        {
            var options = new LearningActivityListOptionsBag();

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
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "LearningActivityId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<LearningActivity> GetListQueryable( RockContext rockContext )
        {
            var baseQuery = base.GetListQueryable( rockContext )
                .Include( a => a.LearningActivityCompletions );

            var contextCourse = RequestContext.GetContextEntity<LearningCourse>();
            var contextClass = RequestContext.GetContextEntity<LearningClass>();
            if (
                contextCourse != null &&
                contextCourse.LearningProgram?.ConfigurationMode == ConfigurationMode.OnDemandLearning &&
                contextCourse.LearningClasses.FirstOrDefault() != null )
            {
                var classId = contextCourse.LearningClasses.FirstOrDefault().Id;
                baseQuery = baseQuery.Where( a => a.LearningClassId == classId);
            }
            else if ( contextClass?.Id > 0 )
            {
                baseQuery = baseQuery.Where( a => a.LearningClassId == contextClass.Id);
            }

            return baseQuery;
        }

        /// <inheritdoc/>
        protected override GridBuilder<LearningActivity> GetGridBuilder()
        {
            var now = DateTime.Now;
            return new GridBuilder<LearningActivity>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddField( "assignTo", a => a.AssignTo )
                .AddField( "type", a => a.ActivityComponentId )
                .AddField( "dates", a => a.DatesDescription )
                .AddField( "isPastDue", a => a.DueDateCalculated == null ? false : a.DueDateCalculated >= now )
                .AddField( "count", a => a.LearningActivityCompletions.Count() )
                .AddField( "completedCount", a => a.LearningActivityCompletions.Count( c => c.IsStudentCompleted ) )
                .AddField( "componentIconCssClass", a => "fa fa-list" )
                .AddField( "componentHighlightColor", a => "#735f95" )
                .AddField( "componentName", a => "Check-Off" )
                .AddField( "points", a => a.Points )
                .AddField( "isAttentionNeeded", a => a.LearningActivityCompletions.Any( c => c.IsStudentCompleted && !c.IsFacilitatorCompleted ) )
                .AddField( "hasStudentComments", a => a.LearningActivityCompletions.Any( c => c.StudentComment.ToStringSafe().Length > 0 ) );
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
            using ( var rockContext = new RockContext() )
            {
                // Get the queryable and make sure it is ordered correctly.
                var qry = GetListQueryable( rockContext );
                qry = GetOrderedListQueryable( qry, rockContext );

                // Get the entities from the database.
                var items = GetListItems( qry, rockContext );

                if ( !items.ReorderEntity( key, beforeKey ) )
                {
                    return ActionBadRequest( "Invalid reorder attempt." );
                }

                rockContext.SaveChanges();

                return ActionOk();
            }
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
                var entityService = new LearningActivityService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{LearningActivity.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${LearningActivity.FriendlyTypeName}." );
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

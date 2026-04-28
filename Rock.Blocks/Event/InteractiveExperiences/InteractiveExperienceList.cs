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
using Rock.ViewModels.Blocks.Event.InteractiveExperiences.InteractiveExperienceList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Event.InteractiveExperiences
{
    /// <summary>
    /// Displays a list of interactive experiences.
    /// </summary>

    [DisplayName( "Interactive Experience List" )]
    [Category( "Event > Interactive Experiences" )]
    [Description( "Displays a list of interactive experiences." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the interactive experience details.",
        Key = AttributeKey.DetailPage )]

    [LinkedPage( "Occurrence Chooser Page",
        Description = "The page that will show the experience manager occurrence chooser.",
        Key = AttributeKey.OccurrenceChooserPage )]

    [Rock.SystemGuid.EntityTypeGuid( "B913F11C-6AD5-4510-A99E-9F5CF9C13933" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "1CCF82ED-DC7F-48E9-B2FA-12EF4F17605A" )]
    [Rock.SystemGuid.BlockTypeGuid( "BD89FE49-4DD2-4313-AFF8-ABAA97B3235D" )] // actual BlockTypeGuid from DB
    [CustomizedGrid]
    public class InteractiveExperienceList : RockEntityListBlockType<InteractiveExperience>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string OccurrenceChooserPage = "OccurrenceChooserPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
            public const string OccurrenceChooserPage = "OccurrenceChooserPage";
        }

        private static class PageParameterKey
        {
            public const string InteractiveExperienceId = "InteractiveExperienceId";
        }

        private static class PreferenceKey
        {
            public const string FilterCampus = "filter-campus";
            public const string FilterIncludeInactive = "filter-include-inactive";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets the campus filter Guid from the person preferences.
        /// </summary>
        protected Guid? FilterCampusGuid => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterCampus )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        /// <summary>
        /// Gets the include inactive filter value from the person preferences.
        /// </summary>
        protected bool FilterIncludeInactive => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterIncludeInactive )
            .AsBoolean();

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<InteractiveExperienceListOptionsBag>();
            var builder = GetGridBuilder();

            var isAddDeleteEnabled = GetIsAddDeleteEnabled();
            box.IsAddEnabled = isAddDeleteEnabled;
            box.IsDeleteEnabled = isAddDeleteEnabled;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private InteractiveExperienceListOptionsBag GetBoxOptions()
        {
            return new InteractiveExperienceListOptionsBag
            {
                IsManagerButtonVisible = GetAttributeValue( AttributeKey.OccurrenceChooserPage ).IsNotNullOrWhiteSpace()
            };
        }

        /// <summary>
        /// Determines if the add and delete buttons should be enabled in the grid.
        /// </summary>
        /// <returns>A boolean value that indicates if the add and delete buttons should be enabled.</returns>
        private bool GetIsAddDeleteEnabled()
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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, PageParameterKey.InteractiveExperienceId, "((Key))" ),
                [NavigationUrlKey.OccurrenceChooserPage] = this.GetLinkedPageUrl( AttributeKey.OccurrenceChooserPage, PageParameterKey.InteractiveExperienceId, "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<InteractiveExperience> GetListQueryable( RockContext rockContext )
        {
            var queryable = base.GetListQueryable( rockContext )
                .Include( ie => ie.InteractiveExperienceSchedules.Select( ies => ies.Schedule ) )
                .Include( ie => ie.InteractiveExperienceSchedules.Select( ies => ies.InteractiveExperienceScheduleCampuses.Select( iesc => iesc.Campus ) ) )
                .Include( ie => ie.InteractiveExperienceActions );

            // Apply campus filter.
            var campusGuid = FilterCampusGuid;
            if ( campusGuid.HasValue )
            {
                queryable = queryable.Where( ie => ie.InteractiveExperienceSchedules
                    .Any( ies => ies.InteractiveExperienceScheduleCampuses
                        .Any( iesc => iesc.Campus != null && iesc.Campus.Guid == campusGuid.Value ) ) );
            }

            // Apply active filter.
            if ( !FilterIncludeInactive )
            {
                queryable = queryable.Where( ie => ie.IsActive );
            }

            return queryable;
        }

        /// <inheritdoc/>
        protected override IQueryable<InteractiveExperience> GetOrderedListQueryable( IQueryable<InteractiveExperience> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( ie => ie.Name );
        }

        /// <inheritdoc/>
        protected override GridBuilder<InteractiveExperience> GetGridBuilder()
        {
            return new GridBuilder<InteractiveExperience>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddDateTimeField( "nextStartDateTime", a => GetNextStartDateTime( a ) )
                .AddField( "actionCount", a => a.InteractiveExperienceActions.Count )
                .AddTextField( "campus", a => GetCampusText( a ) )
                .AddField( "isActive", a => a.IsActive )
                .AddField( "hasOccurrences", a => a.HasActiveOccurrencesForDate( RockDateTime.Now ) );
        }

        /// <summary>
        /// Gets the next start date time for an experience across all of its schedules.
        /// </summary>
        /// <param name="experience">The interactive experience to determine the next start date of.</param>
        /// <returns>A <see cref="DateTime"/> representing the next start date and time or <c>null</c> if one is not available.</returns>
        private static DateTime? GetNextStartDateTime( InteractiveExperience experience )
        {
            return experience.InteractiveExperienceSchedules
                .Select( ies => ies.Schedule.GetNextStartDateTime( RockDateTime.Now ) )
                .Where( d => d.HasValue )
                .OrderBy( d => d )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the text to display for the campus column.
        /// </summary>
        /// <param name="experience">The interactive experience.</param>
        /// <returns>A string that represents which campuses this experience is for.</returns>
        private static string GetCampusText( InteractiveExperience experience )
        {
            var campusIds = experience.InteractiveExperienceSchedules
                .SelectMany( ies => ies.InteractiveExperienceScheduleCampuses )
                .Select( iesc => iesc.CampusId )
                .Distinct()
                .ToList();

            if ( !campusIds.Any() )
            {
                return "All";
            }

            return campusIds
                .Select( id => CampusCache.Get( id ) )
                .Where( c => c != null )
                .OrderBy( c => c.Name )
                .Select( c => c.Name )
                .JoinStrings( ", " );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Deletes the specified interactive experience and its related answers.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new InteractiveExperienceService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{InteractiveExperience.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete this {InteractiveExperience.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            // Answers reference actions via a non-cascade FK, but actions
            // cascade-delete with the experience. Remove the answers first
            // so the action cascade doesn't violate the FK constraint.
            var interactiveExperienceAnswerService = new InteractiveExperienceAnswerService( RockContext );
            var answers = interactiveExperienceAnswerService.Queryable()
                .Where( a => a.InteractiveExperienceAction.InteractiveExperienceId == entity.Id )
                .ToList();

            RockContext.WrapTransaction( () =>
            {
                interactiveExperienceAnswerService.DeleteRange( answers );
                RockContext.SaveChanges();

                entityService.Delete( entity );
                RockContext.SaveChanges();
            } );

            return ActionOk();
        }

        #endregion Block Actions
    }
}

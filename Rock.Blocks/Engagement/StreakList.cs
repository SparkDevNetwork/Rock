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
using System.Text;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Engagement.StreakList;
using Rock.Web.Cache;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays a list of streaks.
    /// </summary>
    [DisplayName( "Streak List" )]
    [Category( "Streaks" )]
    [Description( "Lists all the people enrolled in a streak type." )]
    [IconCssClass( "fa fa-list" )]
    //[SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the streak details.",
        Key = AttributeKey.DetailPage )]

    [LinkedPage(
        "Person Profile Page",
        Description = "Page used for viewing a person's profile. If set, a view profile button will show for each enrollment.",
        Key = AttributeKey.ProfilePage,
        IsRequired = false,
        Order = 1 )]

    [Rock.SystemGuid.EntityTypeGuid( "b7894ceb-837a-468e-92b1-53a1631c828e" )]
    [Rock.SystemGuid.BlockTypeGuid( "73efc838-d5e3-4dbd-b5af-c3c81d3e7daf" )]
    [CustomizedGrid]
    public class StreakList : RockEntityListBlockType<Streak>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string ProfilePage = "PersonProfilePage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
            public const string ProfilePage = "PersonProfilePage";
        }

        private static class PageParameterKey
        {
            public const string StreakTypeId = "StreakTypeId";
            public const string StreakId = "StreakId";
            public const string PersonId = "PersonId";
        }

        private static class PreferenceKey
        {
            public const string FilterFirstName = "filter-first-name";
            public const string FilterLastName = "filter-last-name";
            public const string FilterEnrollmentDateUpperValue = "filter-enrollment-date-upper-value";
            public const string FilterEnrollmentDateLowerValue = "filter-enrollment-date-lower-value";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<StreakListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
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
        private StreakListOptionsBag GetBoxOptions()
        {
            var streakType = GetStreakType();

            var options = new StreakListOptionsBag()
            {
                StreakTypeName = streakType?.Name,
                StreakTypeIdKey = streakType?.IdKey
            };

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var streakType = GetStreakType();

            return streakType?.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) == true
                || streakType?.IsAuthorized( Authorization.MANAGE_MEMBERS, GetCurrentPerson() ) == true;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var streakType = GetStreakType();
            var queryParams = new Dictionary<string, string>()
            {
                { PageParameterKey.StreakId, "((Key))" }
            };

            if ( streakType != null )
            {
                queryParams.Add( PageParameterKey.StreakTypeId, streakType.IdKey );
            }

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, queryParams ),
                [NavigationUrlKey.ProfilePage] = this.GetLinkedPageUrl( AttributeKey.ProfilePage, new Dictionary<string, string> { { PageParameterKey.PersonId, "((Key))" } } )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<Streak> GetListQueryable( RockContext rockContext )
        {
            var streakType = GetStreakType();

            if ( streakType != null )
            {
                var query = new StreakService( RockContext ).Queryable()
                    .Include( se => se.PersonAlias.Person )
                    .AsNoTracking()
                    .Where( se => se.StreakTypeId == streakType.Id );

                return query;
            }

            return new List<Streak>().AsQueryable();
        }

        /// <inheritdoc/>
        protected override IQueryable<Streak> GetOrderedListQueryable( IQueryable<Streak> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( s => s.PersonAlias.Person.LastName ).ThenBy( s => s.PersonAlias.Person.NickName );
        }

        /// <inheritdoc/>
        protected override GridBuilder<Streak> GetGridBuilder()
        {
            return new GridBuilder<Streak>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddField( "personId", a => a.PersonAlias.PersonId )
                .AddTextField( "lastName", a => a.PersonAlias.Person.LastName )
                .AddTextField( "nickName", a => a.PersonAlias.Person.NickName )
                .AddTextField( "fullName", a => a.PersonAlias.Person.FullName )
                .AddDateTimeField( "enrollmentDate", a => a.EnrollmentDate )
                .AddField( "engagementCount", a => a.EngagementCount )
                .AddField( "currentStreakCount", a => a.CurrentStreakCount )
                .AddField( "longestStreakCount", a => a.LongestStreakCount )
                .AddField( "isDeceased", a => a.PersonAlias.Person.IsDeceased )
                .AddTextField( "signalMarkup", a => a.PersonAlias.Person.GetSignalMarkup() )
                .AddTextField( "engagementMap", a => GetEngagementGraph( a ) )
                .AddPersonField( "person", a => a.PersonAlias?.Person );
        }

        /// <summary>
        /// Generates an engagement graph based on the person's engagement map.
        /// </summary>
        /// <param name="streak">The streak.</param>
        /// <returns></returns>
        private string GetEngagementGraph( Streak streak )
        {
            var streakType = GetStreakType();
            var engagementGraph = string.Empty;

            if ( streakType != null )
            {
                var occurrenceEngagements = new StreakTypeService( RockContext )
                    .GetRecentEngagementBits( streakType.Id, streak.PersonAlias.PersonId, 24, out string errorMessage ) ?? new OccurrenceEngagement[0];
                var stringBuilder = new StringBuilder();
                foreach ( var occurrence in occurrenceEngagements )
                {
                    var hasEngagement = occurrence?.HasEngagement == true;
                    var hasExclusion = occurrence?.HasExclusion == true;
                    var title = occurrence != null ? occurrence.DateTime.ToShortDateString() : string.Empty;
                    stringBuilder.Insert( 0, string.Format( @"<li class=""binary-state-graph-bit {2} {3}"" title=""{0}""><span style=""height: {1}%""></span></li>",
                        title, // 0
                        hasEngagement ? "100" : "5", // 1
                        hasEngagement ? "has-engagement" : string.Empty, // 2
                        hasExclusion ? "has-exclusion" : string.Empty ) ); // 3
                }

                engagementGraph = string.Format( @"
                        <div class=""chart-container"">
                            <ul class=""trend-chart trend-chart-sm text-info"">{0}</ul>
                        </div>", stringBuilder );
            }

            return engagementGraph;
        }

        /// <summary>
        /// Get the streak type
        /// </summary>
        /// <returns></returns>
        private StreakTypeCache GetStreakType()
        {
            var streakTypeId = PageParameter( PageParameterKey.StreakTypeId ).AsIntegerOrNull();

            return streakTypeId.HasValue ? StreakTypeCache.Get( streakTypeId.Value ) : null;
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
            var entityService = new StreakService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{Streak.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {Streak.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion
    }
}

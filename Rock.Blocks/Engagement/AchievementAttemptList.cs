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

using Rock.Achievement;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Engagement.AchievementAttemptList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays a list of achievement attempts.
    /// </summary>

    [DisplayName( "Achievement Attempt List" )]
    [Category( "Achievements" )]
    [Description( "Lists all the people that have made an attempt at earning an achievement." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the achievement attempt details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "039c87ae-0835-4844-ac9b-a66ae1d19530" )]
    [Rock.SystemGuid.BlockTypeGuid( "b294c1b9-8368-422c-8054-9672c7f41477" )]
    [CustomizedGrid]
    public class AchievementAttemptList : RockListBlockType<AchieverAttemptItem>
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
            public const string AchievementTypeId = "AchievementTypeId";
            public const string AchievementAttemptId = "AchievementAttemptId";
        }

        private static class PreferenceKey
        {
            public const string FilterAchieverName = "filter-achiever-name";
            public const string FilterAttemptStartDateRangeFrom = "filter-attempt-start-date-range-from";
            public const string FilterAttemptStartDateRangeTo = "filter-attempt-start-date-range-to";
            public const string FilterStatus = "filter-status";
            public const string FilterAchievementType = "filter-achievement-type";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets the name of the person whose attempts should be included in the result.
        /// </summary>
        /// <value>
        /// The name of the achiever.
        /// </value>
        protected string FilterAchieverName => GetBlockPersonPreferences()
            .GetValue( MakeKeyUniqueToAchievementType( PreferenceKey.FilterAchieverName ) );

        /// <summary>
        /// Gets the start date after which the results should have occurred.
        /// </summary>
        /// <value>
        /// The attempt start date from filter.
        /// </value>
        protected DateTime? FilterAttemptStartDateFrom => GetBlockPersonPreferences()
            .GetValue( MakeKeyUniqueToAchievementType( PreferenceKey.FilterAttemptStartDateRangeFrom ) )
            .AsDateTime();

        /// <summary>
        /// Gets the start date before which the results should have occurred.
        /// </summary>
        /// <value>
        /// The attempt start date from filter.
        /// </value>
        protected DateTime? FilterAttemptStartDateTo => GetBlockPersonPreferences()
            .GetValue( MakeKeyUniqueToAchievementType( PreferenceKey.FilterAttemptStartDateRangeTo ) )
            .AsDateTime();

        /// <summary>
        /// Gets the status with which to filter the results.
        /// </summary>
        /// <value>
        /// The status filter.
        /// </value>
        protected string FilterStatus => GetBlockPersonPreferences()
            .GetValue( MakeKeyUniqueToAchievementType( PreferenceKey.FilterStatus ) );

        /// <summary>
        /// Gets the achievement type with which the results should be filter with.
        /// </summary>
        /// <value>
        /// The achievement type guid.
        /// </value>
        protected Guid? FilterAchievementType => GetBlockPersonPreferences()
            .GetValue( MakeKeyUniqueToAchievementType( PreferenceKey.FilterAchievementType ) )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<AchievementAttemptListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = true;
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
        private AchievementAttemptListOptionsBag GetBoxOptions()
        {
            var achievementTypeCache = GetAchievementTypeCache();
            var options = new AchievementAttemptListOptionsBag()
            {
                AchievementType = achievementTypeCache?.ToListItemBag(),
                CanViewBlock = achievementTypeCache?.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) == true,
            };

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            return GetAttributeValue( AttributeKey.DetailPage ).IsNotNullOrWhiteSpace();
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var achievementType = GetAchievementTypeCache();
            var queryParams = new Dictionary<string, string>()
            {
                { PageParameterKey.AchievementAttemptId, "((Key))" }
            };

            if ( achievementType != null )
            {
                queryParams.Add( PageParameterKey.AchievementTypeId, achievementType.Id.ToString() );
            }

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, queryParams )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<AchieverAttemptItem> GetListQueryable( RockContext rockContext )
        {
            var achievementType = GetAchievementTypeCache();
            var achievementTypes = AchievementTypeCache.All();

            if ( achievementType != null )
            {
                achievementTypes = achievementTypes.Where( at => at.Id == achievementType.Id ).ToList();
            }

            var subQueries = new List<IQueryable<AchieverAttemptItem>>();

            foreach ( var at in achievementTypes )
            {
                var component = at.AchievementComponent;

                if ( component == null )
                {
                    continue;
                }

                var componentQuery = component.GetAchieverAttemptQuery( at, rockContext ).AsNoTracking();
                subQueries.Add( componentQuery );
            }

            var queryable = subQueries.Any()
                ? subQueries.Aggregate( ( a, b ) => a.Union( b ) )
                : new List<AchieverAttemptItem>().AsQueryable();

            // Filter by Achiever Name
            if ( !FilterAchieverName.IsNullOrWhiteSpace() )
            {
                queryable = queryable.Where( aa => aa.AchieverName.StartsWith( FilterAchieverName ) );
            }

            // Filter by start Date
            if ( FilterAttemptStartDateFrom.HasValue )
            {
                queryable = queryable.Where( aa => aa.AchievementAttempt.AchievementAttemptStartDateTime >= FilterAttemptStartDateFrom.Value );
            }

            if ( FilterAttemptStartDateTo.HasValue )
            {
                queryable = queryable.Where( aa => aa.AchievementAttempt.AchievementAttemptStartDateTime <= FilterAttemptStartDateTo.Value );
            }

            // Filter by achievement type
            if ( FilterAchievementType.HasValue )
            {
                queryable = queryable.Where( aa => aa.AchievementAttempt.AchievementType.Guid == FilterAchievementType.Value );
            }

            // Filter by status
            if ( !FilterStatus.IsNullOrWhiteSpace() )
            {
                switch ( FilterStatus )
                {
                    case "Successful":
                        {
                            queryable = queryable.Where( aa => aa.AchievementAttempt.IsSuccessful );
                            break;
                        }

                    case "Unsuccessful":
                        {
                            queryable = queryable.Where( aa => !aa.AchievementAttempt.IsSuccessful && aa.AchievementAttempt.IsClosed );
                            break;
                        }

                    case "Open":
                        {
                            queryable = queryable.Where( aa => !aa.AchievementAttempt.IsClosed );
                            break;
                        }
                }
            }

            return queryable;
        }

        /// <inheritdoc/>
        protected override IQueryable<AchieverAttemptItem> GetOrderedListQueryable( IQueryable<AchieverAttemptItem> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( a => a.AchievementAttempt.IsClosed )
                    .ThenByDescending( a => a.AchievementAttempt.AchievementAttemptStartDateTime )
                    .ThenBy( a => a.AchieverName );
        }

        /// <inheritdoc/>
        protected override GridBuilder<AchieverAttemptItem> GetGridBuilder()
        {
            return new GridBuilder<AchieverAttemptItem>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.AchievementAttempt.IdKey )
                .AddTextField( "achiever", a => a.AchieverName )
                .AddTextField( "achievement", a => a.AchievementAttempt.AchievementType.Name )
                .AddTextField( "personId", a => GetPersonId( a.Achiever ) )
                .AddDateTimeField( "startDate", a => a.AchievementAttempt.AchievementAttemptStartDateTime )
                .AddDateTimeField( "endDate", a => a.AchievementAttempt.AchievementAttemptEndDateTime )
                .AddField( "isSuccessful", a => a.AchievementAttempt.IsSuccessful )
                .AddField( "isClosed", a => a.AchievementAttempt.IsClosed )
                .AddField( "progress", a => Convert.ToInt64( decimal.Round( a.AchievementAttempt.Progress * 100 ) ) );
        }

        /// <summary>
        /// Gets the person identifier if the <see cref="AchieverAttemptItem.Achiever"/> is a person.
        /// </summary>
        /// <param name="entity">The achiever.</param>
        /// <returns></returns>
        private string GetPersonId( IEntity entity )
        {
            if ( entity is PersonAlias personAlias )
            {
                return personAlias.PersonId.ToString();
            }

            return default;
        }

        /// <summary>
        /// Gets the achievement type cache matching the AchievementTypeId from the page parameter.
        /// Returns null if not matching achievement type is found
        /// </summary>
        /// <returns><see cref="AchievementTypeCache"/></returns>
        private AchievementTypeCache GetAchievementTypeCache()
        {
            var achievementTypeId = PageParameter( PageParameterKey.AchievementTypeId ).AsIntegerOrNull();

            if ( achievementTypeId.HasValue )
            {
                return AchievementTypeCache.Get( achievementTypeId.Value );
            }

            return default;
        }

        /// <summary>
        /// Makes the preference key unique to the current Achievement Type.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string MakeKeyUniqueToAchievementType( string key )
        {
            var achievementType = GetAchievementTypeCache();
            return achievementType != null ? $"{achievementType.Guid}-{key}" : key;
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
                var entityService = new AchievementAttemptService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{AchievementAttempt.FriendlyTypeName} not found." );
                }

                if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${AchievementAttempt.FriendlyTypeName}." );
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

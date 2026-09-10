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

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.PersonDetail.PersonGroupHistory;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// Displays a timeline of a person's history in groups.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Person Group History" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Displays a timeline of a person's history in groups." )]
    [IconCssClass( "ti ti-history" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [GroupTypesField(
        "Group Types",
        Key = AttributeKey.GroupTypes,
        Description = "List of Group Types that this block defaults to, and the user is able to choose from in the options filter. Leave blank to include all group types that have history enabled.",
        IsRequired = false,
        Order = 0 )]

    #endregion Block Attributes

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Secondary )]
    [Rock.SystemGuid.EntityTypeGuid( "9E6F6F3F-AAAA-4EB9-9690-4CF0E4F9614F" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "EFEC18D6-FE2E-4C30-94E2-EF1740DB1E65" )]
    [Rock.SystemGuid.BlockTypeGuid( "F8E351BC-607E-4897-B732-F590B5155451" )]
    [ContextAware( typeof( Person ) )]
    public class PersonGroupHistory : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string GroupTypes = "GroupTypes";
        }

        private static class PageParameterKey
        {
            public const string PersonId = "PersonId";
        }

        private static class PersonPreferenceKey
        {
            public const string GroupTypes = "GroupTypes";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<PersonGroupHistoryBag, PersonGroupHistoryOptionsBag>();

            var person = GetContextPerson();

            // Match the legacy behavior of hiding the block when there is no person in context.
            if ( person == null )
            {
                box.Bag = new PersonGroupHistoryBag
                {
                    IsVisible = false
                };

                return box;
            }

            var availableGroupTypes = GetAvailableGroupTypes();

            box.Bag = new PersonGroupHistoryBag
            {
                IsVisible = true,
                SelectedGroupTypes = GetSavedGroupTypeIdPreference()
                    .Select( id => GroupTypeCache.Get( id ) )
                    .Where( groupType => groupType != null )
                    .Select( groupType => new ListItemBag { Value = groupType.Guid.ToString(), Text = groupType.Name } )
                    .ToList()
            };

            box.Options.AvailableGroupTypeGuids = availableGroupTypes.Select( groupType => groupType.Guid ).ToList();
            box.Options.DefaultViewMode = "Year";

            return box;
        }

        /// <summary>
        /// Gets the current person from the page context, falling back to the page parameter.
        /// </summary>
        /// <returns>The resolved <see cref="Person"/>, or <c>null</c> if one could not be determined.</returns>
        private Person GetContextPerson()
        {
            var person = RequestContext.GetContextEntity<Person>();
            if ( person != null )
            {
                return person;
            }

            var personKey = PageParameter( PageParameterKey.PersonId );
            if ( personKey.IsNotNullOrWhiteSpace() )
            {
                return new PersonService( RockContext ).Get( personKey, !PageCache.Layout.Site.DisablePredictableIds );
            }

            return null;
        }

        /// <summary>
        /// Gets the group types the filter picker is allowed to choose from. When the block setting
        /// names specific group types those are used, otherwise all history-enabled group types are returned.
        /// </summary>
        /// <returns>The available group types, ordered by name.</returns>
        private List<GroupTypeCache> GetAvailableGroupTypes()
        {
            var blockSettingGroupTypeIds = GetBlockSettingGroupTypeIds();

            if ( blockSettingGroupTypeIds.Any() )
            {
                return GroupTypeCache.All()
                    .Where( groupType => blockSettingGroupTypeIds.Contains( groupType.Id ) )
                    .OrderBy( groupType => groupType.Name )
                    .ToList();
            }

            return GroupTypeCache.All()
                .Where( groupType => groupType.EnableGroupHistory )
                .OrderBy( groupType => groupType.Name )
                .ToList();
        }

        /// <summary>
        /// Gets the group type ids named in the block's Group Types setting.
        /// </summary>
        /// <returns>The configured group type ids, or an empty list when the setting is blank.</returns>
        private List<int> GetBlockSettingGroupTypeIds()
        {
            return GetAttributeValue( AttributeKey.GroupTypes )
                .SplitDelimitedValues()
                .AsGuidList()
                .Select( guid => GroupTypeCache.Get( guid ) )
                .Where( groupType => groupType != null )
                .Select( groupType => groupType.Id )
                .ToList();
        }

        /// <summary>
        /// Gets the person's saved group type filter preference as a list of group type ids.
        /// </summary>
        /// <returns>The saved group type ids, or an empty list when no preference is stored.</returns>
        private List<int> GetSavedGroupTypeIdPreference()
        {
            return GetBlockPersonPreferences()
                .GetValue( PersonPreferenceKey.GroupTypes )
                .SplitDelimitedValues()
                .AsIntegerList();
        }

        /// <summary>
        /// Builds the legend of history-enabled group types, filtered to the resolved selection and ordered by name.
        /// </summary>
        /// <param name="filterGroupTypeIds">The resolved group type ids the timeline is filtered to, or an empty list for all.</param>
        /// <returns>The legend entries.</returns>
        private List<GroupHistoryLegendItemBag> GetLegend( List<int> filterGroupTypeIds )
        {
            var legendGroupTypes = GroupTypeCache.All().Where( groupType => groupType.EnableGroupHistory );

            if ( filterGroupTypeIds.Any() )
            {
                legendGroupTypes = legendGroupTypes.Where( groupType => filterGroupTypeIds.Contains( groupType.Id ) );
            }

            return legendGroupTypes
                .OrderBy( groupType => groupType.Name )
                .Select( groupType => new GroupHistoryLegendItemBag
                {
                    Value = groupType.Guid.ToString(),
                    Text = groupType.Name,
                    Color = groupType.GroupTypeColor
                } )
                .ToList();
        }

        /// <summary>
        /// Converts a service group historical summary into the lane bag consumed by the timeline.
        /// </summary>
        /// <param name="summary">The group historical summary.</param>
        /// <returns>The lane bag.</returns>
        private static GroupHistoryLaneBag ToLaneBag( GroupMemberHistoricalService.GroupHistoricalSummary summary )
        {
            return new GroupHistoryLaneBag
            {
                GroupIdKey = summary.Group.IdKey,
                GroupTypeId = summary.GroupTypeId,
                GroupTypeColor = summary.GroupTypeColor,
                GroupTypeName = summary.GroupTypeName,
                StartStopHistory = summary.StartStopHistory?
                    .Select( history => new GroupHistoryLaneItemBag
                    {
                        GroupName = history.GroupName,
                        StartDateTime = history.StartDateTime.ToString( "s" ),
                        StopDateTime = history.StopDateTime.ToString( "s" ),
                        IsLeader = history.IsLeader,
                        GroupRoleName = history.GroupRoleName
                    } )
                    .ToList() ?? new List<GroupHistoryLaneItemBag>()
            };
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the group history timeline data for the current person, optionally persisting the selected filter.
        /// </summary>
        /// <param name="request">The selected group types and whether to persist them as a preference.</param>
        /// <returns>The timeline lanes and legend.</returns>
        [BlockAction]
        public BlockActionResult GetGroupHistory( PersonGroupHistoryRequestBag request )
        {
            var person = GetContextPerson();

            if ( person == null )
            {
                return ActionOk( new PersonGroupHistoryDataBag
                {
                    Lanes = new List<GroupHistoryLaneBag>(),
                    Legend = new List<GroupHistoryLegendItemBag>()
                } );
            }

            var selectedGroupTypeIds = ( request?.SelectedGroupTypeGuids ?? new List<Guid>() )
                .Select( guid => GroupTypeCache.Get( guid ) )
                .Where( groupType => groupType != null )
                .Select( groupType => groupType.Id )
                .ToList();

            // Persist the selection as the person's filter preference only when the user applies it.
            if ( request?.IsSavingPreference == true )
            {
                var preferences = GetBlockPersonPreferences();
                preferences.SetValue( PersonPreferenceKey.GroupTypes, selectedGroupTypeIds.AsDelimited( "," ) );
                preferences.Save();
            }

            // The user's selection takes precedence; otherwise fall back to the block setting (which may be empty for "all").
            var filterGroupTypeIds = selectedGroupTypeIds.Any()
                ? selectedGroupTypeIds
                : GetBlockSettingGroupTypeIds();

            // Match the legacy behavior of always showing the last ten years of history.
            var startDateTime = DateTime.SpecifyKind( RockDateTime.Now.AddYears( -10 ), DateTimeKind.Unspecified );
            var stopDateTime = HistoricalTracking.MaxExpireDateTime;

            // The service applies per-record VIEW authorization using the current person, so it must be passed.
            var summaries = new GroupMemberHistoricalService( RockContext )
                .GetGroupHistoricalSummary( person.Id, startDateTime, stopDateTime, filterGroupTypeIds, RequestContext.CurrentPerson );

            return ActionOk( new PersonGroupHistoryDataBag
            {
                Lanes = summaries.Select( ToLaneBag ).ToList(),
                Legend = GetLegend( filterGroupTypeIds )
            } );
        }

        #endregion Block Actions
    }
}

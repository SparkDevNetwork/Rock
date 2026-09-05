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
using System.Globalization;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.SystemKey;
using Rock.Utility.Settings.DataAutomation;
using Rock.ViewModels.Blocks.Administration.DataAutomationSettings;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Administration
{
    /// <summary>
    /// Block used to view and change the shared configuration that controls the
    /// Data Automation service job (reactivating and inactivating people,
    /// updating family campus, moving adult children, and updating connection
    /// and family status).
    /// </summary>
    [DisplayName( "Data Automation Settings" )]
    [Category( "Administration" )]
    [Description( "Block used to set values specific to data automation (updating person status, family campus, etc.)." )]
    [IconCssClass( "ti ti-adjustments-horizontal" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [SystemGuid.EntityTypeGuid( "9BD3B6EF-D0D5-4D27-8BF9-F4826A4807FF" )]
    [SystemGuid.BlockTypeGuid( "D700AB2C-3E35-4EB8-B75F-2FBE00AF9283" )]
    // TODO WILL BECOME [Rock.SystemGuid.BlockTypeGuid( "E34C45E9-97CA-4902-803B-1EFAC9174083" )]
    public class DataAutomationSettings : RockBlockType
    {
        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var knownRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );

            return new DataAutomationSettingsInitializationBox
            {
                Settings = GetSettingsBag(),
                AttendanceGroupTypes = GetAttendanceGroupTypeItems(),
                PersonAttributes = GetPersonAttributeItems(),
                MostAttendanceOrGivingOptions = GetMostAttendanceOrGivingOptions(),
                CampusChangeBasedOnOptions = GetCampusChangeBasedOnOptions(),
                KnownRelationshipGroupTypeGuid = knownRelationshipGroupType?.Guid,

                // The campus update section only makes sense when more than one campus exists.
                IsUpdateFamilyCampusVisible = CampusCache.All().Count > 1
            };
        }

        /// <summary>
        /// Builds the complete settings bag from the current system settings.
        /// </summary>
        /// <returns>A populated <see cref="DataAutomationSettingsBag"/>.</returns>
        private DataAutomationSettingsBag GetSettingsBag()
        {
            var reactivateSettings = Rock.Web.SystemSettings.GetValue( SystemSetting.DATA_AUTOMATION_REACTIVATE_PEOPLE ).FromJsonOrNull<ReactivatePeople>() ?? new ReactivatePeople();
            var inactivateSettings = Rock.Web.SystemSettings.GetValue( SystemSetting.DATA_AUTOMATION_INACTIVATE_PEOPLE ).FromJsonOrNull<InactivatePeople>() ?? new InactivatePeople();
            var campusSettings = Rock.Web.SystemSettings.GetValue( SystemSetting.DATA_AUTOMATION_CAMPUS_UPDATE ).FromJsonOrNull<UpdateFamilyCampus>() ?? new UpdateFamilyCampus();
            var adultChildrenSettings = Rock.Web.SystemSettings.GetValue( SystemSetting.DATA_AUTOMATION_ADULT_CHILDREN ).FromJsonOrNull<MoveAdultChildren>() ?? new MoveAdultChildren();
            var connectionStatusSettings = Rock.Web.SystemSettings.GetValue( SystemSetting.DATA_AUTOMATION_UPDATE_PERSON_CONNECTION_STATUS ).FromJsonOrNull<UpdatePersonConnectionStatus>() ?? new UpdatePersonConnectionStatus();
            var familyStatusSettings = Rock.Web.SystemSettings.GetValue( SystemSetting.DATA_AUTOMATION_UPDATE_FAMILY_STATUS ).FromJsonOrNull<UpdateFamilyStatus>() ?? new UpdateFamilyStatus();

            var channels = GetInteractionChannels();

            return new DataAutomationSettingsBag
            {
                GenderAutoFillConfidence = Rock.Web.SystemSettings.GetValue( SystemSetting.GENDER_AUTO_FILL_CONFIDENCE ).AsDoubleOrNull(),
                ReactivatePeople = GetReactivatePeopleBag( reactivateSettings, channels ),
                InactivatePeople = GetInactivatePeopleBag( inactivateSettings, channels ),
                UpdateFamilyCampus = GetUpdateFamilyCampusBag( campusSettings ),
                MoveAdultChildren = GetMoveAdultChildrenBag( adultChildrenSettings ),
                UpdateConnectionStatus = GetUpdateConnectionStatusBag( connectionStatusSettings ),
                UpdateFamilyStatus = GetUpdateFamilyStatusBag( familyStatusSettings )
            };
        }

        /// <summary>
        /// Maps the reactivate settings into its bag.
        /// </summary>
        /// <param name="settings">The stored reactivate settings.</param>
        /// <param name="channels">The available interaction channels.</param>
        /// <returns>The populated bag.</returns>
        private ReactivatePeopleSettingsBag GetReactivatePeopleBag( ReactivatePeople settings, List<InteractionChannelItem> channels )
        {
            return new ReactivatePeopleSettingsBag
            {
                IsEnabled = settings.IsEnabled,
                IsLastContributionEnabled = settings.IsLastContributionEnabled,
                LastContributionPeriod = settings.LastContributionPeriod,
                IsAttendanceInServiceGroupEnabled = settings.IsAttendanceInServiceGroupEnabled,
                AttendanceInServiceGroupPeriod = settings.AttendanceInServiceGroupPeriod,
                IsRegisteredInAnyEventEnabled = settings.IsRegisteredInAnyEventEnabled,
                RegisteredInAnyEventPeriod = settings.RegisteredInAnyEventPeriod,
                IsAttendanceInGroupTypeEnabled = settings.IsAttendanceInGroupTypeEnabled,
                AttendanceInGroupType = ToGroupTypeGuidStrings( settings.AttendanceInGroupType ),
                AttendanceInGroupTypeDays = settings.AttendanceInGroupTypeDays,
                IsSiteLoginEnabled = settings.IsSiteLoginEnabled,
                SiteLoginPeriod = settings.SiteLoginPeriod,
                IsPrayerRequestEnabled = settings.IsPrayerRequestEnabled,
                PrayerRequestPeriod = settings.PrayerRequestPeriod,
                IsPersonAttributesEnabled = settings.IsPersonAttributesEnabled,
                PersonAttributes = ToAttributeGuidStrings( settings.PersonAttributes ),
                PersonAttributesDays = settings.PersonAttributesDays,
                IsIncludeDataViewEnabled = settings.IsIncludeDataViewEnabled,
                IncludeDataView = ToDataViewListItemBag( settings.IncludeDataView ),
                IsExcludeDataViewEnabled = settings.IsExcludeDataViewEnabled,
                ExcludeDataView = ToDataViewListItemBag( settings.ExcludeDataView ),
                IsInteractionsEnabled = settings.IsInteractionsEnabled,
                Interactions = BuildInteractionItemBags( channels, settings.Interactions )
            };
        }

        /// <summary>
        /// Maps the inactivate settings into its bag.
        /// </summary>
        /// <param name="settings">The stored inactivate settings.</param>
        /// <param name="channels">The available interaction channels.</param>
        /// <returns>The populated bag.</returns>
        private InactivatePeopleSettingsBag GetInactivatePeopleBag( InactivatePeople settings, List<InteractionChannelItem> channels )
        {
            return new InactivatePeopleSettingsBag
            {
                IsEnabled = settings.IsEnabled,
                RecordsOlderThan = settings.RecordsOlderThan,
                IsNoLastContributionEnabled = settings.IsNoLastContributionEnabled,
                NoLastContributionPeriod = settings.NoLastContributionPeriod,
                IsNoAttendanceInGroupTypeEnabled = settings.IsNoAttendanceInGroupTypeEnabled,
                AttendanceInGroupType = ToGroupTypeGuidStrings( settings.AttendanceInGroupType ),
                NoAttendanceInGroupTypeDays = settings.NoAttendanceInGroupTypeDays,
                IsNotRegisteredInAnyEventEnabled = settings.IsNotRegisteredInAnyEventEnabled,
                NotRegisteredInAnyEventDays = settings.NotRegisteredInAnyEventDays,
                IsNoSiteLoginEnabled = settings.IsNoSiteLoginEnabled,
                NoSiteLoginPeriod = settings.NoSiteLoginPeriod,
                IsNoPrayerRequestEnabled = settings.IsNoPrayerRequestEnabled,
                NoPrayerRequestPeriod = settings.NoPrayerRequestPeriod,
                IsNoPersonAttributesEnabled = settings.IsNoPersonAttributesEnabled,
                PersonAttributes = ToAttributeGuidStrings( settings.PersonAttributes ),
                NoPersonAttributesDays = settings.NoPersonAttributesDays,
                IsNotInDataViewEnabled = settings.IsNotInDataviewEnabled,
                NotInDataView = ToDataViewListItemBag( settings.NotInDataview ),
                IsNoInteractionsEnabled = settings.IsNoInteractionsEnabled,
                NoInteractions = BuildInteractionItemBags( channels, settings.NoInteractions )
            };
        }

        /// <summary>
        /// Maps the update family campus settings into its bag.
        /// </summary>
        /// <param name="settings">The stored campus update settings.</param>
        /// <returns>The populated bag.</returns>
        private UpdateFamilyCampusSettingsBag GetUpdateFamilyCampusBag( UpdateFamilyCampus settings )
        {
            var excludeScheduleItems = new List<ListItemBag>();
            if ( settings.ExcludeSchedules != null && settings.ExcludeSchedules.Any() )
            {
                excludeScheduleItems = new ScheduleService( RockContext ).Queryable()
                    .Where( s => settings.ExcludeSchedules.Contains( s.Id ) )
                    .ToList()
                    .Select( s => new ListItemBag { Value = s.Guid.ToString(), Text = s.Name } )
                    .ToList();
            }

            var ignoreCampusChanges = ( settings.IgnoreCampusChanges ?? new List<IgnoreCampusChangeItem>() )
                .Select( c => new IgnoreCampusChangeBag
                {
                    FromCampus = CampusCache.Get( c.FromCampus )?.ToListItemBag(),
                    ToCampus = CampusCache.Get( c.ToCampus )?.ToListItemBag(),
                    BasedOn = c.BasedOn.HasValue ? ( ( int ) c.BasedOn.Value ).ToString() : string.Empty
                } )
                .ToList();

            return new UpdateFamilyCampusSettingsBag
            {
                IsEnabled = settings.IsEnabled,
                IsMostFamilyAttendanceEnabled = settings.IsMostFamilyAttendanceEnabled,
                MostFamilyAttendancePeriod = settings.MostFamilyAttendancePeriod,
                TimesToTriggerCampusChange = settings.TimesToTriggerCampusChange,
                ExcludeSchedules = excludeScheduleItems,
                IsMostFamilyGivingEnabled = settings.IsMostFamilyGivingEnabled,
                MostFamilyGivingPeriod = settings.MostFamilyGivingPeriod,
                MostAttendanceOrGiving = ( ( int ) settings.MostAttendanceOrGiving ).ToString(),
                IsIgnoreIfManualUpdateEnabled = settings.IsIgnoreIfManualUpdateEnabled,
                IgnoreIfManualUpdatePeriod = settings.IgnoreIfManualUpdatePeriod,
                IsIgnoreCampusChangesEnabled = settings.IsIgnoreCampusChangesEnabled,
                IgnoreCampusChanges = ignoreCampusChanges
            };
        }

        /// <summary>
        /// Maps the move adult children settings into its bag.
        /// </summary>
        /// <param name="settings">The stored adult children settings.</param>
        /// <returns>The populated bag.</returns>
        private MoveAdultChildrenSettingsBag GetMoveAdultChildrenBag( MoveAdultChildren settings )
        {
            return new MoveAdultChildrenSettingsBag
            {
                IsEnabled = settings.IsEnabled,
                IsOnlyMoveGraduated = settings.IsOnlyMoveGraduated,
                AdultAge = settings.AdultAge,
                ParentRelationship = settings.ParentRelationshipId.HasValue ? GroupTypeRoleCache.Get( settings.ParentRelationshipId.Value )?.ToListItemBag() : null,
                SiblingRelationship = settings.SiblingRelationshipId.HasValue ? GroupTypeRoleCache.Get( settings.SiblingRelationshipId.Value )?.ToListItemBag() : null,
                UseSameHomeAddress = settings.UseSameHomeAddress,
                UseSameHomePhone = settings.UseSameHomePhone,
                Workflows = ( settings.WorkflowTypeIds ?? new List<int>() )
                    .Select( id => WorkflowTypeCache.Get( id )?.ToListItemBag() )
                    .Where( item => item != null )
                    .ToList(),
                MaximumRecords = settings.MaximumRecords
            };
        }

        /// <summary>
        /// Maps the update connection status settings into its bag.
        /// </summary>
        /// <param name="settings">The stored connection status settings.</param>
        /// <returns>The populated bag.</returns>
        private UpdateConnectionStatusSettingsBag GetUpdateConnectionStatusBag( UpdatePersonConnectionStatus settings )
        {
            var connectionStatuses = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() )?.DefinedValues ?? new List<DefinedValueCache>();

            return new UpdateConnectionStatusSettingsBag
            {
                IsEnabled = settings.IsEnabled,
                StatusDataViews = connectionStatuses
                    .Select( dv => new StatusDataViewMappingBag
                    {
                        Status = dv.ToListItemBag(),
                        DataView = ToDataViewListItemBag( settings.ConnectionStatusValueIdDataviewIdMapping.GetValueOrNull( dv.Id ) )
                    } )
                    .ToList()
            };
        }

        /// <summary>
        /// Maps the update family status settings into its bag.
        /// </summary>
        /// <param name="settings">The stored family status settings.</param>
        /// <returns>The populated bag.</returns>
        private UpdateFamilyStatusSettingsBag GetUpdateFamilyStatusBag( UpdateFamilyStatus settings )
        {
            var familyStatuses = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FAMILY_GROUP_STATUS.AsGuid() )?.DefinedValues ?? new List<DefinedValueCache>();

            return new UpdateFamilyStatusSettingsBag
            {
                IsEnabled = settings.IsEnabled,
                StatusDataViews = familyStatuses
                    .Select( dv => new StatusDataViewMappingBag
                    {
                        Status = dv.ToListItemBag(),
                        DataView = ToDataViewListItemBag( settings.GroupStatusValueIdDataviewIdMapping.GetValueOrNull( dv.Id ) )
                    } )
                    .ToList()
            };
        }

        /// <summary>
        /// Builds the per-channel interaction rows, applying the saved selections
        /// on top of the full channel list. When nothing was previously saved,
        /// every channel defaults to enabled at 90 days.
        /// </summary>
        /// <param name="channels">The available interaction channels.</param>
        /// <param name="savedItems">The previously saved interaction selections.</param>
        /// <returns>The interaction rows to display.</returns>
        private List<DataAutomationInteractionItemBag> BuildInteractionItemBags( List<InteractionChannelItem> channels, List<InteractionItem> savedItems )
        {
            var bags = channels
                .Select( c => new DataAutomationInteractionItemBag
                {
                    Guid = c.Guid,
                    Name = c.Name,
                    IsInteractionTypeEnabled = true,
                    LastInteractionDays = 90
                } )
                .ToList();

            if ( savedItems == null )
            {
                return bags;
            }

            // When none of the saved channels were enabled we treat all of them
            // as enabled so the criteria still evaluates against every channel.
            var isNoneSelected = !savedItems.Any( i => i.IsInteractionTypeEnabled );

            foreach ( var saved in savedItems )
            {
                var bag = bags.FirstOrDefault( a => a.Guid == saved.Guid );
                if ( bag != null )
                {
                    bag.IsInteractionTypeEnabled = isNoneSelected || saved.IsInteractionTypeEnabled;
                    bag.LastInteractionDays = saved.LastInteractionDays;
                }
            }

            // Any channel that was not part of the saved selection is unchecked.
            foreach ( var bag in bags.Where( b => savedItems.All( s => s.Guid != b.Guid ) ) )
            {
                bag.IsInteractionTypeEnabled = false;
            }

            return bags;
        }

        /// <summary>
        /// Gets the interaction channels available for the reactivate and
        /// inactivate criteria.
        /// </summary>
        /// <returns>The list of interaction channels.</returns>
        private List<InteractionChannelItem> GetInteractionChannels()
        {
            return new InteractionChannelService( RockContext ).Queryable().AsNoTracking()
                .Select( a => new { a.Guid, a.Name } )
                .ToList()
                .Select( a => new InteractionChannelItem { Guid = a.Guid, Name = a.Name } )
                .ToList();
        }

        /// <summary>
        /// Gets the group types that take attendance as selectable items, ordered
        /// by order and name.
        /// </summary>
        /// <returns>A list of group type items.</returns>
        private List<ListItemBag> GetAttendanceGroupTypeItems()
        {
            return GroupTypeCache.All()
                .Where( t => t.TakesAttendance )
                .OrderBy( t => t.Order )
                .ThenBy( t => t.Name )
                .Select( t => new ListItemBag { Value = t.Guid.ToString(), Text = t.Name } )
                .ToList();
        }

        /// <summary>
        /// Gets the active person attributes as selectable items.
        /// </summary>
        /// <returns>A list of person attribute items.</returns>
        private List<ListItemBag> GetPersonAttributeItems()
        {
            var personEntityTypeId = EntityTypeCache.Get( typeof( Person ) ).Id;

            return new AttributeService( RockContext )
                .GetByEntityTypeId( personEntityTypeId, false )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .Select( a => new { a.Guid, a.Name } )
                .ToList()
                .Select( a => new ListItemBag { Value = a.Guid.ToString(), Text = a.Name } )
                .ToList();
        }

        /// <summary>
        /// Gets the options for the attendance-versus-giving tie-breaker.
        /// </summary>
        /// <returns>A list of campus criteria options.</returns>
        private List<ListItemBag> GetMostAttendanceOrGivingOptions()
        {
            return Enum.GetValues( typeof( CampusCriteria ) )
                .Cast<CampusCriteria>()
                .Select( c => new ListItemBag { Value = ( ( int ) c ).ToString(), Text = c.ConvertToString() } )
                .ToList();
        }

        /// <summary>
        /// Gets the options for the "based on" criteria of an ignore campus
        /// change rule. An empty value represents "either".
        /// </summary>
        /// <returns>A list of criteria options.</returns>
        private List<ListItemBag> GetCampusChangeBasedOnOptions()
        {
            return new List<ListItemBag>
            {
                new ListItemBag { Value = string.Empty, Text = "Either" },
                new ListItemBag { Value = ( ( int ) CampusCriteria.UseGiving ).ToString(), Text = "Giving" },
                new ListItemBag { Value = ( ( int ) CampusCriteria.UseAttendance ).ToString(), Text = "Attendance" }
            };
        }

        #endregion Methods

        #region Conversion Helpers

        /// <summary>
        /// Converts a list of group type ids into their unique identifier strings.
        /// </summary>
        /// <param name="ids">The group type ids.</param>
        /// <returns>The corresponding unique identifier strings.</returns>
        private List<string> ToGroupTypeGuidStrings( List<int> ids )
        {
            return ( ids ?? new List<int>() )
                .Select( id => GroupTypeCache.Get( id )?.Guid )
                .Where( g => g.HasValue )
                .Select( g => g.Value.ToString() )
                .ToList();
        }

        /// <summary>
        /// Converts a list of group type unique identifier strings into their ids.
        /// </summary>
        /// <param name="guids">The group type unique identifier strings.</param>
        /// <returns>The corresponding ids.</returns>
        private List<int> ToGroupTypeIds( List<string> guids )
        {
            return ( guids ?? new List<string>() )
                .Select( g => g.AsGuidOrNull() )
                .Where( g => g.HasValue )
                .Select( g => GroupTypeCache.Get( g.Value )?.Id )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();
        }

        /// <summary>
        /// Converts a list of attribute ids into their unique identifier strings.
        /// </summary>
        /// <param name="ids">The attribute ids.</param>
        /// <returns>The corresponding unique identifier strings.</returns>
        private List<string> ToAttributeGuidStrings( List<int> ids )
        {
            return ( ids ?? new List<int>() )
                .Select( id => AttributeCache.Get( id )?.Guid )
                .Where( g => g.HasValue )
                .Select( g => g.Value.ToString() )
                .ToList();
        }

        /// <summary>
        /// Converts a list of attribute unique identifier strings into their ids.
        /// </summary>
        /// <param name="guids">The attribute unique identifier strings.</param>
        /// <returns>The corresponding ids.</returns>
        private List<int> ToAttributeIds( List<string> guids )
        {
            return ( guids ?? new List<string>() )
                .Select( g => g.AsGuidOrNull() )
                .Where( g => g.HasValue )
                .Select( g => AttributeCache.Get( g.Value )?.Id )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();
        }

        /// <summary>
        /// Converts a data view id into a list item bag.
        /// </summary>
        /// <param name="dataViewId">The data view id.</param>
        /// <returns>A list item bag, or <c>null</c> when the id has no value.</returns>
        private ListItemBag ToDataViewListItemBag( int? dataViewId )
        {
            return dataViewId.HasValue ? DataViewCache.Get( dataViewId.Value )?.ToListItemBag() : null;
        }

        /// <summary>
        /// Resolves the data view id from a list item bag.
        /// </summary>
        /// <param name="bag">The list item bag containing a data view unique identifier.</param>
        /// <returns>The data view id, or <c>null</c> when it cannot be resolved.</returns>
        private int? ResolveDataViewId( ListItemBag bag )
        {
            var guid = bag?.Value.AsGuidOrNull();
            return guid.HasValue ? DataViewCache.Get( guid.Value )?.Id : null;
        }

        #endregion Conversion Helpers

        #region Block Actions

        /// <summary>
        /// Saves all of the data automation settings.
        /// </summary>
        /// <param name="bag">The settings to save.</param>
        /// <returns>An empty OK result when the save succeeds.</returns>
        [BlockAction]
        public BlockActionResult SaveSettings( DataAutomationSettingsBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest( "Settings are required." );
            }

            Rock.Web.SystemSettings.SetValue( SystemSetting.GENDER_AUTO_FILL_CONFIDENCE, bag.GenderAutoFillConfidence?.ToString( CultureInfo.InvariantCulture ) ?? string.Empty );
            Rock.Web.SystemSettings.SetValue( SystemSetting.DATA_AUTOMATION_REACTIVATE_PEOPLE, BuildReactivateSettings( bag.ReactivatePeople ).ToJson() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.DATA_AUTOMATION_INACTIVATE_PEOPLE, BuildInactivateSettings( bag.InactivatePeople ).ToJson() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.DATA_AUTOMATION_CAMPUS_UPDATE, BuildCampusSettings( bag.UpdateFamilyCampus ).ToJson() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.DATA_AUTOMATION_ADULT_CHILDREN, BuildAdultChildrenSettings( bag.MoveAdultChildren ).ToJson() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.DATA_AUTOMATION_UPDATE_PERSON_CONNECTION_STATUS, BuildConnectionStatusSettings( bag.UpdateConnectionStatus ).ToJson() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.DATA_AUTOMATION_UPDATE_FAMILY_STATUS, BuildFamilyStatusSettings( bag.UpdateFamilyStatus ).ToJson() );

            return ActionOk();
        }

        #endregion Block Actions

        #region Save Helpers

        /// <summary>
        /// Builds the reactivate settings POCO from its bag.
        /// </summary>
        /// <param name="bag">The reactivate settings bag.</param>
        /// <returns>The settings POCO.</returns>
        private ReactivatePeople BuildReactivateSettings( ReactivatePeopleSettingsBag bag )
        {
            bag = bag ?? new ReactivatePeopleSettingsBag();

            return new ReactivatePeople
            {
                IsEnabled = bag.IsEnabled,
                IsLastContributionEnabled = bag.IsLastContributionEnabled,
                LastContributionPeriod = bag.LastContributionPeriod ?? 0,
                IsAttendanceInServiceGroupEnabled = bag.IsAttendanceInServiceGroupEnabled,
                AttendanceInServiceGroupPeriod = bag.AttendanceInServiceGroupPeriod ?? 0,
                IsRegisteredInAnyEventEnabled = bag.IsRegisteredInAnyEventEnabled,
                RegisteredInAnyEventPeriod = bag.RegisteredInAnyEventPeriod ?? 0,
                IsAttendanceInGroupTypeEnabled = bag.IsAttendanceInGroupTypeEnabled,
                AttendanceInGroupType = ToGroupTypeIds( bag.AttendanceInGroupType ),
                AttendanceInGroupTypeDays = bag.AttendanceInGroupTypeDays ?? 0,
                IsSiteLoginEnabled = bag.IsSiteLoginEnabled,
                SiteLoginPeriod = bag.SiteLoginPeriod ?? 0,
                IsPrayerRequestEnabled = bag.IsPrayerRequestEnabled,
                PrayerRequestPeriod = bag.PrayerRequestPeriod ?? 0,
                IsPersonAttributesEnabled = bag.IsPersonAttributesEnabled,
                PersonAttributes = ToAttributeIds( bag.PersonAttributes ),
                PersonAttributesDays = bag.PersonAttributesDays ?? 0,
                IsIncludeDataViewEnabled = bag.IsIncludeDataViewEnabled,
                IncludeDataView = ResolveDataViewId( bag.IncludeDataView ),
                IsExcludeDataViewEnabled = bag.IsExcludeDataViewEnabled,
                ExcludeDataView = ResolveDataViewId( bag.ExcludeDataView ),
                IsInteractionsEnabled = bag.IsInteractionsEnabled,
                Interactions = BuildInteractionItems( bag.Interactions )
            };
        }

        /// <summary>
        /// Builds the inactivate settings POCO from its bag.
        /// </summary>
        /// <param name="bag">The inactivate settings bag.</param>
        /// <returns>The settings POCO.</returns>
        private InactivatePeople BuildInactivateSettings( InactivatePeopleSettingsBag bag )
        {
            bag = bag ?? new InactivatePeopleSettingsBag();

            return new InactivatePeople
            {
                IsEnabled = bag.IsEnabled,
                RecordsOlderThan = bag.RecordsOlderThan ?? 0,
                IsNoLastContributionEnabled = bag.IsNoLastContributionEnabled,
                NoLastContributionPeriod = bag.NoLastContributionPeriod ?? 0,
                IsNoAttendanceInGroupTypeEnabled = bag.IsNoAttendanceInGroupTypeEnabled,
                AttendanceInGroupType = ToGroupTypeIds( bag.AttendanceInGroupType ),
                NoAttendanceInGroupTypeDays = bag.NoAttendanceInGroupTypeDays ?? 0,
                IsNotRegisteredInAnyEventEnabled = bag.IsNotRegisteredInAnyEventEnabled,
                NotRegisteredInAnyEventDays = bag.NotRegisteredInAnyEventDays ?? 0,
                IsNoSiteLoginEnabled = bag.IsNoSiteLoginEnabled,
                NoSiteLoginPeriod = bag.NoSiteLoginPeriod ?? 0,
                IsNoPrayerRequestEnabled = bag.IsNoPrayerRequestEnabled,
                NoPrayerRequestPeriod = bag.NoPrayerRequestPeriod ?? 0,
                IsNoPersonAttributesEnabled = bag.IsNoPersonAttributesEnabled,
                PersonAttributes = ToAttributeIds( bag.PersonAttributes ),
                NoPersonAttributesDays = bag.NoPersonAttributesDays ?? 0,
                IsNotInDataviewEnabled = bag.IsNotInDataViewEnabled,
                NotInDataview = ResolveDataViewId( bag.NotInDataView ),
                IsNoInteractionsEnabled = bag.IsNoInteractionsEnabled,
                NoInteractions = BuildInteractionItems( bag.NoInteractions )
            };
        }

        /// <summary>
        /// Builds the update family campus settings POCO from its bag.
        /// </summary>
        /// <param name="bag">The campus update settings bag.</param>
        /// <returns>The settings POCO.</returns>
        private UpdateFamilyCampus BuildCampusSettings( UpdateFamilyCampusSettingsBag bag )
        {
            bag = bag ?? new UpdateFamilyCampusSettingsBag();

            var scheduleGuids = ( bag.ExcludeSchedules ?? new List<ListItemBag>() )
                .Select( s => s.Value.AsGuidOrNull() )
                .Where( g => g.HasValue )
                .Select( g => g.Value )
                .ToList();

            var excludeScheduleIds = scheduleGuids.Any()
                ? new ScheduleService( RockContext ).Queryable().Where( s => scheduleGuids.Contains( s.Guid ) ).Select( s => s.Id ).ToList()
                : new List<int>();

            var ignoreCampusChanges = ( bag.IgnoreCampusChanges ?? new List<IgnoreCampusChangeBag>() )
                .Select( c => new
                {
                    FromCampusId = CampusCache.Get( c.FromCampus?.Value.AsGuid() ?? Guid.Empty )?.Id,
                    ToCampusId = CampusCache.Get( c.ToCampus?.Value.AsGuid() ?? Guid.Empty )?.Id,
                    BasedOn = ResolveCampusCriteria( c.BasedOn )
                } )
                .Where( c => c.FromCampusId.HasValue && c.ToCampusId.HasValue )
                .Select( c => new IgnoreCampusChangeItem
                {
                    FromCampus = c.FromCampusId.Value,
                    ToCampus = c.ToCampusId.Value,
                    BasedOn = c.BasedOn
                } )
                .ToList();

            return new UpdateFamilyCampus
            {
                IsEnabled = bag.IsEnabled,
                IsMostFamilyAttendanceEnabled = bag.IsMostFamilyAttendanceEnabled,
                MostFamilyAttendancePeriod = bag.MostFamilyAttendancePeriod ?? 0,
                TimesToTriggerCampusChange = bag.TimesToTriggerCampusChange ?? 0,
                ExcludeSchedules = excludeScheduleIds,
                IsMostFamilyGivingEnabled = bag.IsMostFamilyGivingEnabled,
                MostFamilyGivingPeriod = bag.MostFamilyGivingPeriod ?? 0,
                MostAttendanceOrGiving = ( CampusCriteria ) ( bag.MostAttendanceOrGiving.AsIntegerOrNull() ?? 0 ),
                IsIgnoreIfManualUpdateEnabled = bag.IsIgnoreIfManualUpdateEnabled,
                IgnoreIfManualUpdatePeriod = bag.IgnoreIfManualUpdatePeriod ?? 0,
                IsIgnoreCampusChangesEnabled = bag.IsIgnoreCampusChangesEnabled,
                IgnoreCampusChanges = ignoreCampusChanges
            };
        }

        /// <summary>
        /// Builds the move adult children settings POCO from its bag.
        /// </summary>
        /// <param name="bag">The adult children settings bag.</param>
        /// <returns>The settings POCO.</returns>
        private MoveAdultChildren BuildAdultChildrenSettings( MoveAdultChildrenSettingsBag bag )
        {
            bag = bag ?? new MoveAdultChildrenSettingsBag();

            var parentRoleGuid = bag.ParentRelationship?.Value.AsGuidOrNull();
            var siblingRoleGuid = bag.SiblingRelationship?.Value.AsGuidOrNull();

            return new MoveAdultChildren
            {
                IsEnabled = bag.IsEnabled,
                IsOnlyMoveGraduated = bag.IsOnlyMoveGraduated,
                AdultAge = bag.AdultAge ?? 18,
                ParentRelationshipId = parentRoleGuid.HasValue ? GroupTypeRoleCache.Get( parentRoleGuid.Value )?.Id : null,
                SiblingRelationshipId = siblingRoleGuid.HasValue ? GroupTypeRoleCache.Get( siblingRoleGuid.Value )?.Id : null,
                UseSameHomeAddress = bag.UseSameHomeAddress,
                UseSameHomePhone = bag.UseSameHomePhone,
                WorkflowTypeIds = ( bag.Workflows ?? new List<ListItemBag>() )
                    .Select( w => w.Value.AsGuidOrNull() )
                    .Where( g => g.HasValue )
                    .Select( g => WorkflowTypeCache.Get( g.Value )?.Id )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToList(),
                MaximumRecords = bag.MaximumRecords ?? 200
            };
        }

        /// <summary>
        /// Builds the update connection status settings POCO from its bag.
        /// </summary>
        /// <param name="bag">The connection status settings bag.</param>
        /// <returns>The settings POCO.</returns>
        private UpdatePersonConnectionStatus BuildConnectionStatusSettings( UpdateConnectionStatusSettingsBag bag )
        {
            bag = bag ?? new UpdateConnectionStatusSettingsBag();

            var settings = new UpdatePersonConnectionStatus { IsEnabled = bag.IsEnabled };

            foreach ( var row in bag.StatusDataViews ?? new List<StatusDataViewMappingBag>() )
            {
                var statusValueId = DefinedValueCache.Get( row.Status?.Value.AsGuid() ?? Guid.Empty )?.Id;
                if ( statusValueId.HasValue )
                {
                    settings.ConnectionStatusValueIdDataviewIdMapping.AddOrReplace( statusValueId.Value, ResolveDataViewId( row.DataView ) );
                }
            }

            return settings;
        }

        /// <summary>
        /// Builds the update family status settings POCO from its bag.
        /// </summary>
        /// <param name="bag">The family status settings bag.</param>
        /// <returns>The settings POCO.</returns>
        private UpdateFamilyStatus BuildFamilyStatusSettings( UpdateFamilyStatusSettingsBag bag )
        {
            bag = bag ?? new UpdateFamilyStatusSettingsBag();

            var settings = new UpdateFamilyStatus { IsEnabled = bag.IsEnabled };

            foreach ( var row in bag.StatusDataViews ?? new List<StatusDataViewMappingBag>() )
            {
                var statusValueId = DefinedValueCache.Get( row.Status?.Value.AsGuid() ?? Guid.Empty )?.Id;
                if ( statusValueId.HasValue )
                {
                    settings.GroupStatusValueIdDataviewIdMapping.AddOrReplace( statusValueId.Value, ResolveDataViewId( row.DataView ) );
                }
            }

            return settings;
        }

        /// <summary>
        /// Builds the interaction items POCO list from its bags.
        /// </summary>
        /// <param name="bags">The interaction item bags.</param>
        /// <returns>The interaction items.</returns>
        private List<InteractionItem> BuildInteractionItems( List<DataAutomationInteractionItemBag> bags )
        {
            return ( bags ?? new List<DataAutomationInteractionItemBag>() )
                .Select( i => new InteractionItem( i.Guid, string.Empty )
                {
                    IsInteractionTypeEnabled = i.IsInteractionTypeEnabled,
                    LastInteractionDays = i.LastInteractionDays ?? 0
                } )
                .ToList();
        }

        /// <summary>
        /// Resolves the campus criteria from its string value. An empty value
        /// maps to <c>null</c> ("either").
        /// </summary>
        /// <param name="value">The string value of the criteria.</param>
        /// <returns>The campus criteria, or <c>null</c>.</returns>
        private CampusCriteria? ResolveCampusCriteria( string value )
        {
            var criteria = value.AsIntegerOrNull();
            return criteria.HasValue ? ( CampusCriteria? ) criteria.Value : null;
        }

        #endregion Save Helpers

        #region Support Classes

        /// <summary>
        /// A lightweight interaction channel identifier and name pair.
        /// </summary>
        private class InteractionChannelItem
        {
            /// <summary>
            /// Gets or sets the interaction channel unique identifier.
            /// </summary>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the interaction channel name.
            /// </summary>
            public string Name { get; set; }
        }

        #endregion Support Classes
    }
}

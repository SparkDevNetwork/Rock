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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Controls;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.PersonalDeviceInteractions;
using Rock.ViewModels.Controls;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm
{
    [DisplayName( "Personal Device Interactions" )]
    [Category( "CRM" )]
    [Description( "Shows a list of all interactions for a personal device." )]

    [IntegerField(
        "Presence Interval (Minutes)",
        Key = AttributeKey.CurrentlyPresentInterval,
        Description = "The number of minutes to use when determining if a device is still present. For example, if set to 5, a device is considered present if its last interaction occurred within the past 5 minutes.",
        IsRequired = true,
        DefaultIntegerValue = 5,
        Order = 0 )]

    // was [Rock.SystemGuid.BlockTypeGuid( "B734D303-E116-497D-9A03-E641DCF193C3" )]
    [Rock.SystemGuid.BlockTypeGuid( "D6224911-2590-427F-9DCE-6D14E79806BA" )]
    public class PersonalDeviceInteractions : RockListBlockType<PersonalDeviceInteractions.PersonalDeviceInteractionRow>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string CurrentlyPresentInterval = "CurrentlyPresentInterval";
        }

        private static class PageParameterKey
        {
            public const string PersonalDeviceId = "PersonalDeviceId";
        }

        private static class PreferenceKey
        {
            public const string FilterDateRange = "filter-date-range";
            public const string FilterShowUnassignedDevices = "filter-show-unassigned-devices";
            public const string FilterPresentDevices = "filter-present-devices";
        }

        #endregion Keys

        #region Properties

        private PersonPreferenceCollection BlockPersonPreferences => this.GetBlockPersonPreferences();

        private SlidingDateRangeBag FilterDateRange => BlockPersonPreferences
            .GetValue( PreferenceKey.FilterDateRange )
            .ToSlidingDateRangeBagOrNull();

        private bool FilterShowUnassignedDevices => BlockPersonPreferences
            .GetValue( PreferenceKey.FilterShowUnassignedDevices )
            .AsBoolean();

        private bool FilterPresentDevices => BlockPersonPreferences
            .GetValue( PreferenceKey.FilterPresentDevices )
            .AsBoolean();

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<PersonalDeviceInteractionsOptionsBag>();

            var builder = GetGridBuilder();

            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box; 
        }

        /// <summary>
        /// Gets the box options required for the component.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private PersonalDeviceInteractionsOptionsBag GetBoxOptions()
        {
            var options = new PersonalDeviceInteractionsOptionsBag();

            var title = "Personal Device Interactions";
            var personalDevice = GetPersonalDevice();
            if ( personalDevice?.PersonAlias?.Person != null )
            {
                title = $"{personalDevice.PersonAlias.Person.FullName.ToPossessive()} Device Interactions";
            }
            options.BlockTitle = title;

            // We use this to conditionally render certain columns.
            options.IsFilteredByDevice = personalDevice != null;

            if ( personalDevice != null )
            {
                options.Platform = personalDevice.Platform?.Value;
                options.Version = personalDevice.DeviceVersion;

                if ( personalDevice.PersonalDeviceTypeValueId.HasValue )
                {
                    var deviceTypeValueId = personalDevice.PersonalDeviceTypeValueId.Value;
                    var dv = DefinedValueCache.Get( deviceTypeValueId );
                    options.IconCssClass = dv?.GetAttributeValue( "IconCssClass" );
                }
            }

            return options;
        }

        /// <inheritdoc/>
        protected override IQueryable<PersonalDeviceInteractionRow> GetListQueryable( RockContext rockContext )
        {
            rockContext.SqlLogging( true );
            var interactionService = new InteractionService( rockContext );
            var baseQuery = interactionService.Queryable()
                .AsNoTracking()
				.Where( i => i.PersonalDeviceId != null )
				.Include( i => i.PersonalDevice.PersonalDeviceType )
				.Include( i => i.PersonalDevice.Platform )
				.Include( i => i.PersonAlias.Person );

            var personalDevice = GetPersonalDevice();

            if ( personalDevice != null )
            {
                var deviceId = personalDevice.Id;
                baseQuery = baseQuery.Where( i => i.PersonalDeviceId == deviceId );
            }

            var queryable = baseQuery.Select( i => new PersonalDeviceInteractionRow
            {
                Id = i.Id.ToString(),
                PersonalDeviceId = i.PersonalDeviceId.Value.ToString(),
                InteractionDateTime = i.InteractionDateTime,
                InteractionEndDateTime = i.InteractionEndDateTime,
                Details = i.InteractionSummary,
                AssignedIndividual = i.PersonAlias != null ? i.PersonAlias.Person : null,
                DeviceTypeValueId = i.PersonalDevice.PersonalDeviceTypeValueId,
                Platform = i.PersonalDevice.Platform != null
                    ? i.PersonalDevice.Platform.Value
                    : null,
                Version = i.PersonalDevice.DeviceVersion
            } );

            queryable = FilterByDateRange( queryable );

            if ( FilterShowUnassignedDevices )
            {
                queryable = queryable.Where( r => r.AssignedIndividual == null );
            }

            if ( FilterPresentDevices )
            {
                var startDateTime = RockDateTime.Now.AddMinutes( -GetAttributeValue( AttributeKey.CurrentlyPresentInterval ).AsInteger() );
                queryable = queryable.Where( r => r.InteractionEndDateTime.HasValue && r.InteractionEndDateTime.Value >= startDateTime );
            }

            rockContext.SqlLogging( false );

            return queryable;
        }

        /// <inheritdoc/>
        protected override GridBuilder<PersonalDeviceInteractionRow> GetGridBuilder()
        {
            return new GridBuilder<PersonalDeviceInteractionRow>()
                .WithBlock( this )
                .AddTextField( "id", r => r.Id )
                .AddDateTimeField( "dateTime", r => r.InteractionDateTime.Date )
                .AddPersonField( "assignedIndividual", r => r.AssignedIndividual )
                .AddTextField( "details", r => r.Details )
                .AddTextField( "deviceType", r =>
                {
                    if ( r.DeviceTypeValueId.HasValue )
                    {
                        var dv = DefinedValueCache.Get( r.DeviceTypeValueId.Value );
                        return dv?.Value ?? string.Empty;
                    }

                    return string.Empty;
                } )
                .AddTextField( "deviceTypeIconCssClass", r =>
                {
                    if ( r.DeviceTypeValueId.HasValue )
                    {
                        var dv = DefinedValueCache.Get( r.DeviceTypeValueId.Value );
                        return dv?.GetAttributeValue( "IconCssClass" ) ?? string.Empty;
                    }

                    return string.Empty;
                } )
                .AddTextField( "platform", r => r.Platform )
                .AddTextField( "version", r => r.Version );
        }

        #endregion Methods

        #region Helper Methods

        /// <summary>
        /// Gets the personal device from the page parameters if exists.
        /// </summary>
        private PersonalDevice GetPersonalDevice()
        {
            var key = PageParameter( PageParameterKey.PersonalDeviceId );

            if ( key.IsNullOrWhiteSpace() )
            {
                return null;
            }

			var personalDeviceService = new PersonalDeviceService( RockContext );
			var personalDevice = personalDeviceService
				.GetQueryableByKey( key )
				.AsNoTracking()
				.Include( pd => pd.Platform )
				.Include( pd => pd.PersonAlias.Person )
				.FirstOrDefault();

			return personalDevice;
        }

        /// <summary>
        /// Filters the queryable by the selected date range.
        /// </summary>
        /// <param name="queryable">The <see cref="PersonalDeviceInteractionRow"/> queryable</param>
        /// <returns></returns>
        private IQueryable<PersonalDeviceInteractionRow> FilterByDateRange( IQueryable<PersonalDeviceInteractionRow> queryable )
        {
            // Default to the last 6 months if a null/invalid range was selected.
            var defaultSlidingDateRange = new SlidingDateRangeBag
            {
                RangeType = SlidingDateRangeType.Last,
                TimeUnit = TimeUnitType.Month,
                TimeValue = 6
            };

            var dateRange = FilterDateRange.Validate( defaultSlidingDateRange ).ActualDateRange;
            var dateTimeStart = dateRange.Start;
            var dateTimeEnd = dateRange.End;

            queryable = queryable
                .Where( i =>
                    i.InteractionDateTime >= dateTimeStart &&
                    i.InteractionDateTime <= dateTimeEnd );

            return queryable;
        }

        #endregion Helper Methods

        #region Helper Classes

        /// <summary>
        /// A lightweight class containing only the data needed for the grid to function.
        /// </summary>
        public class PersonalDeviceInteractionRow
        {
            public string Id { get; set; }

            public string PersonalDeviceId { get; set; }

            public DateTime InteractionDateTime { get; set; }

            public DateTime? InteractionEndDateTime { get; set; }

            public string Details { get; set; }

            public Person AssignedIndividual { get; set; }

            public int? DeviceTypeValueId { get; set; }

            public string Platform { get; set; }
                
            public string Version { get; set; }
        }

        #endregion Helper Classes
    }
}

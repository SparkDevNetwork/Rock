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

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Content;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Types.Mobile.Events
{
    /// <summary>
    /// Block that takes an audience and displays calendar item occurrences for it using Lava.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    [DisplayName( "Event Item Occurrence List By Audience Lava" )]
    [Category( "Mobile > Events" )]
    [Description( "Block that takes an audience and displays calendar item occurrences for it using Lava." )]
    [IconCssClass( "fa fa-list-alt" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [TextField( "List Title",
        Description = "The title to make available in the lava.",
        IsRequired = false,
        DefaultValue = "Upcoming Events",
        Key = AttributeKeys.ListTitle,
        Order = 0 )]

    [DefinedValueField( name: "Audience", definedTypeGuid: Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE,
        Description = "The audience to show calendar items for.",
        IsRequired = true,
        Key = AttributeKeys.Audience,
        Order = 1 )]

    [EventCalendarField( "Calendar",
        Description = "Filters the events by a specific calendar.",
        IsRequired = false,
        Key = AttributeKeys.Calendar,
        Order = 2 )]

    [CampusesField( "Campuses", includeInactive: true,
        Description = "List of which campuses to show occurrences for. This setting will be ignored if 'Use Campus Context' is enabled.",
        IsRequired = false,
        Key = AttributeKeys.Campuses,
        Order = 3 )]

    [BooleanField( "Use Campus Context",
        Description = "Determine if the campus should be read from the campus context of the page.",
        DefaultBooleanValue = false,
        Key = AttributeKeys.UseCampusContext,
        Order = 4 )]

    [SlidingDateRangeField( "Date Range",
        Description = "Optional date range to filter the occurrences on.",
        IsRequired = false,
        EnabledSlidingDateRangeTypes = "Next,Upcoming,Current",
        Key = AttributeKeys.DateRange,
        Order = 5 )]

    [IntegerField( "Max Occurrences",
        Description = "The maximum number of occurrences to show.",
        IsRequired = false,
        DefaultIntegerValue = 5,
        Key = AttributeKeys.MaxOccurrences,
        Order = 6 )]

    [LinkedPage( "Event Detail Page",
        Description = "The page to use for showing event details.",
        IsRequired = false,
        Key = AttributeKeys.EventDetailPage,
        Order = 7 )]

    [BlockTemplateField( "Lava Template",
        Description = "The template to use when rendering event items.",
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_EVENT_ITEM_OCCURRENCE_LIST_BY_AUDIENCE,
        IsRequired = true,
        DefaultValue = "",
        Key = AttributeKeys.LavaTemplate,
        Order = 8 )]

    [LavaCommandsField( "Enabled Lava Commands",
        Description = "The Lava commands that should be enabled for this block, only affects Lava rendered on the server.",
        IsRequired = false,
        Key = AttributeKeys.EnabledLavaCommands,
        Order = 9 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_EVENTS_EVENTITEMOCCURRENCELISTBYAUDIENCELAVA_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( "FC2879AC-5967-43E7-8759-6888BF21CE21")]
    public class EventItemOccurrenceListByAudienceLava : RockBlockType
    {
        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the EventItemOccurrenceListByAudienceLava block.
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The list title
            /// </summary>
            public const string ListTitle = "ListTitle";

            /// <summary>
            /// The audience
            /// </summary>
            public const string Audience = "Audience";

            /// <summary>
            /// The calendar
            /// </summary>
            public const string Calendar = "Calendar";

            /// <summary>
            /// The campuses
            /// </summary>
            public const string Campuses = "Campuses";

            /// <summary>
            /// The use campus context
            /// </summary>
            public const string UseCampusContext = "UseCampusContext";

            /// <summary>
            /// The date range
            /// </summary>
            public const string DateRange = "DateRange";

            /// <summary>
            /// The maximum occurrences
            /// </summary>
            public const string MaxOccurrences = "MaxOccurrences";

            /// <summary>
            /// The detail page
            /// </summary>
            public const string EventDetailPage = "DetailPage";

            /// <summary>
            /// The lava template
            /// </summary>
            public const string LavaTemplate = "LavaTemplate";

            /// <summary>
            /// The enabled lava commands
            /// </summary>
            public const string EnabledLavaCommands = "EnabledLavaCommands";
        }

        /// <summary>
        /// Gets the list title.
        /// </summary>
        /// <value>
        /// The list title.
        /// </value>
        protected string ListTitle => GetAttributeValue( AttributeKeys.ListTitle );

        /// <summary>
        /// Gets the audience to limit results to.
        /// </summary>
        /// <value>
        /// The audience to limit results to.
        /// </value>
        protected Guid? Audience => GetAttributeValue( AttributeKeys.Audience ).AsGuidOrNull();

        /// <summary>
        /// Gets the calendar Guid to be displayed.
        /// </summary>
        /// <value>
        /// The calendar Guid to be displayed.
        /// </value>
        protected Guid? Calendar => GetAttributeValue( AttributeKeys.Calendar ).AsGuidOrNull();

        /// <summary>
        /// Gets the campuses that should be used for filtering.
        /// </summary>
        /// <value>
        /// The campuses that should be used for filtering.
        /// </value>
        protected IEnumerable<Guid> Campuses => GetAttributeValue( AttributeKeys.Campuses ).SplitDelimitedValues().AsGuidList();

        /// <summary>
        /// Gets a value indicating whether campus context should be used instead of <see cref="Campuses"/>.
        /// </summary>
        /// <value>
        ///   <c>true</c> if campus context should be used instead of <see cref="Campuses"/>; otherwise, <c>false</c>.
        /// </value>
        protected bool UseCampusContext => GetAttributeValue( AttributeKeys.UseCampusContext ).AsBoolean();

        /// <summary>
        /// Gets the date range.
        /// </summary>
        /// <value>
        /// The date range.
        /// </value>
        protected DateRange DateRange => SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( GetAttributeValue( AttributeKeys.DateRange ) );

        /// <summary>
        /// Gets the maximum occurrences.
        /// </summary>
        /// <value>
        /// The maximum occurrences.
        /// </value>
        protected int MaxOccurrences => GetAttributeValue( AttributeKeys.MaxOccurrences ).AsInteger();

        /// <summary>
        /// Gets the event detail page.
        /// </summary>
        /// <value>
        /// The event detail page.
        /// </value>
        protected Guid? EventDetailPage => GetAttributeValue( AttributeKeys.EventDetailPage ).AsGuidOrNull();

        /// <summary>
        /// Gets the lava template.
        /// </summary>
        /// <value>
        /// The lava template.
        /// </value>
        protected string LavaTemplate => Rock.Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKeys.LavaTemplate ) );

        /// <summary>
        /// Gets the enabled lava commands.
        /// </summary>
        /// <value>
        /// The enabled lava commands.
        /// </value>
        protected string EnabledLavaCommands => GetAttributeValue( AttributeKeys.EnabledLavaCommands );

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 1 );

        /// <inheritdoc/>
        public override Guid? MobileBlockTypeGuid => new Guid( "7258A210-E936-4260-B573-9FA1193AD9E2" ); // Content block.

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            var additionalSettings = BlockCache?.AdditionalSettings.FromJsonOrNull<AdditionalBlockSettings>() ?? new AdditionalBlockSettings();

            return new Rock.Common.Mobile.Blocks.Content.Configuration
            {
                ProcessLava = additionalSettings.ProcessLavaOnClient,
                CacheDuration = additionalSettings.CacheDuration,
                DynamicContent = true
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the filtered campuses.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<CampusCache> GetFilteredCampuses()
        {
            if ( RequestContext.GetPageParameter( "CampusGuid" ).IsNotNullOrWhiteSpace() )
            {
                var campusCache = CampusCache.Get( RequestContext.GetPageParameter( "CampusGuid" ).AsGuid() );

                return campusCache != null ? new CampusCache[] { campusCache } : new CampusCache[0];
            }
            else if ( GetAttributeValue( "UseCampusContext" ).AsBoolean() )
            {
                var contextCampus = RequestContext.GetContextEntity<Campus>();

                return contextCampus != null ? new[] { CampusCache.Get( contextCampus.Id ) } : new CampusCache[0];
            }
            else if ( Campuses.Any() )
            {
                return Campuses.Select( a => CampusCache.Get( a ) )
                    .Where( a => a != null )
                    .ToList();
            }
            else
            {
                return new CampusCache[0];
            }
        }

        /// <summary>
        /// Gets the occurrences that should be displayed.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        /// An enumerable of <see cref="EventItemOccurrence" /> items to be displayed.
        /// </returns>
        private IEnumerable<EventItemOccurrence> GetOccurrences( RockContext rockContext )
        {
            if ( !Audience.HasValue )
            {
                return null;
            }

            // get event occurrences
            var qry = new EventItemOccurrenceService( rockContext ).Queryable()
                .Where( e => e.EventItem.EventItemAudiences.Any( a => a.DefinedValue.Guid == Audience.Value ) && e.EventItem.IsActive );

            // filter by campus
            var campusIds = GetFilteredCampuses().Select( a => a.Id ).ToList();
            if ( campusIds.Any() )
            {
                // If an EventItemOccurrence's CampusId is null, then the occurrence is an 'All Campuses' event occurrence, so include those
                qry = qry.Where( e => !e.CampusId.HasValue || campusIds.Contains( e.CampusId.Value ) );
            }

            // filter by calendar
            if ( Calendar.HasValue )
            {
                qry = qry.Where( e => e.EventItem.EventCalendarItems.Any( c => c.EventCalendar.Guid == Calendar ) );
            }

            // retrieve occurrences
            var itemOccurrences = qry.ToList();

            // filter by date range
            var dateRange = DateRange;
            if ( dateRange.Start != null && dateRange.End != null )
            {
                itemOccurrences.RemoveAll( o => o.GetStartTimes( dateRange.Start.Value, dateRange.End.Value ).Count() == 0 );
            }
            else
            {
                // default show all future
                itemOccurrences.RemoveAll( o => o.GetStartTimes( RockDateTime.Now, RockDateTime.Now.AddDays( 365 ) ).Count() == 0 );
            }

            // limit results
            int maxItems = GetAttributeValue( "MaxOccurrences" ).AsInteger();
            itemOccurrences = itemOccurrences.OrderBy( i => i.NextStartDateTime ).Take( maxItems ).ToList();

            return itemOccurrences;
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the initial content for this block.
        /// </summary>
        /// <returns>The initial content.</returns>
        [BlockAction]
        public object GetInitialContent()
        {
            var rockContext = new RockContext();

            var mergeFields = RequestContext.GetCommonMergeFields();

            mergeFields.Add( "CurrentPage", PageCache );
            mergeFields.Add( "ListTitle", ListTitle );
            mergeFields.Add( "EventDetailPage", EventDetailPage );
            mergeFields.Add( "EventItemOccurrences", GetOccurrences( rockContext ) );
            mergeFields.Add( "FilteredCampuses", GetFilteredCampuses() );

            if ( Audience.HasValue )
            {
                mergeFields.Add( "Audience", DefinedValueCache.Get( Audience.Value ) );
            }

            if ( Calendar.HasValue )
            {
                mergeFields.Add( "Calendar", new EventCalendarService( rockContext ).Get( Calendar.Value ) );
            }

            return new CallbackResponse
            {
                Content = LavaTemplate.ResolveMergeFields( mergeFields, null, EnabledLavaCommands )
            };
        }

        #endregion
    }
}

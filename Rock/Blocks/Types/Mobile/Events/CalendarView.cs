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
using System.Net.Http;
using System.Text;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Content;
using Rock.Data;
using Rock.Lava;
using Rock.Mobile;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Events
{
    /// <summary>
    /// Displays a calendar of events.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Calendar View" )]
    [Category( "Mobile > Events" )]
    [Description( "Views events from a calendar." )]
    [IconCssClass( "fa fa-calendar-alt" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [EventCalendarField( "Calendar",
        Description = "The calendar to pull events from",
        IsRequired = true,
        Key = AttributeKeys.Calendar,
        Order = 0 )]

    [LinkedPage( "Detail Page",
        Description = "The page to push onto the navigation stack when viewing details of an event.",
        IsRequired = false,
        Key = AttributeKeys.DetailPage,
        Order = 1 )]

    [DefinedValueField( "Audience Filter",
        Description = "Determines which audiences should be displayed in the filter.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE,
        IsRequired = false,
        AllowMultiple = true,
        Key = AttributeKeys.AudienceFilter,
        Order = 2 )]

    [CodeEditorField( "Event Summary",
        Description = "The XAML to use when rendering the event summaries below the calendar.",
        IsRequired = true,
        DefaultValue = AttributeDefaults.EventSummary,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Xml,
        Key = AttributeKeys.EventSummary,
        Order = 3 )]

    [BooleanField( "Show Filter",
        Description = "If enabled then the user will be able to apply custom filtering.",
        IsRequired = false,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.ShowFilter,
        Order = 4 )]

    [BooleanField( "Show All Events in Detail",
        Description = "Determines if all events for the month should be listed in the detail section or only the selected days events.",
        IsRequired = false,
        DefaultBooleanValue = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.ShowAllEventsInDetail,
        Order = 5 )]

    [BooleanField( "Show Per Audience Event Indicators",
        Description = "Determines if multiple colored dots will be used on the calendar to indicate which audience types exist on that day.",
        IsRequired = false,
        DefaultBooleanValue = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.ShowPerAudienceEventIndicators,
        Order = 6 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_EVENTS_CALENDARVIEW_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( "14B447B3-6117-4142-92E7-E3F289106140")]
    public class CalendarView : RockBlockType
    {
        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the CalendarView block.
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The calendar
            /// </summary>
            public const string Calendar = "Calendar";

            /// <summary>
            /// The detail page
            /// </summary>
            public const string DetailPage = "DetailPage";

            /// <summary>
            /// The audience filter
            /// </summary>
            public const string AudienceFilter = "AudienceFilter";

            /// <summary>
            /// The event summary
            /// </summary>
            public const string EventSummary = "EventSummary";

            /// <summary>
            /// Whether the filter should be shown or not.
            /// </summary>
            public const string ShowFilter = "ShowFilter";

            /// <summary>
            /// Determines if all events for the month should be listed in the detail section or only the selected days events.
            /// </summary>
            public const string ShowAllEventsInDetail = "ShowAllEventsInDetail";

            /// <summary>
            /// Determines if multiple colored dots will be used on the calendar to indicate which audience types exist on that day.
            /// </summary>
            public const string ShowPerAudienceEventIndicators = "ShowPerAudienceEventIndicators";
        }

        /// <summary>
        /// The block attribute default values for the CalendarView block.
        /// </summary>
        public static class AttributeDefaults
        {
            /// <summary>
            /// The event summary default value
            /// </summary>
            public const string EventSummary = @"<Frame HasShadow=""false"" StyleClass=""calendar-event-summary"">
    <StackLayout Spacing=""0"">
        <Label StyleClass=""calendar-event-title"" Text=""{Binding Name}"" />
        {% if Item.EndDateTime == null %}
            <Label StyleClass=""calendar-event-text"" Text=""{{ Item.StartDateTime | Date:'h:mm tt' }}"" LineBreakMode=""NoWrap"" />
        {% else %}
            <Label StyleClass=""calendar-event-text"" Text=""{{ Item.StartDateTime | Date:'h:mm tt' }} - {{ Item.EndDateTime | Date:'h:mm tt' }}"" LineBreakMode=""NoWrap"" />
        {% endif %}
        <StackLayout Orientation=""Horizontal"">
            <Label HorizontalOptions=""FillAndExpand"" StyleClass=""calendar-event-audience"" Text=""{{ Item.Audiences | Select:'Name' | Join:', ' }}"" />
            <Label StyleClass=""calendar-event-campus"" Text=""{{ Item.Campus }}"" HorizontalTextAlignment=""End"" LineBreakMode=""NoWrap"" />
        </StackLayout>
    </StackLayout>
</Frame>
";
        }

        /// <summary>
        /// Gets the calendar Guid to be displayed.
        /// </summary>
        /// <value>
        /// The calendar Guid to be displayed.
        /// </value>
        protected Guid? Calendar => GetAttributeValue( AttributeKeys.Calendar ).AsGuidOrNull();

        /// <summary>
        /// Gets the detail page.
        /// </summary>
        /// <value>
        /// The detail page.
        /// </value>
        protected Guid? DetailPage => GetAttributeValue( AttributeKeys.DetailPage ).AsGuidOrNull();

        /// <summary>
        /// Gets the audience filter.
        /// </summary>
        /// <value>
        /// The audience filter.
        /// </value>
        protected IEnumerable<Guid> AudienceFilter => GetAttributeValue( AttributeKeys.AudienceFilter ).SplitDelimitedValues().AsGuidList();

        /// <summary>
        /// Gets the event summary.
        /// </summary>
        /// <value>
        /// The event summary.
        /// </value>
        protected string EventSummary => GetAttributeValue( AttributeKeys.EventSummary );

        /// <summary>
        /// Gets a value indicating whether the filter should be available to the user.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the filter should be available to the user; otherwise, <c>false</c>.
        /// </value>
        protected bool ShowFilter => GetAttributeValue( AttributeKeys.ShowFilter ).AsBoolean();

        /// <summary>
        /// Gets a value to determine which events should be shown in the detail view.
        /// </summary>
        /// <value>
        ///   <c>true</c> if all events should be shown in the detail view; otherwise, <c>false</c>.
        /// </value>
        protected bool ShowAllEventsInDetail => GetAttributeValue( AttributeKeys.ShowAllEventsInDetail ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether unique audience indicators will be shown on each day.
        /// </summary>
        /// <value>
        ///   <c>true</c> if unique audience indicators will be shown on each day; otherwise, <c>false</c>.
        /// </value>
        protected bool ShowPerAudienceEventIndicators => GetAttributeValue( AttributeKeys.ShowPerAudienceEventIndicators ).AsBoolean();

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 1 );

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            //
            // Indicate that we are a dynamic content providing block.
            //
            return new
            {
                Audiences = GetAudiences().Select( a => new
                {
                    a.Guid,
                    Name = a.Value,
                    Color = a.GetAttributeValue( "HighlightColor")
                } ),
                SummaryContent = EventSummary,
                DetailPage,
                ShowFilter,
                ShowAllEventsInDetail,
                ShowPerAudienceEventIndicators,
                IncludeCampusFilter = true /* Tell shell we support campus filtering */
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the audiences.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<DefinedValueCache> GetAudiences()
        {
            var filterAudiences = AudienceFilter;
            var audiences = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE )
                .DefinedValues
                .Where( a => a.IsActive );

            if ( !filterAudiences.Any() )
            {
                return audiences;
            }
            else
            {
                return audiences.Where( a => filterAudiences.Contains( a.Guid ) );
            }
        }

        /// <summary>
        /// Creates the lava template from the list of fields.
        /// </summary>
        /// <returns></returns>
        private string CreateLavaTemplate()
        {
            var properties = new Dictionary<string, string>
            {
                { "Id", "Id" },
                { "Guid", "Guid" },
                { "Name", "Name" },
                { "StartDateTime", "DateTime" },
                { "EndDateTime", "EndDateTime" },
                { "Campus", "Campus" },
                { "CampusGuid", "CampusGuid" },
                { "Audiences", "Audiences" }
            };

            return MobileHelper.CreateItemLavaTemplate( properties, null );
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the list of events in the indicated date range.
        /// </summary>
        /// <param name="beginDate">The inclusive begin date.</param>
        /// <param name="endDate">The exclusive end date.</param>
        /// <returns></returns>
        [BlockAction]
        public object GetEvents( DateTime beginDate, DateTime endDate )
        {
            using ( var rockContext = new RockContext() )
            {
                var eventCalendar = new EventCalendarService( rockContext ).Get( Calendar ?? Guid.Empty );
                var eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );

                if ( eventCalendar == null )
                {
                    return new List<object>();
                }

                // Grab events
                var qry = eventItemOccurrenceService
                        .Queryable( "EventItem, EventItem.EventItemAudiences, Schedule" )
                        .Where( m =>
                            m.EventItem.EventCalendarItems.Any( i => i.EventCalendarId == eventCalendar.Id ) &&
                            m.EventItem.IsActive &&
                            m.EventItem.IsApproved );

                // Get the occurrences
                var occurrences = qry.ToList()
                    .SelectMany( a =>
                    {
                        var duration = a.Schedule?.DurationInMinutes ?? 0;

                        return a.GetStartTimes( beginDate, endDate )
                            .Where( b => b >= beginDate && b < endDate )
                            .Select( b => new
                            {
                                Date = b.ToRockDateTimeOffset(),
                                Duration = duration,
                                AudienceGuids = a.EventItem.EventItemAudiences.Select( c => DefinedValueCache.Get( c.DefinedValueId )?.Guid ).Where( c => c.HasValue ).Select( c => c.Value ).ToList(),
                                EventItemOccurrence = a
                            } );
                    } )
                    .Select( a => new
                    {
                        a.EventItemOccurrence,
                        a.EventItemOccurrence.Guid,
                        a.EventItemOccurrence.Id,
                        a.EventItemOccurrence.EventItem.Name,
                        DateTime = a.Date,
                        EndDateTime = a.Duration > 0 ? ( DateTimeOffset? ) a.Date.AddMinutes( a.Duration ) : null,
                        Date = a.Date.ToString( "d" ), // Short date
                        Time = a.Date.ToString( "t" ), // Short time
                        CampusGuid = a.EventItemOccurrence.Campus?.Guid,
                        Campus = a.EventItemOccurrence.Campus != null ? a.EventItemOccurrence.Campus.Name : "All Campuses",
                        Location = a.EventItemOccurrence.Campus != null ? a.EventItemOccurrence.Campus.Name : "All Campuses",
                        LocationDescription = a.EventItemOccurrence.Location,
                        Audiences = a.AudienceGuids,
                        a.EventItemOccurrence.EventItem.Description,
                        a.EventItemOccurrence.EventItem.Summary,
                        OccurrenceNote = a.EventItemOccurrence.Note.SanitizeHtml()
                    } );

                var lavaTemplate = CreateLavaTemplate();

                var commonMergeFields = new CommonMergeFieldsOptions();
                var mergeFields = RequestContext.GetCommonMergeFields( null, commonMergeFields );
                mergeFields.Add( "Items", occurrences.ToList() );

                var output = lavaTemplate.ResolveMergeFields( mergeFields );

                return ActionOk( new StringContent( output, Encoding.UTF8, "application/json" ) );
            }
        }

        #endregion
    }
}

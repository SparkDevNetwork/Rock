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
using Rock.Data;
using Rock.Lava;
using Rock.Mobile;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Events
{
    /// <summary>
    /// Displays a list of events from a calendar.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockMobileBlockType" />

    [DisplayName( "Calendar Event List" )]
    [Category( "Mobile > Events" )]
    [Description( "Displays a list of events from a calendar." )]
    [IconCssClass( "fa fa-list-alt" )]

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

    [BlockTemplateField( "Event Template",
        Description = "The template to use when rendering event items.",
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CALENDAR_EVENT_LIST,
        IsRequired = true,
        DefaultValue = "",
        Key = AttributeKeys.EventTemplate,
        Order = 2 )]

    [CodeEditorField( "Day Header Template",
        Description = "The XAML to use when rendering the day header above a grouping of events.",
        IsRequired = true,
        DefaultValue = AttributeDefaults.DayHeaderTemplate,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Xml,
        Key = AttributeKeys.DayHeaderTemplate,
        Order = 3 )]

    #endregion

    public class CalendarEventList : RockMobileBlockType
    {
        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the CalendarEventList block.
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
            /// The event template
            /// </summary>
            public const string EventTemplate = "EventTemplate";

            /// <summary>
            /// The day header template
            /// </summary>
            public const string DayHeaderTemplate = "DayHeaderTemplate";
        }

        /// <summary>
        /// The block attribute default values for the CalendarEventList block.
        /// </summary>
        public static class AttributeDefaults
        {
            /// <summary>
            /// The event template default value.
            /// </summary>
            public const string EventTemplate = @"<Frame HasShadow=""false"" StyleClass=""calendar-event-summary"">
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

            /// <summary>
            /// The day header template default value.
            /// </summary>
            public const string DayHeaderTemplate = @"<Label Text=""{Binding ., StringFormat='{}{0:dddd MMMM d}'}"" StyleClass=""calendar-events-day, heading2"" />";
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
        /// Gets the event template.
        /// </summary>
        /// <value>
        /// The event template.
        /// </value>
        protected string EventTemplate => Rock.Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKeys.EventTemplate ) );

        /// <summary>
        /// Gets the day header template.
        /// </summary>
        /// <value>
        /// The day header template.
        /// </value>
        protected string DayHeaderTemplate => GetAttributeValue( AttributeKeys.DayHeaderTemplate );

        #endregion

        #region IRockMobileBlockType Implementation

        /// <summary>
        /// Gets the required mobile application binary interface version required to render this block.
        /// </summary>
        /// <value>
        /// The required mobile application binary interface version required to render this block.
        /// </value>
        public override int RequiredMobileAbiVersion => 1;

        /// <summary>
        /// Gets the class name of the mobile block to use during rendering on the device.
        /// </summary>
        /// <value>
        /// The class name of the mobile block to use during rendering on the device
        /// </value>
        public override string MobileBlockType => "Rock.Mobile.Blocks.Events.CalendarEventList";

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            return new
            {
                Audiences = GetAudiences().Select( a => new
                {
                    a.Id,
                    Name = a.Value,
                    Color = a.GetAttributeValue( "HighlightColor" )
                } ),
                EventTemplate,
                DayHeaderTemplate,
                DetailPage
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
            var audiences = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE )
                .DefinedValues
                .Where( a => a.IsActive );

            return audiences;
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
                { "Name", "Name" },
                { "StartDateTime", "DateTime" },
                { "EndDateTime", "EndDateTime" },
                { "Campus", "Campus" },
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

                // Filter by audiences
                var audiences = GetAudiences().Select( a => a.Id ).ToList();
                if ( audiences.Any() )
                {
                    qry = qry.Where( i => i.EventItem.EventItemAudiences.Any( c => audiences.Contains( c.DefinedValueId ) ) );
                }

                // Get the occurrences
                var occurrences = qry.ToList()
                    .SelectMany( a =>
                    {
                        var duration = a.Schedule?.DurationInMinutes ?? 0;

                        return a.GetStartTimes( beginDate, endDate )
                            .Where( b => b >= beginDate && b < endDate )
                            .Select( b => new
                            {
                                Date = b,
                                Duration = duration,
                                AudienceIds = a.EventItem.EventItemAudiences.Select( c => c.DefinedValueId ).ToList(),
                                EventItemOccurrence = a
                            } );
                    } )
                    .Select( a => new
                    {
                        a.EventItemOccurrence,
                        a.EventItemOccurrence.EventItem.Id,
                        a.EventItemOccurrence.EventItem.Name,
                        DateTime = a.Date,
                        EndDateTime = a.Duration > 0 ? ( DateTime? ) a.Date.AddMinutes( a.Duration ) : null,
                        Date = a.Date.ToShortDateString(),
                        Time = a.Date.ToShortTimeString(),
                        Campus = a.EventItemOccurrence.Campus != null ? a.EventItemOccurrence.Campus.Name : "All Campuses",
                        Location = a.EventItemOccurrence.Campus != null ? a.EventItemOccurrence.Campus.Name : "All Campuses",
                        LocationDescription = a.EventItemOccurrence.Location,
                        Audiences = a.AudienceIds,
                        a.EventItemOccurrence.EventItem.Description,
                        a.EventItemOccurrence.EventItem.Summary,
                        OccurrenceNote = a.EventItemOccurrence.Note.SanitizeHtml()
                    } );

                var lavaTemplate = CreateLavaTemplate();

                var commonMergeFields = new CommonMergeFieldsOptions
                {
                    GetLegacyGlobalMergeFields = false
                };

                var mergeFields = RequestContext.GetCommonMergeFields( null, commonMergeFields );
                mergeFields.Add( "Items", occurrences.ToList() );

                var output = lavaTemplate.ResolveMergeFields( mergeFields );

                return ActionOk( new StringContent( output, Encoding.UTF8, "application/json" ) );
            }
        }

        #endregion
    }
}

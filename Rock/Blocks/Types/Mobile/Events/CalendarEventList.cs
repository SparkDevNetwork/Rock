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
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Calendar Event List" )]
    [Category( "Mobile > Events" )]
    [Description( "Displays a list of events from a calendar." )]
    [IconCssClass( "fa fa-list-alt" )]
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

    [BooleanField( "Enable Campus Filtering",
        Description = "If enabled then events will be filtered by campus to the campus context of the page and user.",
        IsRequired = false,
        DefaultBooleanValue = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.EnableCampusFiltering,
        Order = 4 )]

    [BooleanField( "Show Past Events",
        Description = "When enabled past events will be included on the calendar, otherwise only future events will be shown.",
        IsRequired = false,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.ShowPastEvents,
        Order = 5 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_EVENTS_CALENDAREVENTLIST_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( "A9149623-6A82-4F25-8F4D-0961557BE78C")]
    public class CalendarEventList : RockBlockType
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

            /// <summary>
            /// The enable campus filtering
            /// </summary>
            public const string EnableCampusFiltering = "EnableCampusFiltering";

            /// <summary>
            /// When enabled past events will be included on the calendar, otherwise only future events will be shown.
            /// </summary>
            public const string ShowPastEvents = "ShowPastEvents";
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
            public const string DayHeaderTemplate = @"<Label Text=""{Binding ., StringFormat='{}{0:dddd MMMM d}'}"" StyleClass=""calendar-events-day, h2"" />";
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

        /// <summary>
        /// Gets a value indicating whether campus filtering is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if campus filtering is enabled; otherwise, <c>false</c>.
        /// </value>
        protected bool EnableCampusFiltering => GetAttributeValue( AttributeKeys.EnableCampusFiltering ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether past events will be included on the calendar.
        /// </summary>
        /// <value>
        ///   <c>true</c> if past events will be included on the calendar, otherwise <c>false</c>.
        /// </value>
        protected bool ShowPastEvents => GetAttributeValue( AttributeKeys.ShowPastEvents ).AsBoolean();

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
            return new
            {
                Audiences = GetAudiences().Select( a => new
                {
                    a.Guid,
                    Name = a.Value,
                    Color = a.GetAttributeValue( "HighlightColor" )
                } ),
                EventTemplate,
                DayHeaderTemplate,
                DetailPage,
                HidePastEvents = !ShowPastEvents
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
                { "Guid", "Guid" },
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

                // Check for Campus Parameter or Campus Context.
                if ( EnableCampusFiltering )
                {
                    var campusGuid = RequestContext.GetPageParameter( "CampusGuid" ).AsGuidOrNull();
                    if ( campusGuid.HasValue )
                    {
                        // Check if there's a campus with this guid.
                        var campus = CampusCache.Get( campusGuid.Value );
                        if ( campus != null )
                        {
                            qry = qry.Where( a => !a.CampusId.HasValue || a.CampusId == campus.Id );
                        }
                    }
                    else
                    {
                        var contextCampus = RequestContext.GetContextEntity<Campus>();
                        if ( contextCampus != null )
                        {
                            qry = qry.Where( a => !a.CampusId.HasValue || a.Campus.Id == contextCampus.Id );
                        }
                        else if ( RequestContext.CurrentPerson != null && RequestContext.CurrentPerson.PrimaryCampusId.HasValue )
                        {
                            var campusId = RequestContext.CurrentPerson.PrimaryCampusId.Value;

                            qry = qry.Where( a => !a.CampusId.HasValue || a.CampusId == campusId );
                        }
                    }
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

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
using System.Linq;

using Rock.Model;
using Rock.UniversalSearch.IndexModels.Attributes;
using Rock.Utility;

namespace Rock.UniversalSearch.IndexModels
{
    /// <summary>
    /// Group Index
    /// </summary>
    /// <seealso cref="Rock.UniversalSearch.IndexModels.IndexModelBase" />
    public class EventItemIndex : IndexModelBase
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [RockIndexField( Analyzer = "snowball", Boost = 3 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the summary.
        /// </summary>
        /// <value>
        /// The summary.
        /// </value>
        [RockIndexField( Analyzer = "snowball" )]
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [RockIndexField( Analyzer = "snowball" )]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the photo URL.
        /// </summary>
        /// <value>
        /// The photo URL.
        /// </value>
        [RockIndexField( Index = IndexType.NotIndexed )]
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the details URL.
        /// </summary>
        /// <value>
        /// The details URL.
        /// </value>
        [RockIndexField( Index = IndexType.NotIndexed )]
        public string DetailsUrl { get; set; }

        /// <summary>
        /// Gets or sets the next start date time.
        /// </summary>
        /// <value>
        /// The next start date time.
        /// </value>
        [RockIndexField( Type = IndexFieldType.Date )]
        public DateTime? NextStartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the event item audiences.
        /// </summary>
        /// <value>
        /// The event item audiences.
        /// </value>
        [RockIndexField( Index = IndexType.NotIndexed )]
        public List<EventItemAudience> EventItemAudiences { get; set; } = new List<EventItemAudience>();

        /// <summary>
        /// Gets or sets the event calendars.
        /// </summary>
        /// <value>
        /// The event calendars.
        /// </value>
        [RockIndexField( Index = IndexType.NotIndexed )]
        public List<EventCalendar> EventCalendars { get; set; } = new List<EventCalendar>();

        /// <summary>
        /// Gets or sets the event item occurrences.
        /// </summary>
        /// <value>
        /// The event item occurrences.
        /// </value>
        [RockIndexField( Index = IndexType.NotIndexed )]
        public List<EventItemOccurrence> EventItemOccurrences { get; set; } = new List<EventItemOccurrence>();

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [RockIndexField( Type = IndexFieldType.String, Index = IndexType.NotIndexed )]
        public override string IconCssClass
        {
            get
            {
                return iconCssClass;
            }
            set
            {
                iconCssClass = value;
            }
        }
        private string iconCssClass = "fa fa-calendar-alt";

        /// <summary>
        /// Loads the by model.
        /// </summary>
        /// <param name="eventItem">The event item.</param>
        /// <returns></returns>
        public static EventItemIndex LoadByModel( EventItem eventItem )
        {
            var eventItemIndex = new EventItemIndex();
            eventItemIndex.SourceIndexModel = "Rock.Model.EventItem";

            eventItemIndex.Id = eventItem.Id;
            eventItemIndex.Name = eventItem.Name;
            eventItemIndex.Description = eventItem.Description;
            eventItemIndex.DocumentName = eventItem.Name;
            eventItemIndex.Summary = eventItem.Summary;
            eventItemIndex.PhotoUrl = eventItem?.Photo?.Url;
            eventItemIndex.DetailsUrl = eventItem.DetailsUrl;
            eventItemIndex.NextStartDateTime = eventItem.NextStartDateTime;

            // Add audiences
            foreach( var audience in eventItem.EventItemAudiences )
            {
                eventItemIndex.EventItemAudiences.Add(
                    new EventItemAudience {
                        DefinedValueId = audience.DefinedValueId, 
                        AudienceName = audience.DefinedValue.Value
                    } );
            }

            // Add event item occurrences
            foreach( var occurrence in eventItem.EventItemOccurrences )
            {
                if ( occurrence.NextStartDateTime.HasValue )
                {
                    eventItemIndex.EventItemOccurrences.Add(
                        new EventItemOccurrence
                        {
                            Id = occurrence.Id,
                            CampusId = occurrence.CampusId,
                            ContactPersonAliasId = occurrence.ContactPersonAliasId,
                            ContactPersonName = occurrence.ContactPersonAlias?.Person.FullName,
                            ContactPersonEmail = occurrence.ContactEmail,
                            ContactPersonPhone = occurrence.ContactPhone,
                            Location = occurrence.Location,
                            NextStartDateTime = occurrence.NextStartDateTime,
                            Note = occurrence.Note,
                            ScheduleId = occurrence.ScheduleId
                        } );
                }
            }

            // If there are no upcoming occurrences then don't index
            if ( eventItemIndex.EventItemOccurrences.Count == 0 )
            {
                return null;
            }

            // Add calendars
            foreach( var calendar in eventItem.EventCalendarItems )
            {
                // Only add calendars that are set to be indexed
                if ( calendar.EventCalendar.IsIndexEnabled )
                {
                    eventItemIndex.EventCalendars.Add(
                        new EventCalendar
                        {
                            Id = calendar.EventCalendarId,
                            Name = calendar.EventCalendar.Name
                        } );
                }
            }

            AddIndexableAttributes( eventItemIndex, eventItem );

            return eventItemIndex;
        }

        /// <summary>
        /// Formats the search result.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="displayOptions">The display options.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <returns></returns>
        public override FormattedSearchResult FormatSearchResult( Person person, Dictionary<string, object> displayOptions = null, Dictionary<string, object> mergeFields = null )
        {
            var result = base.FormatSearchResult( person, displayOptions );
            bool isSecurityDisabled = false;

            if ( displayOptions != null )
            {
                if ( displayOptions.ContainsKey( "EventItem-IsSecurityDisabled" ) )
                {
                    isSecurityDisabled = displayOptions["EventItem-IsSecurityDisabled"].ToString().AsBoolean();
                }
            }

            // If security is disabled true all items
            if ( isSecurityDisabled )
            {
                result.IsViewAllowed = true;
                return result;
            }

            // Otherwise we're checking security
            var eventItem = new EventItemService( new Data.RockContext() ).Get( ( int ) this.Id );

            // Check if item in database has been deleted
            if ( eventItem.IsNull() )
            {
                result.IsViewAllowed = false;
                return result;
            }

            if ( eventItem.IsAuthorized( "View", person ) )
            {
                result.IsViewAllowed = true;
                return result;
            }
            else
            {
                result.IsViewAllowed = false;
                return result;
            }
        }
    }

    #region POCOs
    /// <summary>
    /// POCO to store information about the audience
    /// </summary>
    public class EventItemAudience : RockDynamic
    {
        /// <summary>
        /// Gets or sets the defined value identifier.
        /// </summary>
        /// <value>
        /// The defined value identifier.
        /// </value>
        public int DefinedValueId { get; set; }

        /// <summary>
        /// Gets or sets the name of the audience.
        /// </summary>
        /// <value>
        /// The name of the audience.
        /// </value>
        public string AudienceName { get; set; }
    }

    /// <summary>
    /// POCO to store basic information on the calendars the event is on.
    /// </summary>
    public class EventCalendar : RockDynamic
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
    }

    /// <summary>
    /// POCO to store information on the upcoming occurrences
    /// </summary>
    public class EventItemOccurrence : RockDynamic
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        public int? CampusId{ get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the schedule identifier.
        /// </summary>
        /// <value>
        /// The schedule identifier.
        /// </value>
        public int? ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the contact person alias identifier.
        /// </summary>
        /// <value>
        /// The contact person alias identifier.
        /// </value>
        public int? ContactPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the name of the contact person.
        /// </summary>
        /// <value>
        /// The name of the contact person.
        /// </value>
        public string ContactPersonName { get; set; }

        /// <summary>
        /// Gets or sets the contact person email.
        /// </summary>
        /// <value>
        /// The contact person email.
        /// </value>
        public string ContactPersonEmail { get; set; }

        /// <summary>
        /// Gets or sets the contact person phone.
        /// </summary>
        /// <value>
        /// The contact person phone.
        /// </value>
        public string ContactPersonPhone { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>
        /// The note.
        /// </value>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the next start date time.
        /// </summary>
        /// <value>
        /// The next start date time.
        /// </value>
        public DateTime? NextStartDateTime { get; set; }
    }
    #endregion
}

// <copyright>
// Copyright by BEMA Information Technologies
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
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using DDay.iCal;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace com.bemaservices.ClientPackage.VOCH.Controllers
{
    public partial class EventLinkController : Rock.Rest.ApiController<Rock.Model.EventItemOccurrence>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupToolsController"/> class.
        /// </summary>
        public EventLinkController() : base( new Rock.Model.EventItemOccurrenceService( new Rock.Data.RockContext() ) ) { }
    }

    public partial class EventLinkController
    {

        [Authenticate, Secured]
        [System.Web.Http.Route( "api/com_bemaservices/EventLink/GetCalendarItems" )]
        public IQueryable<CalendarItemInfo> GetCalendarItems(
            string categoryIds = "",
            string campusIds = "",
            string calendarIds = "",
            DateTime? startDateTime = null,
            DateTime? endDateTime = null )
        {
            var rockContext = new RockContext();
            var calendarInfoList = new List<CalendarItemInfo>();
            var definedValueService = new DefinedValueService( rockContext );
            var binaryFileService = new BinaryFileService( rockContext );
            int entityTypeId = EntityTypeCache.GetId( typeof( EventItemOccurrence ) ) ?? 0;

            var categoryIdList = categoryIds.SplitDelimitedValues().AsIntegerList();
            var campusIdList = campusIds.SplitDelimitedValues().AsIntegerList();
            var calendarIdList = calendarIds.SplitDelimitedValues().AsIntegerList();

            var eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );
            var qry = eventItemOccurrenceService.Queryable();

            if ( calendarIdList.Any() )
            {
                qry = qry.Where( g => g.EventItem.EventCalendarItems.Any( eci => calendarIdList.Contains( eci.EventCalendarId ) ) );
            }

            if ( campusIdList.Any() )
            {
                qry = qry.Where( g => g.CampusId != null && campusIdList.Contains( g.CampusId.Value ) );
            }

            if ( categoryIdList.Any() )
            {
                var eventCalendarItemQry = qry.SelectMany( g => g.EventItem.EventCalendarItems );
                var categoryList = new DefinedValueService( rockContext ).GetByIds( categoryIdList ).Select( c => c.Guid.ToString().ToUpper() ).ToList();
                var eventCalendarItemIdList = eventCalendarItemQry.WhereAttributeValue( rockContext, av => av.Attribute.Key == "Category" && categoryList.Any( c => av.Value.ToUpper().Contains( c ) ) )
                                                .Select( c => c.Id )
                                                .ToList();
                qry = qry.Where( g => g.EventItem.EventCalendarItems.Any( c => eventCalendarItemIdList.Contains( c.Id ) ) );
            }

            qry = qry.Where( eio => eio != null && eio.EventItem != null );

            if ( startDateTime == null )
            {
                startDateTime = DateTime.Now;
            }

            if ( endDateTime == null )
            {
                endDateTime = DateTime.Now.AddMonths( 1 );
            }

            var qryStartDateTime = startDateTime.Value.AddMonths( -1 );
            var qryEndDateTime = endDateTime.Value.AddMonths( 1 );

            var eventItemOccurrences = qry.ToList();
            var eventItemOccurrencesWithDates = eventItemOccurrences
                .Select( r => new CalendarDate
                {
                    EventItemOccurrence = r,
                    CalendarDateTimes = GetCalendarTimes( r.Schedule, qryStartDateTime, qryEndDateTime ).Where( c => (
                          ( ( c.StartDateTime >= startDateTime ) || ( c.EndDateTime >= startDateTime ) ) &&
                          ( ( c.StartDateTime < endDateTime ) || ( c.EndDateTime < endDateTime ) ) ) )
                            .OrderBy( d => d.StartDateTime )
                            .ToList()
                } )
                .Where( r => r.CalendarDateTimes.Any() )
                .ToList();

            foreach ( var eventItemOccurrenceDate in eventItemOccurrencesWithDates )
            {
                var eventItemOccurrence = eventItemOccurrenceDate.EventItemOccurrence;
                var eventItem = eventItemOccurrence.EventItem;

                var calendarItemInfo = new CalendarItemInfo();

                calendarItemInfo.OccurrenceId = eventItemOccurrence.Id;
                calendarItemInfo.OccurrenceLocation = eventItemOccurrence.Location;
                calendarItemInfo.OccurrenceNote = eventItemOccurrence.Note;

                if ( eventItemOccurrence.ContactPersonAlias != null )
                {
                    calendarItemInfo.ContactPersonAliasId = eventItemOccurrence.ContactPersonAliasId;
                    calendarItemInfo.ContactFirstName = eventItemOccurrence.ContactPersonAlias.Person.NickName;
                    calendarItemInfo.ContactLastName = eventItemOccurrence.ContactPersonAlias.Person.LastName;
                }

                calendarItemInfo.ContactEmail = eventItemOccurrence.ContactEmail;
                calendarItemInfo.ContactPhone = eventItemOccurrence.ContactPhone;

                calendarItemInfo.EventItemId = eventItem.Id;
                calendarItemInfo.EventName = eventItem.Name;
                calendarItemInfo.EventDescription = eventItem.Description;
                calendarItemInfo.EventSummary = eventItem.Summary;
                calendarItemInfo.EventDetailsUrl = eventItem.DetailsUrl;

                if ( eventItemOccurrence.Campus != null )
                {
                    calendarItemInfo.CampusId = eventItemOccurrence.CampusId;
                    calendarItemInfo.CampusName = eventItemOccurrence.Campus.Name;
                }

                if ( eventItem.Photo != null )
                {
                    calendarItemInfo.EventPhoto = eventItem.Photo;
                }

                List<DefinedValue> itemCategoryList = new List<DefinedValue>();
                foreach ( var eventCalendarItem in eventItem.EventCalendarItems )
                {
                    eventCalendarItem.LoadAttributes();
                    var categoryGuids = eventCalendarItem.GetAttributeValue( "Category" ).SplitDelimitedValues().AsGuidList();
                    if ( categoryGuids.Any() )
                    {
                        var categories = definedValueService.GetByGuids( categoryGuids );
                        foreach ( var category in categories )
                        {
                            itemCategoryList.Add( category );
                        }
                    }
                }
                calendarItemInfo.Categories = itemCategoryList;

                calendarItemInfo.UpcomingDates = eventItemOccurrenceDate.CalendarDateTimes;
                calendarItemInfo.EventNextStartDate = calendarItemInfo.UpcomingDates.FirstOrDefault();

                calendarItemInfo.SocialMediaLinks = new List<SocialMediaLink>();

                eventItem.LoadAttributes();
                var facebookPhotoGuid = eventItem.GetAttributeValue( "core_calendar_FacebookPhoto" ).AsGuidOrNull();
                if ( facebookPhotoGuid != null )
                {
                    var facebookPhoto = binaryFileService.Get( facebookPhotoGuid.Value );
                    if ( facebookPhoto != null )
                    {
                        var facebookLink = new SocialMediaLink();
                        facebookLink.Platform = "Facebook";
                        facebookLink.Link = facebookPhoto.Url;
                        calendarItemInfo.SocialMediaLinks.Add( facebookLink );
                    }
                }

                var twitterPhotoGuid = eventItem.GetAttributeValue( "core_calendar_TwitterPhoto" ).AsGuidOrNull();
                if ( twitterPhotoGuid != null )
                {
                    var twitterPhoto = binaryFileService.Get( twitterPhotoGuid.Value );
                    if ( twitterPhoto != null )
                    {
                        var twitterLink = new SocialMediaLink();
                        twitterLink.Platform = "Twitter";
                        twitterLink.Link = twitterPhoto.Url;
                        calendarItemInfo.SocialMediaLinks.Add( twitterLink );
                    }
                }

                calendarItemInfo.RegistrationInformation = new List<RegistrationInformation>();
                foreach ( var linkage in eventItemOccurrence.Linkages )
                {
                    var registrationInformation = new RegistrationInformation();
                    registrationInformation.RegistrationPublicName = linkage.PublicName;
                    registrationInformation.RegistrationPublicSlug = linkage.UrlSlug;

                    var registrationInstance = linkage.RegistrationInstance;
                    if ( registrationInstance != null )
                    {
                        registrationInformation.RegistrationInstanceId = registrationInstance.Id;
                        registrationInformation.RegistrationStartDate = registrationInstance.StartDateTime;
                        registrationInformation.RegistrationEndDate = registrationInstance.EndDateTime;
                        registrationInformation.RegistrationMaxAttendees = registrationInstance.MaxAttendees;
                        registrationInformation.RegistrationTotalRegistrants = registrationInstance.Registrations.Sum( r => r.Registrants.Count );
                    }

                    calendarItemInfo.RegistrationInformation.Add( registrationInformation );
                }

                calendarInfoList.Add( calendarItemInfo );
            }

            var calendarInfoQry = calendarInfoList.AsQueryable();

            return calendarInfoQry;
        }

        public List<CalendarDateTime> GetCalendarTimes( Schedule schedule, DateTime beginDateTime, DateTime endDateTime )
        {
            if ( schedule != null )
            {
                var result = new List<CalendarDateTime>();

                DDay.iCal.Event calEvent = schedule.GetCalendarEvent();
                if ( calEvent != null && calEvent.DTStart != null )
                {
                    var occurrences = ScheduleICalHelper.GetOccurrences( calEvent, beginDateTime, endDateTime );
                    result = occurrences
                        .Where( a =>
                            a.Period != null &&
                            a.Period.StartTime != null &&
                            a.Period.EndTime != null )
                        .Select( a => new CalendarDateTime
                        {
                            StartDateTime = DateTime.SpecifyKind( a.Period.StartTime.Value, DateTimeKind.Local ),
                            EndDateTime = DateTime.SpecifyKind( a.Period.EndTime.Value, DateTimeKind.Local )
                        } )
                        .OrderBy( a => a.StartDateTime )
                        .ToList();
                    {
                        // ensure the datetime is DateTimeKind.Local since iCal returns DateTimeKind.UTC
                    }
                }

                return result;
            }
            else
            {
                return new List<CalendarDateTime>();
            }

        }
    }

    /// <summary>
    /// A class to store group data to be returned by the API
    /// </summary>
    public class CalendarItemInfo
    {
        public int? OccurrenceId { get; set; }
        public string OccurrenceLocation { get; set; }
        public string OccurrenceNote { get; set; }

        public int? ContactPersonAliasId { get; set; }
        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }

        public int EventItemId { get; set; }
        public string EventName { get; set; }
        public string EventDescription { get; set; }
        public string EventSummary { get; set; }
        public string EventDetailsUrl { get; set; }

        public BinaryFile EventPhoto { get; set; }

        public List<DefinedValue> Categories { get; set; }

        public int? CampusId { get; set; }
        public string CampusName { get; set; }

        public CalendarDateTime EventNextStartDate { get; set; }
        public List<CalendarDateTime> UpcomingDates { get; set; }

        public List<SocialMediaLink> SocialMediaLinks { get; set; }

        public List<RegistrationInformation> RegistrationInformation { get; set; }

    }

    public class SocialMediaLink
    {
        public string Platform { get; set; }
        public string Link { get; set; }
    }

    public class CalendarDateTime
    {
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
    }

    public class CalendarDate
    {
        public EventItemOccurrence EventItemOccurrence { get; set; }
        public List<CalendarDateTime> CalendarDateTimes { get; set; }
    }

    public class RegistrationInformation
    {
        public int? RegistrationInstanceId { get; set; }
        public DateTime? RegistrationStartDate { get; set; }
        public DateTime? RegistrationEndDate { get; set; }
        public string RegistrationPublicName { get; set; }
        public string RegistrationPublicSlug { get; set; }
        public int? RegistrationMaxAttendees { get; set; }
        public int? RegistrationTotalRegistrants { get; set; }
    }


}

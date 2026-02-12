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

using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Engagement.OutreachDashboard;
using Rock.Common.Mobile.Blocks.Engagement.ContactProfile;
using Rock.Common.Mobile.Blocks.Engagement.OutreachOnboarding.cs;
using Rock.Enums.Core;
using Rock.Enums.Engagement;
using Rock.Mobile;
using Rock.Model;
using Rock.Utility;

namespace Rock.Blocks.Types.Mobile.Engagement
{
    /// <summary>
    /// Beacon dashboard allows you to view your touchpoint statistic and as well as start connecting with your contact.
    /// </summary>
    [DisplayName( "Outreach Dashboard" )]
    [Category( "Engagement" )]
    [IconCssClass( "ti ti-device-desktop" )]
    [Description( "Outreach dashboard allows you to view your touchpoint statistic and as well as start connecting with your contact." )]
    [SupportedSiteTypes( SiteType.Mobile )]

    [LinkedPage( "Detail Page",
        Description = "The page to link to when user taps on a Start Connecting.",
        IsRequired = false,
        Key = AttributeKeys.DetailPage,
        Order = 1 )]

    [LinkedPage( "Contact List Page",
        Description = "The page to open when someone taps on a contact.",
        IsRequired = false,
        Key = AttributeKeys.MyContact,
        Order = 2 )]

    [TextField( "Toolbox Name",
        Description = "The public name of this experience.",
        IsRequired = false,
        DefaultValue = "Beacon",
        Key = AttributeKeys.ToolboxName,
        Order = 3 )]

    [TextField( "Toolbox Subtitle",
        Description = "The subtitle appears below the Toolbox name.",
        IsRequired = false,
        DefaultValue = "The subtitle that appears below the Toolbox Name.",
        Key = AttributeKeys.ToolboxSubtitle,
        Order = 4 )]

    [IntegerField(
        "Completion Lookback Period",
        Description = "The number of days to look back when calculating on-time completion.",
        IsRequired = true,
        DefaultValue = "30",
        Order = 5,
        Key = AttributeKeys.CompletionLookbackPeriod )]

    [SystemGuid.EntityTypeGuid( SystemGuid.EntityType.MOBILE_OUTREACH_OUTREACH_DASHBOARD_BLOCK_TYPE )]
    [SystemGuid.BlockTypeGuid( SystemGuid.BlockType.MOBILE_OUTREACH_OUTREACH_BEACON_DASHBOARD )]
    public class OutreachDashboard : RockBlockType
    {
        #region Constants

        private static class AttributeKeys
        {
            public const string BaptismInfo = "BaptismInfo";
            public const string DetailPage = "DetailPage";
            public const string MyContact = "MyContact";
            public const string ToolboxName = "ToolboxName";
            public const string ToolboxSubtitle = "ToolboxSubtitle";
            public const string CompletionLookbackPeriod = "CompletionLookbackPeriod";
        }

        #endregion

        #region Methods

        /// <summary>
        /// Resolves the URL.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string ResolveURL( string url )
        {
            if ( url.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            if ( url.StartsWith( "http://" ) || url.StartsWith( "https://" ) )
            {
                return url;
            }
            else
            {
                return "https://" + url;
            }
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the initial data.
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetInitialData()
        {
            var person = RequestContext.CurrentPerson;

            if ( person == null )
            {
                return ActionBadRequest( "Current person not found." );
            }

            var now = RockDateTime.Now;

            // Get all the person's contact ids.
            ContactService contactService = new ContactService( RockContext );
            var personContactIds = contactService
                .Queryable()
                .Where( c => c.OwnerPersonAliasId == person.PrimaryAliasId )
                .Select( c => c.Id );

            // Get count of pending touchpoints.
            ContactTouchpointService touchpointService = new ContactTouchpointService( RockContext );
            var pendingTouchpoints = touchpointService
                .Queryable()
                .Where( tp => personContactIds.Contains( tp.ContactId ) )
                .Where( tp => tp.CompletedDateTime == null )
                .Where( tp => tp.ScheduledDateTime <= now )
                .OrderBy( tp => tp.ScheduledDateTime )
                .Select( tp => new
                {
                    tp.Contact.PhotoId
                } ).ToList();
            var pendingTouchpointCount = pendingTouchpoints.Count();


            var startOfMonth = new DateTime( now.Year, now.Month, 1 );
            var nextMonthStart = startOfMonth.AddMonths( 1 );

            var completedTouchpoint = touchpointService
                .Queryable()
                .Where( tp => personContactIds.Contains( tp.ContactId ) )
                .Where( tp => tp.CompletedDateTime != null )
                .GroupBy( tp => tp.Type )
                .Select( g => new
                {
                    Type = g.Key,
                    Total = g.Count(),
                    ThisMonth = g.Count( tp => tp.CompletedDateTime >= startOfMonth && tp.CompletedDateTime < nextMonthStart )
                } )
              .ToList();

            // Get prayer touchpoints completed this month and total.
            var completedPrayerTouchpoint = completedTouchpoint.FirstOrDefault( tp => tp.Type == TouchpointType.Prayer );
            var totalPrayerCompletedCount = completedPrayerTouchpoint?.Total ?? 0;
            var prayerCompletedThisMonthCount = completedPrayerTouchpoint?.ThisMonth ?? 0;

            // Get connection touchpoints completed this month and total.
            var completedConnectionTouchpoint = completedTouchpoint.FirstOrDefault( tp => tp.Type == TouchpointType.Connection );
            var totalCompletedConnectionCount = completedConnectionTouchpoint?.Total ?? 0;
            var connectionCompletedThisMonthCount = completedConnectionTouchpoint?.ThisMonth ?? 0;

            // Get any special events that haven't been completed that occurred in the past week.
            var weekAgoDay = now.AddDays( -7 );
            var pastSpecialEventsTouchpoint = touchpointService
                .Queryable()
                .Where( tp => personContactIds.Contains( tp.ContactId ) )
                .Where( tp => tp.Type == TouchpointType.Birthday
                    || tp.Type == TouchpointType.WeddingAnniversary
                    || tp.Type == TouchpointType.BaptismAnniversary
                    || tp.Type == TouchpointType.SalvationAnniversary )
                .Where( tp => tp.ScheduledDateTime <= now && tp.ScheduledDateTime >= weekAgoDay )
                .Where( tp => tp.CompletedDateTime == null )
                .OrderByDescending( tp => tp.ScheduledDateTime )
                .Select( tp => new
                {
                    tp.Id,
                    tp.Contact.Gender,
                    tp.Type,
                    tp.ScheduledDateTime,
                    tp.Contact.PhotoId,
                    tp.Contact.FirstName,

                } )
                .ToList()
                .Select( tp =>
                {
                    var profileURL = tp.PhotoId.HasValue
                        ? MobileHelper.BuildPublicApplicationRootUrl( FileUrlHelper.GetImageUrl( tp.PhotoId.Value, new GetImageUrlOptions { Width = 256, Height = 256 } ) )
                        : "";

                    return new PastTouchpointEvent
                    {
                        TouchpointIdKey = IdHasher.Instance.GetHash( tp.Id ),
                        Gender = tp.Gender.ToMobile(),
                        ProfileURL = profileURL,
                        contactName = tp.FirstName,
                        TouchpointType = tp.Type.ToMobile(),
                        ScheduledDate = tp.ScheduledDateTime
                    };
                } ).ToList();

            // Get the profile image urls for pending touchpoints.
            var pendingTouchpointImageUrls = pendingTouchpoints
                .Select( tp =>
                {
                    var profileURL = tp.PhotoId.HasValue
                        ? MobileHelper.BuildPublicApplicationRootUrl( FileUrlHelper.GetImageUrl( tp.PhotoId.Value, new GetImageUrlOptions { Width = 256, Height = 256 } ) )
                        : "";
                    return profileURL;
                } ).ToList();

            // Calculate the percentage of touchpoints finished on time.
            var lookbackPeriodStartDate = RockDateTime.Now.AddDays( -GetAttributeValue( AttributeKeys.CompletionLookbackPeriod ).AsInteger() );
            var onTimeStats = touchpointService.Queryable()
                .Where( tp => personContactIds.Contains( tp.ContactId ) )
                .Where( tp => tp.CompletedDateTime != null && tp.ScheduledDateTime != null )
                .Where( tp => tp.ScheduledDateTime >= lookbackPeriodStartDate )
                .GroupBy( _ => 1 )
                .Select( g => new
                {
                    Total = g.Count(),
                    OnTime = g.Count( tp => tp.CompletedDateTime <= DbFunctions.AddDays( tp.ScheduledDateTime, 1 ) )
                } )
                .FirstOrDefault();

            var totalCompleted = onTimeStats?.Total ?? 0;
            var finishedOnTime = onTimeStats?.OnTime ?? 0;

            var percentTouchpointsFinishedOnTime = totalCompleted == 0
                ? 0
                : ( int ) Math.Round( ( double ) finishedOnTime / totalCompleted * 100 );

            // Get the average number of touchpoints generated per day.
            var contacts = contactService.Queryable().Where( c => c.OwnerPersonAliasId == person.PrimaryAliasId );
            var numberOfTouchpointDays = person.OutreachTouchpointSchedule.AsDayOfWeekList().Count;
            var count = ContactTouchpointService.GetDailyTouchpointCount( contacts, TouchpointType.Prayer, numberOfTouchpointDays );
            count += ContactTouchpointService.GetDailyTouchpointCount( contacts, TouchpointType.Connection, numberOfTouchpointDays );

            // Get weekly touchpoints completed
            var startWeek = now.StartOfWeek( RockDateTime.FirstDayOfWeek ).Date;
            var startWeekDt = startWeek;
            var startNextWeekDt = startWeekDt.AddDays( 7 );

            var weeklyCompletedTouchpoint = touchpointService.Queryable()
                    .Where( tp => personContactIds.Contains( tp.ContactId ) )
                    .Where( tp => tp.CompletedDateTime != null )
                    .Where( tp => tp.CompletedDateTime.Value >= startWeekDt
                                && tp.CompletedDateTime.Value < startNextWeekDt )
                    .Select( tp => new
                    {
                        tp.CompletedDateTime
                    } )
                    .AsEnumerable()
                    .GroupBy( tp => tp.CompletedDateTime.Value.DayOfWeek )
                    .ToDictionary(
                        g => g.Key,
                        g => g.Count()
                    );

            // Build the POCO to send out.
            var data = new InitialDataBag
            {
                ContactCount = personContactIds.Count(),
                PendingTouchpointCount = pendingTouchpointCount,
                PrayerCompletedThisMonth = prayerCompletedThisMonthCount,
                TotalCompletedPrayerCount = totalPrayerCompletedCount,
                ConnectionsCompletedThisMonth = connectionCompletedThisMonthCount,
                TotalCompletedConnectionsCount = totalCompletedConnectionCount,
                PastSpecialEvents = pastSpecialEventsTouchpoint,
                TouchpointContactImageUrls = pendingTouchpointImageUrls,
                PercentTouchpointFinishedOnTime = percentTouchpointsFinishedOnTime,
                DailyNotificationsEnabled = person.OutreachEnableDailyNotification,
                SpecialEventNotificationsEnabled = person.OutreachEnableSpecialEventsNotification,
                dayOfWeekFlag = ( Common.Mobile.Enums.DayOfWeekFlag ) ( ( int ) person.OutreachTouchpointSchedule ),
                OutreachNotificationTimeOfDay = ( Common.Mobile.Enums.OutreachNotificationTimeOfDay? ) person.OutreachNotificationTimeOfDay,
                PersonProfileUrl = MobileHelper.BuildPublicApplicationRootUrl( GetCurrentPerson().PhotoUrl ),
                NumberOfTouchpointsGeneratedPerDay = ( int ) Math.Round( count ),
                TouchpointCountCompletedDayOfWeek = weeklyCompletedTouchpoint,
            };

            return ActionOk( data );
        }

        /// <summary>
        /// Saves the preferences.
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult SavePreferences( SavePreferencesBag savePreferenceBag )
        {
            var person = RequestContext.CurrentPerson;
            if ( person == null )
            {
                return ActionBadRequest( "Current person not found." );
            }

            PersonService personService = new PersonService( RockContext );
            person = personService.Get( person.Id );
            person.OutreachTouchpointSchedule = ( DaysOfWeekFlags ) ( ( int ) savePreferenceBag.DayOfWeek );
            person.OutreachEnableDailyNotification = savePreferenceBag.DailyNotificationsEnabled;
            person.OutreachNotificationTimeOfDay = savePreferenceBag.DailyNotificationsEnabled ? ( OutreachNotificationTimeOfDay? ) savePreferenceBag.TimeOfDay : null; // Clear out time of day if daily notifications are disabled
            person.OutreachEnableSpecialEventsNotification = savePreferenceBag.SpecialEventNotificationsEnabled;

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Gets the contact touchpoint history.
        /// </summary>
        /// <param name="contactId"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetContactTouchpointHistory( int contactId )
        {
            ContactService contactService = new ContactService( RockContext );
            var contact = contactService.Get( contactId );
            if ( contact == null )
            {
                return ActionBadRequest( "Contact not found." );
            }

            ContactTouchpointService contactTouchpointService = new ContactTouchpointService( RockContext );
            var qry = contactTouchpointService.Queryable()
                .Where( tp => tp.ContactId == contact.Id )
                .Where( tp => tp.CompletedDateTime.HasValue );

            var touchpointHistoryBag = qry
                .OrderByDescending( bag => bag.CompletedDateTime )
                .AsEnumerable()
                .Select( tp => new TouchpointHistoryBag
                {
                    CommunicationMedium = tp.CommunicationMedium?.ToMobile(),
                    TouchpointType = tp.Type.ToMobile(),
                    ContactFirstName = contact.FirstName,
                    ScheduleDateTime = tp.ScheduledDateTime,
                    CompletedDateTime = tp.CompletedDateTime.Value,
                    Note = tp.Note
                } ).ToList();

            return ActionOk( touchpointHistoryBag );
        }

        /// <summary>
        /// Stops all touchpoints.
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult StopAllTouchpoints()
        {
            var person = RequestContext.CurrentPerson;
            if ( person == null )
            {
                return ActionBadRequest( "Current person not found." );
            }

            person.OutreachTouchpointGenerationEnabled = false;

            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion

        /// <inheritdoc/>
        public override object GetMobileConfigurationValues()
        {
            return new Rock.Common.Mobile.Blocks.Engagement.OutreachDashboard.Configuration
            {
                DetailPage = GetAttributeValue( AttributeKeys.DetailPage ).AsGuidOrNull(),
                BaptismInfoUrl = ResolveURL( GetAttributeValue( AttributeKeys.BaptismInfo ) ),
                MyContactPage = GetAttributeValue( AttributeKeys.MyContact ).AsGuidOrNull(),
                ToolboxName = GetAttributeValue( AttributeKeys.ToolboxName ),
                ToolboxSubtitle = GetAttributeValue( AttributeKeys.ToolboxSubtitle )
            };
        }
    }
}

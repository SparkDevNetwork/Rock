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
using System.Web;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.CheckIn
{
    /// <summary>
    /// 
    /// </summary>
    public static class CheckinManagerHelper
    {
        /// <summary>
        /// 
        /// </summary>
        public class PageParameterKey
        {
            /// <summary>
            /// Gets or sets the current 'Check-in Configuration' Guid (which is a <see cref="Rock.Model.GroupType" /> Guid).
            /// For example "Weekly Service Check-in".
            /// </summary>
            public const string Area = "Area";

            /// <summary>
            /// The location identifier
            /// </summary>
            public const string LocationId = "LocationId";
        }

        /// <summary>
        /// 
        /// </summary>
        public class BlockAttributeKey
        {
            /// <summary>
            /// Show all areas (Weekly Service Checkin, Volunteer Checkin, etc)
            /// </summary>
            public const string ShowAllAreas = "ShowAllAreas";

            /// <summary>
            /// Gets or sets the current 'Check-in Configuration' Guid (which is a <see cref="Rock.Model.GroupType" /> Guid).
            /// For example "Weekly Service Check-in".
            /// </summary>
            public const string CheckInAreaGuid = "CheckInAreaGuid";
        }

        /// <summary>
        /// Gets the checkin area filter that the (Checkin Manager) block uses.
        /// Determined by 'Area' PageParameter, 'ShowAllAreas' block setting, Cookie or 'CheckInAreaGuid' block setting
        /// </summary>
        /// <param name="rockBlock">The rock block.</param>
        /// <returns></returns>
        public static GroupTypeCache GetCheckinAreaFilter( RockBlock rockBlock )
        {
            // If a Check-in Area query string parameter is defined, it takes precedence.
            Guid? checkinManagerPageParameterCheckinAreaGuid = rockBlock.PageParameter( PageParameterKey.Area ).AsGuidOrNull();
            if ( checkinManagerPageParameterCheckinAreaGuid.HasValue )
            {
                var checkinManagerPageParameterCheckinArea = GroupTypeCache.Get( checkinManagerPageParameterCheckinAreaGuid.Value );

                if ( checkinManagerPageParameterCheckinArea != null )
                {
                    return checkinManagerPageParameterCheckinArea;
                }
            }

            // If ShowAllAreas is enabled, we won't filter by Check-in Area (unless there was a page parameter).
            if ( rockBlock.GetAttributeValue( BlockAttributeKey.ShowAllAreas ).AsBoolean() )
            {
                return null;
            }

            // If ShowAllAreas is false, get the area filter from the cookie.
            var checkinManagerCookieCheckinAreaGuid = CheckinManagerHelper.GetCheckinManagerConfigurationFromCookie().CheckinAreaGuid;
            if ( checkinManagerCookieCheckinAreaGuid != null )
            {
                var checkinManagerCookieCheckinArea = GroupTypeCache.Get( checkinManagerCookieCheckinAreaGuid.Value );
                if ( checkinManagerCookieCheckinArea != null )
                {
                    return checkinManagerCookieCheckinArea;
                }
            }

            // Next, check the Block AttributeValue.
            var checkinManagerBlockAttributeCheckinAreaGuid = rockBlock.GetAttributeValue( BlockAttributeKey.CheckInAreaGuid ).AsGuidOrNull();
            if ( checkinManagerBlockAttributeCheckinAreaGuid.HasValue )
            {
                var checkinManagerBlockAttributeCheckinArea = GroupTypeCache.Get( checkinManagerBlockAttributeCheckinAreaGuid.Value );
                if ( checkinManagerBlockAttributeCheckinArea != null )
                {
                    return checkinManagerBlockAttributeCheckinArea;
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the selected location.
        /// Note this will redirect to the current page to include a LocationId query parameter if a LocationId parameter in the URL is missing or doesn't match.
        /// </summary>
        /// <param name="rockBlock">The rock block.</param>
        /// <param name="lpLocation">The lp location.</param>
        /// <param name="locationId">The identifier of the location.</param>
        /// <param name="campusId">The campus identifier.</param>
        public static void SetSelectedLocation( RockBlock rockBlock, LocationPicker lpLocation, int? locationId, int campusId )
        {
            if ( locationId.HasValue && locationId > 0 )
            {
                CheckinManagerHelper.SaveCampusLocationConfigurationToCookie( campusId, locationId );
                var pageParameterLocationId = rockBlock.PageParameter( PageParameterKey.LocationId ).AsIntegerOrNull();
                if ( !pageParameterLocationId.HasValue || pageParameterLocationId.Value != locationId )
                {
                    var additionalQueryParameters = new Dictionary<string, string>();
                    additionalQueryParameters.Add( PageParameterKey.LocationId, locationId.ToString() );
                    rockBlock.NavigateToCurrentPageReference( additionalQueryParameters );
                    return;
                }

                using ( var rockContext = new RockContext() )
                {
                    if ( locationId.HasValue )
                    {
                        lpLocation.SetNamedLocation( NamedLocationCache.Get( locationId.Value ) );
                    }
                }
            }
            else
            {
                lpLocation.Location = null;
                CheckinManagerHelper.SaveCampusLocationConfigurationToCookie( campusId, null );
            }
        }

        /// <summary>
        /// Gets the selected location.
        /// </summary>
        /// <param name="rockBlock">The rock block.</param>
        /// <param name="campus">The campus.</param>
        /// <param name="lpLocation">The lp location.</param>
        /// <returns></returns>
        public static int? GetSelectedLocation( RockBlock rockBlock, CampusCache campus, LocationPicker lpLocation )
        {
            // If the Campus selection has changed, we need to reload the LocationItemPicker with the Locations specific to that Campus.
            lpLocation.NamedPickerRootLocationId = campus.LocationId.GetValueOrDefault();

            // Check the LocationPicker for the Location ID.
            int locationId = lpLocation.NamedLocation?.Id ?? 0;

            if ( locationId > 0 )
            {
                return locationId;
            }

            // If not defined on the LocationPicker, check first for a LocationId Page parameter.
            locationId = rockBlock.PageParameter( PageParameterKey.LocationId ).AsInteger();

            if ( locationId > 0 )
            {
                // double check the locationId in the URL is valid for the Campus (just in case it was altered or is no longer valid for the campus)
                var locationCampusId = NamedLocationCache.Get( locationId ).CampusId;
                if ( locationCampusId != campus.Id )
                {
                    locationId = 0;
                }
            }

            if ( locationId > 0 )
            {
                CheckinManagerHelper.SaveCampusLocationConfigurationToCookie( campus.Id, locationId );
            }
            else
            {
                // If still not defined, check for cookie setting.
                locationId = CheckinManagerHelper.GetCheckinManagerConfigurationFromCookie().LocationIdFromSelectedCampusId.GetValueOrNull( campus.Id ) ?? 0;
                
                if ( locationId > 0 )
                {
                    // double check the locationId in the cookie is valid for the Campus (just in case it was altered or is no longer valid for the campus)
                    var locationCampusId = NamedLocationCache.Get( locationId )?.CampusId;
                    if ( locationCampusId != campus.Id )
                    {
                        locationId = 0;
                    }
                }

                if ( locationId <= 0 )
                {
                    return null;
                }
            }

            return locationId;
        }

        /// <summary>
        /// Saves the campus location configuration to the response cookie
        /// </summary>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        public static void SaveCampusLocationConfigurationToCookie( int campusId, int? locationId )
        {
            CheckinManagerConfiguration checkinManagerConfiguration = GetCheckinManagerConfigurationFromCookie();

            if ( locationId.HasValue )
            {
                checkinManagerConfiguration.LocationIdFromSelectedCampusId.AddOrReplace( campusId, locationId.Value );
            }
            else
            {
                checkinManagerConfiguration.LocationIdFromSelectedCampusId.Remove( campusId );
            }

            SaveCheckinManagerConfigurationToCookie( checkinManagerConfiguration );
        }

        /// <summary>
        /// Saves the roster configuration to the response cookie.
        /// </summary>
        /// <param name="rosterStatusFilter">The roster status filter.</param>
        public static void SaveRosterConfigurationToCookie( RosterStatusFilter rosterStatusFilter )
        {
            CheckinManagerConfiguration checkinManagerConfiguration = GetCheckinManagerConfigurationFromCookie();
            checkinManagerConfiguration.RosterStatusFilter = rosterStatusFilter;
            SaveCheckinManagerConfigurationToCookie( checkinManagerConfiguration );
        }

        /// <summary>
        /// Saves the selected checkin area unique identifier to the response cookie
        /// </summary>
        /// <returns></returns>
        public static void SaveSelectedCheckinAreaGuidToCookie( Guid? checkinAreaGuid )
        {
            CheckinManagerConfiguration checkinManagerConfiguration = GetCheckinManagerConfigurationFromCookie();
            checkinManagerConfiguration.CheckinAreaGuid = checkinAreaGuid;
            SaveCheckinManagerConfigurationToCookie( checkinManagerConfiguration );
        }

        /// <summary>
        /// Saves the selected label printer to cookie.
        /// Use labelPrinterGuid of Guid.Empty for "local printer".
        /// Use null for nothing selected.
        /// </summary>
        /// <param name="labelPrinterGuid">The label printer unique identifier.</param>
        public static void SaveSelectedLabelPrinterToCookie( Guid? labelPrinterGuid )
        {
            CheckinManagerConfiguration checkinManagerConfiguration = GetCheckinManagerConfigurationFromCookie();
            checkinManagerConfiguration.LabelPrinterGuid = labelPrinterGuid;
            SaveCheckinManagerConfigurationToCookie( checkinManagerConfiguration );
        }

        /// <summary>
        /// Saves the room list filter to the response cookie
        /// </summary>
        /// <param name="roomListScheduleIdsFilter">The room list schedule ids filter.</param>
        public static void SaveRoomListFilterToCookie( int[] roomListScheduleIdsFilter )
        {
            CheckinManagerConfiguration checkinManagerConfiguration = GetCheckinManagerConfigurationFromCookie();
            checkinManagerConfiguration.RoomListScheduleIdsFilter = roomListScheduleIdsFilter;
            SaveCheckinManagerConfigurationToCookie( checkinManagerConfiguration );
        }

        /// <summary>
        /// Saves plugin custom settings to cookie.
        /// </summary>
        /// <param name="customSettings">The custom settings.</param>
        public static void SaveCustomSettingsToCookie( Dictionary<string, string> customSettings )
        {
            CheckinManagerConfiguration checkinManagerConfiguration = GetCheckinManagerConfigurationFromCookie();
            checkinManagerConfiguration.CustomSettings = customSettings;
            SaveCheckinManagerConfigurationToCookie( checkinManagerConfiguration );
        }

        /// <summary>
        /// Saves the checkin manager configuration to response cookie.
        /// </summary>
        /// <param name="checkinManagerConfiguration">The checkin manager configuration.</param>
        private static void SaveCheckinManagerConfigurationToCookie( CheckinManagerConfiguration checkinManagerConfiguration )
        {
            var checkinManagerConfigurationJson = checkinManagerConfiguration.ToJson( indentOutput: false );
            Rock.Web.UI.RockPage.AddOrUpdateCookie( CheckInManagerCookieKey.CheckinManagerConfiguration, checkinManagerConfigurationJson, RockDateTime.Now.AddYears( 1 ) );

            // Also save the Configuration in the Request.Items so that we can grab the configuration from there instead
            // of the Cookie if the configuration changes from what is in the request cookie.
            HttpContext.Current.AddOrReplaceItem( CheckInManagerCookieKey.CheckinManagerConfiguration, checkinManagerConfigurationJson );
        }

        /// <summary>
        /// Gets the roster configuration from the request cookie.
        /// Always returns a non-null CheckinManagerConfiguration.
        /// </summary>
        /// <returns></returns>
        public static CheckinManagerConfiguration GetCheckinManagerConfigurationFromCookie()
        {
            CheckinManagerConfiguration checkinManagerConfiguration = null;

            // First check Request.Items in case we have changed the configuration during this request.
            var currentlySavedCheckinManagerConfigurationJson = HttpContext.Current.Items[CheckInManagerCookieKey.CheckinManagerConfiguration] as string;
            if ( currentlySavedCheckinManagerConfigurationJson.IsNotNullOrWhiteSpace() )
            {
                checkinManagerConfiguration = currentlySavedCheckinManagerConfigurationJson.FromJsonOrNull<CheckinManagerConfiguration>();
            }
            else
            {
                // If we haven't changed the configuration in this request yet, get it from the cookie.
                var checkinManagerRosterConfigurationRequestCookie = HttpContext.Current.Request.Cookies[CheckInManagerCookieKey.CheckinManagerConfiguration];
                if ( checkinManagerRosterConfigurationRequestCookie != null )
                {
                    checkinManagerConfiguration = checkinManagerRosterConfigurationRequestCookie.Value.FromJsonOrNull<CheckinManagerConfiguration>();
                }
            }

            if ( checkinManagerConfiguration == null )
            {
                checkinManagerConfiguration = new CheckinManagerConfiguration();
            }

            if ( checkinManagerConfiguration.LocationIdFromSelectedCampusId == null )
            {
                checkinManagerConfiguration.LocationIdFromSelectedCampusId = new Dictionary<int, int>();
            }

            if ( checkinManagerConfiguration.RoomListScheduleIdsFilter == null )
            {
                checkinManagerConfiguration.RoomListScheduleIdsFilter = new int[0];
            }

            if ( checkinManagerConfiguration.CustomSettings == null )
            {
                checkinManagerConfiguration.CustomSettings = new Dictionary<string, string>();
            }

            return checkinManagerConfiguration;
        }

        /// <summary>
        /// Filters the by active check-ins.
        /// </summary>
        /// <param name="currentDateTime">The current date time.</param>
        /// <param name="attendanceList">The attendance list.</param>
        /// <returns></returns>
        [RockObsolete( "1.12.4" )]
        [Obsolete( "No longer supported. Use FilterByActiveCheckins RosterAttendeeAttendance instead", error: true )]
        public static List<Attendance> FilterByActiveCheckins( DateTime currentDateTime, List<Attendance> attendanceList )
        {
            return null;
        }

        /// <summary>
        /// If an attendance's GroupType' AllowCheckout is false, remove all Attendees whose schedules are not currently active.
        /// </summary>
        /// <param name="currentDateTime">The current date time.</param>
        /// <param name="attendanceList">The attendance list.</param>
        /// <returns></returns>
        public static List<RosterAttendeeAttendance> FilterByActiveCheckins( DateTime currentDateTime, List<RosterAttendeeAttendance> attendanceList )
        {
            var groupTypeIds = attendanceList.Select( a => a.GroupTypeId ).Distinct().ToList();
            var groupTypes = groupTypeIds.Select( a => GroupTypeCache.Get( a ) );
            var groupTypeIdsWithAllowCheckout = new HashSet<int>( groupTypes
                .Where( gt => gt.GetCheckInConfigurationAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT ).AsBoolean() )
                .Where( a => a != null )
                .Select( a => a.Id )
                .Distinct().ToList() );

            var scheduleList = attendanceList.Select( a => a.Schedule ).Where( a => a != null ).Distinct().ToList();
            var scheduleIdsWasScheduleOrCheckInActiveForCheckOut =
                new HashSet<int>( scheduleList.Where( a => a.WasScheduleOrCheckInActiveForCheckOut( currentDateTime ) ).Select( a => a.Id ).ToList() );

            attendanceList = attendanceList.Where( a =>
            {
                var allowCheckout = groupTypeIdsWithAllowCheckout.Contains( a.GroupTypeId );
                if ( !allowCheckout )
                {
                    /* 
                       If AllowCheckout is false, remove all Attendees whose schedules are not currently active. Per the 'WasSchedule...ActiveForCheckOut()'
                       method below: "Check-out can happen while check-in is active or until the event ends (start time + duration)." This will help to keep
                       the list of 'Present' attendees cleaned up and accurate, based on the room schedules, since the volunteers have no way to manually mark
                       an Attendee as 'Checked-out'.

                       If, on the other hand, AllowCheckout is true, it will be the volunteers' responsibility to click the [Check-out] button when an
                       Attendee leaves the room, in order to keep the list of 'Present' Attendees in order. This will also allow the volunteers to continue
                       'Checking-out' Attendees in the case that the parents are running late in picking them up.
                   */
                    return a.ScheduleId.HasValue && scheduleIdsWasScheduleOrCheckInActiveForCheckOut.Contains( a.ScheduleId.Value );
                }
                else
                {
                    return true;
                }
            } ).ToList();

            return attendanceList;
        }

        /// <summary>
        /// Filters an IQueryable of Attendance by the specified roster status filter.
        /// </summary>
        /// <param name="attendanceQuery">The attendance query.</param>
        /// <param name="rosterStatusFilter">The roster status filter.</param>
        /// <returns></returns>
        public static IQueryable<Attendance> FilterByRosterStatusFilter( IQueryable<Attendance> attendanceQuery, RosterStatusFilter rosterStatusFilter )
        {
            /*
                If StatusFilter == All, no further filtering is needed.
                If StatusFilter == Checked-in, only retrieve records that have neither a EndDateTime nor a PresentDateTime value.
                If StatusFilter == Present, only retrieve records that have a PresentDateTime value but don't have a EndDateTime value.
                If StatusFilter == Checked-out, only retrieve records that have an EndDateTime
            */
            switch ( rosterStatusFilter )
            {
                case RosterStatusFilter.CheckedIn:
                    attendanceQuery = attendanceQuery.Where( a => !a.PresentDateTime.HasValue && !a.EndDateTime.HasValue );
                    break;
                case RosterStatusFilter.Present:
                    attendanceQuery = attendanceQuery.Where( a => a.PresentDateTime.HasValue && !a.EndDateTime.HasValue );
                    break;
                case RosterStatusFilter.CheckedOut:
                    attendanceQuery = attendanceQuery.Where( a => a.EndDateTime.HasValue );
                    break;
                default:
                    break;
            }

            return attendanceQuery;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class CheckInManagerCookieKey
    {
        /// <summary>
        /// The checkin manager roster configuration
        /// </summary>
        public static readonly string CheckinManagerConfiguration = "CheckinManager.CheckinManagerConfiguration";
    }

    /// <summary>
    /// The status filter to be applied to attendees displayed.
    /// </summary>
    public enum RosterStatusFilter
    {
        /// <summary>
        /// Status filter not set to anything yet
        /// </summary>
        [Description( "Unknown" )]
        Unknown = 0,

        /// <summary>
        /// Don't filter
        /// </summary>
        [Description( "All" )]
        All = 1,

        /// <summary>
        /// Only show attendees that are checked-in, but haven't been marked present
        /// </summary>
        [Description( "Checked-in" )]
        CheckedIn = 2,

        /// <summary>
        /// Only show attendees are the marked present.
        /// Note that if Presence is NOT enabled, the attendance records will automatically marked as Present.
        /// So this would be the default filter mode when Presence is not enabled
        /// </summary>
        [Description( "Present" )]
        Present = 3,

        /// <summary>
        /// Only show attendees that are checked-out.
        /// </summary>
        [Description( "Checked-out" )]
        CheckedOut = 4
    }

    /// <summary>
    /// 
    /// </summary>
    public class CheckinManagerConfiguration
    {
        /// <summary>
        /// Gets or sets the location identifier from selected campus identifier.
        /// </summary>
        /// <value>
        /// The location identifier from selected campus identifier.
        /// </value>
        public Dictionary<int, int> LocationIdFromSelectedCampusId { get; set; }

        /// <summary>
        /// Gets or sets the roster status filter.
        /// </summary>
        /// <value>
        /// The roster status filter.
        /// </value>
        public RosterStatusFilter RosterStatusFilter { get; set; }

        /// <summary>
        /// Gets or sets the checkin area unique identifier.
        /// </summary>
        /// <value>
        /// The checkin area unique identifier.
        /// </value>
        public Guid? CheckinAreaGuid { get; set; }

        /// <summary>
        /// Gets or sets the label printer Guid <see cref="Rock.Model.Device"/>
        /// Returns Guid.Empty if 'local printer' is selected
        /// and "" if nothing is selected.
        /// </summary>
        /// <value>
        /// The label printer unique identifier.
        /// </value>
        public Guid? LabelPrinterGuid { get; set; }

        /// <summary>
        /// Gets or sets the room list schedule ids filter.
        /// </summary>
        /// <value>
        /// The room list schedule ids filter.
        /// </value>
        public int[] RoomListScheduleIdsFilter { get; set; }

        /// <summary>
        /// Gets or sets the custom settings that can be used by plugins
        /// </summary>
        /// <value>
        /// The custom settings.
        /// </value>
        public Dictionary<string, string> CustomSettings { get; set; }
    }
}
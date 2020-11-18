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
using System.Web;

namespace Rock.CheckIn
{
    /// <summary>
    /// 
    /// </summary>
    public static class CheckinManagerHelper
    {
        /// <summary>
        /// Saves the campus location configuration to cookie.
        /// </summary>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        public static void SaveCampusLocationConfigurationToCookie( int campusId, int? locationId )
        {
            SaveCheckinManagerConfigurationToCookie( campusId, locationId, null, null );
        }

        /// <summary>
        /// Saves the roster configuration to cookie.
        /// </summary>
        /// <param name="rosterStatusFilter">The roster status filter.</param>
        public static void SaveRosterConfigurationToCookie( RosterStatusFilter rosterStatusFilter )
        {
            SaveCheckinManagerConfigurationToCookie( null, null, rosterStatusFilter, null );
        }

        /// <summary>
        /// Sets the selected checkin area unique identifier to a cookie
        /// </summary>
        /// <returns></returns>
        public static void SetSelectedCheckinAreaGuidToCookie( Guid? checkinAreaGuid )
        {
            SaveCheckinManagerConfigurationToCookie( null, null, null, checkinAreaGuid );
        }

        /// <summary>
        /// Saves the CheckinManager configuration to a cookie.
        /// </summary>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="rosterStatusFilter">The roster status filter.</param>
        /// <param name="checkinAreaGuid">The checkin area unique identifier.</param>
        private static void SaveCheckinManagerConfigurationToCookie( int? campusId, int? locationId, RosterStatusFilter? rosterStatusFilter, Guid? checkinAreaGuid )
        {
            CheckinManagerConfiguration checkinManagerConfiguration = GetCheckinManagerConfigurationFromCookie();
            if ( campusId.HasValue )
            {
                if ( locationId.HasValue )
                {
                    checkinManagerConfiguration.LocationIdFromSelectedCampusId.AddOrReplace( campusId.Value, locationId.Value );
                }
                else
                {
                    checkinManagerConfiguration.LocationIdFromSelectedCampusId.Remove( campusId.Value );
                }
            }

            if ( rosterStatusFilter.HasValue )
            {
                checkinManagerConfiguration.RosterStatusFilter = rosterStatusFilter.Value;
            }

            if ( checkinAreaGuid.HasValue )
            {
                checkinManagerConfiguration.CheckinAreaGuid = checkinAreaGuid;
            }

            var checkinManagerConfigurationJson = checkinManagerConfiguration.ToJson( Newtonsoft.Json.Formatting.None );
            Rock.Web.UI.RockPage.AddOrUpdateCookie( CheckInManagerCookieKey.CheckinManagerConfiguration, checkinManagerConfigurationJson, RockDateTime.Now.AddYears( 1 ) );
        }

        /// <summary>
        /// Gets the roster configuration from cookie.
        /// Always returns a non-null CheckinManagerConfiguration.
        /// </summary>
        /// <returns></returns>
        public static CheckinManagerConfiguration GetCheckinManagerConfigurationFromCookie()
        {
            CheckinManagerConfiguration checkinManagerConfiguration = null;
            var checkinManagerRosterConfigurationCookie = HttpContext.Current.Request.Cookies[CheckInManagerCookieKey.CheckinManagerConfiguration];
            if ( checkinManagerRosterConfigurationCookie != null )
            {
                checkinManagerConfiguration = checkinManagerRosterConfigurationCookie.Value.FromJsonOrNull<CheckinManagerConfiguration>();
            }

            if ( checkinManagerConfiguration == null )
            {
                checkinManagerConfiguration = new CheckinManagerConfiguration();
            }

            if ( checkinManagerConfiguration.LocationIdFromSelectedCampusId == null )
            {
                checkinManagerConfiguration.LocationIdFromSelectedCampusId = new Dictionary<int, int>();
            }

            return checkinManagerConfiguration;
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
        Unknown = 0,

        /// <summary>
        /// Don't filter
        /// </summary>
        All = 1,

        /// <summary>
        /// Only show attendees that are checked-in, but haven't been marked present
        /// </summary>
        CheckedIn = 2,

        /// <summary>
        /// Only show attendees are the marked present.
        /// Note that if Presence is NOT enabled, the attendance records will automatically marked as Present.
        /// So this would be the default filter mode when Presence is not enabled
        /// </summary>
        Present = 3
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
    }
}
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
using System.Web;

using Rock.Web.Cache;

namespace Rock.CheckIn
{
    /// <summary>
    /// Helper functions for Checkin
    /// </summary>
    public static class CheckinConfigurationHelper
    {
        /// <summary>
        /// Determines if the device is "mobile" and if it is no longer valid.
        /// </summary>
        /// <returns>true if the mobile device has expired; false otherwise.</returns>
        public static bool IsMobileAndExpiredDevice( System.Web.HttpRequest request )
        {
            if ( request.Cookies[CheckInCookieKey.IsMobile] != null
                && request.Cookies[CheckInCookieKey.DeviceId] == null )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static string _rockVersion = Rock.VersionInfo.VersionInfo.GetRockProductVersionFullName();

        /// <summary>
        /// Gets the local device configuration status.
        /// </summary>
        /// <param name="localDeviceConfiguration">The local device configuration.</param>
        /// <param name="httpRequest">The HTTP request.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">LocalDeviceConfiguration with a valid KioskId and Checkin Type  is required</exception>
        public static LocalDeviceConfigurationStatus GetLocalDeviceConfigurationStatus( LocalDeviceConfiguration localDeviceConfiguration, HttpRequest httpRequest )
        {
            if ( localDeviceConfiguration?.CurrentKioskId == null || localDeviceConfiguration?.CurrentCheckinTypeId == null )
            {

                throw new ArgumentNullException( "LocalDeviceConfiguration with a valid KioskId and Checkin Type is required" );
            }

            var kiosk = KioskDevice.Get( localDeviceConfiguration.CurrentKioskId.Value, localDeviceConfiguration.CurrentGroupTypeIds );

            DateTime nextActiveDateTime = kiosk.FilteredGroupTypes( localDeviceConfiguration.CurrentGroupTypeIds ).Min( g => ( DateTime? ) g.NextActiveTime ) ?? DateTime.MaxValue;
            nextActiveDateTime = DateTime.SpecifyKind( nextActiveDateTime, DateTimeKind.Unspecified );

            bool isMobileAndExpired = CheckinConfigurationHelper.IsMobileAndExpiredDevice( httpRequest );

            CheckInState checkInState = new CheckInState( localDeviceConfiguration );

            CheckinConfigurationHelper.CheckinStatus checkinStatus = CheckinConfigurationHelper.GetCheckinStatus( checkInState );

            CheckIn.CheckinType checkinType = new Rock.CheckIn.CheckinType( localDeviceConfiguration.CurrentCheckinTypeId.Value );

            var configurationData = new
            {
                CheckinType = checkinType,
                IsMobileAndExpired = isMobileAndExpired,
                CheckinStatus = checkinStatus,
                NextActiveDateTime = nextActiveDateTime,
                RockVersion = _rockVersion
            };

            var configurationString = configurationData.ToJson();

            DateTime campusCurrentDateTime = RockDateTime.Now;
            if ( kiosk.CampusId.HasValue )
            {
                campusCurrentDateTime = CampusCache.Get( kiosk.CampusId.Value )?.CurrentDateTime ?? RockDateTime.Now;
            }

            LocalDeviceConfigurationStatus localDeviceConfigurationStatus = new LocalDeviceConfigurationStatus();

            localDeviceConfigurationStatus.ConfigurationHash = configurationString.XxHash();
            localDeviceConfigurationStatus.ServerCurrentDateTime = RockDateTime.Now;
            localDeviceConfigurationStatus.CampusCurrentDateTime = campusCurrentDateTime;
            localDeviceConfigurationStatus.NextActiveDateTime = nextActiveDateTime;
            return localDeviceConfigurationStatus;
        }

        /// <summary>
        /// Determines whether [is temporarily closed] [the specified kiosk].
        /// </summary>
        /// <param name="kiosk">The kiosk.</param>
        /// <param name="configuredGroupTypeIds">The configured group type ids.</param>
        /// <param name="checkInState">State of the check in.</param>
        /// <returns>
        ///   <c>true</c> if [is temporarily closed] [the specified kiosk]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsTemporarilyClosed( KioskDevice kiosk, List<int> configuredGroupTypeIds, CheckInState checkInState )
        {
            bool isTemporarilyClosed = ( !kiosk.HasLocations( configuredGroupTypeIds ) && !checkInState.AllowCheckout )
                    || ( checkInState.AllowCheckout && !kiosk.HasActiveCheckOutLocations( configuredGroupTypeIds ) );

            return isTemporarilyClosed;
        }

        /// <summary>
        /// Determines whether the specified kiosk is closed.
        /// </summary>
        /// <param name="kiosk">The kiosk.</param>
        /// <param name="configuredGroupTypeIds">The configured group type ids.</param>
        /// <param name="checkInState">State of the check in.</param>
        /// <returns>
        ///   <c>true</c> if the specified kiosk is closed; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsClosed( KioskDevice kiosk, List<int> configuredGroupTypeIds, CheckInState checkInState )
        {
            // Closed if there are no active locations and check-out is not allowed, or if check -out is allowed but there
            // are no active check-out locations.
            bool isClosed = ( !kiosk.HasActiveLocations( configuredGroupTypeIds ) && !checkInState.AllowCheckout )
                    || ( checkInState.AllowCheckout && !kiosk.HasActiveCheckOutLocations( configuredGroupTypeIds ) );

            return isClosed;
        }

        /// <summary>
        /// Gets the checkin status.
        /// </summary>
        /// <param name="checkInState">State of the check in.</param>
        /// <returns></returns>
        public static CheckinStatus GetCheckinStatus( CheckInState checkInState )
        {
            KioskDevice kiosk = checkInState.Kiosk;
            List<int> configuredGroupTypeIds = checkInState.ConfiguredGroupTypes;

            bool hasGroupTypes = kiosk.FilteredGroupTypes( configuredGroupTypeIds ).Any();

            if ( !hasGroupTypes )
            {
                return CheckinStatus.Inactive;
            }

            if ( IsTemporarilyClosed( kiosk, configuredGroupTypeIds, checkInState ) )
            {
                return CheckinStatus.TemporarilyClosed;
            }
            else if ( IsClosed( kiosk, configuredGroupTypeIds, checkInState ) )
            {
                return CheckinStatus.Closed;
            }
            else
            {
                return CheckinStatus.Active;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public enum CheckinStatus
        {
            /// <summary>
            /// Checkin is Active
            /// </summary>
            Active,

            /// <summary>
            /// Checkin is Inactive
            /// </summary>
            Inactive,

            /// <summary>
            /// Checkin is temporarily closed
            /// </summary>
            TemporarilyClosed,

            /// <summary>
            /// Checkin is closed
            /// </summary>
            Closed,
        }

        /// <summary>
        /// If a group has more than one available location how should one be chosen. Choosing an option other than "Ask" will auto select a location for the user.
        /// </summary>
        public enum LocationSelectionStrategy
        {
            /// <summary>
            /// The Ask strategy will present a list of rooms to the user so they can select one. This is the default behavior.
            /// </summary>
            Ask = 0,

            /// <summary>
            /// The balance strategy will attempt to fill all locations with an equal number of persons up to the soft threshold
            /// </summary>
            Balance = 1,

            /// <summary>
            /// The Fill In Order strategy will fill in the locations in a group in their sort order. When the location's soft threshold is reached the next one is used untill it fills up.
            /// </summary>
            FillInOrder = 2
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class CheckInCookieKey
    {
        /// <summary>
        /// The local device configuration
        /// </summary>
        public static readonly string LocalDeviceConfig = "Checkin.LocalDeviceConfig";

        /// <summary>
        /// The name of the cookie that holds the DeviceId. Setters of this cookie should
        /// be sure to set the expiration to a time when the device is no longer valid.
        /// </summary>
        public static readonly string DeviceId = "Checkin.DeviceId";

        /// <summary>
        /// The name of the cookie that holds whether or not the device was a mobile device.
        /// </summary>
        public static readonly string IsMobile = "Checkin.IsMobile";

        /// <summary>
        /// The phone number used to check in could be in this cookie.
        /// </summary>
        public static readonly string PhoneNumber = "Checkin.PhoneNumber";

        /// <summary>
        /// Cookie to store whether the user has previously allowed location access
        /// </summary>
        public static readonly string RockHasLocationApproval = "Checkin.RockHasLocationApproval";

        /// <summary>
        /// A cookie that stores the AttendanceSession Guid(s) that have been used by the local device
        /// This cookie should expire 8 hours after last use
        /// </summary>
        public static readonly string AttendanceSessionGuids = "Checkin.AttendanceSessionGuids";
    }
}

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

namespace Rock.CheckIn
{
    /// <summary>
    /// Checkin Device Configuration
    /// Used for the Checkin Cookie and REST status operations
    /// </summary>
    [System.Diagnostics.DebuggerDisplay( "CurrentTheme:{CurrentTheme}, CurrentKioskId:{CurrentKioskId}, CurrentCheckinTypeId:{CurrentCheckinTypeId}, CurrentGroupTypeIds:{CurrentGroupTypeIds}.." )]
    public class LocalDeviceConfiguration
    {
        /// <summary>
        /// Gets or sets the current theme.
        /// </summary>
        /// <value>
        /// The current theme.
        /// </value>
        public string CurrentTheme { get; set; }

        /// <summary>
        /// Gets or sets the current kiosk identifier <see cref="Rock.Model.Device"/>
        /// </summary>
        /// <value>
        /// The current kiosk identifier.
        /// </value>
        public int? CurrentKioskId { get; set; }

        /// <summary>
        /// Gets or sets the current checkin type identifier (which is a <see cref="Rock.Model.GroupType" />)
        /// </summary>
        /// <value>
        /// The current checkin type identifier.
        /// </value>
        public int? CurrentCheckinTypeId { get; set; }

        /// <summary>
        /// Gets or sets the current group type ids (Checkin Areas)
        /// </summary>
        /// <value>
        /// The current group type ids.
        /// </value>
        public List<int> CurrentGroupTypeIds { get; set; }

        /// <summary>
        /// Gets home page Guid to use instead of the one configured in <seealso cref="CheckInBlock"/>'s HomePage block setting.
        /// This is handy for things such as a checkin that start with the MobileLauncher Page
        /// </summary>
        /// <value>
        /// </value>
        public Guid? HomePageOverride { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether checkin pages should disable IdleRedirect blocks
        /// </summary>
        /// <value>
        ///   <c>true</c> if [disable idle redirect]; otherwise, <c>false</c>.
        /// </value>
        public bool DisableIdleRedirect { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether [generate qr code for attendance sessions].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [generate qr code for attendance sessions]; otherwise, <c>false</c>.
        /// </value>
        public bool GenerateQRCodeForAttendanceSessions { get; set; } = false;

        /// <summary>
        /// Gets or sets pages that are not allowed when navigating.
        /// Navigation to block pages will send user back to the HomePage.
        /// </summary>
        /// <value>
        /// The blocked page ids.
        /// </value>
        public int[] BlockedPageIds { get; set; } = null;

        /// <summary>
        /// Set this to override the <seealso cref="CheckinType.AllowCheckout"/> setting  
        /// </summary>
        /// <value>
        /// The allow checkout.
        /// </value>
        public bool? AllowCheckout { get; set; } = null;

        /// <summary>
        /// Determines whether this instance is configured.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is configured; otherwise, <c>false</c>.
        /// </returns>
        public bool IsConfigured()
        {
            return this.CurrentKioskId.HasValue && this.CurrentGroupTypeIds.Any() && this.CurrentCheckinTypeId.HasValue;
        }

        /// <summary>
        /// Clears the following overrides or configurations back to their default:
        /// <see cref="HomePageOverride" >HomePageOverride</see>,
        /// <see cref="BlockedPageIds" >BlockedPageIds</see>,
        /// <see cref="AllowCheckout" >AllowCheckout</see>,
        /// <see cref="DisableIdleRedirect" >DisableIdleRedirect</see>,
        /// <see cref="GenerateQRCodeForAttendanceSessions" >GenerateQRCodeForAttendanceSessions</see>,
        /// </summary>
        public void ClearOverrides()
        {
            this.HomePageOverride = null;
            this.BlockedPageIds = null;
            this.AllowCheckout = null;
            this.DisableIdleRedirect = false;
            this.GenerateQRCodeForAttendanceSessions = false;
        }

        /// <summary>
        /// Saves the LocalDeviceConfig to the <seealso cref="CheckInCookieKey.LocalDeviceConfig"/> cookie
        /// </summary>
        /// <param name="page">The page.</param>
        public void SaveToCookie( System.Web.UI.Page page )
        {
            var localDeviceConfigCookie = page.Request.Cookies[CheckInCookieKey.LocalDeviceConfig];
            if ( localDeviceConfigCookie == null )
            {
                localDeviceConfigCookie = new System.Web.HttpCookie( CheckInCookieKey.LocalDeviceConfig );
            }

            localDeviceConfigCookie.Expires = RockDateTime.Now.AddYears( 1 );
            localDeviceConfigCookie.Value = this.ToJson( Newtonsoft.Json.Formatting.None );

            page.Response.Cookies.Set( localDeviceConfigCookie );
        }

        /// <summary>
        /// Gets from cookie.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public LocalDeviceConfiguration GetFromCookie( System.Web.UI.Page page )
        {
            var localDeviceConfigCookie = page.Request.Cookies[CheckInCookieKey.LocalDeviceConfig];
            return localDeviceConfigCookie?.Value?.FromJsonOrNull<LocalDeviceConfiguration>();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class LocalDeviceConfigurationStatus
    {
        /// <summary>
        /// Gets or sets the configuration hash.
        /// </summary>
        /// <value>
        /// The configuration hash.
        /// </value>
        public string ConfigurationHash { get; set; }

        /// <summary>
        /// Gets or sets the next active date time.
        /// </summary>
        /// <value>
        /// The next active date time.
        /// </value>
        public DateTime NextActiveDateTime { get; set; }

        /// <summary>
        /// Gets the campus date time.
        /// </summary>
        /// <value>
        /// The campus date time.
        /// </value>
        public DateTime CampusDateTime { get; set; }

        /// <summary>
        /// Gets the server current date time.
        /// </summary>
        /// <value>
        /// The server current date time.
        /// </value>
        public DateTime ServerCurrentDateTime { get; set; }

        /// <summary>
        /// Gets the campus current date time.
        /// </summary>
        /// <value>
        /// The campus current date time.
        /// </value>
        public DateTime CampusCurrentDateTime { get; set; }
    }
}
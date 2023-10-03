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
using Rock.Security;
using Rock.Web.UI;

namespace Rock.CheckIn
{
    /// <summary>
    /// Checkin Device Configuration
    /// Used for the Checkin Cookie and REST status operations
    /// </summary>
    [System.Diagnostics.DebuggerDisplay( "CurrentTheme:{CurrentTheme}, CurrentKioskId:{CurrentKioskId}, CurrentCheckinTypeId:{CurrentCheckinTypeId}, CurrentGroupTypeIds:{CurrentGroupTypeIds}.." )]
    public class LocalDeviceConfiguration
    {
        /* 08-05-2021 MDP

        If you make changes to this class, keep in mind that the maximum cookie size is 4096 bytes.
        As of 08-05-2021 the size is less than 300 bytes.
         
         */

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
        /// Gets or sets the current 'Check-in Configuration' Id (which is a <see cref="Rock.Model.GroupType" /> Id).
        /// For example "Weekly Service Check-in".
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
        /// Gets or sets the current group ids.
        /// </summary>
        /// <value>
        /// The current group ids.
        /// </value>
        public List<int> CurrentGroupIds { get; set; }

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
        /// Saves the LocalDeviceConfig to the cookie.
        /// </summary>
        /// <param name="page">The page.</param>
        [Obsolete( "Use SaveToCookie( ) instead." )]
        [RockObsolete( "1.12" )]
        public void SaveToCookie( System.Web.UI.Page page )
        {
            SaveToCookie();
        }

        /// <summary>
        /// Saves to cookie.
        /// We are now encrypting this cookie see Asana: REF# 20210224-MSB1 for details.
        /// </summary>
        public void SaveToCookie()
        {
            var localDeviceConfigValue = this.ToJson( indentOutput: false );
            var encryptedValue = Encryption.EncryptString( localDeviceConfigValue );

            RockPage.AddOrUpdateCookie( CheckInCookieKey.LocalDeviceConfig, encryptedValue, RockDateTime.Now.AddYears( 1 ) );
        }

        /// <summary>
        /// Gets from cookie.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public LocalDeviceConfiguration GetFromCookie( System.Web.UI.Page page)
        {
            return GetFromCookie( page, false );
        }

        /// <summary>
        /// Gets from cookie.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="loadUnencryptedCookie">if set to <c>true</c> [load unencrypted cookie].</param>
        /// <returns></returns>
        public LocalDeviceConfiguration GetFromCookie( System.Web.UI.Page page, bool loadUnencryptedCookie )
        {
            /*
                02.24.2021 MSB
                Asana: REF# 20210224-MSB1
                As of v13 the cookie is now encrypted, but for Admin.ascx we want to be able to handle the old
                unencrypted cookie. So we look for it and encrypt it if it exists.

                Reason: Backwards Compatibility
            */

            var localDeviceConfigCookie = page.Request.Cookies[CheckInCookieKey.LocalDeviceConfig]?.Value ?? string.Empty;
            if ( loadUnencryptedCookie && localDeviceConfigCookie.IsNotNullOrWhiteSpace() && localDeviceConfigCookie.Contains( "CurrentKioskId" ) )
            {
                return localDeviceConfigCookie.FromJsonOrNull<LocalDeviceConfiguration>();
            }

            var decryptedValue = Encryption.DecryptString( localDeviceConfigCookie );

            /*
                7/11/2023 JME
                Some proxies like Weglot will URL encode the cookie value when they proxy the request to add the cookie. This breaks
                the format of the encrypted string that contains the check-in configuration. If the decrypted  value is blank we'll
                try decoding it then decrypting it.
            */
            if ( decryptedValue.IsNullOrWhiteSpace() )
            {
                decryptedValue = Encryption.DecryptString( HttpUtility.UrlDecode( localDeviceConfigCookie ) );
            }

            return decryptedValue.FromJsonOrNull<LocalDeviceConfiguration>();
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
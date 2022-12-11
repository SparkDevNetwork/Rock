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
using System.ComponentModel;
using System.Linq;
using System.Web;

using Rock.Model;
using Rock.Net;

namespace Rock.Personalization
{
    /// <summary>
    /// Class DeviceTypeRequestFilter.
    /// Implements the <see cref="Rock.Personalization.PersonalizationRequestFilter" />
    /// </summary>
    /// <seealso cref="Rock.Personalization.PersonalizationRequestFilter" />
    public class DeviceTypeRequestFilter : PersonalizationRequestFilter
    {
        #region Configuration

        /// <summary>
        /// Gets or sets the device types.
        /// </summary>
        /// <value>The device types.</value>
        public DeviceType[] DeviceTypes { get; set; } = new DeviceType[0];

        private string[] DeviceTypeStrings => DeviceTypes?.Select( x => x.ToString() ).ToArray();

        #endregion Configuration        

        /// <inheritdoc/>
        public override bool IsMatch( HttpRequest httpRequest )
        {
            if ( !DeviceTypes.Any() )
            {
                // If there is no DeviceType criteria, return true;
                return true;
            }

            var clientType = InteractionDeviceType.GetClientType( httpRequest.UserAgent );

            return DeviceTypeStrings.Contains( clientType, StringComparer.OrdinalIgnoreCase );
        }

        /// <inheritdoc/>
        internal override bool IsMatch( RockRequestContext request )
        {
            if ( !DeviceTypes.Any() )
            {
                // If there is no DeviceType criteria, return true;
                return true;
            }

            var clientType = InteractionDeviceType.GetClientType( request.ClientInformation.UserAgent );

            return DeviceTypeStrings.Contains( clientType, StringComparer.OrdinalIgnoreCase );
        }

        /// <summary>
        /// Enum DeviceType
        /// </summary>
        public enum DeviceType
        {
            /// <summary>
            /// The desktop
            /// </summary>
            [Description( "Desktop" )]
            Desktop = 0,

            /// <summary>
            /// The tablet
            /// </summary>
            [Description( "Tablet" )]
            Tablet = 1,

            /// <summary>
            /// The mobile
            /// </summary>
            [Description( "Mobile" )]
            Mobile = 2
        }
    }
}

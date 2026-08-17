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

using Rock.Net;

namespace Rock.Model
{
    /// <summary>
    /// InteractionDeviceType Logic
    /// </summary>
    public partial class InteractionDeviceType
    {
        #region Public Methods

        /// <summary>
        /// Determines the ClientType (Mobile, Desktop, Tablet, etc) from a UserAgent string
        /// </summary>
        /// <param name="userAgent">The user agent.</param>
        /// <returns></returns>
        [Obsolete( "Use RockApp.Current.GetRequiredService<IUserAgentParser>().Parse(userAgent).ClientType instead." )]
        [RockObsolete( "20.0" )]
        public static string GetClientType( string userAgent )
        {
            return UserAgentInfo.DetermineClientType( userAgent );
        }

        #endregion
    }
}

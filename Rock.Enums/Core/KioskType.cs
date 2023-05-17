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

using System.ComponentModel;

namespace Rock.Model
{
    /// <summary>
    /// The various types of checkin clients that a Check-in Kiosk could be using.
    /// </summary>
    [Enums.EnumDomain( "Core" )]
    public enum KioskType
    {
        /// <summary>
        /// The Kiosk is using IPad iOS Checkin Client app.
        /// </summary>
        [Description( "iPad" )]
        IPad = 0,

        /// <summary>
        /// The Kiosk is using Windows Checkin Client.
        /// </summary>
        [Description( "Windows App" )]
        WindowsApp = 1,

        /// <summary>
        /// This kiosk is using a browser
        /// </summary>
        [Description( "Browser" )]
        Browser = 2,
    }
}

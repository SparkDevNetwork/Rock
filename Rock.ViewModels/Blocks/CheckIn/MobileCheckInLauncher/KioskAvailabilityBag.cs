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

using Rock.ViewModels.Blocks.CheckIn.CheckInKiosk;

namespace Rock.ViewModels.Blocks.CheckIn.MobileCheckInLauncher
{
    /// <summary>
    /// Whether check-in can start at the resolved kiosk right now, along with the message shown either way.
    /// </summary>
    public class KioskAvailabilityBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether check-in can start at the kiosk right now. When <c>false</c> the
        /// individual is offered a retry instead of the check-in flow.
        /// </summary>
        public bool IsCheckInAvailable { get; set; }

        /// <summary>
        /// Gets or sets the rendered message shown alongside the availability, welcoming the individual back when
        /// check-in is open and explaining that no services are ready when it is not.
        /// </summary>
        public string MessageHtml { get; set; }

        /// <summary>
        /// Gets or sets the kiosk, template and areas the check-in flow runs against. Only populated when
        /// <see cref="IsCheckInAvailable"/> is <c>true</c>.
        /// </summary>
        public KioskConfigurationBag Configuration { get; set; }
    }
}

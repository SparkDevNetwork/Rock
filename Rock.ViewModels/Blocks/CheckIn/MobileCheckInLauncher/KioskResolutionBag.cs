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

using Rock.ViewModels.CheckIn;

namespace Rock.ViewModels.Blocks.CheckIn.MobileCheckInLauncher
{
    /// <summary>
    /// The outcome of resolving the kiosk the individual will check in at, either from their location or from
    /// the campus they picked.
    /// </summary>
    public class KioskResolutionBag
    {
        /// <summary>
        /// Gets or sets the kiosk that was matched. When <c>null</c> no kiosk was matched and
        /// <see cref="MessageHtml"/> explains why.
        /// </summary>
        public CheckInItemBag Kiosk { get; set; }

        /// <summary>
        /// Gets or sets the rendered message explaining why no kiosk was matched.
        /// </summary>
        public string MessageHtml { get; set; }

        /// <summary>
        /// Gets or sets whether check-in can start at <see cref="Kiosk"/> right now. <c>null</c> when no kiosk was
        /// matched.
        /// </summary>
        public KioskAvailabilityBag Availability { get; set; }
    }
}

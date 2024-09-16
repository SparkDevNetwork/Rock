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

namespace Rock.ViewModels.Rest.CheckIn
{
    /// <summary>
    /// The response to the request to get the check-in kiosk status.
    /// </summary>
    public class KioskStatusResponseBag
    {
        /// <summary>
        /// Gets or sets the current status of the kiosk.
        /// </summary>
        /// <value>The current status of the kiosk.</value>
        public KioskStatusBag Status { get; set; }
    }
}

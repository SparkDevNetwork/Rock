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
using System.Collections.Generic;

namespace Rock.ViewModels.Rest.CheckIn
{
    /// <summary>
    /// The request parameters to use when requesting the current status of a kiosk.
    /// </summary>
    public class KioskStatusOptionsBag
    {
        /// <summary>
        /// Gets or sets the kiosk identifier.
        /// </summary>
        /// <value>The kiosk identifier.</value>
        public string KioskId { get; set; }

        /// <summary>
        /// Gets or sets the area identifiers the kiosk is currently
        /// configured for.
        /// </summary>
        /// <value>The area unique identifiers.</value>
        public List<string> AreaIds { get; set; }
    }
}

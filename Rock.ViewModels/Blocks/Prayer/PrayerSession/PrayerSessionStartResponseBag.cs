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

namespace Rock.ViewModels.Blocks.Prayer.PrayerSession
{
    /// <summary>
    /// The response returned when a prayer session is started.
    /// </summary>
    public class PrayerSessionStartResponseBag
    {
        /// <summary>
        /// Gets or sets the ordered identifiers of the prayer requests that make up
        /// the session. The order reflects urgency first and then least-prayed-for.
        /// </summary>
        public List<string> PrayerRequestKeys { get; set; }
    }
}

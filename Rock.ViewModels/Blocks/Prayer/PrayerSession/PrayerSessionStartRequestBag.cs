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
    /// The request sent to start a new prayer session.
    /// </summary>
    public class PrayerSessionStartRequestBag
    {
        /// <summary>
        /// Gets or sets the selected category values to include in the session.
        /// </summary>
        public List<string> CategoryValues { get; set; }

        /// <summary>
        /// Gets or sets the selected campus value used to filter the session, if any.
        /// </summary>
        public string CampusValue { get; set; }
    }
}

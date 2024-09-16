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

using Rock.ViewModels.CheckIn;

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// A representation of a single location that can be used for check-in.
    /// </summary>
    internal class LocationOpportunity : LocationOpportunityBag
    {
        /// <summary>
        /// Gets or sets the person identifiers that are checked into
        /// this location.
        /// </summary>
        /// <value>The current person identifiers.</value>
        public HashSet<string> CurrentPersonIds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this location is closed.
        /// </summary>
        /// <value><c>true</c> if this location is closed; otherwise, <c>false</c>.</value>
        public bool IsClosed { get; set; }
    }
}

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

namespace Rock.ViewModels.Blocks.Crm.BulkUpdate
{
    /// <summary>
    /// The per-person detail for a person who was processed but had one or more requested
    /// actions that could not be applied.
    /// </summary>
    public class BulkUpdatePersonResultBag
    {
        /// <summary>
        /// Gets or sets the person identifier, used as a stable key for the result list.
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the person's IdKey, used to build the link to the person record.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the person's full name for display.
        /// </summary>
        public string PersonName { get; set; }

        /// <summary>
        /// Gets or sets the human-readable reasons each requested action could not be
        /// applied (e.g. "Not able to complete Baptism as there are unmet prerequisites.").
        /// </summary>
        public List<string> Issues { get; set; } = new List<string>();
    }
}

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

namespace Rock.ViewModels.CheckIn
{
    /// <summary>
    /// Identifies a single attendance record that is being requested to be
    /// created with the specified options.
    /// </summary>
    public class AttendanceRequestBag
    {
        /// <summary>
        /// Gets or sets the identifier of the person to create an attendance
        /// record for.
        /// </summary>
        /// <value>The person identifier.</value>
        public string PersonId { get; set; }

        /// <summary>
        /// Gets or sets the selection that identify where the person should
        /// be checked in to. Only the Id values are used.
        /// </summary>
        /// <value>The selections.</value>
        public OpportunitySelectionBag Selection { get; set; }

        /// <summary>
        /// Gets or sets the note to put on the attendance record. If an existing
        /// attendance record is updated then this will replace the note value.
        /// </summary>
        /// <value>The note text.</value>
        public string Note { get; set; }
    }
}

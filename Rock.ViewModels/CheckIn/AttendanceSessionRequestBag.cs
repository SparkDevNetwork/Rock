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

using System;

using Rock.Enums.CheckIn;

namespace Rock.ViewModels.CheckIn
{
    /// <summary>
    /// Identifies information about an attendance session that is being
    /// requested to be created.
    /// </summary>
    public class AttendanceSessionRequestBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the check-in session. This is
        /// a unique identifier instead of a regular identifier since it needs
        /// to come from the client.
        /// </summary>
        /// <value>The unique identifier of the check-in session.</value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this session should create
        /// pending attendance records that will later be made permanent all
        /// at once.
        /// </summary>
        /// <value><c>true</c> if this session is pending; otherwise, <c>false</c>.</value>
        public bool IsPending { get; set; }

        /// <summary>
        /// Gets or sets the family identifier that was determined during
        /// the family search operation.
        /// </summary>
        /// <value>The family identifier.</value>
        public string FamilyId { get; set; }

        /// <summary>
        /// Gets or sets the mode used when searching for families.
        /// </summary>
        /// <value>The search mode.</value>
        public FamilySearchMode SearchMode { get; set; }

        /// <summary>
        /// Gets or sets the term used when searching for families.
        /// </summary>
        /// <value>The search term.</value>
        public string SearchTerm { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the person performing the action
        /// for this session.
        /// </summary>
        /// <value>The identifier of the person performing the action.</value>
        public string PerformedByPersonId { get; set; }
    }
}

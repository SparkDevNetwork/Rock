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

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// Identifies information about an attendance session that is being
    /// requested to be created.
    /// </summary>
    internal class AttendanceSessionRequest : AttendanceSessionRequestBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether this session should enforce
        /// location capacity limits on the locations.
        /// </summary>
        /// <value><c>true</c> if this instance should enforce location capacity; otherwise, <c>false</c>.</value>
        public bool IsCapacityThresholdEnforced { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this session is for an
        /// administrative override.
        /// </summary>
        /// <value><c>true</c> if this session is for an override; otherwise, <c>false</c>.</value>
        public bool IsOverride { get; set; }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AttendanceSessionRequest"/> class.
        /// </summary>
        public AttendanceSessionRequest()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttendanceSessionRequest"/> class.
        /// </summary>
        /// <param name="requestBag">The request bag to copy values from.</param>
        public AttendanceSessionRequest( AttendanceSessionRequestBag requestBag )
        {
            Guid = requestBag.Guid;
            IsPending = requestBag.IsPending;
            FamilyId = requestBag.FamilyId;
            SearchMode = requestBag.SearchMode;
            SearchTerm = requestBag.SearchTerm;
            PerformedByPersonId = requestBag.PerformedByPersonId;
        }

        #endregion
    }
}

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

namespace Rock.ViewModels.Blocks.CheckIn.RapidAttendanceEntry
{
    /// <summary>
    /// A person listed on the entry screen: a family member, or a guest with a "Can check-in" relationship to one.
    /// </summary>
    public class RapidAttendanceEntryPersonBag
    {
        /// <summary>
        /// Gets or sets the person's unique identifier.
        /// </summary>
        public Guid PersonGuid { get; set; }

        /// <summary>
        /// Gets or sets the person's nick name.
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the person's full name.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the URL of the person's photo, shown as the navigation tab avatar.
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the person's age in years, or null when unknown.
        /// </summary>
        public int? Age { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person is below the Minimum Attendance Age setting and so
        /// cannot be marked as attended.
        /// </summary>
        public bool IsBelowMinimumAge { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person is marked as attended for the session's occurrence.
        /// </summary>
        public bool DidAttend { get; set; }
    }
}

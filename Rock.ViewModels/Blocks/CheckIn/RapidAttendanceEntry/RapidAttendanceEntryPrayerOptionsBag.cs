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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.RapidAttendanceEntry
{
    /// <summary>
    /// The configuration for the prayer request section of an individual's entry panel.
    /// </summary>
    public class RapidAttendanceEntryPrayerOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the Urgent checkbox is shown on the prayer request form.
        /// </summary>
        public bool IsUrgentFlagShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Public checkbox is shown on the prayer request form.
        /// </summary>
        public bool IsPublicFlagShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Public checkbox starts checked for each new prayer request.
        /// </summary>
        public bool IsPublicByDefault { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the category picker is shown on the prayer request form. When
        /// hidden, the default category is applied on save.
        /// </summary>
        public bool IsCategoryPickerShown { get; set; }

        /// <summary>
        /// Gets or sets the category preselected in the category picker. Null when no default category is
        /// configured.
        /// </summary>
        public ListItemBag DefaultCategory { get; set; }
    }
}

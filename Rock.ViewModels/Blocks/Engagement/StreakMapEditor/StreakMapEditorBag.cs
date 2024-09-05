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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.StreakMapEditor
{
    /// <summary>
    /// Represents a container for streak map editor related properties and settings.
    /// </summary>
    public class StreakMapEditorBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the Enrollment Exclusion Map should be shown.
        /// </summary>
        /// <value><c>true</c> if Enrollment Exclusion Map should be shown; otherwise, <c>false</c>.</value>
        public bool IsEngagementExclusion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the streak type is daily.
        /// </summary>
        /// <value><c>true</c> if the streak type is daily; otherwise, <c>false</c>.</value>
        public bool IsStreakTypeDaily { get; set; }

        /// <summary>
        /// Gets or sets a string of delimited date values.
        /// </summary>
        /// <value>A string containing date values separated by a delimiter.</value>
        public string DelimitedDateValues { get; set; }

        /// <summary>
        /// Gets or sets the title of the map.
        /// </summary>
        /// <value>The title displayed on the map.</value>
        public string MapTitle { get; set; }

        /// <summary>
        /// Gets or sets the label for the checkbox.
        /// </summary>
        /// <value>The text label associated with the checkbox.</value>
        public string CheckboxLabel { get; set; }

        /// <summary>
        /// Gets or sets the list of checkbox items.
        /// </summary>
        /// <value>A list of ListItemBag objects representing checkbox items.</value>
        public List<ListItemBag> CheckboxItems { get; set; }

        /// <summary>
        /// Gets or sets the list of selected dates.
        /// </summary>
        /// <value>A list of strings representing the selected dates.</value>
        public List<string> SelectedDates { get; set; }

        /// <summary>
        /// Gets or sets the success message.
        /// </summary>
        /// <value>A message to be displayed upon successful operations.</value>
        public string SuccessMessage { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>A message to be displayed when an error occurs.</value>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the panel is hidden.
        /// </summary>
        /// <value><c>true</c> if the panel is hidden; otherwise, <c>false</c>.</value>
        public bool IsPanelHidden { get; set; }
    }
}

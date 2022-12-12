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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Controls
{
    /// <summary>
    /// An item bag used by the Campus Picker to gather additional details about
    /// a campus.
    /// </summary>
    public class CampusPickerItemBag : ListItemBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether this campus is active.
        /// </summary>
        /// <value><c>true</c> if this campus is active; otherwise, <c>false</c>.</value>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the type of campus.
        /// </summary>
        /// <value>The type of campus.</value>
        public Guid? CampusType { get; set; }

        /// <summary>
        /// Gets or sets the campus status.
        /// </summary>
        /// <value>The campus status.</value>
        public Guid? CampusStatus { get; set; }
    }
}

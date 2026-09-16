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

namespace Rock.ViewModels.Blocks.Event.CalendarTypes
{
    /// <summary>
    /// The initial state for the Calendar Types block.
    /// </summary>
    public class CalendarTypesBag
    {
        /// <summary>
        /// Gets or sets the event calendar tiles the current person is authorized to view.
        /// </summary>
        public List<CalendarTypesCalendarBag> Calendars { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current person has block-level
        /// Administrate rights, which controls whether the add-calendar button is shown.
        /// </summary>
        public bool CanAdministrate { get; set; }
    }
}

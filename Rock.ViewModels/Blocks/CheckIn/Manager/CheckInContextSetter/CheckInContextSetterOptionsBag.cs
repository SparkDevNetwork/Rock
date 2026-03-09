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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.Manager.CheckInContextSetter
{
    /// <summary>
    /// The initialization options for the Check-in Context Setter block.
    /// </summary>
    public class CheckInContextSetterOptionsBag
    {
        /// <summary>
        /// The list of campuses to display in the dropdown.
        /// </summary>
        public List<ListItemBag> Campuses { get; set; }

        /// <summary>
        /// The list of schedules that are valid for the currently selected
        /// location.
        /// </summary>
        public List<ListItemBag> Schedules { get; set; } 

        /// <summary>
        /// The root locations to use for the location picker.
        /// </summary>
        public Dictionary<Guid, Guid> RootLocations { get; set; }

        /// <summary>
        /// The selected campus when the block loaded.
        /// </summary>
        public ListItemBag SelectedCampus { get; set; }

        /// <summary>
        /// The selected location when the block loaded.
        /// </summary>
        public ListItemBag SelectedLocation { get; set; }

        /// <summary>
        /// The selected schedule when the block loaded.
        /// </summary>
        public ListItemBag SelectedSchedule { get; set; }
    }
}

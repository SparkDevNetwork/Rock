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

using Rock.Enums.Blocks.Group.Scheduling;

using System;

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduleToolbox
{
    /// <summary>
    /// A bag that contains information about an action to be performed for a current attendance
    /// or person schedule exclusion for the group schedule toolbox block.
    /// </summary>
    public class PerformScheduleRowActionRequestBag
    {
        /// <summary>
        /// Gets or sets the selected person unique identifier.
        /// </summary>
        public Guid SelectedPersonGuid { get; set; }

        /// <summary>
        /// Gets or sets the attendance or person schedule exclusion unique identifier.
        /// <para>
        /// If action type is "Delete," this value represents a person schedule exclusion entity.
        /// Otherwise, this value represents an attendance entity.
        /// </para>
        /// </summary>
        public Guid EntityGuid { get; set; }

        /// <summary>
        /// Gets or sets the row action type to be performed.
        /// </summary>
        public ToolboxScheduleRowActionType ActionType { get; set; }
    }
}

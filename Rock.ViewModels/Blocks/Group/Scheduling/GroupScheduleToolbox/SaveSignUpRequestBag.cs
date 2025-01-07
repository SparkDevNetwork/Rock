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

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduleToolbox
{
    /// <summary>
    /// A bag that contains information about an additional time sign-up to be saved or deleted for the group schedule toolbox block.
    /// </summary>
    public class SaveSignUpRequestBag
    {
        /// <summary>
        /// Gets or sets the selected person unique identifier.
        /// </summary>
        public Guid SelectedPersonGuid { get; set; }

        /// <summary>
        /// Gets or sets the selected group unique identifier.
        /// </summary>
        public Guid SelectedGroupGuid { get; set; }

        /// <summary>
        /// Gets or sets the selected schedule unique identifier.
        /// </summary>
        public Guid SelectedScheduleGuid { get; set; }

        /// <summary>
        /// Gets or sets the selected location unique identifier.
        /// </summary>
        public Guid SelectedLocationGuid { get; set; }

        /// <summary>
        /// Gets or sets the occurrence date time.
        /// </summary>
        public DateTimeOffset OccurrenceDateTime { get; set; }
    }
}

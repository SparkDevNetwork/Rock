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

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduleToolbox
{
    /// <summary>
    /// A bag that contains information about the outcome of "save assignment" request for the group schedule toolbox block.
    /// </summary>
    public class SaveAssignmentResponseBag
    {
        /// <summary>
        /// Gets or sets a friendly error message to describe any problems encountered while saving.
        /// </summary>
        public string SaveError { get; set; }

        /// <summary>
        /// Gets or sets the current assignment options.
        /// </summary>
        public AssignmentOptionsBag AssignmentOptions { get; set; }
    }
}

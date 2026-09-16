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

namespace Rock.ViewModels.Blocks.Engagement.PersonProgramStepList
{
    /// <summary>
    /// One existing step shown in the hover table of a step type card.
    /// </summary>
    public class PersonProgramStepBag
    {
        /// <summary>
        /// Gets or sets the step identifier, used to build the Step Entry URL.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the step identifier key.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the step status name.
        /// </summary>
        public string StatusName { get; set; }

        /// <summary>
        /// Gets or sets the campus name, or null when the step has no campus.
        /// </summary>
        public string CampusName { get; set; }

        /// <summary>
        /// Gets or sets the start date in ISO 8601 format, or null.
        /// </summary>
        public string StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the completed date in ISO 8601 format, or null.
        /// </summary>
        public string CompletedDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current person can edit this step.
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current person can delete this step.
        /// </summary>
        public bool CanDelete { get; set; }
    }
}

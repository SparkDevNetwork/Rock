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

namespace Rock.ViewModels.Blocks.Engagement.PersonProgramStepList
{
    /// <summary>
    /// One step type of the program as shown on a card and as an add button.
    /// </summary>
    public class PersonProgramStepTypeBag
    {
        /// <summary>
        /// Gets or sets the step type identifier, used to build the Step Entry URL.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the step type identifier key.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the step type name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the step type icon CSS class.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the step type highlight color used for the card icon.
        /// </summary>
        public string HighlightColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person can add a step
        /// of this type. Computed on the server from security, manual editing,
        /// prerequisites, and the allow multiple rule.
        /// </summary>
        public bool IsAddEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person has completed
        /// every prerequisite step type.
        /// </summary>
        public bool HasMetPrerequisites { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person has a completed
        /// step of this type.
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person has any step of this type.
        /// </summary>
        public bool HasSteps { get; set; }

        /// <summary>
        /// Gets or sets the prerequisite step types with their completion state.
        /// </summary>
        public List<PersonProgramPrerequisiteBag> Prerequisites { get; set; }

        /// <summary>
        /// Gets or sets the person's existing steps of this type.
        /// </summary>
        public List<PersonProgramStepBag> Steps { get; set; }
    }
}

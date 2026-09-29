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

using Rock.ViewModels.Core.Grid;

namespace Rock.ViewModels.Blocks.Engagement.PersonProgramStepList
{
    /// <summary>
    /// The block configuration options for the Personal Step List block.
    /// </summary>
    public class PersonProgramStepListOptionsBag
    {
        /// <summary>
        /// Gets or sets the name of the step program shown as the panel title.
        /// </summary>
        public string ProgramName { get; set; }

        /// <summary>
        /// Gets or sets the term the program uses for its steps (defaults to "Step").
        /// </summary>
        public string StepTerm { get; set; }

        /// <summary>
        /// Gets or sets the number of step cards shown per row on desktop.
        /// </summary>
        public int StepsPerRow { get; set; }

        /// <summary>
        /// Gets or sets the number of step cards shown per row on mobile.
        /// </summary>
        public int StepsPerRowMobile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the campus column is
        /// visible. False when the setting is off or only one campus exists.
        /// </summary>
        public bool IsCampusColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the start date column is visible.
        /// </summary>
        public bool IsStartDateColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets the grid column definition for the grid view.
        /// </summary>
        public GridDefinitionBag GridDefinition { get; set; }

        /// <summary>
        /// Gets or sets the Step Entry page URL template. Contains the
        /// placeholders ((StepTypeId)) and ((StepId)) that the client fills in.
        /// Null when no Step Entry page is configured.
        /// </summary>
        public string StepEntryUrlTemplate { get; set; }
    }
}

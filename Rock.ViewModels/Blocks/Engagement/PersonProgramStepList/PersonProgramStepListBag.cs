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

using Rock.ViewModels.Core.Grid;

namespace Rock.ViewModels.Blocks.Engagement.PersonProgramStepList
{
    /// <summary>
    /// The per-load data for the Personal Step List block. Returned on
    /// initialization and again after a delete so the client can refresh.
    /// </summary>
    public class PersonProgramStepListBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the card view should be
        /// shown instead of the grid view.
        /// </summary>
        public bool IsCardView { get; set; }

        /// <summary>
        /// Gets or sets the active step types of the program, each with its
        /// rendered card and the person's steps of that type.
        /// </summary>
        public List<PersonProgramStepTypeBag> StepTypes { get; set; }

        /// <summary>
        /// Gets or sets the grid rows for the person's steps.
        /// </summary>
        public GridDataBag GridData { get; set; }

        /// <summary>
        /// Gets or sets the colors of the statuses in use by the person's
        /// steps, keyed by status name, used to color the grid status label.
        /// </summary>
        public Dictionary<string, string> StepStatusColors { get; set; }

        /// <summary>
        /// Gets or sets the error message to display when the block cannot
        /// render, such as when the program or person was not found.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}

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

namespace Rock.ViewModels.Blocks.Crm.Motivators
{
    /// <summary>
    /// Contains all the initial configuration data required to render the Motivators block.
    /// </summary>
    public class MotivatorsInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the date the individual started the assessment (should be after clicking start).
        /// </summary>
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds the individual took to complete the test, measured by the client
        /// from when the test was started. Null when the client did not provide a measurement.
        /// </summary>
        public double? TimeToTake { get; set; }

        /// <summary>
        /// Gets or sets the instructions for the assessment.
        /// </summary>
        public string Instructions { get; set; }

        /// <summary>
        /// Gets or sets an optional informational message to display to the individual.
        /// </summary>
        public string InfoMessage { get; set; }

        /// <summary>
        /// Gets or sets the questions/responses for the assessment.
        /// </summary>
        public List<MotivatorsResponseBag> Responses { get; set; }

        /// <summary>
        /// Gets or sets the CSS class to use for the panel icon.
        /// </summary>
        public string PanelIcon { get; set; }

        /// <summary>
        /// Gets or sets the title to use for the panel.
        /// </summary>
        public string PanelTitle { get; set; }

        /// <summary>
        /// Gets or sets the number of questions to show per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the assessment record the results should be saved to.
        /// A value of zero indicates that a new assessment should be created when the test is completed.
        /// </summary>
        public int AssessmentId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the completed results should be displayed rather than the test.
        /// </summary>
        public bool ShowResults { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the individual is allowed to retake a completed test.
        /// </summary>
        public bool CanRetakeTest { get; set; }

        /// <summary>
        /// Gets or sets the resolved Results Message HTML to display. Only populated when <see cref="ShowResults"/> is <c>true</c>.
        /// </summary>
        public string ResultsHtml { get; set; }
    }

    /// <summary>
    /// Contains the data representing a single question and the individual's response for the Motivators assessment.
    /// </summary>
    [Serializable]
    public class MotivatorsResponseBag
    {
        /// <summary>
        /// Gets or sets the question code (its unique identifier).
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the question text.
        /// </summary>
        public string Question { get; set; }

        /// <summary>
        /// Gets or sets the answer options. Each option's value is the score that is recorded when the option is selected.
        /// </summary>
        public List<ListItemBag> Options { get; set; }

        /// <summary>
        /// Gets or sets the selected option value (the recorded score). A null value indicates the question is unanswered.
        /// </summary>
        public string Response { get; set; }
    }
}

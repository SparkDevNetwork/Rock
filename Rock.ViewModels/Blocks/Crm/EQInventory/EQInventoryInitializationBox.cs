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

namespace Rock.ViewModels.Blocks.Crm.EQInventory
{
    /// <summary>
    /// Contains all the initial configuration data required to render the EQ Inventory Assessment block.
    /// </summary>
    public class EQInventoryInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the date the individual started the assessment (should be after clicking start).
        /// </summary>
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the instructions for the assessment.
        /// </summary>
        public string Instructions { get; set; }

        /// <summary>
        /// An optional informational message to display to the individual.
        /// </summary>
        public string InfoMessage { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the assessment being taken or viewed. A value of zero
        /// indicates that a new assessment should be created when the responses are saved (e.g. a retake).
        /// </summary>
        public int? AssessmentId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the assessment is for the person currently viewing the assessment.
        /// </summary>
        public bool IsAssessmentForCurrentPerson { get; set; }

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
        /// Gets or sets whether the individual can retake the test.
        /// </summary>
        public bool CanRetakeTest { get; set; }

        /// <summary>
        /// Gets or sets the responses/questions for the assessment.
        /// </summary>
        public List<AssessmentResponseBag> Responses { get; set; }

        /// <summary>
        /// Gets or sets the personalized greeting shown above the results. Only populated when results are shown.
        /// </summary>
        public string ResultsGreeting { get; set; }

        /// <summary>
        /// Gets or sets the scored dimension results of the most recent assessment. This is <c>null</c> until the assessment is completed.
        /// </summary>
        public List<EQInventoryDimensionScoreBag> Results { get; set; }
    }
}

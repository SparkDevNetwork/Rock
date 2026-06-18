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

namespace Rock.ViewModels.Blocks.Crm.EQInventory
{
    /// <summary>
    /// Contains the data representing a single question and the individual's response for the EQ Inventory assessment.
    /// </summary>
    [Serializable]
    public class AssessmentResponseBag
    {
        /// <summary>
        /// Gets or sets the question code (the unique key used for scoring).
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the question text.
        /// </summary>
        public string Question { get; set; }

        /// <summary>
        /// Gets or sets the answer options. Each option's value is the score that is recorded when the option is selected.
        /// The scale is reversed for negatively-keyed questions so the displayed labels stay in the same order.
        /// </summary>
        public List<ListItemBag> Options { get; set; }

        /// <summary>
        /// Gets or sets the selected option value (the recorded score). A null or empty value indicates the question is unanswered.
        /// </summary>
        public string Response { get; set; }
    }
}

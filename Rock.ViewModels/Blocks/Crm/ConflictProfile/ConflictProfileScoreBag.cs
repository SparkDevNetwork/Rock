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

namespace Rock.ViewModels.Blocks.Crm.ConflictProfile
{
    /// <summary>
    /// Represents the scored result for a single Conflict Profile mode or theme.
    /// </summary>
    [Serializable]
    public class ConflictProfileScoreBag
    {
        /// <summary>
        /// Gets or sets the name of the mode or theme (e.g. "Winning").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the descriptive copy explaining what the mode or theme means.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the score for the mode or theme.
        /// </summary>
        public double Percentage { get; set; }

        /// <summary>
        /// Gets or sets the hex color used for this item in the results chart.
        /// </summary>
        public string ChartColor { get; set; }
    }
}

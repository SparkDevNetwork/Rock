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

namespace Rock.ViewModels.Blocks.Crm.EQInventory
{
    /// <summary>
    /// Represents the scored result for a single EQ Inventory dimension.
    /// </summary>
    [Serializable]
    public class EQInventoryDimensionScoreBag
    {
        /// <summary>
        /// Gets or sets the name of the dimension (e.g. "Self Awareness").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the descriptive HTML explaining what the dimension measures.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the interpretation sentence describing what the individual's score means.
        /// </summary>
        public string Interpretation { get; set; }

        /// <summary>
        /// Gets or sets the percentile score (0-100) for the dimension.
        /// </summary>
        public double Percentage { get; set; }
    }
}

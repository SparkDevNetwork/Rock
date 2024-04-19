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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Lms.LearningGradingSystemScaleDetail
{
    /// <summary>
    /// The item details for the Learning Grading System Scale Detail block.
    /// </summary>
    public class LearningGradingSystemScaleBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the description of the grading system scale.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets a value indicating whether this grading system scale is considered passing.
        /// </summary>
        public bool IsPassing { get; set; }

        /// <summary>
        /// Gets or sets the name of the grading system scale.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the minimum threshold percentage of the grading system scale.
        /// </summary>
        public decimal ThresholdPercentage { get; set; }
    }
}

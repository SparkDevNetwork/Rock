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

namespace Rock.ViewModels.Blocks.Lms.LearningGradingSystemDetail
{
    /// <summary>
    /// The item details for the Learning Grading System Detail block.
    /// </summary>
    public class LearningGradingSystemBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the description of the grading system.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets a value indicating whether this grading system is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the name of the grading system.
        /// </summary>
        public string Name { get; set; }
    }
}

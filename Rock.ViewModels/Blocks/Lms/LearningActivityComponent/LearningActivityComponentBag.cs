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

namespace Rock.ViewModels.Blocks.Lms.LearningActivityDetail
{
    /// <summary>
    /// The item details for the Learning Activity Detail block.
    /// </summary>
    public class LearningActivityComponentBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the id of the related LearningActivityComponent for this activity.
        /// </summary>
        public int ActivityComponentId { get; set; }

        /// <summary>
        /// Gets or sets the name of the activity component.
        /// </summary>
        public string ActivityComponentName { get; set; }

        /// <summary>
        /// Gets or sets the path to the activity component .obs file (relative to the web root).
        /// </summary>
        public string ActivityComponentPath { get; set; }

        public string HighlightColor { get; set; }

        public string IconCssClass { get; set; }

        public string Guid { get; set; }
    }
}

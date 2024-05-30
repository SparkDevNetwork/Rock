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

namespace Rock.ViewModels.Blocks.Lms.LearningActivityComponent
{
    /// <summary>
    /// The item details for the Learning Activity Detail block.
    /// </summary>
    public class LearningActivityComponentBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the name of the activity component.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the url to the activity component.
        /// </summary>
        public string ComponentUrl { get; set; }

        /// <summary>
        /// Gets or sets the highlight color of the activity component.
        /// </summary>
        public string HighlightColor { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class of the activity component.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the guid of the activity component.
        /// </summary>
        public string Guid { get; set; }
    }
}

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

using Rock.ViewModels.Blocks.Lms.LearningActivityComponent;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Lms.LearningActivityDetail
{
    /// <summary>
    /// The additional configuration options for the Learning Activity Detail block.
    /// </summary>
    public class LearningActivityDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the available activity types as list items.
        /// </summary>
        public List<ListItemBag> ActivityTypeListItems { get; set; }

        /// <summary>
        /// Gets or sets the list of available activity types.
        /// </summary>
        public List<LearningActivityComponentBag> ActivityTypes { get; set; }

        /// <summary>
        /// Gets or sets whether any students have completed this activity yet.
        /// </summary>
        public bool HasCompletions { get; set; }
    }
}

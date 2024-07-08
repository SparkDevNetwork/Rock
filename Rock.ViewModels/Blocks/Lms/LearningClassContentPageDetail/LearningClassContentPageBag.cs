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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Lms.LearningClassContentPageDetail
{
    /// <summary>
    /// The item details for the Learning Class Announcement Detail block.
    /// </summary>
    public class LearningClassContentPageBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the main content of the page.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the LearningClassId that this content relates to.
        /// </summary>
        public int LearningClassId { get; set; }

        /// <summary>
        /// Gets or sets the date and time the page will be available.
        /// </summary>
        public DateTime? StartDateTime {get; set;}

        /// <summary>
        /// Gets or sets the title of the page.
        /// </summary>
        public string Title { get; set; }
    }
}

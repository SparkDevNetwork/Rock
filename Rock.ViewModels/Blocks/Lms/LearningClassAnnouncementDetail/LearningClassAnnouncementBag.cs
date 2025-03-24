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

using Rock.Enums.Lms;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Lms.LearningClassAnnouncementDetail
{
    /// <summary>
    /// The item details for the Learning Class Announcement Detail block.
    /// </summary>
    public class LearningClassAnnouncementBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the communication mode for the announcement.
        /// </summary>
        public CommunicationMode CommunicationMode { get; set; }

        /// <summary>
        /// Gets or sets whether the announcement has been sent.
        /// </summary>
        public bool CommunicationSent { get; set; }

        /// <summary>
        /// Gets or sets the main content of the announcement.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the date and time the announcement will be published.
        /// </summary>
        public DateTime? PublishDateTime {get; set;}

        /// <summary>
        /// Gets or sets the title of the announcement.
        /// </summary>
        public string Title { get; set; }
    }
}

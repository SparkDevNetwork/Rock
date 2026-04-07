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

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// The additional configuration options for the Connections Hub block.
    /// </summary>
    public class CampusLabelBag
    {
        /// <summary>
        /// Gets or sets the Campus Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Campus Short Code
        /// </summary>
        public string ShortCode { get; set; }

        /// <summary>
        /// Gets or sets the Campus Color
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets the Campus Guid
        /// </summary>
        public Guid guid { get; set; }
    }
}

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

namespace Rock.ViewModels.Blocks.Group.GroupDetail
{
    /// <summary>
    /// A single linkage row (registration, event item occurrence, or content item) rendered in
    /// the Linkages section of the Overview card.
    /// </summary>
    public class GroupLinkageBag
    {
        /// <summary>
        /// Gets or sets the friendly name displayed for the linkage.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the resolved navigation URL.
        /// </summary>
        public string Url { get; set; }
    }
}

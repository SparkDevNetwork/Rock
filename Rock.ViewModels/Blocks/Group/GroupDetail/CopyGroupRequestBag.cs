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
    /// Request payload for the <c>Copy</c> block action on the Group Detail block.
    /// </summary>
    public class CopyGroupRequestBag
    {
        /// <summary>
        /// Gets or sets the identifier of the source group to copy.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether descendant groups are included in the copy.
        /// </summary>
        public bool IncludeChildGroups { get; set; }
    }
}

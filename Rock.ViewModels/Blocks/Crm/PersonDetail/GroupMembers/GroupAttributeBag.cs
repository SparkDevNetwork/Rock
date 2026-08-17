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

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.GroupMembers
{
    /// <summary>
    /// Describes one group attribute value displayed on a group card of the
    /// Group Members block.
    /// </summary>
    public class GroupAttributeBag
    {
        /// <summary>
        /// Gets or sets the attribute name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the formatted attribute value HTML.
        /// </summary>
        public string FormattedValue { get; set; }
    }
}

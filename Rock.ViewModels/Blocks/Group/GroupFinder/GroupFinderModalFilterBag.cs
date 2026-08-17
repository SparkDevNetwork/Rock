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

namespace Rock.ViewModels.Blocks.Group.GroupFinder
{
    /// <summary>
    /// A non-featured attribute filter rendered in the More Filters modal via the standard field-type filter control.
    /// </summary>
    public class GroupFinderModalFilterBag
    {
        /// <summary>
        /// Gets or sets the attribute key this filter targets.
        /// </summary>
        public string AttributeKey { get; set; }

        /// <summary>
        /// Gets or sets the public attribute used to render the field type's own filter control.
        /// </summary>
        public PublicAttributeBag Attribute { get; set; }
    }
}

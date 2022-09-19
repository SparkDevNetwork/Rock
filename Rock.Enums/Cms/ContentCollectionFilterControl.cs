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

namespace Rock.Enums.Cms
{
    /// <summary>
    /// Defines the type of UI controls that can be rendered for filters
    /// on the content collection pages.
    /// </summary>
    public enum ContentCollectionFilterControl
    {
        /// <summary>
        /// Filter will be rendered as a pill and will behave as either a
        /// radio button or checkbox.
        /// </summary>
        Pills = 0,

        /// <summary>
        /// Filter will be rendered as a dropdown selection.
        /// </summary>
        Dropdown = 1,

        /// <summary>
        /// Filter will be rendered as a single boolean option.
        /// </summary>
        Boolean = 2
    }
}

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

namespace Rock.Enums.Cms
{
    /// <summary>
    /// Specifies the role of a block within a page, defining its purpose and
    /// how it contributes to the page's functionality.
    /// </summary>
    public enum BlockRole
    {
        /// <summary>
        /// The block provides core functionality for the system on the page.
        /// This is typically used for blocks that are essential for the page's
        /// operation, such as the Notifications block or Universal Search
        /// block on the internal site.
        /// </summary>
        System = 0,

        /// <summary>
        /// The block provides navigation features for the page. This is
        /// typically used for blocks that would cause the individual to
        /// navigate away from the current page. But it could also apply to
        /// blocks that change the context of the current page in a way that
        /// would be considered a navigation action.
        /// </summary>
        Navigation = 1,

        /// <summary>
        /// The block provides content for the page. This is typically used
        /// for blocks that display static content on the page, although the
        /// content might be dynamic in nature. An example would be a copyright
        /// footer that uses Lava to display the current year.
        /// </summary>
        Content = 2,

        /// <summary>
        /// The block provides primary functionality for the page. This is
        /// typically used for list and detail blocks that provide the primary
        /// interaction for the individual on the page.
        /// </summary>
        Primary = 3,

        /// <summary>
        /// The block provides secondary functionality for the page. This is
        /// typically used for blocks that provide additional features or
        /// information that is not essential to the primary interaction. These
        /// are often hidden when the primary block enters an edit mode.
        /// </summary>
        Secondary = 4,
    }
}

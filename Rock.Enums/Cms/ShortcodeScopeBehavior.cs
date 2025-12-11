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
    /// Determines how variables defined within Lava shortcodes are scoped.
    /// </summary>
    public enum ShortcodeScopeBehavior
    {
        /// <summary>
        /// Variables defined in the shortcode are not accessible in the surrounding Lava after the shortcode runs.
        /// </summary>
        Isolated = 0,

        /// <summary>
        /// Variables defined in the shortcode are accessible in the surrounding Lava after the shortcode runs.
        /// </summary>
        Shared = 1
    }
}

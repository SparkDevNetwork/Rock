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
using Rock.Attribute;

namespace Rock.Utility.HtmlSanitization
{
    /// <summary>
    /// What the allowlist sanitizer does with an element that the options do
    /// not allow. Elements that must never pass are always dropped with their
    /// content, whatever the mode.
    /// </summary>
    [RockInternal( "17.12" )]
    public enum HtmlTagRemovalMode
    {
        /// <summary>
        /// Removes the element together with all of its content.
        /// </summary>
        Drop = 0,

        /// <summary>
        /// Removes the element but keeps its sanitized child nodes in its place.
        /// </summary>
        Unwrap = 1
    }
}

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

namespace Rock.Lava
{
    /// <summary>
    /// Determines the type of tag (inline or block). Block type requires an end tag.
    /// </summary>
    public enum LavaShortcodeTypeSpecifier
    {
        /// <summary>
        /// A language element that consists of a single Tag.
        /// </summary>
        Inline = 1,

        /// <summary>
        /// A language element that consists of an opening Tag and a corresponding closing Tag.
        /// </summary>
        Block = 2
    }

    /// <summary>
    /// Represents a Lava shortcode definition, a tag that represents a parameterized Lava template.
    /// Shortcodes can be used to generate complex Lava components from a simple tag.
    /// </summary>
    public interface ILavaShortcode : IRockLavaElement
    {
        LavaShortcodeTypeSpecifier ElementType { get; }
    }
}

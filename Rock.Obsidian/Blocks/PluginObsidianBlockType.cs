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

namespace Rock.Obsidian.Blocks
{
    /// <summary>
    /// Plugin Obsidian Block Type
    /// </summary>
    /// <seealso cref="Rock.Obsidian.Blocks.ObsidianBlockType" />
    public abstract class PluginObsidianBlockType : ObsidianBlockType
    {
        #region Properties

        /// <summary>
        /// Gets the block markup file identifier.
        /// </summary>
        /// <value>
        /// The block markup file identifier.
        /// </value>
        public override string BlockMarkupFileIdentifier
        {
            get
            {
                var type = GetType();
                var ns = type.Namespace;
                return $"{ns}.{type.Name}";
            }
        }

        #endregion Properties
    }
}

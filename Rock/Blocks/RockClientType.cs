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

namespace Rock.Blocks
{
    /// <summary>
    /// Rock Client Type
    /// </summary>
    public enum RockClientType
    {
        /// <summary>
        /// Web Forms
        /// </summary>
        WebForms = 0,

        /// <summary>
        /// Mobile
        /// </summary>
        Mobile = 1,

        /// <summary>
        /// Obsidian
        /// </summary>
        Obsidian = 2
    }

    /// <summary>
    /// Rock Client Type Helpers
    /// </summary>
    internal static class RockClientTypeHelpers
    {
        /// <summary>
        /// Gets the type of the rock client.
        /// </summary>
        /// <param name="blockType">Type of the block.</param>
        /// <returns></returns>
        internal static RockClientType GetRockClientType( this IRockBlockType blockType )
        {
            return
                blockType is IObsidianBlockType ? RockClientType.Obsidian :
                blockType is IRockMobileBlockType ? RockClientType.Mobile :
                RockClientType.WebForms;
        }
    }
}

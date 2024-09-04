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

using System;

using Rock.Blocks;

namespace Rock.Obsidian.UI
{
    /// <summary>
    /// Options to be used with <see cref="GridBuilderExtensions.WithBlock{T}(GridBuilder{T}, IRockBlockType, GridBuilderGridOptions{T})"/>
    /// to provide additional configuration data.
    /// </summary>
    /// <typeparam name="T">The type of the grid builder.</typeparam>
    public class GridBuilderGridOptions<T>
    {
        /// <summary>
        /// Gets or sets the method that will be called to translate the row
        /// object into one that can be used with Lava for custom columns.
        /// </summary>
        /// <value>
        /// The method that will be called to translate the row into a lava object.
        /// </value>
        public Func<T, object> LavaObject { get; set; }
    }
}

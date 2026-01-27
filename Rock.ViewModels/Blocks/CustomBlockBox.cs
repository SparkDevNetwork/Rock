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

namespace Rock.ViewModels.Blocks
{
    /// <summary>
    /// A box that contains the required information to render a custom
    /// block component in the rare case that a block does not conform
    /// to an Entity.
    /// </summary>
    /// <typeparam name="TCustomBag">The type of the settings property.</typeparam>
    /// <typeparam name="TOptions">The type of the options property.</typeparam>
    public class CustomBlockBox<TCustomBag, TOptions> : BlockBox
        where TOptions : new()
    {
        /// <summary>
        /// Gets or sets a custom bag of data required by the block.
        /// </summary>
        /// <value>The data.</value>
        public TCustomBag Bag { get; set; }

        /// <summary>
        /// Gets or sets the block properties and common data options.
        /// </summary>
        /// <value>The options.</value>
        public TOptions Options { get; set; } = new TOptions();
    }
}

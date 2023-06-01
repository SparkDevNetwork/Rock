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

using Rock.ViewModels.Core.Grid;

namespace Rock.ViewModels.Blocks
{
    /// <summary>
    /// The information required to render a standard list block.
    /// </summary>
    /// <typeparam name="TOptions">The type of the bag options.</typeparam>
    public class ListBlockBox<TOptions> : BlockBox
        where TOptions : new()
    {
        /// <summary>
        /// Gets or sets the grid definition.
        /// </summary>
        /// <value>The grid definition.</value>
        public GridDefinitionBag GridDefinition { get; set; }

        /// <summary>
        /// Gets or sets the expected row count. This is used to determine
        /// optimization settings and also the number of skeleton rows to show.
        /// A value of <c>null</c> implies no fast way to estimate the number
        /// of expected rows.
        /// </summary>
        /// <value>The expected row count.</value>
        public int? ExpectedRowCount { get; set; }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <value>The options.</value>
        public TOptions Options { get; set; } = new TOptions();

        /// <summary>
        /// Gets or sets a value indicating whether add item is enabled.
        /// </summary>
        /// <value><c>true</c> if add item is enabled; otherwise, <c>false</c>.</value>
        public bool IsAddEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether delete is enabled.
        /// </summary>
        /// <value><c>true</c> if delete is enabled; otherwise, <c>false</c>.</value>
        public bool IsDeleteEnabled { get; set; }
    }
}

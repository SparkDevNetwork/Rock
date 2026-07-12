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

namespace Rock.ViewModels.Blocks.Crm.DocumentList
{
    /// <summary>
    /// The additional configuration options for the Document List block.
    /// </summary>
    public class DocumentListOptionsBag
    {
        /// <summary>
        /// Gets or sets the title shown in the block heading.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the security column should
        /// be shown in the grid.
        /// </summary>
        public bool IsSecurityColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the block (and its grid)
        /// should be shown. This is <c>false</c> when there is no context entity
        /// for the documents to be associated with.
        /// </summary>
        public bool IsBlockVisible { get; set; }

        /// <summary>
        /// Gets or sets the configuration warning message to display when the
        /// block has not been correctly configured. A <c>null</c> or empty value
        /// indicates there is no warning to show.
        /// </summary>
        public string WarningMessage { get; set; }
    }
}

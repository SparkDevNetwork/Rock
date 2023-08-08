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

using Rock.Enums.Cms;
using Rock.ViewModels.Utility;
using System;

namespace Rock.ViewModels.Blocks.Cms.LibraryViewer
{
    /// <summary>
    /// Bag containing Library Viewer item information.
    /// </summary>
    public class LibraryViewerValidationResultsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is author attribute mapped.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is author attribute mapped; otherwise, <c>false</c>.
        /// </value>
        public bool IsAuthorAttributeMapped { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is summary attribute mapped.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is summary attribute mapped; otherwise, <c>false</c>.
        /// </value>
        public bool IsSummaryAttributeMapped { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is image attribute mapped.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is image attribute mapped; otherwise, <c>false</c>.
        /// </value>
        public bool IsImageAttributeMapped { get; set; }
    }
}

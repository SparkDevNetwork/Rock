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

using Rock.ViewModels.Utility;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Core.BinaryFileTypeDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class BinaryFileTypeDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the preferred color depth options for the client dropdown.
        /// </summary>
        /// <value>
        /// The preferred color depth options.
        /// </value>
        public List<ListItemBag> PreferredColorDepthOptions { get; set; }
        /// <summary>
        /// Gets or sets the preferred resolution options for the client dropdown.
        /// </summary>
        /// <value>
        /// The preferred resolution options.
        /// </value>
        public List<ListItemBag> PreferredResolutionOptions { get; set; }
        /// <summary>
        /// Gets or sets the preferred format options for the client dropdown.
        /// </summary>
        /// <value>
        /// The preferred format options.
        /// </value>
        public List<ListItemBag> PreferredFormatOptions { get; set; }
    }
}

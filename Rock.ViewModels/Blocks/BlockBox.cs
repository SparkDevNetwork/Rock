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

using System.Collections.Generic;

namespace Rock.ViewModels.Blocks
{
    /// <summary>
    /// The information required to render a generic block.
    /// </summary>
    public class BlockBox : Internal.IBlockBox
    {
        /// <summary>
        /// Gets or sets the error message. A non-empty value indicates that
        /// an error is preventing the block from being displayed.
        /// </summary>
        /// <value>The error message.</value>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the navigation urls.
        /// </summary>
        /// <value>The navigation urls.</value>
        public Dictionary<string, string> NavigationUrls { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the security grant token.
        /// </summary>
        /// <value>The security grant token.</value>
        public string SecurityGrantToken { get; set; }
    }
}

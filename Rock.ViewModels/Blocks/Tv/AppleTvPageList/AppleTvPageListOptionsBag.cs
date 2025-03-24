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

namespace Rock.ViewModels.Blocks.Tv.AppleTvPageList
{
    /// <summary>
    /// The additional configuration options for the Page List block.
    /// </summary>
    public class AppleTvPageListOptionsBag
    {
        /// <summary>
        /// Gets or sets the default page identifier key of the current application.
        /// </summary>
        /// <value>
        /// The default page identifier key.
        /// </value>
        public string DefaultPageIdKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the block is visible, the block is displayed if an ApplicationId is provided as a query parameter.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the block should be visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsBlockVisible { get; set; }
    }
}

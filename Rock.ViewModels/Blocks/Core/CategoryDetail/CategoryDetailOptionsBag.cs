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

namespace Rock.ViewModels.Blocks.Core.CategoryDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class CategoryDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets whether the block should be shown.
        /// </summary>
        /// <remarks>
        /// Because this block may be used with the Category Tree View control
        /// - we want this block to be hidden when there is no selection.
        /// </remarks>
        public bool ShowBlock { get; set; }
    }
}

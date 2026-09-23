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

namespace Rock.ViewModels.Blocks.Cms.PageBuilder
{
    /// <summary>
    /// A module type listed in the Page Builder's sidebar.
    /// </summary>
    public class PageBuilderModuleTypeBag
    {
        /// <summary>
        /// Gets or sets the value that uniquely identifies the module type.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the name shown on the module type's tile.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the icon shown on the module type's tile.
        /// </summary>
        public string IconCssClass { get; set; }
    }
}
